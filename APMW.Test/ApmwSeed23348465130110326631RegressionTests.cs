using ChessV.Base;
using ChessV.Games;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChessV.Test
{
    [TestClass]
    public class ApmwSeed23348465130110326631RegressionTests
    {
        [TestMethod]
        public void Seed23348465130110326631_CreateReportedSuperSizedGameState_DoesNotCrash()
        {
            // The support helper's constants are distilled from
            // sgf\AP_23348465130110326631\AP_23348465130110326631.archipelago
            // and first-column counts in sgf\23348465130110326631.txt; keep this
            // regression test deterministic by not reading those large files at runtime.
            // Do not catch GameInitializationException/IndexOutOfRangeException here;
            // the reported super-sized AddPieceTypes crash should fail this test naturally.
            var game = ApmwSeed23348465130110326631TestSupport.CreateReportedGame();

            Assert.IsNotNull(game);
            Assert.IsInstanceOfType(game, typeof(ApmwChessGame), "Reported state should create an APMW game.");
            Assert.IsInstanceOfType(game, typeof(ApmwGrandChess), "Super-Size Me should select the super-sized APMW variant.");
            Assert.AreEqual(ApmwSeed23348465130110326631TestSupport.ReportedGameName, game.GameAttribute.GameName);
            Assert.IsNotNull(game.Board, "Created game should be initialized with a board.");
            Assert.AreEqual(10, game.NumFiles, "Super-sized APMW should use 10 files.");
            Assert.AreEqual(10, game.Board.NumFiles, "Super-sized APMW board should use 10 files.");
            Assert.AreEqual(8, game.Board.NumRanks, "Super-sized APMW board should keep the standard 8 ranks.");
            Assert.AreEqual(80, game.Board.NumSquares, "Super-sized APMW board should expose a 10x8 playable board.");
            Assert.AreEqual(86, game.Board.NumSquaresExtended, "Super-sized APMW should include three pocket squares per player.");
            Assert.AreEqual(0, ApmwCore.getInstance().GeriProvider(), "The reported Play as White item should configure the human player as White.");

            var root = game.RootMoveListForTest;
            Assert.IsNotNull(root, "Created game should have an initialized root move list.");
            root.Reset();
            game.GenerateMovesForTest(game.CurrentSide);
            Assert.IsTrue(root.MoveCursor > 0, "The reported initial position should generate at least one legal move.");
        }
    }
}
