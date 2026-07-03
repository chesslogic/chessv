using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Archipelago.APChessV;

namespace ChessV.Test
{
    internal sealed class ApmwGeneratedFuzzCase
    {
        public ApmwGeneratedFuzzCase(
            ApmwFuzzAssignment assignment,
            ApmwFuzzCase fuzzCase,
            string canonicalKey)
        {
            Assignment = assignment ?? throw new ArgumentNullException(nameof(assignment));
            FuzzCase = fuzzCase ?? throw new ArgumentNullException(nameof(fuzzCase));
            CanonicalKey = canonicalKey ?? throw new ArgumentNullException(nameof(canonicalKey));
        }

        public ApmwFuzzAssignment Assignment { get; }

        public ApmwFuzzCase FuzzCase { get; }

        public string CanonicalKey { get; }
    }

    internal static class ApmwPairwiseCaseGenerator
    {
        private const string PairwiseMasterSeed = "periphery-pairwise";
        private const string InteractionMasterSeed = "periphery-interactions";
        private const string PairwiseCategory = "generation-pairwise";
        private const string InteractionCategory = "generation-periphery";
        private const int StandardBoardWidth = 8;
        private const int SuperSizedBoardWidth = 10;

        private static readonly string[] PairwiseCategoricalAxes =
        {
            ApmwFuzzOptionSpace.AxisIsSuperSized,
            ApmwFuzzOptionSpace.AxisGoal,
            ApmwFuzzOptionSpace.AxisPieceLocations,
            ApmwFuzzOptionSpace.AxisPlayerPieceTypes,
            ApmwFuzzOptionSpace.AxisEnemyPieceTypes,
            ApmwFuzzOptionSpace.AxisFairyChessArmy,
            ApmwFuzzOptionSpace.AxisFairyChessPawns,
            ApmwFuzzOptionSpace.AxisFairyChessPawnUpgrades,
            ApmwFuzzOptionSpace.AxisDeathLink,
        };

        public static IReadOnlyList<string> CategoricalAxisNames
        {
            get { return new ReadOnlyCollection<string>(PairwiseCategoricalAxes); }
        }

        public static IReadOnlyList<ApmwFuzzCase> PairwiseCases()
        {
            return new ReadOnlyCollection<ApmwFuzzCase>(
                PairwiseGeneratedCases().Select(generatedCase => generatedCase.FuzzCase).ToList());
        }

        public static IReadOnlyList<ApmwFuzzCase> PeripheryCases()
        {
            return new ReadOnlyCollection<ApmwFuzzCase>(
                PeripheryGeneratedCases().Select(generatedCase => generatedCase.FuzzCase).ToList());
        }

        public static string CanonicalKey(ApmwFuzzCase fuzzCase)
        {
            if (fuzzCase == null)
                throw new ArgumentNullException(nameof(fuzzCase));

            return string.Join("|", new[]
            {
                "target=" + fuzzCase.TargetStage,
                "is-super-sized=" + BooleanKey(fuzzCase.IsSuperSized),
                "goal=" + EnumKey(fuzzCase.Goal),
                "enemy_piece_types=" + EnumKey(fuzzCase.EnemyPieceTypes),
                "piece_locations=" + EnumKey(fuzzCase.PieceLocations),
                "piece_types=" + EnumKey(fuzzCase.PlayerPieceTypes),
                "fairy_chess_army=" + EnumKey(fuzzCase.FairyChessArmy),
                "army=" + IntSetKey(fuzzCase.ArmyIndexes),
                "fairy_chess_pawns=" + EnumKey(fuzzCase.FairyChessPawns),
                "fairy_chess_pawn_upgrades=" + EnumKey(fuzzCase.FairyChessPawnUpgrades),
                "piece_upgrade_preference_profile=" + EnumKey(fuzzCase.PieceUpgradePreferenceProfile),
                "minor_piece_limit_by_type=" + IntKey(fuzzCase.MinorPieceLimitByType),
                "major_piece_limit_by_type=" + IntKey(fuzzCase.MajorPieceLimitByType),
                "queen_piece_limit_by_type=" + IntKey(fuzzCase.QueenPieceLimitByType),
                "pocket_limit_by_pocket=" + IntKey(fuzzCase.PocketLimitByPocket),
                "death_link=" + BooleanKey(fuzzCase.DeathLink),
                "pocket_seed=" + IntKey(fuzzCase.PocketSeed),
                "pawn_seed=" + IntKey(fuzzCase.PawnSeed),
                "minor_seed=" + IntKey(fuzzCase.MinorSeed),
                "major_seed=" + IntKey(fuzzCase.MajorSeed),
                "queen_seed=" + IntKey(fuzzCase.QueenSeed),
                "deterministic_chaos_seed=" + NullableIntKey(fuzzCase.DeterministicChaosSeed),
                "pocket-count=" + IntKey(fuzzCase.PocketCount),
                "pocket-range-count=" + IntKey(fuzzCase.PocketRangeCount),
                "pocket-gem-count=" + IntKey(fuzzCase.PocketGemCount),
                "ai-intelligence-malus-count=" + IntKey(fuzzCase.AIIntelligenceMalusCount),
                "pawn-count=" + IntKey(fuzzCase.PawnCount),
                "minor-piece-count=" + IntKey(fuzzCase.MinorPieceCount),
                "major-piece-count=" + IntKey(fuzzCase.MajorPieceCount),
                "jack-count=" + IntKey(fuzzCase.JackCount),
                "major-to-queen-count=" + IntKey(fuzzCase.MajorToQueenCount),
                "amazon-count=" + IntKey(fuzzCase.AmazonCount),
                "pawn-forwardness-count=" + IntKey(fuzzCase.PawnForwardnessCount),
                "consul-count=" + IntKey(fuzzCase.ConsulCount),
                "king-promotion-count=" + IntKey(fuzzCase.KingPromotionCount),
                "super-size-me-count=" + IntKey(fuzzCase.SuperSizeMeCount),
                "play-as-white-count=" + IntKey(fuzzCase.PlayAsWhiteCount),
                "victory-count=" + IntKey(fuzzCase.VictoryCount),
            });
        }

