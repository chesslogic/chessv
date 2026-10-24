using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ChessV.PieceAnalysis.Test
{
    [TestClass]
    public class MobilityAnalyzerTests
    {
        [TestMethod]
        public void Analyze_KnightOnEightByEight_ReturnsConstantExpectedStatistics()
        {
            var analyzer = new MobilityAnalyzer();
            var knight = PieceAnalysisTestHelpers.ConstructKnownPiece("Knight");

            var statistics = analyzer.Analyze(knight, 8, 8, MobilityAnalyzer.DefaultDensityPercents);

            Assert.AreEqual(5.25, statistics.AverageDirectionsAttacked, 0.0000001);
            Assert.AreEqual(5.25, statistics.AverageSafeChecks, 0.0000001);
            CollectionAssert.AreEqual(
                new[] { 10, 15, 20, 25, 30, 35, 40, 45, 50 },
                statistics.MobilityByDensityPercent.Keys.OrderBy(value => value).ToArray());

            foreach (var entry in statistics.MobilityByDensityPercent)
                Assert.AreEqual(5.25, entry.Value, 0.0000001, $"Unexpected mobility at density {entry.Key}.");
        }

        [TestMethod]
        public void Analyze_RookOnEightByEight_ReturnsExpectedDensitySweep()
        {
            var analyzer = new MobilityAnalyzer();
            var rook = PieceAnalysisTestHelpers.ConstructKnownPiece("Rook");

            var statistics = analyzer.Analyze(rook, 8, 8, MobilityAnalyzer.DefaultDensityPercents);

            Assert.AreEqual(3.5, statistics.AverageDirectionsAttacked, 0.0000001);
            Assert.AreEqual(10.5, statistics.AverageSafeChecks, 0.0000001);
            Assert.AreEqual(6.0078125, statistics.MobilityByDensityPercent[50], 0.0000001);
            Assert.AreEqual(8.8009033203125, statistics.MobilityByDensityPercent[25], 0.0000001);
            Assert.AreEqual(11.5233605, statistics.MobilityByDensityPercent[10], 0.0000001);
        }

        [TestMethod]
        public void Analyze_RookTravelMatchesExactEmptyBoardDistances()
        {
            var statistics = new MobilityAnalyzer().Analyze(
                PieceAnalysisTestHelpers.ConstructKnownPiece("Rook"), 8, 8, MobilityAnalyzer.DefaultDensityPercents);

            Assert.AreEqual(8, statistics.Files);
            Assert.AreEqual(8, statistics.Ranks);
            Assert.IsNull(statistics.EmptyBoardTravel.UnsupportedReason);
            Assert.AreEqual(1.0, statistics.EmptyBoardTravel.ReachableSquareFraction.Value, 0.0000001);
            Assert.AreEqual(16.0 / 9.0, statistics.EmptyBoardTravel.AverageMovesToReachableSquare.Value, 0.0000001);
            Assert.AreEqual(1.0, statistics.EmptyBoardTravel.HomeToFarRankReachableFraction.Value, 0.0000001);
            Assert.AreEqual(15.0 / 8.0, statistics.EmptyBoardTravel.AverageMovesHomeToFarRank.Value, 0.0000001);
        }

        [TestMethod]
        public void Analyze_BishopTravelSeparatesUnreachableColorsFromMoveCounts()
        {
            var statistics = new MobilityAnalyzer().Analyze(
                PieceAnalysisTestHelpers.ConstructKnownPiece("Bishop"), 8, 8, MobilityAnalyzer.DefaultDensityPercents);

            Assert.AreEqual(31.0 / 63.0, statistics.EmptyBoardTravel.ReachableSquareFraction.Value, 0.0000001);
            Assert.AreEqual(0.5, statistics.EmptyBoardTravel.HomeToFarRankReachableFraction.Value, 0.0000001);
            Assert.IsTrue(statistics.EmptyBoardTravel.AverageMovesToReachableSquare.Value > 1.0);
            Assert.IsTrue(statistics.EmptyBoardTravel.AverageMovesToReachableSquare.Value < 2.0);
        }

        [TestMethod]
        public void Analyze_TransposedBoardsHaveEqualLocalMobilityButDifferentKnightTravel()
        {
            var analyzer = new MobilityAnalyzer();
            var tall = analyzer.Analyze(
                PieceAnalysisTestHelpers.ConstructKnownPiece("Knight"), 8, 12, MobilityAnalyzer.DefaultDensityPercents);
            var wide = analyzer.Analyze(
                PieceAnalysisTestHelpers.ConstructKnownPiece("Knight"), 12, 8, MobilityAnalyzer.DefaultDensityPercents);

            Assert.AreEqual(tall.MobilityByDensityPercent[10], wide.MobilityByDensityPercent[10], 0.0000001);
            Assert.AreEqual(tall.EmptyBoardTravel.AverageMovesToReachableSquare.Value,
                wide.EmptyBoardTravel.AverageMovesToReachableSquare.Value, 0.0000001);
            Assert.IsTrue(tall.EmptyBoardTravel.AverageMovesHomeToFarRank.Value >
                wide.EmptyBoardTravel.AverageMovesHomeToFarRank.Value);
        }

        [TestMethod]
        public void Analyze_PawnTravelUsesMoveOnlyCapabilitiesAndExcludesCaptures()
        {
            var statistics = new MobilityAnalyzer().Analyze(
                PieceAnalysisTestHelpers.ConstructKnownPiece("Pawn"), 8, 8, MobilityAnalyzer.DefaultDensityPercents);

            Assert.IsNull(statistics.EmptyBoardTravel.UnsupportedReason);
            Assert.AreEqual(1.0 / 8.0, statistics.EmptyBoardTravel.HomeToFarRankReachableFraction.Value, 0.0000001);
            Assert.AreEqual(7.0, statistics.EmptyBoardTravel.AverageMovesHomeToFarRank.Value, 0.0000001);
            Assert.IsTrue(statistics.AnalysisLimitations.Any(text => text.Contains("move-only")));
        }

        [TestMethod]
        public void Analyze_ScreenCapturePieceDoesNotClaimSupportedTravel()
        {
            var statistics = new MobilityAnalyzer().Analyze(
                PieceAnalysisTestHelpers.ConstructKnownPiece("Cannon"), 8, 8, MobilityAnalyzer.DefaultDensityPercents);

            Assert.IsNotNull(statistics.EmptyBoardTravel.UnsupportedReason);
            Assert.IsNull(statistics.EmptyBoardTravel.AverageMovesToReachableSquare);
            Assert.IsNull(statistics.EmptyBoardTravel.ReachableSquareFraction);
            Assert.IsTrue(statistics.AnalysisLimitations.Any(text => text.Contains("screens")));
        }
    }
}
