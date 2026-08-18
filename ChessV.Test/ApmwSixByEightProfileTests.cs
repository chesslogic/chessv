using ChessV.Base;
using ChessV.Games;
using ChessV.Games.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessV.Test
{
  [TestClass]
  [DoNotParallelize]
  public class ApmwSixByEightProfileTests
  {
    [TestCleanup]
    public void Cleanup()
    {
      ApmwCore._instance = null;
    }

    [TestMethod]
    public void RegisteredGame_LoadsExactCompactPositionAndRules()
    {
      ConfigureApmwCore();
      var manager = new ChessV.Manager.Manager();

      Assert.IsTrue(manager.GameClasses.ContainsKey(ApmwProfiles.SixByEightGameName));
      Assert.AreEqual(
        typeof(ApmwSixByEightChess),
        manager.GameClasses[ApmwProfiles.SixByEightGameName]);

      GameAttribute attribute = manager.GameAttributes[ApmwProfiles.SixByEightGameName];
      Assert.AreEqual(typeof(Geometry.Rectangular), attribute.GeometryType);
      CollectionAssert.AreEqual(new[] { 6, 8, 3 }, attribute.GeometryParameters);

      var game = (ApmwSixByEightChess)manager.CreateGame(
        ApmwProfiles.SixByEightGameName);

      Assert.AreSame(ApmwProfiles.SixByEight, game.ApmwProfile);
      Assert.AreEqual(6, game.Board.NumFiles);
      Assert.AreEqual(8, game.Board.NumRanks);
      Assert.AreEqual(48, game.Board.NumSquares);
      Assert.AreEqual(
        "nbrkbn/pppppp/6/6/6/6/PPPPPP/NBRKBN",
        game.FENStart.Split(' ')[0]);
      AssertInventory(game, 0);
      AssertInventory(game, 1);

      Assert.IsFalse(game.ApmwProfile.SupportsCastling);
      Assert.AreEqual("None", game.Castling.Value);
      Assert.AreEqual("-", game.GetCustomProperty("CastleRooks"));
      Assert.AreEqual("-", game.FENStart.Split(' ')[3]);
      Assert.AreEqual("-", game.FEN["castling"]);
      Assert.IsFalse(game.GetRules().OfType<CastlingRule>().Any());
      Assert.ThrowsException<InvalidOperationException>(
        () => game.ApmwProfile.CreateCastlingPlan(0, false));

      Assert.IsTrue(game.PawnDoubleMove);
      Assert.IsTrue(game.EnPassant);
      Assert.AreEqual("Standard", game.PromotionRule.Value);
      CollectionAssert.AreEquivalent(
        new[] { "Queen", "Rook", "Bishop", "Knight" },
        game.ParseTypeListFromString(game.PromotionTypes)
          .Select(type => type.Name)
          .Distinct()
          .ToArray());
      Assert.IsTrue(game.GetRules().OfType<BasicPromotionRule>().Any());

      MoveList moves = game.RootMoveListForTest;
      moves.Reset();
      game.GenerateMovesForTest(game.CurrentSide);
      Assert.IsTrue(moves.MoveCursor > 0);
      int a2 = game.Board.DefaultNotationToSquare("a2");
      int a4 = game.Board.DefaultNotationToSquare("a4");
      Assert.IsTrue(
        Enumerable.Range(0, moves.MoveCursor)
          .Select(index => moves.GetMoveForTest(index))
          .Any(move => move.FromSquare == a2 && move.ToSquare == a4),
        "The compact stage should retain the standard initial two-square pawn move.");
    }

    private static void ConfigureApmwCore()
    {
      ApmwCore._instance = new ApmwCore();
      ApmwCore core = ApmwCore.getInstance();
      core.foundPockets = 0;
      core.foundPocketRange = 0;
      core.foundPocketGems = 0;
      core.foundPawns = 0;
      core.foundMinors = 0;
      core.foundMajors = 0;
      core.foundJacks = 0;
      core.foundQueens = 0;
      core.foundAmazons = 0;
      core.foundConsuls = 0;
      core.foundKingPromotions = 0;
      core.foundPawnForwardness = 0;
      core.foundChessmen = 0;
      core.foundMaterialBudget = 0;
      core.foundCastlers = 0;
      core.foundPlayAsWhite = 1;
      core.isGrand = false;
      core.foundArmy = null;
      core.GeriProvider = () => 0;
      core.EngineWeakeningProvider = () => 0;
      core.PlayerPocketPiecesProvider = () => new List<PieceType>();
      core.GeometryAwarePlayerPieceSetProvider = (files, ranks) =>
      {
        Assert.AreEqual(6, files);
        Assert.AreEqual(8, ranks);

        PieceType king = core.kings.Single(type => type.Name == "King");
        PieceType rook = core.majors.Single(type => type.Name == "Rook");
        PieceType bishop = core.minors.Single(type => type.Name == "Bishop");
        PieceType knight = core.minors.Single(type => type.Name == "Knight");
        PieceType pawn = core.pawns.Single(type => type.Name == "Pawn");
        var pieces = new Dictionary<KeyValuePair<int, int>, PieceType>();
        for (int file = 0; file < files; file++)
          pieces[new KeyValuePair<int, int>(3, file)] = pawn;
        pieces[new KeyValuePair<int, int>(4, 0)] = knight;
        pieces[new KeyValuePair<int, int>(4, 1)] = bishop;
        pieces[new KeyValuePair<int, int>(4, 2)] = rook;
        pieces[new KeyValuePair<int, int>(4, 3)] = king;
        pieces[new KeyValuePair<int, int>(4, 4)] = bishop;
        pieces[new KeyValuePair<int, int>(4, 5)] = knight;
        return (pieces, "QRNB");
      };
    }

    private static void AssertInventory(ApmwSixByEightChess game, int player)
    {
      Dictionary<string, int> inventory = Enumerable.Range(0, game.Board.NumSquares)
        .Select(square => game.Board[square])
        .Where(piece => piece != null && piece.Player == player)
        .GroupBy(piece => piece.PieceType.Name)
        .ToDictionary(group => group.Key, group => group.Count());

      Assert.AreEqual(12, inventory.Values.Sum());
      Assert.AreEqual(1, inventory["King"]);
      Assert.AreEqual(1, inventory["Rook"]);
      Assert.AreEqual(2, inventory["Bishop"]);
      Assert.AreEqual(2, inventory["Knight"]);
      Assert.AreEqual(6, inventory["Pawn"]);
      Assert.IsFalse(inventory.ContainsKey("Queen"));
    }
  }
}