        internal static IReadOnlyList<ApmwGeneratedFuzzCase> PairwiseGeneratedCases()
        {
            ApmwFuzzOptionSpace optionSpace = ApmwFuzzOptionSpace.CreateDefault();
            IReadOnlyList<ApmwFuzzAxis> axes = PairwiseCategoricalAxes
                .Select(optionSpace.GetAxis)
                .ToArray();

            var generatedCases = new List<ApmwGeneratedFuzzCase>();
            IReadOnlyList<ApmwFuzzAssignment> assignments = BuildPairwiseAssignments(optionSpace, axes);
            for (int index = 0; index < assignments.Count; index++)
            {
                ApmwFuzzAssignment assignment = ApplyBoardDefaults(assignments[index]);
                AssertValid(optionSpace, assignment, "pairwise case " + index);
                generatedCases.Add(CreateGeneratedCase(
                    optionSpace,
                    "pairwise-" + index.ToString("D4", CultureInfo.InvariantCulture) + "-" + BoardLabel(assignment),
                    PairwiseMasterSeed,
                    index,
                    PairwiseCategory,
                    assignment,
                    true,
                    null));
            }

            return new ReadOnlyCollection<ApmwGeneratedFuzzCase>(generatedCases);
        }

        internal static IReadOnlyList<ApmwGeneratedFuzzCase> PeripheryGeneratedCases()
        {
            return new ReadOnlyCollection<ApmwGeneratedFuzzCase>(
                Deduplicate(PairwiseGeneratedCases().Concat(InteractionGeneratedCases())).ToList());
        }

        internal static string CategoricalValueKey(ApmwFuzzCase fuzzCase, string axisName)
        {
            if (fuzzCase == null)
                throw new ArgumentNullException(nameof(fuzzCase));
            if (axisName == null)
                throw new ArgumentNullException(nameof(axisName));

            switch (axisName)
            {
                case ApmwFuzzOptionSpace.AxisIsSuperSized:
                    return BooleanKey(fuzzCase.IsSuperSized);
                case ApmwFuzzOptionSpace.AxisGoal:
                    return EnumKey(fuzzCase.Goal);
                case ApmwFuzzOptionSpace.AxisPieceLocations:
                    return EnumKey(fuzzCase.PieceLocations);
                case ApmwFuzzOptionSpace.AxisPlayerPieceTypes:
                    return EnumKey(fuzzCase.PlayerPieceTypes);
                case ApmwFuzzOptionSpace.AxisEnemyPieceTypes:
                    return EnumKey(fuzzCase.EnemyPieceTypes);
                case ApmwFuzzOptionSpace.AxisFairyChessArmy:
                    return EnumKey(fuzzCase.FairyChessArmy);
                case ApmwFuzzOptionSpace.AxisFairyChessPawns:
                    return EnumKey(fuzzCase.FairyChessPawns);
                case ApmwFuzzOptionSpace.AxisFairyChessPawnUpgrades:
                    return EnumKey(fuzzCase.FairyChessPawnUpgrades);
                case ApmwFuzzOptionSpace.AxisDeathLink:
                    return BooleanKey(fuzzCase.DeathLink);
                default:
                    throw new ArgumentException("Unsupported categorical axis '" + axisName + "'.", nameof(axisName));
            }
        }

