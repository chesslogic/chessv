using ChessV.Base;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.APChessV
{
  internal sealed class ApmwSidecarUpgradePreference
  {
    public ApmwSidecarUpgradePreference(
      string action,
      int priority,
      int proportionNumerator,
      int proportionDenominator)
    {
      Action = action;
      Priority = priority;
      ProportionNumerator = proportionNumerator;
      ProportionDenominator = proportionDenominator;
    }

    public string Action { get; }
    public int Priority { get; }
    public int ProportionNumerator { get; }
    public int ProportionDenominator { get; }
  }

  internal sealed class ApmwSidecarFraction
  {
    private const int MaximumDecimalPlaces = 9;

    private ApmwSidecarFraction(int numerator, int denominator)
    {
      Numerator = numerator;
      Denominator = denominator;
    }

    public int Numerator { get; }
    public int Denominator { get; }

    public static ApmwSidecarFraction FromDoubleExact(double value)
    {
      if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
        throw new ArgumentOutOfRangeException(nameof(value), "proportion must be a finite nonnegative number");

      string text = value.ToString("R", CultureInfo.InvariantCulture);
      if (!decimal.TryParse(
        text,
        NumberStyles.Float,
        CultureInfo.InvariantCulture,
        out decimal decimalValue))
      {
        throw new ArgumentException("proportion cannot be represented as a decimal fraction", nameof(value));
      }

      int[] bits = decimal.GetBits(decimalValue);
      int scale = (bits[3] >> 16) & 0x7f;
      if (scale > MaximumDecimalPlaces)
      {
        throw new ArgumentException(
          "proportion requires more than " + MaximumDecimalPlaces + " decimal places",
          nameof(value));
      }

      BigInteger unscaled = (uint)bits[0] |
        ((BigInteger)(uint)bits[1] << 32) |
        ((BigInteger)(uint)bits[2] << 64);
      if ((bits[3] & unchecked((int)0x80000000)) != 0)
        unscaled = -unscaled;
      BigInteger denominator = Pow10(scale);
      BigInteger divisor = BigInteger.GreatestCommonDivisor(BigInteger.Abs(unscaled), denominator);
      unscaled /= divisor;
      denominator /= divisor;
      if (unscaled > int.MaxValue || denominator > int.MaxValue)
      {
        throw new ArgumentException(
          "proportion cannot be represented by bounded 32-bit fraction fields",
          nameof(value));
      }

      return new ApmwSidecarFraction((int)unscaled, (int)denominator);
    }

    private static BigInteger Pow10(int exponent)
    {
      BigInteger result = 1;
      for (int index = 0; index < exponent; index++)
        result *= 10;
      return result;
    }
  }

  /// <summary>
  /// Immutable semantic input for one projector batch. It has no dependence on
  /// process-wide APMW state after it has been constructed.
  /// </summary>
  internal sealed class ApmwSidecarInputSnapshot
  {
    internal static readonly IReadOnlyList<string> AllGeometryStages =
      Array.AsReadOnly(new[] { "8x8", "10x8", "10x10", "12x10", "12x12" });

    private static readonly string[] SeedNames =
    {
      "pocket_seed", "pawn_seed", "minor_seed", "major_seed", "queen_seed",
    };

    private readonly string canonicalJson;

    public ApmwSidecarInputSnapshot(
      ApmwContractV2 contract,
      string itemization,
      string ordering,
      IEnumerable<KeyValuePair<string, string>> seeds,
      IEnumerable<KeyValuePair<string, int>> itemCounts,
      IEnumerable<ApmwSidecarUpgradePreference> upgradePreferences = null)
    {
      if (contract == null)
        throw new ArgumentNullException(nameof(contract));
      if (itemization != "legacy" && itemization != "fundamental")
        throw new ArgumentException("itemization must be legacy or fundamental", nameof(itemization));
      if (ordering != "stable" && ordering != "chaos")
        throw new ArgumentException("ordering must be stable or chaos", nameof(ordering));
      if (itemization == "fundamental" && ordering != "stable")
        throw new ArgumentException("fundamental itemization supports stable ordering only", nameof(ordering));

      ContractHash = contract.ManifestSha256;
      Itemization = itemization;
      Ordering = ordering;
      Seeds = NormalizeSeeds(seeds);
      ItemCounts = NormalizeItemCounts(contract, itemization, itemCounts);
      UpgradePreferences = NormalizeUpgradePreferences(contract, upgradePreferences);
      canonicalJson = BuildCanonicalJson();
      InputSha256 = Sha256(canonicalJson);
    }

    public string ContractHash { get; }
    public string Itemization { get; }
    public string Ordering { get; }
    public IReadOnlyDictionary<string, string> Seeds { get; }
    public IReadOnlyDictionary<string, int> ItemCounts { get; }
    public IReadOnlyList<ApmwSidecarUpgradePreference> UpgradePreferences { get; }
    public bool HasUpgradePreferences { get { return UpgradePreferences != null; } }
    public string InputSha256 { get; }
    public string CanonicalJson { get { return canonicalJson; } }

    public JsonElement ToJsonElement()
    {
      using (JsonDocument document = JsonDocument.Parse(canonicalJson))
        return document.RootElement.Clone();
    }

    private static IReadOnlyDictionary<string, string> NormalizeSeeds(
      IEnumerable<KeyValuePair<string, string>> seeds)
    {
      if (seeds == null)
        throw new ArgumentNullException(nameof(seeds));

      var result = new Dictionary<string, string>(StringComparer.Ordinal);
      foreach (KeyValuePair<string, string> seed in seeds)
      {
        if (!SeedNames.Contains(seed.Key, StringComparer.Ordinal))
          throw new ArgumentException("unknown seed field '" + seed.Key + "'", nameof(seeds));
        if (!result.TryAdd(seed.Key, seed.Value))
          throw new ArgumentException("duplicate seed field '" + seed.Key + "'", nameof(seeds));
        if (seed.Value == null)
          throw new ArgumentException("seed field '" + seed.Key + "' must be a string", nameof(seeds));
      }
      if (result.Count != SeedNames.Length)
        throw new ArgumentException("seeds must contain exactly the five required seed fields", nameof(seeds));
      return new ReadOnlyDictionary<string, string>(result);
    }

    private static IReadOnlyDictionary<string, int> NormalizeItemCounts(
      ApmwContractV2 contract,
      string itemization,
      IEnumerable<KeyValuePair<string, int>> itemCounts)
    {
      if (itemCounts == null)
        throw new ArgumentNullException(nameof(itemCounts));

      var known = new Dictionary<string, int>(contract.EffectiveItemMaxima["common"], StringComparer.Ordinal);
      foreach (KeyValuePair<string, int> item in contract.EffectiveItemMaxima[itemization])
        known.Add(item.Key, item.Value);

      var result = new Dictionary<string, int>(StringComparer.Ordinal);
      foreach (KeyValuePair<string, int> item in itemCounts)
      {
        if (!known.TryGetValue(item.Key, out int maximum))
          throw new ArgumentException("unknown item name '" + item.Key + "' for " + itemization, nameof(itemCounts));
        if (!result.TryAdd(item.Key, item.Value))
          throw new ArgumentException("duplicate item name '" + item.Key + "'", nameof(itemCounts));
        if (item.Value < 0)
          throw new ArgumentOutOfRangeException(nameof(itemCounts), "item count must not be negative");
        if (item.Value > maximum)
          throw new ArgumentOutOfRangeException(
            nameof(itemCounts),
            "item count for '" + item.Key + "' exceeds the current contract maximum");
      }
      return new ReadOnlyDictionary<string, int>(result);
    }

    private static IReadOnlyList<ApmwSidecarUpgradePreference> NormalizeUpgradePreferences(
      ApmwContractV2 contract,
      IEnumerable<ApmwSidecarUpgradePreference> preferences)
    {
      if (preferences == null)
        return null;

      var known = new HashSet<string>(
        contract.UpgradeDag.PawnCreationActions.Concat(contract.UpgradeDag.Transitions.Select(item => item.Action)),
        StringComparer.Ordinal);
      var result = new List<ApmwSidecarUpgradePreference>();
      var actions = new HashSet<string>(StringComparer.Ordinal);
      foreach (ApmwSidecarUpgradePreference preference in preferences)
      {
        if (preference == null || !known.Contains(preference.Action))
          throw new ArgumentException("unknown upgrade action", nameof(preferences));
        if (!actions.Add(preference.Action))
          throw new ArgumentException("duplicate upgrade action '" + preference.Action + "'", nameof(preferences));
        if (preference.ProportionNumerator < 0)
          throw new ArgumentOutOfRangeException(nameof(preferences), "upgrade proportion numerator must not be negative");
        if (preference.ProportionDenominator <= 0)
          throw new ArgumentOutOfRangeException(nameof(preferences), "upgrade proportion denominator must be positive");
        result.Add(preference);
      }
      return new ReadOnlyCollection<ApmwSidecarUpgradePreference>(
        result.OrderBy(item => item.Action, StringComparer.Ordinal).ToList());
    }

    private string BuildCanonicalJson()
    {
      var buffer = new ArrayBufferWriter<byte>();
      using (var writer = new Utf8JsonWriter(buffer))
      {
        writer.WriteStartObject();
        writer.WriteString("itemization", Itemization);
        writer.WriteString("ordering", Ordering);
        writer.WritePropertyName("seeds");
        writer.WriteStartObject();
        foreach (string name in SeedNames)
          writer.WriteString(name, Seeds[name]);
        writer.WriteEndObject();
        writer.WritePropertyName("item_counts");
        writer.WriteStartObject();
        foreach (KeyValuePair<string, int> item in ItemCounts.OrderBy(item => item.Key, StringComparer.Ordinal))
          writer.WriteNumber(item.Key, item.Value);
        writer.WriteEndObject();
        if (HasUpgradePreferences)
        {
          writer.WritePropertyName("upgrade_preferences");
          writer.WriteStartArray();
          foreach (ApmwSidecarUpgradePreference preference in UpgradePreferences)
          {
            writer.WriteStartObject();
            writer.WriteString("action", preference.Action);
            writer.WriteNumber("priority", preference.Priority);
            writer.WriteNumber("proportion_numerator", preference.ProportionNumerator);
            writer.WriteNumber("proportion_denominator", preference.ProportionDenominator);
            writer.WriteEndObject();
          }
          writer.WriteEndArray();
        }
        writer.WriteEndObject();
      }
      return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    private static string Sha256(string value)
    {
      using (SHA256 algorithm = SHA256.Create())
        return string.Concat(algorithm.ComputeHash(Encoding.UTF8.GetBytes(value)).Select(byteValue => byteValue.ToString("x2", CultureInfo.InvariantCulture)));
    }
  }

  internal static class ApmwSidecarSnapshotCapture
  {
    public static ApmwSidecarInputSnapshot Capture(
      ApmwCore core,
      ApmwConfig config,
      ApmwContractV2 contract)
    {
      if (core == null)
        throw new ArgumentNullException(nameof(core));
      if (config == null)
        throw new ArgumentNullException(nameof(config));
      if (contract == null)
        throw new ArgumentNullException(nameof(contract));
      if (!config.UsesCurrentContract || config.CurrentContract == null ||
          !string.Equals(config.CurrentContract.ManifestSha256, contract.ManifestSha256, StringComparison.Ordinal))
      {
        throw new InvalidOperationException("sidecar snapshots require the supplied current validated contract");
      }

      string itemization = config.UsesFundamentalProgressionItemization ? "fundamental" : "legacy";
      string ordering = itemization == "fundamental"
        ? "stable"
        : config.Types == PieceTypes.Chaos || config.Locs == PieceLocations.Chaos
          ? "chaos"
          : "stable";
      var items = CommonItems(core);
      if (itemization == "fundamental")
      {
        int materialItemValue = contract.ExpectedMaterial["material_item"];
        if (config.materialItemValue != materialItemValue)
          throw new InvalidOperationException("fundamental sidecar snapshots require the current contract material item value");
        int budget = RequiredCount(core.foundMaterialBudget, "Material budget");
        if (budget % materialItemValue != 0)
          throw new InvalidOperationException("fundamental material budget is not an exact number of Material items");
        items.Add(new KeyValuePair<string, int>(
          ApmwConstants.ProgressiveItems.Chessmen, RequiredCount(core.foundChessmen, "Chessmen")));
        items.Add(new KeyValuePair<string, int>(
          ApmwConstants.ProgressiveItems.Material, budget / materialItemValue));
        items.Add(new KeyValuePair<string, int>(
          ApmwConstants.ProgressiveItems.Castler, RequiredCount(core.foundCastlers, "Castler")));
      }
      else
      {
        items.Add(new KeyValuePair<string, int>(
          ApmwConstants.ProgressiveItems.Pawn, RequiredCount(core.foundPawns, "Progressive Pawn")));
        items.Add(new KeyValuePair<string, int>(
          ApmwConstants.ProgressiveItems.PawnForwardness, RequiredCount(core.foundPawnForwardness, "Progressive Pawn Forwardness")));
        items.Add(new KeyValuePair<string, int>(
          ApmwConstants.ProgressiveItems.MinorPiece, RequiredCount(core.foundMinors, "Progressive Minor Piece")));
        items.Add(new KeyValuePair<string, int>(
          ApmwConstants.ProgressiveItems.MajorPiece, RequiredCount(core.foundMajors, "Progressive Major Piece")));
        items.Add(new KeyValuePair<string, int>(
          ApmwConstants.ProgressiveItems.MajorToQueen, RequiredCount(core.foundQueens, "Progressive Major To Queen")));
        items.Add(new KeyValuePair<string, int>(
          ApmwConstants.ProgressiveItems.Jack, RequiredCount(core.foundJacks, "Progressive Jack")));
      }

      var preferences = new List<ApmwSidecarUpgradePreference>();
      foreach (ApmwConfig.PieceUpgradeActionResolution resolution in config.PieceUpgradeActions.Values)
      {
        if (!resolution.IsEnabled)
          continue;
        ApmwSidecarFraction fraction = ApmwSidecarFraction.FromDoubleExact(resolution.Proportion);
        preferences.Add(new ApmwSidecarUpgradePreference(
          resolution.ActionName,
          resolution.Priority,
          fraction.Numerator,
          fraction.Denominator));
      }

      return new ApmwSidecarInputSnapshot(
        contract,
        itemization,
        ordering,
        new[]
        {
          new KeyValuePair<string, string>("pocket_seed", StableSlotSeed(config, "pocket_seed")),
          new KeyValuePair<string, string>("pawn_seed", StableSlotSeed(config, "pawn_seed")),
          new KeyValuePair<string, string>("minor_seed", StableSlotSeed(config, "minor_seed")),
          new KeyValuePair<string, string>("major_seed", StableSlotSeed(config, "major_seed")),
          new KeyValuePair<string, string>("queen_seed", StableSlotSeed(config, "queen_seed")),
        },
        items,
        preferences);
    }

    private static List<KeyValuePair<string, int>> CommonItems(ApmwCore core)
    {
      if (core.EngineWeakeningProvider == null)
        throw new InvalidOperationException("AI Intelligence Malus count is unavailable");
      return new List<KeyValuePair<string, int>>
      {
        new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.PlayAsWhite, RequiredCount(core.foundPlayAsWhite, "Play as White")),
        new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.AIIntelligenceMalus, RequiredCount(core.EngineWeakeningProvider(), "Progressive AI Intelligence Malus")),
        new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.Pocket, RequiredCount(core.foundPockets, "Progressive Pocket")),
        new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.PocketRange, RequiredCount(core.foundPocketRange, "Progressive Pocket Range")),
        new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.KingPromotion, RequiredCount(core.foundKingPromotions, "Progressive King Promotion")),
        new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.Consul, RequiredCount(core.foundConsuls, "Progressive Consul")),
      };
    }

    private static int RequiredCount(int value, string name)
    {
      if (value < 0)
        throw new InvalidOperationException(name + " count is not initialized");
      return value;
    }

    private static string StableSlotSeed(ApmwConfig config, string key)
    {
      if (config.SlotData != null && config.SlotData.TryGetValue(key, out object value) && value != null)
        return Convert.ToString(value, CultureInfo.InvariantCulture);
      return "0";
    }
  }

  internal sealed class ApmwSidecarIdentity
  {
    private ApmwSidecarIdentity(string contractHash, string runtimeSemanticVersion)
    {
      ContractHash = contractHash;
      RuntimeSemanticVersion = runtimeSemanticVersion;
    }

    public string ContractHash { get; }
    public string RuntimeSemanticVersion { get; }

    public static ApmwSidecarIdentity FromValidatedLock(ApmwProjectorLock projectorLock, ApmwContractV2 contract)
    {
      if (projectorLock == null)
        throw new ArgumentNullException(nameof(projectorLock));
      if (contract == null)
        throw new ArgumentNullException(nameof(contract));
      if (projectorLock.ProtocolVersion != ApmwSidecarRequest.ProtocolVersion ||
          !string.Equals(
            projectorLock.RuntimeSemanticVersion,
            ApmwProjectorLockParser.RuntimeSemanticVersion,
            StringComparison.Ordinal) ||
          !string.Equals(projectorLock.ContractHash, contract.ManifestSha256, StringComparison.Ordinal))
      {
        throw new ArgumentException("projector lock does not match the supplied validated current contract", nameof(projectorLock));
      }
      return new ApmwSidecarIdentity(projectorLock.ContractHash, projectorLock.RuntimeSemanticVersion);
    }

    public ApmwSidecarRequest CreateRequest(ApmwSidecarInputSnapshot snapshot)
    {
      if (snapshot == null)
        throw new ArgumentNullException(nameof(snapshot));
      if (!string.Equals(snapshot.ContractHash, ContractHash, StringComparison.Ordinal))
        throw new ArgumentException("snapshot contract hash does not match the pinned projector identity", nameof(snapshot));
      return new ApmwSidecarRequest(
        "apmw-" + snapshot.InputSha256,
        ContractHash,
        snapshot.ToJsonElement(),
        ApmwSidecarInputSnapshot.AllGeometryStages,
        RuntimeSemanticVersion);
    }
  }

  /// <summary>Thread-safe batch cache for a future sidecar-backed projection backend.</summary>
  internal sealed class ApmwSidecarBatchCache
  {
    private sealed class CacheEntry
    {
      public Task<ApmwSidecarSuccessResponse> Task;
    }

    private readonly object sync = new object();
    private readonly IApmwSidecarRunner runner;
    private readonly ApmwSidecarIdentity identity;
    private readonly Dictionary<string, CacheEntry> entries =
      new Dictionary<string, CacheEntry>(StringComparer.Ordinal);

    public ApmwSidecarBatchCache(IApmwSidecarRunner runner, ApmwSidecarIdentity identity)
    {
      this.runner = runner ?? throw new ArgumentNullException(nameof(runner));
      this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
    }

    public async Task<ApmwSidecarSuccessResponse> GetAsync(
      ApmwSidecarInputSnapshot snapshot,
      CancellationToken cancellationToken)
    {
      if (snapshot == null)
        throw new ArgumentNullException(nameof(snapshot));
      cancellationToken.ThrowIfCancellationRequested();
      ApmwSidecarRequest request = identity.CreateRequest(snapshot);
      string key = identity.ContractHash + "|" + identity.RuntimeSemanticVersion + "|" + snapshot.InputSha256;
      CacheEntry entry;
      lock (sync)
      {
        if (!entries.TryGetValue(key, out entry))
        {
          entry = new CacheEntry();
          entries.Add(key, entry);
          entry.Task = RunAsync(key, entry, request);
        }
      }
      ApmwSidecarSuccessResponse response = await entry.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
      return Copy(response);
    }

    public async Task<JsonElement> GetProjectionAsync(
      ApmwSidecarInputSnapshot snapshot,
      string geometryStage,
      CancellationToken cancellationToken)
    {
      if (!ApmwSidecarInputSnapshot.AllGeometryStages.Contains(geometryStage, StringComparer.Ordinal))
        throw new ArgumentException("geometry must be one of the current contract stages", nameof(geometryStage));
      ApmwSidecarSuccessResponse response = await GetAsync(snapshot, cancellationToken).ConfigureAwait(false);
      ApmwSidecarProjectionResult result = response.Results.Single(item => item.GeometryStage == geometryStage);
      return result.Projection.Clone();
    }

    public void Invalidate()
    {
      lock (sync)
        entries.Clear();
    }

    private async Task<ApmwSidecarSuccessResponse> RunAsync(
      string key,
      CacheEntry entry,
      ApmwSidecarRequest request)
    {
      try
      {
        return Copy(await runner.RunAsync(request, CancellationToken.None).ConfigureAwait(false));
      }
      catch
      {
        lock (sync)
        {
          if (entries.TryGetValue(key, out CacheEntry current) && ReferenceEquals(current, entry))
            entries.Remove(key);
        }
        throw;
      }
    }

    private static ApmwSidecarSuccessResponse Copy(ApmwSidecarSuccessResponse response)
    {
      if (response == null)
        throw new InvalidOperationException("sidecar runner returned no response");
      return new ApmwSidecarSuccessResponse(
        response.RequestId,
        response.ContractHash,
        response.RuntimeSemanticVersion,
        response.Results
          .Select(result => new ApmwSidecarProjectionResult(result.GeometryStage, result.Projection))
          .ToList()
          .AsReadOnly());
    }
  }
}
