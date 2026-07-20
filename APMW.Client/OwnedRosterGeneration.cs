using ChessV;
using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Archipelago.APChessV
{
  internal enum SourcePlacementRole
  {
    PrimaryRoyal,
    AdditionalRoyal,
    LockedCastler,
    JackSlot,
    MajorSlot,
    MinorSlot,
    PawnSlot,
  }

  internal enum FinalPieceFamily
  {
    Pawn,
    Minor,
    Major,
    Jack,
    Queen,
    Amazon,
  }

  internal enum OwnedMaterialCategory
  {
    PrimaryRoyal,
    ChessmanBase,
    DirectPiece,
    Upgrade,
    LockedCastler,
    AdditionalRoyal,
    Dormant,
    ConcreteEvaluationAdjustment,
    Unallocated,
  }

  internal sealed class OwnedMaterialLedgerEntry
  {
    public OwnedMaterialLedgerEntry(OwnedMaterialCategory category, int amount, string stableId, string action)
    {
      Category = category;
      Amount = amount;
      StableId = stableId;
      Action = action;
    }

    public OwnedMaterialCategory Category { get; }
    public int Amount { get; }
    public string StableId { get; }
    public string Action { get; }
  }

  internal sealed class OwnedMaterialLedger
  {
    private readonly List<OwnedMaterialLedgerEntry> entries = new List<OwnedMaterialLedgerEntry>();

    public IReadOnlyList<OwnedMaterialLedgerEntry> Entries
    {
      get { return new ReadOnlyCollection<OwnedMaterialLedgerEntry>(entries); }
    }

    public int GrantedTotal
    {
      get
      {
        return entries
          .Where(entry => entry.Category != OwnedMaterialCategory.ConcreteEvaluationAdjustment)
          .Sum(entry => entry.Amount);
      }
    }

    public int ConcreteEvaluationAdjustment
    {
      get
      {
        return entries
          .Where(entry => entry.Category == OwnedMaterialCategory.ConcreteEvaluationAdjustment)
          .Sum(entry => entry.Amount);
      }
    }

    public void Add(OwnedMaterialCategory category, int amount, string stableId = null, string action = null)
    {
      if (amount != 0)
        entries.Add(new OwnedMaterialLedgerEntry(category, amount, stableId, action));
    }
  }

  internal sealed class RosterPiece
  {
    private readonly List<string> upgradePath = new List<string>();
    private readonly HashSet<string> promotionEntitlements = new HashSet<string>(StringComparer.Ordinal);
    private readonly HashSet<string> promotionEntitlementFamilies = new HashSet<string>(StringComparer.Ordinal);

    public RosterPiece(
      string stableId,
      SourcePlacementRole sourcePlacementRole,
      int sourceOrdinal,
      string roleOriginAction,
      FinalPieceFamily? finalFamily,
      PieceType concretePieceType,
      bool lockedCastler,
      int grantedMaterial,
      int finalExpectedMaterial,
      IEnumerable<string> initialUpgradePath = null,
      IEnumerable<string> initialPromotionEntitlementFamilies = null)
    {
      StableId = stableId;
      SourcePlacementRole = sourcePlacementRole;
      SourceOrdinal = sourceOrdinal;
      RoleOriginAction = roleOriginAction;
      FinalFamily = finalFamily;
      ConcretePieceType = concretePieceType;
      LockedCastler = lockedCastler;
      GrantedMaterial = grantedMaterial;
      FinalExpectedMaterial = finalExpectedMaterial;
      AddPromotionEntitlement(concretePieceType);
      if (concretePieceType != null && finalFamily.HasValue)
        AddPromotionEntitlementFamily(finalFamily.Value.ToString().ToLowerInvariant());
      if (initialUpgradePath != null)
        upgradePath.AddRange(initialUpgradePath.Where(action => !string.IsNullOrWhiteSpace(action)));
      if (initialPromotionEntitlementFamilies != null)
      {
        foreach (string family in initialPromotionEntitlementFamilies)
          AddPromotionEntitlementFamily(family);
      }
    }

    public string StableId { get; }
    public SourcePlacementRole SourcePlacementRole { get; private set; }
    public int SourceOrdinal { get; private set; }
    public string RoleOriginAction { get; private set; }
    public FinalPieceFamily? FinalFamily { get; private set; }
    public PieceType ConcretePieceType { get; private set; }
    public bool LockedCastler { get; private set; }
    public int GrantedMaterial { get; private set; }
    public int FinalExpectedMaterial { get; private set; }
    public int EvaluatedMidgame { get { return ConcretePieceType == null ? 0 : ConcretePieceType.MidgameValue; } }
    public int EvaluatedEndgame { get { return ConcretePieceType == null ? 0 : ConcretePieceType.EndgameValue; } }
    public IReadOnlyList<string> UpgradePath { get { return new ReadOnlyCollection<string>(upgradePath); } }
    public IReadOnlyCollection<string> PromotionEntitlements
    {
      get { return new ReadOnlyCollection<string>(promotionEntitlements.OrderBy(value => value).ToList()); }
    }
    public IReadOnlyCollection<string> PromotionEntitlementFamilies
    {
      get { return new ReadOnlyCollection<string>(promotionEntitlementFamilies.OrderBy(value => value).ToList()); }
    }

    public void EstablishSourceRole(SourcePlacementRole role, int ordinal, string action)
    {
      if (SourcePlacementRole != SourcePlacementRole.PawnSlot)
        return;
      SourcePlacementRole = role;
      SourceOrdinal = ordinal;
      RoleOriginAction = action;
    }

    public void ReclassifyAsLockedCastler(int ordinal)
    {
      SourcePlacementRole = SourcePlacementRole.LockedCastler;
      SourceOrdinal = ordinal;
      RoleOriginAction = "castler";
      LockedCastler = true;
    }

    public void Upgrade(string action, FinalPieceFamily family, PieceType concretePieceType, int materialCredit)
    {
      upgradePath.Add(action);
      FinalFamily = family;
      ConcretePieceType = concretePieceType;
      GrantedMaterial += materialCredit;
      FinalExpectedMaterial = OwnedRosterGeneration.ExpectedMaterial(family);
      AddPromotionEntitlement(concretePieceType);
      AddPromotionEntitlementFamily(family.ToString().ToLowerInvariant());
    }

    public void SetConcretePiece(PieceType concretePieceType)
    {
      ConcretePieceType = concretePieceType;
      AddPromotionEntitlement(concretePieceType);
      if (FinalFamily.HasValue)
        AddPromotionEntitlementFamily(FinalFamily.Value.ToString().ToLowerInvariant());
    }

    public void AddPromotionEntitlementFamily(string family)
    {
      if (!string.IsNullOrWhiteSpace(family))
        promotionEntitlementFamilies.Add(family);
    }

    private void AddPromotionEntitlement(PieceType piece)
    {
      if (piece == null || piece.Notation == null)
        return;
      int player = ApmwCore.getInstance().GeriProvider();
      if (player >= 0 && player < piece.Notation.Length && !string.IsNullOrEmpty(piece.Notation[player]))
        promotionEntitlements.Add(piece.Notation[player]);
    }
  }

  internal sealed class GeneratedRoster
  {
    public GeneratedRoster(
      PieceType primaryKing,
      IEnumerable<RosterPiece> rosterPieces,
      OwnedMaterialLedger ownedMaterialLedger,
      int unallocatedMaterial,
      string generationIdentity,
      int primaryKingGrantedMaterial = 0,
      int primaryKingExpectedMaterial = 0,
      int dormantMaterial = 0)
    {
      PrimaryKing = primaryKing;
      RosterPieces = new ReadOnlyCollection<RosterPiece>((rosterPieces ?? Enumerable.Empty<RosterPiece>()).ToList());
      OwnedMaterialLedger = ownedMaterialLedger;
      UnallocatedMaterial = Math.Max(0, unallocatedMaterial);
      GenerationIdentity = generationIdentity;
      PrimaryKingGrantedMaterial = Math.Max(0, primaryKingGrantedMaterial);
      PrimaryKingExpectedMaterial = Math.Max(0, primaryKingExpectedMaterial);
      DormantMaterial = Math.Max(0, dormantMaterial);
      PromotionEntitlements = new ReadOnlyCollection<string>(
        RosterPieces.SelectMany(piece => piece.PromotionEntitlements).Distinct().OrderBy(value => value).ToList());
    }

    public PieceType PrimaryKing { get; }
    public IReadOnlyList<RosterPiece> RosterPieces { get; }
    public IReadOnlyList<string> PromotionEntitlements { get; }
    public OwnedMaterialLedger OwnedMaterialLedger { get; }
    public int UnallocatedMaterial { get; }
    public string GenerationIdentity { get; }
    public int PrimaryKingGrantedMaterial { get; }
    public int PrimaryKingExpectedMaterial { get; }
    public int DormantMaterial { get; }
  }

  internal sealed class CounterBasedSeedSeries
  {
    private readonly string root;
    private readonly string seriesId;

    public CounterBasedSeedSeries(string root, string seriesId)
    {
      this.root = root ?? "";
      this.seriesId = seriesId ?? "";
    }

    public ulong Value(long counter)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(
        root + "\n" + seriesId + "\n" + counter.ToString(CultureInfo.InvariantCulture));
      using SHA256 sha = SHA256.Create();
      byte[] hash = sha.ComputeHash(bytes);
      return ((ulong)hash[0] << 56) |
        ((ulong)hash[1] << 48) |
        ((ulong)hash[2] << 40) |
        ((ulong)hash[3] << 32) |
        ((ulong)hash[4] << 24) |
        ((ulong)hash[5] << 16) |
        ((ulong)hash[6] << 8) |
        hash[7];
    }

    public int Index(long counter, int count)
    {
      if (count <= 0)
        throw new ArgumentOutOfRangeException(nameof(count));
      return (int)(Value(counter) % (ulong)count);
    }

    public double Unit(long counter)
    {
      return (Value(counter) >> 11) * (1.0 / (1UL << 53));
    }
  }

  internal static class ApmwSeedSeries
  {
    public static CounterBasedSeedSeries Semantic(ApmwConfig config, string seriesId)
    {
      return new CounterBasedSeedSeries(StableRoot(config), seriesId);
    }

    public static CounterBasedSeedSeries UpgradeSource(ApmwConfig config, string action)
    {
      return Semantic(config, UpgradeSourceSeriesId(action));
    }

    public static string UpgradeSourceSeriesId(string action)
    {
      if (string.IsNullOrWhiteSpace(action))
        throw new ArgumentException("Upgrade action is required.", nameof(action));
      return "upgrade-source." + action;
    }

    public static CounterBasedSeedSeries Presentation(ApmwConfig config, string seriesId)
    {
      bool chaos = config.Types == PieceTypes.Chaos || config.Locs == PieceLocations.Chaos;
      string root = PresentationRoot(config, seriesId, chaos);
      return new CounterBasedSeedSeries(root, seriesId);
    }

    public static CounterBasedSeedSeries SemanticPresentation(ApmwConfig config, string seriesId)
    {
      return new CounterBasedSeedSeries(PresentationRoot(config, seriesId, false), seriesId);
    }

    private static string StableRoot(ApmwConfig config)
    {
      return string.Join("|",
        SlotSeed(config, "pocket_seed"),
        SlotSeed(config, "pawn_seed"),
        SlotSeed(config, "minor_seed"),
        SlotSeed(config, "major_seed"),
        SlotSeed(config, "queen_seed"));
    }

    private static string SlotSeed(ApmwConfig config, string key)
    {
      if (config.SlotData != null && config.SlotData.TryGetValue(key, out object value) && value != null)
        return Convert.ToString(value, CultureInfo.InvariantCulture);
      return "0";
    }

    private static string PresentationRoot(ApmwConfig config, string seriesId, bool chaos)
    {
      if (seriesId.StartsWith("upgrade-target.", StringComparison.Ordinal))
      {
        if (seriesId.EndsWith("to-minor", StringComparison.Ordinal))
          return chaos ? config.minorSeed.ToString(CultureInfo.InvariantCulture) : SlotSeed(config, "minor_seed");
        if (seriesId.EndsWith("to-queen", StringComparison.Ordinal) ||
            seriesId.EndsWith("to-amazon", StringComparison.Ordinal))
          return chaos ? config.queenSeed.ToString(CultureInfo.InvariantCulture) : SlotSeed(config, "queen_seed");
        return chaos ? config.majorSeed.ToString(CultureInfo.InvariantCulture) : SlotSeed(config, "major_seed");
      }
      if (seriesId.Contains("pawn", StringComparison.Ordinal))
        return chaos ? config.pawnSeed.ToString(CultureInfo.InvariantCulture) : SlotSeed(config, "pawn_seed");
      if (seriesId.Contains("minor", StringComparison.Ordinal))
        return chaos ? config.minorSeed.ToString(CultureInfo.InvariantCulture) : SlotSeed(config, "minor_seed");
      if (seriesId.Contains("queen", StringComparison.Ordinal) ||
          seriesId.Contains("amazon", StringComparison.Ordinal))
        return chaos ? config.queenSeed.ToString(CultureInfo.InvariantCulture) : SlotSeed(config, "queen_seed");
      if (seriesId.Contains("major", StringComparison.Ordinal) ||
          seriesId.Contains("jack", StringComparison.Ordinal) ||
          seriesId.Contains("castler", StringComparison.Ordinal))
        return chaos ? config.majorSeed.ToString(CultureInfo.InvariantCulture) : SlotSeed(config, "major_seed");
      return chaos
        ? string.Join("|", config.pawnSeed, config.minorSeed, config.majorSeed, config.queenSeed)
        : StableRoot(config);
    }
  }

  internal static class OwnedRosterGeneration
  {
    internal sealed class FamilyPieceChooser
    {
      private readonly Dictionary<string, long> counters = new Dictionary<string, long>(StringComparer.Ordinal);
      private readonly Dictionary<FinalPieceFamily, Dictionary<PieceType, int>> chosen =
        new Dictionary<FinalPieceFamily, Dictionary<PieceType, int>>();

      public PieceType Choose(FinalPieceFamily family, ApmwConfig config, string seriesId = null)
      {
        List<PieceType> options = ArmyPieceFilter.Filter(PiecesForFamily(family))
          .OrderBy(piece => piece.Name, StringComparer.Ordinal)
          .ThenBy(piece => piece.Notation == null ? "" : piece.Notation[0], StringComparer.Ordinal)
          .ToList();
        if (options.Count == 0)
          return null;

        string effectiveSeriesId = seriesId ?? "piece-type." + FamilyId(family);
        long counter = counters.TryGetValue(effectiveSeriesId, out long current) ? current : 0;
        counters[effectiveSeriesId] = counter + 1;
        CounterBasedSeedSeries series = ApmwSeedSeries.Presentation(
          config,
          effectiveSeriesId);
        int limit = TypeLimit(family, config);
        if (limit <= 0)
          return options[series.Index(counter, options.Count)];

        if (!chosen.TryGetValue(family, out Dictionary<PieceType, int> familyChosen))
        {
          familyChosen = new Dictionary<PieceType, int>();
          chosen[family] = familyChosen;
        }

        List<PieceType> eligible = options
          .Where(piece => !familyChosen.TryGetValue(piece, out int count) || count < limit)
          .ToList();
        if (eligible.Count == 0)
          eligible = options;
        PieceType selected = eligible[series.Index(counter, eligible.Count)];
        familyChosen[selected] = familyChosen.TryGetValue(selected, out int selectedCount) ? selectedCount + 1 : 1;
        return selected;
      }
    }

    public static GeneratedRoster Generate()
    {
      ApmwCore core = ApmwCore.getInstance();
      ApmwConfig config = ApmwConfig.getInstance();
      config.seed();
      return config.UsesFundamentalProgressionItemization
        ? GenerateFundamental(core, config)
        : GenerateLegacy(core, config);
    }

    internal static int ExpectedMaterial(FinalPieceFamily family)
    {
      switch (family)
      {
        case FinalPieceFamily.Pawn: return ItemGenerationValues.Pawn;
        case FinalPieceFamily.Minor: return ItemGenerationValues.Minor;
        case FinalPieceFamily.Major: return ItemGenerationValues.Major;
        case FinalPieceFamily.Jack: return ItemGenerationValues.Jack;
        case FinalPieceFamily.Queen: return ItemGenerationValues.Queen;
        case FinalPieceFamily.Amazon: return ItemGenerationValues.Amazon;
        default: throw new ArgumentOutOfRangeException(nameof(family));
      }
    }

    private static GeneratedRoster GenerateLegacy(ApmwCore core, ApmwConfig config)
    {
      var ledger = new OwnedMaterialLedger();
      var pieces = new List<RosterPiece>();
      var chooser = new FamilyPieceChooser();
      NonPawnGenerationPlan plan = NonPawnUpgradeGeneration.Plan(core, config);

      AddDirectFamily(pieces, ledger, chooser, config, FinalPieceFamily.Minor, SourcePlacementRole.MinorSlot,
        plan.DirectCount(NonPawnPieceFamily.Minor));
      AddDirectFamily(pieces, ledger, chooser, config, FinalPieceFamily.Major, SourcePlacementRole.MajorSlot,
        plan.DirectCount(NonPawnPieceFamily.Major));
      AddDirectFamily(pieces, ledger, chooser, config, FinalPieceFamily.Jack, SourcePlacementRole.JackSlot,
        plan.DirectCount(NonPawnPieceFamily.Jack));

      ApplyNonPawnUpgrades(pieces, ledger, chooser, config, plan);

      int pawnCount = Math.Max(0, core.foundPawns);
      for (int ordinal = 0; ordinal < pawnCount; ordinal++)
      {
        PieceType pawn = ChoosePawn(config, ordinal);
        string stableId = StableId(SourcePlacementRole.PawnSlot, ordinal);
        var piece = new RosterPiece(
          stableId,
          SourcePlacementRole.PawnSlot,
          ordinal,
          "new-pawn",
          FinalPieceFamily.Pawn,
          pawn,
          false,
          ItemGenerationValues.Pawn,
          ItemGenerationValues.Pawn);
        pieces.Add(piece);
        ledger.Add(OwnedMaterialCategory.DirectPiece, ItemGenerationValues.Pawn, stableId, "new-pawn");
      }

      AddAdditionalRoyals(pieces, ledger, core);
      int dormant = plan.UnusedUpgradeCounts.Sum(pair =>
        pair.Value * (pair.Key == NonPawnPieceFamily.Queen
          ? ItemGenerationValues.Queen - ItemGenerationValues.Major
          : ItemGenerationValues.Amazon - ItemGenerationValues.Queen));
      ledger.Add(OwnedMaterialCategory.Dormant, dormant, null, "parentless-upgrade");
      int unallocated = RecordCommonOutsideRosterGrants(ledger, core);
      foreach (RosterPiece piece in pieces)
        AddConcreteAdjustment(ledger, piece);
      return BuildRoster(core, config, pieces, ledger, unallocated, dormant, "legacy");
    }

    private static GeneratedRoster GenerateFundamental(ApmwCore core, ApmwConfig config)
    {
      var ledger = new OwnedMaterialLedger();
      var pieces = new List<RosterPiece>();
      var chooser = new FamilyPieceChooser();
      PieceGenerationAllocation allocation = FundamentalSlotGraduationPlanner.PlanOwnedRoster(core, config);
      int chessmen = Math.Max(0, core.foundChessmen);

      for (int ordinal = 0; ordinal < chessmen; ordinal++)
      {
        string stableId = "chessman:" + ordinal.ToString("D6", CultureInfo.InvariantCulture);
        var piece = new RosterPiece(
          stableId,
          SourcePlacementRole.PawnSlot,
          ordinal,
          "chessman",
          FinalPieceFamily.Pawn,
          null,
          false,
          ItemGenerationValues.Pawn,
          ItemGenerationValues.Pawn);
        pieces.Add(piece);
        ledger.Add(OwnedMaterialCategory.ChessmanBase, ItemGenerationValues.Pawn, stableId, "chessman");
      }

      // TODO(chesslogic): These isolated Stable Fundamental series guarantee deterministic
      // current-snapshot planning, not preservation of every previously displayed concrete
      // piece; see FundamentalSlotGraduationPlannerTests' fixed-Material characterization.
      CounterBasedSeedSeries gatewaySeries = ApmwSeedSeries.Semantic(config, "fundamental.gateway.role");
      int lockedCount = Math.Min(allocation.LockedMajorCount, pieces.Count);
      List<RosterPiece> unlocked = pieces.ToList();
      for (int ordinal = 0; ordinal < lockedCount; ordinal++)
      {
        int selectedIndex = gatewaySeries.Index(ordinal, unlocked.Count);
        RosterPiece selected = unlocked[selectedIndex];
        unlocked.RemoveAt(selectedIndex);
        selected.ReclassifyAsLockedCastler(ordinal);
        selected.Upgrade(
          "castler",
          FinalPieceFamily.Major,
          chooser.Choose(FinalPieceFamily.Major, config),
          ItemGenerationValues.Castler);
        ledger.Add(
          OwnedMaterialCategory.LockedCastler,
          ItemGenerationValues.Castler,
          selected.StableId,
          "castler");
      }

      int minorOrdinal = 0;
      int majorOrdinal = 0;
      ApplyFundamentalGateway(
        pieces,
        ledger,
        chooser,
        config,
        allocation.AppliedGraduationCount(ApmwConstants.PieceUpgradeActions.PawnToMinor),
        ApmwConstants.PieceUpgradeActions.PawnToMinor,
        SourcePlacementRole.MinorSlot,
        FinalPieceFamily.Minor,
        ref minorOrdinal);
      ApplyFundamentalGateway(
        pieces,
        ledger,
        chooser,
        config,
        allocation.AppliedGraduationCount(ApmwConstants.PieceUpgradeActions.PawnToMajor),
        ApmwConstants.PieceUpgradeActions.PawnToMajor,
        SourcePlacementRole.MajorSlot,
        FinalPieceFamily.Major,
        ref majorOrdinal);

      ApplyNonPawnUpgrades(pieces, ledger, chooser, config, allocation.PrecomputedNonPawnPlan);
      int unallocated = allocation.InitialSpareMaterial;
      unallocated -= ApplyLockedCastlerJackUpgrades(
        pieces,
        ledger,
        chooser,
        config,
        unallocated);
      foreach (RosterPiece pawn in pieces.Where(piece => piece.FinalFamily == FinalPieceFamily.Pawn))
        pawn.SetConcretePiece(ChoosePawn(config, pawn.SourceOrdinal));

      ledger.Add(OwnedMaterialCategory.Unallocated, unallocated, null, "unspent-fundamental-material");
      unallocated += RecordCommonOutsideRosterGrants(ledger, core);
      AddAdditionalRoyals(pieces, ledger, core);
      foreach (RosterPiece piece in pieces)
        AddConcreteAdjustment(ledger, piece);
      return BuildRoster(core, config, pieces, ledger, unallocated, 0, "fundamental");
    }

    private static int ApplyLockedCastlerJackUpgrades(
      List<RosterPiece> pieces,
      OwnedMaterialLedger ledger,
      FamilyPieceChooser chooser,
      ApmwConfig config,
      int availableMaterial)
    {
      string action = ApmwConstants.PieceUpgradeActions.MajorToJack;
      if (!config.PieceUpgradeActions.TryGetValue(
          action,
          out ApmwConfig.PieceUpgradeActionResolution resolution) ||
        !resolution.IsEnabled ||
        resolution.Priority <= 0)
      {
        return 0;
      }

      int materialCredit = ItemGenerationValues.Jack - ItemGenerationValues.Castler;
      int upgradeCount = Math.Min(
        pieces.Count(piece => piece.LockedCastler && piece.FinalFamily == FinalPieceFamily.Major),
        Math.Max(0, availableMaterial) / materialCredit);
      List<RosterPiece> candidates = pieces
        .Where(piece => piece.LockedCastler && piece.FinalFamily == FinalPieceFamily.Major)
        .ToList();
      CounterBasedSeedSeries series = ApmwSeedSeries.UpgradeSource(config, action);
      for (int index = 0; index < upgradeCount; index++)
      {
        int candidateIndex = series.Index(index, candidates.Count);
        RosterPiece piece = candidates[candidateIndex];
        candidates.RemoveAt(candidateIndex);
        piece.Upgrade(
          action,
          FinalPieceFamily.Jack,
          chooser.Choose(FinalPieceFamily.Jack, config, "upgrade-target." + action),
          materialCredit);
        ledger.Add(OwnedMaterialCategory.Upgrade, materialCredit, piece.StableId, action);
      }
      return upgradeCount * materialCredit;
    }

    private static void ApplyFundamentalGateway(
      List<RosterPiece> pieces,
      OwnedMaterialLedger ledger,
      FamilyPieceChooser chooser,
      ApmwConfig config,
      int count,
      string action,
      SourcePlacementRole role,
      FinalPieceFamily family,
      ref int roleOrdinal)
    {
      List<RosterPiece> candidates = pieces
        .Where(piece => !piece.LockedCastler && piece.FinalFamily == FinalPieceFamily.Pawn)
        .ToList();
      CounterBasedSeedSeries series = ApmwSeedSeries.UpgradeSource(config, action);
      for (int index = 0; index < count && candidates.Count > 0; index++)
      {
        int candidateIndex = series.Index(index, candidates.Count);
        RosterPiece piece = candidates[candidateIndex];
        candidates.RemoveAt(candidateIndex);
        piece.EstablishSourceRole(role, roleOrdinal++, action);
        int credit = ExpectedMaterial(family) - ItemGenerationValues.Pawn;
        piece.Upgrade(
          action,
          family,
          chooser.Choose(family, config, "upgrade-target." + action),
          credit);
        ledger.Add(OwnedMaterialCategory.Upgrade, credit, piece.StableId, action);
      }
    }

    private static void ApplyNonPawnUpgrades(
      List<RosterPiece> pieces,
      OwnedMaterialLedger ledger,
      FamilyPieceChooser chooser,
      ApmwConfig config,
      NonPawnGenerationPlan plan)
    {
      if (plan == null)
        return;
      foreach (PlannedNonPawnUpgradeAction planned in plan.UpgradeActions)
      {
        FinalPieceFamily sourceFamily = ToFinalFamily(planned.Metadata.SourceFamily);
        FinalPieceFamily targetFamily = ToFinalFamily(planned.Metadata.TargetFamily);
        List<RosterPiece> candidates = pieces
          .Where(piece => !piece.LockedCastler && piece.FinalFamily == sourceFamily)
          .ToList();
        CounterBasedSeedSeries series =
          ApmwSeedSeries.UpgradeSource(config, planned.Metadata.ActionName);
        for (int index = 0; index < planned.RequestedUpgrades && candidates.Count > 0; index++)
        {
          int candidateIndex = series.Index(index, candidates.Count);
          RosterPiece piece = candidates[candidateIndex];
          candidates.RemoveAt(candidateIndex);
          PieceType concrete = chooser.Choose(
            targetFamily,
            config,
            "upgrade-target." + planned.Metadata.ActionName);
          piece.Upgrade(planned.Metadata.ActionName, targetFamily, concrete, planned.Metadata.UpgradeMaterialCredit);
          ledger.Add(OwnedMaterialCategory.Upgrade, planned.Metadata.UpgradeMaterialCredit,
            piece.StableId, planned.Metadata.ActionName);
        }
      }
    }

    private static void AddDirectFamily(
      List<RosterPiece> pieces,
      OwnedMaterialLedger ledger,
      FamilyPieceChooser chooser,
      ApmwConfig config,
      FinalPieceFamily family,
      SourcePlacementRole role,
      int count)
    {
      for (int ordinal = 0; ordinal < Math.Max(0, count); ordinal++)
      {
        string stableId = StableId(role, ordinal);
        var piece = new RosterPiece(
          stableId,
          role,
          ordinal,
          "direct-" + FamilyId(family),
          family,
          chooser.Choose(family, config),
          false,
          ExpectedMaterial(family),
          ExpectedMaterial(family));
        pieces.Add(piece);
        ledger.Add(OwnedMaterialCategory.DirectPiece, ExpectedMaterial(family), stableId, piece.RoleOriginAction);
      }
    }

    private static void AddAdditionalRoyals(List<RosterPiece> pieces, OwnedMaterialLedger ledger, ApmwCore core)
    {
      int count = Math.Max(0, core.foundConsuls);
      for (int ordinal = 0; ordinal < count; ordinal++)
      {
        string stableId = StableId(SourcePlacementRole.AdditionalRoyal, ordinal);
        PieceType concrete = core.kings == null || core.kings.Count == 0 ? null : core.kings[0];
        var piece = new RosterPiece(
          stableId,
          SourcePlacementRole.AdditionalRoyal,
          ordinal,
          "consul",
          null,
          concrete,
          false,
          ItemGenerationValues.Consul,
          ItemGenerationValues.Consul);
        piece.AddPromotionEntitlementFamily("royal");
        pieces.Add(piece);
        ledger.Add(OwnedMaterialCategory.AdditionalRoyal, ItemGenerationValues.Consul, stableId, "consul");
      }
    }

    private static GeneratedRoster BuildRoster(
      ApmwCore core,
      ApmwConfig config,
      List<RosterPiece> pieces,
      OwnedMaterialLedger ledger,
      int unallocated,
      int dormant,
      string itemization)
    {
      PieceType primaryKing = null;
      if (core.kings != null && core.kings.Count > 0)
      {
        int kingIndex = Math.Min(Math.Max(0, core.foundKingPromotions), core.kings.Count - 1);
        primaryKing = core.kings[kingIndex];
      }
      string ordering = config.Types == PieceTypes.Chaos || config.Locs == PieceLocations.Chaos ? "chaos" : "stable";
      int primaryKingMaterial = Math.Max(0, core.foundKingPromotions) * ItemGenerationValues.KingPromotion;
      ledger.Add(OwnedMaterialCategory.PrimaryRoyal, primaryKingMaterial, "primary-royal:000000", "king-promotion");
      return new GeneratedRoster(
        primaryKing,
        pieces,
        ledger,
        unallocated,
        itemization + "-" + ordering,
        primaryKingMaterial,
        primaryKingMaterial,
        dormant);
    }

    private static int RecordCommonOutsideRosterGrants(OwnedMaterialLedger ledger, ApmwCore core)
    {
      int playAsWhite = Math.Min(
        ApmwEffectiveMaxima.PlayAsWhite,
        Math.Max(0, core.foundPlayAsWhite)) * ItemGenerationValues.PlayAsWhite;
      int pockets = Math.Min(
        ApmwEffectiveMaxima.Pocket,
        Math.Max(0, core.foundPockets)) * ItemGenerationValues.Pocket;
      ledger.Add(OwnedMaterialCategory.Unallocated, playAsWhite, null, ApmwConstants.ProgressiveItems.PlayAsWhite);
      ledger.Add(OwnedMaterialCategory.Unallocated, pockets, null, ApmwConstants.ProgressiveItems.Pocket);
      return playAsWhite + pockets;
    }

    internal static PieceType ChoosePawn(ApmwConfig config, int ordinal)
    {
      List<PieceType> options = PawnGeneration.SetupPawnOptions()
        .OrderBy(piece => piece.Name, StringComparer.Ordinal)
        .ThenBy(piece => piece.Notation == null ? "" : piece.Notation[0], StringComparer.Ordinal)
        .ToList();
      if (options.Count == 0)
        return null;
      return options[ApmwSeedSeries.Presentation(config, "piece-type.pawn").Index(ordinal, options.Count)];
    }

    private static void AddConcreteAdjustment(OwnedMaterialLedger ledger, RosterPiece piece)
    {
      if (piece.ConcretePieceType != null)
      {
        ledger.Add(
          OwnedMaterialCategory.ConcreteEvaluationAdjustment,
          piece.ConcretePieceType.MidgameValue - piece.FinalExpectedMaterial,
          piece.StableId);
      }
    }

    private static string StableId(SourcePlacementRole role, int ordinal)
    {
      return RoleId(role) + ":" + ordinal.ToString("D6", CultureInfo.InvariantCulture);
    }

    private static string RoleId(SourcePlacementRole role)
    {
      switch (role)
      {
        case SourcePlacementRole.PrimaryRoyal: return "primary-royal";
        case SourcePlacementRole.AdditionalRoyal: return "additional-royal";
        case SourcePlacementRole.LockedCastler: return "locked-castler";
        case SourcePlacementRole.JackSlot: return "jack-slot";
        case SourcePlacementRole.MajorSlot: return "major-slot";
        case SourcePlacementRole.MinorSlot: return "minor-slot";
        case SourcePlacementRole.PawnSlot: return "pawn-slot";
        default: throw new ArgumentOutOfRangeException(nameof(role));
      }
    }

    private static string FamilyId(FinalPieceFamily family)
    {
      return family.ToString().ToLowerInvariant();
    }

    private static FinalPieceFamily ToFinalFamily(NonPawnPieceFamily family)
    {
      return (FinalPieceFamily)((int)family + 1);
    }

    private static IEnumerable<PieceType> PiecesForFamily(FinalPieceFamily family)
    {
      ApmwCore core = ApmwCore.getInstance();
      switch (family)
      {
        case FinalPieceFamily.Pawn: return core.pawns ?? Enumerable.Empty<PieceType>();
        case FinalPieceFamily.Minor: return core.minors ?? Enumerable.Empty<PieceType>();
        case FinalPieceFamily.Major: return core.majors ?? Enumerable.Empty<PieceType>();
        case FinalPieceFamily.Jack: return core.jacks ?? Enumerable.Empty<PieceType>();
        case FinalPieceFamily.Queen: return core.queens ?? Enumerable.Empty<PieceType>();
        case FinalPieceFamily.Amazon: return core.amazons ?? Enumerable.Empty<PieceType>();
        default: return Enumerable.Empty<PieceType>();
      }
    }

    private static int TypeLimit(FinalPieceFamily family, ApmwConfig config)
    {
      switch (family)
      {
        case FinalPieceFamily.Minor: return config.minorTypeLimit;
        case FinalPieceFamily.Major:
        case FinalPieceFamily.Jack: return config.majorTypeLimit;
        case FinalPieceFamily.Queen:
        case FinalPieceFamily.Amazon: return config.queenTypeLimit;
        default: return -1;
      }
    }
  }
}
