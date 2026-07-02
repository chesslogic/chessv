using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.APChessV;

namespace ChessV.Test
{
    internal static class ApmwFuzzCaseGenerator
    {
        private const string SmokeMasterSeed = "smoke";
        private const string BoundaryMasterSeed = "boundary";
        private const string OverCapMasterSeed = "over-cap";
        private const string StartupStandardCategory = "startup-standard";
        private const string StartupSuperSizedCategory = "startup-super-sized";
        private const string GenerationSmokeCategory = "generation-smoke";
        private const string GenerationBoundaryCategory = "generation-boundary";
        private const string GenerationOverCapCategory = "generation-over-cap";
        private const string GenerationRandomCategory = "generation-random";
        private const int StandardBoardWidth = 8;
        private const int SuperSizedBoardWidth = 10;
        private const int DefaultPocketLimitByPocket = 4;
        private const int PocketSlots = 3;
        private const int MaxPocketRange = 6;
        private const int FsCheckSampleSize = 64;

        private static readonly int[] AllArmyIndexes = { 0, 1, 2, 3, 4, 5, 6 };

        public static IEnumerable<ApmwFuzzCase> SmokeCases()
        {
            var cases = new List<ApmwFuzzCase>();

            AddCase(cases, SmokeMasterSeed, "smoke-standard-default", false,
                ApmwFuzzCase.TargetStages.StandardBoardStartup, StartupStandardCategory, null);
            AddCase(cases, SmokeMasterSeed, "smoke-super-sized-default", true,
                ApmwFuzzCase.TargetStages.SuperBoardStartup, StartupSuperSizedCategory, null);

            AddCase(cases, SmokeMasterSeed, "smoke-standard-progressive-inventory", false,
                ApmwFuzzCase.TargetStages.ItemHandlerGeneration, GenerationSmokeCategory, builder =>
            {
                builder.Goal = Goal.Progressive;
                builder.PieceLocations = PieceLocations.Ordered;
                builder.PlayerPieceTypes = PieceTypes.Stable;
                builder.EnemyPieceTypes = PieceTypes.Book;
                builder.FairyChessArmy = FairyArmy.Stable;
                builder.ArmyIndexes = new[] { 0, 1, 2 };
                builder.FairyChessPawns = FairyPawns.AnyClassical;
                builder.FairyChessPawnUpgrades = FairyPawnUpgrades.Pool;
                builder.PocketCount = 3;
                builder.PocketRangeCount = 2;
                builder.PocketGemCount = 1;
                builder.PawnCount = StandardBoardWidth;
                builder.MinorPieceCount = StandardBoardWidth - 1;
                builder.MajorPieceCount = 3;
                builder.MajorToQueenCount = 1;
                builder.PawnForwardnessCount = 1;
                builder.VictoryCount = 1;
            });

            AddCase(cases, SmokeMasterSeed, "smoke-super-sized-rich-inventory", true,
                ApmwFuzzCase.TargetStages.ItemHandlerGeneration, GenerationSmokeCategory, builder =>
            {
                builder.Goal = Goal.Super;
                builder.PieceLocations = PieceLocations.Stable;
                builder.PlayerPieceTypes = PieceTypes.Stable;
                builder.EnemyPieceTypes = PieceTypes.Chaos;
                builder.FairyChessArmy = FairyArmy.Limited;
                builder.ArmyIndexes = new[] { 0, 3, 5, 6 };
                builder.FairyChessPawns = FairyPawns.Mixed;
                builder.FairyChessPawnUpgrades = FairyPawnUpgrades.Max;
                builder.PocketCount = DefaultPocketLimitByPocket * 2;
                builder.PocketRangeCount = MaxPocketRange;
                builder.PocketGemCount = 2;
                builder.AIIntelligenceMalusCount = 5;
                builder.PawnCount = SuperSizedBoardWidth + 2;
                builder.MinorPieceCount = SuperSizedBoardWidth;
                builder.MajorPieceCount = 4;
                builder.JackCount = 1;
                builder.MajorToQueenCount = 2;
                builder.AmazonCount = 1;
                builder.PawnForwardnessCount = SuperSizedBoardWidth;
                builder.ConsulCount = 1;
                builder.KingPromotionCount = 1;
                builder.DeathLink = true;
            });

            return cases;
        }

