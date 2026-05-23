using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChessV.Test
{
    [TestClass]
    [DoNotParallelize]
    public class ApmwFuzzSuiteTests
    {
        [TestMethod]
        public void SelectCases_ShardingIsDeterministicAndPartitionsExpandedSuite()
        {
            IReadOnlyList<ApmwFuzzCase> allCases = ApmwFuzzSuite.ExpandedPeripheryCases();
            ApmwFuzzSuiteSelection firstSelection = ApmwFuzzSuite.SelectExpandedPeripheryCases(1, 3, null);
            ApmwFuzzSuiteSelection repeatedSelection = ApmwFuzzSuite.SelectExpandedPeripheryCases(1, 3, null);

            CollectionAssert.AreEqual(
                CanonicalKeys(firstSelection.Cases),
                CanonicalKeys(repeatedSelection.Cases));

            string[] partitionedKeys = Enumerable.Range(0, 3)
                .SelectMany(shardIndex => ApmwFuzzSuite.SelectExpandedPeripheryCases(shardIndex, 3, null).Cases)
                .Select(fuzzCase => fuzzCase.CanonicalOptionKey)
                .ToArray();

            Assert.AreEqual(allCases.Count, partitionedKeys.Length);
            Assert.AreEqual(allCases.Count, partitionedKeys.Distinct(StringComparer.Ordinal).Count());
            CollectionAssert.AreEquivalent(CanonicalKeys(allCases), partitionedKeys);
            Assert.AreEqual(1, firstSelection.ShardIndex);
            Assert.AreEqual(3, firstSelection.ShardCount);
            Assert.AreEqual(allCases.Count, firstSelection.TotalCaseCount);
        }

        [TestMethod]
        public void SelectCases_CaseBudgetIsAppliedAfterSharding()
        {
            ApmwFuzzSuiteSelection unbudgeted = ApmwFuzzSuite.SelectExpandedPeripheryCases(0, 4, null);
            ApmwFuzzSuiteSelection budgeted = ApmwFuzzSuite.SelectExpandedPeripheryCases(0, 4, 5);
            ApmwFuzzSuiteSelection zeroBudget = ApmwFuzzSuite.SelectExpandedPeripheryCases(0, 4, 0);
            ApmwFuzzSuiteSelection overBudget = ApmwFuzzSuite.SelectExpandedPeripheryCases(
                0,
                4,
                unbudgeted.ShardedCaseCount + 10);

            Assert.AreEqual(5, budgeted.Cases.Count);
            Assert.AreEqual(unbudgeted.TotalCaseCount, budgeted.TotalCaseCount);
            Assert.AreEqual(unbudgeted.ShardedCaseCount, budgeted.ShardedCaseCount);
            CollectionAssert.AreEqual(
                CanonicalKeys(unbudgeted.Cases.Take(5)),
                CanonicalKeys(budgeted.Cases));
            Assert.AreEqual(0, zeroBudget.Cases.Count);
            Assert.AreEqual(unbudgeted.ShardedCaseCount, overBudget.Cases.Count);
        }

        [TestMethod]
        public void ExpandedSuites_DoNotContainDuplicateCanonicalOptionKeys()
        {
            IReadOnlyList<ApmwFuzzCase> expandedCases = ApmwFuzzSuite.ExpandedPeripheryCases();

            Assert.IsTrue(expandedCases.Count > ApmwPairwiseCaseGenerator.PeripheryCases().Count);
            Assert.AreEqual(0, ApmwFuzzSuite.RegressionCases().Count);
            AssertNoDuplicateCanonicalKeys(expandedCases);
            AssertNoDuplicateCanonicalKeys(ApmwFuzzSuite.CiExpandedPeripheryPairwiseSubset().Cases);
        }

        [TestMethod]
        public void CiExpandedPeripheryPairwiseSubset_RunItemGeneration()
        {
            ApmwFuzzSuiteSelection selection = ApmwFuzzSuite.CiExpandedPeripheryPairwiseSubset();

            Assert.AreEqual(ApmwFuzzSuite.CiExpandedCaseBudget, selection.Cases.Count);
            Assert.IsTrue(selection.Cases.Any(fuzzCase => fuzzCase.CaseName.StartsWith("pairwise-", StringComparison.Ordinal)));
            Assert.IsTrue(selection.Cases.Any(fuzzCase => fuzzCase.CaseName.StartsWith("interaction-", StringComparison.Ordinal)));

            RunSelection(selection, 1, ApmwFuzzStage.ItemHandlerGeneration);
        }

        [TestMethod]
        [Ignore("Local expanded APMW periphery suite entry point; enable manually to collect up to 10 failures.")]
        public void ExpandedPeripherySuite_AllCases_RunItemGeneration()
        {
            RunSelection(ApmwFuzzSuite.SelectExpandedPeripheryCases(0, 1, null), 10, ApmwFuzzStage.ItemHandlerGeneration);
        }

        [TestMethod]
        [Ignore("Nightly-style expanded APMW periphery shard 0/4; run all shard methods for complete coverage.")]
        public void ExpandedPeripherySuite_Shard0Of4_RunItemGeneration()
        {
            RunExpandedPeripheryShard(0, 4);
        }

        [TestMethod]
        [Ignore("Nightly-style expanded APMW periphery shard 1/4; run all shard methods for complete coverage.")]
        public void ExpandedPeripherySuite_Shard1Of4_RunItemGeneration()
        {
            RunExpandedPeripheryShard(1, 4);
        }

        [TestMethod]
        [Ignore("Nightly-style expanded APMW periphery shard 2/4; run all shard methods for complete coverage.")]
        public void ExpandedPeripherySuite_Shard2Of4_RunItemGeneration()
        {
            RunExpandedPeripheryShard(2, 4);
        }

        [TestMethod]
        [Ignore("Nightly-style expanded APMW periphery shard 3/4; run all shard methods for complete coverage.")]
        public void ExpandedPeripherySuite_Shard3Of4_RunItemGeneration()
        {
            RunExpandedPeripheryShard(3, 4);
        }

        private static void RunExpandedPeripheryShard(int shardIndex, int shardCount)
        {
            RunSelection(
                ApmwFuzzSuite.SelectExpandedPeripheryCases(shardIndex, shardCount, null),
                10,
                ApmwFuzzStage.ItemHandlerGeneration);
        }

        private static void RunSelection(ApmwFuzzSuiteSelection selection, int maxFailures, ApmwFuzzStage finalStage)
        {
            ApmwFuzzRunResult result = ApmwFuzzRunner.RunCases(selection.Cases, maxFailures, finalStage);
            if (!result.Succeeded)
                Assert.Fail(selection.ToDiagnosticString() + Environment.NewLine + result.ToFailureMessage());
        }

        private static void AssertNoDuplicateCanonicalKeys(IEnumerable<ApmwFuzzCase> cases)
        {
            string[] duplicates = cases
                .GroupBy(fuzzCase => fuzzCase.CanonicalOptionKey)
                .Where(group => group.Count() > 1)
                .Select(group => string.Join(", ", group.Select(fuzzCase => fuzzCase.CaseName)))
                .ToArray();

            Assert.AreEqual(0, duplicates.Length, "Duplicate expanded fuzz case keys: " + string.Join("; ", duplicates));
        }

        private static string[] CanonicalKeys(IEnumerable<ApmwFuzzCase> cases)
        {
            return cases.Select(fuzzCase => fuzzCase.CanonicalOptionKey).ToArray();
        }
    }
}