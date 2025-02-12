using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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
            handler = new ItemHandler();
        }

        [TestMethod]
        public void TestPawnDistribution_BasicCase()
        {
            // Test with 8 pawns total
            var minors = Enumerable.Repeat<PieceType>(null, NUM_FILES).ToList();
            ApmwCore.getInstance().foundPawns = 8;
            
            var result = handler.GeneratePawns(NUM_FILES, minors);
            
            // Should all be in first rank
            Assert.AreEqual(8, result.Take(NUM_FILES).Count(x => x != null));
            Assert.AreEqual(0, result.Skip(NUM_FILES).Count(x => x != null));
        }

        [TestMethod]
        public void TestPawnDistribution_PartialFirstRank()
        {
            // Test with 4 pawns total
            var minors = Enumerable.Repeat<PieceType>(null, NUM_FILES).ToList();
            ApmwCore.getInstance().foundPawns = 4;
            
            var result = handler.GeneratePawns(NUM_FILES, minors);
            
            // Should all be in first rank
            Assert.AreEqual(4, result.Take(NUM_FILES).Count(x => x != null));
            Assert.AreEqual(0, result.Skip(NUM_FILES).Count(x => x != null));
        }

        [TestMethod]
        public void TestPawnDistribution_MultipleRanks()
        {
            // Test with 20 pawns total
            var minors = Enumerable.Repeat<PieceType>(null, NUM_FILES).ToList();
            ApmwCore.getInstance().foundPawns = 20;
            
            var result = handler.GeneratePawns(NUM_FILES, minors);
            
            // Should fill first two ranks and part of third
            Assert.AreEqual(8, result.Take(NUM_FILES).Count(x => x != null));
            Assert.AreEqual(8, result.Skip(NUM_FILES).Take(NUM_FILES).Count(x => x != null));
            Assert.AreEqual(4, result.Skip(NUM_FILES * 2).Take(NUM_FILES).Count(x => x != null));
        }

        [TestMethod]
        public void TestPawnDistribution_WithExistingPieces()
        {
            // Test with existing pieces in first rank
            var minors = Enumerable.Repeat<PieceType>(null, NUM_FILES).ToList();
            minors[0] = MockPieceType();  // Add a piece to first position
            ApmwCore.getInstance().foundPawns = 8;
            
            var result = handler.GeneratePawns(NUM_FILES, minors);
            
            // Should have 7 new pawns plus 1 existing piece in first rank
            Assert.AreEqual(8, result.Take(NUM_FILES).Count(x => x != null));
            Assert.AreEqual(1, result.Take(NUM_FILES).Count(x => x == minors[0]));
        }

        [TestMethod]
        public void TestPawnDistribution_MaximumPawns()
        {
            // Test with maximum possible pawns (32 for 8x8 board)
            var minors = Enumerable.Repeat<PieceType>(null, NUM_FILES).ToList();
            ApmwCore.getInstance().foundPawns = NUM_FILES * 4;
            
            var result = handler.GeneratePawns(NUM_FILES, minors);
            
            // Should fill all ranks evenly
            for (int rank = 0; rank < 4; rank++)
            {
                Assert.AreEqual(NUM_FILES, 
                    result.Skip(rank * NUM_FILES).Take(NUM_FILES).Count(x => x != null));
            }
        }

        private PieceType MockPieceType()
        {
            // Create a mock piece type for testing
            return new Pawn("Test Pawn", "T", 100, 100);
        }
    }
} 