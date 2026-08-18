using ChessV.Games;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.APChessV
{
  public sealed record ApmwGeometryOption(
    string StageId,
    int Files,
    int Ranks,
    string DisplayLabel,
    string RegisteredGameName)
  {
    public override string ToString()
    {
      return DisplayLabel;
    }
  }

  public sealed record ApmwGeometryUnlockSnapshot(
    int BoardFileUnlockCount,
    int BoardRankUnlockCount,
    bool LegacySuperSizeUnlocked)
  {
    public static ApmwGeometryUnlockSnapshot Empty { get; } =
      new ApmwGeometryUnlockSnapshot(0, 0, false);
  }

  public static class ApmwGeometryResolver
  {
    private static readonly IReadOnlyDictionary<string, string> RegisteredGameNames =
      new ReadOnlyDictionary<string, string>(
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
          { "6x8", ApmwProfiles.SixByEightGameName },
          { "8x8", ApmwProfiles.StandardGameName },
          { "10x8", ApmwProfiles.GrandGameName },
          { "10x10", ApmwProfiles.TenByTenGameName },
          { "12x10", ApmwProfiles.TwelveByTenGameName },
          { "12x12", ApmwProfiles.TwelveByTwelveGameName },
        });

    public static IReadOnlyList<ApmwGeometryOption> ResolveCurrent(
      ApmwContractV2 contract,
      int boardFileUnlockCount,
      int boardRankUnlockCount)
    {
      if (contract == null)
        throw new ArgumentNullException(nameof(contract));

      GeometryUnlockRole fileRole = contract.GeometryUnlocks.Roles.Single(
        role => role.RoleId == "board-file-unlock");
      GeometryUnlockRole rankRole = contract.GeometryUnlocks.Roles.Single(
        role => role.RoleId == "board-rank-unlock");
      int effectiveFiles = EffectiveDimension(fileRole, boardFileUnlockCount);
      int effectiveRanks = EffectiveDimension(rankRole, boardRankUnlockCount);

      return new ReadOnlyCollection<ApmwGeometryOption>(
        contract.Stages
          .Where(stage => stage.Files <= effectiveFiles && stage.Ranks <= effectiveRanks)
          .Select(stage => ToRegisteredOption(stage.StageId, stage.Files, stage.Ranks))
          .ToList());
    }

    public static IReadOnlyList<ApmwGeometryOption> ResolveCurrent(
      ApmwContractV2 contract,
      int boardFileUnlockCount,
      int boardRankUnlockCount,
      Goal goal)
    {
      if (contract == null)
        throw new ArgumentNullException(nameof(contract));
      if (!ApmwGoalSemantics.UsesSixByEightOpening(goal))
        return ResolveCurrent(contract, boardFileUnlockCount, boardRankUnlockCount);

      var options = new List<ApmwGeometryOption>
      {
        ToRegisteredOption("6x8", 6, 8),
      };
      if (boardFileUnlockCount > 0)
      {
        options.AddRange(ResolveCurrent(
          contract,
          boardFileUnlockCount - 1,
          boardRankUnlockCount));
      }
      return new ReadOnlyCollection<ApmwGeometryOption>(options);
    }

    public static IReadOnlyList<ApmwGeometryOption> ResolveLegacy(bool superSizeUnlocked)
    {
      var options = new List<ApmwGeometryOption>
      {
        ToRegisteredOption("8x8", 8, 8),
      };
      if (superSizeUnlocked)
        options.Add(ToRegisteredOption("10x8", 10, 8));
      return new ReadOnlyCollection<ApmwGeometryOption>(options);
    }

    public static ApmwContractV2 ParseCurrentContract(object rawContract)
    {
      if (rawContract == null)
        throw new ApmwContractException("apmw_contract is null; expected the v2 manifest object.");

      string json;
      if (rawContract is string stringContract)
      {
        json = stringContract;
      }
      else if (rawContract is JValue value && value.Type == JTokenType.String)
      {
        json = (string)value.Value;
      }
      else if (rawContract is JToken token)
      {
        json = token.ToString(Formatting.None);
      }
      else
      {
        try
        {
          json = JsonConvert.SerializeObject(rawContract, Formatting.None);
        }
        catch (JsonException exception)
        {
          throw new ApmwContractException(
            "apmw_contract could not be serialized as a v2 manifest object.",
            exception);
        }
      }

      return ApmwContractV2Parser.Parse(json);
    }

    private static int EffectiveDimension(GeometryUnlockRole role, int receivedCount)
    {
      int maximumCount = (role.Maximum - role.Base) / role.Increment;
      int effectiveCount = Math.Min(maximumCount, Math.Max(0, receivedCount));
      return role.Base + role.Increment * effectiveCount;
    }

    private static ApmwGeometryOption ToRegisteredOption(string stageId, int files, int ranks)
    {
      if (!RegisteredGameNames.TryGetValue(stageId, out string gameName))
      {
        throw new NotSupportedException(
          "The APMW contract geometry " + stageId + " has no statically registered ChessV game.");
      }

      return new ApmwGeometryOption(stageId, files, ranks, files + "x" + ranks, gameName);
    }
  }

  public sealed class ApmwGeometrySelectionModel
  {
    private IReadOnlyList<ApmwGeometryOption> availableOptions;

    public ApmwGeometrySelectionModel()
    {
      Reset();
    }

    public bool IsConnected { get; private set; }
    public IReadOnlyList<ApmwGeometryOption> AvailableOptions { get { return availableOptions; } }
    public ApmwGeometryOption SelectedOption { get; private set; }

    public void Connect(IReadOnlyList<ApmwGeometryOption> options)
    {
      Apply(options, true);
      IsConnected = true;
    }

    public void Refresh(IReadOnlyList<ApmwGeometryOption> options)
    {
      Apply(options, false);
    }

    public bool Select(string stageId)
    {
      ApmwGeometryOption option = availableOptions.FirstOrDefault(
        candidate => string.Equals(candidate.StageId, stageId, StringComparison.Ordinal));
      if (option == null)
        return false;
      SelectedOption = option;
      return true;
    }

    public void Reset()
    {
      availableOptions = ApmwGeometryResolver.ResolveLegacy(false);
      SelectedOption = availableOptions[0];
      IsConnected = false;
    }

    private void Apply(IReadOnlyList<ApmwGeometryOption> options, bool selectLargest)
    {
      if (options == null)
        throw new ArgumentNullException(nameof(options));
      if (options.Count == 0)
        throw new InvalidOperationException("At least one APMW geometry must be available.");

      string selectedStageId = SelectedOption == null ? null : SelectedOption.StageId;
      availableOptions = new ReadOnlyCollection<ApmwGeometryOption>(options.ToList());
      SelectedOption = selectLargest
        ? availableOptions[availableOptions.Count - 1]
        : availableOptions.FirstOrDefault(option => option.StageId == selectedStageId) ??
          availableOptions[availableOptions.Count - 1];
    }
  }
}
