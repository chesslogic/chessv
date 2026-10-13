using System.Drawing;
using System.Linq;
using ChessV.Games;
using ChessV.GUI;

namespace ChessV.Test
{
  [TestClass]
  public class KingThreatRenderingTests
  {
    [TestMethod]
    public void SharedDestination_RetainsEachAttackersColorWithoutCoveringItsCenter()
    {
      var game = KingThreatAnalysisTests.CreateGame("4r2k/8/8/1b6/8/8/8/4K3 w - - 0 1");
      var hints = KingThreatAnalysis.Analyze(game, game.Board[KingThreatAnalysisTests.Square(game, "e1")]);
      var renderer = new KingThreatRenderer(hints);
      using var bitmap = new Bitmap(400, 400);
      using var graphics = Graphics.FromImage(bitmap);
      graphics.Clear(Color.White);

      Rectangle Map(int square)
      {
        Location location = game.Board.SquareToLocation(square);
        return new Rectangle(location.File * 50, (7 - location.Rank) * 50, 50, 50);
      }
      renderer.Render(graphics, game.Board, Map, null);

      int target = KingThreatAnalysisTests.Square(game, "e2");
      foreach (string source in new[] { "b5", "e8" })
      {
        int square = KingThreatAnalysisTests.Square(game, source);
        Color color = renderer.AttackerColors[square];
        Assert.IsTrue(ContainsColor(bitmap, Map(square), color), source + " source border");
        Assert.IsTrue(ContainsColor(bitmap, Map(target), color), source + " target segment");
      }
      Rectangle targetRectangle = Map(target);
      Assert.AreEqual(Color.White.ToArgb(),
        bitmap.GetPixel(targetRectangle.Left + 25, targetRectangle.Top + 25).ToArgb());
      Assert.AreEqual(2, renderer.AttackerColors.Values.Distinct().Count());
    }

    [TestMethod]
    public void HoveredCannonPath_TurnsRedOnlyAfterTheScreen()
    {
      var game = KingThreatAnalysisTests.CreateGame("4i2k/8/8/8/4P3/8/8/4K3 w - - 0 1");
      int king = KingThreatAnalysisTests.Square(game, "e1");
      var renderer = new KingThreatRenderer(KingThreatAnalysis.Analyze(game, game.Board[king]));
      using var bitmap = new Bitmap(480, 480);
      using var graphics = Graphics.FromImage(bitmap);
      graphics.Clear(Color.White);
      Rectangle Map(int square)
      {
        Location location = game.Board.SquareToLocation(square);
        return new Rectangle(location.File * 60, (7 - location.Rank) * 60, 60, 60);
      }
      renderer.Render(graphics, game.Board, Map, null);
      Assert.AreEqual(Color.White.ToArgb(), bitmap.GetPixel(270, 150).ToArgb());
      renderer.Render(graphics, game.Board, Map, king);
      Assert.IsTrue(ContainsColor(bitmap, new Rectangle(265, 145, 10, 10), Color.ForestGreen));
      Assert.IsTrue(ContainsColor(bitmap, new Rectangle(265, 385, 10, 10), Color.Firebrick));
    }

    [TestMethod]
    public void CastlingAttackPath_IsYellowRatherThanAnOrdinaryRedThreat()
    {
      var game = CastlingExplanationTests.CreateGame("1k3r2/8/8/8/8/8/8/R3K2R w KQ - 0 1");
      var renderer = new KingThreatRenderer(KingThreatAnalysis.Analyze(game,
        game.Board[KingThreatAnalysisTests.Square(game, "e1")]));
      using var bitmap = new Bitmap(480, 480);
      using var graphics = Graphics.FromImage(bitmap);
      graphics.Clear(Color.White);
      Rectangle Map(int square)
      {
        Location location = game.Board.SquareToLocation(square);
        return new Rectangle(location.File * 60, (7 - location.Rank) * 60, 60, 60);
      }
      renderer.Render(graphics, game.Board, Map, KingThreatAnalysisTests.Square(game, "f1"));
      Assert.IsTrue(ContainsColor(bitmap, new Rectangle(325, 265, 10, 10), Color.Goldenrod));
    }

    [DataTestMethod]
    [DataRow(24, false)]
    [DataRow(24, true)]
    [DataRow(40, true)]
    public void SmallAndRotatedRendering_KeepsTheSafeKingAndBlockerDistinct(int size, bool rotated)
    {
      var game = KingThreatAnalysisTests.CreateGame("7k/8/8/8/8/8/3P4/4K3 w - - 0 1");
      int king = KingThreatAnalysisTests.Square(game, "e1");
      int blocker = KingThreatAnalysisTests.Square(game, "d2");
      var renderer = new KingThreatRenderer(KingThreatAnalysis.Analyze(game, game.Board[king]));
      using var bitmap = new Bitmap(size * 8, size * 8);
      using var graphics = Graphics.FromImage(bitmap);
      graphics.Clear(Color.White);
      Rectangle Map(int square)
      {
        Location location = game.Board.SquareToLocation(square);
        return new Rectangle((rotated ? 7 - location.File : location.File) * size,
          (rotated ? location.Rank : 7 - location.Rank) * size, size, size);
      }
      renderer.Render(graphics, game.Board, Map, null);
      Assert.IsTrue(ContainsColor(bitmap, Map(king), Color.ForestGreen));
      Assert.IsTrue(ContainsColor(bitmap, Map(blocker), Color.Maroon));
    }

    private static bool ContainsColor(Bitmap bitmap, Rectangle rectangle, Color color)
    {
      for (int x = rectangle.Left; x < rectangle.Right; x++)
        for (int y = rectangle.Top; y < rectangle.Bottom; y++)
          if (bitmap.GetPixel(x, y).ToArgb() == color.ToArgb())
            return true;
      return false;
    }
  }
}