        public static IEnumerable<ApmwFuzzCase> BoundaryCases()
        {
            var cases = new List<ApmwFuzzCase>();

            AddBoundaryCases(cases, false);
            AddBoundaryCases(cases, true);

            return cases;
        }

        public static IEnumerable<ApmwFuzzCase> OverCapItemCases()
        {
            var cases = new List<ApmwFuzzCase>();

            AddOverCapItemCases(cases, false);
            AddOverCapItemCases(cases, true);

            return cases;
        }

        public static IEnumerable<ApmwFuzzCase> RandomCases(int masterSeed, int count)
        {
            return RandomCases(masterSeed.ToString(), count);
        }

        public static IEnumerable<ApmwFuzzCase> RandomCases(string masterSeed, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Random case count cannot be negative.");

            string normalizedMasterSeed = masterSeed ?? string.Empty;
            int[] caseSeeds = SampleCaseSeeds(normalizedMasterSeed, count);
            for (int caseIndex = 0; caseIndex < caseSeeds.Length; caseIndex++)
                yield return BuildRandomCase(normalizedMasterSeed, caseIndex, caseSeeds[caseIndex]);
        }

        private static void AddBoundaryCases(List<ApmwFuzzCase> cases, bool isSuperSized)
        {
            int width = BoardWidth(isSuperSized);
            int materialCapacity = MaterialCapacity(width);
            int pawnCapacity = PawnCapacity(width);

            AddAxisCases(cases, isSuperSized, "pawn-count",
                WidthAndCapacityThresholds(width, pawnCapacity),
                (builder, value) => builder.PawnCount = value);
            AddAxisCases(cases, isSuperSized, "minor-count",
                WidthAndCapacityThresholds(width, materialCapacity),
                (builder, value) => builder.MinorPieceCount = value);
            AddAxisCases(cases, isSuperSized, "major-count",
                WidthAndCapacityThresholds(width, materialCapacity),
                (builder, value) => builder.MajorPieceCount = value);
            AddAxisCases(cases, isSuperSized, "jack-count",
                WidthAndCapacityThresholds(width, materialCapacity),
                (builder, value) => builder.JackCount = value);
            AddAxisCases(cases, isSuperSized, "major-to-queen-count",
                WidthAndCapacityThresholds(width, materialCapacity),
                (builder, value) => builder.MajorToQueenCount = value);
            AddAxisCases(cases, isSuperSized, "amazon-count",
                WidthAndCapacityThresholds(width, materialCapacity),
                (builder, value) => builder.AmazonCount = value);
            AddAxisCases(cases, isSuperSized, "pawn-forwardness-count",
                PawnForwardnessThresholds(width),
                (builder, value) => builder.PawnForwardnessCount = value);
            AddAxisCases(cases, isSuperSized, "pocket-count",
                PocketCountThresholds(width),
                (builder, value) => builder.PocketCount = value);
            AddAxisCases(cases, isSuperSized, "pocket-range-count",
                PocketRangeThresholds(width),
                (builder, value) => builder.PocketRangeCount = value);
            AddAxisCases(cases, isSuperSized, "pocket-gem-count",
                PocketRangeThresholds(width),
                (builder, value) => builder.PocketGemCount = value);
            AddAxisCases(cases, isSuperSized, "pocket-limit-by-pocket",
                UniqueNonNegative(0, 1, DefaultPocketLimitByPocket - 1, DefaultPocketLimitByPocket,
                    DefaultPocketLimitByPocket + 1, width - 1, width, width + 1),
                (builder, value) => builder.PocketLimitByPocket = value);
            AddAxisCases(cases, isSuperSized, "minor-type-limit",
                UniqueNonNegative(0, 1, 2, width - 1, width, width + 1),
                (builder, value) => builder.MinorPieceLimitByType = value);
            AddAxisCases(cases, isSuperSized, "major-type-limit",
                UniqueNonNegative(0, 1, 2, width - 1, width, width + 1),
                (builder, value) => builder.MajorPieceLimitByType = value);
            AddAxisCases(cases, isSuperSized, "queen-type-limit",
                UniqueNonNegative(0, 1, 2, width - 1, width, width + 1),
                (builder, value) => builder.QueenPieceLimitByType = value);
            AddAxisCases(cases, isSuperSized, "ai-malus-count",
                UniqueNonNegative(0, 1, 4, 5, 6),
                (builder, value) => builder.AIIntelligenceMalusCount = value);
            AddAxisCases(cases, isSuperSized, "consul-count",
                UniqueNonNegative(0, 1, 2, 3),
                (builder, value) => builder.ConsulCount = value);
            AddAxisCases(cases, isSuperSized, "king-promotion-count",
                UniqueNonNegative(0, 1, 2, 3),
                (builder, value) => builder.KingPromotionCount = value);
            AddAxisCases(cases, isSuperSized, "play-as-white-count",
                UniqueNonNegative(0, 1, 2),
                (builder, value) => builder.PlayAsWhiteCount = value);
            AddAxisCases(cases, isSuperSized, "victory-count",
                UniqueNonNegative(0, 1, 2),
                (builder, value) => builder.VictoryCount = value);
        }

