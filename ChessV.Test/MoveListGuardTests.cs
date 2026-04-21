using System.Collections.Generic;
using System.Reflection;
using ChessV;
using ChessV.Games;
using ChessV.Games.Pieces.Apmw;

namespace ChessV.Test
{
  // Tests for the Phase 3 defensive guards in ChessV.Base.MoveList:
  //   1) UndoPickup throws an informative InvalidBoardStateException when
  //      asked to restore a pickup whose Piece is null (the symptom of a
  //      half-completed PerformPickup).
  //   2) MakeMove rolls back any pickups/drops it had already applied if
  //      a later step throws, so the board is restored to its pre-call
  //      state instead of being silently half-mutated.
  [TestClass]
  public class MoveListGuardTests
  {
    // Reuse the Checkers/Cannon test variant so we don't depend on the
    // full ApmwChess wiring. It gives us a real Board + MoveList + Game
    // pipeline with cheap setup.
    private static CheckersCannonTestGame CreateGame()
    {
      var game = new CheckersCannonTestGame();
      var attrs = typeof(CheckersCannonTestGame)
        .GetCustomAttributes(typeof(GameAttribute), inherit: false);
      Assert.IsTrue(attrs.Length > 0);
      var gameAttr = (GameAttribute)attrs[0];
      game.Initialize(gameAttr, null, null);
      game.LoadFEN("4k3/8/8/8/8/8/8/4K3 w - - 0 1");
      return game;
    }

    // ------------------------------------------------------------------
    // Part A — UndoPickup null-Piece guard
    // ------------------------------------------------------------------
    [TestMethod]
    public void UndoPickup_OnNullPiece_ThrowsClearException()
    {
      var g = CreateGame();
      var ml = g.RootMoveListForTest;

      // Plant a pickup record whose Piece is null at a known square.
      int square = 0;
      ml.SetPickupForTest(0, new Pickup { Piece = null, Square = square });

      var ex = Assert.ThrowsException<InvalidBoardStateException>(
        () => ml.UndoPickupForTest(0));

      StringAssert.Contains(ex.Message, "Piece is null");
      StringAssert.Contains(ex.Message, "pickup #0");
    }

    // ------------------------------------------------------------------
    // Part B — MakeMove rollback bounded by lastApplied indices
    //
    // We exercise the helper directly via the test hook: apply a real
    // pickup and a real drop, then call RollbackPartialApply with the
    // matching bounds and verify the board hash returns to its initial
    // value. This validates the LIFO Undo ordering and that the helper
    // restores all mutations a partial Make would have left behind.
    // ------------------------------------------------------------------
    [TestMethod]
    public void MakeMove_PartialApplyRollback_RestoresBoardState()
    {
      var g = CreateGame();
      var ml = g.RootMoveListForTest;
      var board = g.Board;

      // Pick a piece on the board to "move" — the white King at e1.
      int from = -1, to = -1;
      for (int sq = 0; sq < board.NumSquares; sq++)
      {
        var p = board[sq];
        if (p != null && p.Player == 0)
        {
          from = sq;
          // Find any empty adjacent square.
          for (int t = 0; t < board.NumSquares; t++)
            if (board[t] == null) { to = t; break; }
          break;
        }
      }
      Assert.IsTrue(from >= 0 && to >= 0, "Test setup: need a piece + empty square.");

      var preHash = board.HashCode;
      var prePiece = board[from];

      // Manually populate pickup[0] and drop[0] for a from->to move.
      ml.SetPickupForTest(0, new Pickup { Piece = null, Square = from });
      ml.SetDropForTest(0, new Drop { Piece = prePiece, Square = to, NewType = null });

      // Apply them as MakeMove would.
      ml.PerformPickupForTest(0);
      ml.PerformDropForTest(0);

      // Sanity: board has actually changed.
      Assert.AreNotEqual(preHash, board.HashCode,
        "PerformPickup+PerformDrop should have mutated the board hash.");
      Assert.IsNull(board[from]);
      Assert.IsNotNull(board[to]);

      // Now invoke the rollback helper as MakeMove's catch path would,
      // with lastApplied bounds matching what we actually applied.
      ml.RollbackPartialApplyForTest(0, 0, 0, 0);

      Assert.AreEqual(preHash, board.HashCode,
        "RollbackPartialApply must fully restore the board hash.");
      Assert.AreSame(prePiece, board[from],
        "Rolled-back piece must be back on its original square.");
      Assert.IsNull(board[to],
        "Rolled-back drop square must be empty again.");
    }

    // Verifies that the rollback respects the lastApplied bound: if the
    // pickup loop "stopped" before applying index 1 (so pickup[1].Piece
    // is still null), the rollback should NOT touch index 1 and must not
    // throw the Phase 3 null-Piece guard.
    [TestMethod]
    public void MakeMove_PartialApplyRollback_RespectsLastAppliedBound()
    {
      var g = CreateGame();
      var ml = g.RootMoveListForTest;
      var board = g.Board;

      int from = -1;
      for (int sq = 0; sq < board.NumSquares; sq++)
      {
        var p = board[sq];
        if (p != null && p.Player == 0) { from = sq; break; }
      }
      Assert.IsTrue(from >= 0);

      var preHash = board.HashCode;

      // pickup[0] is real and gets applied; pickup[1] is the "would-have-been"
      // slot whose PerformPickup never ran — so its Piece is still null.
      ml.SetPickupForTest(0, new Pickup { Piece = null, Square = from });
      ml.SetPickupForTest(1, new Pickup { Piece = null, Square = 0 });

      ml.PerformPickupForTest(0);
      // Simulated failure: pickup[1] never applied.

      // Rollback bounded at lastAppliedPickup = 0. Must NOT throw on slot 1.
      ml.RollbackPartialApplyForTest(0, 0, 0, -1);

      Assert.AreEqual(preHash, board.HashCode,
        "Bounded rollback must restore the board hash without touching unapplied slots.");
    }
  }
}
