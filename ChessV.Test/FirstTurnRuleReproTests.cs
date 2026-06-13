using System.Collections.Generic;
using ChessV;
using ChessV.Games;
using ChessV.Games.Pieces.Apmw;
using ChessV.Games.Rules.Apmw;

namespace ChessV.Test
{
  // Regression tests for the persistent crash documented in
  // _refs/2026-02-12.txt: during Player 1's response generation on the
  // first turn, ApmwFirstTurnRule.GenerateSpecialMoves throws
  // "No piece to clear at square 9 (b2)" out of MoveList.PerformPickup
  // (called from EndMoveAdd's LegalMovesOnly Make/Unmake validation).
  //
  // Strategy:
  //   - Build the smallest possible variant that still wires up the rule
  //     by extending the existing CheckersCannonTestGame scaffolding with
  //     ApmwFirstTurnRule. CheckmateRule is supplied by base Chess so the
  //     rule's Initialize() finds it.
  //   - Construct a position where Player 1 (Black) is in check on the
  //     first turn, and the attacker cannot be captured by any normal
  //     move - that is the exact branch (lines 127-149 of
  //     ApmwFirstTurnRule.cs) that issues BaroqueCapture pickups whose
  //     squares the report says go stale.
  //   - For each scenario snapshot the board pre-generation, invoke
  //     Game.GenerateMovesForTest(CurrentSide), and assert (a) no
  //     exception, (b) board pristine, (c) every recorded pickup still
  //     references a live piece.

  [Game("Apmw FirstTurnRule Repro Variant",
      typeof(Geometry.Rectangular), 8, 8,
      Template = true)]
  public class FirstTurnRuleTestGame : Chess
  {
    public Checkers CheckersType;
    public Cannon CannonType;

    public override void SetGameVariables()
    {
      base.SetGameVariables();
      Castling.Value = "None";
      Array = "8/8/8/8/8/8/8/8";
      FENStart = "8/8/8/8/8/8/8/8 w - - 0 1";
      PromotionTypes = "QRNB";
      EnPassant = false;
    }

    public override void AddPieceTypes()
    {
      base.AddPieceTypes();
      // Use letters that don't collide with K/Q/R/B/N/P.
      CheckersType = new Checkers("Checkers", "E", 150, 200);
      AddPieceType(CheckersType);
      CannonType = new Cannon("Cannon", "I", 400, 400);
      AddPieceType(CannonType);
    }

    public override void AddRules()
    {
      base.AddRules();
      CheckersType.SetPromotionTypes(new List<PieceType> { Queen, Rook, Bishop, Knight });
      // The unit under test.
      AddRule(new ApmwFirstTurnRule());
    }
  }

  [TestClass]
  public class FirstTurnRuleReproTests
  {
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

    // Same as LoadPosition but never throws - returns the raised exception
    // (or null on success). Used by tests that consider a LoadFEN-time
    // generateMoves crash itself a positive reproduction of the bug.
    private static System.Exception TryLoadPosition(FirstTurnRuleTestGame g, string array, char side)
    {
      try
      {
        g.LoadFEN(array + " " + side + " - - 0 1");
        return null;
      }
      catch (System.Exception ex)
      {
        return ex;
      }
    }

    private static int Sq(Game g, string notation) => g.Board.DefaultNotationToSquare(notation);

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

