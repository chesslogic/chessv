using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Archipelago.APChessV.Generation.Settings
{
  public static class ChecksMateGenerationSettingsYamlSerializer
  {
    private const string NameKey = "name";
    private const string GameKey = "game";
    private const string DescriptionKey = "description";
    private const string RequiresKey = "requires";
    private const string VersionKey = "version";

    private static readonly HashSet<string> KnownTopLevelKeys = new HashSet<string>(StringComparer.Ordinal)
    {
      NameKey,
      GameKey,
      DescriptionKey,
      RequiresKey,
      ChecksMateGenerationSettings.DefaultGame,
    };

    private static readonly HashSet<string> KnownRequiresKeys = new HashSet<string>(StringComparer.Ordinal)
    {
      VersionKey,
    };

    private static readonly HashSet<string> KnownChecksMateOptionKeys = new HashSet<string>(StringComparer.Ordinal)
    {
      ChecksMateGenerationOptionKeys.ProgressionBalancing,
      ChecksMateGenerationOptionKeys.Accessibility,
      ChecksMateGenerationOptionKeys.Goal,
      ChecksMateGenerationOptionKeys.Difficulty,
      ChecksMateGenerationOptionKeys.EnableTactics,
      ChecksMateGenerationOptionKeys.PieceLocations,
      ChecksMateGenerationOptionKeys.PieceTypes,
      ChecksMateGenerationOptionKeys.EarlyMaterial,
      ChecksMateGenerationOptionKeys.MaxEnginePenalties,
      ChecksMateGenerationOptionKeys.MaxPocket,
      ChecksMateGenerationOptionKeys.MaxKings,
      ChecksMateGenerationOptionKeys.FairyKings,
      ChecksMateGenerationOptionKeys.FairyChessPieces,
      ChecksMateGenerationOptionKeys.FairyChessPiecesConfigure,
      ChecksMateGenerationOptionKeys.FairyChessArmy,
      ChecksMateGenerationOptionKeys.FairyChessPawns,
      ChecksMateGenerationOptionKeys.FairyChessPawnUpgrades,
      ChecksMateGenerationOptionKeys.MinorPieceLimitByType,
      ChecksMateGenerationOptionKeys.MajorPieceLimitByType,
      ChecksMateGenerationOptionKeys.QueenPieceLimitByType,
      ChecksMateGenerationOptionKeys.QueenPieceLimit,
      ChecksMateGenerationOptionKeys.PocketLimitByPocket,
      ChecksMateGenerationOptionKeys.DeathLink,
      ChecksMateGenerationOptionKeys.AsymmetricTrades,
    };

    private static readonly IDictionary<string, ChecksMateAccessibility> AccessibilityNames =
      new Dictionary<string, ChecksMateAccessibility>(StringComparer.OrdinalIgnoreCase)
      {
        ["full"] = ChecksMateAccessibility.Full,
        ["minimal"] = ChecksMateAccessibility.Minimal,
        ["items"] = ChecksMateAccessibility.Items,
        ["locations"] = ChecksMateAccessibility.Locations,
      };

    private static readonly IDictionary<string, ChecksMateGoal> GoalNames =
      new Dictionary<string, ChecksMateGoal>(StringComparer.OrdinalIgnoreCase)
      {
        ["single"] = ChecksMateGoal.Single,
        ["ordered_progressive"] = ChecksMateGoal.OrderedProgressive,
        ["progressive"] = ChecksMateGoal.Progressive,
        ["super"] = ChecksMateGoal.Super,
      };

    private static readonly IDictionary<string, ChecksMateDifficulty> DifficultyNames =
      new Dictionary<string, ChecksMateDifficulty>(StringComparer.OrdinalIgnoreCase)
      {
        ["daily"] = ChecksMateDifficulty.Daily,
      };

    private static readonly IDictionary<string, ChecksMateTactics> TacticsNames =
      new Dictionary<string, ChecksMateTactics>(StringComparer.OrdinalIgnoreCase)
      {
        ["all"] = ChecksMateTactics.All,
      };

    private static readonly IDictionary<string, ChecksMatePieceLocations> PieceLocationNames =
      new Dictionary<string, ChecksMatePieceLocations>(StringComparer.OrdinalIgnoreCase)
      {
        ["chaos"] = ChecksMatePieceLocations.Chaos,
        ["stable"] = ChecksMatePieceLocations.Stable,
        ["ordered"] = ChecksMatePieceLocations.Ordered,
      };

    private static readonly IDictionary<string, ChecksMatePieceTypes> PieceTypeNames =
      new Dictionary<string, ChecksMatePieceTypes>(StringComparer.OrdinalIgnoreCase)
      {
        ["chaos"] = ChecksMatePieceTypes.Chaos,
        ["stable"] = ChecksMatePieceTypes.Stable,
        ["book"] = ChecksMatePieceTypes.Book,
      };

    private static readonly IDictionary<string, ChecksMateSwitch> SwitchNames =
      new Dictionary<string, ChecksMateSwitch>(StringComparer.OrdinalIgnoreCase)
      {
        ["off"] = ChecksMateSwitch.Off,
      };

    private static readonly IDictionary<string, ChecksMateLimitValue> RandomLimitNames =
      new Dictionary<string, ChecksMateLimitValue>(StringComparer.OrdinalIgnoreCase)
      {
        ["random"] = ChecksMateLimitValue.Random,
        ["random-low"] = ChecksMateLimitValue.RandomLow,
        ["random-high"] = ChecksMateLimitValue.RandomHigh,
      };

    private static readonly IDictionary<string, ChecksMateFairyChessPieces> FairyChessPieceNames =
      new Dictionary<string, ChecksMateFairyChessPieces>(StringComparer.OrdinalIgnoreCase)
      {
        ["fide"] = ChecksMateFairyChessPieces.Fide,
        ["betza"] = ChecksMateFairyChessPieces.Betza,
        ["full"] = ChecksMateFairyChessPieces.Full,
        ["configure"] = ChecksMateFairyChessPieces.Configure,
      };

    private static readonly IDictionary<string, ChecksMateFairyChessPieceSet> FairyChessPieceSetNames =
      new Dictionary<string, ChecksMateFairyChessPieceSet>(StringComparer.OrdinalIgnoreCase)
      {
        ["Petal"] = ChecksMateFairyChessPieceSet.Petal,
        ["Cannon"] = ChecksMateFairyChessPieceSet.Cannon,
        ["Nutty"] = ChecksMateFairyChessPieceSet.Nutty,
        ["Rookies"] = ChecksMateFairyChessPieceSet.Rookies,
        ["FIDE"] = ChecksMateFairyChessPieceSet.Fide,
        ["Clobberers"] = ChecksMateFairyChessPieceSet.Clobberers,
        ["Camel"] = ChecksMateFairyChessPieceSet.Camel,
      };

    private static readonly IDictionary<string, ChecksMateFairyChessArmy> FairyChessArmyNames =
      new Dictionary<string, ChecksMateFairyChessArmy>(StringComparer.OrdinalIgnoreCase)
      {
        ["chaos"] = ChecksMateFairyChessArmy.Chaos,
        ["stable"] = ChecksMateFairyChessArmy.Stable,
        ["limited"] = ChecksMateFairyChessArmy.Limited,
      };

    private static readonly IDictionary<string, ChecksMateFairyChessPawns> FairyChessPawnNames =
      new Dictionary<string, ChecksMateFairyChessPawns>(StringComparer.OrdinalIgnoreCase)
      {
        ["vanilla"] = ChecksMateFairyChessPawns.Vanilla,
        ["mixed"] = ChecksMateFairyChessPawns.Mixed,
        ["berolina"] = ChecksMateFairyChessPawns.Berolina,
        ["checkers"] = ChecksMateFairyChessPawns.Checkers,
        ["reserved"] = ChecksMateFairyChessPawns.Reserved,
        ["any_pawn"] = ChecksMateFairyChessPawns.AnyPawn,
        ["any_fairy"] = ChecksMateFairyChessPawns.AnyFairy,
        ["any_classical"] = ChecksMateFairyChessPawns.AnyClassical,
      };

    private static readonly IDictionary<string, ChecksMateFairyChessPawnUpgrades> FairyChessPawnUpgradeNames =
      new Dictionary<string, ChecksMateFairyChessPawnUpgrades>(StringComparer.OrdinalIgnoreCase)
      {
        ["off"] = ChecksMateFairyChessPawnUpgrades.Off,
        ["pool"] = ChecksMateFairyChessPawnUpgrades.Pool,
        ["max"] = ChecksMateFairyChessPawnUpgrades.Max,
        ["supermax"] = ChecksMateFairyChessPawnUpgrades.SuperMax,
      };

    private static readonly IDictionary<string, ChecksMateAsymmetricTrade> AsymmetricTradeNames =
      new Dictionary<string, ChecksMateAsymmetricTrade>(StringComparer.OrdinalIgnoreCase)
      {
        ["disabled"] = ChecksMateAsymmetricTrade.Disabled,
        ["jacks"] = ChecksMateAsymmetricTrade.Jacks,
      };

    public static string Serialize(ChecksMateGenerationSettings settings)
    {
      if (settings == null)
      {
        throw new ArgumentNullException(nameof(settings));
      }

      return new LimitedYamlWriter().Write(settings.ToYamlCompatibleMap());
    }

    public static void Save(string path, ChecksMateGenerationSettings settings)
    {
      if (path == null)
      {
        throw new ArgumentNullException(nameof(path));
      }

      File.WriteAllText(path, Serialize(settings), Encoding.UTF8);
    }

    public static ChecksMateGenerationSettings Deserialize(string yaml)
    {
      if (yaml == null)
      {
        throw new ArgumentNullException(nameof(yaml));
      }

      var root = new LimitedYamlParser(yaml).Parse();
      var requires = ReadRequiredMap(root, RequiresKey, RequiresKey);
      var checksMateOptions = ReadRequiredMap(
        root,
        ChecksMateGenerationSettings.DefaultGame,
        ChecksMateGenerationSettings.DefaultGame);

      var settings = new ChecksMateGenerationSettings
      {
        Name = ReadRequiredString(root, NameKey, NameKey),
        Game = ReadRequiredString(root, GameKey, GameKey),
        Description = ReadRequiredString(root, DescriptionKey, DescriptionKey),
        RequiredArchipelagoVersion = ReadRequiredString(
          requires,
          VersionKey,
          RequiresKey + "." + VersionKey),
      };

      ApplyChecksMateOptions(settings, checksMateOptions);
      CopyUnsupportedFields(root, settings.UnsupportedTopLevelFields, KnownTopLevelKeys);
      CopyUnsupportedFields(requires, settings.UnsupportedRequiresFields, KnownRequiresKeys);
      CopyUnsupportedFields(checksMateOptions, settings.UnsupportedChecksMateOptions, KnownChecksMateOptionKeys);
      EnsureParsedSettingsAreValid(settings);

      return settings;
    }

    public static ChecksMateGenerationSettings Load(string path)
    {
      if (path == null)
      {
        throw new ArgumentNullException(nameof(path));
      }

      return Deserialize(File.ReadAllText(path, Encoding.UTF8));
    }

    private static void ApplyChecksMateOptions(
      ChecksMateGenerationSettings settings,
      IDictionary<string, object> options)
    {
      object value;
      if (options.TryGetValue(ChecksMateGenerationOptionKeys.ProgressionBalancing, out value))
      {
        settings.ProgressionBalancing = ReadInt(value, ChecksMatePath(ChecksMateGenerationOptionKeys.ProgressionBalancing));
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.Accessibility, out value))
      {
        settings.Accessibility = ReadEnum(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.Accessibility),
          AccessibilityNames);
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.Goal, out value))
      {
        settings.Goal = ReadEnum(value, ChecksMatePath(ChecksMateGenerationOptionKeys.Goal), GoalNames);
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.Difficulty, out value))
      {
        settings.Difficulty = ReadEnum(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.Difficulty),
          DifficultyNames);
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.EnableTactics, out value))
      {
        settings.EnableTactics = ReadEnum(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.EnableTactics),
          TacticsNames);
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.PieceLocations, out value))
      {
        settings.PieceLocations = ReadEnum(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.PieceLocations),
          PieceLocationNames);
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.PieceTypes, out value))
      {
        settings.PieceTypes = ReadEnum(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.PieceTypes),
          PieceTypeNames);
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.EarlyMaterial, out value))
      {
        settings.EarlyMaterial = ReadEnum(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.EarlyMaterial),
          SwitchNames);
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.MaxEnginePenalties, out value))
      {
        settings.MaxEnginePenalties = ReadLimitValue(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.MaxEnginePenalties));
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.MaxPocket, out value))
      {
        settings.MaxPocket = ReadLimitValue(value, ChecksMatePath(ChecksMateGenerationOptionKeys.MaxPocket));
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.MaxKings, out value))
      {
        settings.MaxKings = ReadLimitValue(value, ChecksMatePath(ChecksMateGenerationOptionKeys.MaxKings));
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.FairyKings, out value))
      {
        settings.FairyKings = ReadLimitValue(value, ChecksMatePath(ChecksMateGenerationOptionKeys.FairyKings));
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.FairyChessPieces, out value))
      {
        settings.FairyChessPieces = ReadEnum(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.FairyChessPieces),
          FairyChessPieceNames);
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.FairyChessPiecesConfigure, out value))
      {
        ReadFairyChessPieceSetList(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.FairyChessPiecesConfigure),
          settings.FairyChessPiecesConfigure);
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.FairyChessArmy, out value))
      {
        settings.FairyChessArmy = ReadEnum(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.FairyChessArmy),
          FairyChessArmyNames);
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.FairyChessPawns, out value))
      {
        settings.FairyChessPawns = ReadEnum(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.FairyChessPawns),
          FairyChessPawnNames);
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.FairyChessPawnUpgrades, out value))
      {
        settings.FairyChessPawnUpgrades = ReadEnum(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.FairyChessPawnUpgrades),
          FairyChessPawnUpgradeNames);
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.MinorPieceLimitByType, out value))
      {
        settings.MinorPieceLimitByType = ReadInt(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.MinorPieceLimitByType));
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.MajorPieceLimitByType, out value))
      {
        settings.MajorPieceLimitByType = ReadInt(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.MajorPieceLimitByType));
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.QueenPieceLimitByType, out value))
      {
        settings.QueenPieceLimitByType = ReadInt(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.QueenPieceLimitByType));
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.QueenPieceLimit, out value))
      {
        settings.QueenPieceLimit = ReadInt(value, ChecksMatePath(ChecksMateGenerationOptionKeys.QueenPieceLimit));
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.PocketLimitByPocket, out value))
      {
        settings.PocketLimitByPocket = ReadInt(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.PocketLimitByPocket));
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.DeathLink, out value))
      {
        settings.DeathLink = ReadBool(value, ChecksMatePath(ChecksMateGenerationOptionKeys.DeathLink));
      }

      if (options.TryGetValue(ChecksMateGenerationOptionKeys.AsymmetricTrades, out value))
      {
        ReadAsymmetricTrades(
          value,
          ChecksMatePath(ChecksMateGenerationOptionKeys.AsymmetricTrades),
          settings.AsymmetricTrades);
      }
    }

    private static string ChecksMatePath(string optionName)
    {
      return ChecksMateGenerationSettings.DefaultGame + "." + optionName;
    }

    private static void CopyUnsupportedFields(
      IDictionary<string, object> source,
      IDictionary<string, object> target,
      ISet<string> knownKeys)
    {
      target.Clear();
      foreach (var item in source)
      {
        if (!knownKeys.Contains(item.Key))
        {
          target[item.Key] = item.Value;
        }
      }
    }

    private static IDictionary<string, object> ReadRequiredMap(
      IDictionary<string, object> map,
      string key,
      string path)
    {
      object value;
      if (!map.TryGetValue(key, out value))
      {
        throw new ChecksMateGenerationSettingsYamlException(
          "Missing required YAML field '" + path + "'.");
      }

      return ReadMap(value, path);
    }

    private static IDictionary<string, object> ReadMap(object value, string path)
    {
      var map = value as IDictionary<string, object>;
      if (map == null)
      {
        throw new ChecksMateGenerationSettingsYamlException(
          "YAML field '" + path + "' must be a mapping.");
      }

      return map;
    }

    private static string ReadRequiredString(
      IDictionary<string, object> map,
      string key,
      string path)
    {
      object value;
      if (!map.TryGetValue(key, out value))
      {
        throw new ChecksMateGenerationSettingsYamlException(
          "Missing required YAML field '" + path + "'.");
      }

      return ReadString(value, path);
    }

    private static string ReadString(object value, string path)
    {
      var text = value as string;
      if (text != null)
      {
        return text;
      }

      if (value is int)
      {
        return ((int)value).ToString(CultureInfo.InvariantCulture);
      }

      throw new ChecksMateGenerationSettingsYamlException(
        "YAML field '" + path + "' must be a scalar text value.");
    }

    private static int ReadInt(object value, string path)
    {
      if (value is int)
      {
        return (int)value;
      }

      var text = value as string;
      int result;
      if (text != null &&
          int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
      {
        return result;
      }

      throw new ChecksMateGenerationSettingsYamlException(
        "YAML field '" + path + "' must be a whole number.");
    }

    private static bool ReadBool(object value, string path)
    {
      if (value is bool)
      {
        return (bool)value;
      }

      var text = value as string;
      bool result;
      if (text != null && bool.TryParse(text, out result))
      {
        return result;
      }

      throw new ChecksMateGenerationSettingsYamlException(
        "YAML field '" + path + "' must be true or false.");
    }

    private static TEnum ReadEnum<TEnum>(
      object value,
      string path,
      IDictionary<string, TEnum> names)
      where TEnum : struct
    {
      var text = ReadString(value, path).Trim();
      TEnum result;
      if (names.TryGetValue(text, out result))
      {
        return result;
      }

      throw new ChecksMateGenerationSettingsYamlException(
        "YAML field '" + path + "' has unsupported value '" + text +
        "'. Expected one of: " + string.Join(", ", names.Keys) + ".");
    }

    private static ChecksMateLimitValue ReadLimitValue(object value, string path)
    {
      int fixedValue;
      if (value is int)
      {
        fixedValue = (int)value;
        if (fixedValue < 0)
        {
          throw new ChecksMateGenerationSettingsYamlException(
            "YAML field '" + path + "' must be zero or greater, random, random-low, or random-high.");
        }

        return ChecksMateLimitValue.Fixed(fixedValue);
      }

      var text = ReadString(value, path).Trim();
      if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out fixedValue))
      {
        if (fixedValue < 0)
        {
          throw new ChecksMateGenerationSettingsYamlException(
            "YAML field '" + path + "' must be zero or greater, random, random-low, or random-high.");
        }

        return ChecksMateLimitValue.Fixed(fixedValue);
      }

      ChecksMateLimitValue result;
      if (RandomLimitNames.TryGetValue(text, out result))
      {
        return result;
      }

      throw new ChecksMateGenerationSettingsYamlException(
        "YAML field '" + path + "' has unsupported value '" + text +
        "'. Expected a non-negative number, random, random-low, or random-high.");
    }

    private static void ReadFairyChessPieceSetList(
      object value,
      string path,
      IList<ChecksMateFairyChessPieceSet> target)
    {
      if (value is string || value is IDictionary<string, object>)
      {
        throw new ChecksMateGenerationSettingsYamlException(
          "YAML field '" + path + "' must be a sequence.");
      }

      var sequence = value as IEnumerable;
      if (sequence == null)
      {
        throw new ChecksMateGenerationSettingsYamlException(
          "YAML field '" + path + "' must be a sequence.");
      }

      target.Clear();
      var index = 0;
      foreach (var item in sequence)
      {
        target.Add(ReadEnum(
          item,
          path + "[" + index.ToString(CultureInfo.InvariantCulture) + "]",
          FairyChessPieceSetNames));
        index++;
      }
    }

    private static void ReadAsymmetricTrades(
      object value,
      string path,
      IDictionary<ChecksMateAsymmetricTrade, int> target)
    {
      var weights = ReadMap(value, path);
      target.Clear();

      foreach (var weight in weights)
      {
        ChecksMateAsymmetricTrade trade;
        if (!AsymmetricTradeNames.TryGetValue(weight.Key, out trade))
        {
          throw new ChecksMateGenerationSettingsYamlException(
            "YAML field '" + path + "." + weight.Key +
            "' is not a supported asymmetric trade option.");
        }

        target[trade] = ReadInt(weight.Value, path + "." + weight.Key);
      }
    }

    private static void EnsureParsedSettingsAreValid(ChecksMateGenerationSettings settings)
    {
      var errors = settings.Validate();
      if (errors.Count > 0)
      {
        throw new ChecksMateGenerationSettingsYamlException(
          "ChecksMate settings YAML is invalid: " +
          string.Join("; ", errors.Select(error => error.ToString())));
      }
    }

    private sealed class LimitedYamlWriter
    {
      private readonly StringBuilder builder = new StringBuilder();

      public string Write(IDictionary<string, object> map)
      {
        AppendMap(map, 0);
        return builder.ToString();
      }

      private void AppendMap(IDictionary<string, object> map, int indent)
      {
        foreach (var item in map)
        {
          AppendIndent(indent);
          builder.Append(FormatKey(item.Key));

          IDictionary<string, object> childMap;
          IEnumerable childSequence;
          if (TryGetMap(item.Value, out childMap))
          {
            builder.AppendLine(":");
            AppendMap(childMap, indent + 2);
          }
          else if (TryGetSequence(item.Value, out childSequence))
          {
            builder.AppendLine(":");
            AppendSequence(childSequence, indent + 2);
          }
          else
          {
            builder.Append(": ");
            builder.AppendLine(FormatScalar(item.Value));
          }
        }
      }

      private void AppendSequence(IEnumerable sequence, int indent)
      {
        foreach (var item in sequence)
        {
          AppendIndent(indent);
          builder.Append("- ");

          IDictionary<string, object> childMap;
          IEnumerable childSequence;
          if (TryGetMap(item, out childMap))
          {
            builder.AppendLine();
            AppendMap(childMap, indent + 2);
          }
          else if (TryGetSequence(item, out childSequence))
          {
            builder.AppendLine();
            AppendSequence(childSequence, indent + 2);
          }
          else
          {
            builder.AppendLine(FormatScalar(item));
          }
        }
      }

      private void AppendIndent(int indent)
      {
        builder.Append(' ', indent);
      }

      private static bool TryGetMap(object value, out IDictionary<string, object> map)
      {
        map = value as IDictionary<string, object>;
        if (map != null)
        {
          return true;
        }

        var dictionary = value as IDictionary;
        if (dictionary == null)
        {
          return false;
        }

        map = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (DictionaryEntry entry in dictionary)
        {
          var key = entry.Key as string;
          if (key == null)
          {
            return false;
          }

          map[key] = entry.Value;
        }

        return true;
      }

      private static bool TryGetSequence(object value, out IEnumerable sequence)
      {
        if (value is string || value is IDictionary)
        {
          sequence = null;
          return false;
        }

        sequence = value as IEnumerable;
        return sequence != null;
      }

      private static string FormatKey(string key)
      {
        if (IsPlainKey(key))
        {
          return key;
        }

        return QuoteString(key);
      }

      private static bool IsPlainKey(string key)
      {
        if (string.IsNullOrEmpty(key))
        {
          return false;
        }

        foreach (var character in key)
        {
          if (!char.IsLetterOrDigit(character) && character != '_' && character != '-')
          {
            return false;
          }
        }

        return true;
      }

      private static string FormatScalar(object value)
      {
        if (value == null)
        {
          return "null";
        }

        if (value is bool)
        {
          return ((bool)value) ? "true" : "false";
        }

        if (value is int)
        {
          return ((int)value).ToString(CultureInfo.InvariantCulture);
        }

        var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        return ShouldQuoteString(text) ? QuoteString(text) : text;
      }

      private static bool ShouldQuoteString(string text)
      {
        if (text.Length == 0 ||
            !string.Equals(text.Trim(), text, StringComparison.Ordinal) ||
            text.IndexOfAny(new[] { '\r', '\n', '\t' }) >= 0 ||
            text.Contains(": ") ||
            text.Contains(" #") ||
            StartsWithYamlSpecialCharacter(text))
        {
          return true;
        }

        int ignored;
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out ignored))
        {
          return true;
        }

        return IsYamlBooleanLike(text) ||
               string.Equals(text, "null", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(text, "~", StringComparison.Ordinal);
      }

      private static bool StartsWithYamlSpecialCharacter(string text)
      {
        switch (text[0])
        {
          case '-':
          case '?':
          case ':':
          case ',':
          case '[':
          case ']':
          case '{':
          case '}':
          case '#':
          case '&':
          case '*':
          case '!':
          case '|':
          case '>':
          case '\'':
          case '"':
          case '%':
          case '@':
          case '`':
            return true;
          default:
            return false;
        }
      }

      private static bool IsYamlBooleanLike(string text)
      {
        return string.Equals(text, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(text, "false", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(text, "no", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(text, "on", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(text, "off", StringComparison.OrdinalIgnoreCase);
      }

      private static string QuoteString(string text)
      {
        return "'" + text.Replace("'", "''") + "'";
      }
    }

    private sealed class LimitedYamlParser
    {
      private readonly IList<YamlLine> lines;
      private int index;

      public LimitedYamlParser(string yaml)
      {
        lines = NormalizeLines(yaml);
      }

      public IDictionary<string, object> Parse()
      {
        if (lines.Count == 0)
        {
          throw new ChecksMateGenerationSettingsYamlException("ChecksMate settings YAML is empty.");
        }

        index = 0;
        var map = ParseMap(lines[0].Indent);
        if (index < lines.Count)
        {
          throw Error(lines[index], "Unexpected YAML content.");
        }

        return map;
      }

      private IDictionary<string, object> ParseMap(int indent)
      {
        var map = new Dictionary<string, object>(StringComparer.Ordinal);
        while (index < lines.Count)
        {
          var line = lines[index];
          if (line.Indent < indent)
          {
            break;
          }

          if (line.Indent > indent)
          {
            throw Error(line, "Unexpected indentation.");
          }

          var colonIndex = FindMappingColon(line.Text);
          if (colonIndex < 0)
          {
            throw Error(line, "Expected a YAML mapping entry in the form 'key: value'.");
          }

          var key = ParseKey(line.Text.Substring(0, colonIndex), line.LineNumber);
          if (key.Length == 0)
          {
            throw Error(line, "YAML mapping keys cannot be empty.");
          }

          if (map.ContainsKey(key))
          {
            throw Error(line, "Duplicate YAML key '" + key + "'.");
          }

          var valueText = line.Text.Substring(colonIndex + 1).Trim();
          index++;
          if (valueText.Length > 0)
          {
            map[key] = ParseScalar(valueText, line.LineNumber);
          }
          else if (index >= lines.Count || lines[index].Indent <= indent)
          {
            map[key] = new Dictionary<string, object>(StringComparer.Ordinal);
          }
          else
          {
            map[key] = ParseNode(lines[index].Indent);
          }
        }

        return map;
      }

      private object ParseNode(int indent)
      {
        if (index >= lines.Count)
        {
          throw new ChecksMateGenerationSettingsYamlException("Unexpected end of YAML.");
        }

        var line = lines[index];
        if (line.Indent != indent)
        {
          throw Error(line, "Unexpected indentation.");
        }

        if (line.Text.StartsWith("-", StringComparison.Ordinal))
        {
          return ParseSequence(indent);
        }

        if (line.Text.StartsWith("[", StringComparison.Ordinal))
        {
          index++;
          return ParseScalar(line.Text, line.LineNumber);
        }

        if (FindMappingColon(line.Text) >= 0)
        {
          return ParseMap(indent);
        }

        index++;
        return ParseScalar(line.Text, line.LineNumber);
      }

      private IList<object> ParseSequence(int indent)
      {
        var sequence = new List<object>();
        while (index < lines.Count)
        {
          var line = lines[index];
          if (line.Indent < indent)
          {
            break;
          }

          if (line.Indent != indent || !line.Text.StartsWith("-", StringComparison.Ordinal))
          {
            throw Error(line, "Expected a YAML sequence item.");
          }

          var valueText = line.Text.Substring(1).Trim();
          index++;
          if (valueText.Length > 0)
          {
            sequence.Add(ParseScalar(valueText, line.LineNumber));
          }
          else if (index >= lines.Count || lines[index].Indent <= indent)
          {
            sequence.Add(null);
          }
          else
          {
            sequence.Add(ParseNode(lines[index].Indent));
          }
        }

        return sequence;
      }

      private static IList<YamlLine> NormalizeLines(string yaml)
      {
        var rawLines = yaml.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var startIndex = 0;
        for (var i = 0; i < rawLines.Length; i++)
        {
          if (string.Equals(rawLines[i].Trim(), "-----", StringComparison.Ordinal))
          {
            startIndex = i + 1;
            break;
          }
        }

        var result = new List<YamlLine>();
        for (var i = startIndex; i < rawLines.Length; i++)
        {
          var rawLine = i == startIndex ? rawLines[i].TrimStart('\uFEFF') : rawLines[i];
          if (string.Equals(rawLine.Trim(), "---", StringComparison.Ordinal) ||
              string.Equals(rawLine.Trim(), "...", StringComparison.Ordinal))
          {
            continue;
          }

          var indent = CountIndent(rawLine, i + 1);
          var withoutIndent = rawLine.Substring(indent);
          var withoutComment = StripComment(withoutIndent).TrimEnd();
          if (withoutComment.Trim().Length == 0)
          {
            continue;
          }

          result.Add(new YamlLine(indent, withoutComment.TrimStart(), i + 1));
        }

        return result;
      }

      private static int CountIndent(string line, int lineNumber)
      {
        var indent = 0;
        while (indent < line.Length)
        {
          if (line[indent] == ' ')
          {
            indent++;
          }
          else if (line[indent] == '\t')
          {
            throw new ChecksMateGenerationSettingsYamlException(
              "Line " + lineNumber.ToString(CultureInfo.InvariantCulture) +
              ": Tabs are not supported for YAML indentation.");
          }
          else
          {
            break;
          }
        }

        return indent;
      }

      private static string StripComment(string text)
      {
        var inSingleQuote = false;
        var inDoubleQuote = false;
        for (var i = 0; i < text.Length; i++)
        {
          var character = text[i];
          if (inSingleQuote)
          {
            if (character == '\'' && i + 1 < text.Length && text[i + 1] == '\'')
            {
              i++;
            }
            else if (character == '\'')
            {
              inSingleQuote = false;
            }
          }
          else if (inDoubleQuote)
          {
            if (character == '\\')
            {
              i++;
            }
            else if (character == '"')
            {
              inDoubleQuote = false;
            }
          }
          else if (character == '\'')
          {
            inSingleQuote = true;
          }
          else if (character == '"')
          {
            inDoubleQuote = true;
          }
          else if (character == '#' &&
                   (i == 0 || char.IsWhiteSpace(text[i - 1])))
          {
            return text.Substring(0, i);
          }
        }

        return text;
      }

      private static int FindMappingColon(string text)
      {
        var inSingleQuote = false;
        var inDoubleQuote = false;
        for (var i = 0; i < text.Length; i++)
        {
          var character = text[i];
          if (inSingleQuote)
          {
            if (character == '\'' && i + 1 < text.Length && text[i + 1] == '\'')
            {
              i++;
            }
            else if (character == '\'')
            {
              inSingleQuote = false;
            }
          }
          else if (inDoubleQuote)
          {
            if (character == '\\')
            {
              i++;
            }
            else if (character == '"')
            {
              inDoubleQuote = false;
            }
          }
          else if (character == '\'')
          {
            inSingleQuote = true;
          }
          else if (character == '"')
          {
            inDoubleQuote = true;
          }
          else if (character == ':' &&
                   (i + 1 == text.Length || char.IsWhiteSpace(text[i + 1])))
          {
            return i;
          }
        }

        return -1;
      }

      private static string ParseKey(string text, int lineNumber)
      {
        var key = text.Trim();
        if (key.StartsWith("'", StringComparison.Ordinal) ||
            key.StartsWith("\"", StringComparison.Ordinal))
        {
          var value = ParseScalar(key, lineNumber) as string;
          if (value == null)
          {
            throw new ChecksMateGenerationSettingsYamlException(
              "Line " + lineNumber.ToString(CultureInfo.InvariantCulture) +
              ": YAML mapping key must be text.");
          }

          return value;
        }

        return key;
      }

      private static object ParseScalar(string text, int lineNumber)
      {
        text = text.Trim();
        if (text.StartsWith("[", StringComparison.Ordinal))
        {
          return ParseFlowSequence(text, lineNumber);
        }

        if (text.StartsWith("'", StringComparison.Ordinal))
        {
          if (!text.EndsWith("'", StringComparison.Ordinal) || text.Length == 1)
          {
            throw new ChecksMateGenerationSettingsYamlException(
              "Line " + lineNumber.ToString(CultureInfo.InvariantCulture) +
              ": Single-quoted YAML scalar is not closed.");
          }

          return text.Substring(1, text.Length - 2).Replace("''", "'");
        }

        if (text.StartsWith("\"", StringComparison.Ordinal))
        {
          if (!text.EndsWith("\"", StringComparison.Ordinal) || text.Length == 1)
          {
            throw new ChecksMateGenerationSettingsYamlException(
              "Line " + lineNumber.ToString(CultureInfo.InvariantCulture) +
              ": Double-quoted YAML scalar is not closed.");
          }

          return UnescapeDoubleQuotedString(text.Substring(1, text.Length - 2));
        }

        if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase))
        {
          return true;
        }

        if (string.Equals(text, "false", StringComparison.OrdinalIgnoreCase))
        {
          return false;
        }

        if (string.Equals(text, "null", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(text, "~", StringComparison.Ordinal))
        {
          return null;
        }

        int number;
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
        {
          return number;
        }

        return text;
      }

      private static IList<object> ParseFlowSequence(string text, int lineNumber)
      {
        if (!text.EndsWith("]", StringComparison.Ordinal))
        {
          throw new ChecksMateGenerationSettingsYamlException(
            "Line " + lineNumber.ToString(CultureInfo.InvariantCulture) +
            ": Flow sequence is not closed.");
        }

        var content = text.Substring(1, text.Length - 2).Trim();
        var sequence = new List<object>();
        if (content.Length == 0)
        {
          return sequence;
        }

        var itemStart = 0;
        var inSingleQuote = false;
        var inDoubleQuote = false;
        for (var i = 0; i <= content.Length; i++)
        {
          var atEnd = i == content.Length;
          var character = atEnd ? ',' : content[i];
          if (inSingleQuote)
          {
            if (character == '\'' && i + 1 < content.Length && content[i + 1] == '\'')
            {
              i++;
            }
            else if (character == '\'')
            {
              inSingleQuote = false;
            }
          }
          else if (inDoubleQuote)
          {
            if (character == '\\')
            {
              i++;
            }
            else if (character == '"')
            {
              inDoubleQuote = false;
            }
          }
          else if (character == '\'')
          {
            inSingleQuote = true;
          }
          else if (character == '"')
          {
            inDoubleQuote = true;
          }
          else if (character == ',')
          {
            var item = content.Substring(itemStart, i - itemStart).Trim();
            if (item.Length == 0)
            {
              throw new ChecksMateGenerationSettingsYamlException(
                "Line " + lineNumber.ToString(CultureInfo.InvariantCulture) +
                ": Flow sequence contains an empty item.");
            }

            sequence.Add(ParseScalar(item, lineNumber));
            itemStart = i + 1;
          }
        }

        return sequence;
      }

      private static string UnescapeDoubleQuotedString(string text)
      {
        var result = new StringBuilder();
        for (var i = 0; i < text.Length; i++)
        {
          var character = text[i];
          if (character != '\\' || i + 1 == text.Length)
          {
            result.Append(character);
            continue;
          }

          i++;
          switch (text[i])
          {
            case 'n':
              result.Append('\n');
              break;
            case 'r':
              result.Append('\r');
              break;
            case 't':
              result.Append('\t');
              break;
            case '\\':
              result.Append('\\');
              break;
            case '"':
              result.Append('"');
              break;
            default:
              result.Append(text[i]);
              break;
          }
        }

        return result.ToString();
      }

      private static ChecksMateGenerationSettingsYamlException Error(YamlLine line, string message)
      {
        return new ChecksMateGenerationSettingsYamlException(
          "Line " + line.LineNumber.ToString(CultureInfo.InvariantCulture) + ": " + message);
      }
    }

    private sealed class YamlLine
    {
      public YamlLine(int indent, string text, int lineNumber)
      {
        Indent = indent;
        Text = text;
        LineNumber = lineNumber;
      }

      public int Indent { get; private set; }

      public string Text { get; private set; }

      public int LineNumber { get; private set; }
    }
  }

  public sealed class ChecksMateGenerationSettingsYamlException : FormatException
  {
    public ChecksMateGenerationSettingsYamlException(string message)
      : base(message)
    {
    }

    public ChecksMateGenerationSettingsYamlException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}
