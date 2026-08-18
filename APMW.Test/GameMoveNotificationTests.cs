using ChessV;
using ChessV.Base;
using Moq;
using System;
using System.Collections.Generic;

namespace Archipelago.APChessV
{
  [TestClass]
  [DoNotParallelize]
  public class GameMoveNotificationTests
  {
    [TestInitialize]
    public void ResetApmwCallbacks()
    {
      ApmwCore core = ApmwCore.getInstance();
      core.NewMoveSetup.Clear();
      core.NewMovePlayed.Clear();
      core.MatchFinished.Clear();
    }

    [TestCleanup]
    public void ClearApmwCallbacks()
    {
      ApmwCore core = ApmwCore.getInstance();
      core.NewMoveSetup.Clear();
      core.NewMovePlayed.Clear();
      core.MatchFinished.Clear();
    }

    [TestMethod]
    public void MatchValidation_emitsApmwCallbacksOnlyForCommittedMove()
    {
      Game game = CreateMatchGame(out Mock<Player>[] players);
      CallbackCounts callbacks = ObserveCallbacks(game);
      Movement move = game.MoveFromDescription("e2e4", MoveNotation.XBoard);
      bool previousValidation = DebugFlags.ValidateMovesBeforeCommit;

      try
      {
        DebugFlags.ValidateMovesBeforeCommit = true;
        game.Match.OnMoveMade(
          players[game.CurrentSide].Object,
          new List<Movement> { move });
      }
      finally
      {
        DebugFlags.ValidateMovesBeforeCommit = previousValidation;
      }

      callbacks.AssertCounts(1, 1, 2, 2, 0, 1);
    }

    [TestMethod]
    public void MatchWithoutValidation_emitsOneCommittedCallbackSet()
    {
      Game game = CreateMatchGame(out Mock<Player>[] players);
      CallbackCounts callbacks = ObserveCallbacks(game);
      Movement move = game.MoveFromDescription("e2e4", MoveNotation.XBoard);
      bool previousValidation = DebugFlags.ValidateMovesBeforeCommit;

      try
      {
        DebugFlags.ValidateMovesBeforeCommit = false;
        game.Match.OnMoveMade(
          players[game.CurrentSide].Object,
          new List<Movement> { move });
      }
      finally
      {
        DebugFlags.ValidateMovesBeforeCommit = previousValidation;
      }

      callbacks.AssertCounts(1, 1, 1, 1, 0, 0);
    }

    [TestMethod]
    public void CommittedMoveWithoutProbe_emitsCallbacksOnce()
    {
      Game game = CreateGame();
      CallbackCounts callbacks = ObserveCallbacks(game);

      game.MakeMove(
        game.MoveFromDescription("e2e4", MoveNotation.XBoard),
        true);

      callbacks.AssertCounts(1, 1, 1, 1, 0, 0);
    }

    [TestMethod]
    public void RepeatedSpeculativeProbes_areSilentAndNextCommitNotifiesOnce()
    {
      Game game = CreateGame();
      CallbackCounts callbacks = ObserveCallbacks(game);
      ulong initialHash = game.GetPositionHashCode(1);

      for (int cycle = 0; cycle < 100; cycle++)
      {
        Movement move = game.MoveFromDescription("e2e4", MoveNotation.XBoard);
        game.MakeMove(move, true, MoveExecutionMode.Speculative);
        game.UndoMove(false, MoveExecutionMode.Speculative);
      }

      callbacks.AssertCounts(0, 0, 100, 100, 0, 100);
      Assert.AreEqual(0, game.GameMoveNumber);
      Assert.AreEqual(0, game.BoardMoveStack.MoveCount);
      Assert.AreEqual(initialHash, game.GetPositionHashCode(1));

      game.MakeMove(
        game.MoveFromDescription("e2e4", MoveNotation.XBoard),
        true);

      callbacks.AssertCounts(1, 1, 101, 101, 0, 100);
    }

    [TestMethod]
    public void GenuineUndoReplayCycles_notifyOncePerCommitAndTakeback()
    {
      Game game = CreateGame();
      CallbackCounts callbacks = ObserveCallbacks(game);

      for (int cycle = 0; cycle < 100; cycle++)
      {
        game.MakeMove(
          game.MoveFromDescription("e2e4", MoveNotation.XBoard),
          true);
        game.UndoMove();
      }

      game.MakeMove(
        game.MoveFromDescription("e2e4", MoveNotation.XBoard),
        true);

      callbacks.AssertCounts(101, 101, 101, 101, 100, 0);
    }

