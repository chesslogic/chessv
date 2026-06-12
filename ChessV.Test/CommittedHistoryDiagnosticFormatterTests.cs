using ChessV.Games;

namespace ChessV.Test
{
  [TestClass]
  public class CommittedHistoryDiagnosticFormatterTests
  {
    [TestMethod]
    public void Format_NullGame_ReturnsExplicitNullGameBlock()
    {
      string text = CommittedHistoryDiagnosticFormatter.Format(null);

      StringAssert.Contains(text, "=== Committed History Metadata ===");
      StringAssert.Contains(text, "Game: <null>");
      StringAssert.Contains(text, "Committed history metadata unavailable: game is null.");
      StringAssert.Contains(text, "=== End Committed History Metadata ===");
    }

    [TestMethod]
    public void Format_EmptyGame_IncludesMoveAndTurnNumbersWithoutHistory()
    {
      Chess game = CreateChess();

      string text = CommittedHistoryDiagnosticFormatter.Format(game);

      StringAssert.Contains(text, "Game Move Number: 0");
      StringAssert.Contains(text, "Current Turn Number: 1");
      StringAssert.Contains(text, "Recent Committed Moves (last 0 of 0):");
      StringAssert.Contains(text, "No committed moves recorded.");
    }

    [TestMethod]
    public void Format_PlayedGame_IncludesRecentMovesAndTurnBoundaryContext()
    {
      Chess game = CreateChess();
      game.PlayMoves("e2e4 e7e5 g1f3 b8c6");
      int moveNumberBeforeFormat = game.GameMoveNumber;
      ulong hashBeforeFormat = game.GetPositionHashCode(1);

      string text = CommittedHistoryDiagnosticFormatter.Format(game);

      StringAssert.Contains(text, "Game Move Number: 4");
      StringAssert.Contains(text, "Current Turn Number: 3");
      StringAssert.Contains(text, "#1 recorded-turn=1: e2e4");
      StringAssert.Contains(text, "#2 recorded-turn=2: e7e5");
      StringAssert.Contains(text, "#3 recorded-turn=2: g1f3");
      StringAssert.Contains(text, "#4 recorded-turn=3: b8c6");
      StringAssert.Contains(text, "Recorded turn ranges in inspected history:");
      StringAssert.Contains(text, "Turn number changed from 1 to 2 at committed move #2.");
      StringAssert.Contains(text, "Turn number changed from 2 to 3 at committed move #4.");
      StringAssert.Contains(text, "Current turn matches the latest recorded turn.");
      Assert.AreEqual(moveNumberBeforeFormat, game.GameMoveNumber,
        "Formatting committed-history diagnostics must not mutate game history.");
      Assert.AreEqual(hashBeforeFormat, game.GetPositionHashCode(1),
        "Formatting committed-history diagnostics must not mutate board state.");
    }

    [TestMethod]
    public void Format_RecentMoveLimit_TrimsOlderMovesButKeepsContext()
    {
      Chess game = CreateChess();
      game.PlayMoves("e2e4 e7e5 g1f3 b8c6");

      string text = CommittedHistoryDiagnosticFormatter.Format(game, recentMoveLimit: 2);

      StringAssert.Contains(text, "Recent Committed Moves (last 2 of 4):");
      StringAssert.Contains(text, "Earlier committed moves omitted before #3.");
      Assert.IsFalse(text.Contains("#1 recorded-turn="),
        "Recent move limit should omit older committed move details.");
      Assert.IsFalse(text.Contains("#2 recorded-turn="),
        "Recent move limit should omit older committed move details.");
      StringAssert.Contains(text, "#3 recorded-turn=2: g1f3");
      StringAssert.Contains(text, "#4 recorded-turn=3: b8c6");
      StringAssert.Contains(text, "Last committed move #4 recorded turn 3.");
    }

    private static Chess CreateChess()
    {
      var game = new Chess();
      object[] attrs = typeof(Chess).GetCustomAttributes(typeof(GameAttribute), false);
      Assert.IsTrue(attrs.Length > 0, "Chess test game must carry a [Game] attribute.");
      game.Initialize((GameAttribute) attrs[0], null, null);
      return game;
    }
  }
}
