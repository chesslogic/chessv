using System.Collections.Generic;
using System.Linq;
using Archipelago.APChessV;

namespace ChessV.Test
{
    [TestClass]
    [DoNotParallelize]
    public class ApmwPairwiseCaseGeneratorTests
    {
        [TestMethod]
        public void PairwiseCases_AreDeterministicAndCoverCategoricalPairs()
        {
            IReadOnlyList<ApmwFuzzCase> cases = ApmwPairwiseCaseGenerator.PairwiseCases();
            IReadOnlyList<ApmwFuzzCase> repeatedCases = ApmwPairwiseCaseGenerator.PairwiseCases();

            Assert.IsTrue(cases.Count > 0);
            CollectionAssert.AreEqual(
                cases.Select(fuzzCase => fuzzCase.CaseName).ToArray(),
                repeatedCases.Select(fuzzCase => fuzzCase.CaseName).ToArray());
            CollectionAssert.AreEqual(
                new[]
                {
                    "pairwise-0000-standard",
                    "pairwise-0001-standard",
                    "pairwise-0002-standard",
                    "pairwise-0003-standard",
                    "pairwise-0004-standard",
                    "pairwise-0005-super-sized",
                    "pairwise-0006-super-sized",
                    "pairwise-0007-super-sized",
                    "pairwise-0008-super-sized",
                    "pairwise-0009-super-sized",
                },
                cases.Take(10).Select(fuzzCase => fuzzCase.CaseName).ToArray());
            Assert.AreEqual(cases.Count, cases.Select(fuzzCase => fuzzCase.CaseName).Distinct().Count());

            Assert.IsTrue(cases.Where(fuzzCase => fuzzCase.IsSuperSized)
                .All(fuzzCase => fuzzCase.SuperSizeMeCount == 1 &&
                    (fuzzCase.ProgressionItemization == ProgressionItemization.Fundamental || fuzzCase.PawnCount == 10)));
            Assert.IsTrue(cases.Any(fuzzCase => fuzzCase.ProgressionItemization == ProgressionItemization.Legacy));
            Assert.IsTrue(cases.Any(fuzzCase => fuzzCase.ProgressionItemization == ProgressionItemization.Fundamental));
            AssertFundamentalCasesUseFundamentalItemCounts(cases);
            AssertAllCategoricalPairsCovered(cases);
        }

        [TestMethod]
        public void PeripheryCases_AreDeterministicDedupedAndIncludeInteractionFamilies()
        {
            IReadOnlyList<ApmwFuzzCase> cases = ApmwPairwiseCaseGenerator.PeripheryCases();
            IReadOnlyList<ApmwFuzzCase> repeatedCases = ApmwPairwiseCaseGenerator.PeripheryCases();

            Assert.IsTrue(cases.Count > ApmwPairwiseCaseGenerator.PairwiseCases().Count);
            CollectionAssert.AreEqual(
                cases.Select(fuzzCase => fuzzCase.CaseName).ToArray(),
                repeatedCases.Select(fuzzCase => fuzzCase.CaseName).ToArray());
            AssertNoDuplicateCanonicalKeys(cases);
            CollectionAssert.AreEqual(
                ExpectedInteractionCaseNames(),
                cases
                    .Where(fuzzCase => fuzzCase.CaseName.StartsWith("interaction-"))
                    .Select(fuzzCase => fuzzCase.CaseName)
                    .ToArray());
        }

        [TestMethod]
        public void PeripherySubset_RunItemGeneration()
        {
            IReadOnlyDictionary<string, ApmwFuzzCase> casesByName = ApmwPairwiseCaseGenerator.PeripheryCases()
                .ToDictionary(fuzzCase => fuzzCase.CaseName);
            string[] subsetNames =
            {
                "pairwise-0000-standard",
                "pairwise-0005-super-sized",
                "pairwise-0012-standard",
                "interaction-pockets-standard-default-limit-max-fill",
                "interaction-pawns-super-over-forwardness-any-classical",
                "interaction-majors-standard-queen-conversion-cap",
                "interaction-majors-standard-list-minor-to-jack-chain",
                "interaction-fundamental-standard-chessmen-material-castler",
                "interaction-chaos-locations-seeded-max",
                "interaction-over-cap-super-material-and-pockets",
            };

            ApmwFuzzCase[] subset = subsetNames.Select(caseName => casesByName[caseName]).ToArray();
            Assert.AreEqual(subsetNames.Length, subset.Length);

            ApmwFuzzRunner.AssertNoFailures(ApmwFuzzRunner.RunCases(
                subset,
                1,
                ApmwFuzzStage.ItemHandlerGeneration));
        }

