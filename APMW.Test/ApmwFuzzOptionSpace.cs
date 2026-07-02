using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Archipelago.APChessV;

namespace ChessV.Test
{
    internal enum ApmwFuzzOptionAxisKind
    {
        Boolean,
        Enumeration,
        Numeric,
        Set,
    }

    internal sealed class ApmwFuzzOptionValue
    {
        public ApmwFuzzOptionValue(string label, object value, string canonicalKey)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Option value labels must be specified.", nameof(label));
            if (string.IsNullOrWhiteSpace(canonicalKey))
                throw new ArgumentException("Option value canonical keys must be specified.", nameof(canonicalKey));

            Label = label;
            Value = value;
            CanonicalKey = canonicalKey;
        }

        public string Label { get; }

        public object Value { get; }

        public string CanonicalKey { get; }

        public static ApmwFuzzOptionValue Boolean(bool value)
        {
            return new ApmwFuzzOptionValue(
                value ? "true" : "false",
                value,
                value ? "true" : "false");
        }

        public static ApmwFuzzOptionValue Numeric(int value)
        {
            string key = value.ToString(CultureInfo.InvariantCulture);
            return new ApmwFuzzOptionValue(key, value, key);
        }

        public static ApmwFuzzOptionValue Enumeration<T>(T value)
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new ArgumentException("Enumeration option values require an enum type.", nameof(value));

