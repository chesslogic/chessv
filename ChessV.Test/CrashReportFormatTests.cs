using System;
using ChessV;
using ChessV.Games;

namespace ChessV.Test
{
  // Tests pinning the on-screen / on-disk crash report contract.
  //
  // The GUI's ExceptionForm presents the message in two surfaces:
  //   - label2 (visually fixed-size; long messages are clipped at the bounds
  //     of the control even though the underlying string is intact - this is
  //     the source of user reports of "(more lines hidden)" feeling)
  //   - txtExceptionDetails (multiline TextBox; full content scrolls)
  //   - "Save Log" output (full inner-exception chain)
  //
  // Everything that ExceptionForm formats now flows through
  // CrashReportFormatter (ChessV.Base), so these tests cover both the
  // GUI Details pane and the Save Log without referencing WinForms.
  [TestClass]
  public class CrashReportFormatTests
  {
    // ----- (1) Critical-information-present tests -----

    [TestMethod]
    public void FormatException_IncludesTypeMessageSourceAndStack()
    {
      Exception ex;
      try { throw new InvalidOperationException("e5 went stale"); }
      catch (Exception caught) { ex = caught; }

      string text = CrashReportFormatter.FormatException(ex);

      StringAssert.Contains(text, "Exception type: System.InvalidOperationException");
      StringAssert.Contains(text, "Message: e5 went stale");
      StringAssert.Contains(text, "Source:");
      StringAssert.Contains(text, "Stack Trace:");
      StringAssert.Contains(text, "FormatException_IncludesTypeMessageSourceAndStack",
        "Stack frame for the throwing method must survive into the formatted text.");
    }

    [TestMethod]
    public void FormatException_ContextStackEmbeddedInMessage_IsNotStripped()
    {
      // Simulate the enriched message that Board.ClearSquare/PerformPickup
      // produce after the diagnostics layer appends MoveGenerationContext.
      string enrichedMessage =
        "No piece to clear at square 9 (b2)\n" +
        "Board state around the target square:\n" +
        "a1:R0 b1:C0 c1:C0\n" +
        "a2:C0 b2:.. c2:P0\n" +
        "a3:C0 b3:P0 c3:P0\n" +
        "\n" +
        "=== Move Generation Context Stack ===\n" +
        "Depth: 1   CurrentRule: ApmwFirstTurnRule\n" +
        "  [0] rule=(AddMove) ply=1 type=StandardMove from=36 to=43 " +
        "pickupCursor=9 dropCursor=7 moveCursor=6 boardHash=0xDEADBEEFCAFEBABE\n" +
        "=== End Move Generation Context Stack ===";

      Exception ex;
      try { throw new InvalidOperationException(enrichedMessage); }
      catch (Exception caught) { ex = caught; }

      string text = CrashReportFormatter.FormatException(ex);

      // Every critical token from the enriched message survives the format.
      StringAssert.Contains(text, "No piece to clear at square 9 (b2)");
      StringAssert.Contains(text, "Board state around the target square");
      StringAssert.Contains(text, "Move Generation Context Stack");
      StringAssert.Contains(text, "ApmwFirstTurnRule");
      StringAssert.Contains(text, "rule=(AddMove)");
      StringAssert.Contains(text, "pickupCursor=9");
      StringAssert.Contains(text, "boardHash=0xDEADBEEFCAFEBABE");
    }

    [TestMethod]
    public void FormatExceptionChain_PreservesEveryInnerMessage_NoTruncation()
    {
      // Build a 4-level chain that mirrors real ChessV crashes:
      //   Match.OnMoveMade wrapper
      //   -> Game.MakeMove failure
      //      -> MoveList.PerformPickup wrapper
      //         -> InvalidBoardStateException
      Exception leaf = new Exception("LEAF: No piece to clear at square 36 (e5)");
      Exception lvl3 = new Exception("LVL3: PerformPickup failed", leaf);
      Exception lvl2 = new Exception("LVL2: MakeMove failed", lvl3);
      Exception lvl1 = new Exception("LVL1: Error in Match.OnMoveMade", lvl2);

      string text = CrashReportFormatter.FormatExceptionChain(lvl1);

      foreach (string token in new[] { "LEAF:", "LVL3:", "LVL2:", "LVL1:",
                                       "No piece to clear at square 36 (e5)" })
        StringAssert.Contains(text, token,
          "Inner exception '" + token + "' was lost when walking the chain - " +
          "the Save Log button on ExceptionForm depends on this.");
    }

