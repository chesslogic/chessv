using System;
using ChessV;

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
  }
}
