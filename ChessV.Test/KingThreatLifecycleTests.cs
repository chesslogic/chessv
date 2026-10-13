using System.Linq;
using System;
using ChessV.Base;
using ChessV.Games;

namespace ChessV.Test
{
  [TestClass]
  [DoNotParallelize]
  public class KingThreatLifecycleTests
  {
    [TestMethod]
    public void StableMoveNotification_ObservesNoMoveResultAfterAdjudication()
    {
      var game = new Chess();
      var attribute = (GameAttribute)typeof(Chess).GetCustomAttributes(typeof(GameAttribute), false).Single();
      game.Initialize(attribute, null, null);
      Result observed = null;
      int? immobileSide = null;
      game.PositionSettled += kind =>
      {
        observed = game.Result;
        immobileSide = game.NoMoveSide;
      };

      game.PlayMoves("f2f3 e7e5 g2g4 d8h4", MoveNotation.XBoard);

      Assert.AreEqual(ResultType.Win, observed.Type);
      Assert.AreEqual(1, observed.Winner);
      Assert.AreEqual(0, immobileSide);
    }

    [TestMethod]
    public void StableNotificationFailure_DoesNotSuppressMatchFinishedCallbacks()
    {
      ApmwCore previous = ApmwCore._instance;
      try
      {
        ApmwCore._instance = new ApmwCore();
        int finished = 0;
        ApmwCore.getInstance().MatchFinished.Add(_ => finished++);
        var game = KingThreatInputTests.CreateChess();
        game.PositionSettled += _ =>
        {
          if (!game.Result.IsNone)
            throw new InvalidOperationException("UI inspection failed");
        };

        Assert.ThrowsException<InvalidOperationException>(() =>
          game.PlayMoves("f2f3 e7e5 g2g4 d8h4", MoveNotation.XBoard));
        Assert.AreEqual(1, finished);
        Assert.AreEqual(0, game.NoMoveSide);
      }
      finally
      {
        ApmwCore._instance = previous;
      }
    }
  }
}
