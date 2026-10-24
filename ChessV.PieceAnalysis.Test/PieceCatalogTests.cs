using ChessV.Games;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ChessV.PieceAnalysis.Test
{
    [TestClass]
    public class PieceCatalogTests
    {
        [TestMethod]
        public void References_IncludeEveryCanonicalPieceWithoutInventingTiers()
        {
            var catalog = ApmwPieceCatalog.Create();
            var pieces = catalog.Kings.Concat(catalog.Pawns).Concat(catalog.Sergeants)
                .Concat(catalog.Minors).Concat(catalog.Majors).Concat(catalog.Jacks)
                .Concat(catalog.Queens).Concat(catalog.Amazons).ToList();

            Assert.AreEqual(pieces.Count, TierReferenceData.Entries.Count);
            foreach (var piece in pieces)
            {
                var reference = TierReferenceData.Entries[piece.GetType().Name];
                Assert.AreEqual(piece.MidgameValue, reference.MidgameValue, reference.PieceTypeName);
                Assert.AreEqual(piece.EndgameValue, reference.EndgameValue, reference.PieceTypeName);
                Assert.AreEqual(piece.Notation[0], reference.Notation, reference.PieceTypeName);
            }
            Assert.IsNull(TierReferenceData.Entries["Pawn"].Tier);
            Assert.IsNull(TierReferenceData.Entries["King"].Tier);
        }

        [TestMethod]
        public void Catalog_PreservesCuratedDefaultAndSeparatelyExposesFullApmwRoster()
        {
            var catalog = new PieceCatalog(TierReferenceData.Entries);

            CollectionAssert.DoesNotContain(catalog.GetDefaultRosterPieceNames().ToList(), "Pawn");
            CollectionAssert.Contains(catalog.GetApmwRosterPieceNames().ToList(), "Pawn");
            CollectionAssert.Contains(catalog.GetApmwRosterPieceNames().ToList(), "King");
        }

        [TestMethod]
        public void ConstructPiece_TierHintDoesNotInventMaterialForUnknownPiece()
        {
            var catalog = new PieceCatalog(TierReferenceData.Entries);
            Assert.IsTrue(catalog.TryResolvePieceType("Ferz", out var pieceType));
            var piece = catalog.ConstructPiece(pieceType, PieceTier.Minor, out var ignoredHint);

            Assert.AreEqual(PieceTier.Minor, piece.Tier.Value);
            Assert.AreEqual(0, piece.MidgameValue);
            Assert.IsNull(piece.MaterialValueSource);
            Assert.IsNull(ignoredHint);
            var statistics = new MobilityAnalyzer().Analyze(piece, 8, 8, MobilityAnalyzer.DefaultDensityPercents);
            var assessment = new MaterialAssessment().Assess(new[] { statistics }, new MobilityStatistics[0]).Single();
            Assert.IsFalse(assessment.Warnings.Any(warning => warning.Type == "value_tier_mismatch"));
        }
    }
}
