using System;
using System.Collections.Generic;
using System.Reflection;
using ChessV;
using ChessV.Games;

namespace ChessV.Test
{
  // *** CROSS-GAME MAKE/UNMAKE SAFETY TESTS ***
  //
  // Commit 022b223 removed two writes from ChessV.Base/MoveList.cs
  // MakeMove() at the top of the method:
  //
  //     tempPickupCursor = pickupCursor;
  //     tempDropCursor   = dropCursor;
  //
  // Those were dead writes - MakeMove never reads them. The fields are
  // owned by BeginMoveAdd / EndMoveAddCore as a snapshot used to roll back
  // the global cursors when a generated move turns out to be illegal.
  // Aliasing them inside MakeMove silently corrupted the rollback target
  // and caused the Checkers multi-jump corruption documented in
  // FirstTurnRuleReproTests.Repros_2026_02_12_*.
  //
  // The Checkers regression tests already prove the *fix* works for the
  // original reproducer. These tests add cross-game coverage for ordinary
  // chess variants whose Make/Unmake interactions were never broken, so we
  // can be confident the removal doesn't introduce a NEW regression.
  //
  // For each scenario we:
  //   a) Build a realistic FEN that allows the target move.
  //   b) Load the position; generate legal moves; locate the move.
  //   c) Snapshot the full board (piece+player per square, side to move)
  //      AND the MoveList cursor fields (pickupCursor, dropCursor,
  //      tempPickupCursor, tempDropCursor, moveCursor) - the last two via
  //      reflection because they're private.
  //   d) MakeMove and assert the post-Make state DIFFERS from pre-Make.
  //   e) UnmakeMove and assert the post-Unmake state matches pre-Make
  //      byte-for-byte (board + side to move + cursor fields).
  //
  // We also add a "bulk" test per game type that round-trips EVERY
  // generated move from a realistic position. The bulk tests exercise the
  // EndMoveAddCore legality-check path - the exact path where the removed
  // writes were biting Checkers - across many disparate moves.

  // *** TEST-ONLY GAME VARIANTS ***
  //
  // The base Chess / EurasianChess classes set Array to their full starting
  // position. LoadFEN then places those pieces, but does NOT clear the
  // board first - so calling LoadFEN after Initialize would re-add pieces
  // on top of the starting array.
  //
  // We can't simply override Array to "8/8/.../8" the way other test games
  // (CheckersCannonTestGame, FirstTurnRuleTestGame) do because Generic8x8
  // / Generic10x10 castling registration requires the king to start on
  // d1 or e1 (etc.), and we want castling enabled for the chess scenarios.
  //
  // Solution: keep the standard starting Array so castling can register,
  // but RESET the board + game piece tables before each test's LoadFEN
  // so the new FEN's pieces land on a clean slate. ResetForLoadFEN does
  // both via reflection (nPieces is protected).

  [Game("Cross-Game Safety Test Eurasian",
      typeof(Geometry.Rectangular), 10, 10,
      Template = true)]
  public class CrossGameSafetyTestEurasian : EurasianChess
  {
    public override void SetGameVariables()
    {
      base.SetGameVariables();
      // Castling on a 10x10 board needs the king on its starting square
      // (e1/f1 depending on Eurasian's value). Disable it because our
      // hand-placed king sits elsewhere; the cannon scenarios under test
      // do not depend on castling.
      Castling.Value = "None";
      Array = "10/10/10/10/10/10/10/10/10/10";
      FENStart = "10/10/10/10/10/10/10/10/10/10 w - - 0 1";
    }
  }

  [TestClass]
  public class MakeMoveCrossGameSafetyTests
  {
    // --- reflection plumbing ---------------------------------------------

    private static readonly MethodInfo s_unmakeByIndex =
      typeof(MoveList).GetMethod(
        "UnmakeMove",
        BindingFlags.Instance | BindingFlags.NonPublic,
        binder: null,
        types: new Type[] { typeof(int) },
        modifiers: null);

