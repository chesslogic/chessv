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
    }
}