    [TestMethod]
    public void FormatException_NullExceptionReturnsEmpty()
    {
      Assert.AreEqual(string.Empty, CrashReportFormatter.FormatException(null));
      Assert.AreEqual(string.Empty, CrashReportFormatter.FormatExceptionChain(null));
    }

    [TestMethod]
    public void FormatException_InvalidBoardStateException_AppendsBoardAndHistoryDiagnostics()
    {
      Chess game = CreateChess();
      var ex = new InvalidBoardStateException(
        "diagnostic probe",
        0,
        game.Board.GetDefaultSquareNotation(0),
        game);

      string text = CrashReportFormatter.FormatException(ex);

      StringAssert.Contains(text, "Message: diagnostic probe");
      StringAssert.Contains(text, "Board State Details:");
      StringAssert.Contains(text, "=== InvalidBoardStateException Diagnostics ===");
      StringAssert.Contains(text, "Board Diagnostic");
      StringAssert.Contains(text, "Dimensions: 8 files x 8 ranks");
      StringAssert.Contains(text, "=== Committed History Metadata ===");
      StringAssert.Contains(text, "Recent Committed Moves (last 0 of 0):");
    }

    [TestMethod]
    public void InvalidBoardStateException_MessageStaysConciseWhileDetailsIncludeFullDiagnostics()
    {
      Chess game = CreateChess();
      int square = game.NotationToSquare("e4");
      var ex = new InvalidBoardStateException(
        "diagnostic probe",
        square,
        game.Board.GetDefaultSquareNotation(square),
        game);

      StringAssert.Contains(ex.Message, "diagnostic probe");
      StringAssert.Contains(ex.Message, "Board State Details:");
      Assert.IsTrue(ex.Message.Length <= CrashReportFormatter.LabelDisplayBudgetBytes,
        "The message shown in the fixed-size dialog label must stay concise. Actual: " +
        ex.Message.Length + ".");
      Assert.IsFalse(ex.Message.Contains("Board Diagnostic"),
        "Full board diagnostics belong in the Details pane, not Exception.Message.");
      Assert.IsFalse(ex.Message.Contains("=== Committed History Metadata ==="),
        "Committed-history diagnostics belong in the Details pane, not Exception.Message.");

      string text = CrashReportFormatter.FormatException(ex);

      StringAssert.Contains(text, "Board Diagnostic");
      StringAssert.Contains(text, "Occupied cells:");
      StringAssert.Contains(text, "=== Committed History Metadata ===");
    }

    [TestMethod]
    public void FormatException_InvalidBoardStateExceptionWithNullGame_AppendsUnavailableDiagnostics()
    {
      var ex = new InvalidBoardStateException("missing game", 7, "h1", null);

      string text = CrashReportFormatter.FormatException(ex);

      StringAssert.Contains(text, "Current Player: <unavailable: game is null>");
      StringAssert.Contains(text, "Board diagnostic unavailable: game is null.");
      StringAssert.Contains(text, "Game: <null>");
    }

    [TestMethod]
    public void FormatExceptionChain_InvalidBoardStateException_AppendsDiagnosticsOnce()
    {
      Chess game = CreateChess();
      var leaf = new InvalidBoardStateException(
        "diagnostic leaf",
        0,
        game.Board.GetDefaultSquareNotation(0),
        game);
      var outer = new Exception("outer wrapper", leaf);

      string text = CrashReportFormatter.FormatExceptionChain(outer);

      Assert.AreEqual(1, CountOccurrences(text, "=== InvalidBoardStateException Diagnostics ==="));
      Assert.AreEqual(1, CountOccurrences(text, "Board Diagnostic"));
      Assert.AreEqual(1, CountOccurrences(text, "=== Committed History Metadata ==="));
    }

