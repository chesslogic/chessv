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

    [TestMethod]
    public void SpeculativeProbesDoNotChangeRepetitionOrHashState()
    {
      Game game = CreateChessGame();
      game.PlayMoves(KnightCycle);
      ulong settledHash = game.GetPositionHashCode(1);

      for (int probe = 0; probe < 100; probe++)
      {
        Movement move = game.MoveFromDescription("g1f3", MoveNotation.XBoard);
        game.MakeMove(move, true, MoveExecutionMode.Speculative);
        game.UndoMove(false, MoveExecutionMode.Speculative);
      }

      Assert.AreEqual(settledHash, game.GetPositionHashCode(1));
      Assert.AreEqual(4, game.GameMoveNumber);
      Assert.AreEqual(ResultType.NoResult, game.Result.Type);

      game.PlayMoves(KnightCycle);

      Assert.AreEqual(ResultType.Draw, game.Result.Type,
        "Speculative probes must not add or remove committed repetition occurrences.");
    }

    private static Game CreateChessGame()
    {
      Manager.Manager manager = new Manager.Manager();
      return manager.CreateGame("Chess");
    }
  }
}