        private static void AddOverCapItemCases(List<ApmwFuzzCase> cases, bool isSuperSized)
        {
            int width = BoardWidth(isSuperSized);
            int materialCapacity = MaterialCapacity(width);
            int pawnCapacity = PawnCapacity(width);
            string boardLabel = BoardLabel(isSuperSized);

            AddCase(cases, OverCapMasterSeed, "over-cap-" + boardLabel + "-pocket-range-7", isSuperSized,
                ApmwFuzzCase.TargetStages.ItemHandlerGeneration, GenerationOverCapCategory,
                builder => builder.PocketRangeCount = MaxPocketRange + 1);
            AddCase(cases, OverCapMasterSeed, "over-cap-" + boardLabel + "-pocket-range-and-gems-9", isSuperSized,
                ApmwFuzzCase.TargetStages.ItemHandlerGeneration, GenerationOverCapCategory,
                builder =>
                {
                    builder.PocketRangeCount = MaxPocketRange + 3;
                    builder.PocketGemCount = MaxPocketRange + 3;
                });
            AddCase(cases, OverCapMasterSeed, "over-cap-" + boardLabel + "-pockets-default-limit", isSuperSized,
                ApmwFuzzCase.TargetStages.ItemHandlerGeneration, GenerationOverCapCategory,
                builder =>
                {
                    builder.PocketLimitByPocket = DefaultPocketLimitByPocket;
                    builder.PocketCount = DefaultPocketLimitByPocket * PocketSlots + 1;
                });
            AddCase(cases, OverCapMasterSeed, "over-cap-" + boardLabel + "-pockets-limit-one", isSuperSized,
                ApmwFuzzCase.TargetStages.ItemHandlerGeneration, GenerationOverCapCategory,
                builder =>
                {
                    builder.PocketLimitByPocket = 1;
                    builder.PocketCount = PocketSlots + 1;
                });
            AddCase(cases, OverCapMasterSeed, "over-cap-" + boardLabel + "-pawns", isSuperSized,
                ApmwFuzzCase.TargetStages.ItemHandlerGeneration, GenerationOverCapCategory,
                builder =>
                {
                    builder.PawnCount = pawnCapacity + width;
                    builder.PawnForwardnessCount = pawnCapacity + 1;
                });
            AddCase(cases, OverCapMasterSeed, "over-cap-" + boardLabel + "-minors", isSuperSized,
                ApmwFuzzCase.TargetStages.ItemHandlerGeneration, GenerationOverCapCategory,
                builder => builder.MinorPieceCount = materialCapacity + width);
            AddCase(cases, OverCapMasterSeed, "over-cap-" + boardLabel + "-major-upgrades", isSuperSized,
                ApmwFuzzCase.TargetStages.ItemHandlerGeneration, GenerationOverCapCategory,
                builder =>
                {
                    builder.MajorPieceCount = materialCapacity + width;
                    builder.MajorToQueenCount = materialCapacity + width;
                    builder.AmazonCount = materialCapacity + width;
                });
            AddCase(cases, OverCapMasterSeed, "over-cap-" + boardLabel + "-mixed-starting-inventory", isSuperSized,
                ApmwFuzzCase.TargetStages.ItemHandlerGeneration, GenerationOverCapCategory,
                builder =>
                {
                    builder.PocketRangeCount = MaxPocketRange + 1;
                    builder.PocketCount = DefaultPocketLimitByPocket * PocketSlots + 2;
                    builder.PawnCount = pawnCapacity + 1;
                    builder.MinorPieceCount = materialCapacity + 1;
                    builder.MajorPieceCount = materialCapacity + 1;
                    builder.JackCount = width + 1;
                    builder.MajorToQueenCount = materialCapacity + 1;
                    builder.AmazonCount = materialCapacity + 1;
                    builder.PawnForwardnessCount = pawnCapacity + 1;
                    builder.ConsulCount = 3;
                    builder.KingPromotionCount = 3;
                });
        }