        private static void AssertFundamentalCasesUseFundamentalItemCounts(IReadOnlyList<ApmwFuzzCase> cases)
        {
            ApmwFuzzCase legacy = cases.First(fuzzCase => fuzzCase.ProgressionItemization == ProgressionItemization.Legacy);
            Assert.IsFalse(legacy.BuildSlotData().ContainsKey(ApmwConstants.SlotKeyProgressionItemization));
            Assert.IsFalse(legacy.BuildItemCountMap().ContainsKey(ApmwConstants.ProgressiveItems.Chessmen));

            ApmwFuzzCase fundamental = cases.First(fuzzCase => fuzzCase.ProgressionItemization == ProgressionItemization.Fundamental);
            Assert.AreEqual(
                (int)ProgressionItemization.Fundamental,
                (int)fundamental.BuildSlotData()[ApmwConstants.SlotKeyProgressionItemization]);
            Dictionary<string, int> itemCounts = fundamental.BuildItemCountMap();
            Assert.IsTrue(itemCounts.ContainsKey(ApmwConstants.ProgressiveItems.Chessmen));
            Assert.IsTrue(itemCounts.ContainsKey(ApmwConstants.ProgressiveItems.Material));
            Assert.IsTrue(itemCounts.ContainsKey(ApmwConstants.ProgressiveItems.Castler));
            Assert.IsFalse(itemCounts.ContainsKey(ApmwConstants.ProgressiveItems.Pawn));
            Assert.IsFalse(itemCounts.ContainsKey(ApmwConstants.ProgressiveItems.MinorPiece));
        }

        private static void AssertAllCategoricalPairsCovered(IReadOnlyList<ApmwFuzzCase> cases)
        {
            ApmwFuzzOptionSpace optionSpace = ApmwFuzzOptionSpace.CreateDefault();
            ApmwFuzzAxis[] axes = ApmwPairwiseCaseGenerator.CategoricalAxisNames
                .Select(optionSpace.GetAxis)
                .ToArray();
            HashSet<string> expectedPairs = BuildExpectedPairs(axes);
            var coveredPairs = new HashSet<string>();

            foreach (ApmwFuzzCase fuzzCase in cases)
            {
                for (int firstIndex = 0; firstIndex < axes.Length; firstIndex++)
                {
                    for (int secondIndex = firstIndex + 1; secondIndex < axes.Length; secondIndex++)
                    {
                        ApmwFuzzAxis firstAxis = axes[firstIndex];
                        ApmwFuzzAxis secondAxis = axes[secondIndex];
                        coveredPairs.Add(ApmwFuzzOptionSpace.BuildPairCoverageKey(
                            firstAxis,
                            firstAxis.GetValue(ApmwPairwiseCaseGenerator.CategoricalValueKey(fuzzCase, firstAxis.Name)),
                            secondAxis,
                            secondAxis.GetValue(ApmwPairwiseCaseGenerator.CategoricalValueKey(fuzzCase, secondAxis.Name))));
                    }
                }
            }

            Assert.IsTrue(expectedPairs.Count > 0);
            CollectionAssert.AreEquivalent(
                expectedPairs.ToArray(),
                coveredPairs.ToArray());
        }

        private static HashSet<string> BuildExpectedPairs(IReadOnlyList<ApmwFuzzAxis> axes)
        {
            var expectedPairs = new HashSet<string>();
            for (int firstIndex = 0; firstIndex < axes.Count; firstIndex++)
            {
                for (int secondIndex = firstIndex + 1; secondIndex < axes.Count; secondIndex++)
                {
                    ApmwFuzzAxis firstAxis = axes[firstIndex];
                    ApmwFuzzAxis secondAxis = axes[secondIndex];
                    foreach (ApmwFuzzOptionValue firstValue in firstAxis.Values)
                    {
                        foreach (ApmwFuzzOptionValue secondValue in secondAxis.Values)
                        {
                            expectedPairs.Add(ApmwFuzzOptionSpace.BuildPairCoverageKey(
                                firstAxis,
                                firstValue,
                                secondAxis,
                                secondValue));
                        }
                    }
                }
            }

            return expectedPairs;
        }

        private static void AssertNoDuplicateCanonicalKeys(IEnumerable<ApmwFuzzCase> cases)
        {
            string[] duplicates = cases
                .GroupBy(ApmwPairwiseCaseGenerator.CanonicalKey)
                .Where(group => group.Count() > 1)
                .Select(group => string.Join(", ", group.Select(fuzzCase => fuzzCase.CaseName)))
                .ToArray();

            Assert.AreEqual(0, duplicates.Length, "Duplicate periphery fuzz case keys: " + string.Join("; ", duplicates));
        }

        private static string[] ExpectedInteractionCaseNames()
        {
            return new[]
            {
                "interaction-pockets-standard-empty-limit-disabled",
                "interaction-pockets-standard-default-limit-max-fill",
                "interaction-pockets-super-over-limit-range-gems",
                "interaction-pawns-standard-none-upgrade-pool",
                "interaction-pawns-standard-cap-forwardness-upgrades",
                "interaction-pawns-super-over-forwardness-any-classical",
                "interaction-majors-standard-queen-conversion-cap",
                "interaction-majors-super-jacks-consuls",
                "interaction-majors-standard-over-cap-mixed",
                "interaction-majors-standard-list-minor-to-jack-chain",
                "interaction-majors-standard-priority-map-disable-major-to-queen",
                "interaction-fundamental-standard-chessmen-only",
                "interaction-fundamental-standard-chessmen-material-castler",
                "interaction-fundamental-super-over-chessmen-material-castlers",
                "interaction-army-limited-empty-type-limits-one",
                "interaction-army-stable-scattered-standard-width-limits",
                "interaction-army-chaos-opening-super-over-width-limits",
                "interaction-chaos-types-seeded-zero",
                "interaction-chaos-locations-seeded-max",
                "interaction-chaos-types-and-locations-seeded-fixed",
                "interaction-over-cap-standard-pawns-forwardness",
                "interaction-over-cap-super-material-and-pockets",
            };
        }
    }
}