        private static IReadOnlyList<ApmwGeneratedFuzzCase> InteractionGeneratedCases()
        {
            ApmwFuzzOptionSpace optionSpace = ApmwFuzzOptionSpace.CreateDefault();
            var definitions = new List<InteractionDefinition>();

            AddPocketInteractions(optionSpace, definitions);
            AddPawnInteractions(optionSpace, definitions);
            AddMajorInteractions(optionSpace, definitions);
            AddArmyAndTypeLimitInteractions(optionSpace, definitions);
            AddChaosSeedInteractions(optionSpace, definitions);
            AddOverCapInteractions(optionSpace, definitions);

            var generatedCases = new List<ApmwGeneratedFuzzCase>();
            for (int index = 0; index < definitions.Count; index++)
            {
                InteractionDefinition definition = definitions[index];
                generatedCases.Add(CreateGeneratedCase(
                    optionSpace,
                    definition.CaseName,
                    InteractionMasterSeed,
                    index,
                    InteractionCategory + "-" + definition.Family,
                    definition.Assignment,
                    definition.RequireValid,
                    definition.ChaosSeedOverride));
            }

            return new ReadOnlyCollection<ApmwGeneratedFuzzCase>(generatedCases);
        }

        private static IReadOnlyList<ApmwFuzzAssignment> BuildPairwiseAssignments(
            ApmwFuzzOptionSpace optionSpace,
            IReadOnlyList<ApmwFuzzAxis> axes)
        {
            if (axes.Count == 0)
                return new ReadOnlyCollection<ApmwFuzzAssignment>(new List<ApmwFuzzAssignment>());
            if (axes.Count == 1)
            {
                return new ReadOnlyCollection<ApmwFuzzAssignment>(
                    axes[0].Values.Select(value => optionSpace.DefaultAssignment().WithValue(axes[0].Name, value)).ToList());
            }

            var rows = new List<Dictionary<string, ApmwFuzzOptionValue>>();
            foreach (ApmwFuzzOptionValue firstValue in axes[0].Values)
            {
                foreach (ApmwFuzzOptionValue secondValue in axes[1].Values)
                {
                    rows.Add(new Dictionary<string, ApmwFuzzOptionValue>(StringComparer.Ordinal)
                    {
                        [axes[0].Name] = firstValue,
                        [axes[1].Name] = secondValue,
                    });
                }
            }

            for (int axisIndex = 2; axisIndex < axes.Count; axisIndex++)
                ExtendRows(rows, axes, axisIndex);

            return new ReadOnlyCollection<ApmwFuzzAssignment>(
                rows.Select(row => ToAssignment(optionSpace, row)).ToList());
        }

        private static void ExtendRows(
            List<Dictionary<string, ApmwFuzzOptionValue>> rows,
            IReadOnlyList<ApmwFuzzAxis> axes,
            int axisIndex)
        {
            ApmwFuzzAxis newAxis = axes[axisIndex];
            IReadOnlyList<ApmwFuzzAxis> previousAxes = axes.Take(axisIndex).ToArray();
            List<PairRequirement> requirements = BuildPairRequirements(previousAxes, newAxis);
            var uncovered = new HashSet<string>(requirements.Select(requirement => requirement.Key), StringComparer.Ordinal);

            foreach (Dictionary<string, ApmwFuzzOptionValue> row in rows)
            {
                ApmwFuzzOptionValue bestValue = ChooseBestValue(row, previousAxes, newAxis, uncovered);
                row[newAxis.Name] = bestValue;
                CoverPairs(row, previousAxes, newAxis, bestValue, uncovered);
            }

            foreach (PairRequirement requirement in requirements)
            {
                if (!uncovered.Contains(requirement.Key))
                    continue;

                var row = BuildRowForRequirement(axes, axisIndex, newAxis, requirement, uncovered);
                rows.Add(row);
                CoverPairs(row, previousAxes, newAxis, row[newAxis.Name], uncovered);
            }
        }

        private static List<PairRequirement> BuildPairRequirements(
            IEnumerable<ApmwFuzzAxis> previousAxes,
            ApmwFuzzAxis newAxis)
        {
            var requirements = new List<PairRequirement>();
            foreach (ApmwFuzzAxis previousAxis in previousAxes)
            {
                foreach (ApmwFuzzOptionValue previousValue in previousAxis.Values)
                {
                    foreach (ApmwFuzzOptionValue newValue in newAxis.Values)
                    {
                        requirements.Add(new PairRequirement(
                            previousAxis,
                            previousValue,
                            newValue,
                            PairKey(previousAxis, previousValue, newAxis, newValue)));
                    }
                }
            }

            return requirements;
        }

        private static ApmwFuzzOptionValue ChooseBestValue(
            IReadOnlyDictionary<string, ApmwFuzzOptionValue> row,
            IEnumerable<ApmwFuzzAxis> previousAxes,
            ApmwFuzzAxis newAxis,
            ISet<string> uncovered)
        {
            ApmwFuzzOptionValue bestValue = null;
            int bestScore = -1;

            foreach (ApmwFuzzOptionValue candidate in newAxis.Values)
            {
                int score = CountPairs(row, previousAxes, newAxis, candidate, uncovered);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestValue = candidate;
                }
            }

            return bestValue ?? newAxis.DefaultValue;
        }