        private static ApmwFuzzCase BuildRandomCase(string masterSeed, int caseIndex, int caseSeed)
        {
            var random = new Random(caseSeed);
            bool isSuperSized = random.Next(2) == 0;
            int width = BoardWidth(isSuperSized);
            int materialCapacity = MaterialCapacity(width);
            int pawnCapacity = PawnCapacity(width);
            string caseName = "random-" + SanitizeCaseName(masterSeed) + "-" + caseIndex.ToString("D4");

            return CreateCase(caseName, masterSeed, caseIndex, isSuperSized,
                ApmwFuzzCase.TargetStages.ItemHandlerGeneration, GenerationRandomCategory, builder =>
            {
                builder.Goal = Pick(random, EnumValues<Goal>());
                builder.EnemyPieceTypes = Pick(random, EnumValues<PieceTypes>());
                builder.PieceLocations = Pick(random, EnumValues<PieceLocations>());
                builder.PlayerPieceTypes = Pick(random, EnumValues<PieceTypes>());
                builder.FairyChessArmy = Pick(random, EnumValues<FairyArmy>());
                builder.ArmyIndexes = PickArmyIndexes(random);
                builder.FairyChessPawns = Pick(random, EnumValues<FairyPawns>());
                builder.FairyChessPawnUpgrades = Pick(random, EnumValues<FairyPawnUpgrades>());
                builder.MinorPieceLimitByType = Pick(random, new[] { 0, 1, 2, 3 });
                builder.MajorPieceLimitByType = Pick(random, new[] { 0, 1, 2, 3 });
                builder.QueenPieceLimitByType = Pick(random, new[] { 0, 1, 2, 3 });
                builder.PocketLimitByPocket = Pick(random, new[] { 1, 2, 3, 4 });
                builder.DeathLink = random.Next(2) == 0;

                builder.PocketSeed = NextSeed(random);
                builder.PawnSeed = NextSeed(random);
                builder.MinorSeed = NextSeed(random);
                builder.MajorSeed = NextSeed(random);
                builder.QueenSeed = NextSeed(random);
                builder.DeterministicChaosSeed =
                    builder.PlayerPieceTypes == PieceTypes.Chaos || builder.PieceLocations == PieceLocations.Chaos
                        ? (int?)NextSeed(random)
                        : null;

                builder.PocketCount = random.Next(0, builder.PocketLimitByPocket * PocketSlots + 1);
                builder.PocketRangeCount = random.Next(0, MaxPocketRange + 1);
                builder.PocketGemCount = random.Next(0, MaxPocketRange + 2);
                builder.AIIntelligenceMalusCount = random.Next(0, 6);
                builder.ConsulCount = random.Next(0, 3);
                builder.KingPromotionCount = random.Next(0, 3);

                int occupiedMaterialSlots = builder.ConsulCount;
                builder.JackCount = random.Next(0, Math.Min(2, Math.Max(0, materialCapacity - occupiedMaterialSlots)) + 1);
                occupiedMaterialSlots += builder.JackCount;
                builder.MajorPieceCount = random.Next(0, Math.Min(width + 1, Math.Max(0, materialCapacity - occupiedMaterialSlots)) + 1);
                int majorUpgradeCount = builder.MajorPieceCount == 0 ? 0 : random.Next(0, builder.MajorPieceCount + 1);
                builder.MajorToQueenCount = majorUpgradeCount == 0 ? 0 : random.Next(0, majorUpgradeCount + 1);
                builder.AmazonCount = majorUpgradeCount - builder.MajorToQueenCount;
                occupiedMaterialSlots += builder.MajorPieceCount;
                builder.MinorPieceCount = random.Next(0, Math.Max(0, materialCapacity - occupiedMaterialSlots) + 1);

                builder.PawnCount = random.Next(0, pawnCapacity + 1);
                builder.PawnForwardnessCount = random.Next(0, Math.Min(pawnCapacity, width * 3) + 1);
                builder.SuperSizeMeCount = isSuperSized ? 1 : 0;
                builder.PlayAsWhiteCount = random.Next(0, 2);
                builder.VictoryCount = random.Next(0, 2);
            });
        }