    private static void AssertPickupsReferenceLivePieces(MoveList ml, Game g, string context)
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
            $"{context}: move {i} ({mv}) pickup#{p - firstPickup} references EMPTY square " +
            $"{pu.Square} ({g.Board.GetDefaultSquareNotation(pu.Square)}). " +
            "This is the PerformPickup crash signature from _refs/2026-02-12.txt.");
        }
      }
    }

    private static void GenerateAndValidate(FirstTurnRuleTestGame g, int player, string scenario)
    {
      var before = SnapshotBoard(g);
      var ml = g.RootMoveListForTest;
      try
      {
        g.GenerateMovesForTest(player);
      }
      catch (InvalidBoardStateException ex)
      {
        Assert.Fail(
          $"{scenario}: generateMoves threw InvalidBoardStateException at square " +
          $"{ex.Square} ({ex.SquareNotation}): {ex.Message}. This reproduces the " +
          "ApmwFirstTurnRule corruption crash from _refs/2026-02-12.txt.");
      }
      catch (System.Exception ex)
      {
        Assert.Fail(
          $"{scenario}: generateMoves threw {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
      }

      AssertBoardUnchanged(g, before, scenario);
      AssertPickupsReferenceLivePieces(ml, g, scenario);
    }

    private static int CountMovesOfType(MoveList ml, MoveType moveType)
    {
      int count = 0;
      for (int i = 0; i < ml.MoveCursor; i++)
        if (ml.GetMoveForTest(i).MoveType == moveType)
          count++;
      return count;
    }

    [TestMethod]
    public void FirstTurn_AttackerNotNormallyCapturable_EmitsBaroqueCapture()
    {
      var g = CreateGame();
      LoadPosition(g, "4k3/8/8/8/8/4R3/8/4K3", 'b');

      GenerateAndValidate(g, g.CurrentSide, "FirstTurn_AttackerNotNormallyCapturable");

      Assert.IsTrue(
        CountMovesOfType(g.RootMoveListForTest, MoveType.BaroqueCapture) > 0,
        "ApmwFirstTurnRule should emit emergency BaroqueCapture moves when no normal capture can answer check.");
    }

    [TestMethod]
    public void FirstTurn_AttackerNormallyCapturable_DoesNotEmitBaroqueCapture()
    {
      var g = CreateGame();
      LoadPosition(g, "4k3/4R3/8/8/8/8/8/4K3", 'b');

      GenerateAndValidate(g, g.CurrentSide, "FirstTurn_AttackerNormallyCapturable");

      Assert.AreEqual(
        0,
        CountMovesOfType(g.RootMoveListForTest, MoveType.BaroqueCapture),
        "ApmwFirstTurnRule should not add emergency BaroqueCapture moves when a normal capture exists.");
    }

    // ---------------------------------------------------------------------
    // (a) Minimal in-check first-turn position: White Rook on e3 attacks
    //     Black King on e8 along the empty e-file. Black has only Checkers
    //     and a King, so no normal move can capture the rook (Checkers
    //     pieces don't have CanCapture set on their step moves), forcing
    //     ApmwFirstTurnRule into the BaroqueCapture branch for every
    //     friendly piece on a valid direction line to e3.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void FirstTurn_BlackInCheck_NoNormalCapture_GenerationStaysClean()
    {
      var g = CreateGame();
      //   8: 4k3   (Black King e8)
      //   7: 4e3   (Black Checkers e7) - on the e-file with attacker
      //   5: 3e4   (Black Checkers d5) - diagonally aligned to e3
      //   3: 4R3   (White Rook e3 - the attacker)
      //   1: 4K3   (White King e1)
      LoadPosition(g, "4k3/4e3/8/3e4/8/4R3/8/4K3", 'b');
      Assert.AreEqual(1, g.CurrentSide, "Black must be to move for ApmwFirstTurnRule to fire on Black.");
      Assert.IsTrue(g.GameTurnNumber <= 2, "Rule guards on GameTurnNumber > 2.");

      GenerateAndValidate(g, g.CurrentSide, "FirstTurn_BlackInCheck_NoNormalCapture");
    }

    // ---------------------------------------------------------------------
    // (b) Snapshot-and-compare with Checkers multi-jump available to the
    //     side to move IN ADDITION to being in check on the first turn.
    //     This is the dangerous combination the report points at: the
    //     per-piece Checkers Make/Unmake runs first; the special-move rule
    //     runs after; if Checkers leaves any board square stale, the rule
    //     will read it.
    // ---------------------------------------------------------------------
    // ---------------------------------------------------------------------
    // (b) **Repros _refs/2026-02-12.txt** — REGRESSION FAILING TEST.
    //
    // Snapshot-and-compare with Checkers multi-jump available to the side
    // to move IN ADDITION to being in check on the first turn. This is the
    // dangerous combination the report points at: the per-piece Checkers
    // Make/Unmake runs first; the special-move rule runs after; if
    // Checkers leaves any board square stale, the rule (or the very next
    // standard step move on the same Checkers piece) will read it.
    //
    // Empirically this position triggers
    //   InvalidBoardStateException: No piece to clear at square 36 (e5)
    // *during* LoadFEN's implicit generateMoves pass: AddMove for the
    // Checkers' standard NW step move calls MakeMove, which calls
    // PerformPickup on e5 — and e5 is empty, even though AddMove saw a
    // piece there a few lines earlier. That window only contains the
    // CustomMoveGenerator's prior multi-jump Make/Unmake cycles, so the
    // corruption is in
    //   Pieces/Apmw/Checkers.GenerateMultiCaptureMoves (and/or in the
    //   MoveList.MakeMove/UnmakeMove pickup chain it depends on).
    //
    // This is the same crash family as the report: PerformPickup on an
    // empty square, with a stale pickup square recorded earlier in the
    // generation pass. Leaving the test live (NOT [Ignore]) so the build
    // surfaces it until the underlying corruption is fixed.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void Repros_2026_02_12_CheckersMultiJumpCorruptsBoardDuringGeneration()
    {
      var g = CreateGame();
      //   8: 4k3      Black King e8
      //   7: 7e       Black Checkers h7
      //   6: 6P1      White Pawn g6  (h7 jumps over g6 to f5 if f5 empty)
      //   5: 4e3      Black Checkers e5
      //   4: 3P4      White Pawn d4  (e5 jumps over d4 to c3 if c3 empty)
      //   3: 4R3      White Rook e3 (attacker on e-file vs. e8 king)
      //   1: 4K3      White King e1
      System.Exception loadErr = TryLoadPosition(g, "4k3/7e/6P1/4e3/3P4/4R3/8/4K3", 'b');

      if (loadErr is InvalidBoardStateException ibse)
      {
        // Direct repro at LoadFEN time. Surface it as a failure so the
        // regression is visible and the message is searchable in CI.
        Assert.Fail(
          "Repros _refs/2026-02-12.txt: LoadFEN's implicit generateMoves " +
          $"raised InvalidBoardStateException at square {ibse.Square} " +
          $"({ibse.SquareNotation}): {ibse.Message}. The PerformPickup site " +
          "was reached on a board square that no longer holds a piece, " +
          "matching the crash family in the report.");
      }
      Assert.IsNull(loadErr,
        "Repros _refs/2026-02-12.txt: LoadFEN raised an unexpected exception: " +
        (loadErr == null ? "<none>" : loadErr.GetType().Name + ": " + loadErr.Message));

      // If LoadFEN somehow stops crashing in the future, also exercise the
      // explicit GenerateMovesForTest path so the test still has teeth.
      GenerateAndValidate(g, g.CurrentSide, "Repros_2026_02_12_CheckersMultiJump");
    }

    // ---------------------------------------------------------------------
    // (c) Stress: Player 1 has multiple Checkers, a Cannon, and pawns;
    //     Player 0 attacks the king; same generation pass must complete
    //     without any pickup pointing at an empty square.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void FirstTurn_StressMultipleCheckersCannonPawns_NoCorruption()
    {
      var g = CreateGame();
      //   8: 4k3      Black King e8
      //   7: ppp1pppp Black Pawns
      //   6: 2e2e2    Black Checkers c6, f6
      //   5: 7I       Black Cannon h5
      //   4: 3e4      Black Checkers d4
      //   3: 4R3      White Rook e3 attacks Black king on e-file (e7 pawn blocks!)
      //               -> need to clear e7. Use:
      //   7: pppp1ppp Black Pawns minus e7
      //   1: 4K3      White King e1
      // Reconstructed FEN with e7 empty so the Rook actually checks:
      //   8: 4k3
      //   7: pppp1ppp
      //   6: 2e2e2
      //   5: 7I
      //   4: 3e4
      //   3: 4R3
      //   2: 8
      //   1: 4K3
      LoadPosition(g, "4k3/pppp1ppp/2e2e2/7I/3e4/4R3/8/4K3", 'b');

      GenerateAndValidate(g, g.CurrentSide, "FirstTurn_StressMultipleCheckersCannonPawns");
    }

    // ---------------------------------------------------------------------
    // (d) Repros _refs/2026-02-12.txt: the report's outer move was Player 0
    //     Checkers e3->d4. We can't faithfully replay the live ApmwChess
    //     opening (its piece layout depends on randomized providers), so
    //     instead we model the *narrow trigger conditions*: Player 1 (the
    //     side computing its response) is in check on the first turn from
    //     a piece that no normal capture can answer, AND Player 1 has a
    //     friendly piece on the b2 square that the report's stack trace
    //     blames. ApmwFirstTurnRule will iterate that b2 piece in its
    //     BaroqueCapture loop; if any earlier per-piece Make/Unmake left
    //     b2 stale, the AddPickup -> EndMoveAdd -> PerformPickup chain
    //     will throw "No piece to clear at square 9 (b2)" exactly as in
    //     the report.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void Repros_2026_02_12_BaroqueCaptureFromB2_AfterCheckersGeneration()
    {
      var g = CreateGame();
      //   8: 4k3      Black King e8 (in check)
      //   6: 4e3      Black Checkers e6 (on e-file with attacker)
      //   4: 4R3      White Rook e4 - the checker. e7..e5 must be empty.
      //               Actually need it on e3-e7 path empty. Use Rook at e4
      //               with e5,e6 also empty. Move Black Checkers off e6.
      //   So:
      //   8: 4k3
      //   7: 8
      //   6: 8
      //   5: 8
      //   4: 4R3       attacker
      //   3: 8
      //   2: 1e6       Black Checkers b2  <-- the square the crash report names
      //   1: 4K3
      // Need also: the b2 Checkers must satisfy DirectionLookup(b2, e4) >= 0
      // for the rule's "find friendly pieces aligned with attacker" loop to
      // include it. b2->e4 isn't on a king-line (file +3, rank +2). To
      // ensure a valid direction, place an additional Checkers on a square
      // diagonally aligned with e4, and keep b2 as a non-aligned witness.
      //
      // Arrange so MULTIPLE pieces qualify:
      //   - d3 Checkers: NE diagonal to e4 (direction valid)
      //   - e2 Checkers: N to e4 (direction valid)
      //   - b2 Checkers: not on a king-line to e4 -> rule will skip via
      //     DirectionLookup<0 and NOT add a pickup. So this exercises the
      //     "skip non-aligned" branch on b2.
      LoadPosition(g, "4k3/8/8/8/4R3/3e4/1e2e3/4K3", 'b');

      GenerateAndValidate(g, g.CurrentSide, "Repros_2026_02_12_BaroqueCaptureFromB2");
    }

    // ---------------------------------------------------------------------
    // (e) Variant of (d) where the b2 friendly piece IS on a valid
    //     direction line to the attacker, so the BaroqueCapture branch
    //     actually emits a pickup at b2. If anything earlier in the
    //     generation pass cleared b2, this is where the crash will land.
    // ---------------------------------------------------------------------
    [TestMethod]
    public void Repros_2026_02_12_BaroqueCaptureWithB2Aligned()
    {
      var g = CreateGame();
      // Place attacker at b8 so Black King on e8 is on the rank with it
      // (needs e8..c8 clear - 4k3 leaves that path clear of black pieces;
      // attacker is white so own pieces don't matter). But we also need
      // a king-line direction from b2 to b8 (yes: due North).
      //
      //   8: 1R2k3   White Rook b8 attacks Black King e8 along rank 8
      //   2: 1e6     Black Checkers b2 (aligned NORTH to b8 attacker)
      //   1: 4K3     White King e1
      LoadPosition(g, "1R2k3/8/8/8/8/8/1e6/4K3", 'b');

      GenerateAndValidate(g, g.CurrentSide, "Repros_2026_02_12_BaroqueCaptureWithB2Aligned");
    }
  }
}