    [TestMethod]
    public void SpeculativeMatingProbe_restoresResultAndDefersMatchFinishedUntilCommittedMove()
    {
      Game game = CreateGame();
      int matchFinishedCallbacks = 0;
      ApmwCore.getInstance().MatchFinished.Add(_ => matchFinishedCallbacks++);
      game.PlayMoves("f2f3 e7e5 g2g4", MoveNotation.XBoard);
      Movement mate = game.MoveFromDescription("d8h4", MoveNotation.XBoard);
      Result resultBeforeProbe = game.Result;

      game.MakeMove(mate, true, MoveExecutionMode.Speculative);

      Assert.AreEqual(ResultType.Win, game.Result.Type);
      Assert.AreEqual(0, matchFinishedCallbacks);

      game.UndoMove(false, MoveExecutionMode.Speculative);
      Assert.AreSame(resultBeforeProbe, game.Result);
      Assert.AreEqual(ResultType.NoResult, game.Result.Type);

      game.MakeMove(mate, true);

      Assert.AreEqual(ResultType.Win, game.Result.Type);
      Assert.AreEqual(1, matchFinishedCallbacks);
    }

    [TestMethod]
    public void NestedSpeculativeMoves_restoreExactResultSnapshotsInLifoOrder()
    {
      Game game = CreateGame();
      Result initialResult = game.Result;

      game.MakeMove(
        game.MoveFromDescription("e2e4", MoveNotation.XBoard),
        true,
        MoveExecutionMode.Speculative);

      Result resultBeforeNestedProbe =
        new Result(ResultType.Adjudication, 0, "outer speculative state");
      game.Result = resultBeforeNestedProbe;
      game.MakeMove(
        game.MoveFromDescription("e7e5", MoveNotation.XBoard),
        true,
        MoveExecutionMode.Speculative);
      game.Result = new Result(ResultType.Timeout, 1, "nested speculative state");

      game.UndoMove(false, MoveExecutionMode.Speculative);
      Assert.AreSame(resultBeforeNestedProbe, game.Result);
      Assert.AreEqual(1, game.GameMoveNumber);

      game.UndoMove(false, MoveExecutionMode.Speculative);
      Assert.AreSame(initialResult, game.Result);
      Assert.AreEqual(0, game.GameMoveNumber);
    }

    [TestMethod]
    public void SpeculativeUndo_requiresMatchingSpeculativeMoveWithoutMutatingState()
    {
      Game game = CreateGame();
      ulong initialHash = game.GetPositionHashCode(1);

      Assert.ThrowsException<InvalidOperationException>(
        () => game.UndoMove(false, MoveExecutionMode.Speculative));
      Assert.AreEqual(0, game.GameMoveNumber);
      Assert.AreEqual(initialHash, game.GetPositionHashCode(1));

      game.MakeMove(
        game.MoveFromDescription("e2e4", MoveNotation.XBoard),
        true);
      ulong committedHash = game.GetPositionHashCode(1);

      Assert.ThrowsException<InvalidOperationException>(
        () => game.UndoMove(false, MoveExecutionMode.Speculative));
      Assert.AreEqual(1, game.GameMoveNumber);
      Assert.AreEqual(committedHash, game.GetPositionHashCode(1));

      game.UndoMove();
      Assert.AreEqual(initialHash, game.GetPositionHashCode(1));
    }

    [TestMethod]
    public void CommittedUndo_rejectsActiveSpeculativeMoveWithoutLosingSnapshot()
    {
      Game game = CreateGame();
      Result initialResult = game.Result;
      game.MakeMove(
        game.MoveFromDescription("e2e4", MoveNotation.XBoard),
        true,
        MoveExecutionMode.Speculative);
      ulong speculativeHash = game.GetPositionHashCode(1);

      Assert.ThrowsException<InvalidOperationException>(() => game.UndoMove());
      Assert.AreEqual(1, game.GameMoveNumber);
      Assert.AreEqual(speculativeHash, game.GetPositionHashCode(1));

      game.UndoMove(false, MoveExecutionMode.Speculative);
      Assert.AreSame(initialResult, game.Result);
      Assert.AreEqual(0, game.GameMoveNumber);
    }