        private static void AddAxisCases(
            List<ApmwFuzzCase> cases,
            bool isSuperSized,
            string axisName,
            IEnumerable<int> values,
            Action<ApmwFuzzCase.Builder, int> setValue)
        {
            foreach (int value in values)
            {
                AddCase(cases, BoundaryMasterSeed,
                    "boundary-" + BoardLabel(isSuperSized) + "-" + axisName + "-" + value,
                    isSuperSized,
                    ApmwFuzzCase.TargetStages.ItemHandlerGeneration,
                    GenerationBoundaryCategory,
                    builder => setValue(builder, value));
            }
        }

        private static void AddCase(
            List<ApmwFuzzCase> cases,
            string masterSeed,
            string caseName,
            bool isSuperSized,
            string targetStage,
            string category,
            Action<ApmwFuzzCase.Builder> configure)
        {
            cases.Add(CreateCase(caseName, masterSeed, cases.Count, isSuperSized, targetStage, category, configure));
        }

        private static ApmwFuzzCase CreateCase(
            string caseName,
            string masterSeed,
            int caseIndex,
            bool isSuperSized,
            string targetStage,
            string category,
            Action<ApmwFuzzCase.Builder> configure)
        {
            ApmwFuzzCase baseCase = isSuperSized
                ? ApmwFuzzCase.DefaultSuperSized()
                : ApmwFuzzCase.DefaultStandard();

            return baseCase.With(builder =>
            {
                builder.CaseName = caseName;
                builder.CaseIndex = caseIndex;
                builder.MasterSeed = masterSeed ?? string.Empty;
                builder.TargetStage = targetStage ?? ApmwFuzzCase.TargetStages.ItemHandlerGeneration;
                builder.Category = BuildCategory(category, isSuperSized);
                builder.IsSuperSized = isSuperSized;
                builder.SuperSizeMeCount = isSuperSized ? Math.Max(1, builder.SuperSizeMeCount) : 0;
                if (isSuperSized && builder.PawnCount == StandardBoardWidth)
                    builder.PawnCount = SuperSizedBoardWidth;

                configure?.Invoke(builder);
            });
        }

        private static string BuildCategory(string category, bool isSuperSized)
        {
            string baseCategory = string.IsNullOrWhiteSpace(category) ? "uncategorized" : category;
            string boardLabel = BoardLabel(isSuperSized);
            return baseCategory.EndsWith("-" + boardLabel, StringComparison.Ordinal)
                ? baseCategory
                : baseCategory + "-" + boardLabel;
        }

