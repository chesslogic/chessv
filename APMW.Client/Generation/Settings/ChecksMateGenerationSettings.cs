using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Archipelago.APChessV.Generation.Settings
{
  public sealed class ChecksMateGenerationSettings
  {
    public const string DefaultGame = "ChecksMate";
    public const string DefaultName = "HakknivChess{number}";
    public const string DefaultDescription = "All pieces, stable board";
    public const string DefaultRequiredArchipelagoVersion = "0.5.1";

    public ChecksMateGenerationSettings()
    {
      FairyChessPiecesConfigure = new List<ChecksMateFairyChessPieceSet>
      {
        ChecksMateFairyChessPieceSet.Petal,
        ChecksMateFairyChessPieceSet.Cannon,
        ChecksMateFairyChessPieceSet.Nutty,
        ChecksMateFairyChessPieceSet.Rookies,
        ChecksMateFairyChessPieceSet.Fide,
        ChecksMateFairyChessPieceSet.Clobberers,
        ChecksMateFairyChessPieceSet.Camel,
      };

      AsymmetricTrades = new Dictionary<ChecksMateAsymmetricTrade, int>
      {
        [ChecksMateAsymmetricTrade.Disabled] = 50,
        [ChecksMateAsymmetricTrade.Jacks] = 0,
      };

      UnsupportedTopLevelFields = new Dictionary<string, object>(StringComparer.Ordinal);
      UnsupportedRequiresFields = new Dictionary<string, object>(StringComparer.Ordinal);
      UnsupportedChecksMateOptions = new Dictionary<string, object>(StringComparer.Ordinal);
    }

    public string Name { get; set; } = DefaultName;

    public string Game { get; set; } = DefaultGame;

    public string Description { get; set; } = DefaultDescription;

    public string RequiredArchipelagoVersion { get; set; } = DefaultRequiredArchipelagoVersion;

    public int ProgressionBalancing { get; set; } = 0;

    public ChecksMateAccessibility Accessibility { get; set; } = ChecksMateAccessibility.Full;

    public ChecksMateGoal Goal { get; set; } = ChecksMateGoal.OrderedProgressive;

    public ChecksMateDifficulty Difficulty { get; set; } = ChecksMateDifficulty.Daily;

    public ChecksMateTactics EnableTactics { get; set; } = ChecksMateTactics.All;

    public ChecksMatePieceLocations PieceLocations { get; set; } = ChecksMatePieceLocations.Stable;

    public ChecksMatePieceTypes PieceTypes { get; set; } = ChecksMatePieceTypes.Stable;

    public ChecksMateSwitch EarlyMaterial { get; set; } = ChecksMateSwitch.Off;

    public ChecksMateLimitValue MaxEnginePenalties { get; set; } = ChecksMateLimitValue.RandomHigh;

    public ChecksMateLimitValue MaxPocket { get; set; } = ChecksMateLimitValue.RandomHigh;

    public ChecksMateLimitValue MaxKings { get; set; } = ChecksMateLimitValue.Random;

    public ChecksMateLimitValue FairyKings { get; set; } = ChecksMateLimitValue.Fixed(2);

    public ChecksMateFairyChessPieces FairyChessPieces { get; set; } = ChecksMateFairyChessPieces.Full;

    public IList<ChecksMateFairyChessPieceSet> FairyChessPiecesConfigure { get; private set; }

    public ChecksMateFairyChessArmy FairyChessArmy { get; set; } = ChecksMateFairyChessArmy.Chaos;

    public ChecksMateFairyChessPawns FairyChessPawns { get; set; } = ChecksMateFairyChessPawns.Mixed;

    public ChecksMateFairyChessPawnUpgrades FairyChessPawnUpgrades { get; set; } = ChecksMateFairyChessPawnUpgrades.Off;

    public int MinorPieceLimitByType { get; set; } = 0;

    public int MajorPieceLimitByType { get; set; } = 0;

    public int QueenPieceLimitByType { get; set; } = 0;

    public int QueenPieceLimit { get; set; } = 0;

    public int PocketLimitByPocket { get; set; } = 4;

    public bool DeathLink { get; set; } = false;

    public IDictionary<ChecksMateAsymmetricTrade, int> AsymmetricTrades { get; private set; }

    public IDictionary<string, object> UnsupportedTopLevelFields { get; private set; }

    public IDictionary<string, object> UnsupportedRequiresFields { get; private set; }

    public IDictionary<string, object> UnsupportedChecksMateOptions { get; private set; }

    public IDictionary<string, object> ToYamlCompatibleMap()
    {
      EnsureValid();

      var requires = new Dictionary<string, object>
      {
        ["version"] = RequiredArchipelagoVersion,
      };
      AddUnsupportedFields(requires, UnsupportedRequiresFields);

      var map = new Dictionary<string, object>
      {
        ["name"] = Name,
        ["game"] = Game,
        ["description"] = Description,
        ["requires"] = requires,
        [DefaultGame] = ToYamlOptionMap(),
      };
      AddUnsupportedFields(map, UnsupportedTopLevelFields);

      return map;
    }

    public IDictionary<string, object> ToYamlOptionMap()
    {
      EnsureValid();

      var map = new Dictionary<string, object>
      {
        [ChecksMateGenerationOptionKeys.ProgressionBalancing] = FormatNumber(ProgressionBalancing),
        [ChecksMateGenerationOptionKeys.Accessibility] = ChecksMateYamlNames.Accessibility(Accessibility),
        [ChecksMateGenerationOptionKeys.Goal] = ChecksMateYamlNames.Goal(Goal),
        [ChecksMateGenerationOptionKeys.Difficulty] = ChecksMateYamlNames.Difficulty(Difficulty),
        [ChecksMateGenerationOptionKeys.EnableTactics] = ChecksMateYamlNames.Tactics(EnableTactics),
        [ChecksMateGenerationOptionKeys.PieceLocations] = ChecksMateYamlNames.PieceLocations(PieceLocations),
        [ChecksMateGenerationOptionKeys.PieceTypes] = ChecksMateYamlNames.PieceTypes(PieceTypes),
        [ChecksMateGenerationOptionKeys.EarlyMaterial] = ChecksMateYamlNames.Switch(EarlyMaterial),
        [ChecksMateGenerationOptionKeys.MaxEnginePenalties] = MaxEnginePenalties.ToYamlValue(),
        [ChecksMateGenerationOptionKeys.MaxPocket] = MaxPocket.ToYamlValue(),
        [ChecksMateGenerationOptionKeys.MaxKings] = MaxKings.ToYamlValue(),
        [ChecksMateGenerationOptionKeys.FairyKings] = FairyKings.ToYamlValue(),
        [ChecksMateGenerationOptionKeys.FairyChessPieces] = ChecksMateYamlNames.FairyChessPieces(FairyChessPieces),
        [ChecksMateGenerationOptionKeys.FairyChessPiecesConfigure] = FairyChessPiecesConfigure
          .Select(ChecksMateYamlNames.FairyChessPieceSet)
          .ToList(),
        [ChecksMateGenerationOptionKeys.FairyChessArmy] = ChecksMateYamlNames.FairyChessArmy(FairyChessArmy),
        [ChecksMateGenerationOptionKeys.FairyChessPawns] = ChecksMateYamlNames.FairyChessPawns(FairyChessPawns),
        [ChecksMateGenerationOptionKeys.FairyChessPawnUpgrades] = ChecksMateYamlNames.FairyChessPawnUpgrades(FairyChessPawnUpgrades),
        [ChecksMateGenerationOptionKeys.MinorPieceLimitByType] = FormatNumber(MinorPieceLimitByType),
        [ChecksMateGenerationOptionKeys.MajorPieceLimitByType] = FormatNumber(MajorPieceLimitByType),
        [ChecksMateGenerationOptionKeys.QueenPieceLimitByType] = FormatNumber(QueenPieceLimitByType),
        [ChecksMateGenerationOptionKeys.QueenPieceLimit] = FormatNumber(QueenPieceLimit),
        [ChecksMateGenerationOptionKeys.PocketLimitByPocket] = FormatNumber(PocketLimitByPocket),
        [ChecksMateGenerationOptionKeys.DeathLink] = DeathLink,
        [ChecksMateGenerationOptionKeys.AsymmetricTrades] = AsymmetricTrades.ToDictionary(
          pair => ChecksMateYamlNames.AsymmetricTrade(pair.Key),
          pair => pair.Value),
      };
      AddUnsupportedFields(map, UnsupportedChecksMateOptions);

      return map;
    }

    public IReadOnlyList<ChecksMateGenerationValidationError> Validate()
    {
      var errors = new List<ChecksMateGenerationValidationError>();

      AddRequiredStringError(errors, nameof(Name), Name);
      AddRequiredStringError(errors, nameof(Game), Game);

      if (!string.IsNullOrWhiteSpace(Game) &&
          !string.Equals(Game, DefaultGame, StringComparison.Ordinal))
      {
        errors.Add(new ChecksMateGenerationValidationError(
          nameof(Game),
          "Game must be ChecksMate for ChecksMate generation settings."));
      }

      AddRequiredStringError(errors, nameof(RequiredArchipelagoVersion), RequiredArchipelagoVersion);
      if (!string.IsNullOrWhiteSpace(RequiredArchipelagoVersion) &&
          !Version.TryParse(RequiredArchipelagoVersion, out _))
      {
        errors.Add(new ChecksMateGenerationValidationError(
          nameof(RequiredArchipelagoVersion),
          "Required Archipelago version must be a version string such as 0.5.1."));
      }

      AddRangeError(errors, nameof(ProgressionBalancing), ProgressionBalancing, 0, 100);
      AddEnumError(errors, nameof(Accessibility), Accessibility);
      AddEnumError(errors, nameof(Goal), Goal);
      AddEnumError(errors, nameof(Difficulty), Difficulty);
      AddEnumError(errors, nameof(EnableTactics), EnableTactics);
      AddEnumError(errors, nameof(PieceLocations), PieceLocations);
      AddEnumError(errors, nameof(PieceTypes), PieceTypes);
      AddEnumError(errors, nameof(EarlyMaterial), EarlyMaterial);
      AddRequiredLimitError(errors, nameof(MaxEnginePenalties), MaxEnginePenalties);
      AddRequiredLimitError(errors, nameof(MaxPocket), MaxPocket);
      AddRequiredLimitError(errors, nameof(MaxKings), MaxKings);
      AddRequiredLimitError(errors, nameof(FairyKings), FairyKings);
      AddEnumError(errors, nameof(FairyChessPieces), FairyChessPieces);
      AddEnumError(errors, nameof(FairyChessArmy), FairyChessArmy);
      AddEnumError(errors, nameof(FairyChessPawns), FairyChessPawns);
      AddEnumError(errors, nameof(FairyChessPawnUpgrades), FairyChessPawnUpgrades);
      AddNonNegativeError(errors, nameof(MinorPieceLimitByType), MinorPieceLimitByType);
      AddNonNegativeError(errors, nameof(MajorPieceLimitByType), MajorPieceLimitByType);
      AddNonNegativeError(errors, nameof(QueenPieceLimitByType), QueenPieceLimitByType);
      AddNonNegativeError(errors, nameof(QueenPieceLimit), QueenPieceLimit);
      AddNonNegativeError(errors, nameof(PocketLimitByPocket), PocketLimitByPocket);
      ValidateFairyChessPiecesConfigure(errors);
      ValidateAsymmetricTrades(errors);

      return errors;
    }

    public void EnsureValid()
    {
      var errors = Validate();
      if (errors.Count > 0)
      {
        throw new InvalidOperationException(
          "ChecksMate generation settings are invalid: " +
          string.Join("; ", errors.Select(error => error.ToString())));
      }
    }

    private static void AddRequiredStringError(
      ICollection<ChecksMateGenerationValidationError> errors,
      string propertyName,
      string value)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        errors.Add(new ChecksMateGenerationValidationError(
          propertyName,
          "Value is required."));
      }
    }

    private static void AddRequiredLimitError(
      ICollection<ChecksMateGenerationValidationError> errors,
      string propertyName,
      ChecksMateLimitValue value)
    {
      if (value == null)
      {
        errors.Add(new ChecksMateGenerationValidationError(
          propertyName,
          "Limit value is required."));
      }
    }

    private static void AddEnumError<TEnum>(
      ICollection<ChecksMateGenerationValidationError> errors,
      string propertyName,
      TEnum value)
      where TEnum : struct
    {
      if (!Enum.IsDefined(typeof(TEnum), value))
      {
        errors.Add(new ChecksMateGenerationValidationError(
          propertyName,
          "Value is not a supported ChecksMate YAML option name."));
      }
    }

    private static void AddRangeError(
      ICollection<ChecksMateGenerationValidationError> errors,
      string propertyName,
      int value,
      int minimum,
      int maximum)
    {
      if (value < minimum || value > maximum)
      {
        errors.Add(new ChecksMateGenerationValidationError(
          propertyName,
          "Value must be between " + minimum.ToString(CultureInfo.InvariantCulture) +
          " and " + maximum.ToString(CultureInfo.InvariantCulture) + "."));
      }
    }

    private static void AddNonNegativeError(
      ICollection<ChecksMateGenerationValidationError> errors,
      string propertyName,
      int value)
    {
      if (value < 0)
      {
        errors.Add(new ChecksMateGenerationValidationError(
          propertyName,
          "Value must be zero or greater."));
      }
    }

    private void ValidateFairyChessPiecesConfigure(ICollection<ChecksMateGenerationValidationError> errors)
    {
      if (FairyChessPiecesConfigure == null)
      {
        errors.Add(new ChecksMateGenerationValidationError(
          nameof(FairyChessPiecesConfigure),
          "Configured fairy chess pieces are required."));
        return;
      }

      if (FairyChessPieces == ChecksMateFairyChessPieces.Configure &&
          FairyChessPiecesConfigure.Count == 0)
      {
        errors.Add(new ChecksMateGenerationValidationError(
          nameof(FairyChessPiecesConfigure),
          "At least one configured fairy chess piece set is required when Fairy Chess Pieces is Configure."));
      }

      var seen = new HashSet<ChecksMateFairyChessPieceSet>();
      foreach (var pieceSet in FairyChessPiecesConfigure)
      {
        if (!Enum.IsDefined(typeof(ChecksMateFairyChessPieceSet), pieceSet))
        {
          errors.Add(new ChecksMateGenerationValidationError(
            nameof(FairyChessPiecesConfigure),
            "Configured fairy chess piece set is not supported."));
        }
        else if (!seen.Add(pieceSet))
        {
          errors.Add(new ChecksMateGenerationValidationError(
            nameof(FairyChessPiecesConfigure),
            "Configured fairy chess piece sets must not contain duplicates."));
        }
      }
    }

    private void ValidateAsymmetricTrades(ICollection<ChecksMateGenerationValidationError> errors)
    {
      if (AsymmetricTrades == null)
      {
        errors.Add(new ChecksMateGenerationValidationError(
          nameof(AsymmetricTrades),
          "Asymmetric trade weights are required."));
        return;
      }

      foreach (var trade in AsymmetricTrades)
      {
        if (!Enum.IsDefined(typeof(ChecksMateAsymmetricTrade), trade.Key))
        {
          errors.Add(new ChecksMateGenerationValidationError(
            nameof(AsymmetricTrades),
            "Asymmetric trade option is not supported."));
        }

        if (trade.Value < 0)
        {
          errors.Add(new ChecksMateGenerationValidationError(
            nameof(AsymmetricTrades),
            "Asymmetric trade weights must be zero or greater."));
        }
      }

      if (AsymmetricTrades.Count == 0)
      {
        errors.Add(new ChecksMateGenerationValidationError(
          nameof(AsymmetricTrades),
          "At least one asymmetric trade weight is required."));
      }
    }

    private static string FormatNumber(int value)
    {
      return value.ToString(CultureInfo.InvariantCulture);
    }

    private static void AddUnsupportedFields(
      IDictionary<string, object> target,
      IDictionary<string, object> unsupportedFields)
    {
      if (unsupportedFields == null)
      {
        return;
      }

      foreach (var field in unsupportedFields)
      {
        if (!target.ContainsKey(field.Key))
        {
          target[field.Key] = field.Value;
        }
      }
    }
  }

  public sealed class ChecksMateGenerationValidationError
  {
    public ChecksMateGenerationValidationError(string propertyName, string message)
    {
      if (propertyName == null)
      {
        throw new ArgumentNullException(nameof(propertyName));
      }

      if (message == null)
      {
        throw new ArgumentNullException(nameof(message));
      }

      PropertyName = propertyName;
      Message = message;
    }

    public string PropertyName { get; private set; }

    public string Message { get; private set; }

    public override string ToString()
    {
      return PropertyName + ": " + Message;
    }
  }

  public static class ChecksMateGenerationOptionKeys
  {
    public const string ProgressionBalancing = "progression_balancing";
    public const string Accessibility = "accessibility";
    public const string Goal = "goal";
    public const string Difficulty = "difficulty";
    public const string EnableTactics = "enable_tactics";
    public const string PieceLocations = "piece_locations";
    public const string PieceTypes = "piece_types";
    public const string EarlyMaterial = "early_material";
    public const string MaxEnginePenalties = "max_engine_penalties";
    public const string MaxPocket = "max_pocket";
    public const string MaxKings = "max_kings";
    public const string FairyKings = "fairy_kings";
    public const string FairyChessPieces = "fairy_chess_pieces";
    public const string FairyChessPiecesConfigure = "fairy_chess_pieces_configure";
    public const string FairyChessArmy = "fairy_chess_army";
    public const string FairyChessPawns = "fairy_chess_pawns";
    public const string FairyChessPawnUpgrades = "fairy_chess_pawn_upgrades";
    public const string MinorPieceLimitByType = "minor_piece_limit_by_type";
    public const string MajorPieceLimitByType = "major_piece_limit_by_type";
    public const string QueenPieceLimitByType = "queen_piece_limit_by_type";
    public const string QueenPieceLimit = "queen_piece_limit";
    public const string PocketLimitByPocket = "pocket_limit_by_pocket";
    public const string DeathLink = "death_link";
    public const string AsymmetricTrades = "asymmetric_trades";
  }

  public sealed class ChecksMateLimitValue
  {
    private readonly int? fixedValue;
    private readonly ChecksMateRandomLimit? randomLimit;

    public static readonly ChecksMateLimitValue Random =
      new ChecksMateLimitValue(ChecksMateRandomLimit.Random);

    public static readonly ChecksMateLimitValue RandomLow =
      new ChecksMateLimitValue(ChecksMateRandomLimit.RandomLow);

    public static readonly ChecksMateLimitValue RandomHigh =
      new ChecksMateLimitValue(ChecksMateRandomLimit.RandomHigh);

    private ChecksMateLimitValue(int value)
    {
      if (value < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(value), "Limit value must be zero or greater.");
      }

      fixedValue = value;
    }

    private ChecksMateLimitValue(ChecksMateRandomLimit randomLimit)
    {
      if (!Enum.IsDefined(typeof(ChecksMateRandomLimit), randomLimit))
      {
        throw new InvalidEnumArgumentException(
          nameof(randomLimit),
          (int)randomLimit,
          typeof(ChecksMateRandomLimit));
      }

      this.randomLimit = randomLimit;
    }

    public bool IsFixedValue
    {
      get { return fixedValue.HasValue; }
    }

    public int FixedValue
    {
      get
      {
        if (!fixedValue.HasValue)
        {
          throw new InvalidOperationException("Limit is not a fixed numeric value.");
        }

        return fixedValue.Value;
      }
    }

    public static ChecksMateLimitValue Fixed(int value)
    {
      return new ChecksMateLimitValue(value);
    }

    public string ToYamlValue()
    {
      if (fixedValue.HasValue)
      {
        return fixedValue.Value.ToString(CultureInfo.InvariantCulture);
      }

      return ChecksMateYamlNames.RandomLimit(randomLimit.Value);
    }

    public override string ToString()
    {
      return ToYamlValue();
    }
  }

  public enum ChecksMateAccessibility
  {
    Full,
    Minimal,
    Items,
    Locations,
  }

  public enum ChecksMateGoal
  {
    Single,
    OrderedProgressive,
    Progressive,
    Super,
  }

  public enum ChecksMateDifficulty
  {
    Daily,
  }

  public enum ChecksMateTactics
  {
    All,
  }

  public enum ChecksMatePieceLocations
  {
    Chaos,
    Stable,
    Ordered,
  }

  public enum ChecksMatePieceTypes
  {
    Chaos,
    Stable,
    Book,
  }

  public enum ChecksMateSwitch
  {
    Off,
  }

  public enum ChecksMateRandomLimit
  {
    Random,
    RandomLow,
    RandomHigh,
  }

  public enum ChecksMateFairyChessPieces
  {
    Fide,
    Betza,
    Full,
    Configure,
  }

  public enum ChecksMateFairyChessPieceSet
  {
    Petal,
    Cannon,
    Nutty,
    Rookies,
    Fide,
    Clobberers,
    Camel,
  }

  public enum ChecksMateFairyChessArmy
  {
    Chaos,
    Stable,
    Limited,
  }

  public enum ChecksMateFairyChessPawns
  {
    Vanilla,
    Mixed,
    Berolina,
    Checkers,
    Reserved,
    AnyPawn,
    AnyFairy,
    AnyClassical,
  }

  public enum ChecksMateFairyChessPawnUpgrades
  {
    Off,
    Pool,
    Max,
    SuperMax,
  }

  public enum ChecksMateAsymmetricTrade
  {
    Disabled,
    Jacks,
  }

  internal static class ChecksMateYamlNames
  {
    public static string Accessibility(ChecksMateAccessibility value)
    {
      switch (value)
      {
        case ChecksMateAccessibility.Full:
          return "full";
        case ChecksMateAccessibility.Minimal:
          return "minimal";
        case ChecksMateAccessibility.Items:
          return "items";
        case ChecksMateAccessibility.Locations:
          return "locations";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string Goal(ChecksMateGoal value)
    {
      switch (value)
      {
        case ChecksMateGoal.Single:
          return "single";
        case ChecksMateGoal.OrderedProgressive:
          return "ordered_progressive";
        case ChecksMateGoal.Progressive:
          return "progressive";
        case ChecksMateGoal.Super:
          return "super";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string Difficulty(ChecksMateDifficulty value)
    {
      switch (value)
      {
        case ChecksMateDifficulty.Daily:
          return "daily";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string Tactics(ChecksMateTactics value)
    {
      switch (value)
      {
        case ChecksMateTactics.All:
          return "all";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string PieceLocations(ChecksMatePieceLocations value)
    {
      switch (value)
      {
        case ChecksMatePieceLocations.Chaos:
          return "chaos";
        case ChecksMatePieceLocations.Stable:
          return "stable";
        case ChecksMatePieceLocations.Ordered:
          return "ordered";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string PieceTypes(ChecksMatePieceTypes value)
    {
      switch (value)
      {
        case ChecksMatePieceTypes.Chaos:
          return "chaos";
        case ChecksMatePieceTypes.Stable:
          return "stable";
        case ChecksMatePieceTypes.Book:
          return "book";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string Switch(ChecksMateSwitch value)
    {
      switch (value)
      {
        case ChecksMateSwitch.Off:
          return "off";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string RandomLimit(ChecksMateRandomLimit value)
    {
      switch (value)
      {
        case ChecksMateRandomLimit.Random:
          return "random";
        case ChecksMateRandomLimit.RandomLow:
          return "random-low";
        case ChecksMateRandomLimit.RandomHigh:
          return "random-high";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string FairyChessPieces(ChecksMateFairyChessPieces value)
    {
      switch (value)
      {
        case ChecksMateFairyChessPieces.Fide:
          return "fide";
        case ChecksMateFairyChessPieces.Betza:
          return "betza";
        case ChecksMateFairyChessPieces.Full:
          return "full";
        case ChecksMateFairyChessPieces.Configure:
          return "configure";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string FairyChessPieceSet(ChecksMateFairyChessPieceSet value)
    {
      switch (value)
      {
        case ChecksMateFairyChessPieceSet.Petal:
          return "Petal";
        case ChecksMateFairyChessPieceSet.Cannon:
          return "Cannon";
        case ChecksMateFairyChessPieceSet.Nutty:
          return "Nutty";
        case ChecksMateFairyChessPieceSet.Rookies:
          return "Rookies";
        case ChecksMateFairyChessPieceSet.Fide:
          return "FIDE";
        case ChecksMateFairyChessPieceSet.Clobberers:
          return "Clobberers";
        case ChecksMateFairyChessPieceSet.Camel:
          return "Camel";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string FairyChessArmy(ChecksMateFairyChessArmy value)
    {
      switch (value)
      {
        case ChecksMateFairyChessArmy.Chaos:
          return "chaos";
        case ChecksMateFairyChessArmy.Stable:
          return "stable";
        case ChecksMateFairyChessArmy.Limited:
          return "limited";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string FairyChessPawns(ChecksMateFairyChessPawns value)
    {
      switch (value)
      {
        case ChecksMateFairyChessPawns.Vanilla:
          return "vanilla";
        case ChecksMateFairyChessPawns.Mixed:
          return "mixed";
        case ChecksMateFairyChessPawns.Berolina:
          return "berolina";
        case ChecksMateFairyChessPawns.Checkers:
          return "checkers";
        case ChecksMateFairyChessPawns.Reserved:
          return "reserved";
        case ChecksMateFairyChessPawns.AnyPawn:
          return "any_pawn";
        case ChecksMateFairyChessPawns.AnyFairy:
          return "any_fairy";
        case ChecksMateFairyChessPawns.AnyClassical:
          return "any_classical";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string FairyChessPawnUpgrades(ChecksMateFairyChessPawnUpgrades value)
    {
      switch (value)
      {
        case ChecksMateFairyChessPawnUpgrades.Off:
          return "off";
        case ChecksMateFairyChessPawnUpgrades.Pool:
          return "pool";
        case ChecksMateFairyChessPawnUpgrades.Max:
          return "max";
        case ChecksMateFairyChessPawnUpgrades.SuperMax:
          return "supermax";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    public static string AsymmetricTrade(ChecksMateAsymmetricTrade value)
    {
      switch (value)
      {
        case ChecksMateAsymmetricTrade.Disabled:
          return "disabled";
        case ChecksMateAsymmetricTrade.Jacks:
          return "jacks";
        default:
          throw InvalidEnum(nameof(value), value);
      }
    }

    private static InvalidEnumArgumentException InvalidEnum<TEnum>(string name, TEnum value)
      where TEnum : struct
    {
      return new InvalidEnumArgumentException(name, Convert.ToInt32(value), typeof(TEnum));
    }
  }
}