    [TestMethod]
    public void FailedSpeculativeMake_doesNotLeaveResultSnapshot()
    {
      Game game = CreateGame();
      Movement movement = game.MoveFromDescription("e2e4", MoveNotation.XBoard);
      game.GetRootMoves(out MoveInfo[] rootMoves);
      MoveInfo move = default(MoveInfo);
      bool foundMove = false;
      foreach (MoveInfo candidate in rootMoves)
      {
        if (candidate.Hash == movement.Hash)
        {
          move = candidate;
          foundMove = true;
          break;
        }
      }
      Assert.IsTrue(foundMove);

      game.MakeMove(move, true);
      Assert.ThrowsException<Exception>(
        () => game.MakeMove(move, true, MoveExecutionMode.Speculative));

      game.UndoMove();
      game.MakeMove(
        game.MoveFromDescription("e2e4", MoveNotation.XBoard),
        true,
        MoveExecutionMode.Speculative);
      game.UndoMove(false, MoveExecutionMode.Speculative);

      Assert.AreEqual(0, game.GameMoveNumber);
      Assert.AreEqual(ResultType.NoResult, game.Result.Type);
    }

    [TestMethod]
    public void CommittedRepetitionDraw_doesNotEmitMatchFinished()
    {
      Game game = CreateGame();
      int matchFinishedCallbacks = 0;
      ApmwCore.getInstance().MatchFinished.Add(_ => matchFinishedCallbacks++);

      game.PlayMoves(
        "g1f3 g8f6 f3g1 f6g8 g1f3 g8f6 f3g1 f6g8",
        MoveNotation.XBoard);

      Assert.AreEqual(ResultType.Draw, game.Result.Type);
      Assert.AreEqual(0, matchFinishedCallbacks);
    }

    private static Game CreateMatchGame(out Mock<Player>[] players)
    {
      Game game = CreateGame();
      game.StartMatch();
      players = new Mock<Player>[2];
      for (int side = 0; side < players.Length; side++)
      {
        players[side] = new Mock<Player>();
        players[side].Object.Evaluation = new MoveEvaluation();
        game.Match.SetPlayer(side, players[side].Object);
      }
      return game;
    }

    private static Game CreateGame()
    {
      return new ChessV.Manager.Manager().CreateGame("Chess");
    }

    private static CallbackCounts ObserveCallbacks(Game game)
    {
      var callbacks = new CallbackCounts();
      ApmwCore.getInstance().NewMoveSetup.Add(_ => callbacks.ApmwSetup++);
      ApmwCore.getInstance().NewMovePlayed.Add(_ => callbacks.ApmwPlayed++);
      game.MoveBeingPlayed += _ => callbacks.MoveBeingPlayed++;
      game.MovePlayed += _ => callbacks.MovePlayed++;
      game.MoveTakenBack += () => callbacks.CommittedTakeBack++;
      game.MoveReverted += mode =>
      {
        if (mode == MoveExecutionMode.Speculative)
          callbacks.SpeculativeReversion++;
      };
      return callbacks;
    }

    private sealed class CallbackCounts
    {
      public int ApmwSetup;
      public int ApmwPlayed;
      public int MoveBeingPlayed;
      public int MovePlayed;
      public int CommittedTakeBack;
      public int SpeculativeReversion;

      public void AssertCounts(
        int apmwSetup,
        int apmwPlayed,
        int moveBeingPlayed,
        int movePlayed,
        int committedTakeBack,
        int speculativeReversion)
      {
        Assert.AreEqual(apmwSetup, ApmwSetup, "APMW setup callback count");
        Assert.AreEqual(apmwPlayed, ApmwPlayed, "APMW played callback count");
        Assert.AreEqual(moveBeingPlayed, MoveBeingPlayed, "MoveBeingPlayed callback count");
        Assert.AreEqual(movePlayed, MovePlayed, "MovePlayed callback count");
        Assert.AreEqual(committedTakeBack, CommittedTakeBack, "committed MoveTakenBack callback count");
        Assert.AreEqual(speculativeReversion, SpeculativeReversion, "speculative MoveReverted callback count");
      }
    }
  }
}
