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
    // The Checkers multi-jump crash from _refs/2026-02-12.txt is fixed
    // (Phase 3.6, MoveList: removed dead writes to tempPickupCursor /
    // tempDropCursor inside MakeMove that aliased the BeginMoveAdd
    // rollback snapshot). This test now pins the FIX: LoadFEN must
    // complete cleanly on the historical repro position.
    //
    // The diagnostic-message-content contract that this test originally
    // exercised is still covered by:
    //   MoveListGuardTests   -- guards against partial-Make corruption
    //   CrashReportFormatTests -- enriched message tokens / budgets
    //   MoveGenerationContextTests (the rest of this class) -- frame
    //     stack format, LIFO, Reset, SetCurrentRule
    // -----------------------------------------------------------------
    [TestMethod]
    public void Repros_2026_02_12_LoadFenNoLongerCrashes()
    {
      var game = new FirstTurnRuleTestGame();
      var attrs = typeof(FirstTurnRuleTestGame)
        .GetCustomAttributes(typeof(GameAttribute), inherit: false);
      Assert.IsTrue(attrs.Length > 0, "Test game must carry a [Game] attribute.");
      var gameAttr = (GameAttribute)attrs[0];
      game.Initialize(gameAttr, null, null);

      try
      {
        game.LoadFEN("4k3/7e/6P1/4e3/3P4/4R3/8/4K3 b - - 0 1");
      }
      catch (InvalidBoardStateException ex)
      {
        Assert.Fail(
          "Regression: the 2026-02-12 Checkers multi-jump crash has " +
          "returned. LoadFEN raised InvalidBoardStateException: " +
          ex.Message);
      }
    }
  }
}