        private static Dictionary<string, ApmwFuzzOptionValue> BuildRowForRequirement(
            IReadOnlyList<ApmwFuzzAxis> axes,
            int axisIndex,
            ApmwFuzzAxis newAxis,
            PairRequirement requirement,
            ISet<string> uncovered)
        {
            var row = new Dictionary<string, ApmwFuzzOptionValue>(StringComparer.Ordinal);
            for (int index = 0; index <= axisIndex; index++)
                row[axes[index].Name] = axes[index].DefaultValue;

            row[requirement.PreviousAxis.Name] = requirement.PreviousValue;
            row[newAxis.Name] = requirement.NewValue;

            for (int index = 0; index < axisIndex; index++)
            {
                ApmwFuzzAxis axis = axes[index];
                if (axis.Name == requirement.PreviousAxis.Name)
                    continue;

                row[axis.Name] = ChooseBestPreviousAxisValue(axis, newAxis, requirement.NewValue, uncovered);
            }

            return row;
        }

        private static ApmwFuzzOptionValue ChooseBestPreviousAxisValue(
            ApmwFuzzAxis axis,
            ApmwFuzzAxis newAxis,
            ApmwFuzzOptionValue newValue,
            ISet<string> uncovered)
        {
            ApmwFuzzOptionValue bestValue = axis.DefaultValue;
            int bestScore = -1;

            foreach (ApmwFuzzOptionValue candidate in axis.Values)
            {
                int score = uncovered.Contains(PairKey(axis, candidate, newAxis, newValue)) ? 1 : 0;
                if (score > bestScore || (score == bestScore && candidate.CanonicalKey == axis.DefaultValue.CanonicalKey))
                {
                    bestScore = score;
                    bestValue = candidate;
                }
            }

            return bestValue;
        }

        private static int CountPairs(
            IReadOnlyDictionary<string, ApmwFuzzOptionValue> row,
            IEnumerable<ApmwFuzzAxis> previousAxes,
            ApmwFuzzAxis newAxis,
            ApmwFuzzOptionValue newValue,
            ISet<string> uncovered)
        {
            int score = 0;
            foreach (ApmwFuzzAxis previousAxis in previousAxes)
            {
                if (uncovered.Contains(PairKey(previousAxis, row[previousAxis.Name], newAxis, newValue)))
                    score++;
            }

            return score;
        }

        private static void CoverPairs(
            IReadOnlyDictionary<string, ApmwFuzzOptionValue> row,
            IEnumerable<ApmwFuzzAxis> previousAxes,
            ApmwFuzzAxis newAxis,
            ApmwFuzzOptionValue newValue,
            ISet<string> uncovered)
        {
            foreach (ApmwFuzzAxis previousAxis in previousAxes)
                uncovered.Remove(PairKey(previousAxis, row[previousAxis.Name], newAxis, newValue));
        }

        private static ApmwFuzzAssignment ToAssignment(
            ApmwFuzzOptionSpace optionSpace,
            IReadOnlyDictionary<string, ApmwFuzzOptionValue> row)
        {
            ApmwFuzzAssignment assignment = optionSpace.DefaultAssignment();
            foreach (ApmwFuzzAxis axis in optionSpace.Axes)
            {
                ApmwFuzzOptionValue value;
                if (row.TryGetValue(axis.Name, out value))
                    assignment = assignment.WithValue(axis.Name, value);
            }

            return assignment;
        }

        private static string PairKey(
            ApmwFuzzAxis firstAxis,
            ApmwFuzzOptionValue firstValue,
            ApmwFuzzAxis secondAxis,
            ApmwFuzzOptionValue secondValue)
        {
            return ApmwFuzzOptionSpace.BuildPairCoverageKey(firstAxis, firstValue, secondAxis, secondValue);
        }

