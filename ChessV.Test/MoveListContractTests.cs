using ChessV;
using ChessV.Games;

namespace ChessV.Test
{
  // Generation append monotonicity is covered by existing Checkers/Cannon
  // diagnostics and intentionally left out here to avoid root-cause scope.
  [Game("MoveList Contract Test Variant",
      typeof(Geometry.Rectangular), 8, 8,
      Template = true)]
  public class MoveListContractTestGame : Chess
  {
    public override void SetGameVariables()
    {
      base.SetGameVariables();
      Castling.Value = "None";
      PromotionRule.Value = "None";
      PromotionTypes = "";
      EnPassant = false;
      PawnDoubleMove = false;
      Array = "8/8/8/8/8/8/8/8";
      FENStart = "8/8/8/8/8/8/8/8 w - - 0 1";
    }
  }

  [TestClass]
  public class MoveListContractTests
  {
    [TestCleanup]
    public void ResetRecoverableDiagnostics()
    {
      RecoverableDiagnostics.ResetForTest();
    }

    private static MoveListContractTestGame CreateGame()
    {
      var game = new MoveListContractTestGame();
      object[] attrs = typeof(MoveListContractTestGame)
        .GetCustomAttributes(typeof(GameAttribute), inherit: false);
      Assert.IsTrue(attrs.Length > 0, "Test game must carry a [Game] attribute.");
      game.Initialize((GameAttribute) attrs[0], null, null);
      game.LoadFEN("4k3/8/8/8/8/8/8/4K2R w - - 0 1");
      return game;
    }

    [TestMethod]
    public void Restart_DoesNotClearGeneratedMoves_OnlyRewindsIteration()
    {
      MoveListContractTestGame game = CreateGame();
      MoveList root = game.RootMoveListForTest;
      int moveCursor = root.MoveCursor;
      int pickupCursor = root.PickupCursorForTest;
      int dropCursor = root.DropCursorForTest;

      Assert.IsTrue(moveCursor > 0, "Test setup must generate at least one root move.");

      root.Restart(0);

      Assert.AreEqual(moveCursor, root.MoveCursor, "Restart must preserve generated move count.");
      Assert.AreEqual(moveCursor, root.Count, "Restart must preserve generated Count.");
      Assert.AreEqual(pickupCursor, root.PickupCursorForTest, "Restart must preserve pickup cursor.");
      Assert.AreEqual(dropCursor, root.DropCursorForTest, "Restart must preserve drop cursor.");
    }

    [TestMethod]
    public void CurrentMove_IsUnsetUntilMakeNextMoveSelectsMove()
    {
      MoveListContractTestGame game = CreateGame();
      MoveList root = game.RootMoveListForTest;
      int gameMoveNumber = game.GameMoveNumber;
      int boardMoveStackCount = game.BoardMoveStack.MoveCount;

      Assert.IsTrue(root.Count > 0, "Test setup must generate at least one root move.");
      Assert.AreEqual(MoveType.Invalid, root.CurrentMove.MoveType,
        "CurrentMove is search iteration state and must be unset before MakeNextMove.");

      Assert.IsTrue(root.MakeNextMove(), "MakeNextMove should select and make a generated move.");

      Assert.AreNotEqual(MoveType.Invalid, root.CurrentMove.MoveType,
        "CurrentMove should identify the move selected by MakeNextMove.");
      Assert.AreEqual(gameMoveNumber, game.GameMoveNumber,
        "MoveList.MakeNextMove must not commit to game history.");
      Assert.AreEqual(boardMoveStackCount, game.BoardMoveStack.MoveCount,
        "MoveList.MakeNextMove must not push BoardMoveStack history.");

      root.UnmakeMove();

      Assert.AreEqual(gameMoveNumber, game.GameMoveNumber,
        "MoveList.UnmakeMove must not change game history.");
      Assert.AreEqual(boardMoveStackCount, game.BoardMoveStack.MoveCount,
        "MoveList.UnmakeMove must not change BoardMoveStack history.");
    }

    [TestMethod]
    public void UnmakeMove_WithoutSelectedMove_ReportsDiagnosticAndLeavesState()
    {
      MoveListContractTestGame game = CreateGame();
      MoveList root = game.RootMoveListForTest;
      RecoverableDiagnostic diagnostic = null;
      int gameMoveNumber = game.GameMoveNumber;
      int boardMoveStackCount = game.BoardMoveStack.MoveCount;
      ulong boardHash = game.Board.HashCode;

      RecoverableDiagnostics.Handler = d =>
      {
        diagnostic = d;
        return RecoverableDiagnosticResponse.Continue;
      };

      root.UnmakeMove();

      Assert.IsNotNull(diagnostic, "UnmakeMove without a selected search move should report a diagnostic.");
      Assert.AreEqual("MoveList.UnmakeMove", diagnostic.Source);
      Assert.AreEqual(gameMoveNumber, game.GameMoveNumber,
        "No-selected UnmakeMove must not change game history.");
      Assert.AreEqual(boardMoveStackCount, game.BoardMoveStack.MoveCount,
        "No-selected UnmakeMove must not change BoardMoveStack history.");
      Assert.AreEqual(boardHash, game.Board.HashCode,
        "No-selected UnmakeMove must not mutate the board.");
    }
  }
}