        private static int[] SampleCaseSeeds(string masterSeed, int count)
        {
            var seedGenerator = FsCheck.FSharp.Gen.Choose(0, int.MaxValue);
            return FsCheck.FSharp.Gen.Sample(
                new FsCheck.Rnd(StableSeed64(masterSeed)),
                FsCheckSampleSize,
                count,
                seedGenerator);
        }

        private static ulong StableSeed64(string masterSeed)
        {
            unchecked
            {
                ulong hash = 14695981039346656037UL;
                foreach (char character in masterSeed ?? string.Empty)
                {
                    hash ^= character;
                    hash *= 1099511628211UL;
                }

                return hash == 0 ? 1UL : hash;
            }
        }

        private static int NextSeed(Random random)
        {
            return random.Next(0, int.MaxValue);
        }

        private static int BoardWidth(bool isSuperSized)
        {
            return isSuperSized ? SuperSizedBoardWidth : StandardBoardWidth;
        }

        private static string BoardLabel(bool isSuperSized)
        {
            return isSuperSized ? "super-sized" : "standard";
        }

        private static int MaterialCapacity(int boardWidth)
        {
            return boardWidth * 2 - 1;
        }

        private static int PawnCapacity(int boardWidth)
        {
            return boardWidth * 4;
        }

        private static IEnumerable<int> WidthAndCapacityThresholds(int boardWidth, int capacity)
        {
            return UniqueNonNegative(0, 1, boardWidth - 1, boardWidth, boardWidth + 1,
                capacity - 1, capacity, capacity + 1);
        }

        private static IEnumerable<int> PawnForwardnessThresholds(int boardWidth)
        {
            int pawnCapacity = PawnCapacity(boardWidth);
            int forwardnessCapacity = boardWidth * 3;
            return UniqueNonNegative(0, 1, boardWidth - 1, boardWidth, boardWidth + 1,
                forwardnessCapacity - 1, forwardnessCapacity, forwardnessCapacity + 1,
                pawnCapacity - 1, pawnCapacity, pawnCapacity + 1);
        }

        private static IEnumerable<int> PocketCountThresholds(int boardWidth)
        {
            int pocketCapacity = DefaultPocketLimitByPocket * PocketSlots;
            return UniqueNonNegative(0, 1, boardWidth - 1, boardWidth, boardWidth + 1,
                DefaultPocketLimitByPocket - 1, DefaultPocketLimitByPocket,
                DefaultPocketLimitByPocket + 1, pocketCapacity - 1, pocketCapacity,
                pocketCapacity + 1);
        }

        private static IEnumerable<int> PocketRangeThresholds(int boardWidth)
        {
            return UniqueNonNegative(0, 1, boardWidth - 1, boardWidth, boardWidth + 1,
                MaxPocketRange - 1, MaxPocketRange, MaxPocketRange + 1);
        }

        private static IEnumerable<int> UniqueNonNegative(params int[] values)
        {
            return values.Where(value => value >= 0).Distinct().OrderBy(value => value);
        }

        private static T Pick<T>(Random random, IReadOnlyList<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Count == 0)
                throw new ArgumentException("At least one value is required.", nameof(values));

            return values[random.Next(values.Count)];
        }

        private static T[] EnumValues<T>()
        {
            return (T[])Enum.GetValues(typeof(T));
        }

        private static IEnumerable<int> PickArmyIndexes(Random random)
        {
            if (random.Next(5) == 0)
                return Enumerable.Empty<int>();

            int[] indexes = AllArmyIndexes.ToArray();
            for (int i = indexes.Length - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                int temp = indexes[i];
                indexes[i] = indexes[swapIndex];
                indexes[swapIndex] = temp;
            }

            int count = random.Next(1, indexes.Length + 1);
            return indexes.Take(count).OrderBy(index => index).ToArray();
        }

        private static string SanitizeCaseName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "seed";

            var sanitized = new string(value
                .Select(character => char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '-')
                .Take(32)
                .ToArray()).Trim('-');

            return string.IsNullOrEmpty(sanitized) ? "seed" : sanitized;
        }
    }
}
