using System.Collections.Generic;
using System.Linq;

namespace ChessV.Test
{
    [TestClass]
    public class ApmwFuzzOptionSpaceTests
    {
        [TestMethod]
        public void NumericBand_BuildsPeripheryValuesWithoutFullRange()
        {
            var band = new ApmwFuzzNumericBand("test-band", 3, 5, 8, 13);

            CollectionAssert.AreEqual(
                new[] { 0, 1, 3, 4, 5, 6, 8, 13 },
                band.Values.ToArray());
            CollectionAssert.DoesNotContain(band.Values.ToList(), 2);
            CollectionAssert.DoesNotContain(band.Values.ToList(), 7);
        }

        [TestMethod]
        public void NumericAxis_DeduplicatesPeripheryValuesDeterministically()
        {
            var axis = ApmwFuzzAxis.Numeric(
                "count",
                1,
                new ApmwFuzzNumericBand("tiny-cap", 1, 1, 2, 2, 1));

            CollectionAssert.AreEqual(
                new[] { "0", "1", "2" },
                axis.Values.Select(value => value.CanonicalKey).ToArray());
            Assert.AreEqual("1", axis.DefaultValue.CanonicalKey);
        }

        [TestMethod]
        public void SetAxis_CanonicalizesAndDeduplicatesEquivalentValues()
        {
            ApmwFuzzOptionValue unsorted = ApmwFuzzOptionValue.IntSet("unsorted", new[] { 2, 1, 2 });
            ApmwFuzzOptionValue sorted = ApmwFuzzOptionValue.IntSet("sorted", new[] { 1, 2 });
            var axis = ApmwFuzzAxis.IntSet(
                "army",
                "default",
                new[] { 0, 1 },
                unsorted,
                sorted,
                ApmwFuzzOptionValue.IntSet("empty", Enumerable.Empty<int>()));

            Assert.AreEqual("1,2", unsorted.CanonicalKey);
            Assert.AreEqual(unsorted.CanonicalKey, sorted.CanonicalKey);
            CollectionAssert.AreEqual(
                new[] { "0,1", "1,2", "empty" },
                axis.Values.Select(value => value.CanonicalKey).ToArray());
        }

        [TestMethod]
        public void AssignmentCanonicalKey_UsesAxisOrderAndCanonicalValueKeys()
        {
            ApmwFuzzOptionValue unsorted = ApmwFuzzOptionValue.IntSet("unsorted", new[] { 2, 1, 2 });
            ApmwFuzzOptionValue sorted = ApmwFuzzOptionValue.IntSet("sorted", new[] { 1, 2 });
            var space = new ApmwFuzzOptionSpace(new[]
            {
                ApmwFuzzAxis.Boolean("flag", false),
                ApmwFuzzAxis.IntSet("army", "default", new[] { 0 }, unsorted),
            });

            ApmwFuzzAssignment first = space.DefaultAssignment()
                .With("flag", "true")
                .WithValue("army", unsorted);
            ApmwFuzzAssignment second = space.DefaultAssignment()
                .WithValue("army", sorted)
                .With("flag", "true");

            Assert.AreEqual("flag=true|army=1,2", first.CanonicalKey);
            Assert.AreEqual(first.CanonicalKey, second.CanonicalKey);
        }

