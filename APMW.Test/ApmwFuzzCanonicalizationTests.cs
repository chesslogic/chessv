using System.Linq;
using Archipelago.APChessV;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChessV.Test
{
    [TestClass]
    [DoNotParallelize]
    public class ApmwFuzzCanonicalizationTests
    {
        [TestMethod]
        public void CanonicalOptionKey_NormalizesOptionVectorAndIgnoresDiagnosticMetadata()
        {
            ApmwFuzzCase first = ApmwFuzzCase.DefaultStandard().With(builder =>
            {
                builder.CaseName = "canonical-first";
                builder.Category = "generation-periphery-standard";
                builder.TargetStage = ApmwFuzzCase.TargetStages.ItemHandlerGeneration;
                builder.ArmyIndexes = new[] { 2, 1, 2 };
            });
            ApmwFuzzCase second = ApmwFuzzCase.DefaultStandard().With(builder =>
            {
                builder.CaseName = "canonical-second";
                builder.Category = "generation-random-standard";
                builder.TargetStage = ApmwFuzzCase.TargetStages.MoveGeneration;
                builder.ArmyIndexes = new[] { 1, 2 };
            });

            Assert.AreEqual(first.CanonicalOptionKey, second.CanonicalOptionKey);
            Assert.AreNotEqual(first.ToDiagnosticString(), second.ToDiagnosticString());
            StringAssert.Contains(first.CanonicalOptionKey, "army=1,2");
        }

        [TestMethod]
        public void CanonicalOptionKey_TracksProgressionItemizationAndActiveItemCounts()
        {
            ApmwFuzzCase legacy = ApmwFuzzCase.DefaultStandard();
            ApmwFuzzCase fundamental = legacy.With(builder =>
            {
                builder.CaseName = "canonical-fundamental";
                builder.ProgressionItemization = ProgressionItemization.Fundamental;
                builder.ChessmenCount = 8;
                builder.MaterialCount = 2;
                builder.CastlerCount = 1;
            });

            Assert.AreNotEqual(legacy.CanonicalOptionKey, fundamental.CanonicalOptionKey);
            StringAssert.Contains(legacy.CanonicalOptionKey, "progression_itemization=legacy");
            StringAssert.Contains(fundamental.CanonicalOptionKey, "progression_itemization=fundamental");
            StringAssert.Contains(fundamental.CanonicalOptionKey, "chessmen-count=8");
            StringAssert.Contains(fundamental.CanonicalOptionKey, "material-count=2");
            StringAssert.Contains(fundamental.CanonicalOptionKey, "castler-count=1");

            Assert.IsFalse(legacy.BuildSlotData().ContainsKey(ApmwConstants.SlotKeyProgressionItemization));
            Assert.AreEqual(
                (int)ProgressionItemization.Fundamental,
                (int)fundamental.BuildSlotData()[ApmwConstants.SlotKeyProgressionItemization]);

            var legacyItems = legacy.BuildItemCountMap();
            Assert.AreEqual(8, legacyItems[ApmwConstants.ProgressiveItems.Pawn]);
            Assert.IsFalse(legacyItems.ContainsKey(ApmwConstants.ProgressiveItems.Chessmen));
            Assert.IsFalse(legacyItems.ContainsKey(ApmwConstants.ProgressiveItems.Material));
            Assert.IsFalse(legacyItems.ContainsKey(ApmwConstants.ProgressiveItems.Castler));

            var fundamentalItems = fundamental.BuildItemCountMap();
            Assert.AreEqual(8, fundamentalItems[ApmwConstants.ProgressiveItems.Chessmen]);
            Assert.AreEqual(2, fundamentalItems[ApmwConstants.ProgressiveItems.Material]);
            Assert.AreEqual(1, fundamentalItems[ApmwConstants.ProgressiveItems.Castler]);
            Assert.IsFalse(fundamentalItems.ContainsKey(ApmwConstants.ProgressiveItems.Pawn));
            Assert.IsFalse(fundamentalItems.ContainsKey(ApmwConstants.ProgressiveItems.MinorPiece));
        }

        [TestMethod]
        public void RunCases_DeduplicatesByCanonicalOptionKeyBeforeExecution()
        {
            ApmwFuzzCase first = InvalidArmyCase("duplicate-first", new[] { 999 });
            ApmwFuzzCase duplicate = InvalidArmyCase("duplicate-second", new[] { 999, 999 });

            Assert.AreEqual(first.CanonicalOptionKey, duplicate.CanonicalOptionKey);

            ApmwFuzzRunResult result = ApmwFuzzRunner.RunCases(
                new[] { first, duplicate },
                2,
                ApmwFuzzStage.ItemHandlerGeneration);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(2, result.TotalCaseCount);
            Assert.AreEqual(1, result.ExecutedCaseCount);
            Assert.AreEqual(1, result.Failures.Count);
        }

        [TestMethod]
        public void ShrinkCandidates_IncludeFamilyAndPeripheryBoundaryAxisResets()
        {
            ApmwFuzzCase fuzzCase = ApmwFuzzCase.DefaultSuperSized().With(builder =>
            {
                builder.CaseName = "periphery-family-boundaries";
                builder.Category = "generation-periphery-super-sized";
                builder.PawnCount = 37;
            });

            var candidates = ApmwFuzzRunner.ShrinkCandidates(fuzzCase).ToList();

            Assert.IsTrue(candidates.Any(candidate =>
                !candidate.IsSuperSized &&
                candidate.PawnCount == fuzzCase.PawnCount &&
                candidate.SuperSizeMeCount == fuzzCase.SuperSizeMeCount));
            Assert.IsTrue(candidates.Any(candidate =>
                candidate.IsSuperSized == fuzzCase.IsSuperSized &&
                candidate.SuperSizeMeCount == fuzzCase.SuperSizeMeCount &&
                candidate.PawnCount == 8));
            Assert.IsTrue(candidates.Any(candidate =>
                candidate.IsSuperSized == fuzzCase.IsSuperSized &&
                candidate.SuperSizeMeCount == fuzzCase.SuperSizeMeCount &&
                candidate.PawnCount == 40));
        }

        [TestMethod]
        public void PeripheryFailure_ReportIncludesMinimizedReproCase()
        {
            ApmwFuzzCase fuzzCase = InvalidArmyCase("periphery-shrinking-invalid-army", new[] { 999 }).With(builder =>
            {
                builder.Category = "generation-periphery-standard";
                builder.PocketCount = 3;
                builder.PawnCount = 9;
            });

            ApmwFuzzRunResult result = ApmwFuzzRunner.RunCases(
                new[] { fuzzCase },
                1,
                ApmwFuzzStage.ItemHandlerGeneration);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(1, result.Failures.Count);
            ApmwFuzzFailure failure = result.Failures[0];
            Assert.IsNotNull(failure.MinimizedFailure);
            Assert.AreEqual(failure.Kind, failure.MinimizedFailure.Kind);
            Assert.AreEqual(failure.StageName, failure.MinimizedFailure.StageName);
            Assert.AreEqual(failure.ExceptionType, failure.MinimizedFailure.ExceptionType);
            Assert.AreEqual(0, failure.MinimizedFailure.FuzzCase.PocketCount);
            Assert.AreEqual(8, failure.MinimizedFailure.FuzzCase.PawnCount);
            StringAssert.Contains(result.ToFailureMessage(), "Minimized repro case");
        }

        private static ApmwFuzzCase InvalidArmyCase(string caseName, int[] armyIndexes)
        {
            return ApmwFuzzCase.DefaultStandard().With(builder =>
            {
                builder.CaseName = caseName;
                builder.MasterSeed = "canonical-shrink-test";
                builder.Category = "generation-periphery-standard";
                builder.ArmyIndexes = armyIndexes;
            });
        }
    }
}
