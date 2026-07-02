using System.Collections.Generic;
using System.IO;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using ChessV.Games.Pieces.Apmw;

namespace ChessV.Test
{
  [TestClass]
  public class RootMoveListLifecycleTests
  {
    private const string ApmwGameName = "Archipelago Multiworld";

    [TestInitialize]
    public void ResetGlobalApmwState()
    {
      ResetApmwCore();
    }

    [TestMethod]
    public void FreshChess_InitializeLeavesGeneratedRootCandidatesWithoutPlayedHistory()
    {
      Game game = CreateGame("Chess");

      AssertGeneratedRootCandidates(game, "fresh Chess");
      Assert.AreEqual(0, game.BoardMoveStack.MoveCount, "fresh Chess should have no played moves.");
      Assert.AreEqual(0, game.GameMoveNumber, "fresh Chess should be at game move 0.");
      Assert.AreEqual(1, game.Ply, "fresh Chess root generation should leave Ply at root.");
    }

    [TestMethod]
    public void FreshApmw_InitializeLeavesGeneratedRootCandidatesWithoutPlayedHistory()
    {
      Game game = CreateGame(ApmwGameName);

      AssertGeneratedRootCandidates(game, "fresh APMW");
      Assert.AreEqual(0, game.BoardMoveStack.MoveCount, "fresh APMW should have no played moves.");
      Assert.AreEqual(0, game.GameMoveNumber, "fresh APMW should be at game move 0.");
      Assert.AreEqual(1, game.Ply, "fresh APMW root generation should leave Ply at root.");
    }

    [TestMethod]
    public void ExplicitResetClearsRootCursors_AndGenerateRepopulatesThem()
    {
      Game game = CreateGame(ApmwGameName);
      CursorSnapshot initialized = CursorSnapshot.Take(game);

      game.RootMoveListForTest.Reset();
      CursorSnapshot reset = CursorSnapshot.Take(game);

      Assert.AreEqual(0, reset.MoveCursor, "root.Reset should clear move cursor.");
      Assert.AreEqual(0, reset.PickupCursor, "root.Reset should clear pickup cursor.");
      Assert.AreEqual(0, reset.DropCursor, "root.Reset should clear drop cursor.");

      game.GenerateMovesForTest(game.CurrentSide);
      CursorSnapshot regenerated = CursorSnapshot.Take(game);

      Assert.AreEqual(initialized.MoveCursor, regenerated.MoveCursor, "generation should restore initial move count.");
      Assert.AreEqual(initialized.PickupCursor, regenerated.PickupCursor, "generation should restore initial pickup count.");
      Assert.AreEqual(initialized.DropCursor, regenerated.DropCursor, "generation should restore initial drop count.");
    }

    [TestMethod]
    public void MakeMoveAndUndo_RegenerateRootCandidatesForCurrentPosition()
    {
      Game game = CreateGame(ApmwGameName);
      CursorSnapshot initial = CursorSnapshot.Take(game);

      Movement firstMove = FirstRootMovement(game);
      game.MakeMove(firstMove, true);

      AssertGeneratedRootCandidates(game, "after first APMW move");
      Assert.AreEqual(1, game.BoardMoveStack.MoveCount, "first move should be committed.");
      Assert.AreEqual(1, game.GameMoveNumber, "first move should advance game move number.");
      Assert.AreEqual(1, game.Ply, "post-move generation should leave Ply at root.");

      game.UndoMove();

      CursorSnapshot afterUndo = CursorSnapshot.Take(game);
      Assert.AreEqual(0, game.BoardMoveStack.MoveCount, "undo should remove the committed move.");
      Assert.AreEqual(0, game.GameMoveNumber, "undo should restore game move number.");
      Assert.AreEqual(1, game.Ply, "post-undo generation should leave Ply at root.");
      Assert.AreEqual(initial.MoveCursor, afterUndo.MoveCursor, "undo should regenerate the initial root move count.");
      Assert.AreEqual(initial.PickupCursor, afterUndo.PickupCursor, "undo should regenerate the initial root pickup count.");
      Assert.AreEqual(initial.DropCursor, afterUndo.DropCursor, "undo should regenerate the initial root drop count.");
    }

