using System;
using System.Collections.Generic;
using System.Reflection;
using ChessV;
using ChessV.Games;
using ChessV.Games.Pieces.Apmw;

namespace ChessV.Test
{
  // *** CHECKERS MAKE/UNMAKE SAFETY NET ***
  //
  // Commit 022b223 removed two writes from MoveList.MakeMove that were
  // overwriting the BeginMoveAdd/EndMoveAddCore snapshot of pickup/drop
  // cursors and corrupting nested move-add sequences during legality
  // checks. The change fixed the 2026-02-12 generation crash in Checkers
  // multi-jump positions (see FirstTurnRuleReproTests.cs).
  //
  // The concern these tests put to bed is whether ANY production code
  // path - and Checkers multi-piece capture chains in particular -
  // depended on those writes for Make/Unmake to round-trip cleanly.
  //
  // Each test:
  //   1. Builds a realistic Checkers multi-jump position via the
  //      FirstTurnRuleTestGame harness (it wires up the Checkers piece
  //      type with promotion configured so triple-jumps to rank 8
  //      promote).
  //   2. Generates moves and asserts no exception is thrown - this is
  //      the same EndMoveAddCore nested-MakeMove path that crashed
  //      before 022b223.
  //   3. For the multi-capture move (and, in the branching test, every
  //      generated move) snapshots the full board, side-to-move, and
  //      MoveList cursors (including the private temp cursors via
  //      reflection), runs MakeMove + UnmakeMove(), and asserts
  //      byte-for-byte equality with the pre-Make snapshot.
  //
  [TestClass]
  public class MakeMoveSafetyTests
  {
    // --- internal-field reflection helpers --------------------------------

