using System.Collections.Generic;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using ChessV.Games.Pieces.Apmw;

namespace ChessV.Test
{
  [Game("Apmw Mixed En Passant Edge Test Variant",
      typeof(Geometry.Rectangular), 8, 8,
      Template = true)]
  public class ApmwMixedEnPassantEdgeGame : ApmwChessGame
  {
    public override void SetGameVariables()
    {
      base.SetGameVariables();
      Castling.Value = "None";
      FENStart = "4k3/8/8/8/8/8/3P4/K7 w - - e1 0 1";
    }
  }

  [TestClass]
  public class ApmwMixedEnPassantReproTests
  {
    private const string GameName = "Archipelago Multiworld";

    [TestInitialize]
    public void ResetGlobalApmwState()
    {
      ResetApmwCore();
    }

    [TestMethod]
    public void InitialApmwPosition_GenerationKeepsBoardAndMoveListCoherent()
    {
      Game game = CreateApmw();

      GenerateAndValidate(game, game.CurrentSide, "InitialApmwPosition");
    }

    [TestMethod]
    public void MixedEnPassant_OffBoardCaptureScan_DoesNotThrowDuringLoadFen()
    {
      Game game = CreateEdgeCaseGame();

      AssertMoveListCoherent(game, game.RootMoveListForTest, "MixedEnPassant_OffBoardCaptureScan");
    }

    private static Game CreateApmw()
    {
      Manager.Manager manager = new Manager.Manager();
      return manager.CreateGame(GameName);
    }

    private static Game CreateEdgeCaseGame()
    {
      ApmwMixedEnPassantEdgeGame game = new ApmwMixedEnPassantEdgeGame();
      object[] attrs = typeof(ApmwMixedEnPassantEdgeGame)
        .GetCustomAttributes(typeof(GameAttribute), inherit: false);
      Assert.IsTrue(attrs.Length > 0, "Test game must carry a [Game] attribute.");
      game.Initialize((GameAttribute)attrs[0], null, null);
      return game;
    }

    private static void GenerateAndValidate(Game game, int player, string scenario)
    {
      Piece[] before = SnapshotBoard(game);
      ulong hashBefore = game.Board.HashCode;
      MoveList root = game.RootMoveListForTest;

      root.Reset();
      game.GenerateMovesForTest(player);

      Assert.AreEqual(hashBefore, game.Board.HashCode, scenario + ": generation changed board hash.");
      AssertBoardUnchanged(game, before, scenario);
      AssertMoveListCoherent(game, root, scenario);
    }

    private static Piece[] SnapshotBoard(Game game)
    {
      Piece[] snap = new Piece[game.Board.NumSquaresExtended];
      for (int sq = 0; sq < game.Board.NumSquaresExtended; sq++)
        snap[sq] = game.Board[sq];
      return snap;
    }

    private static void AssertBoardUnchanged(Game game, Piece[] before, string context)
    {
      for (int sq = 0; sq < before.Length; sq++)
      {
        if (!object.ReferenceEquals(before[sq], game.Board[sq]))
        {
          Assert.Fail(
            context + ": board state mutated at square " + sq +
            ". before=" + FormatPiece(before[sq]) +
            " after=" + FormatPiece(game.Board[sq]));
        }
      }
    }

    private static void AssertMoveListCoherent(Game game, MoveList root, string context)
    {
      Assert.IsNotNull(root, context + ": root MoveList was null.");
      Assert.AreEqual(root.MoveCursor, root.Count, context + ": Count must mirror MoveCursor.");

      for (int i = 0; i < root.MoveCursor; i++)
      {
        MoveInfo move = root.GetMoveForTest(i);
        int firstPickup = i == 0 ? 0 : root.GetMoveForTest(i - 1).PickupCursor;
        int firstDrop = i == 0 ? 0 : root.GetMoveForTest(i - 1).DropCursor;

        Assert.IsTrue(move.PickupCursor >= firstPickup,
          context + ": pickup cursor went backwards at move " + i + ".");
        Assert.IsTrue(move.DropCursor >= firstDrop,
          context + ": drop cursor went backwards at move " + i + ".");
        Assert.IsTrue(move.PickupCursor <= root.PickupCursorForTest,
          context + ": move pickup cursor exceeds root pickup cursor at move " + i + ".");
        Assert.IsTrue(move.DropCursor <= root.DropCursorForTest,
          context + ": move drop cursor exceeds root drop cursor at move " + i + ".");

        for (int p = firstPickup; p < move.PickupCursor; p++)
        {
          Pickup pickup = root.GetPickupForTest(p);
          Assert.IsTrue(pickup.Square >= 0 && pickup.Square < game.Board.NumSquaresExtended,
            context + ": pickup square out of bounds at move " + i + ", pickup " + p + ".");
          Assert.IsNotNull(game.Board[pickup.Square],
            context + ": pickup references empty square at move " + i + ", pickup " + p + ".");
        }
      }
    }

    private static string FormatPiece(Piece piece)
    {
      return piece == null ? "empty" : piece.PieceType.Name + ":" + piece.Player;
    }

    private static void ResetApmwCore()
    {
      ApmwCore core = ApmwCore.getInstance();
      core.foundPockets = -1;
      core.foundPocketRange = -1;
      core.foundPocketGems = -1;
      core.foundPawns = -1;
      core.foundMinors = -1;
      core.foundMajors = -1;
      core.foundJacks = -1;
      core.foundQueens = -1;
      core.foundAmazons = -1;
      core.foundConsuls = -1;
      core.foundKingPromotions = -1;
      core.foundPawnForwardness = -1;
      core.isGrand = false;
      core.foundArmy = null;

      PieceType king = new King("King", "K", 325, 325);
      PieceType rook = new Rook("Rook", "R", 500, 550);
      PieceType pawn = new Pawn("Pawn", "P", 100, 125);

      Dictionary<KeyValuePair<int, int>, PieceType> starters =
        new Dictionary<KeyValuePair<int, int>, PieceType>();
      for (int f = 0; f < 8; f++)
        starters[new KeyValuePair<int, int>(3, f)] = pawn;
      starters[new KeyValuePair<int, int>(4, 0)] = rook;
      starters[new KeyValuePair<int, int>(4, 4)] = king;
      starters[new KeyValuePair<int, int>(4, 7)] = rook;

      core.PlayerPieceSetProvider = (numFiles) => (starters, "QRNB");
      core.PlayerPocketPiecesProvider = () => new List<PieceType>();
      core.GeriProvider = () => 1;
      core.EngineWeakeningProvider = () => 0;
    }
  }
}
