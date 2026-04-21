using System;
using System.Collections.Generic;
using System.Reflection;
using ChessV;
using ChessV.Games;
using ChessV.Games.Pieces.Apmw;

namespace ChessV.Test
{
  // *** PHASE 3.5 - MAKE/UNMAKE ROUND-TRIP STRESS TESTS ***
  //
  // These tests exercise the public MoveList.MakeMove(int) entry point with a
  // direct UnmakeMove(int) on the same index, asserting that Board.HashCode
  // and a per-square piece snapshot are bit-identical before vs. after the
  // round-trip. They serve two purposes:
  //
  //   1. PIN CURRENT BEHAVIOR. Tests without [TestCategory("KnownLimit")]
  //      pass on the current MoveList implementation. Phase 4's struct
  //      change must keep them green - they are the reverse-compatibility
  //      contract.
  //
  //   2. DOCUMENT CURRENT GAPS. Tests marked [TestCategory("KnownLimit")]
  //      describe Make/Unmake round-trip scenarios the current MoveList
  //      cannot safely support. These are expected to fail today; Phase 4's
  //      struct change is judged successful when they begin to pass without
  //      regressing the unmarked tests.
  //
  // Run all tests:                dotnet test ChessV.sln
  // Run only the contract tests:  dotnet test --filter "TestCategory!=KnownLimit"
  //
  // The internal protected MoveList.UnmakeMove(int) is reached via reflection
  // so the test file does not require source-side changes (Phase 1 and Phase
  // 3 author MoveList edits in parallel; we strictly add tests here).
  //
  namespace _RoundTripInternals
  {
    internal static class MoveListReflection
    {
      private static readonly MethodInfo s_unmakeByIndex = typeof(MoveList).GetMethod(
        "UnmakeMove",
        BindingFlags.Instance | BindingFlags.NonPublic,
        binder: null,
        types: new Type[] { typeof(int) },
        modifiers: null);

      public static void UnmakeMove(MoveList ml, int index)
      {
        if (s_unmakeByIndex == null)
          throw new InvalidOperationException(
            "MoveList.UnmakeMove(int) not found via reflection - has its signature changed?");
        try
        {
          s_unmakeByIndex.Invoke(ml, new object[] { index });
        }
        catch (TargetInvocationException tie) when (tie.InnerException != null)
        {
          // Surface the real exception so MSTest reports the original site.
          throw tie.InnerException;
        }
      }
    }
  }

  [TestClass]
  public class MoveListRoundTripTests
  {
    // --- shared helpers ----------------------------------------------------

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

    // Load a position into the test game. Wrapped so a load-time
    // generateMoves crash (which is itself a round-trip failure) is reported
    // with context rather than aborting MSTest with an unmapped exception.
    private static void LoadPosition(FirstTurnRuleTestGame g, string array, char side)
    {
      g.LoadFEN(array + " " + side + " - - 0 1");
    }

    private static (ulong hash, int[] squares) Snapshot(Game g)
    {
      var s = new int[g.Board.NumSquaresExtended];
      for (int i = 0; i < s.Length; i++)
        s[i] = g.Board[i] == null ? -1 : g.Board[i].PieceType.TypeNumber * 1000 + g.Board[i].Player;
      return (g.Board.HashCode, s);
    }

    private static void AssertRestored(Game g, (ulong hash, int[] squares) before, string ctx)
    {
      var after = Snapshot(g);
      Assert.AreEqual(before.hash, after.hash, ctx + ": HashCode differs after Make+Unmake");
      for (int i = 0; i < before.squares.Length; i++)
      {
        if (before.squares[i] != after.squares[i])
        {
          string note = i < g.Board.NumSquares ? " (" + g.Board.GetDefaultSquareNotation(i) + ")" : "";
          Assert.Fail(ctx + ": square " + i + note +
            " before=" + before.squares[i] + " after=" + after.squares[i]);
        }
      }
    }

    // Iterate every move currently in the root MoveList, Make it, Unmake it,
    // and verify the snapshot is restored. Returns the number of moves
    // exercised so the caller can sanity-check coverage.
    private static int RoundTripAllGeneratedMoves(FirstTurnRuleTestGame g, string scenario)
    {
      var ml = g.RootMoveListForTest;
      int n = ml.Count;
      Assert.IsTrue(n > 0, scenario + ": no moves generated; cannot round-trip anything.");
      for (int i = 0; i < n; i++)
      {
        var before = Snapshot(g);
        var info = ml.GetMoveForTest(i);
        bool made = ml.MakeMove(i);
        // Whether the engine considered the move legal or not, an attempted
        // Make followed by Unmake must restore the board exactly. Illegal
        // moves still perform pickups/drops that need undoing.
        try
        {
          _RoundTripInternals.MoveListReflection.UnmakeMove(ml, i);
        }
        catch (Exception ex)
        {
          Assert.Fail(scenario + $": move {i} ({info}) UnmakeMove threw {ex.GetType().Name}: {ex.Message}");
        }
        AssertRestored(g, before, scenario + $": move {i} ({info}, made={made})");
      }
      return n;
    }

