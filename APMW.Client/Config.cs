using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.APChessV
{
  public enum Goal
  {
    Single = 0, OrderedProgressive = 1, Progressive = 2, Super = 3
  }
  public enum PieceLocations
  {
    Chaos = 0, Stable = 1, Ordered = 2,
  }
  /** Applies to Player and Enemy */
  public enum PieceTypes
  {
    Chaos = 0, Stable = 1, Book = 2,
  }
  public enum FairyTypes
  {
    Vanilla = 0, Full = 1, CwDA = 2, Cannon = 3, Eurasian = 4, // not used I guess
  }
  public enum FairyArmy
  {
    Chaos = 0, Stable = 1, Limited = 2,
  }
  public enum FairyPawns
  {
    Vanilla = 0, Mixed = 1, Berolina = 2, Checkers = 3, Reserved = 4, AnyPawn = 5, AnyFairy = 6, AnyClassical = 7
  }
  public enum FairyPawnUpgrades
  {
    Off = 0,
    Pool = 1,
    Max = 2,
    SuperMax = 3,
    Configure = 4
  }
  public enum ProgressionItemization
  {
    Legacy = 0,
    Fundamental = 1
  }

  public class ApmwConfig
  {
    public const int DefaultMaterialItemValue = 400;
    public const int DefaultCastlingLocationCount = 2;

    public sealed class PieceUpgradeActionResolution
    {
      public PieceUpgradeActionResolution(string actionName, int priority, bool isEnabled, double proportion = 1.0)
      {
        ActionName = actionName;
        Priority = priority;
        IsEnabled = isEnabled;
        Proportion = proportion > 0 ? proportion : 0;
      }

      public string ActionName { get; private set; }
      public int Priority { get; private set; }
      public bool IsEnabled { get; private set; }
      public bool IsDisabled { get { return !IsEnabled; } }

      /// <summary>
      /// Relative per-draw weight used only to arbitrate among actions tied at the same
      /// <see cref="Priority"/> (see <see cref="ApmwConstants.SlotKeyPieceUpgradeRatio"/>).
      /// Never negative; defaults to 1 when not explicitly configured.
      /// </summary>
      public double Proportion { get; private set; }
    }

    private static readonly string[] ValidPieceUpgradeActions =
    {
      ApmwConstants.PieceUpgradeActions.NewPawn,
      ApmwConstants.PieceUpgradeActions.MorePawn,
      ApmwConstants.PieceUpgradeActions.BetterPawn,
      ApmwConstants.PieceUpgradeActions.PoolPawnUpgrade,
      ApmwConstants.PieceUpgradeActions.PawnToMinor,
      ApmwConstants.PieceUpgradeActions.PawnToMajor,
      ApmwConstants.PieceUpgradeActions.MinorToMajor,
      ApmwConstants.PieceUpgradeActions.MajorToJack,
      ApmwConstants.PieceUpgradeActions.MinorToJack,
      ApmwConstants.PieceUpgradeActions.MajorToQueen,
      ApmwConstants.PieceUpgradeActions.JackToQueen,
      ApmwConstants.PieceUpgradeActions.QueenToAmazon,
    };

    private static readonly HashSet<string> ValidPieceUpgradePreferences =
      new HashSet<string>(ValidPieceUpgradeActions, StringComparer.Ordinal);

    private static readonly Dictionary<string, int> PieceUpgradeActionOrder =
      ValidPieceUpgradeActions
        .Select((actionName, index) => new { actionName, index })
        .ToDictionary(item => item.actionName, item => item.index, StringComparer.Ordinal);

    private static readonly IReadOnlyDictionary<string, PieceUpgradeActionResolution> DefaultPieceUpgradeActions =
      ResolvePieceUpgradeActionsFromLegacyDefaults(FairyPawnUpgrades.Off, ProgressionItemization.Legacy, null).Actions;

    public static ApmwConfig _instance;
    public static ApmwConfig getInstance()
    {
      if (_instance == null)
      {
        lock (typeof(ApmwConfig))
        {
          if (_instance == null)
          {
            _instance = new ApmwConfig();
          }
        }
      }
      return _instance;
    }

    public Dictionary<string, object> SlotData { get; private set; }
    public bool UsesCurrentContract { get; private set; }
    public ApmwContractV2 CurrentContract { get; private set; }

    public int pocketSeed = -1;
    public List<int> pocketChoiceSeed { get; private set; } = new List<int>();
    public int pawnSeed = -1;
    public int pawnLocSeed = -1;
    public int minorSeed = -1;
    public int minorLocSeed = -1;
    public int majorSeed = -1;
    public int majorLocSeed = -1;
    public int queenSeed = -1;
    public int queenLocSeed = -1;
    public int fundamentalGraduationSeed = -1;

    internal const string DeterministicChaosSeedSlotKeyForTest = "deterministic_chaos_seed";
    internal int? DeterministicChaosSeedForTest { get; set; }

    public int minorTypeLimit = -1;
    public int majorTypeLimit = -1;
    public int queenTypeLimit = -1;
    public int pocketLimit = -1;
    public int materialItemValue = DefaultMaterialItemValue;
    public int castlingLocationCount = DefaultCastlingLocationCount;
    public List<int> Army = new List<int>();

    private Goal goal;
    public Goal Goal { get { return goal; } }
    public int GoalInt
    {
      set
      {
        goal = (Goal)value;
      }
    }
    private PieceLocations locs;
    public PieceLocations Locs { get { return locs; } }
    public int LocsInt
    {
      set
      {
        locs = (PieceLocations)value;
      }
    }
    private PieceTypes types;
    public PieceTypes Types { get { return types; } }
    public int TypesInt
    {
      set
      {
        types = (PieceTypes)value;
      }
    }
    private PieceTypes enemyTypes;
    public PieceTypes EnemyTypes { get { return enemyTypes; } }
    public int EnemyTypesInt
    {
      set
      {
        enemyTypes = (PieceTypes)value;
      }
    }
    private FairyArmy fairyArmy;
    /// <summary>
    /// Will be used to implement Stable vs Limited - Limited will randomize the Army value and ignore slotdata
    /// </summary>
    public FairyArmy FairyArmy { get { return fairyArmy; } }
    public int FairyArmyInt
    {
      set
      {
        fairyArmy = (FairyArmy)value;
      }
    }
    private FairyPawns pawns;
    public FairyPawns Pawns { get { return pawns; } }
    public int PawnsInt
    {
      set
      {
        pawns = (FairyPawns)value;
      }
    }
    private FairyPawnUpgrades pawnUpgrades;
    public FairyPawnUpgrades PawnUpgrades { get { return pawnUpgrades; } }
    public List<string> PieceUpgradePreferences { get; private set; } =
      ResolvePieceUpgradeActionsFromLegacyDefaults(FairyPawnUpgrades.Off, ProgressionItemization.Legacy, null).Preferences;
    public IReadOnlyDictionary<string, PieceUpgradeActionResolution> PieceUpgradeActions { get; private set; } =
      DefaultPieceUpgradeActions;
    public int PawnUpgradesInt
    {
      set
      {
        SetPawnUpgrades(ReadLegacyPawnUpgradeMode(value));
      }
    }

    private void SetPawnUpgrades(FairyPawnUpgrades value)
    {
      pawnUpgrades = value;
      // Always resolves Legacy-mode defaults here, independent of the shared config's current
      // ProgressionItemization: Instantiate() immediately re-resolves and overwrites
      // PieceUpgradePreferences/PieceUpgradeActions afterward using the real mode (see below), so
      // this setter's own resolution only actually matters for tests that poke PawnUpgradesInt
      // directly (bypassing Instantiate) -- and those are all Legacy pawn-distribution tests.
      var pieceUpgradeResolution = ResolvePieceUpgradeActionsFromLegacyDefaults(pawnUpgrades, ProgressionItemization.Legacy, null);
      PieceUpgradePreferences = pieceUpgradeResolution.Preferences;
      PieceUpgradeActions = pieceUpgradeResolution.Actions;
    }

    public bool IsPieceUpgradeActionEnabled(string actionName)
    {
      PieceUpgradeActionResolution action;
      return PieceUpgradeActions.TryGetValue(actionName, out action) && action.IsEnabled;
    }

    public bool IsPieceUpgradeActionPreferredBefore(string actionName, string laterActionName)
    {
      PieceUpgradeActionResolution action;
      if (!PieceUpgradeActions.TryGetValue(actionName, out action) || !action.IsEnabled)
        return false;

      PieceUpgradeActionResolution laterAction;
      if (!PieceUpgradeActions.TryGetValue(laterActionName, out laterAction) || !laterAction.IsEnabled)
        return true;

      return action.Priority > laterAction.Priority;
    }

    public bool UsesSuperMaxPawnGuarantee
    {
      get { return PawnUpgrades == FairyPawnUpgrades.SuperMax; }
    }

    private ProgressionItemization progressionItemization;
    public ProgressionItemization ProgressionItemization { get { return progressionItemization; } }
    public bool UsesFundamentalProgressionItemization
    {
      get { return progressionItemization == ProgressionItemization.Fundamental; }
    }

    /// <summary>
    /// True when the configured upgrade list enables the major-to-queen action.
    /// Future queen substitution should keep rook-first replacement under this action.
    /// </summary>
    public bool MajorToQueenUpgradeEnabled
    {
      get { return IsPieceUpgradeActionEnabled(ApmwConstants.PieceUpgradeActions.MajorToQueen); }
    }

    public void Instantiate(
      Dictionary<string, object> slotData,
      ApmwContractV2 validatedContract = null)
    {
      SlotData = slotData;
      CurrentContract = SlotData.ContainsKey("apmw_contract")
        ? validatedContract ?? ApmwGeometryResolver.ParseCurrentContract(SlotData["apmw_contract"])
        : null;
      SlotDataContract slotDataContract = DetectSlotDataContract(SlotData);
      UsesCurrentContract = SlotData.ContainsKey("apmw_contract") ||
        SlotData.ContainsKey(ApmwConstants.SlotKeyPieceUpgradeRatio);
      DeterministicChaosSeedForTest = SlotData.ContainsKey(DeterministicChaosSeedSlotKeyForTest)
        ? Convert.ToInt32(SlotData[DeterministicChaosSeedSlotKeyForTest])
        : (int?)null;

      // Implemented by ChecksMate protocol
      //SlotData["max_material"]
      //SlotData["min_material"]
      //SlotData["early_material"]
      //SlotData["queen_piece_limit"]
      //SlotData["max_pocket"]
      //SlotData["fairy_chess_pieces"]

      // Progressive Goal
      GoalInt = Convert.ToInt32(SlotData.GetValueOrDefault(
        "goal", Goal.Single));
      progressionItemization = ReadProgressionItemization(SlotData.GetValueOrDefault(
        ApmwConstants.SlotKeyProgressionItemization,
        ProgressionItemization.Legacy));
      materialItemValue = Math.Max(1, Convert.ToInt32(SlotData.GetValueOrDefault(
        ApmwConstants.SlotKeyMaterialItemValue,
        DefaultMaterialItemValue)));
      castlingLocationCount = Math.Max(0, Convert.ToInt32(SlotData.GetValueOrDefault(
        ApmwConstants.SlotKeyCastlingLocationCount,
        DefaultCastlingLocationCount)));
      EnemyTypesInt = Convert.ToInt32(SlotData.GetValueOrDefault(
        "enemy_piece_types", PieceTypes.Book));

      // Chaotic Material Randomization
      // Non-Progressive Material
      LocsInt = Convert.ToInt32(SlotData.GetValueOrDefault(
        "piece_locations", PieceLocations.Stable));
      TypesInt = Convert.ToInt32(SlotData.GetValueOrDefault(
        "piece_types", PieceTypes.Stable));

      // Army-Constrained Material
      FairyArmyInt = Convert.ToInt32(SlotData.GetValueOrDefault(
        "fairy_chess_army", FairyArmy.Chaos));
      Army = ((JArray)SlotData.GetValueOrDefault(
        "army", new JArray())).ToObject<int[]>().ToList();

      // Non-Fairy Chess
      PawnsInt = Convert.ToInt32(SlotData.GetValueOrDefault(
        "fairy_chess_pawns", FairyPawns.Mixed));
      SetPawnUpgrades(ReadPawnUpgradeMode(
        SlotData.GetValueOrDefault(ApmwConstants.SlotKeyFairyChessPawnUpgrades, FairyPawnUpgrades.Off),
        slotDataContract));
      bool hasPieceUpgradePreferences = SlotData.ContainsKey(ApmwConstants.SlotKeyPieceUpgradePreferences);
      IDictionary<string, double> pieceUpgradeProportions = ReadPieceUpgradeProportions(
        SlotData.GetValueOrDefault(
          slotDataContract == SlotDataContract.Current
            ? ApmwConstants.SlotKeyPieceUpgradeRatio
            : ApmwConstants.LegacySlotKeyPieceUpgradeProportion,
          null));
      var pieceUpgradeResolution = ResolvePieceUpgradeActions(
        hasPieceUpgradePreferences ? SlotData[ApmwConstants.SlotKeyPieceUpgradePreferences] : null,
        hasPieceUpgradePreferences,
        PawnUpgrades,
        progressionItemization,
        pieceUpgradeProportions);
      PieceUpgradePreferences = pieceUpgradeResolution.Preferences;
      PieceUpgradeActions = pieceUpgradeResolution.Actions;

      // Piece Limits
      minorTypeLimit = Convert.ToInt32(SlotData.GetValueOrDefault(
        "minor_piece_limit_by_type", 0));
      majorTypeLimit = Convert.ToInt32(SlotData.GetValueOrDefault(
        "major_piece_limit_by_type", 0));
      queenTypeLimit = Convert.ToInt32(SlotData.GetValueOrDefault(
        "queen_piece_limit_by_type", 0));
      pocketLimit = Convert.ToInt32(SlotData.GetValueOrDefault(
        "pocket_limit_by_pocket", 4));
    }

    public void ResetConnectionState()
    {
      SlotData = null;
      CurrentContract = null;
      UsesCurrentContract = false;
    }

    private enum SlotDataContract
    {
      Legacy,
      Current,
    }

    private static SlotDataContract DetectSlotDataContract(IDictionary<string, object> slotData)
    {
      if (slotData.ContainsKey("apmw_contract"))
        return SlotDataContract.Current;

      if (slotData.ContainsKey(ApmwConstants.LegacySlotKeyPieceUpgradeProportion))
        return SlotDataContract.Legacy;

      return slotData.ContainsKey(ApmwConstants.SlotKeyPieceUpgradeRatio) ||
        slotData.ContainsKey(ApmwConstants.SlotKeyPieceUpgradePreferences)
          ? SlotDataContract.Current
          : SlotDataContract.Legacy;
    }

    private static FairyPawnUpgrades ReadPawnUpgradeMode(object rawValue, SlotDataContract slotDataContract)
    {
      int value;
      try
      {
        value = Convert.ToInt32(rawValue);
      }
      catch (FormatException)
      {
        return FairyPawnUpgrades.Off;
      }
      catch (InvalidCastException)
      {
        return FairyPawnUpgrades.Off;
      }
      catch (OverflowException)
      {
        return FairyPawnUpgrades.Off;
      }

      switch (value)
      {
        case 1:
          return FairyPawnUpgrades.Pool;
        case 2:
          return FairyPawnUpgrades.Max;
        case 3:
          return slotDataContract == SlotDataContract.Current
            ? FairyPawnUpgrades.Configure
            : FairyPawnUpgrades.SuperMax;
        case 4:
          return FairyPawnUpgrades.Configure;
        default:
          return FairyPawnUpgrades.Off;
      }
    }

    private static FairyPawnUpgrades ReadLegacyPawnUpgradeMode(int value)
    {
      return ReadPawnUpgradeMode(value, SlotDataContract.Legacy);
    }

    private static ProgressionItemization ReadProgressionItemization(object value)
    {
      if (value == null)
        return ProgressionItemization.Legacy;

      if (value is ProgressionItemization itemization)
        return itemization;

      if (value is JValue jValue)
        value = jValue.Value;

      if (value is JToken jToken)
        value = jToken.ToObject<object>();

      if (value is string itemizationName)
      {
        if (string.Equals(itemizationName, "fundamental", StringComparison.OrdinalIgnoreCase))
          return ProgressionItemization.Fundamental;
        if (string.Equals(itemizationName, "legacy", StringComparison.OrdinalIgnoreCase))
          return ProgressionItemization.Legacy;
      }

      int rawValue;
      try
      {
        rawValue = Convert.ToInt32(value);
      }
      catch (Exception)
      {
        return ProgressionItemization.Legacy;
      }

      return Enum.IsDefined(typeof(ProgressionItemization), rawValue)
        ? (ProgressionItemization)rawValue
        : ProgressionItemization.Legacy;
    }

    private sealed class PieceUpgradeActionResolutionResult
    {
      public PieceUpgradeActionResolutionResult(
        List<string> preferences,
        IReadOnlyDictionary<string, PieceUpgradeActionResolution> actions)
      {
        Preferences = preferences;
        Actions = actions;
      }

      public List<string> Preferences { get; private set; }
      public IReadOnlyDictionary<string, PieceUpgradeActionResolution> Actions { get; private set; }
    }

    private static PieceUpgradeActionResolutionResult ResolvePieceUpgradeActions(
      object rawPreferences,
      bool hasPreferences,
      FairyPawnUpgrades legacyMode,
      ProgressionItemization progressionItemization,
      IDictionary<string, double> proportions)
    {
      if (!hasPreferences)
        return ResolvePieceUpgradeActionsFromLegacyDefaults(legacyMode, progressionItemization, proportions);

      if (TryReadPieceUpgradePriorityMap(rawPreferences, out var priorityMap))
        return ResolvePieceUpgradeActionsFromPriorityMap(priorityMap, proportions);

      var requestedPreferences = ReadPieceUpgradePreferenceNames(rawPreferences);
      if (requestedPreferences.Count == 0)
        return ResolvePieceUpgradeActionsFromLegacyDefaults(legacyMode, progressionItemization, proportions);

      var preferences = requestedPreferences
        .Where(preference => ValidPieceUpgradePreferences.Contains(preference))
        .Distinct(StringComparer.Ordinal)
        .ToList();
      return preferences.Count == 0
        ? ResolvePieceUpgradeActionsFromLegacyDefaults(FairyPawnUpgrades.Off, progressionItemization, proportions)
        : ResolvePieceUpgradeActionsFromNames(preferences, proportions);
    }

    // The only entry point that consults LegacyPieceUpgradePreferences: converts its weighted
    // (name, priority) pairs into a priority map so ties (e.g. the default Fundamental-mode
    // Configure/Off action set, which the game intentionally wants tied) resolve exactly like an
    // explicit slot-data priority map would -- see ResolvePieceUpgradeActionsFromPriorityMap.
    private static PieceUpgradeActionResolutionResult ResolvePieceUpgradeActionsFromLegacyDefaults(
      FairyPawnUpgrades legacyMode,
      ProgressionItemization progressionItemization,
      IDictionary<string, double> proportions)
    {
      var priorities = LegacyPieceUpgradePreferences(legacyMode, progressionItemization)
        .GroupBy(pair => pair.ActionName, StringComparer.Ordinal)
        .ToDictionary(group => group.Key, group => group.First().Priority, StringComparer.Ordinal);
      return ResolvePieceUpgradeActionsFromPriorityMap(priorities, proportions);
    }

    private static PieceUpgradeActionResolutionResult ResolvePieceUpgradeActionsFromNames(
      List<string> preferences,
      IDictionary<string, double> proportions)
    {
      var priorities = new Dictionary<string, int>(StringComparer.Ordinal);
      for (int index = 0; index < preferences.Count; index++)
      {
        string preference = preferences[index];
        if (ValidPieceUpgradePreferences.Contains(preference) && !priorities.ContainsKey(preference))
          priorities[preference] = preferences.Count - index;
      }

      var actions = ValidPieceUpgradeActions
        .Select(actionName => new PieceUpgradeActionResolution(
          actionName,
          priorities.ContainsKey(actionName) ? priorities[actionName] : 0,
          true,
          ProportionFor(actionName, proportions)))
        .ToList();

      return new PieceUpgradeActionResolutionResult(
        actions.Where(action => action.IsEnabled)
          .OrderByDescending(action => action.Priority)
          .ThenBy(action => PieceUpgradeActionOrder[action.ActionName])
          .Select(action => action.ActionName)
          .ToList(),
        actions.ToDictionary(action => action.ActionName, action => action, StringComparer.Ordinal));
    }

    private static PieceUpgradeActionResolutionResult ResolvePieceUpgradeActionsFromPriorityMap(
      IDictionary<string, int> priorities,
      IDictionary<string, double> proportions)
    {
      var actions = ValidPieceUpgradeActions
        .Select(actionName =>
        {
          int priority = priorities.ContainsKey(actionName) ? priorities[actionName] : 0;
          return new PieceUpgradeActionResolution(actionName, priority, priority != -1, ProportionFor(actionName, proportions));
        })
        .ToList();

      return new PieceUpgradeActionResolutionResult(
        actions.Where(action => action.IsEnabled)
          .OrderByDescending(action => action.Priority)
          .ThenBy(action => PieceUpgradeActionOrder[action.ActionName])
          .Select(action => action.ActionName)
          .ToList(),
        actions.ToDictionary(action => action.ActionName, action => action, StringComparer.Ordinal));
    }

    private static bool TryReadPieceUpgradePriorityMap(
      object rawPreferences,
      out IDictionary<string, int> priorities)
    {
      priorities = new Dictionary<string, int>(StringComparer.Ordinal);

      if (rawPreferences is JObject jObject)
      {
        foreach (var property in jObject.Properties())
        {
          if (ValidPieceUpgradePreferences.Contains(property.Name) &&
            TryConvertPriority(property.Value, out int priority))
            priorities[property.Name] = priority;
        }

        return true;
      }

      if (rawPreferences is IDictionary dictionary)
      {
        foreach (DictionaryEntry entry in dictionary)
        {
          string actionName = entry.Key == null ? null : entry.Key.ToString();
          if (ValidPieceUpgradePreferences.Contains(actionName) &&
            TryConvertPriority(entry.Value, out int priority))
            priorities[actionName] = priority;
        }

        return true;
      }

      return false;
    }

    private static bool TryConvertPriority(object value, out int priority)
    {
      priority = 0;
      if (value == null)
        return false;

      try
      {
        if (value is JValue jValue)
          value = jValue.Value;

        if (value is JToken jToken)
          value = jToken.ToObject<object>();

        priority = Convert.ToInt32(value);
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    // Reads the selected contract's optional ratio/proportion slot-data dictionary (action name -> relative
    // weight). Mirrors TryReadPieceUpgradePriorityMap's JObject/IDictionary handling, but values
    // are doubles and there's no "true/parsed at all" distinction to report: any action absent
    // from the map (or an entirely absent/unparseable raw value) simply falls back to weight 1
    // via ProportionFor.
    private static IDictionary<string, double> ReadPieceUpgradeProportions(object rawProportions)
    {
      var proportions = new Dictionary<string, double>(StringComparer.Ordinal);
      if (rawProportions == null)
        return proportions;

      if (rawProportions is JObject jObject)
      {
        foreach (var property in jObject.Properties())
        {
          if (ValidPieceUpgradePreferences.Contains(property.Name) &&
            TryConvertProportion(property.Value, out double proportion))
            proportions[property.Name] = proportion;
        }

        return proportions;
      }

      if (rawProportions is IDictionary dictionary)
      {
        foreach (DictionaryEntry entry in dictionary)
        {
          string actionName = entry.Key == null ? null : entry.Key.ToString();
          if (ValidPieceUpgradePreferences.Contains(actionName) &&
            TryConvertProportion(entry.Value, out double proportion))
            proportions[actionName] = proportion;
        }
      }

      return proportions;
    }

    private static bool TryConvertProportion(object value, out double proportion)
    {
      proportion = 1.0;
      if (value == null)
        return false;

      try
      {
        if (value is JValue jValue)
          value = jValue.Value;

        if (value is JToken jToken)
          value = jToken.ToObject<object>();

        proportion = Convert.ToDouble(value);
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    private static double ProportionFor(string actionName, IDictionary<string, double> proportions)
    {
      double proportion;
      return proportions != null && proportions.TryGetValue(actionName, out proportion) ? proportion : 1.0;
    }

    private static IEnumerable<(string ActionName, int Priority)> LegacyPieceUpgradePreferences(
      FairyPawnUpgrades legacyMode,
      ProgressionItemization progressionItemization)
    {
      // @chesslogic confirmed (2026-07) that the new tied graduation-action default set is scoped
      // to Fundamental-mode slot graduation only -- Legacy mode's default action set (and its
      // dependent tests, e.g. ApmwGenerationFuzzTests' Neutral*/ApmwItemHandlerCharacterizationTests'
      // Generation_NeutralAmazonUpgrade* cases) must keep behaving exactly as before, since
      // minor-to-major is a real, functioning action for Legacy (unlike pawn-to-minor/pawn-to-major,
      // which have no Legacy metadata and are pure no-ops there either way).
      if (progressionItemization != ProgressionItemization.Fundamental)
        return LegacyModePieceUpgradePreferences(legacyMode);

      // @chesslogic's confirmed default action set for Fundamental-mode slot graduation: these 5
      // are a deliberately unordered tie at the same priority, differentiated only by proportion
      // (PieceUpgradeActionResolution.Proportion), not by priority order. NewPawn rides along
      // untouched by this tie: PickNewOrMorePawnAction/UpgradePawns (ItemGeneration.cs) only ever
      // check whether NewPawn is *enabled*, never compare its priority against another action, so
      // folding it into this tied group is behaviorally free in every legacy mode.
      var graduationDefaults = TiedAtPriorityOne(
        ApmwConstants.PieceUpgradeActions.NewPawn,
        ApmwConstants.PieceUpgradeActions.PawnToMinor,
        ApmwConstants.PieceUpgradeActions.MinorToMajor,
        ApmwConstants.PieceUpgradeActions.PawnToMajor,
        ApmwConstants.PieceUpgradeActions.MajorToQueen);

      // PoolPawnUpgrade/MorePawn/BetterPawn are a disjoint subsystem (pawn board-slot *variant*
      // selection, not ChessmanTier graduation) whose exact relative priority order is load-bearing:
      // ShouldApplyPoolPawnUpgradeAction/ShouldApplyBetterPawnActionBeforeMorePawn/
      // ShouldApplyDelayedBetterPawnAction (ItemGeneration.cs) compare these three actions' priorities
      // directly via IsPieceUpgradeActionPreferredBefore (strict >), so their original per-mode
      // relative order must be preserved verbatim -- only their exact values changed here (folded
      // out of the same flat list NewPawn/MajorToQueen used to share), not their mutual ordering.
      switch (legacyMode)
      {
        case FairyPawnUpgrades.Pool:
          return graduationDefaults.Concat(new[]
          {
            (ApmwConstants.PieceUpgradeActions.PoolPawnUpgrade, 4),
            (ApmwConstants.PieceUpgradeActions.MorePawn, 3),
            (ApmwConstants.PieceUpgradeActions.BetterPawn, 2),
          });
        case FairyPawnUpgrades.Max:
        case FairyPawnUpgrades.SuperMax:
          return graduationDefaults.Concat(new[]
          {
            (ApmwConstants.PieceUpgradeActions.BetterPawn, 3),
            (ApmwConstants.PieceUpgradeActions.MorePawn, 2),
          });
        case FairyPawnUpgrades.Off:
        case FairyPawnUpgrades.Configure:
        default:
          return graduationDefaults.Concat(new[]
          {
            (ApmwConstants.PieceUpgradeActions.MorePawn, 3),
            (ApmwConstants.PieceUpgradeActions.BetterPawn, 2),
          });
      }
    }

    // Legacy mode's original default tables (byte-for-byte identical to pre-Fundamental-redesign
    // behavior): only major-to-queen is a default-enabled graduation action; pawn-to-minor/
    // minor-to-major/pawn-to-major stay at priority 0 (disabled) unless explicitly requested via
    // slot data. Preserved verbatim so Legacy generation/tests are unaffected by the Fundamental
    // default-table redesign.
    private static IEnumerable<(string ActionName, int Priority)> LegacyModePieceUpgradePreferences(FairyPawnUpgrades legacyMode)
    {
      switch (legacyMode)
      {
        case FairyPawnUpgrades.Pool:
          return OrderedByListPosition(
            ApmwConstants.PieceUpgradeActions.NewPawn,
            ApmwConstants.PieceUpgradeActions.PoolPawnUpgrade,
            ApmwConstants.PieceUpgradeActions.MorePawn,
            ApmwConstants.PieceUpgradeActions.BetterPawn,
            ApmwConstants.PieceUpgradeActions.MajorToQueen);
        case FairyPawnUpgrades.Max:
        case FairyPawnUpgrades.SuperMax:
          return OrderedByListPosition(
            ApmwConstants.PieceUpgradeActions.NewPawn,
            ApmwConstants.PieceUpgradeActions.BetterPawn,
            ApmwConstants.PieceUpgradeActions.MorePawn,
            ApmwConstants.PieceUpgradeActions.MajorToQueen);
        case FairyPawnUpgrades.Off:
        case FairyPawnUpgrades.Configure:
        default:
          return OrderedByListPosition(
            ApmwConstants.PieceUpgradeActions.NewPawn,
            ApmwConstants.PieceUpgradeActions.MorePawn,
            ApmwConstants.PieceUpgradeActions.BetterPawn,
            ApmwConstants.PieceUpgradeActions.MajorToQueen);
      }
    }

    // Mirrors the strictly-decreasing priority formula ResolvePieceUpgradeActionsFromNames applies
    // to an ordered slot-data array (preferences.Count - index): first entry gets the highest
    // priority, ties are impossible. Used only for Legacy mode's original default tables, where
    // this strict ordering is load-bearing (see LegacyModePieceUpgradePreferences).
    private static IEnumerable<(string ActionName, int Priority)> OrderedByListPosition(params string[] actionNames)
    {
      return actionNames.Select((actionName, index) => (actionName, actionNames.Length - index));
    }

    private static IEnumerable<(string ActionName, int Priority)> TiedAtPriorityOne(params string[] actionNames)
    {
      return actionNames.Select(actionName => (actionName, 1));
    }

    private static List<string> ReadPieceUpgradePreferenceNames(object rawPreferences)
    {
      if (rawPreferences == null)
        return new List<string>();

      if (rawPreferences is JArray jArray)
        return NormalizePieceUpgradePreferenceNames(jArray.Select(JTokenToString));

      if (rawPreferences is JToken jToken)
        return NormalizePieceUpgradePreferenceNames(new[] { JTokenToString(jToken) });

      if (rawPreferences is IEnumerable<string> strings)
        return NormalizePieceUpgradePreferenceNames(strings);

      if (rawPreferences is IEnumerable<object> objects)
        return NormalizePieceUpgradePreferenceNames(objects.Select(item => item == null ? null : item.ToString()));

      return NormalizePieceUpgradePreferenceNames(new[] { rawPreferences.ToString() });
    }

    private static List<string> NormalizePieceUpgradePreferenceNames(IEnumerable<string> names)
    {
      return names
        .Where(name => !string.IsNullOrWhiteSpace(name))
        .Select(name => name.Trim())
        .ToList();
    }

    private static string JTokenToString(JToken token)
    {
      if (token == null || token.Type == JTokenType.Null)
        return null;

      return token.Type == JTokenType.String
        ? token.Value<string>()
        : token.ToString();
    }

    public void seed()
    {
      Random random = DeterministicChaosSeedForTest.HasValue
        ? new Random(DeterministicChaosSeedForTest.Value)
        : new Random();

      // Types
      if (this.Types != PieceTypes.Chaos)
      {
        pocketSeed = Convert.ToInt32(SlotData["pocket_seed"]);
        pawnSeed = Convert.ToInt32(SlotData["pawn_seed"]);
        minorSeed = Convert.ToInt32(SlotData["minor_seed"]);
        majorSeed = Convert.ToInt32(SlotData["major_seed"]);
        queenSeed = Convert.ToInt32(SlotData["queen_seed"]);
      }
      else
      {
        pocketSeed = random.Next();
        pawnSeed = random.Next();
        minorSeed = random.Next();
        majorSeed = random.Next();
        queenSeed = random.Next();
      }

      // Locations
      if (this.Locs != PieceLocations.Chaos)
      {
        // TODO(chesslogic): I thought about it for a moment, and I think this is fine
        // But maybe Mersenne twister is happier with some sort of offset
        pawnLocSeed = Convert.ToInt32(SlotData["pawn_seed"]);
        minorLocSeed = Convert.ToInt32(SlotData["minor_seed"]);
        majorLocSeed = Convert.ToInt32(SlotData["major_seed"]);
        queenLocSeed = Convert.ToInt32(SlotData["queen_seed"]);
      }
      else
      {
        pawnLocSeed = random.Next();
        minorLocSeed = random.Next();
        majorLocSeed = random.Next();
        queenLocSeed = random.Next();
      }

      // Used to break ties among equally-preferred piece-upgrade/graduation actions
      // (FundamentalSlotGraduationPlanner.Simulate for Fundamental mode; NonPawnUpgradeGeneration.
      // Plan for Legacy mode -- the two are mutually exclusive per game, so sharing one seed is
      // safe). No dedicated slot-data key exists for this yet (no apworld protocol change to
      // introduce one), so it's derived deterministically from the already-transmitted
      // pocket/pawn seeds, keeping tie-breaking reproducible/stable without requiring one.
      fundamentalGraduationSeed = unchecked(pocketSeed * 397 ^ pawnSeed);
    }

    /** Possibly not stable - will generate a different pocket distribution as the player progresses through different foundPockets - but it is uniform */
    public List<int> generatePocketValues(int foundPockets)
    {
      if (pocketSeed == -1) { throw new InvalidOperationException("Please set Starter.pocket_seed"); }

      // Limit number of pocket items based on player preferences, even if more got force added
      if (pocketLimit > 0) {
        if (pocketLimit * 3 < foundPockets) {
          double ceiling = Math.Min(4.0, Math.Ceiling(foundPockets / 3.0));
          pocketLimit = ceiling % 1.0 == 0.0 ? (int)ceiling : 4;
        }
        foundPockets = Math.Min(foundPockets, pocketLimit * 3);
      }

      // preserve choices separate from values
      Random pocketRandom = new Random(pocketSeed);
      pocketChoiceSeed = new List<int>() { pocketRandom.Next(), pocketRandom.Next(), pocketRandom.Next() };

      // If no pockets found yet, return all zeros
      if (foundPockets <= 0)
        return new List<int>() { 0, 0, 0 };

      // probably not uniform... but it's within range so it works for now. will break FEN later
      Random random = new Random(pocketSeed);
      var minX = Math.Max(0, foundPockets - pocketLimit * 2);
      var maxX = Math.Min(foundPockets, pocketLimit + 1);
      var x = random.Next(minX, Math.Max(minX + 1, maxX));

      if (x == foundPockets)
      {
        return new List<int>() { x, 0, 0 };
      }
      if (x == foundPockets - 1)
      {
        return new List<int>() { x, 1, 0 };
      }

      var minY = Math.Max(0, foundPockets - pocketLimit - x);
      var maxY = Math.Min(foundPockets - x, pocketLimit + 1);
      var y = random.Next(minY, Math.Max(minY + 1, maxY));
      var z = foundPockets - (y + x);
      if (z < 0)
      {
        (x, y, z) = (pocketLimit - x, pocketLimit - y, -z);
      }

      return new List<int>() { x, y, z };
    }

    /**
     * arg spaces should be TOTAL spaces not EMPTY spaces
     * 
     * vaguely inspired by this cacophanous suggestion:
     * https://stackoverflow.com/questions/28544808/random-distribution-of-items-in-list-with-exact-number-of-occurences
     */
    public static Dictionary<int, Item> distribute<Item>(List<Item> items, int spaces)
    {
      return distribute(items, spaces, new Random());
    }

    internal static Dictionary<int, Item> distribute<Item>(List<Item> items, int spaces, Random random)
    {
      if (random == null)
        throw new ArgumentNullException(nameof(random));

      // Create list of items * z
      Dictionary<int, Item> allItems = new Dictionary<int, Item>();
      for (int i = 0; i < items.Count; i++)
        allItems.Add(i, items[i]);
      for (int i = items.Count; i < spaces; i++)
        allItems.Add(i, default);

      int n = allItems.Count;
      while (n > 1)
      {
        n--;
        int k = random.Next(n + 1);
        (allItems[k], allItems[n]) = (allItems[n], allItems[k]);
      }

      return allItems;
    }
  }
}