    private static readonly FieldInfo s_tempPickupCursor =
      typeof(MoveList).GetField(
        "tempPickupCursor",
        BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo s_tempDropCursor =
      typeof(MoveList).GetField(
        "tempDropCursor",
        BindingFlags.Instance | BindingFlags.NonPublic);

    private static void Unmake(MoveList ml, int index)
    {
      if (s_unmakeByIndex == null)
        throw new InvalidOperationException(
          "MoveList.UnmakeMove(int) not found via reflection - signature change?");
      try
      {
        s_unmakeByIndex.Invoke(ml, new object[] { index });
      }
      catch (TargetInvocationException tie) when (tie.InnerException != null)
      {
        throw tie.InnerException;
      }
    }

    private static int TempPickupCursor(MoveList ml)
    {
      if (s_tempPickupCursor == null)
        throw new InvalidOperationException("tempPickupCursor field not found.");
      return (int)s_tempPickupCursor.GetValue(ml);
    }

    private static int TempDropCursor(MoveList ml)
    {
      if (s_tempDropCursor == null)
        throw new InvalidOperationException("tempDropCursor field not found.");
      return (int)s_tempDropCursor.GetValue(ml);
    }

    // --- game construction -----------------------------------------------

    private static readonly FieldInfo s_nPieces =
      typeof(Game).GetField("nPieces", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo s_pieces =
      typeof(Game).GetField("pieces", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo s_rules =
      typeof(Game).GetField("rules", BindingFlags.Instance | BindingFlags.NonPublic);

    // Wipe the board AND the per-player piece tables AND any per-rule
    // caches (like CheckmateRule.RoyalPieces) that PositionLoaded would
    // otherwise APPEND to instead of replacing - so a subsequent LoadFEN
    // starts from a truly empty position. LoadFEN itself does not do
    // this, so calling it twice (once implicitly during Initialize, once
    // explicitly from the test) double-counts pieces and leaks stale
    // royal-piece references whose Square is -1.
    private static void ResetForLoadFEN(Game g)
    {
      g.Board.ClearBoard();
      var nPieces = (int[])s_nPieces.GetValue(g);
      for (int p = 0; p < nPieces.Length; p++)
        nPieces[p] = 0;
      var pieces = (Piece[,])s_pieces.GetValue(g);
      for (int p = 0; p < pieces.GetLength(0); p++)
        for (int i = 0; i < pieces.GetLength(1); i++)
          pieces[p, i] = null;

      // CheckmateRule.PositionLoaded *adds* to RoyalPieces; reset it so
      // the second LoadFEN doesn't end up with a stale +1 captured-king
      // reference whose .Square is -1 (which crashes IsSquareAttacked).
      var rules = (System.Collections.IList)s_rules.GetValue(g);
      foreach (var rule in rules)
      {
        var royalsField = rule.GetType().GetField(
          "RoyalPieces",
          BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (royalsField == null) continue;
        var arr = royalsField.GetValue(rule) as Array;
        if (arr == null) continue;
        for (int i = 0; i < arr.Length; i++)
          arr.SetValue(null, i);
      }
    }

    private static T CreateGame<T>() where T : Game, new()
    {
      var g = new T();
      var attrs = typeof(T).GetCustomAttributes(typeof(GameAttribute), inherit: false);
      Assert.IsTrue(attrs.Length > 0, typeof(T).Name + " must carry a [Game] attribute.");
      g.Initialize((GameAttribute)attrs[0], null, null);
      return g;
    }

    // --- snapshots -------------------------------------------------------

    private struct Snap
    {
      public ulong Hash;
      public int[] Squares;       // Per-square: -1 if empty, else type*1000+player.
      public int CurrentSide;
      public int PickupCursor;
      public int DropCursor;
      public int TempPickupCursor;
      public int TempDropCursor;
      public int MoveCursor;
    }

    private static Snap Snapshot(Game g)
    {
      var ml = g.RootMoveListForTest;
      var s = new Snap
      {
        Hash = g.Board.HashCode,
        Squares = new int[g.Board.NumSquaresExtended],
        CurrentSide = g.CurrentSide,
        PickupCursor = ml.PickupCursorForTest,
        DropCursor = ml.DropCursorForTest,
        TempPickupCursor = TempPickupCursor(ml),
        TempDropCursor = TempDropCursor(ml),
        MoveCursor = ml.MoveCursor,
      };
      for (int i = 0; i < s.Squares.Length; i++)
      {
        var p = g.Board[i];
        s.Squares[i] = p == null ? -1 : p.PieceType.TypeNumber * 1000 + p.Player;
      }
      return s;
    }

    private static void AssertSquaresEqual(Snap a, Snap b, Game g, string ctx)
    {
      Assert.AreEqual(a.Hash, b.Hash, ctx + ": Board.HashCode mismatch");
      Assert.AreEqual(a.CurrentSide, b.CurrentSide, ctx + ": CurrentSide mismatch");
      for (int i = 0; i < a.Squares.Length; i++)
      {
        if (a.Squares[i] != b.Squares[i])
        {
          string note = i < g.Board.NumSquares
            ? " (" + g.Board.GetDefaultSquareNotation(i) + ")"
            : "";
          Assert.Fail(ctx + ": square " + i + note +
                      " expected=" + a.Squares[i] + " actual=" + b.Squares[i]);
        }
      }
    }

    private static void AssertCursorsEqual(Snap a, Snap b, string ctx)
    {
      Assert.AreEqual(a.PickupCursor, b.PickupCursor, ctx + ": pickupCursor mismatch");
      Assert.AreEqual(a.DropCursor, b.DropCursor, ctx + ": dropCursor mismatch");
      Assert.AreEqual(a.TempPickupCursor, b.TempPickupCursor, ctx + ": tempPickupCursor mismatch");
      Assert.AreEqual(a.TempDropCursor, b.TempDropCursor, ctx + ": tempDropCursor mismatch");
      Assert.AreEqual(a.MoveCursor, b.MoveCursor, ctx + ": moveCursor mismatch");
    }

    private static bool SquaresDiffer(Snap a, Snap b)
    {
      if (a.Hash != b.Hash) return true;
      for (int i = 0; i < a.Squares.Length; i++)
        if (a.Squares[i] != b.Squares[i]) return true;
      return false;
    }

    // --- move location ---------------------------------------------------

    // Locate a unique move matching the predicate. Fails the test if zero
    // or more than one matches, with diagnostic context.
    private static int FindMoveIndex(
      MoveList ml,
      Func<MoveInfo, bool> match,
      string description)
    {
      int found = -1;
      int matches = 0;
      for (int i = 0; i < ml.Count; i++)
      {
        if (match(ml.GetMoveForTest(i)))
        {
          found = i;
          matches++;
        }
      }
      if (matches != 1)
      {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Expected exactly one " + description + " move; found " +
                      matches + " out of " + ml.Count + " generated moves.");
        sb.AppendLine("All generated moves:");
        for (int i = 0; i < ml.Count; i++)
          sb.AppendLine("  [" + i + "] " + ml.GetMoveForTest(i));
        Assert.Fail(sb.ToString());
      }
      return found;
    }

    // --- core round-trip assertion ---------------------------------------

    private static void AssertRoundTrip(Game g, int moveIndex, string ctx)
    {
      var ml = g.RootMoveListForTest;
      var pre = Snapshot(g);

      bool made = ml.MakeMove(moveIndex);
      Assert.IsTrue(made, ctx + ": MakeMove returned false (move was rejected as illegal).");

      var post = Snapshot(g);
      Assert.IsTrue(SquaresDiffer(pre, post),
        ctx + ": MakeMove produced no observable board change.");

      Unmake(ml, moveIndex);

      var after = Snapshot(g);
      AssertSquaresEqual(pre, after, g, ctx + " [board]");
      AssertCursorsEqual(pre, after, ctx + " [cursors]");
    }

    // Iterate every generated move and run a Make/Unmake round-trip.
    private static int BulkRoundTrip(Game g, string scenario)
    {
      var ml = g.RootMoveListForTest;
      int n = ml.Count;
      Assert.IsTrue(n > 0, scenario + ": no moves generated.");
      for (int i = 0; i < n; i++)
      {
        var pre = Snapshot(g);
        var info = ml.GetMoveForTest(i);
        bool made = ml.MakeMove(i);
        try
        {
          Unmake(ml, i);
        }
        catch (Exception ex)
        {
          Assert.Fail(scenario + $": move {i} ({info}) UnmakeMove threw " +
                      ex.GetType().Name + ": " + ex.Message);
        }
        var after = Snapshot(g);
        AssertSquaresEqual(pre, after, g, scenario + $" move {i} ({info}, made={made}) [board]");
        AssertCursorsEqual(pre, after, scenario + $" move {i} ({info}, made={made}) [cursors]");
      }
      return n;
    }

    // --- helpers for human-readable square notation ---------------------

    private static int Sq(Game g, string algebraic)
    {
      int s = g.NotationToSquare(algebraic);
      Assert.IsTrue(s >= 0 && s < g.Board.NumSquares,
        "NotationToSquare(\"" + algebraic + "\") returned out-of-range " + s);
      return s;
    }

    // ====================================================================
    //                          STANDARD CHESS
    // ====================================================================

    // Mid-game position with both kings, both rooks, and clear castling
    // paths for both colours, both sides. White: rooks a1/h1, king e1,
    // squares b1..d1 and f1..g1 empty. Black: same on rank 8. Minor pieces
    // pushed out of the way.
    private const string CastleReadyFEN =
      "r3k2r/pppq1ppp/2n2n2/3pp3/3PP3/2N2N2/PPPQ1PPP/R3K2R";

    private static Chess CreateChess(string fen)
    {
      var g = CreateGame<Chess>();
      ResetForLoadFEN(g);
      g.LoadFEN(fen);
      g.GenerateMovesForTest(g.CurrentSide);
      return g;
    }

    // --- 1a. White king-side castling (O-O) -----------------------------
    [TestMethod]
    public void Chess_WhiteKingSideCastle_RoundTrips()
    {
      var g = CreateChess(CastleReadyFEN + " w KQkq - 0 8");
      int e1 = Sq(g, "e1");
      int g1 = Sq(g, "g1");
      int idx = FindMoveIndex(
        g.RootMoveListForTest,
        m => m.MoveType == MoveType.Castling && m.FromSquare == e1 && m.ToSquare == g1,
        "white O-O");
      AssertRoundTrip(g, idx, "Chess_WhiteKingSideCastle");
    }

    // --- 1b. Black king-side castling (O-O) -----------------------------
    [TestMethod]
    public void Chess_BlackKingSideCastle_RoundTrips()
    {
      var g = CreateChess(CastleReadyFEN + " b KQkq - 0 8");
      int e8 = Sq(g, "e8");
      int g8 = Sq(g, "g8");
      int idx = FindMoveIndex(
        g.RootMoveListForTest,
        m => m.MoveType == MoveType.Castling && m.FromSquare == e8 && m.ToSquare == g8,
        "black O-O");
      AssertRoundTrip(g, idx, "Chess_BlackKingSideCastle");
    }

    // --- 2a. White queen-side castling (O-O-O) --------------------------
    [TestMethod]
    public void Chess_WhiteQueenSideCastle_RoundTrips()
    {
      var g = CreateChess(CastleReadyFEN + " w KQkq - 0 8");
      int e1 = Sq(g, "e1");
      int c1 = Sq(g, "c1");
      int idx = FindMoveIndex(
        g.RootMoveListForTest,
        m => m.MoveType == MoveType.Castling && m.FromSquare == e1 && m.ToSquare == c1,
        "white O-O-O");
      AssertRoundTrip(g, idx, "Chess_WhiteQueenSideCastle");
    }

    // --- 2b. Black queen-side castling (O-O-O) --------------------------
    [TestMethod]
    public void Chess_BlackQueenSideCastle_RoundTrips()
    {
      var g = CreateChess(CastleReadyFEN + " b KQkq - 0 8");
      int e8 = Sq(g, "e8");
      int c8 = Sq(g, "c8");
      int idx = FindMoveIndex(
        g.RootMoveListForTest,
        m => m.MoveType == MoveType.Castling && m.FromSquare == e8 && m.ToSquare == c8,
        "black O-O-O");
      AssertRoundTrip(g, idx, "Chess_BlackQueenSideCastle");
    }

    // --- 3a. White en-passant capture (exd6 e.p.) -----------------------
    //
    // Black just played d7-d5; White pawn on e5 captures en passant to d6,
    // removing the black pawn from d5.
    [TestMethod]
    public void Chess_WhiteEnPassant_RoundTrips()
    {
      var g = CreateChess(
        "rnbqkbnr/ppp1pppp/8/3pP3/8/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 3");
      int e5 = Sq(g, "e5");
      int d6 = Sq(g, "d6");
      int idx = FindMoveIndex(
        g.RootMoveListForTest,
        m => m.MoveType == MoveType.EnPassant && m.FromSquare == e5 && m.ToSquare == d6,
        "white exd6 e.p.");

      // Sanity: the captured pawn is at d5 before, gone after Make,
      // restored after Unmake. AssertRoundTrip already checks that
      // squares match byte-for-byte; this pre-check just documents intent.
      int d5 = Sq(g, "d5");
      Assert.IsNotNull(g.Board[d5], "Black pawn must exist at d5 before EP.");

      AssertRoundTrip(g, idx, "Chess_WhiteEnPassant");

      Assert.IsNotNull(g.Board[d5], "Black pawn at d5 must be restored after Unmake.");
    }

    // --- 3b. Black en-passant capture (dxe3 e.p.) -----------------------
    //
    // White just played e2-e4; Black pawn on d4 captures en passant to e3.
    [TestMethod]
    public void Chess_BlackEnPassant_RoundTrips()
    {
      var g = CreateChess(
        "rnbqkbnr/pppp1ppp/8/8/3pP3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 3");
      int d4 = Sq(g, "d4");
      int e3 = Sq(g, "e3");
      int idx = FindMoveIndex(
        g.RootMoveListForTest,
        m => m.MoveType == MoveType.EnPassant && m.FromSquare == d4 && m.ToSquare == e3,
        "black dxe3 e.p.");

      int e4 = Sq(g, "e4");
      Assert.IsNotNull(g.Board[e4], "White pawn must exist at e4 before EP.");

      AssertRoundTrip(g, idx, "Chess_BlackEnPassant");

      Assert.IsNotNull(g.Board[e4], "White pawn at e4 must be restored after Unmake.");
    }

    // --- 4a. Promotion-with-capture to QUEEN (white axb8=Q) -------------
    //
    // White pawn a7 captures black knight b8, promoting to a Queen. Picks
    // up the moving pawn, picks up the captured knight, drops a queen.
    private const string PromoCaptureFEN =
      "1n2k3/P7/8/8/8/8/8/4K3";

    [TestMethod]
    public void Chess_WhitePromotionWithCapture_Queen_RoundTrips()
    {
      var g = CreateChess(PromoCaptureFEN + " w - - 0 1");
      int a7 = Sq(g, "a7");
      int b8 = Sq(g, "b8");
      int queenType = ((Chess)g).Queen.TypeNumber;
      int idx = FindMoveIndex(
        g.RootMoveListForTest,
        m => m.MoveType == MoveType.CaptureWithPromotion
             && m.FromSquare == a7 && m.ToSquare == b8
             && m.PromotionType == queenType,
        "white axb8=Q");
      AssertRoundTrip(g, idx, "Chess_WhitePromotionWithCapture_Queen");
    }

    // --- 4b. Promotion-with-capture to KNIGHT (white axb8=N) ------------
    [TestMethod]
    public void Chess_WhitePromotionWithCapture_Knight_RoundTrips()
    {
      var g = CreateChess(PromoCaptureFEN + " w - - 0 1");
      int a7 = Sq(g, "a7");
      int b8 = Sq(g, "b8");
      int knightType = ((Chess)g).Knight.TypeNumber;
      int idx = FindMoveIndex(
        g.RootMoveListForTest,
        m => m.MoveType == MoveType.CaptureWithPromotion
             && m.FromSquare == a7 && m.ToSquare == b8
             && m.PromotionType == knightType,
        "white axb8=N");
      AssertRoundTrip(g, idx, "Chess_WhitePromotionWithCapture_Knight");
    }

    // --- 5. Bulk: every move from the castle-ready position -------------
    //
    // The castle-ready position generates ~40+ legal moves spanning
    // pawn pushes, knight jumps, bishop/queen slides, rook moves and
    // both castles. Round-tripping all of them is the strongest single
    // assertion that MakeMove's removed dead-writes did not perturb the
    // EndMoveAddCore legality-check rollback for ordinary chess.
    [TestMethod]
    public void Chess_BulkRoundTrip_FromCastleReadyPosition()
    {
      var g = CreateChess(CastleReadyFEN + " w KQkq - 0 8");
      int n = BulkRoundTrip(g, "Chess_BulkRoundTrip");
      Assert.IsTrue(n >= 30, "Expected many legal moves; got " + n);
    }

    // ====================================================================
    //                         CANNON (Eurasian Chess)
    // ====================================================================

    // Eurasian Chess (10x10) features the Chinese Cannon: moves like a
    // rook, but to capture must jump over exactly one intervening piece
    // ("the screen"). We construct a position where the cannon's only
    // capturing move is to jump over a friendly pawn-screen and take a
    // black rook on the same file. The pickup-then-drop pattern is the
    // same as a normal capture - the cannon's pickup squares must round-
    // trip cleanly across Make/Unmake.

    // 10x10 board, ranks 10 (top) -> 1 (bottom).
    //   r10: 7k2   black king on h10 - distinct file from the white king
    //              so Eurasian's KingFacingRule (Xiangqi-style: kings may
    //              not face each other on an open file) does not reject
    //              every move.
    //   r9:  4r5   black rook on e9 (capture target)
    //   r8:  10
    //   r7:  10
    //   r6:  10
    //   r5:  4P5   white pawn on e5 (the cannon's screen)
    //   r4:  4C5   white cannon on e4
    //   r3:  10
    //   r2:  10
    //   r1:  5K4   white king on f1 (different file from black king)
    private const string EurasianCannonFEN =
      "7k2/4r5/10/10/10/4P5/4C5/10/10/5K4";

    [TestMethod]
    public void EurasianChess_CannonCapture_RoundTrips()
    {
      var g = CreateGame<CrossGameSafetyTestEurasian>();
      g.LoadFEN(EurasianCannonFEN + " w - - 0 1");
      g.GenerateMovesForTest(g.CurrentSide);

      int e4 = Sq(g, "e4");
      int e9 = Sq(g, "e9");
      int e5 = Sq(g, "e5");
      int cannonType = g.Cannon.TypeNumber;

      // The cannon capture: e4 -> e9, jumping over the friendly pawn on e5.
      int idx = FindMoveIndex(
        g.RootMoveListForTest,
        m => m.FromSquare == e4 && m.ToSquare == e9
             && (m.MoveType & MoveType.CaptureProperty) != 0
             && m.PieceMoved != null && m.PieceMoved.PieceType.TypeNumber == cannonType,
        "white cannon Cxe9 (jumping the pawn screen)");

      Assert.IsNotNull(g.Board[e9], "Black rook must exist at e9 before cannon capture.");

      AssertRoundTrip(g, idx, "EurasianChess_CannonCapture");

      Assert.IsNotNull(g.Board[e9], "Black rook at e9 must be restored after Unmake.");
      // The screen pawn must be untouched.
      Assert.IsNotNull(g.Board[e5], "Screen pawn at e5 must be present and untouched.");
    }

    // --- Bulk: every move from the cannon position ----------------------
    //
    // Stresses the EndMoveAddCore legality-check path across cannon,
    // rook, king and pawn moves on the 10x10 board.
    [TestMethod]
    public void EurasianChess_BulkRoundTrip_FromCannonPosition()
    {
      var g = CreateGame<CrossGameSafetyTestEurasian>();
      g.LoadFEN(EurasianCannonFEN + " w - - 0 1");
      g.GenerateMovesForTest(g.CurrentSide);
      BulkRoundTrip(g, "EurasianChess_BulkRoundTrip");
    }
  }
}
