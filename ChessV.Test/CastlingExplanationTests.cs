using System.Linq;
using ChessV.Games;

namespace ChessV.Test
{
  [Game("Castling Hint Test", typeof(Geometry.Rectangular), 8, 8, Template = true)]
  public class CastlingHintTestGame : CheckersCannonTestGame
  {
    public override void AddRules()
    {
      base.AddRules();
      AddCastlingRule();
      CastlingMove(0, "e1", "g1", "h1", "f1", 'K');
      CastlingMove(0, "e1", "c1", "a1", "d1", 'Q');
      CastlingMove(1, "e8", "g8", "h8", "f8", 'k');
      CastlingMove(1, "e8", "c8", "a8", "d8", 'q');
    }
  }

  [TestClass]
  public class CastlingExplanationTests
  {
    [TestMethod]
    public void AttackedCastlingTransit_IsAttributedToTheAttacker()
    {
      CastlingHintTestGame game = CreateGame("1k3r2/8/8/8/8/8/8/R3K2R w KQ - 0 1");
      Piece king = game.Board[KingThreatAnalysisTests.Square(game, "e1")];

      KingDestinationHint transit = KingThreatAnalysis.Analyze(game, king).Destinations
        .Single(h => h.IsCastling && h.Square == KingThreatAnalysisTests.Square(game, "f1"));

      Assert.IsTrue(transit.EngineAttacked);
      Assert.AreEqual(KingThreatAnalysisTests.Square(game, "f8"), transit.Threats.Single().SourceSquare);
    }

    [DataTestMethod]
    [DataRow("1k3r2/8/8/8/8/8/8/RN1BK2R w KQ - 0 1", 3)]
    [DataRow("1k3r2/8/8/8/8/8/8/RN1BK2R w - - 0 1", 0)]
    public void CastlingExplainsOccupiedPathsAndAttacksButNotLostRights(string fen, int count)
    {
      CastlingHintTestGame game = CreateGame(fen);
      Piece king = game.Board[KingThreatAnalysisTests.Square(game, "e1")];
      string state = KingThreatAnalysisTests.State(game);
      KingDestinationHint[] restrictions = KingThreatAnalysis.Analyze(game, king).Destinations
        .Where(h => h.IsCastling).ToArray();
      Assert.AreEqual(count, restrictions.Length);
      if (count > 0)
      {
        CollectionAssert.AreEquivalent(new[] { "b1", "d1", "f1" },
          restrictions.Select(h => game.Board.GetDefaultSquareNotation(h.Square)).ToArray());
        Assert.AreEqual(2, restrictions.Count(h => h.FriendlyOccupied));
        Assert.AreEqual(1, restrictions.Count(h => h.EngineAttacked && h.Threats.Count == 1));
      }
      Assert.AreEqual(state, KingThreatAnalysisTests.State(game));
    }

    internal static CastlingHintTestGame CreateGame(string fen)
    {
      var game = new CastlingHintTestGame();
      var attribute = (GameAttribute)typeof(CastlingHintTestGame)
        .GetCustomAttributes(typeof(GameAttribute), false).Single();
      game.Initialize(attribute, null, null);
      game.LoadFEN(fen);
      return game;
    }
  }
}