    private static readonly FieldInfo s_tempPickupCursor =
      typeof(MoveList).GetField("tempPickupCursor", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo s_tempDropCursor =
      typeof(MoveList).GetField("tempDropCursor", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo s_moveCursor =
      typeof(MoveList).GetField("moveCursor", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MethodInfo s_unmakeByIndex = typeof(MoveList).GetMethod(
      "UnmakeMove",
      BindingFlags.Instance | BindingFlags.NonPublic,
      binder: null,
      types: new Type[] { typeof(int) },
      modifiers: null);

    private static int TempPickupCursor(MoveList ml) => (int)s_tempPickupCursor.GetValue(ml);
    private static int TempDropCursor(MoveList ml) => (int)s_tempDropCursor.GetValue(ml);
    private static int MoveCursorField(MoveList ml) => (int)s_moveCursor.GetValue(ml);

    // MoveList.MakeMove(int) does not write currentMoveIndex; the public
    // parameterless UnmakeMove() therefore unmakes whatever index was last
    // stamped during generation, NOT the move we just Made. Reach the
    // protected UnmakeMove(int) overload via reflection so tests can pair
    // Make/Unmake by explicit index, matching MoveListRoundTripTests.
    private static void UnmakeByIndex(MoveList ml, int index)
    {
      try
      {
        s_unmakeByIndex.Invoke(ml, new object[] { index });
      }
      catch (TargetInvocationException tie) when (tie.InnerException != null)
      {
        throw tie.InnerException;
      }
    }

    // --- harness ----------------------------------------------------------

    private static FirstTurnRuleTestGame CreateGame()
    {
      var game = new FirstTurnRuleTestGame();
      var attrs = typeof(FirstTurnRuleTestGame)
        .GetCustomAttributes(typeof(GameAttribute), inherit: false);
      Assert.IsTrue(attrs.Length > 0, "Test game must carry a [Game] attribute.");
      var gameAttr = (GameAttribute)attrs[0];
      game.Initialize(gameAttr, null, null);
      return game;
    }

    private static void LoadPosition(FirstTurnRuleTestGame g, string array, char side)
    {
      g.LoadFEN(array + " " + side + " - - 0 1");
    }

    // --- snapshot / compare ----------------------------------------------

    private struct Snapshot
    {
      public ulong Hash;
      public int CurrentSide;
      public int[] Squares;     // -1 = empty; otherwise pieceTypeNumber*1000 + player
      public int PickupCursor;
      public int DropCursor;
      public int TempPickupCursor;
      public int TempDropCursor;
      public int MoveCursor;
    }

    private static Snapshot Take(Game g)
    {
      var ml = g.RootMoveListForTest;
      var snap = new Snapshot
      {
        Hash = g.Board.HashCode,
        CurrentSide = g.CurrentSide,
        Squares = new int[g.Board.NumSquaresExtended],
        PickupCursor = ml.PickupCursorForTest,
        DropCursor = ml.DropCursorForTest,
        TempPickupCursor = TempPickupCursor(ml),
        TempDropCursor = TempDropCursor(ml),
        MoveCursor = MoveCursorField(ml),
      };
      for (int i = 0; i < snap.Squares.Length; i++)
      {
        var p = g.Board[i];
        snap.Squares[i] = p == null ? -1 : p.PieceType.TypeNumber * 1000 + p.Player;
      }
      return snap;
    }

    private static void AssertEqual(Game g, Snapshot before, Snapshot after, string ctx)
    {
      Assert.AreEqual(before.Hash, after.Hash, ctx + ": Board.HashCode differs");
      Assert.AreEqual(before.CurrentSide, after.CurrentSide, ctx + ": CurrentSide differs");
      Assert.AreEqual(before.PickupCursor, after.PickupCursor, ctx + ": pickupCursor differs");
      Assert.AreEqual(before.DropCursor, after.DropCursor, ctx + ": dropCursor differs");
      Assert.AreEqual(before.TempPickupCursor, after.TempPickupCursor, ctx + ": tempPickupCursor differs");
      Assert.AreEqual(before.TempDropCursor, after.TempDropCursor, ctx + ": tempDropCursor differs");
      Assert.AreEqual(before.MoveCursor, after.MoveCursor, ctx + ": moveCursor differs");
      for (int i = 0; i < before.Squares.Length; i++)
      {
        if (before.Squares[i] != after.Squares[i])
        {
          string note = i < g.Board.NumSquares ? " (" + g.Board.GetDefaultSquareNotation(i) + ")" : "";
          Assert.Fail(ctx + ": square " + i + note +
                      " before=" + before.Squares[i] + " after=" + after.Squares[i]);
        }
      }
    }

    // Run Make+Unmake on a single move index and assert full restore.
    private static void RoundTripOne(FirstTurnRuleTestGame g, int moveIndex, string ctx)
    {
      var ml = g.RootMoveListForTest;
      var before = Take(g);

      bool made;
      try
      {
        made = ml.MakeMove(moveIndex);
      }
      catch (Exception ex)
      {
        Assert.Fail(ctx + ": MakeMove threw " + ex.GetType().Name + ": " + ex.Message);
        return;
      }

      // Snapshot the post-Make state for diagnostic context only - it is
      // expected to differ from `before`. We don't assert anything here.
      _ = Take(g);

      try
      {
        UnmakeByIndex(ml, moveIndex);
      }
      catch (Exception ex)
      {
        Assert.Fail(ctx + ": UnmakeMove threw " + ex.GetType().Name + ": " + ex.Message);
        return;
      }

      var after = Take(g);
      AssertEqual(g, before, after, ctx + " (made=" + made + ")");
    }

    // Find the multi-capture move that ends on the given destination
    // notation (e.g. "f6"). Returns -1 if none found.
    private static int FindMoveTo(MoveList ml, Game g, string toNotation)
    {
      int target = g.Board.DefaultNotationToSquare(toNotation);
      for (int i = 0; i < ml.Count; i++)
      {
        var mv = ml.GetMoveForTest(i);
        if (mv.ToSquare == target)
          return i;
      }
      return -1;
    }

    // EndMoveAddCore safety: simply generating moves must not throw. Before
    // 022b223 these positions raised InvalidBoardStateException during the
    // nested Make/Unmake legality check.
    private static void AssertGenerationDoesNotThrow(FirstTurnRuleTestGame g, string ctx)
    {
      try
      {
        g.GenerateMovesForTest(g.CurrentSide);
      }
      catch (Exception ex)
      {
        Assert.Fail(ctx + ": GenerateMovesForTest threw " + ex.GetType().Name + ": " + ex.Message);
      }
    }

    // ---------------------------------------------------------------------
    // 1. Mid-game double jump.
    //    White Checkers b2 jumps NE over Black Checkers c3 to d4, then NE
    //    over Black Checkers e5 to f6. Two captures, no promotion.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void DoubleJump_RoundTripsCleanly()
    {
      var g = CreateGame();
      LoadPosition(g, "4k3/8/8/4e3/8/2e5/1E6/4K3", 'w');
      AssertGenerationDoesNotThrow(g, "DoubleJump:generation");

      var ml = g.RootMoveListForTest;
      int idx = FindMoveTo(ml, g, "f6");
      Assert.AreNotEqual(-1, idx, "DoubleJump: expected a multi-capture move ending on f6.");
      RoundTripOne(g, idx, "DoubleJump b2->d4->f6");
    }

    // ---------------------------------------------------------------------
    // 2. Mid-game triple jump WITH KING PROMOTION on the final landing.
    //    White Checkers b2 chains b2->d4->f6->h8 over c3,e5,g7. Landing
    //    on rank 8 promotes via Checkers.SetPromotionTypes (configured in
    //    FirstTurnRuleTestGame.AddRules to {Q,R,B,N}). UnmakeMove must
    //    restore the original Checkers piece on b2 *and* clear h8.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void TripleJumpWithPromotion_RoundTripsCleanly()
    {
      var g = CreateGame();
      LoadPosition(g, "4k3/6e1/8/4e3/8/2e5/1E6/4K3", 'w');
      AssertGenerationDoesNotThrow(g, "TripleJumpWithPromotion:generation");

      var ml = g.RootMoveListForTest;
      int idx = FindMoveTo(ml, g, "h8");
      Assert.AreNotEqual(-1, idx,
        "TripleJumpWithPromotion: expected the promoting multi-capture move to h8.");

      // Confirm that performing this move actually promotes - the moving
      // piece on h8 must end up as something other than the original
      // Checkers piece type. This protects the test from silently passing
      // if promotion wiring breaks.
      var beforeProbe = Take(g);
      ml.MakeMove(idx);
      var pieceOnH8 = g.Board[g.Board.DefaultNotationToSquare("h8")];
      Assert.IsNotNull(pieceOnH8, "TripleJumpWithPromotion: h8 empty after MakeMove.");
      Assert.AreNotEqual("Checkers", pieceOnH8.PieceType.Name,
        "TripleJumpWithPromotion: piece on h8 was not promoted (still Checkers).");
      UnmakeByIndex(ml, idx);
      AssertEqual(g, beforeProbe, Take(g), "TripleJumpWithPromotion: probe round-trip");

      // Now the formal snapshot/Make/Unmake cycle.
      RoundTripOne(g, idx, "TripleJumpWithPromotion b2->d4->f6->h8");
    }

    // ---------------------------------------------------------------------
    // 3. Black-side multi-jump. The forward direction for player 1
    //    decreases rank, so this also exercises the per-player geometry
    //    inside Checkers.GenerateJumpCaptures.
    //    Black Checkers g7 jumps SW over White Pawn f6? Pawns don't sit on
    //    diagonal jump squares the same way. Use Checkers stones for the
    //    captured pieces so the geometry stays inside the Checkers
    //    generator: Black g7 -> e5 (over f6 White Checkers) -> c3 (over
    //    d4 White Checkers).
    // ---------------------------------------------------------------------
    [TestMethod]
    public void BlackDoubleJump_RoundTripsCleanly()
    {
      var g = CreateGame();
      // 7: 6e1   Black Checkers g7
      // 6: 5E2   White Checkers f6 (jump-over)
      // 4: 3E4   White Checkers d4 (jump-over)
      // landings e5, c3 empty.
      LoadPosition(g, "4k3/6e1/5E2/8/3E4/8/8/4K3", 'b');
      AssertGenerationDoesNotThrow(g, "BlackDoubleJump:generation");

      var ml = g.RootMoveListForTest;
      int idx = FindMoveTo(ml, g, "c3");
      Assert.AreNotEqual(-1, idx, "BlackDoubleJump: expected multi-capture move ending on c3.");
      RoundTripOne(g, idx, "BlackDoubleJump g7->e5->c3");
    }

    // ---------------------------------------------------------------------
    // 4. Multiple legal multi-jumps available. Two White Checkers each
    //    with their own multi-jump chain. AddMove must allocate
    //    pickup/drop ranges sequentially without aliasing, so this
    //    iterates EVERY generated move through the snapshot/Make/Unmake
    //    cycle - the same coverage flavor as MoveListRoundTripTests but
    //    with the additional temp-cursor invariant that became
    //    load-bearing after 022b223.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void BranchingMultiJumps_AllMovesRoundTrip()
    {
      var g = CreateGame();
      // White Checkers at b2 and b4.
      // Black Checkers at c3, c5, e5.
      // Available multi-jumps:
      //   b2 -> d4 (over c3) -> f6 (over e5)
      //   b4 -> d6 (over c5)            (no further jump - e7 empty)
      // Plus simple step moves for both white Checkers and the King.
      LoadPosition(g, "4k3/8/8/2e1e3/1E6/2e5/1E6/4K3", 'w');
      AssertGenerationDoesNotThrow(g, "BranchingMultiJumps:generation");

      var ml = g.RootMoveListForTest;
      int n = ml.Count;
      Assert.IsTrue(n >= 3,
        "BranchingMultiJumps: expected at least 3 moves (got " + n + ").");

      // Confirm the multi-jump destination f6 is among the generated moves
      // so we know AddMove actually had to allocate non-trivial pickup/drop
      // ranges (5 entries: 3 pickups + 2 drops in the chain alone).
      Assert.AreNotEqual(-1, FindMoveTo(ml, g, "f6"),
        "BranchingMultiJumps: expected a chained jump landing on f6.");

      for (int i = 0; i < n; i++)
      {
        var mv = ml.GetMoveForTest(i);
        RoundTripOne(g, i, "BranchingMultiJumps move#" + i + " " + mv);
      }
    }

    // ---------------------------------------------------------------------
    // 5. EndMoveAddCore stress: positions whose generation ALONE was the
    //    crash signature pre-022b223. We don't pin a specific move here -
    //    the assertion is that GenerateMoves completes cleanly. This is
    //    the single test that would have FAILED on the old code.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void GenerationNeverThrows_CheckersMultiJumpPositions()
    {
      string[] fens = new string[]
      {
        // The exact FEN from _refs/2026-02-12.txt.
        "4k3/7e/6P1/4e3/3P4/4R3/8/4K3 b - - 0 1",
        // Double jump position.
        "4k3/8/8/4e3/8/2e5/1E6/4K3 w - - 0 1",
        // Triple jump w/ promotion.
        "4k3/6e1/8/4e3/8/2e5/1E6/4K3 w - - 0 1",
        // Branching multi-jumps.
        "4k3/8/8/2e1e3/1E6/2e5/1E6/4K3 w - - 0 1",
        // Black-side double jump.
        "4k3/6e1/5E2/8/3E4/8/8/4K3 b - - 0 1",
      };

      foreach (var fen in fens)
      {
        var g = CreateGame();
        try
        {
          g.LoadFEN(fen);
        }
        catch (Exception ex)
        {
          Assert.Fail("Generation: LoadFEN(\"" + fen + "\") threw " +
                      ex.GetType().Name + ": " + ex.Message);
        }
        // LoadFEN already runs an implicit generateMoves; explicitly call
        // again to double-cover the standalone path.
        AssertGenerationDoesNotThrow(g, "Generation: " + fen);
      }
    }
  }
}
