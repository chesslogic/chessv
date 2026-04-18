using System.Collections.Generic;
using ChessV;
using ChessV.Games;
using ChessV.Games.Pieces.Apmw;
using ChessV.Games.Rules.Apmw;

namespace ChessV.Test
{
  // Tests for the unified move-generation diagnostics context stack.
  //
  //   1) Pure unit tests of the MoveGenerationContext API: empty, LIFO,
  //      Reset() recovery from leaked frames.
  //   2) End-to-end test that runs the same scenario as
  //      Repros_2026_02_12_CheckersMultiJumpCorruptsBoardDuringGeneration
  //      and asserts the InvalidBoardStateException now carries the
  //      formatted context stack so the failing rule and cursor state
  //      can be diagnosed from the message alone.
  [TestClass]
  public class MoveGenerationContextTests
  {
    [TestInitialize]
    public void ResetContextBeforeEach()
    {
      // Defensive: any prior test that pushed without popping (e.g. an
      // earlier failing repro that escaped before EndMoveAdd's finally
      // ran in older builds) would leave frames behind. This is the
      // documented test-only recovery path.
      MoveGenerationContext.Reset();
    }

    [TestMethod]
    public void EmptyStack_FormatReturnsEmptyString()
    {
      Assert.AreEqual(0, MoveGenerationContext.Depth);
      Assert.AreEqual(string.Empty, MoveGenerationContext.FormatContextStack());
    }

    [TestMethod]
    public void Push_Pop_RespectsLifoOrdering()
    {
      MoveGenerationContext.Push("RuleA", ply: 0, MoveType.StandardMove,
        fromSquare: 1, toSquare: 2, pickupCursor: 0, dropCursor: 0,
        moveCursor: 0, boardHash: 0x1111UL);
      Assert.AreEqual(1, MoveGenerationContext.Depth);

      MoveGenerationContext.Push("RuleB", ply: 1, MoveType.StandardCapture,
        fromSquare: 3, toSquare: 4, pickupCursor: 2, dropCursor: 1,
        moveCursor: 5, boardHash: 0x2222UL);
      Assert.AreEqual(2, MoveGenerationContext.Depth);

      string formatted = MoveGenerationContext.FormatContextStack();
      // Innermost frame is rendered first.
      int idxA = formatted.IndexOf("RuleA");
      int idxB = formatted.IndexOf("RuleB");
      Assert.IsTrue(idxB >= 0 && idxA >= 0, "Both rule names must appear: " + formatted);
      Assert.IsTrue(idxB < idxA, "Innermost (RuleB) frame must render before RuleA. Got:\n" + formatted);
      Assert.IsTrue(formatted.Contains("from=3"), "Innermost frame metadata must appear: " + formatted);
      Assert.IsTrue(formatted.Contains("0x0000000000002222"), "Board hash must be formatted: " + formatted);

      MoveGenerationContext.Pop();
      Assert.AreEqual(1, MoveGenerationContext.Depth);
      Assert.IsFalse(MoveGenerationContext.FormatContextStack().Contains("RuleB"));

      MoveGenerationContext.Pop();
      Assert.AreEqual(0, MoveGenerationContext.Depth);
    }

    [TestMethod]
    public void Reset_RecoversFromLeakedFrames()
    {
      // Simulate a rule that pushed a frame and then the corresponding
      // EndMoveAdd never ran (e.g. an exception that bypassed the finally
      // block in earlier builds). Reset() must clear the leak so test
      // isolation is preserved.
      MoveGenerationContext.Push("LeakyRule", ply: 0, MoveType.StandardMove,
        fromSquare: 0, toSquare: 0, pickupCursor: 0, dropCursor: 0,
        moveCursor: 0, boardHash: 0UL);
      MoveGenerationContext.Push("LeakyRule2", ply: 0, MoveType.StandardMove,
        fromSquare: 0, toSquare: 0, pickupCursor: 0, dropCursor: 0,
        moveCursor: 0, boardHash: 0UL);
      Assert.AreEqual(2, MoveGenerationContext.Depth);

      MoveGenerationContext.Reset();

      Assert.AreEqual(0, MoveGenerationContext.Depth);
      Assert.AreEqual(string.Empty, MoveGenerationContext.FormatContextStack());
      Assert.IsNull(MoveGenerationContext.CurrentRule);
    }