        private static IEnumerable<ApmwGeneratedFuzzCase> Deduplicate(IEnumerable<ApmwGeneratedFuzzCase> generatedCases)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (ApmwGeneratedFuzzCase generatedCase in generatedCases)
            {
                if (seen.Add(generatedCase.CanonicalKey))
                    yield return generatedCase;
            }
        }

        private static void AddPocketInteractions(
            ApmwFuzzOptionSpace optionSpace,
            List<InteractionDefinition> definitions)
        {
            definitions.Add(Define("pockets", "interaction-pockets-standard-empty-limit-disabled",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisPocketLimitByPocket, "0")
                    .With(ApmwFuzzOptionSpace.AxisPocketCount, "0")
                    .With(ApmwFuzzOptionSpace.AxisPocketRangeCount, "0")
                    .With(ApmwFuzzOptionSpace.AxisPocketGemCount, "0"),
                true));
            definitions.Add(Define("pockets", "interaction-pockets-standard-default-limit-max-fill",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisPocketLimitByPocket, "4")
                    .With(ApmwFuzzOptionSpace.AxisPocketCount, "12")
                    .With(ApmwFuzzOptionSpace.AxisPocketRangeCount, "6")
                    .With(ApmwFuzzOptionSpace.AxisPocketGemCount, "6"),
                true));
            definitions.Add(Define("pockets", "interaction-pockets-super-over-limit-range-gems",
                BaseAssignment(optionSpace, true)
                    .With(ApmwFuzzOptionSpace.AxisPocketLimitByPocket, "4")
                    .With(ApmwFuzzOptionSpace.AxisPocketCount, "24")
                    .With(ApmwFuzzOptionSpace.AxisPocketRangeCount, "9")
                    .With(ApmwFuzzOptionSpace.AxisPocketGemCount, "9"),
                false));
        }

        private static void AddPawnInteractions(
            ApmwFuzzOptionSpace optionSpace,
            List<InteractionDefinition> definitions)
        {
            definitions.Add(Define("pawns", "interaction-pawns-standard-none-upgrade-pool",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisFairyChessPawns, "vanilla")
                    .With(ApmwFuzzOptionSpace.AxisFairyChessPawnUpgrades, "pool")
                    .With(ApmwFuzzOptionSpace.AxisPawnCount, "0")
                    .With(ApmwFuzzOptionSpace.AxisPawnForwardnessCount, "0"),
                true));
            definitions.Add(Define("pawns", "interaction-pawns-standard-cap-forwardness-upgrades",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisFairyChessPawns, "anyfairy")
                    .With(ApmwFuzzOptionSpace.AxisFairyChessPawnUpgrades, "max")
                    .With(ApmwFuzzOptionSpace.AxisPawnCount, "32")
                    .With(ApmwFuzzOptionSpace.AxisPawnForwardnessCount, "24"),
                true));
            definitions.Add(Define("pawns", "interaction-pawns-super-over-forwardness-any-classical",
                BaseAssignment(optionSpace, true)
                    .With(ApmwFuzzOptionSpace.AxisFairyChessPawns, "anyclassical")
                    .With(ApmwFuzzOptionSpace.AxisFairyChessPawnUpgrades, "max")
                    .With(ApmwFuzzOptionSpace.AxisPawnCount, "41")
                    .With(ApmwFuzzOptionSpace.AxisPawnForwardnessCount, "41"),
                true));
        }

        private static void AddMajorInteractions(
            ApmwFuzzOptionSpace optionSpace,
            List<InteractionDefinition> definitions)
        {
            definitions.Add(Define("majors", "interaction-majors-standard-queen-conversion-cap",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisMinorPieceCount, "0")
                    .With(ApmwFuzzOptionSpace.AxisMajorPieceCount, "15")
                    .With(ApmwFuzzOptionSpace.AxisMajorToQueenCount, "15")
                    .With(ApmwFuzzOptionSpace.AxisAmazonCount, "0")
                    .With(ApmwFuzzOptionSpace.AxisJackCount, "0")
                    .With(ApmwFuzzOptionSpace.AxisConsulCount, "2"),
                true));
            definitions.Add(Define("majors", "interaction-majors-super-jacks-consuls",
                BaseAssignment(optionSpace, true)
                    .With(ApmwFuzzOptionSpace.AxisMinorPieceCount, "0")
                    .With(ApmwFuzzOptionSpace.AxisMajorPieceCount, "19")
                    .With(ApmwFuzzOptionSpace.AxisMajorToQueenCount, "18")
                    .With(ApmwFuzzOptionSpace.AxisAmazonCount, "1")
                    .With(ApmwFuzzOptionSpace.AxisJackCount, "19")
                    .With(ApmwFuzzOptionSpace.AxisConsulCount, "2")
                    .With(ApmwFuzzOptionSpace.AxisKingPromotionCount, "2"),
                true));
            definitions.Add(Define("majors", "interaction-majors-standard-over-cap-mixed",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisMinorPieceCount, "16")
                    .With(ApmwFuzzOptionSpace.AxisMajorPieceCount, "16")
                    .With(ApmwFuzzOptionSpace.AxisMajorToQueenCount, "15")
                    .With(ApmwFuzzOptionSpace.AxisAmazonCount, "1")
                    .With(ApmwFuzzOptionSpace.AxisJackCount, "16")
                    .With(ApmwFuzzOptionSpace.AxisConsulCount, "3")
                    .With(ApmwFuzzOptionSpace.AxisKingPromotionCount, "3"),
                true));
            definitions.Add(Define("majors", "interaction-majors-standard-list-minor-to-jack-chain",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisPieceUpgradePreferenceProfile, "listminortojackfirst")
                    .With(ApmwFuzzOptionSpace.AxisMinorPieceCount, "4")
                    .With(ApmwFuzzOptionSpace.AxisMajorPieceCount, "1")
                    .With(ApmwFuzzOptionSpace.AxisJackCount, "1")
                    .With(ApmwFuzzOptionSpace.AxisMajorToQueenCount, "1")
                    .With(ApmwFuzzOptionSpace.AxisAmazonCount, "1"),
                true));
            definitions.Add(Define("majors", "interaction-majors-standard-priority-map-disable-major-to-queen",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisPieceUpgradePreferenceProfile, "prioritymapdisablemajortoqueen")
                    .With(ApmwFuzzOptionSpace.AxisMinorPieceCount, "4")
                    .With(ApmwFuzzOptionSpace.AxisMajorPieceCount, "2")
                    .With(ApmwFuzzOptionSpace.AxisJackCount, "1")
                    .With(ApmwFuzzOptionSpace.AxisMajorToQueenCount, "1")
                    .With(ApmwFuzzOptionSpace.AxisAmazonCount, "1"),
                true));
        }

        private static void AddArmyAndTypeLimitInteractions(
            ApmwFuzzOptionSpace optionSpace,
            List<InteractionDefinition> definitions)
        {
            definitions.Add(Define("army-limits", "interaction-army-limited-empty-type-limits-one",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisFairyChessArmy, "limited")
                    .With(ApmwFuzzOptionSpace.AxisArmy, "empty")
                    .With(ApmwFuzzOptionSpace.AxisMinorPieceLimitByType, "1")
                    .With(ApmwFuzzOptionSpace.AxisMajorPieceLimitByType, "1")
                    .With(ApmwFuzzOptionSpace.AxisQueenPieceLimitByType, "1"),
                true));
            definitions.Add(Define("army-limits", "interaction-army-stable-scattered-standard-width-limits",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisFairyChessArmy, "stable")
                    .With(ApmwFuzzOptionSpace.AxisArmy, "0,3,5,6")
                    .With(ApmwFuzzOptionSpace.AxisMinorPieceLimitByType, "8")
                    .With(ApmwFuzzOptionSpace.AxisMajorPieceLimitByType, "8")
                    .With(ApmwFuzzOptionSpace.AxisQueenPieceLimitByType, "8"),
                true));
            definitions.Add(Define("army-limits", "interaction-army-chaos-opening-super-over-width-limits",
                BaseAssignment(optionSpace, true)
                    .With(ApmwFuzzOptionSpace.AxisFairyChessArmy, "chaos")
                    .With(ApmwFuzzOptionSpace.AxisArmy, "0,1,2")
                    .With(ApmwFuzzOptionSpace.AxisMinorPieceLimitByType, "11")
                    .With(ApmwFuzzOptionSpace.AxisMajorPieceLimitByType, "11")
                    .With(ApmwFuzzOptionSpace.AxisQueenPieceLimitByType, "11"),
                true));
        }

        private static void AddChaosSeedInteractions(
            ApmwFuzzOptionSpace optionSpace,
            List<InteractionDefinition> definitions)
        {
            definitions.Add(Define("chaos-seeds", "interaction-chaos-types-seeded-zero",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisPlayerPieceTypes, "chaos")
                    .With(ApmwFuzzOptionSpace.AxisPieceLocations, "stable")
                    .With(ApmwFuzzOptionSpace.AxisEnemyPieceTypes, "chaos"),
                true,
                0));
            definitions.Add(Define("chaos-seeds", "interaction-chaos-locations-seeded-max",
                BaseAssignment(optionSpace, true)
                    .With(ApmwFuzzOptionSpace.AxisPlayerPieceTypes, "stable")
                    .With(ApmwFuzzOptionSpace.AxisPieceLocations, "chaos")
                    .With(ApmwFuzzOptionSpace.AxisFairyChessArmy, "limited")
                    .With(ApmwFuzzOptionSpace.AxisArmy, "0,1,2")
                    .With(ApmwFuzzOptionSpace.AxisDeathLink, "true"),
                true,
                int.MaxValue));
            definitions.Add(Define("chaos-seeds", "interaction-chaos-types-and-locations-seeded-fixed",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisPlayerPieceTypes, "chaos")
                    .With(ApmwFuzzOptionSpace.AxisPieceLocations, "chaos")
                    .With(ApmwFuzzOptionSpace.AxisFairyChessPawns, "reserved"),
                true,
                8675309));
        }

        private static void AddOverCapInteractions(
            ApmwFuzzOptionSpace optionSpace,
            List<InteractionDefinition> definitions)
        {
            definitions.Add(Define("over-cap", "interaction-over-cap-standard-pawns-forwardness",
                BaseAssignment(optionSpace, false)
                    .With(ApmwFuzzOptionSpace.AxisPawnCount, "40")
                    .With(ApmwFuzzOptionSpace.AxisPawnForwardnessCount, "33")
                    .With(ApmwFuzzOptionSpace.AxisFairyChessPawnUpgrades, "pool"),
                true));
            definitions.Add(Define("over-cap", "interaction-over-cap-super-material-and-pockets",
                BaseAssignment(optionSpace, true)
                    .With(ApmwFuzzOptionSpace.AxisMinorPieceCount, "29")
                    .With(ApmwFuzzOptionSpace.AxisMajorPieceCount, "29")
                    .With(ApmwFuzzOptionSpace.AxisMajorToQueenCount, "29")
                    .With(ApmwFuzzOptionSpace.AxisAmazonCount, "29")
                    .With(ApmwFuzzOptionSpace.AxisJackCount, "29")
                    .With(ApmwFuzzOptionSpace.AxisPocketLimitByPocket, "4")
                    .With(ApmwFuzzOptionSpace.AxisPocketCount, "24"),
                false));
        }

        private static InteractionDefinition Define(
            string family,
            string caseName,
            ApmwFuzzAssignment assignment,
            bool requireValid)
        {
            return Define(family, caseName, assignment, requireValid, null);
        }

        private static InteractionDefinition Define(
            string family,
            string caseName,
            ApmwFuzzAssignment assignment,
            bool requireValid,
            int? chaosSeedOverride)
        {
            return new InteractionDefinition(family, caseName, assignment, requireValid, chaosSeedOverride);
        }

        private static ApmwFuzzAssignment BaseAssignment(ApmwFuzzOptionSpace optionSpace, bool isSuperSized)
        {
            return ApplyBoardDefaults(optionSpace.DefaultAssignment()
                .With(ApmwFuzzOptionSpace.AxisIsSuperSized, BooleanKey(isSuperSized)));
        }

        private static ApmwFuzzAssignment ApplyBoardDefaults(ApmwFuzzAssignment assignment)
        {
            bool isSuperSized = assignment.GetBool(ApmwFuzzOptionSpace.AxisIsSuperSized);
            assignment = assignment.With(ApmwFuzzOptionSpace.AxisSuperSizeMeCount, isSuperSized ? "1" : "0");

            if (assignment.GetInt(ApmwFuzzOptionSpace.AxisPawnCount) == StandardBoardWidth)
                assignment = assignment.With(
                    ApmwFuzzOptionSpace.AxisPawnCount,
                    IntKey(isSuperSized ? SuperSizedBoardWidth : StandardBoardWidth));

            return assignment;
        }

        private static ApmwGeneratedFuzzCase CreateGeneratedCase(
            ApmwFuzzOptionSpace optionSpace,
            string caseName,
            string masterSeed,
            int caseIndex,
            string category,
            ApmwFuzzAssignment assignment,
            bool requireValid,
            int? chaosSeedOverride)
        {
            if (requireValid)
                AssertValid(optionSpace, assignment, caseName);

            ApmwFuzzCase fuzzCase = BuildCase(caseName, masterSeed, caseIndex, category, assignment, chaosSeedOverride);
            return new ApmwGeneratedFuzzCase(assignment, fuzzCase, CanonicalKey(fuzzCase));
        }

        private static ApmwFuzzCase BuildCase(
            string caseName,
            string masterSeed,
            int caseIndex,
            string category,
            ApmwFuzzAssignment assignment,
            int? chaosSeedOverride)
        {
            bool isSuperSized = assignment.GetBool(ApmwFuzzOptionSpace.AxisIsSuperSized);
            ApmwFuzzCase baseCase = isSuperSized
                ? ApmwFuzzCase.DefaultSuperSized()
                : ApmwFuzzCase.DefaultStandard();

            return baseCase.With(builder =>
            {
                builder.CaseName = caseName;
                builder.CaseIndex = caseIndex;
                builder.MasterSeed = masterSeed;
                builder.TargetStage = ApmwFuzzCase.TargetStages.ItemHandlerGeneration;
                builder.Category = category + "-" + BoardLabel(assignment);
                builder.IsSuperSized = isSuperSized;
                builder.Goal = GetEnum<Goal>(assignment, ApmwFuzzOptionSpace.AxisGoal);
                builder.EnemyPieceTypes = GetEnum<PieceTypes>(assignment, ApmwFuzzOptionSpace.AxisEnemyPieceTypes);
                builder.PieceLocations = GetEnum<PieceLocations>(assignment, ApmwFuzzOptionSpace.AxisPieceLocations);
                builder.PlayerPieceTypes = GetEnum<PieceTypes>(assignment, ApmwFuzzOptionSpace.AxisPlayerPieceTypes);
                builder.FairyChessArmy = GetEnum<FairyArmy>(assignment, ApmwFuzzOptionSpace.AxisFairyChessArmy);
                builder.ArmyIndexes = assignment.GetIntSet(ApmwFuzzOptionSpace.AxisArmy).ToArray();
                builder.FairyChessPawns = GetEnum<FairyPawns>(assignment, ApmwFuzzOptionSpace.AxisFairyChessPawns);
                builder.FairyChessPawnUpgrades = GetEnum<FairyPawnUpgrades>(assignment, ApmwFuzzOptionSpace.AxisFairyChessPawnUpgrades);
                builder.PieceUpgradePreferenceProfile = GetEnum<ApmwPieceUpgradePreferenceProfile>(assignment, ApmwFuzzOptionSpace.AxisPieceUpgradePreferenceProfile);
                builder.MinorPieceLimitByType = assignment.GetInt(ApmwFuzzOptionSpace.AxisMinorPieceLimitByType);
                builder.MajorPieceLimitByType = assignment.GetInt(ApmwFuzzOptionSpace.AxisMajorPieceLimitByType);
                builder.QueenPieceLimitByType = assignment.GetInt(ApmwFuzzOptionSpace.AxisQueenPieceLimitByType);
                builder.PocketLimitByPocket = assignment.GetInt(ApmwFuzzOptionSpace.AxisPocketLimitByPocket);
                builder.DeathLink = assignment.GetBool(ApmwFuzzOptionSpace.AxisDeathLink);
                builder.PocketCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisPocketCount);
                builder.PocketRangeCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisPocketRangeCount);
                builder.PocketGemCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisPocketGemCount);
                builder.AIIntelligenceMalusCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisAIIntelligenceMalusCount);
                builder.PawnCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisPawnCount);
                builder.MinorPieceCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisMinorPieceCount);
                builder.MajorPieceCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisMajorPieceCount);
                builder.JackCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisJackCount);
                builder.MajorToQueenCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisMajorToQueenCount);
                builder.AmazonCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisAmazonCount);
                builder.PawnForwardnessCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisPawnForwardnessCount);
                builder.ConsulCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisConsulCount);
                builder.KingPromotionCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisKingPromotionCount);
                builder.SuperSizeMeCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisSuperSizeMeCount);
                builder.PlayAsWhiteCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisPlayAsWhiteCount);
                builder.VictoryCount = assignment.GetInt(ApmwFuzzOptionSpace.AxisVictoryCount);
                builder.DeterministicChaosSeed = NeedsChaosSeed(builder)
                    ? chaosSeedOverride ?? StableChaosSeed(caseName, caseIndex)
                    : null;
            });
        }

        private static void AssertValid(
            ApmwFuzzOptionSpace optionSpace,
            ApmwFuzzAssignment assignment,
            string caseName)
        {
            IReadOnlyList<ApmwFuzzConstraintViolation> violations = optionSpace.Validate(assignment);
            if (violations.Count != 0)
            {
                throw new InvalidOperationException(
                    "APMW fuzz option assignment for '" + caseName + "' violated constraints: " +
                    string.Join(", ", violations.Select(violation => violation.Name)));
            }
        }

        private static bool NeedsChaosSeed(ApmwFuzzCase.Builder builder)
        {
            return builder.PlayerPieceTypes == PieceTypes.Chaos ||
                builder.PieceLocations == PieceLocations.Chaos;
        }

        private static T GetEnum<T>(ApmwFuzzAssignment assignment, string axisName)
        {
            return (T)assignment.Get(axisName).Value;
        }

        private static string BoardLabel(ApmwFuzzAssignment assignment)
        {
            return assignment.GetBool(ApmwFuzzOptionSpace.AxisIsSuperSized) ? "super-sized" : "standard";
        }

        private static int StableChaosSeed(string caseName, int caseIndex)
        {
            unchecked
            {
                uint hash = 2166136261U;
                foreach (char character in caseName ?? string.Empty)
                {
                    hash ^= character;
                    hash *= 16777619U;
                }

                hash ^= (uint)caseIndex;
                return (int)(hash & 0x7FFFFFFF);
            }
        }

        private static string BooleanKey(bool value)
        {
            return value ? "true" : "false";
        }

        private static string EnumKey<T>(T value)
        {
            return ApmwFuzzOptionValue.Enumeration(value).CanonicalKey;
        }

        private static string IntSetKey(IEnumerable<int> values)
        {
            return ApmwFuzzOptionValue.IntSet("set", values).CanonicalKey;
        }

        private static string IntKey(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string NullableIntKey(int? value)
        {
            return value.HasValue ? IntKey(value.Value) : "null";
        }

        private sealed class PairRequirement
        {
            public PairRequirement(
                ApmwFuzzAxis previousAxis,
                ApmwFuzzOptionValue previousValue,
                ApmwFuzzOptionValue newValue,
                string key)
            {
                PreviousAxis = previousAxis;
                PreviousValue = previousValue;
                NewValue = newValue;
                Key = key;
            }

            public ApmwFuzzAxis PreviousAxis { get; }

            public ApmwFuzzOptionValue PreviousValue { get; }

            public ApmwFuzzOptionValue NewValue { get; }

            public string Key { get; }
        }

        private sealed class InteractionDefinition
        {
            public InteractionDefinition(
                string family,
                string caseName,
                ApmwFuzzAssignment assignment,
                bool requireValid,
                int? chaosSeedOverride)
            {
                Family = family;
                CaseName = caseName;
                Assignment = assignment;
                RequireValid = requireValid;
                ChaosSeedOverride = chaosSeedOverride;
            }

            public string Family { get; }

            public string CaseName { get; }

            public ApmwFuzzAssignment Assignment { get; }

            public bool RequireValid { get; }

            public int? ChaosSeedOverride { get; }
        }
    }
}