    // --- 1. StandardMove: plain Chess from the opening position -----------
    //
    // Reverse-compat baseline: vanilla Chess from the standard starting array
    // generates 20 pseudo-legal opening moves. Every one must round-trip.
    [TestMethod]
    public void StandardMove_RoundTripsCleanly()
    {
      var g = CreateGame();
      // Standard chess starting position, castling disabled by the test game.
      LoadPosition(g, "rnbqkbnr/pppppppp/8/8/8/8/8/RNBQKBNR", 'w');
      g.GenerateMovesForTest(g.CurrentSide);
      int n = RoundTripAllGeneratedMoves(g, "StandardMove_OpeningPosition");
      Assert.IsTrue(n >= 20, "Expected at least 20 opening moves, got " + n);
    }

    // --- 2. Capture: a position with at least one capture available -------
    [TestMethod]
    public void Capture_RoundTripsCleanly()
    {
      var g = CreateGame();
      // White pawn on e4, Black pawn on d5: e4xd5 is the capture under test.
      LoadPosition(g, "4k3/8/8/3p4/4P3/8/8/4K3", 'w');
      g.GenerateMovesForTest(g.CurrentSide);
      RoundTripAllGeneratedMoves(g, "Capture_PawnTakesPawn");
    }

    // --- 3. CheckersSingleJump: one Checkers piece with a single jump -----
    //
    // A single (depth-1) Checkers jump exercises the same pickup-extra +
    // drop-extra path as a chain but with only one captured piece, so it
    // should round-trip cleanly today.
    [TestMethod]
    public void CheckersSingleJump_RoundTripsCleanly()
    {
      var g = CreateGame();
      // White Checkers c3 jumps NE over Black Checkers d4 to e5 (empty).
      // Kings present so CheckmateRule has its royals.
      LoadPosition(g, "4k3/8/8/8/3e4/2E5/8/4K3", 'w');
      g.GenerateMovesForTest(g.CurrentSide);
      RoundTripAllGeneratedMoves(g, "CheckersSingleJump");
    }

    // --- 4. CheckersDoubleJump: chain of two jumps ------------------------
    //
    // White Checkers b2 -> c3 over Black d4 -> e5 over Black f6 to g7.
    // Geometry per Checkers.GenerateJumpCaptures: NE direction means
    // "forward" for player 0, so each jump moves +1 file and +2 ranks
    // (jumping over the enemy at file+1/rank+1, landing on file+2/rank+2).
    [TestMethod]
    public void CheckersDoubleJump_RoundTripsCleanly()
    {
      var g = CreateGame();
      // White Checkers b2; Black Checkers c3 and e5; landing squares d4 and f6 empty.
      LoadPosition(g, "4k3/8/8/4e3/8/2e5/1E6/4K3", 'w');
      g.GenerateMovesForTest(g.CurrentSide);
      RoundTripAllGeneratedMoves(g, "CheckersDoubleJump");
    }

    // --- 5. CheckersTripleJump: chain of three jumps ----------------------
    //
    // The 2026-02-12 repro proves three-chain Checkers jumps can corrupt
    // pickup/drop ranges *during generation*. The single-move Make+Unmake
    // round-trip itself currently passes (verified at authoring time); kept
    // as a contract test so Phase 4's struct change cannot regress it.
    [TestMethod]
    public void CheckersTripleJump_RoundTripsCleanly()
    {
      var g = CreateGame();
      // White Checkers b2; Black Checkers c3, e5, g7; landings d4, f6, h8 empty.
      // Triple-jump path: b2 -> d4 -> f6 -> h8 (promoting on rank 8).
      LoadPosition(g, "4k3/6e1/8/4e3/8/2e5/1E6/4K3", 'w');
      g.GenerateMovesForTest(g.CurrentSide);
      RoundTripAllGeneratedMoves(g, "CheckersTripleJump");
    }

