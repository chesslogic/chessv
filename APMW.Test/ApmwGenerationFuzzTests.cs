using Archipelago.APChessV;
using ChessV.Base;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChessV.Test
{
    [TestClass]
    [DoNotParallelize]
    public class ApmwGenerationFuzzTests
    {
        private const int StressRandomCaseCount = 32;

        [TestMethod]
        public void SmokeCases_RunItemGenerationGameStartupAndInitialMoveGeneration()
        {
            RunCases(ApmwFuzzCaseGenerator.SmokeCases(), 1);
        }

        [TestMethod]
        public void BoundaryStandardMajorCountEight_RunItemGeneration()
        {
            RunItemHandlerGenerationCase(BoundaryCase("boundary-standard-major-count-8"));
        }

        [TestMethod]
        public void BoundarySuperSizedMajorCountTen_RunItemGeneration()
        {
            RunItemHandlerGenerationCase(BoundaryCase("boundary-super-sized-major-count-10"));
        }

        [TestMethod]
        public void BoundaryStandardHighMajorCounts_RunItemGenerationGameStartupAndInitialMoveGeneration()
        {
            RunCases(new[]
            {
                BoundaryCase("boundary-standard-major-count-14"),
                BoundaryCase("boundary-standard-major-count-15"),
                BoundaryCase("boundary-standard-major-count-16"),
            }, 3);
        }

        [TestMethod]
        public void BoundarySuperSizedHighMajorCounts_RunItemGenerationGameStartupAndInitialMoveGeneration()
        {
            RunCases(new[]
            {
                BoundaryCase("boundary-super-sized-major-count-18"),
                BoundaryCase("boundary-super-sized-major-count-19"),
                BoundaryCase("boundary-super-sized-major-count-20"),
            }, 3);
        }

        [TestMethod]
        public void OverCapItemCases_RunItemGeneration()
        {
            RunCases(ApmwFuzzCaseGenerator.OverCapItemCases(), 1, ApmwFuzzStage.ItemHandlerGeneration);
        }

        [TestMethod]
        public void NeutralUpgradeActions_DoNotConsumeDirectTargetsBeforeFallback()
        {
            var fuzzCase = ApmwFuzzCase.DefaultStandard().With(builder =>
            {
                builder.CaseName = "neutral-upgrades-preserve-direct-targets";
                builder.Category = "generation-boundary";
                builder.MinorPieceCount = 1;
                builder.MajorPieceCount = 2;
                builder.JackCount = 1;
                builder.MajorToQueenCount = 1;
                builder.AmazonCount = 1;
            });

            using (var scope = ApmwFuzzScope.Configure(fuzzCase))
            {
                var result = scope.RunItemHandlerGeneration();
                var core = ApmwCore.getInstance();
                var generated = result.PlayerPieceSet.Values.ToList();

                Assert.AreEqual(1, generated.Count(piece => core.minors.Contains(piece)));
                Assert.AreEqual(1, generated.Count(piece => core.majors.Contains(piece)));
                Assert.AreEqual(1, generated.Count(piece => core.jacks.Contains(piece)));
                Assert.AreEqual(1, generated.Count(piece => core.queens.Contains(piece)));
                Assert.AreEqual(0, generated.Count(piece => core.amazons.Contains(piece)));
            }
        }

        [TestMethod]
        public void RegressionRandomLocalStress0007FilteredArmyMajorPool_RunItemGeneration()
        {
            var fuzzCase = ApmwFuzzCase.DefaultStandard().With(builder =>
            {
                builder.CaseName = "random-local-stress-0007";
                builder.CaseIndex = 7;
                builder.MasterSeed = "local-stress";
                builder.Category = "generation-random-standard";
                builder.ArmyIndexes = new[] { 4 };
            });

            RunItemHandlerGenerationCase(fuzzCase);
        }

        [TestMethod]
        public void RandomCases_IncludeDeterministicChaosSeedWhenChaosIsSelected()
        {
            var cases = ApmwFuzzCaseGenerator.RandomCases("chaos-seed-test", 128).ToList();
            Assert.IsTrue(cases.Any(fuzzCase =>
                fuzzCase.PlayerPieceTypes == PieceTypes.Chaos ||
                fuzzCase.PieceLocations == PieceLocations.Chaos));

            foreach (var fuzzCase in cases)
            {
                if (fuzzCase.PlayerPieceTypes == PieceTypes.Chaos ||
                    fuzzCase.PieceLocations == PieceLocations.Chaos)
                    Assert.IsTrue(
                        fuzzCase.DeterministicChaosSeed.HasValue,
                        "Chaos random fuzz case did not include a deterministic chaos seed: " + fuzzCase.ToDiagnosticString());
            }
        }

        [TestMethod]
        public void RandomFailure_ReportIncludesMinimizedReproCase()
        {
            var fuzzCase = ApmwFuzzCase.DefaultStandard().With(builder =>
            {
                builder.CaseName = "random-shrinking-invalid-army";
                builder.MasterSeed = "shrink-test";
                builder.Category = "generation-random-standard";
                builder.ArmyIndexes = new[] { 999 };
                builder.PocketCount = 3;
                builder.PawnCount = 9;
            });

            ApmwFuzzRunResult result = ApmwFuzzRunner.RunCases(
                new[] { fuzzCase },
                1,
                ApmwFuzzStage.ItemHandlerGeneration);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(1, result.Failures.Count);
            Assert.IsNotNull(result.Failures[0].MinimizedFailure);
            StringAssert.Contains(result.ToFailureMessage(), "Minimized repro case");
        }

        [TestMethod]
        public void RegressionStandardHighMajorCountsWithConsul_RunItemGenerationGameStartupAndInitialMoveGeneration()
        {
            RunCases(new[]
            {
                StandardHighMajorConsulCase("regression-standard-major-count-14-consul-1", 0, 14),
                StandardHighMajorConsulCase("regression-standard-major-count-15-consul-1", 1, 15),
            }, 2);
        }

        [TestMethod]
        [Ignore("Local fuzz stress entry point; enable manually to collect up to 10 failures.")]
        public void StressCases_RunItemGenerationGameStartupAndInitialMoveGeneration_StopAfter10Failures()
        {
            RunCases(StressCases(), 10);
        }

        [TestMethod]
        [Ignore("Local fuzz stress entry point; enable manually to collect up to 100 failures.")]
        public void StressCases_RunItemGenerationGameStartupAndInitialMoveGeneration_StopAfter100Failures()
        {
            RunCases(StressCases(), 100);
        }

        private static ApmwFuzzCase BoundaryCase(string caseName)
        {
            var fuzzCase = ApmwFuzzCaseGenerator.BoundaryCases()
                .SingleOrDefault(item => item.CaseName == caseName);
            Assert.IsNotNull(fuzzCase, "Boundary fuzz case was not generated: " + caseName);
            return fuzzCase;
        }

        private static ApmwFuzzCase StandardHighMajorConsulCase(string caseName, int caseIndex, int majorCount)
        {
            return ApmwFuzzCase.DefaultStandard().With(builder =>
            {
                builder.CaseName = caseName;
                builder.CaseIndex = caseIndex;
                builder.MasterSeed = "regression";
                builder.Category = "generation-regression-standard";
                builder.MajorPieceCount = majorCount;
                builder.ConsulCount = 1;
                builder.MajorToQueenCount = 1;
            });
        }

        private static void RunItemHandlerGenerationCase(ApmwFuzzCase fuzzCase)
        {
            RunCases(new[] { fuzzCase }, 1, ApmwFuzzStage.ItemHandlerGeneration);
        }

        private static IEnumerable<ApmwFuzzCase> StressCases()
        {
            return ApmwFuzzCaseGenerator.BoundaryCases()
                .Concat(ApmwFuzzCaseGenerator.OverCapItemCases())
                .Concat(ApmwFuzzCaseGenerator.RandomCases("local-stress", StressRandomCaseCount));
        }

        private static void RunCases(IEnumerable<ApmwFuzzCase> cases, int maxFailures)
        {
            RunCases(cases, maxFailures, ApmwFuzzStage.MoveGeneration);
        }

        private static void RunCases(IEnumerable<ApmwFuzzCase> cases, int maxFailures, ApmwFuzzStage finalStage)
        {
            ApmwFuzzRunner.AssertNoFailures(ApmwFuzzRunner.RunCases(cases, maxFailures, finalStage));
        }
    }
}