    [TestMethod]
    public void FormatException_InvalidBoardStateException_DoesNotDuplicateMoveGenerationContext()
    {
      Chess game = CreateChess();
      MoveList moveList = game.RootMoveListForTest;
      int fromSquare = game.NotationToSquare("e2");
      int emptySquare = game.NotationToSquare("e4");
      Assert.IsNull(game.Board[emptySquare], "Test setup requires e4 to be empty.");

      MoveGenerationContext.Reset();
      try
      {
        MoveGenerationContext.SetCurrentRule("DiagnosticRule");
        MoveGenerationContext.Push(
          "PerformPickupTest",
          ply: 1,
          moveType: MoveType.StandardMove,
          fromSquare: fromSquare,
          toSquare: emptySquare,
          pickupCursor: 0,
          dropCursor: 0,
          moveCursor: 0,
          boardHash: game.Board.HashCode);
        moveList.SetPickupForTest(0, new Pickup { Piece = null, Square = emptySquare });

        InvalidBoardStateException ex = Assert.ThrowsException<InvalidBoardStateException>(
          () => moveList.PerformPickupForTest(0));
        string text = CrashReportFormatter.FormatException(ex);

        Assert.AreEqual(1, CountOccurrences(ex.Message, "=== Move Generation Context Stack ==="),
          "PerformPickup must not append a second context stack when Board.ClearSquare already did.");
        Assert.AreEqual(1, CountOccurrences(text, "=== Move Generation Context Stack ==="),
          "Exception formatting must preserve exactly one context stack.");
        StringAssert.Contains(text, "PerformPickupTest");
        StringAssert.Contains(text, "Board Diagnostic");
      }
      finally
      {
        MoveGenerationContext.Reset();
      }
    }

    // ----- (2) Length-budget tests -----
    //
    // The label at the top of ExceptionForm is bounded; long messages get
    // visually clipped. These tests guard against the diagnostics layer (or
    // any future enrichment) growing the message past what the user can
    // reasonably read in the dialog without expanding the Details pane.

    [TestMethod]
    public void EnrichedRealisticMessage_FitsWithinLabelBudget()
    {
      // Approximation of the worst-case message we currently emit: original
      // grid + context stack with a few nested frames + cursor info.
      // Mirrors the 2026-02-12 enriched output captured in commit 7439ccf.
      string realistic =
        "No piece to clear at square 36 (e5)\n" +
        "Board state around the target square:\n" +
        "d4:.. e4:.. f4:..\n" +
        "d5:.. e5:.. f5:..\n" +
        "d6:.. e6:.. f6:..\n" +
        "\n" +
        "=== Move Generation Context Stack ===\n" +
        "Depth: 3   CurrentRule: ApmwFirstTurnRule\n" +
        "  [0] rule=ApmwFirstTurnRule ply=1 type=BaroqueCapture from=27 to=36 " +
        "pickupCursor=12 dropCursor=10 moveCursor=8 boardHash=0x531D5B338700DEE4\n" +
        "  [1] rule=(AddMove) ply=1 type=StandardMove from=36 to=43 " +
        "pickupCursor=9 dropCursor=7 moveCursor=6 boardHash=0x531D5B338700DEE4\n" +
        "  [2] rule=BasicPromotionRule ply=1 type=StandardMove from=43 to=51 " +
        "pickupCursor=10 dropCursor=8 moveCursor=7 boardHash=0x531D5B338700DEE4\n" +
        "=== End Move Generation Context Stack ===\n" +
        "\n" +
        "Board State Details:\n" +
        "Square: 36 (e5)\n" +
        "Current Player: 1\n" +
        "Game Move Number: 0";

      Assert.IsTrue(realistic.Length <= CrashReportFormatter.LabelDisplayBudgetBytes,
        "A realistic enriched message must fit in the GUI label's display " +
        "budget (" + CrashReportFormatter.LabelDisplayBudgetBytes + " chars). " +
        "Actual: " + realistic.Length + ". If this fails after a diagnostics " +
        "change, either trim the dump or split critical info into the Details " +
        "pane explicitly.");
    }

    [TestMethod]
    public void FullFormattedDetail_ForRealisticException_StaysWithinDetailBudget()
    {
      string realisticMessage = new string('x', 2000); // generous message body
      Exception ex;
      try { throw new Exception(realisticMessage); }
      catch (Exception caught) { ex = caught; }

      string text = CrashReportFormatter.FormatException(ex);

      Assert.IsTrue(text.Length <= CrashReportFormatter.DetailBudgetBytes,
        "The full Details-pane text for a realistically-sized message + " +
        "stack must stay under " + CrashReportFormatter.DetailBudgetBytes +
        " chars. Actual: " + text.Length + ". A breach indicates either a " +
        "huge stack (recursion?) or runaway diagnostics.");
    }