    [TestMethod]
    public void SetCurrentRule_IsInheritedByPushedFrames()
    {
      MoveGenerationContext.SetCurrentRule("MyTestRule");
      try
      {
        MoveGenerationContext.Push(
          ruleNameOverride: null,
          ply: 0, MoveType.StandardMove,
          fromSquare: 0, toSquare: 0,
          pickupCursor: 0, dropCursor: 0, moveCursor: 0, boardHash: 0UL);
        string formatted = MoveGenerationContext.FormatContextStack();
        Assert.IsTrue(formatted.Contains("MyTestRule"), "Inherited rule name must appear: " + formatted);
        Assert.IsTrue(formatted.Contains("CurrentRule: MyTestRule"), formatted);
      }
      finally
      {
        MoveGenerationContext.Pop();
        MoveGenerationContext.SetCurrentRule(null);
      }
    }

    // -----------------------------------------------------------------
    // End-to-end: drive the still-failing Checkers multi-jump repro
    // through LoadFEN's implicit generateMoves and assert that the
    // exception message now contains the diagnostic context stack with
    // a recognizable rule label and cursor snapshot. The underlying bug
    // is intentionally still failing in
    // FirstTurnRuleReproTests.Repros_2026_02_12_*; this test just
    // verifies the diagnostics surface useful info on that failure.
    // -----------------------------------------------------------------
    [TestMethod]
    public void Repros_2026_02_12_ExceptionMessageContainsContextStack()
    {
      var game = new FirstTurnRuleTestGame();
      var attrs = typeof(FirstTurnRuleTestGame)
        .GetCustomAttributes(typeof(GameAttribute), inherit: false);
      Assert.IsTrue(attrs.Length > 0, "Test game must carry a [Game] attribute.");
      var gameAttr = (GameAttribute)attrs[0];
      game.Initialize(gameAttr, null, null);

      InvalidBoardStateException caught = null;
      try
      {
        game.LoadFEN("4k3/7e/6P1/4e3/3P4/4R3/8/4K3 b - - 0 1");
      }
      catch (InvalidBoardStateException ex)
      {
        caught = ex;
      }

      Assert.IsNotNull(caught,
        "Expected the still-failing Checkers multi-jump scenario to raise " +
        "InvalidBoardStateException so we can inspect the diagnostic message. " +
        "If this passes, either the underlying bug is fixed (good - delete " +
        "this test) or the diagnostics regressed.");

      string message = caught.Message;
      // Existing diagnostics that must remain present.
      StringAssert.Contains(message, "No piece to clear at square",
        "Original Board.ClearSquare diagnostic must be preserved.");
      StringAssert.Contains(message, "Board state around the target square",
        "Original board grid dump must be preserved.");

      // New diagnostics from the unified context stack.
      StringAssert.Contains(message, "=== Move Generation Context Stack ===",
        "Context stack header must be present in the exception message.\n" +
        "Full message:\n" + message);
      StringAssert.Contains(message, "=== End Move Generation Context Stack ===",
        "Context stack footer must be present.\nFull message:\n" + message);
      StringAssert.Contains(message, "pickupCursor=",
        "Cursor snapshot must appear in at least one frame.\nFull message:\n" + message);

      // The failing pickup is reached either via a Rule's BeginMoveAdd
      // (rule label = the Rule subclass name) or via AddMove/AddCapture's
      // synthetic frame. Either way, ONE of these tokens must appear so
      // a future investigator can attribute the crash.
      bool hasRuleAttribution =
        message.Contains("(AddMove)") ||
        message.Contains("(AddCapture)") ||
        message.Contains("Rule") || // any concrete *Rule class name
        message.Contains("(root)");
      Assert.IsTrue(hasRuleAttribution,
        "Context stack must attribute the failing pickup to a rule or to " +
        "(AddMove)/(AddCapture)/(root).\nFull message:\n" + message);

      // Surface the full message in the test output so the diagnostics
      // are visible in CI logs whenever this test runs.
      System.Console.WriteLine("=== Repros_2026_02_12 enriched exception ===");
      System.Console.WriteLine(message);
      System.Console.WriteLine("=== End enriched exception ===");
    }
  }
}
