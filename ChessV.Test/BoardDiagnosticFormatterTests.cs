using ChessV.Games;

namespace ChessV.Test
{
  [TestClass]
  public class BoardDiagnosticFormatterTests
  {
    [TestMethod]
    public void Format_ChessStartingPosition_IncludesFullGridAndOccupiedCellList()
    {
      Chess game = CreateGame<Chess>();

      string text = BoardDiagnosticFormatter.Format(game);

      StringAssert.Contains(text, "Dimensions: 8 files x 8 ranks");
      StringAssert.Contains(text, "a  b  c  d  e  f  g  h");
      StringAssert.Contains(text, "8 | r1 n1 b1 q1 k1 b1 n1 r1 | 8");
      StringAssert.Contains(text, "1 | R0 N0 B0 Q0 K0 B0 N0 R0 | 1");
      StringAssert.Contains(text, "Occupied cells:");
      StringAssert.Contains(text, "a1=R0");
      StringAssert.Contains(text, "h8=r1");
    }

    [TestMethod]
    public void Format_CapablancaStartingPosition_UsesActualTenByEightBoardSize()
    {
      CapablancaChess game = CreateGame<CapablancaChess>();

      string text = BoardDiagnosticFormatter.Format(game);

      StringAssert.Contains(text, "Dimensions: 10 files x 8 ranks");
      StringAssert.Contains(text, "a  b  c  d  e  f  g  h  i  j");
      StringAssert.Contains(text, "8 | r1 n1 a1 b1 q1 k1 b1 c1 n1 r1 | 8");
      StringAssert.Contains(text, "1 | R0 N0 A0 B0 Q0 K0 B0 C0 N0 R0 | 1");
      StringAssert.Contains(text, "j1=R0");
      StringAssert.Contains(text, "j8=r1");
    }

    [TestMethod]
    public void Format_UsesActualPlayerNotationForOccupiedCells()
    {
      Chess game = CreateGame<Chess>();

      string text = BoardDiagnosticFormatter.Format(game);

      StringAssert.Contains(text, "a1=R0");
      StringAssert.Contains(text, "a8=r1");
      Assert.IsFalse(text.Contains("a8=R1"),
        "Black occupied cells must use the black/player-1 piece notation, not player 0 notation.");
    }

    [TestMethod]
    public void Format_NullGame_ReturnsUnavailableDiagnostic()
    {
      string text = BoardDiagnosticFormatter.Format((Game) null);

      StringAssert.Contains(text, "Board Diagnostic");
      StringAssert.Contains(text, "Board diagnostic unavailable: game is null.");
    }

    private static TGame CreateGame<TGame>() where TGame : Game, new()
    {
      var game = new TGame();
      object[] attrs = typeof(TGame).GetCustomAttributes(typeof(GameAttribute), false);
      Assert.IsTrue(attrs.Length > 0, typeof(TGame).Name + " test game must carry a [Game] attribute.");
      game.Initialize((GameAttribute) attrs[0], null, null);
      return game;
    }
  }
}
