using System.Collections.Generic;
using System.Reflection;
using ChessV;
using ChessV.Games;
using ChessV.Games.Pieces.Apmw;

namespace ChessV.Test
{
  // *** TEST-ONLY GAME ***
  //
  // These tests exercise the MoveList / Game.generateMoves pipeline for the
  // two piece types responsible for recent crash reports (Checkers and Cannon).
  // The real crash site lives in MoveList.PerformPickup: when a move is
  // accepted into the MoveList, its pickup squares must contain the exact
  // piece expected at the moment the move is later played back. The full
  // ApmwChess game is expensive to construct and uses randomization that
  // varies piece-type availability per run, so we construct a minimal test
  // variant that adds Checkers and Cannon on top of a standard Chess board.
  //
  // The tests then:
  //   1) Snapshot the board (piece identity per square) and MoveList cursors
  //      (moveCursor, pickupCursor, dropCursor) before generation.
  //   2) Invoke generateMoves via Game.GenerateMovesForTest.
  //   3) Assert the board is pristine and that each generated move's pickups
  //      reference squares that currently hold the expected piece.
  //
  // If any test reproduces the crash it will surface as either
  //   - an InvalidBoardStateException thrown from PerformPickup (caught here
  //     and re-thrown as an Assert.Fail with the originating move), or
  //   - a mismatch between the pickup square and the piece present on it.
  //
  [Game("Apmw CheckersCannon Test Variant",
      typeof(Geometry.Rectangular), 8, 8,
      Template = true)]
  public class CheckersCannonTestGame : Chess
  {
    public Checkers CheckersType;
    public Cannon CannonType;

    public override void SetGameVariables()
    {
      base.SetGameVariables();
      // Disable castling to avoid Generic8x8 castling rule requirements on
      // starting squares, which would conflict with our hand-placed pieces.
      Castling.Value = "None";
      // Standard chess array gets overwritten per-test anyway; keep a
      // syntactically valid FEN so the initial LoadFEN in postInitialize
      // succeeds. We use an empty board as the initial position so that
      // the first "real" LoadFEN call in each test places its pieces onto
      // a fresh board (LoadFEN does not clear state before placing).
      Array = "8/8/8/8/8/8/8/8";
      FENStart = "8/8/8/8/8/8/8/8 w - - 0 1";
      PromotionTypes = "QRNB";
      EnPassant = false;
    }

    public override void AddPieceTypes()
    {
      base.AddPieceTypes();
      // Give Checkers and Cannon unused single-letter notations to avoid
      // collisions with Chess's K/Q/R/B/N/P notations in TypesByNotation.
      CheckersType = new Checkers("Checkers", "E", 150, 200);
      AddPieceType(CheckersType);
      CannonType = new Cannon("Cannon", "I", 400, 400);
      AddPieceType(CannonType);
    }

    public override void AddRules()
    {
      base.AddRules();
      // Checkers needs its promotion-type list explicitly set (its
      // CustomMoveGenerator silently skips promoting jumps otherwise).
      // ApmwChessGame normally does this in AddRules; mirror it here.
      CheckersType.SetPromotionTypes(new List<PieceType> { Queen, Rook, Bishop, Knight });
    }
  }

  [TestClass]
  public class CheckersCannonGenerationTests
  {
    // Helper to instantiate the test game directly, bypassing Manager so we
    // don't pull in the whole Include/ .cvc compile pipeline.
    private static CheckersCannonTestGame CreateGame()
    {
      var game = new CheckersCannonTestGame();
      var attrs = typeof(CheckersCannonTestGame)
        .GetCustomAttributes(typeof(GameAttribute), inherit: false);
      Assert.IsTrue(attrs.Length > 0, "Test game must carry a [Game] attribute.");
      var gameAttr = (GameAttribute)attrs[0];
      game.Initialize(gameAttr, null, null);
      return game;
    }

    // Load the game with a custom FEN array. We go through LoadFEN (not
    // ClearGameState + AddPiece) so that rules such as CheckmateRule get
    // their PositionLoaded callback and properly re-scan royal pieces.
    private static void LoadPosition(CheckersCannonTestGame g, string array, char side = 'w')
    {
      g.LoadFEN(array + " " + side + " - - 0 1");
    }

    private static int Sq(Game g, string notation) => g.Board.DefaultNotationToSquare(notation);

