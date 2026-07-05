using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace ChessV.PieceAnalysis.Test
{
    [TestClass]
    public class MaterialAssessmentTests
    {
        [TestMethod]
        public void Assess_SingleMemberCohortProducesMedianPercentiles()
        {
            var target = PieceAnalysisTestHelpers.CreateStatistics("Target", null, 10.0, 4.0, 3.0);
            var cohort = new[] { PieceAnalysisTestHelpers.CreateStatistics("Peer", null, 10.0, 4.0, 3.0) };

            var assessed = new MaterialAssessment().Assess(new[] { target }, cohort).Single();

            Assert.AreEqual(50.0, assessed.Percentiles.VsAllPieces.AverageMobilityAtDensity10.Value, 0.0000001);
            Assert.AreEqual(50.0, assessed.Percentiles.VsAllPieces.AverageDirectionsAttacked.Value, 0.0000001);
            Assert.AreEqual(50.0, assessed.Percentiles.VsAllPieces.AverageSafeChecks.Value, 0.0000001);
        }

        [TestMethod]
        public void Assess_AllEqualCohortProducesMedianPercentiles()
        {
            var target = PieceAnalysisTestHelpers.CreateStatistics("Target", null, 10.0, 4.0, 3.0);
            var cohort = new[]
            {
                PieceAnalysisTestHelpers.CreateStatistics("Peer1", null, 10.0, 4.0, 3.0),
                PieceAnalysisTestHelpers.CreateStatistics("Peer2", null, 10.0, 4.0, 3.0),
                PieceAnalysisTestHelpers.CreateStatistics("Peer3", null, 10.0, 4.0, 3.0),
                PieceAnalysisTestHelpers.CreateStatistics("Peer4", null, 10.0, 4.0, 3.0),
            };

            var assessed = new MaterialAssessment().Assess(new[] { target }, cohort).Single();

            Assert.AreEqual(50.0, assessed.Percentiles.VsAllPieces.AverageMobilityAtDensity10.Value, 0.0000001);
            Assert.AreEqual(50.0, assessed.Percentiles.VsAllPieces.AverageDirectionsAttacked.Value, 0.0000001);
            Assert.AreEqual(50.0, assessed.Percentiles.VsAllPieces.AverageSafeChecks.Value, 0.0000001);
        }

        [DataTestMethod]
        [DataRow(5.0, 0.0)]
        [DataRow(20.0, 37.5)]
        [DataRow(45.0, 100.0)]
        public void Assess_PercentilesHandleBelowExactAndAboveTargets(double targetMobility, double expectedPercentile)
        {
            var target = PieceAnalysisTestHelpers.CreateStatistics("Target", null, targetMobility, 2.0, 2.0);
            var cohort = new[]
            {
                PieceAnalysisTestHelpers.CreateStatistics("Peer1", null, 10.0, 2.0, 2.0),
                PieceAnalysisTestHelpers.CreateStatistics("Peer2", null, 20.0, 2.0, 2.0),
                PieceAnalysisTestHelpers.CreateStatistics("Peer3", null, 30.0, 2.0, 2.0),
                PieceAnalysisTestHelpers.CreateStatistics("Peer4", null, 40.0, 2.0, 2.0),
            };

            var assessed = new MaterialAssessment().Assess(new[] { target }, cohort).Single();

            Assert.AreEqual(expectedPercentile, assessed.Percentiles.VsAllPieces.AverageMobilityAtDensity10.Value, 0.0000001);
        }

        [TestMethod]
        public void Assess_DoesNotFlagStatisticalOutlier_WhenTierHasFewerThanFourPeers()
        {
            var target = PieceAnalysisTestHelpers.CreateStatistics("Target", PieceTier.Queen, 12.0, 8.0, 10.0, 900, 900);
            var peers = new[]
            {
                PieceAnalysisTestHelpers.CreateStatistics("Peer1", PieceTier.Queen, 10.0, 8.0, 10.0, 900, 900),
                PieceAnalysisTestHelpers.CreateStatistics("Peer2", PieceTier.Queen, 12.0, 8.0, 10.0, 900, 900),
                PieceAnalysisTestHelpers.CreateStatistics("Peer3", PieceTier.Queen, 14.0, 8.0, 10.0, 900, 900),
            };

            var assessed = new MaterialAssessment().Assess(new[] { target }, peers).Single();

            Assert.IsFalse(assessed.Warnings.Any(warning => warning.Type == "statistical_outlier"));
        }

        [TestMethod]
        public void Assess_DoesNotFlagStatisticalOutlier_WhenCandidateIsOnIqrFence()
        {
            var target = PieceAnalysisTestHelpers.CreateStatistics("Target", PieceTier.Queen, 19.0, 8.0, 10.0, 900, 900);
            var peers = new[]
            {
                PieceAnalysisTestHelpers.CreateStatistics("Peer1", PieceTier.Queen, 10.0, 8.0, 10.0, 900, 900),
                PieceAnalysisTestHelpers.CreateStatistics("Peer2", PieceTier.Queen, 12.0, 8.0, 10.0, 900, 900),
                PieceAnalysisTestHelpers.CreateStatistics("Peer3", PieceTier.Queen, 14.0, 8.0, 10.0, 900, 900),
                PieceAnalysisTestHelpers.CreateStatistics("Peer4", PieceTier.Queen, 16.0, 8.0, 10.0, 900, 900),
            };

            var assessed = new MaterialAssessment().Assess(new[] { target }, peers).Single();

            Assert.IsFalse(assessed.Warnings.Any(warning => warning.Type == "statistical_outlier"));
        }

        [TestMethod]
        public void Assess_FlagsStatisticalOutlier_WhenCandidateFallsOutsideIqrFence()
        {
            var target = PieceAnalysisTestHelpers.CreateStatistics("Target", PieceTier.Queen, 19.01, 8.0, 10.0, 900, 900);
            var peers = new[]
            {
                PieceAnalysisTestHelpers.CreateStatistics("Peer1", PieceTier.Queen, 10.0, 8.0, 10.0, 900, 900),
                PieceAnalysisTestHelpers.CreateStatistics("Peer2", PieceTier.Queen, 12.0, 8.0, 10.0, 900, 900),
                PieceAnalysisTestHelpers.CreateStatistics("Peer3", PieceTier.Queen, 14.0, 8.0, 10.0, 900, 900),
                PieceAnalysisTestHelpers.CreateStatistics("Peer4", PieceTier.Queen, 16.0, 8.0, 10.0, 900, 900),
            };

            var assessed = new MaterialAssessment().Assess(new[] { target }, peers).Single();
            var warning = assessed.Warnings.Single(item => item.Type == "statistical_outlier");

            StringAssert.Contains(warning.Message, "density 10 value 19.01");
            StringAssert.Contains(warning.Message, "expected within [7.00, 19.00]");
        }

        [TestMethod]
        public void Assess_FlagsAbsoluteThreshold_WhenMinorSafeChecksAreTooLow()
        {
            var target = PieceAnalysisTestHelpers.CreateStatistics("Target", PieceTier.Minor, 5.0, 5.0, 2.5, 300, 300);

            var assessed = new MaterialAssessment().Assess(new[] { target }, new MobilityStatistics[0]).Single();

            var warning = assessed.Warnings.Single(item => item.Type == "absolute_threshold");
            StringAssert.Contains(warning.Message, "averageSafeChecks 2.50");
        }

        [TestMethod]
        public void Assess_DoesNotFlagAbsoluteThreshold_WhenMinorPieceFitsConfiguredRange()
        {
            var target = PieceAnalysisTestHelpers.CreateStatistics("Target", PieceTier.Minor, 5.0, 5.0, 3.0, 300, 300);

            var assessed = new MaterialAssessment().Assess(new[] { target }, new MobilityStatistics[0]).Single();

            Assert.IsFalse(assessed.Warnings.Any(item => item.Type == "absolute_threshold"));
        }

        [TestMethod]
        public void Assess_FlagsValueTierMismatch_WhenMaterialFitsAnotherTierMuchBetter()
        {
            var target = PieceAnalysisTestHelpers.CreateStatistics("Target", PieceTier.Minor, 5.0, 5.0, 3.0, 1300, 1300);

            var assessed = new MaterialAssessment().Assess(new[] { target }, new MobilityStatistics[0]).Single();

            var warning = assessed.Warnings.Single(item => item.Type == "value_tier_mismatch");
            Assert.AreEqual("info", warning.Severity);
            StringAssert.Contains(warning.Message, "closer to the Amazon baseline (1300) than its assigned Minor baseline (300)");
        }

        [TestMethod]
        public void Assess_DoesNotFlagValueTierMismatch_WhenMaterialMatchesAssignedTier()
        {
            var target = PieceAnalysisTestHelpers.CreateStatistics("Target", PieceTier.Minor, 5.0, 5.0, 3.0, 320, 320);

            var assessed = new MaterialAssessment().Assess(new[] { target }, new MobilityStatistics[0]).Single();

            Assert.IsFalse(assessed.Warnings.Any(item => item.Type == "value_tier_mismatch"));
        }

        [TestMethod]
        public void Assess_RealPiecesFlagKnownSuspiciousAssignmentsWithoutNoisyFalsePositives()
        {
            var catalog = new PieceCatalog(TierReferenceData.Entries);
            var analyzer = new MobilityAnalyzer();
            var assessment = new MaterialAssessment();
            var requestedPieces = new[]
            {
                "ShortRook",
                "Cannon",
                "Vao",
                "Phoenix",
                "Scout",
                "Queen",
                "Knight",
                "Rook",
            }
            .Select(PieceAnalysisTestHelpers.ConstructKnownPiece)
            .ToList();

            var requestedStatistics = analyzer.Analyze(requestedPieces, 8, 8, MobilityAnalyzer.DefaultDensityPercents);
            var assessedPieces = assessment.Assess(requestedStatistics, PieceAnalysisTestHelpers.AnalyzeReferencePieces())
                .ToDictionary(piece => piece.Statistics.SourcePiece.RequestedName);

            CollectionAssert.AreEquivalent(
                new[] { "absolute_threshold", "statistical_outlier", "value_tier_mismatch" },
                assessedPieces["ShortRook"].Warnings.Select(warning => warning.Type).OrderBy(value => value).ToArray());
            CollectionAssert.AreEquivalent(
                new[] { "absolute_threshold", "absolute_threshold", "statistical_outlier" },
                assessedPieces["Cannon"].Warnings.Select(warning => warning.Type).OrderBy(value => value).ToArray());
            CollectionAssert.AreEquivalent(
                new[] { "absolute_threshold", "absolute_threshold", "statistical_outlier" },
                assessedPieces["Vao"].Warnings.Select(warning => warning.Type).OrderBy(value => value).ToArray());
            CollectionAssert.AreEquivalent(
                new[] { "absolute_threshold" },
                assessedPieces["Phoenix"].Warnings.Select(warning => warning.Type).ToArray());
            CollectionAssert.AreEquivalent(
                new[] { "absolute_threshold" },
                assessedPieces["Scout"].Warnings.Select(warning => warning.Type).ToArray());
            CollectionAssert.AreEquivalent(
                new[] { "statistical_outlier" },
                assessedPieces["Queen"].Warnings.Select(warning => warning.Type).ToArray());
            Assert.AreEqual(0, assessedPieces["Knight"].Warnings.Count);
            Assert.AreEqual(0, assessedPieces["Rook"].Warnings.Count);
        }
    }
}
