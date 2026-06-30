namespace ChessV.Test
{
  [TestClass]
  public class RepetitionDrawRuleTests
  {
    private const string KnightCycle = "g1f3 g8f6 f3g1 f6g8";

    [TestMethod]
    public void SecondOccurrenceOfStartingPositionIsNotDraw()
    {
      Game game = CreateChessGame();

      game.PlayMoves(KnightCycle);

      Assert.AreEqual(ResultType.NoResult, game.Result.Type,
        "Returning to the starting position once should be a second occurrence, not a draw.");
      Assert.AreEqual(MoveEventResponse.NotHandled, game.TestForWinLossDraw(game.CurrentSide),
        "A settled root repetition check should not count the current position twice.");
    }

    [TestMethod]
    public void ThirdOccurrenceOfStartingPositionIsDraw()
    {
      Game game = CreateChessGame();

      game.PlayMoves(KnightCycle + " " + KnightCycle);

      Assert.AreEqual(ResultType.Draw, game.Result.Type,
        "Returning to the starting position twice should be a genuine threefold repetition draw.");
    }

    [TestMethod]
    public void TakeBackRemovesRecordedRepetitionOccurrence()
    {
      Game game = CreateChessGame();

      game.PlayMoves(KnightCycle);
      game.TakeBackMoves(4);
      game.PlayMoves(KnightCycle);

      Assert.AreEqual(ResultType.NoResult, game.Result.Type,
        "Taking back a repeated position should remove that occurrence from repetition history.");
    }

    private static Game CreateChessGame()
    {
      Manager.Manager manager = new Manager.Manager();
      return manager.CreateGame("Chess");
    }
  }
}