    //  Construct a Piece and hand it to Game.AddPiece so the board,
    //  piece lists, and bitboards all agree. Mirrors the minimal subset
    //  of what Game.placePiecesByArray does for each character of a FEN
    //  array, but keyed by piece type rather than notation so the tests
    //  read naturally.
    private static void Place(Game g, PieceType type, int player, string notation)
    {
      int square = g.Board.DefaultNotationToSquare(notation);
      Piece piece = new Piece(g, player, type, square);
      g.AddPiece(piece);
    }

    // Snapshot the per-square identity of every piece on the board, so we can
    // assert that move generation (which performs temporary Make+Unmake per
    // candidate when LegalMovesOnly=true) leaves the board pristine.
    private static Piece[] SnapshotBoard(Game g)
    {
      var snap = new Piece[g.Board.NumSquares];
      for (int sq = 0; sq < g.Board.NumSquares; sq++)
        snap[sq] = g.Board[sq];
      return snap;
    }

    private static void AssertBoardUnchanged(Game g, Piece[] before, string context)
    {
      for (int sq = 0; sq < g.Board.NumSquares; sq++)
      {
        if (!object.ReferenceEquals(before[sq], g.Board[sq]))
        {
          Assert.Fail(
            $"{context}: board state mutated at square {sq} " +
            $"({g.Board.GetDefaultSquareNotation(sq)}). " +
            $"before={(before[sq] == null ? "empty" : before[sq].PieceType.Name + ":" + before[sq].Player)} " +
            $"after={(g.Board[sq] == null ? "empty" : g.Board[sq].PieceType.Name + ":" + g.Board[sq].Player)}");
        }
      }
    }

    // For each generated move, validate that every pickup references a square
    // that currently contains a non-null piece. Mirrors the precondition
    // MoveList.PerformPickup relies on (Board.ClearSquare throws if the square
    // is empty). A failure here is the clean reproduction of the crash
    // family we're hunting.
    private static void AssertPickupsReferenceLivePieces(MoveList ml, Game g)
    {
      for (int i = 0; i < ml.MoveCursor; i++)
      {
        var mv = ml.GetMoveForTest(i);
        int firstPickup = i == 0 ? 0 : ml.GetMoveForTest(i - 1).PickupCursor;
        for (int p = firstPickup; p < mv.PickupCursor; p++)
        {
          var pu = ml.GetPickupForTest(p);
          Piece actual = g.Board[pu.Square];
          Assert.IsNotNull(
            actual,
            $"Move {i} ({mv}) pickup#{p - firstPickup} references empty square " +
            $"{pu.Square} ({g.Board.GetDefaultSquareNotation(pu.Square)}). " +
            "This is the PerformPickup crash signature - the move list contains " +
            "a move whose pickup target is not present on the board.");
        }
      }
    }

    private static void GenerateAndValidate(Game g, int player, string scenario)
    {
      var before = SnapshotBoard(g);
      var ml = g.RootMoveListForTest;
      int preMove = ml.MoveCursor;
      int prePickup = ml.PickupCursorForTest;
      int preDrop = ml.DropCursorForTest;

      try
      {
        g.GenerateMovesForTest(player);
      }
      catch (InvalidBoardStateException ex)
      {
        Assert.Fail(
          $"{scenario}: generateMoves threw InvalidBoardStateException at square " +
          $"{ex.Square} ({ex.SquareNotation}): {ex.Message}. This reproduces the " +
          "PerformPickup crash family from the 2025-09-12 and 2026-02-12 reports.");
      }

      AssertBoardUnchanged(g, before, scenario);
      AssertPickupsReferenceLivePieces(ml, g);

      // Cursors should monotonically advance (or stay flat if no moves).
      Assert.IsTrue(ml.MoveCursor >= preMove, $"{scenario}: moveCursor went backwards.");
      Assert.IsTrue(ml.PickupCursorForTest >= prePickup, $"{scenario}: pickupCursor went backwards.");
      Assert.IsTrue(ml.DropCursorForTest >= preDrop, $"{scenario}: dropCursor went backwards.");
    }