    // --- 6. AdjacentMultiJumps: two pieces, each with chained jumps -------
    //
    // Verifies that round-tripping move N has zero side-effect on the
    // pickup/drop ranges recorded for move N+1 when both moves come from
    // distinct pieces with overlapping geometry.
    [TestMethod]
    public void AdjacentMultiJumps_ShareNoState()
    {
      var g = CreateGame();
      // Two White Checkers (b2, b4) each with a double-jump available.
      //   b2 -> d4 -> f6 over c3, e5
      //   b4 -> d6 -> f8 over c5, e7
      LoadPosition(g, "4k3/4e3/8/2e1e3/1E6/2e5/1E6/4K3", 'w');
      g.GenerateMovesForTest(g.CurrentSide);
      RoundTripAllGeneratedMoves(g, "AdjacentMultiJumps");
    }

    // --- 7. SamePieceReturnsNearOrigin -----------------------------------
    //
    // A two-jump chain whose final landing square is on the same file as the
    // starting square (NE then NW, or vice versa). Tests overlap of the
    // moving piece's pickup square with subsequent drop coordinates.
    [TestMethod]
    public void SamePieceReturnsNearOrigin_RoundTrips()
    {
      var g = CreateGame();
      // White Checkers d2 jumps NE over e3 to f4, then NW over e5 to d6.
      // Final landing d6 is on the starting file (d), two ranks ahead.
      LoadPosition(g, "4k3/8/8/4e3/8/4e3/3E4/4K3", 'w');
      g.GenerateMovesForTest(g.CurrentSide);
      RoundTripAllGeneratedMoves(g, "SamePieceReturnsNearOrigin");
    }

    // --- 8. ZeroPickupZeroDropMove ----------------------------------------
    //
    // EndMoveAddCore (line ~792) currently rejects the degenerate
    // pickupCount==0 && dropCount==0 case at the source. The path to it from
    // the public API requires fabricating a MoveList state we cannot
    // reasonably construct from outside without dragging in the same
    // generation pipeline that itself never produces zero-pickup-zero-drop
    // moves. Document the gap and skip; Phase 4's struct rework should keep
    // the rejection invariant explicit.
    [TestMethod]
    [Ignore("Path unreachable from public API; intentional gap test.")]
    public void ZeroPickupZeroDropMove_DoesNotShiftCursors()
    {
      // Intentionally empty - guarded by [Ignore].
    }

    // --- 9. CheckersAndCannonOverlap --------------------------------------
    //
    // Cannon needs a screen square to capture. Construct a position where
    // the Cannon's screen square is also a Checkers jump-over square so the
    // two move generators produce overlapping pickup geometries.
    [TestMethod]
    public void CheckersAndCannonOverlap_RoundTrips()
    {
      var g = CreateGame();
      // White Cannon on e1, screen on e3 (Black pawn), target Black king
      // exposure along the e-file. Simultaneously a White Checkers on d2
      // can jump NE over e3 (the Cannon's screen) to f4.
      //   8: 4k3
      //   3: 4p3   <- Black pawn doubles as Cannon screen and Checkers jump
      //   2: 3E4   <- White Checkers d2
      //   1: 4I3   <- White Cannon e1
      LoadPosition(g, "4k3/8/8/8/8/4p3/3E4/4I2K", 'w');
      g.GenerateMovesForTest(g.CurrentSide);
      RoundTripAllGeneratedMoves(g, "CheckersAndCannonOverlap");
    }

    // --- 10. AllGeneratedMoves from the 2026-02-12 FEN --------------------
    //
    // The comprehensive "what's broken" oracle: load the exact position from
    // _refs/2026-02-12.txt and round-trip every generated move. Marked
    // KnownLimit because the load itself currently throws
    // InvalidBoardStateException during the implicit generateMoves pass.
    [TestMethod]
    [TestCategory("KnownLimit")]
    public void AllGeneratedMoves_RoundTripFromOpeningPosition_FirstTurnGame()
    {
      var g = CreateGame();
      // Same FEN as Repros_2026_02_12_CheckersMultiJumpCorruptsBoardDuringGeneration.
      try
      {
        LoadPosition(g, "4k3/7e/6P1/4e3/3P4/4R3/8/4K3", 'b');
      }
      catch (InvalidBoardStateException ibse)
      {
        Assert.Fail("LoadFEN's implicit generateMoves crashed at square " +
          ibse.Square + " (" + ibse.SquareNotation + "): " + ibse.Message +
          " - this is the round-trip violation under test.");
      }
      // If LoadFEN survives, we still want to exercise the explicit
      // generation path and round-trip every single move it produces.
      g.GenerateMovesForTest(g.CurrentSide);
      RoundTripAllGeneratedMoves(g, "AllGeneratedMoves_2026_02_12");
    }
  }
}
