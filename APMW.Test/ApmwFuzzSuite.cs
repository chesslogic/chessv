using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace ChessV.Test
{
    internal sealed class ApmwFuzzSuiteSelection
    {
        public ApmwFuzzSuiteSelection(
            string suiteName,
            int shardIndex,
            int shardCount,
            int? caseBudget,
            int totalCaseCount,
            int shardedCaseCount,
            IEnumerable<ApmwFuzzCase> cases)
        {
            if (string.IsNullOrWhiteSpace(suiteName))
                throw new ArgumentException("Fuzz suite names must be specified.", nameof(suiteName));
            ValidateShardArguments(shardIndex, shardCount);
            ValidateCaseBudget(caseBudget);
            if (totalCaseCount < 0)
                throw new ArgumentOutOfRangeException(nameof(totalCaseCount), "Total case count cannot be negative.");
            if (shardedCaseCount < 0 || shardedCaseCount > totalCaseCount)
                throw new ArgumentOutOfRangeException(nameof(shardedCaseCount), "Sharded case count must be between zero and total case count.");
            if (cases == null)
                throw new ArgumentNullException(nameof(cases));

            SuiteName = suiteName;
            ShardIndex = shardIndex;
            ShardCount = shardCount;
            CaseBudget = caseBudget;
            TotalCaseCount = totalCaseCount;
            ShardedCaseCount = shardedCaseCount;
            Cases = new ReadOnlyCollection<ApmwFuzzCase>(cases.ToList());

            if (Cases.Any(fuzzCase => fuzzCase == null))
                throw new ArgumentException("Fuzz suite selections cannot contain null cases.", nameof(cases));
        }

        public string SuiteName { get; }

        public int ShardIndex { get; }

        public int ShardCount { get; }

        public int? CaseBudget { get; }

        public int TotalCaseCount { get; }

        public int ShardedCaseCount { get; }

        public IReadOnlyList<ApmwFuzzCase> Cases { get; }

        public string DiagnosticLabel
        {
            get
            {
                return SuiteName + " shard " + ShardIndex.ToString(CultureInfo.InvariantCulture) +
                    " of " + ShardCount.ToString(CultureInfo.InvariantCulture) +
                    ", caseBudget=" + BudgetLabel(CaseBudget);
            }
        }

        public string ToDiagnosticString()
        {
            return "APMW fuzz suite '" + SuiteName + "' selected " +
                Cases.Count.ToString(CultureInfo.InvariantCulture) + " case(s) from " +
                ShardedCaseCount.ToString(CultureInfo.InvariantCulture) + " sharded case(s) out of " +
                TotalCaseCount.ToString(CultureInfo.InvariantCulture) + " canonical case(s); shardIndex=" +
                ShardIndex.ToString(CultureInfo.InvariantCulture) + ", shardCount=" +
                ShardCount.ToString(CultureInfo.InvariantCulture) + ", caseBudget=" +
                BudgetLabel(CaseBudget) + ". Rerun helper: ApmwFuzzSuite.SelectExpandedPeripheryCases(" +
                ShardIndex.ToString(CultureInfo.InvariantCulture) + ", " +
                ShardCount.ToString(CultureInfo.InvariantCulture) + ", " +
                BudgetArgument(CaseBudget) + ").";
        }

        public override string ToString()
        {
            return ToDiagnosticString();
        }

        internal static void ValidateShardArguments(int shardIndex, int shardCount)
        {
            if (shardCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(shardCount), "Shard count must be greater than zero.");
            if (shardIndex < 0 || shardIndex >= shardCount)
                throw new ArgumentOutOfRangeException(nameof(shardIndex), "Shard index must be between zero and shard count minus one.");
        }

        internal static void ValidateCaseBudget(int? caseBudget)
        {
            if (caseBudget.HasValue && caseBudget.Value < 0)
                throw new ArgumentOutOfRangeException(nameof(caseBudget), "Case budget cannot be negative.");
        }

        private static string BudgetLabel(int? caseBudget)
        {
            return caseBudget.HasValue
                ? caseBudget.Value.ToString(CultureInfo.InvariantCulture)
                : "unbounded";
        }

        private static string BudgetArgument(int? caseBudget)
        {
            return caseBudget.HasValue
                ? caseBudget.Value.ToString(CultureInfo.InvariantCulture)
                : "null";
        }
    }

    internal static class ApmwFuzzSuite
    {
        public const int CiExpandedShardIndex = 0;
        public const int CiExpandedShardCount = 4;
        public const int CiExpandedCaseBudget = 12;

        private const string ExpandedPeripherySuiteName = "expanded-periphery";
        private const string CiExpandedPeripherySuiteName = "ci-expanded-periphery-pairwise";

        public static IReadOnlyList<ApmwFuzzCase> RegressionCases()
        {
            return Array.Empty<ApmwFuzzCase>();
        }

        public static IReadOnlyList<ApmwFuzzCase> ExpandedPeripheryCases()
        {
            return new ReadOnlyCollection<ApmwFuzzCase>(
                MaterializeCanonicalCases(BuildExpandedPeripheryCases()).ToList());
        }

        public static ApmwFuzzSuiteSelection CiExpandedPeripheryPairwiseSubset()
        {
            return SelectExpandedPeripheryCases(
                CiExpandedShardIndex,
                CiExpandedShardCount,
                CiExpandedCaseBudget,
                CiExpandedPeripherySuiteName);
        }

        public static ApmwFuzzSuiteSelection SelectExpandedPeripheryCases(
            int shardIndex,
            int shardCount,
            int? caseBudget)
        {
            return SelectExpandedPeripheryCases(shardIndex, shardCount, caseBudget, ExpandedPeripherySuiteName);
        }

        public static ApmwFuzzSuiteSelection SelectCases(
            string suiteName,
            IEnumerable<ApmwFuzzCase> cases,
            int shardIndex,
            int shardCount,
            int? caseBudget)
        {
            ApmwFuzzSuiteSelection.ValidateShardArguments(shardIndex, shardCount);
            ApmwFuzzSuiteSelection.ValidateCaseBudget(caseBudget);

            List<ApmwFuzzCase> canonicalCases = MaterializeCanonicalCases(cases).ToList();
            List<ApmwFuzzCase> shardedCases = ShardMaterializedCases(canonicalCases, shardIndex, shardCount).ToList();
            List<ApmwFuzzCase> selectedCases = ApplyBudgetToMaterializedCases(shardedCases, caseBudget).ToList();

            return new ApmwFuzzSuiteSelection(
                suiteName,
                shardIndex,
                shardCount,
                caseBudget,
                canonicalCases.Count,
                shardedCases.Count,
                selectedCases);
        }

        public static IReadOnlyList<ApmwFuzzCase> ShardCases(
            IEnumerable<ApmwFuzzCase> cases,
            int shardIndex,
            int shardCount)
        {
            ApmwFuzzSuiteSelection.ValidateShardArguments(shardIndex, shardCount);
            List<ApmwFuzzCase> canonicalCases = MaterializeCanonicalCases(cases).ToList();
            return new ReadOnlyCollection<ApmwFuzzCase>(
                ShardMaterializedCases(canonicalCases, shardIndex, shardCount).ToList());
        }

        public static IReadOnlyList<ApmwFuzzCase> ApplyCaseBudget(
            IEnumerable<ApmwFuzzCase> cases,
            int? caseBudget)
        {
            ApmwFuzzSuiteSelection.ValidateCaseBudget(caseBudget);
            List<ApmwFuzzCase> canonicalCases = MaterializeCanonicalCases(cases).ToList();
            return new ReadOnlyCollection<ApmwFuzzCase>(
                ApplyBudgetToMaterializedCases(canonicalCases, caseBudget).ToList());
        }

        private static ApmwFuzzSuiteSelection SelectExpandedPeripheryCases(
            int shardIndex,
            int shardCount,
            int? caseBudget,
            string suiteName)
        {
            return SelectCases(suiteName, ExpandedPeripheryCases(), shardIndex, shardCount, caseBudget);
        }

        private static IEnumerable<ApmwFuzzCase> BuildExpandedPeripheryCases()
        {
            return RegressionCases()
                .Concat(ApmwPairwiseCaseGenerator.PeripheryCases())
                .Concat(ApmwFuzzCaseGenerator.OverCapItemCases())
                .Concat(ApmwFuzzCaseGenerator.BoundaryCases())
                .Concat(ApmwFuzzCaseGenerator.SmokeCases());
        }

        private static IEnumerable<ApmwFuzzCase> MaterializeCanonicalCases(IEnumerable<ApmwFuzzCase> cases)
        {
            if (cases == null)
                throw new ArgumentNullException(nameof(cases));

            var seenOptionKeys = new HashSet<string>(StringComparer.Ordinal);
            int caseIndex = 0;
            foreach (ApmwFuzzCase fuzzCase in cases)
            {
                if (fuzzCase == null)
                    throw new ArgumentException(
                        "Fuzz suite cases cannot contain null entries at index " +
                        caseIndex.ToString(CultureInfo.InvariantCulture) + ".",
                        nameof(cases));

                if (seenOptionKeys.Add(fuzzCase.CanonicalOptionKey))
                    yield return fuzzCase;

                caseIndex++;
            }
        }

        private static IEnumerable<ApmwFuzzCase> ShardMaterializedCases(
            IReadOnlyList<ApmwFuzzCase> cases,
            int shardIndex,
            int shardCount)
        {
            for (int index = 0; index < cases.Count; index++)
            {
                if (index % shardCount == shardIndex)
                    yield return cases[index];
            }
        }

        private static IEnumerable<ApmwFuzzCase> ApplyBudgetToMaterializedCases(
            IEnumerable<ApmwFuzzCase> cases,
            int? caseBudget)
        {
            return caseBudget.HasValue ? cases.Take(caseBudget.Value) : cases;
        }
    }
}