    // ---------------------------------------------------------------------
    // Scenario 1 - Initial position (standard chess + unused Checkers/Cannon
    // types loaded) produces a clean generation pass.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void InitialStandardPosition_GenerationIsClean()
    {
      var g = CreateGame();
      LoadPosition(g, "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
      GenerateAndValidate(g, 0, "InitialStandardPosition");
      Assert.IsTrue(g.RootMoveListForTest.MoveCursor > 0, "Expected at least one move from initial position.");
    }

    // ---------------------------------------------------------------------
    // Scenario 2 - Single Checkers jump opportunity. The 2026-02-12 crash
    // triggered during generation of Checkers moves; this mirrors the
    // minimal configuration.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void CheckersSingleJump_PickupsPointAtEnemy()
    {
      var g = CreateGame();
      //   rank 8: 7k  (Black King at h8)
      //   rank 4: black Checkers (e) at d4
      //   rank 3: white Checkers (E) at e3
      //   rank 1: K7  (White King at a1)
      LoadPosition(g, "7k/8/8/8/3e4/4E3/8/K7");

      GenerateAndValidate(g, 0, "CheckersSingleJump");

      var ml = g.RootMoveListForTest;
      bool foundJump = false;
      int d4 = Sq(g, "d4");
      int c5 = Sq(g, "c5");
      for (int i = 0; i < ml.MoveCursor; i++)
      {
        var mv = ml.GetMoveForTest(i);
        if (mv.ToSquare != c5) continue;
        int first = i == 0 ? 0 : ml.GetMoveForTest(i - 1).PickupCursor;
        for (int p = first; p < mv.PickupCursor; p++)
        {
          var pu = ml.GetPickupForTest(p);
          if (pu.Square == d4)
          {
            foundJump = true;
            Assert.IsNotNull(g.Board[pu.Square], "d4 must hold the enemy piece pre-capture.");
            Assert.AreEqual(1, g.Board[pu.Square].Player);
          }
        }
      }
      Assert.IsTrue(foundJump, "Expected a Checkers e3xd4->c5 jump move with a pickup at d4.");
    }

    // ---------------------------------------------------------------------
    // Scenario 3 - Two-jump chain: a1 -> c3 -> e5 capturing b2 and d4.
    // Stresses GenerateJumpCaptures recursion and the pickup chain.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void CheckersDoubleJumpChain_AllPickupsValid()
    {
      var g = CreateGame();
      //   rank 8: 7k   (Black King at h8)
      //   rank 4: 3e4  (Black Checkers at d4)
      //   rank 2: 1e5K (Black Checkers at b2, White King at h2)
      //   rank 1: E7   (White Checkers at a1)
      LoadPosition(g, "7k/8/8/8/3e4/8/1e5K/E7");

      GenerateAndValidate(g, 0, "CheckersDoubleJumpChain");

      var ml = g.RootMoveListForTest;
      int b2 = Sq(g, "b2"), d4 = Sq(g, "d4"), e5 = Sq(g, "e5");
      bool foundChain = false;
      for (int i = 0; i < ml.MoveCursor; i++)
      {
        var mv = ml.GetMoveForTest(i);
        if (mv.ToSquare != e5) continue;
        int first = i == 0 ? 0 : ml.GetMoveForTest(i - 1).PickupCursor;
        bool sawB2 = false, sawD4 = false;
        for (int p = first; p < mv.PickupCursor; p++)
        {
          var pu = ml.GetPickupForTest(p);
          if (pu.Square == b2) sawB2 = true;
          if (pu.Square == d4) sawD4 = true;
        }
        if (sawB2 && sawD4) foundChain = true;
      }
      Assert.IsTrue(foundChain,
        "Expected a double-jump a1->c3->e5 with pickups at both b2 AND d4.");
    }

    // ---------------------------------------------------------------------
    // Scenario 4 - Jump that lands on the promotion rank (rank 7). Exercises
    // ListForSkipCapture's PromotionProperty branch.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void CheckersJumpToPromotionRank_Succeeds()
    {
      var g = CreateGame();
      //   rank 8: 8
      //   rank 7: 4e3    (Black Checkers at e7)
      //   rank 6: 3E4    (White Checkers at d6)
      //   rank 1: K6k    (White King a1, Black King h1)
      LoadPosition(g, "8/4e3/3E4/8/8/8/8/K6k");

      GenerateAndValidate(g, 0, "CheckersJumpToPromotionRank");

      var ml = g.RootMoveListForTest;
      int f8 = Sq(g, "f8");
      bool foundPromo = false;
      for (int i = 0; i < ml.MoveCursor; i++)
      {
        var mv = ml.GetMoveForTest(i);
        if (mv.ToSquare == f8 && (mv.MoveType & MoveType.PromotionProperty) != 0)
          foundPromo = true;
      }
      Assert.IsTrue(foundPromo, "Expected a promoting Checkers jump to f8.");
    }

    // ---------------------------------------------------------------------
    // Scenario 5 - Cannon screen capture: the cannon must jump over exactly
    // one piece (the "screen") and land on an enemy past it. Pickup must
    // point at the enemy (a7), NOT the screen (a4).
    // ---------------------------------------------------------------------
    [TestMethod]
    public void CannonScreenCapture_PickupIsTargetNotScreen()
    {
      var g = CreateGame();
      //   rank 8: 7k      (Black King h8)
      //   rank 7: p7      (Black Pawn a7 - target)
      //   rank 4: P7      (White Pawn a4 - screen)
      //   rank 1: I6K     (White Cannon a1, White King h1)
      LoadPosition(g, "7k/p7/8/8/P7/8/8/I6K");

      GenerateAndValidate(g, 0, "CannonScreenCapture");

      var ml = g.RootMoveListForTest;
      int a4 = Sq(g, "a4"), a7 = Sq(g, "a7"), a1 = Sq(g, "a1");
      bool foundCapture = false;
      for (int i = 0; i < ml.MoveCursor; i++)
      {
        var mv = ml.GetMoveForTest(i);
        if (mv.FromSquare != a1 || mv.ToSquare != a7) continue;
        int first = i == 0 ? 0 : ml.GetMoveForTest(i - 1).PickupCursor;
        bool pickedUpScreen = false;
        bool pickedUpTarget = false;
        for (int p = first; p < mv.PickupCursor; p++)
        {
          var pu = ml.GetPickupForTest(p);
          if (pu.Square == a4) pickedUpScreen = true;
          if (pu.Square == a7) pickedUpTarget = true;
        }
        Assert.IsFalse(pickedUpScreen,
          "Cannon capture must NOT pick up the screen piece at a4.");
        Assert.IsTrue(pickedUpTarget,
          "Cannon capture must pick up the target piece at a7.");
        foundCapture = true;
      }
      Assert.IsTrue(foundCapture, "Expected a Cannon a1xa7 capture.");
    }

    // ---------------------------------------------------------------------
    // Scenario 6 - Mixed Checkers + Cannon on the same board with targets
    // aimed at interacting squares. Ensures that one piece's temporary
    // Make/Unmake during LegalMovesOnly validation does not corrupt the
    // other's pickups (the tempPickupCursor/tempDropCursor shared-field
    // hypothesis from MoveList.cs:583-584).
    // ---------------------------------------------------------------------
    [TestMethod]
    public void MixedCheckersAndCannon_NoPickupCorruption()
    {
      var g = CreateGame();
      //   rank 8: 7k      (Black King h8)
      //   rank 7: p7      (Black Pawn a7 - Cannon target)
      //   rank 4: P2e4    (White Pawn a4 screen, Black Checkers d4)
      //   rank 3: 4E3     (White Checkers e3)
      //   rank 1: I6K     (White Cannon a1, White King h1)
      LoadPosition(g, "7k/p7/8/8/P2e4/4E3/8/I6K");

      GenerateAndValidate(g, 0, "MixedCheckersAndCannon");

      var ml = g.RootMoveListForTest;
      int a1 = Sq(g, "a1"), a7 = Sq(g, "a7"), a4 = Sq(g, "a4");
      int e3 = Sq(g, "e3"), d4 = Sq(g, "d4"), c5 = Sq(g, "c5");
      bool cannonOK = false, checkersOK = false;
      for (int i = 0; i < ml.MoveCursor; i++)
      {
        var mv = ml.GetMoveForTest(i);
        int first = i == 0 ? 0 : ml.GetMoveForTest(i - 1).PickupCursor;
        if (mv.FromSquare == a1 && mv.ToSquare == a7)
        {
          bool okCannon = true;
          for (int p = first; p < mv.PickupCursor; p++)
          {
            var pu = ml.GetPickupForTest(p);
            if (pu.Square == a4) okCannon = false;
          }
          if (okCannon) cannonOK = true;
        }
        if (mv.FromSquare == e3 && mv.ToSquare == c5)
        {
          for (int p = first; p < mv.PickupCursor; p++)
          {
            var pu = ml.GetPickupForTest(p);
            if (pu.Square == d4) checkersOK = true;
          }
        }
      }
      Assert.IsTrue(cannonOK, "Cannon a1xa7 must be present with correct pickups.");
      Assert.IsTrue(checkersOK, "Checkers e3xd4->c5 must be present with correct pickups.");
    }
  }
}