        [TestMethod]
        public void CoverageTracker_RecordsValuesAndPairsDeterministically()
        {
            ApmwFuzzOptionSpace space = CreateSmallOptionSpace();
            ApmwFuzzAssignment defaultAssignment = space.DefaultAssignment();
            ApmwFuzzAssignment peripheryAssignment = defaultAssignment
                .With("flag", "true")
                .With("count", "3");

            ApmwFuzzCoverageSnapshot snapshot = RecordPair(space, defaultAssignment, peripheryAssignment);
            ApmwFuzzCoverageSnapshot repeatedSnapshot = RecordPair(
                space,
                defaultAssignment,
                defaultAssignment.With("count", "3").With("flag", "true"));

            Assert.AreEqual(2, snapshot.CaseCount);
            CollectionAssert.AreEqual(
                new[] { "count=1", "count=3", "flag=false", "flag=true" },
                snapshot.ValueHitCounts.Keys.ToArray());
            CollectionAssert.AreEqual(
                new[] { "flag=false|count=1", "flag=true|count=3" },
                snapshot.PairHitCounts.Keys.ToArray());
            Assert.AreEqual(1, snapshot.ValueHitCounts["flag=true"]);
            Assert.AreEqual(1, snapshot.ValueHitCounts["count=3"]);
            CollectionAssert.AreEqual(
                snapshot.ValueHitCounts.Keys.ToArray(),
                repeatedSnapshot.ValueHitCounts.Keys.ToArray());
            CollectionAssert.AreEqual(
                snapshot.ValueHitCounts.Values.ToArray(),
                repeatedSnapshot.ValueHitCounts.Values.ToArray());
            CollectionAssert.AreEqual(
                snapshot.PairHitCounts.Keys.ToArray(),
                repeatedSnapshot.PairHitCounts.Keys.ToArray());
        }

        [TestMethod]
        public void DefaultOptionSpace_ExposesApmwAxesBandsAndConstraints()
        {
            ApmwFuzzOptionSpace space = ApmwFuzzOptionSpace.CreateDefault();
            ApmwFuzzAxis pawnAxis = space.GetAxis(ApmwFuzzOptionSpace.AxisPawnCount);

            Assert.AreEqual(ApmwFuzzOptionAxisKind.Numeric, pawnAxis.Kind);
            Assert.AreEqual(2, pawnAxis.NumericBands.Count);
            CollectionAssert.Contains(pawnAxis.Values.Select(value => value.CanonicalKey).ToList(), "32");
            CollectionAssert.Contains(pawnAxis.Values.Select(value => value.CanonicalKey).ToList(), "33");
            CollectionAssert.Contains(pawnAxis.Values.Select(value => value.CanonicalKey).ToList(), "40");
            CollectionAssert.Contains(pawnAxis.Values.Select(value => value.CanonicalKey).ToList(), "41");
            CollectionAssert.DoesNotContain(pawnAxis.Values.Select(value => value.CanonicalKey).ToList(), "2");

            Assert.AreEqual(0, space.Validate(space.DefaultAssignment()).Count);

            IReadOnlyList<ApmwFuzzConstraintViolation> standardViolations = space.Validate(
                space.DefaultAssignment().With(ApmwFuzzOptionSpace.AxisSuperSizeMeCount, "1"));
            Assert.IsTrue(standardViolations.Any(
                violation => violation.Name == "standard-board-has-no-super-size-item"));

            IReadOnlyList<ApmwFuzzConstraintViolation> superViolations = space.Validate(
                space.DefaultAssignment().With(ApmwFuzzOptionSpace.AxisIsSuperSized, "true"));
            Assert.IsTrue(superViolations.Any(
                violation => violation.Name == "super-board-requests-super-size-item"));
        }

        private static ApmwFuzzOptionSpace CreateSmallOptionSpace()
        {
            return new ApmwFuzzOptionSpace(new[]
            {
                ApmwFuzzAxis.Boolean("flag", false),
                ApmwFuzzAxis.Numeric("count", 1, new ApmwFuzzNumericBand("count-band", 1, 2, 3)),
            });
        }

        private static ApmwFuzzCoverageSnapshot RecordPair(
            ApmwFuzzOptionSpace space,
            ApmwFuzzAssignment defaultAssignment,
            ApmwFuzzAssignment peripheryAssignment)
        {
            ApmwFuzzCoverageTracker tracker = space.CreateCoverageTracker();
            tracker.Record(peripheryAssignment);
            tracker.Record(defaultAssignment);
            return tracker.Snapshot();
        }
    }
}
