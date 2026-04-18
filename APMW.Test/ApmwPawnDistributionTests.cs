using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Archipelago.APChessV;
using Archipelago.MultiClient.Net.Helpers;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using ChessV.Games.Pieces.Berolina;
using Moq;

namespace ChessV.Test
{
    [TestClass]
    public class ApmwPawnDistributionTests
    {
        private ItemHandler handler;
        private const int NUM_FILES = 8;

        [TestInitialize]
        public void Setup()
        {
            var core = ApmwCore.getInstance();
            core.pawns = new System.Collections.Generic.HashSet<PieceType>
            {
                new Pawn("Pawn", "P", 100, 125),
                new BerolinaPawn("Berolina Pawn", "Ŕ", 85, 120),
                new ChessV.Games.Pieces.Apmw.Checkers("Checkers", "Ç", 40, 95),
            };
            core.sergeants = new System.Collections.Generic.HashSet<PieceType>
            {
                new ChessV.Games.Pieces.Apmw.Sergeant("Sergeant", "Ŝ", 200, 225),
            };
            core.foundPawnForwardness = 0;

            var helper = new Mock<IReceivedItemsHelper>();
            helper.SetupGet(h => h.AllItemsReceived)
                .Returns(new System.Collections.ObjectModel.ReadOnlyCollection<Archipelago.MultiClient.Net.Models.ItemInfo>(
                    new System.Collections.Generic.List<Archipelago.MultiClient.Net.Models.ItemInfo>()));
            handler = new ItemHandler(helper.Object);
        }

        // GeneratePawns expects `minors` to contain at least 2*numFiles entries:
        //   indices 0..numFiles-1     = back rank (passed through unchanged)
        //   indices numFiles..2*nF-1  = initial pawn rank (filled by GeneratePawns)
        // Output is 5*numFiles long: [back, pawn rank, rank3, rank4, rank5].
        private static System.Collections.Generic.List<PieceType> EmptyMinors()
        {
            return Enumerable.Repeat<PieceType>(null, NUM_FILES * 2).ToList();
        }

        // GeneratePawns is value-targeted, not count-targeted: PickPawns spends an
        // adjustedPawnValues budget (foundPawns * PAWN_VALUE + spare_material + 45),
        // so the actual placed-pawn count typically exceeds foundPawns by a small margin
        // depending on which (cheap/expensive) pawn variants the RNG selects.
        // These tests therefore assert structural invariants rather than exact counts.

        // Returns the count of non-null entries in result rank `rankIndex` (0=back, 1=pawn, ...).
        private static int CountInRank(System.Collections.Generic.List<PieceType> result, int rankIndex)
        {
            return result.Skip(rankIndex * NUM_FILES).Take(NUM_FILES).Count(x => x != null);
        }

        [TestMethod]
        public void TestPawnDistribution_BasicCase()
        {
            // 8 pawns: pawn rank should fill first; back rank stays empty; minimal forward spill.
            var minors = EmptyMinors();
            ApmwCore.getInstance().foundPawns = 8;

            var result = handler.GeneratePawns(NUM_FILES, minors, 0);

            Assert.AreEqual(0, CountInRank(result, 0), "back rank untouched");
            Assert.AreEqual(NUM_FILES, CountInRank(result, 1), "pawn rank should be full");
            Assert.IsTrue(CountInRank(result, 2) <= NUM_FILES / 2, "minimal spill onto third rank");
            Assert.AreEqual(0, CountInRank(result, 3), "no pawns in fourth rank");
            Assert.AreEqual(0, CountInRank(result, 4), "no pawns in fifth rank");
        }

        [TestMethod]
        public void TestPawnDistribution_PartialFirstRank()
        {
            // 4 pawns: should place ~4 in the pawn rank only; nothing forward.
            var minors = EmptyMinors();
            ApmwCore.getInstance().foundPawns = 4;

            var result = handler.GeneratePawns(NUM_FILES, minors, 0);

            Assert.IsTrue(CountInRank(result, 1) >= 4, "at least foundPawns in pawn rank");
            Assert.IsTrue(CountInRank(result, 1) <= NUM_FILES, "pawn rank not over-filled");
            Assert.AreEqual(0, CountInRank(result, 2), "no spill onto third rank");
            Assert.AreEqual(0, CountInRank(result, 0), "back rank untouched");
        }

        [TestMethod]
        public void TestPawnDistribution_MultipleRanks()
        {
            // 20 pawns: pawn and third ranks fill, fourth rank gets the remainder.
            var minors = EmptyMinors();
            ApmwCore.getInstance().foundPawns = 20;

            var result = handler.GeneratePawns(NUM_FILES, minors, 0);

            Assert.AreEqual(NUM_FILES, CountInRank(result, 1), "pawn rank full");
            Assert.AreEqual(NUM_FILES, CountInRank(result, 2), "third rank full");
            Assert.IsTrue(CountInRank(result, 3) >= 4, "fourth rank carries the remainder");
            Assert.IsTrue(CountInRank(result, 3) < NUM_FILES, "fourth rank not full");
            Assert.AreEqual(0, CountInRank(result, 4), "fifth rank empty");
        }

        [TestMethod]
        public void TestPawnDistribution_WithExistingPieces()
        {
            // Existing piece in pawn rank is preserved; new pawns fill remaining slots and spill forward.
            var minors = EmptyMinors();
            var existing = MockPieceType();
            minors[NUM_FILES] = existing; // first slot of the pawn rank
            ApmwCore.getInstance().foundPawns = 8;

            var result = handler.GeneratePawns(NUM_FILES, minors, 0);

            Assert.AreEqual(NUM_FILES, CountInRank(result, 1), "pawn rank full");
            Assert.AreEqual(1, result.Skip(NUM_FILES).Take(NUM_FILES).Count(x => x == existing),
                "existing piece preserved exactly once in pawn rank");
            Assert.AreEqual(0, CountInRank(result, 0), "back rank untouched");
        }

        [TestMethod]
        public void TestPawnDistribution_MaximumPawns()
        {
            // 32 pawns fills pawn/third/fourth/fifth ranks (4 * numFiles slots).
            var minors = EmptyMinors();
            ApmwCore.getInstance().foundPawns = NUM_FILES * 4;

            var result = handler.GeneratePawns(NUM_FILES, minors, 0);

            for (int rank = 1; rank <= 4; rank++)
            {
                Assert.AreEqual(NUM_FILES, CountInRank(result, rank),
                    $"rank index {rank} should be full");
            }
            Assert.AreEqual(0, CountInRank(result, 0), "back rank untouched");
        }

        private PieceType MockPieceType()
        {
            // Create a mock piece type for testing
            return new Pawn("Test Pawn", "T", 100, 100);
        }
    }
} 