            string name = Enum.GetName(enumType, value);
            if (name == null)
                name = Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);

            return new ApmwFuzzOptionValue(name, value, NormalizeKey(name));
        }

        public static ApmwFuzzOptionValue IntSet(string label, IEnumerable<int> values)
        {
            int[] normalizedValues = (values ?? Enumerable.Empty<int>())
                .Distinct()
                .OrderBy(value => value)
                .ToArray();
            string canonicalKey = normalizedValues.Length == 0
                ? "empty"
                : string.Join(",", normalizedValues.Select(value => value.ToString(CultureInfo.InvariantCulture)));
            string normalizedLabel = string.IsNullOrWhiteSpace(label) ? canonicalKey : label;

            return new ApmwFuzzOptionValue(
                normalizedLabel,
                new ReadOnlyCollection<int>(normalizedValues),
                canonicalKey);
        }

        public override string ToString()
        {
            return Label + " [" + CanonicalKey + "]";
        }

        private static string NormalizeKey(string value)
        {
            return value.Trim().ToLowerInvariant();
        }
    }

    internal sealed class ApmwFuzzNumericBand
    {
        public ApmwFuzzNumericBand(string name, int defaultValue, int maxValue, params int[] overCapValues)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Numeric band names must be specified.", nameof(name));
            if (defaultValue < 0)
                throw new ArgumentOutOfRangeException(nameof(defaultValue), "Numeric band defaults cannot be negative.");
            if (maxValue < 0)
                throw new ArgumentOutOfRangeException(nameof(maxValue), "Numeric band maximums cannot be negative.");

            Name = name;
            DefaultValue = defaultValue;
            MaxValue = maxValue;
            OverCapValues = new ReadOnlyCollection<int>((overCapValues ?? new int[0]).Where(value => value >= 0).ToArray());
            Values = new ReadOnlyCollection<int>(BuildValues(defaultValue, maxValue, OverCapValues).ToArray());
        }

        public string Name { get; }

        public int DefaultValue { get; }

        public int MaxValue { get; }

        public IReadOnlyList<int> OverCapValues { get; }

        public IReadOnlyList<int> Values { get; }

        private static IEnumerable<int> BuildValues(int defaultValue, int maxValue, IEnumerable<int> overCapValues)
        {
            var seen = new HashSet<int>();

            foreach (int value in PeripheryValues(defaultValue, maxValue))
            {
                if (value >= 0 && seen.Add(value))
                    yield return value;
            }

            foreach (int value in overCapValues ?? Enumerable.Empty<int>())
            {
                if (value >= 0 && seen.Add(value))
                    yield return value;
            }
        }

        private static IEnumerable<int> PeripheryValues(int defaultValue, int maxValue)
        {
            yield return 0;
            yield return 1;
            yield return defaultValue;
            if (maxValue > 0)
                yield return maxValue - 1;
            yield return maxValue;
            if (maxValue < int.MaxValue)
                yield return maxValue + 1;
        }
    }

    internal sealed class ApmwFuzzAxis
    {
        public ApmwFuzzAxis(
            string name,
            ApmwFuzzOptionAxisKind kind,
            ApmwFuzzOptionValue defaultValue,
            IEnumerable<ApmwFuzzOptionValue> values,
            IEnumerable<ApmwFuzzNumericBand> numericBands = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Axis names must be specified.", nameof(name));
            if (defaultValue == null)
                throw new ArgumentNullException(nameof(defaultValue));
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            Name = name;
            Kind = kind;
            DefaultValue = defaultValue;
            Values = new ReadOnlyCollection<ApmwFuzzOptionValue>(DeduplicateValues(values, defaultValue).ToArray());
            NumericBands = new ReadOnlyCollection<ApmwFuzzNumericBand>((numericBands ?? Enumerable.Empty<ApmwFuzzNumericBand>()).ToArray());
        }

        public string Name { get; }

        public ApmwFuzzOptionAxisKind Kind { get; }

        public ApmwFuzzOptionValue DefaultValue { get; }

        public IReadOnlyList<ApmwFuzzOptionValue> Values { get; }

        public IReadOnlyList<ApmwFuzzNumericBand> NumericBands { get; }

        public static ApmwFuzzAxis Boolean(string name, bool defaultValue)
        {
            return new ApmwFuzzAxis(
                name,
                ApmwFuzzOptionAxisKind.Boolean,
                ApmwFuzzOptionValue.Boolean(defaultValue),
                new[] { ApmwFuzzOptionValue.Boolean(false), ApmwFuzzOptionValue.Boolean(true) });
        }

        public static ApmwFuzzAxis Enumeration<T>(string name, T defaultValue)
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new ArgumentException("Enumeration axes require an enum type.", nameof(defaultValue));

            return new ApmwFuzzAxis(
                name,
                ApmwFuzzOptionAxisKind.Enumeration,
                ApmwFuzzOptionValue.Enumeration(defaultValue),
                Enum.GetValues(enumType).Cast<T>().Select(ApmwFuzzOptionValue.Enumeration));
        }

        public static ApmwFuzzAxis Numeric(string name, int defaultValue, params ApmwFuzzNumericBand[] bands)
        {
            if (bands == null || bands.Length == 0)
                throw new ArgumentException("Numeric axes require at least one periphery band.", nameof(bands));

            return new ApmwFuzzAxis(
                name,
                ApmwFuzzOptionAxisKind.Numeric,
                ApmwFuzzOptionValue.Numeric(defaultValue),
                bands.SelectMany(band => band.Values).Select(ApmwFuzzOptionValue.Numeric),
                bands);
        }

        public static ApmwFuzzAxis IntSet(
            string name,
            string defaultLabel,
            IEnumerable<int> defaultValue,
            params ApmwFuzzOptionValue[] values)
        {
            ApmwFuzzOptionValue defaultOption = ApmwFuzzOptionValue.IntSet(defaultLabel, defaultValue);
            return new ApmwFuzzAxis(
                name,
                ApmwFuzzOptionAxisKind.Set,
                defaultOption,
                new[] { defaultOption }.Concat(values ?? new ApmwFuzzOptionValue[0]));
        }

        public bool ContainsValue(ApmwFuzzOptionValue value)
        {
            if (value == null)
                return false;

            return Values.Any(axisValue => string.Equals(axisValue.CanonicalKey, value.CanonicalKey, StringComparison.Ordinal));
        }

        public ApmwFuzzOptionValue GetValue(string canonicalKey)
        {
            if (canonicalKey == null)
                throw new ArgumentNullException(nameof(canonicalKey));

            ApmwFuzzOptionValue value = Values.FirstOrDefault(
                axisValue => string.Equals(axisValue.CanonicalKey, canonicalKey, StringComparison.Ordinal));
            if (value == null)
                throw new ArgumentException(
                    "Axis '" + Name + "' does not contain value '" + canonicalKey + "'.",
                    nameof(canonicalKey));

            return value;
        }

        private static IEnumerable<ApmwFuzzOptionValue> DeduplicateValues(
            IEnumerable<ApmwFuzzOptionValue> values,
            ApmwFuzzOptionValue defaultValue)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            bool containsDefault = false;

            foreach (ApmwFuzzOptionValue value in values)
            {
                if (value == null)
                    throw new ArgumentException("Option axis values cannot contain null.", nameof(values));

                if (string.Equals(value.CanonicalKey, defaultValue.CanonicalKey, StringComparison.Ordinal))
                    containsDefault = true;

                if (seen.Add(value.CanonicalKey))
                    yield return value;
            }

            if (!containsDefault && seen.Add(defaultValue.CanonicalKey))
                yield return defaultValue;
        }
    }

    internal sealed class ApmwFuzzAssignment
    {
        private readonly IReadOnlyDictionary<string, ApmwFuzzOptionValue> valuesByAxis;

        internal ApmwFuzzAssignment(
            ApmwFuzzOptionSpace optionSpace,
            IDictionary<string, ApmwFuzzOptionValue> valuesByAxis)
        {
            if (optionSpace == null)
                throw new ArgumentNullException(nameof(optionSpace));
            if (valuesByAxis == null)
                throw new ArgumentNullException(nameof(valuesByAxis));

            OptionSpace = optionSpace;
            this.valuesByAxis = new ReadOnlyDictionary<string, ApmwFuzzOptionValue>(
                CompleteValues(optionSpace, valuesByAxis));
        }

        public ApmwFuzzOptionSpace OptionSpace { get; }

        public IReadOnlyDictionary<string, ApmwFuzzOptionValue> ValuesByAxis { get { return valuesByAxis; } }

        public string CanonicalKey
        {
            get
            {
                return string.Join("|", OptionSpace.Axes.Select(axis =>
                    axis.Name + "=" + valuesByAxis[axis.Name].CanonicalKey));
            }
        }

        public ApmwFuzzOptionValue Get(string axisName)
        {
            if (axisName == null)
                throw new ArgumentNullException(nameof(axisName));

            if (!valuesByAxis.TryGetValue(axisName, out ApmwFuzzOptionValue value))
                throw new ArgumentException("Unknown option axis '" + axisName + "'.", nameof(axisName));

            return value;
        }

        public int GetInt(string axisName)
        {
            return Convert.ToInt32(Get(axisName).Value, CultureInfo.InvariantCulture);
        }

        public bool GetBool(string axisName)
        {
            return Convert.ToBoolean(Get(axisName).Value, CultureInfo.InvariantCulture);
        }

        public IReadOnlyList<int> GetIntSet(string axisName)
        {
            var values = Get(axisName).Value as IReadOnlyList<int>;
            if (values == null)
                throw new InvalidOperationException("Axis '" + axisName + "' is not an integer set axis.");

            return values;
        }

        public ApmwFuzzAssignment With(string axisName, string canonicalValueKey)
        {
            ApmwFuzzAxis axis = OptionSpace.GetAxis(axisName);
            return WithValue(axisName, axis.GetValue(canonicalValueKey));
        }

        public ApmwFuzzAssignment WithValue(string axisName, ApmwFuzzOptionValue value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            ApmwFuzzAxis axis = OptionSpace.GetAxis(axisName);
            if (!axis.ContainsValue(value))
                throw new ArgumentException(
                    "Axis '" + axisName + "' does not contain value '" + value.CanonicalKey + "'.",
                    nameof(value));

            var updatedValues = valuesByAxis.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            updatedValues[axisName] = axis.GetValue(value.CanonicalKey);
            return new ApmwFuzzAssignment(OptionSpace, updatedValues);
        }

        public override string ToString()
        {
            return CanonicalKey;
        }

        private static Dictionary<string, ApmwFuzzOptionValue> CompleteValues(
            ApmwFuzzOptionSpace optionSpace,
            IDictionary<string, ApmwFuzzOptionValue> valuesByAxis)
        {
            var completedValues = new Dictionary<string, ApmwFuzzOptionValue>(StringComparer.Ordinal);

            foreach (string axisName in valuesByAxis.Keys)
            {
                if (!optionSpace.HasAxis(axisName))
                    throw new ArgumentException("Unknown option axis '" + axisName + "'.", nameof(valuesByAxis));
            }

            foreach (ApmwFuzzAxis axis in optionSpace.Axes)
            {
                ApmwFuzzOptionValue value;
                if (!valuesByAxis.TryGetValue(axis.Name, out value))
                    value = axis.DefaultValue;

                if (!axis.ContainsValue(value))
                    throw new ArgumentException(
                        "Axis '" + axis.Name + "' does not contain value '" + value.CanonicalKey + "'.",
                        nameof(valuesByAxis));

                completedValues[axis.Name] = axis.GetValue(value.CanonicalKey);
            }

            return completedValues;
        }
    }

    internal sealed class ApmwFuzzConstraint
    {
        private readonly Func<ApmwFuzzAssignment, bool> predicate;

        public ApmwFuzzConstraint(string name, string description, Func<ApmwFuzzAssignment, bool> predicate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Constraint names must be specified.", nameof(name));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Constraint descriptions must be specified.", nameof(description));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            Name = name;
            Description = description;
            this.predicate = predicate;
        }

        public string Name { get; }

        public string Description { get; }

        public bool IsSatisfiedBy(ApmwFuzzAssignment assignment)
        {
            if (assignment == null)
                throw new ArgumentNullException(nameof(assignment));

            return predicate(assignment);
        }
    }

    internal sealed class ApmwFuzzConstraintViolation
    {
        public ApmwFuzzConstraintViolation(ApmwFuzzConstraint constraint)
        {
            if (constraint == null)
                throw new ArgumentNullException(nameof(constraint));

            Name = constraint.Name;
            Description = constraint.Description;
        }

        public string Name { get; }

        public string Description { get; }
    }

    internal sealed class ApmwFuzzOptionSpace
    {
        internal const string AxisIsSuperSized = "is-super-sized";
        internal const string AxisGoal = "goal";
        internal const string AxisEnemyPieceTypes = "enemy_piece_types";
        internal const string AxisPieceLocations = "piece_locations";
        internal const string AxisPlayerPieceTypes = "piece_types";
        internal const string AxisFairyChessArmy = "fairy_chess_army";
        internal const string AxisArmy = "army";
        internal const string AxisFairyChessPawns = "fairy_chess_pawns";
        internal const string AxisFairyChessPawnUpgrades = "fairy_chess_pawn_upgrades";
        internal const string AxisMinorPieceLimitByType = "minor_piece_limit_by_type";
        internal const string AxisMajorPieceLimitByType = "major_piece_limit_by_type";
        internal const string AxisQueenPieceLimitByType = "queen_piece_limit_by_type";
        internal const string AxisPocketLimitByPocket = "pocket_limit_by_pocket";
        internal const string AxisDeathLink = "death_link";
        internal const string AxisPocketCount = "pocket-count";
        internal const string AxisPocketRangeCount = "pocket-range-count";
        internal const string AxisPocketGemCount = "pocket-gem-count";
        internal const string AxisAIIntelligenceMalusCount = "ai-intelligence-malus-count";
        internal const string AxisPawnCount = "pawn-count";
        internal const string AxisMinorPieceCount = "minor-piece-count";
        internal const string AxisMajorPieceCount = "major-piece-count";
        internal const string AxisJackCount = "jack-count";
        internal const string AxisMajorToQueenCount = "major-to-queen-count";
        internal const string AxisAmazonCount = "amazon-count";
        internal const string AxisPawnForwardnessCount = "pawn-forwardness-count";
        internal const string AxisConsulCount = "consul-count";
        internal const string AxisKingPromotionCount = "king-promotion-count";
        internal const string AxisSuperSizeMeCount = "super-size-me-count";
        internal const string AxisPlayAsWhiteCount = "play-as-white-count";
        internal const string AxisVictoryCount = "victory-count";

        private const int StandardBoardWidth = 8;
        private const int SuperSizedBoardWidth = 10;
        private const int BoardRanks = 8;
        private const int DefaultPocketLimitByPocket = 4;
        private const int PocketSlots = 3;
        private const int MaxPocketRange = 6;

        public ApmwFuzzOptionSpace(
            IEnumerable<ApmwFuzzAxis> axes,
            IEnumerable<ApmwFuzzConstraint> constraints = null)
        {
            if (axes == null)
                throw new ArgumentNullException(nameof(axes));

            Axes = new ReadOnlyCollection<ApmwFuzzAxis>(ValidateAxes(axes).ToArray());
            Constraints = new ReadOnlyCollection<ApmwFuzzConstraint>(
                (constraints ?? Enumerable.Empty<ApmwFuzzConstraint>()).ToArray());
        }

        public IReadOnlyList<ApmwFuzzAxis> Axes { get; }

        public IReadOnlyList<ApmwFuzzConstraint> Constraints { get; }

        public static ApmwFuzzOptionSpace CreateDefault()
        {
            int standardMaterialCapacity = MaterialCapacity(StandardBoardWidth);
            int superSizedMaterialCapacity = MaterialCapacity(SuperSizedBoardWidth);
            int standardPawnCapacity = PawnCapacity(StandardBoardWidth);
            int superSizedPawnCapacity = PawnCapacity(SuperSizedBoardWidth);

            var axes = new[]
            {
                ApmwFuzzAxis.Boolean(AxisIsSuperSized, false),
                ApmwFuzzAxis.Enumeration(AxisGoal, Goal.Single),
                ApmwFuzzAxis.Enumeration(AxisEnemyPieceTypes, PieceTypes.Book),
                ApmwFuzzAxis.Enumeration(AxisPieceLocations, PieceLocations.Stable),
                ApmwFuzzAxis.Enumeration(AxisPlayerPieceTypes, PieceTypes.Stable),
                ApmwFuzzAxis.Enumeration(AxisFairyChessArmy, FairyArmy.Chaos),
                ApmwFuzzAxis.IntSet(
                    AxisArmy,
                    "all-armies",
                    Enumerable.Range(0, 7),
                    ApmwFuzzOptionValue.IntSet("empty", Enumerable.Empty<int>()),
                    ApmwFuzzOptionValue.IntSet("opening-armies", new[] { 0, 1, 2 }),
                    ApmwFuzzOptionValue.IntSet("scattered-armies", new[] { 0, 3, 5, 6 })),
                ApmwFuzzAxis.Enumeration(AxisFairyChessPawns, FairyPawns.Mixed),
                ApmwFuzzAxis.Enumeration(AxisFairyChessPawnUpgrades, FairyPawnUpgrades.Off),
                ApmwFuzzAxis.Numeric(
                    AxisMinorPieceLimitByType,
                    0,
                    BoardWidthBand("standard-type-limit", 0, StandardBoardWidth),
                    BoardWidthBand("super-sized-type-limit", 0, SuperSizedBoardWidth)),
                ApmwFuzzAxis.Numeric(
                    AxisMajorPieceLimitByType,
                    0,
                    BoardWidthBand("standard-type-limit", 0, StandardBoardWidth),
                    BoardWidthBand("super-sized-type-limit", 0, SuperSizedBoardWidth)),
                ApmwFuzzAxis.Numeric(
                    AxisQueenPieceLimitByType,
                    0,
                    BoardWidthBand("standard-type-limit", 0, StandardBoardWidth),
                    BoardWidthBand("super-sized-type-limit", 0, SuperSizedBoardWidth)),
                ApmwFuzzAxis.Numeric(
                    AxisPocketLimitByPocket,
                    DefaultPocketLimitByPocket,
                    new ApmwFuzzNumericBand("pocket-limit", DefaultPocketLimitByPocket, DefaultPocketLimitByPocket, 8, 10)),
                ApmwFuzzAxis.Boolean(AxisDeathLink, false),
                ApmwFuzzAxis.Numeric(
                    AxisPocketCount,
                    0,
                    new ApmwFuzzNumericBand("pocket-slots", 0, DefaultPocketLimitByPocket * PocketSlots, 24)),
                ApmwFuzzAxis.Numeric(
                    AxisPocketRangeCount,
                    0,
                    new ApmwFuzzNumericBand("pocket-range", 0, MaxPocketRange, MaxPocketRange + 3)),
                ApmwFuzzAxis.Numeric(
                    AxisPocketGemCount,
                    0,
                    new ApmwFuzzNumericBand("pocket-gems", 0, MaxPocketRange, MaxPocketRange + 3)),
                ApmwFuzzAxis.Numeric(
                    AxisAIIntelligenceMalusCount,
                    0,
                    new ApmwFuzzNumericBand("ai-malus", 0, 5)),
                ApmwFuzzAxis.Numeric(
                    AxisPawnCount,
                    StandardBoardWidth,
                    new ApmwFuzzNumericBand("standard-pawns", StandardBoardWidth, standardPawnCapacity, standardPawnCapacity + StandardBoardWidth),
                    new ApmwFuzzNumericBand("super-sized-pawns", SuperSizedBoardWidth, superSizedPawnCapacity, superSizedPawnCapacity + SuperSizedBoardWidth)),
                ApmwFuzzAxis.Numeric(
                    AxisMinorPieceCount,
                    4,
                    new ApmwFuzzNumericBand("standard-material", 4, standardMaterialCapacity, standardMaterialCapacity + StandardBoardWidth),
                    new ApmwFuzzNumericBand("super-sized-material", 4, superSizedMaterialCapacity, superSizedMaterialCapacity + SuperSizedBoardWidth)),
                ApmwFuzzAxis.Numeric(
                    AxisMajorPieceCount,
                    2,
                    new ApmwFuzzNumericBand("standard-material", 2, standardMaterialCapacity, standardMaterialCapacity + StandardBoardWidth),
                    new ApmwFuzzNumericBand("super-sized-material", 2, superSizedMaterialCapacity, superSizedMaterialCapacity + SuperSizedBoardWidth)),
                ApmwFuzzAxis.Numeric(
                    AxisJackCount,
                    0,
                    new ApmwFuzzNumericBand("standard-material", 0, standardMaterialCapacity, standardMaterialCapacity + StandardBoardWidth),
                    new ApmwFuzzNumericBand("super-sized-material", 0, superSizedMaterialCapacity, superSizedMaterialCapacity + SuperSizedBoardWidth)),
                ApmwFuzzAxis.Numeric(
                    AxisMajorToQueenCount,
                    1,
                    new ApmwFuzzNumericBand("standard-material", 1, standardMaterialCapacity, standardMaterialCapacity + StandardBoardWidth),
                    new ApmwFuzzNumericBand("super-sized-material", 1, superSizedMaterialCapacity, superSizedMaterialCapacity + SuperSizedBoardWidth)),
                ApmwFuzzAxis.Numeric(
                    AxisAmazonCount,
                    0,
                    new ApmwFuzzNumericBand("standard-material", 0, standardMaterialCapacity, standardMaterialCapacity + StandardBoardWidth),
                    new ApmwFuzzNumericBand("super-sized-material", 0, superSizedMaterialCapacity, superSizedMaterialCapacity + SuperSizedBoardWidth)),
                ApmwFuzzAxis.Numeric(
                    AxisPawnForwardnessCount,
                    0,
                    new ApmwFuzzNumericBand("standard-forwardness", 0, ForwardnessCapacity(StandardBoardWidth), standardPawnCapacity + 1),
                    new ApmwFuzzNumericBand("super-sized-forwardness", 0, ForwardnessCapacity(SuperSizedBoardWidth), superSizedPawnCapacity + 1)),
                ApmwFuzzAxis.Numeric(
                    AxisConsulCount,
                    0,
                    new ApmwFuzzNumericBand("consuls", 0, 2, 3)),
                ApmwFuzzAxis.Numeric(
                    AxisKingPromotionCount,
                    0,
                    new ApmwFuzzNumericBand("king-promotions", 0, 2, 3)),
                ApmwFuzzAxis.Numeric(
                    AxisSuperSizeMeCount,
                    0,
                    new ApmwFuzzNumericBand("super-size-items", 0, 1, 2)),
                ApmwFuzzAxis.Numeric(
                    AxisPlayAsWhiteCount,
                    1,
                    new ApmwFuzzNumericBand("play-as-white", 1, 1, 2)),
                ApmwFuzzAxis.Numeric(
                    AxisVictoryCount,
                    0,
                    new ApmwFuzzNumericBand("victory", 0, 1, 2)),
            };

            var constraints = new[]
            {
                new ApmwFuzzConstraint(
                    "standard-board-has-no-super-size-item",
                    "Standard APMW cases should not start with Super Size Me items.",
                    assignment => assignment.GetBool(AxisIsSuperSized) || assignment.GetInt(AxisSuperSizeMeCount) == 0),
                new ApmwFuzzConstraint(
                    "super-board-requests-super-size-item",
                    "Super-sized APMW cases should include at least one Super Size Me item.",
                    assignment => !assignment.GetBool(AxisIsSuperSized) || assignment.GetInt(AxisSuperSizeMeCount) >= 1),
                new ApmwFuzzConstraint(
                    "major-upgrades-not-above-major-count",
                    "Major-to-queen and amazon upgrades cannot exceed available major pieces for constrained generation.",
                    assignment =>
                        assignment.GetInt(AxisMajorToQueenCount) + assignment.GetInt(AxisAmazonCount) <=
                        assignment.GetInt(AxisMajorPieceCount)),
                new ApmwFuzzConstraint(
                    "pocket-items-fit-selected-limit",
                    "Pocket items should fit the selected per-pocket limit unless the limit is disabled.",
                    assignment =>
                    {
                        int pocketLimit = assignment.GetInt(AxisPocketLimitByPocket);
                        return pocketLimit <= 0 || assignment.GetInt(AxisPocketCount) <= pocketLimit * PocketSlots;
                    }),
            };

            return new ApmwFuzzOptionSpace(axes, constraints);
        }

        public ApmwFuzzAssignment DefaultAssignment()
        {
            return new ApmwFuzzAssignment(
                this,
                Axes.ToDictionary(axis => axis.Name, axis => axis.DefaultValue, StringComparer.Ordinal));
        }

        public string BuildCanonicalKey(ApmwFuzzCase fuzzCase)
        {
            if (fuzzCase == null)
                throw new ArgumentNullException(nameof(fuzzCase));

            return string.Join("|", Axes.Select(axis =>
                BuildValueCoverageKey(axis, BuildCanonicalCaseValue(axis, fuzzCase))));
        }

        public bool HasAxis(string axisName)
        {
            return Axes.Any(axis => string.Equals(axis.Name, axisName, StringComparison.Ordinal));
        }

        public ApmwFuzzAxis GetAxis(string axisName)
        {
            if (axisName == null)
                throw new ArgumentNullException(nameof(axisName));

            ApmwFuzzAxis axis = Axes.FirstOrDefault(
                optionAxis => string.Equals(optionAxis.Name, axisName, StringComparison.Ordinal));
            if (axis == null)
                throw new ArgumentException("Unknown option axis '" + axisName + "'.", nameof(axisName));

            return axis;
        }

        public IReadOnlyList<ApmwFuzzConstraintViolation> Validate(ApmwFuzzAssignment assignment)
        {
            if (assignment == null)
                throw new ArgumentNullException(nameof(assignment));
            if (!ReferenceEquals(this, assignment.OptionSpace))
                throw new ArgumentException("Assignments can only be validated by their owning option space.", nameof(assignment));

            return new ReadOnlyCollection<ApmwFuzzConstraintViolation>(
                Constraints
                    .Where(constraint => !constraint.IsSatisfiedBy(assignment))
                    .Select(constraint => new ApmwFuzzConstraintViolation(constraint))
                    .ToArray());
        }

        public ApmwFuzzCoverageTracker CreateCoverageTracker()
        {
            return new ApmwFuzzCoverageTracker(this);
        }

        internal static string BuildValueCoverageKey(ApmwFuzzAxis axis, ApmwFuzzOptionValue value)
        {
            if (axis == null)
                throw new ArgumentNullException(nameof(axis));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return axis.Name + "=" + value.CanonicalKey;
        }

        internal static string BuildPairCoverageKey(
            ApmwFuzzAxis firstAxis,
            ApmwFuzzOptionValue firstValue,
            ApmwFuzzAxis secondAxis,
            ApmwFuzzOptionValue secondValue)
        {
            return BuildValueCoverageKey(firstAxis, firstValue) + "|" + BuildValueCoverageKey(secondAxis, secondValue);
        }

        internal static ApmwFuzzOptionValue BuildCanonicalCaseValue(ApmwFuzzAxis axis, ApmwFuzzCase fuzzCase)
        {
            if (axis == null)
                throw new ArgumentNullException(nameof(axis));

            ApmwFuzzOptionValue value = BuildCaseValue(axis.Name, fuzzCase);
            return axis.ContainsValue(value)
                ? axis.GetValue(value.CanonicalKey)
                : value;
        }

        internal static ApmwFuzzOptionValue BuildCaseValue(string axisName, ApmwFuzzCase fuzzCase)
        {
            if (axisName == null)
                throw new ArgumentNullException(nameof(axisName));
            if (fuzzCase == null)
                throw new ArgumentNullException(nameof(fuzzCase));

            switch (axisName)
            {
                case AxisIsSuperSized:
                    return ApmwFuzzOptionValue.Boolean(fuzzCase.IsSuperSized);
                case AxisGoal:
                    return ApmwFuzzOptionValue.Enumeration(fuzzCase.Goal);
                case AxisEnemyPieceTypes:
                    return ApmwFuzzOptionValue.Enumeration(fuzzCase.EnemyPieceTypes);
                case AxisPieceLocations:
                    return ApmwFuzzOptionValue.Enumeration(fuzzCase.PieceLocations);
                case AxisPlayerPieceTypes:
                    return ApmwFuzzOptionValue.Enumeration(fuzzCase.PlayerPieceTypes);
                case AxisFairyChessArmy:
                    return ApmwFuzzOptionValue.Enumeration(fuzzCase.FairyChessArmy);
                case AxisArmy:
                    return ApmwFuzzOptionValue.IntSet("army", fuzzCase.ArmyIndexes);
                case AxisFairyChessPawns:
                    return ApmwFuzzOptionValue.Enumeration(fuzzCase.FairyChessPawns);
                case AxisFairyChessPawnUpgrades:
                    return ApmwFuzzOptionValue.Enumeration(fuzzCase.FairyChessPawnUpgrades);
                case AxisMinorPieceLimitByType:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.MinorPieceLimitByType);
                case AxisMajorPieceLimitByType:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.MajorPieceLimitByType);
                case AxisQueenPieceLimitByType:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.QueenPieceLimitByType);
                case AxisPocketLimitByPocket:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.PocketLimitByPocket);
                case AxisDeathLink:
                    return ApmwFuzzOptionValue.Boolean(fuzzCase.DeathLink);
                case AxisPocketCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.PocketCount);
                case AxisPocketRangeCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.PocketRangeCount);
                case AxisPocketGemCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.PocketGemCount);
                case AxisAIIntelligenceMalusCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.AIIntelligenceMalusCount);
                case AxisPawnCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.PawnCount);
                case AxisMinorPieceCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.MinorPieceCount);
                case AxisMajorPieceCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.MajorPieceCount);
                case AxisJackCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.JackCount);
                case AxisMajorToQueenCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.MajorToQueenCount);
                case AxisAmazonCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.AmazonCount);
                case AxisPawnForwardnessCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.PawnForwardnessCount);
                case AxisConsulCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.ConsulCount);
                case AxisKingPromotionCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.KingPromotionCount);
                case AxisSuperSizeMeCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.SuperSizeMeCount);
                case AxisPlayAsWhiteCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.PlayAsWhiteCount);
                case AxisVictoryCount:
                    return ApmwFuzzOptionValue.Numeric(fuzzCase.VictoryCount);
                default:
                    throw new ArgumentException("Unknown APMW fuzz case option axis '" + axisName + "'.", nameof(axisName));
            }
        }

        internal static void ApplyCaseValue(
            ApmwFuzzCase.Builder builder,
            string axisName,
            ApmwFuzzOptionValue value)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (axisName == null)
                throw new ArgumentNullException(nameof(axisName));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch (axisName)
            {
                case AxisIsSuperSized:
                    builder.IsSuperSized = ToBool(value);
                    break;
                case AxisGoal:
                    builder.Goal = (Goal)value.Value;
                    break;
                case AxisEnemyPieceTypes:
                    builder.EnemyPieceTypes = (PieceTypes)value.Value;
                    break;
                case AxisPieceLocations:
                    builder.PieceLocations = (PieceLocations)value.Value;
                    break;
                case AxisPlayerPieceTypes:
                    builder.PlayerPieceTypes = (PieceTypes)value.Value;
                    break;
                case AxisFairyChessArmy:
                    builder.FairyChessArmy = (FairyArmy)value.Value;
                    break;
                case AxisArmy:
                    builder.ArmyIndexes = ((IEnumerable<int>)value.Value).ToArray();
                    break;
                case AxisFairyChessPawns:
                    builder.FairyChessPawns = (FairyPawns)value.Value;
                    break;
                case AxisFairyChessPawnUpgrades:
                    builder.FairyChessPawnUpgrades = (FairyPawnUpgrades)value.Value;
                    break;
                case AxisMinorPieceLimitByType:
                    builder.MinorPieceLimitByType = ToInt(value);
                    break;
                case AxisMajorPieceLimitByType:
                    builder.MajorPieceLimitByType = ToInt(value);
                    break;
                case AxisQueenPieceLimitByType:
                    builder.QueenPieceLimitByType = ToInt(value);
                    break;
                case AxisPocketLimitByPocket:
                    builder.PocketLimitByPocket = ToInt(value);
                    break;
                case AxisDeathLink:
                    builder.DeathLink = ToBool(value);
                    break;
                case AxisPocketCount:
                    builder.PocketCount = ToInt(value);
                    break;
                case AxisPocketRangeCount:
                    builder.PocketRangeCount = ToInt(value);
                    break;
                case AxisPocketGemCount:
                    builder.PocketGemCount = ToInt(value);
                    break;
                case AxisAIIntelligenceMalusCount:
                    builder.AIIntelligenceMalusCount = ToInt(value);
                    break;
                case AxisPawnCount:
                    builder.PawnCount = ToInt(value);
                    break;
                case AxisMinorPieceCount:
                    builder.MinorPieceCount = ToInt(value);
                    break;
                case AxisMajorPieceCount:
                    builder.MajorPieceCount = ToInt(value);
                    break;
                case AxisJackCount:
                    builder.JackCount = ToInt(value);
                    break;
                case AxisMajorToQueenCount:
                    builder.MajorToQueenCount = ToInt(value);
                    break;
                case AxisAmazonCount:
                    builder.AmazonCount = ToInt(value);
                    break;
                case AxisPawnForwardnessCount:
                    builder.PawnForwardnessCount = ToInt(value);
                    break;
                case AxisConsulCount:
                    builder.ConsulCount = ToInt(value);
                    break;
                case AxisKingPromotionCount:
                    builder.KingPromotionCount = ToInt(value);
                    break;
                case AxisSuperSizeMeCount:
                    builder.SuperSizeMeCount = ToInt(value);
                    break;
                case AxisPlayAsWhiteCount:
                    builder.PlayAsWhiteCount = ToInt(value);
                    break;
                case AxisVictoryCount:
                    builder.VictoryCount = ToInt(value);
                    break;
                default:
                    throw new ArgumentException("Unknown APMW fuzz case option axis '" + axisName + "'.", nameof(axisName));
            }
        }

        private static IEnumerable<ApmwFuzzAxis> ValidateAxes(IEnumerable<ApmwFuzzAxis> axes)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (ApmwFuzzAxis axis in axes)
            {
                if (axis == null)
                    throw new ArgumentException("Option spaces cannot contain null axes.", nameof(axes));
                if (!seen.Add(axis.Name))
                    throw new ArgumentException("Duplicate option axis '" + axis.Name + "'.", nameof(axes));

                yield return axis;
            }
        }

        private static ApmwFuzzNumericBand BoardWidthBand(string name, int defaultValue, int boardWidth)
        {
            return new ApmwFuzzNumericBand(name, defaultValue, boardWidth, boardWidth + 1);
        }

        private static int MaterialCapacity(int boardWidth)
        {
            return boardWidth * 2 - 1;
        }

        private static int PawnCapacity(int boardWidth)
        {
            return boardWidth * BoardRanks / 2;
        }

        private static int ForwardnessCapacity(int boardWidth)
        {
            return boardWidth * 3;
        }

        private static int ToInt(ApmwFuzzOptionValue value)
        {
            return Convert.ToInt32(value.Value, CultureInfo.InvariantCulture);
        }

        private static bool ToBool(ApmwFuzzOptionValue value)
        {
            return Convert.ToBoolean(value.Value, CultureInfo.InvariantCulture);
        }
    }

    internal sealed class ApmwFuzzCoverageTracker
    {
        private readonly ApmwFuzzOptionSpace optionSpace;
        private readonly Dictionary<string, int> valueHitCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> pairHitCounts = new Dictionary<string, int>(StringComparer.Ordinal);

        public ApmwFuzzCoverageTracker(ApmwFuzzOptionSpace optionSpace)
        {
            this.optionSpace = optionSpace ?? throw new ArgumentNullException(nameof(optionSpace));
        }

        public int CaseCount { get; private set; }

        public void Record(ApmwFuzzAssignment assignment)
        {
            if (assignment == null)
                throw new ArgumentNullException(nameof(assignment));
            if (!ReferenceEquals(optionSpace, assignment.OptionSpace))
                throw new ArgumentException("Coverage can only record assignments from its option space.", nameof(assignment));

            CaseCount++;

            var selectedValues = new List<Tuple<ApmwFuzzAxis, ApmwFuzzOptionValue>>();
            foreach (ApmwFuzzAxis axis in optionSpace.Axes)
            {
                ApmwFuzzOptionValue value = assignment.Get(axis.Name);
                Increment(valueHitCounts, ApmwFuzzOptionSpace.BuildValueCoverageKey(axis, value));
                selectedValues.Add(Tuple.Create(axis, value));
            }

            for (int i = 0; i < selectedValues.Count; i++)
            {
                for (int j = i + 1; j < selectedValues.Count; j++)
                {
                    Increment(
                        pairHitCounts,
                        ApmwFuzzOptionSpace.BuildPairCoverageKey(
                            selectedValues[i].Item1,
                            selectedValues[i].Item2,
                            selectedValues[j].Item1,
                            selectedValues[j].Item2));
                }
            }
        }

        public ApmwFuzzCoverageSnapshot Snapshot()
        {
            return new ApmwFuzzCoverageSnapshot(
                CaseCount,
                ToSortedReadOnlyDictionary(valueHitCounts),
                ToSortedReadOnlyDictionary(pairHitCounts),
                BuildUncoveredValueKeys());
        }

        private IReadOnlyList<string> BuildUncoveredValueKeys()
        {
            var uncovered = new List<string>();

            foreach (ApmwFuzzAxis axis in optionSpace.Axes)
            {
                foreach (ApmwFuzzOptionValue value in axis.Values)
                {
                    string key = ApmwFuzzOptionSpace.BuildValueCoverageKey(axis, value);
                    if (!valueHitCounts.ContainsKey(key))
                        uncovered.Add(key);
                }
            }

            return new ReadOnlyCollection<string>(uncovered);
        }

        private static void Increment(Dictionary<string, int> counts, string key)
        {
            int count;
            counts.TryGetValue(key, out count);
            counts[key] = count + 1;
        }

        private static IReadOnlyDictionary<string, int> ToSortedReadOnlyDictionary(Dictionary<string, int> counts)
        {
            var sorted = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (var pair in counts)
                sorted.Add(pair.Key, pair.Value);

            return new ReadOnlyDictionary<string, int>(sorted);
        }
    }

    internal sealed class ApmwFuzzCoverageSnapshot
    {
        public ApmwFuzzCoverageSnapshot(
            int caseCount,
            IReadOnlyDictionary<string, int> valueHitCounts,
            IReadOnlyDictionary<string, int> pairHitCounts,
            IReadOnlyList<string> uncoveredValueKeys)
        {
            CaseCount = caseCount;
            ValueHitCounts = valueHitCounts ?? throw new ArgumentNullException(nameof(valueHitCounts));
            PairHitCounts = pairHitCounts ?? throw new ArgumentNullException(nameof(pairHitCounts));
            UncoveredValueKeys = uncoveredValueKeys ?? throw new ArgumentNullException(nameof(uncoveredValueKeys));
        }

        public int CaseCount { get; }

        public IReadOnlyDictionary<string, int> ValueHitCounts { get; }

        public IReadOnlyDictionary<string, int> PairHitCounts { get; }

        public IReadOnlyList<string> UncoveredValueKeys { get; }
    }
}