    [TestMethod]
    public void SavedApmwGameReload_ReplaysToSameRootCandidateStateAsDirectPlay()
    {
      Manager.Manager manager = new Manager.Manager();
      Game direct = manager.CreateGame(ApmwGameName);

      Movement firstMove = FirstRootMovement(direct);
      direct.MakeMove(firstMove, true);
      CursorSnapshot directAfterMove = CursorSnapshot.Take(direct);

      string saved;
      using (StringWriter writer = new StringWriter())
      {
        direct.SaveGame(writer);
        saved = writer.ToString();
      }

      ResetApmwCore();

      Game loaded;
      using (StringReader reader = new StringReader(saved))
        loaded = manager.LoadGame(reader);

      CursorSnapshot loadedAfterReplay = CursorSnapshot.Take(loaded);

      Assert.AreEqual(direct.CurrentSide, loaded.CurrentSide, "loaded game should replay to same side to move.");
      Assert.AreEqual(direct.GameMoveNumber, loaded.GameMoveNumber, "loaded game should replay same move number.");
      Assert.AreEqual(direct.BoardMoveStack.MoveCount, loaded.BoardMoveStack.MoveCount, "loaded game should replay same history length.");
      Assert.AreEqual(directAfterMove.MoveCursor, loadedAfterReplay.MoveCursor, "loaded game should regenerate same root move count.");
      Assert.AreEqual(directAfterMove.PickupCursor, loadedAfterReplay.PickupCursor, "loaded game should regenerate same root pickup count.");
      Assert.AreEqual(directAfterMove.DropCursor, loadedAfterReplay.DropCursor, "loaded game should regenerate same root drop count.");
    }

    private static Game CreateGame(string gameName)
    {
      Manager.Manager manager = new Manager.Manager();
      return manager.CreateGame(gameName);
    }

    private static Movement FirstRootMovement(Game game)
    {
      MoveInfo[] moves;
      int count = game.GetRootMoves(out moves);
      Assert.IsTrue(count > 0, "test setup must have at least one generated root move.");
      return moves[0];
    }

    private static void AssertGeneratedRootCandidates(Game game, string context)
    {
      MoveList root = game.RootMoveListForTest;
      Assert.IsNotNull(root, context + ": root MoveList should exist after initialization.");
      Assert.IsTrue(root.MoveCursor > 0, context + ": root MoveList should contain generated candidates.");
      Assert.AreEqual(root.MoveCursor, root.Count, context + ": Count should mirror MoveCursor.");
      Assert.IsTrue(root.PickupCursorForTest >= root.MoveCursor,
        context + ": generated root candidates should have pickup ranges.");
      Assert.IsTrue(root.DropCursorForTest >= root.MoveCursor,
        context + ": generated root candidates should have drop ranges.");
    }

    private static void ResetApmwCore()
    {
      ApmwCore core = ApmwCore.getInstance();
      core.foundPockets = -1;
      core.foundPocketRange = -1;
      core.foundPocketGems = -1;
      core.foundPawns = -1;
      core.foundMinors = -1;
      core.foundMajors = -1;
      core.foundJacks = -1;
      core.foundQueens = -1;
      core.foundAmazons = -1;
      core.foundConsuls = -1;
      core.foundKingPromotions = -1;
      core.foundPawnForwardness = -1;
      core.isGrand = false;
      core.foundArmy = null;

      PieceType king = new King("King", "K", 325, 325);
      PieceType rook = new Rook("Rook", "R", 500, 550);
      PieceType pawn = new Pawn("Pawn", "P", 100, 125);

      Dictionary<KeyValuePair<int, int>, PieceType> starters =
        new Dictionary<KeyValuePair<int, int>, PieceType>();
      for (int f = 0; f < 8; f++)
        starters[new KeyValuePair<int, int>(3, f)] = pawn;
      starters[new KeyValuePair<int, int>(4, 0)] = rook;
      starters[new KeyValuePair<int, int>(4, 4)] = king;
      starters[new KeyValuePair<int, int>(4, 7)] = rook;

      core.PlayerPieceSetProvider = (numFiles) => (starters, "QRNB");
      core.PlayerPocketPiecesProvider = () => new List<PieceType>();
      core.GeriProvider = () => 1;
      core.EngineWeakeningProvider = () => 0;
    }

    private struct CursorSnapshot
    {
      public int MoveCursor;
      public int PickupCursor;
      public int DropCursor;

      public static CursorSnapshot Take(Game game)
      {
        MoveList root = game.RootMoveListForTest;
        return new CursorSnapshot
        {
          MoveCursor = root.MoveCursor,
          PickupCursor = root.PickupCursorForTest,
          DropCursor = root.DropCursorForTest
        };
      }
    }
  }
}