    [TestMethod]
    public void FullFormattedDetail_ForRealisticInvalidBoardStateException_StaysWithinDetailBudget()
    {
      Chess game = CreateChess();
      game.PlayMoves("e2e4 e7e5 g1f3 b8c6");
      int square = game.NotationToSquare("e5");
      string realisticMessage =
        "No piece to clear at square " + square + " (" + game.Board.GetDefaultSquareNotation(square) + ")\n" +
        "Board state around the target square:\n" +
        "d4:.. e4:P0 f4:..\n" +
        "d5:.. e5:.. f5:..\n" +
        "d6:.. e6:.. f6:..\n" +
        "\n" +
        "=== Move Generation Context Stack ===\n" +
        "Depth: 2   CurrentRule: DiagnosticRule\n" +
        "  [1] rule=DiagnosticRule ply=2 type=StandardMove from=36 to=44 " +
        "pickupCursor=12 dropCursor=10 moveCursor=8 boardHash=0x531D5B338700DEE4\n" +
        "  [0] rule=(AddMove) ply=2 type=StandardMove from=44 to=52 " +
        "pickupCursor=9 dropCursor=7 moveCursor=6 boardHash=0x531D5B338700DEE4\n" +
        "=== End Move Generation Context Stack ===";
      Exception ex;
      try
      {
        throw new InvalidBoardStateException(
          realisticMessage,
          square,
          game.Board.GetDefaultSquareNotation(square),
          game);
      }
      catch (Exception caught) { ex = caught; }

      string text = CrashReportFormatter.FormatException(ex);

      Assert.IsTrue(text.Length <= CrashReportFormatter.DetailBudgetBytes,
        "The full Details-pane text for a realistic InvalidBoardStateException " +
        "with board/history diagnostics must stay under " +
        CrashReportFormatter.DetailBudgetBytes + " chars. Actual: " + text.Length + ".");
      StringAssert.Contains(text, "Board Diagnostic");
      StringAssert.Contains(text, "Occupied cells:");
      StringAssert.Contains(text, "Recent Committed Moves (last 4 of 4):");
      Assert.AreEqual(1, CountOccurrences(text, "=== Move Generation Context Stack ==="));
    }

    // ----- (3) Anti-truncation guard -----
    //
    // ChessV historically embedded summaries like "(N more lines hidden)"
    // when wrapping inner exceptions. If any future formatter introduces
    // such elision, this test fails by design.
    [TestMethod]
    public void FormatExceptionChain_DoesNotElideLines()
    {
      // 30-line Message - well above any reasonable "show first K lines" cut.
      var sb = new System.Text.StringBuilder();
      for (int i = 0; i < 30; i++)
        sb.AppendLine("DETAIL_LINE_" + i);

      Exception ex;
      try { throw new Exception(sb.ToString()); }
      catch (Exception caught) { ex = caught; }

      string text = CrashReportFormatter.FormatExceptionChain(ex);

      for (int i = 0; i < 30; i++)
        StringAssert.Contains(text, "DETAIL_LINE_" + i,
          "Line " + i + " was elided. The formatter must not introduce " +
          "'(N more lines)' style truncation - the Save Log is the user's " +
          "only persistent record of the crash.");

      foreach (string forbidden in new[] { "more lines", "more line", "...truncated", "(elided)" })
        Assert.IsFalse(text.Contains(forbidden, StringComparison.OrdinalIgnoreCase),
          "Formatter introduced an elision marker '" + forbidden + "'.");
    }

    private static Chess CreateChess()
    {
      var game = new Chess();
      object[] attrs = typeof(Chess).GetCustomAttributes(typeof(GameAttribute), false);
      Assert.IsTrue(attrs.Length > 0, "Chess test game must carry a [Game] attribute.");
      game.Initialize((GameAttribute) attrs[0], null, null);
      return game;
    }

    private static int CountOccurrences(string text, string value)
    {
      int count = 0;
      int index = 0;
      while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
      {
        count++;
        index += value.Length;
      }
      return count;
    }
  }
}
