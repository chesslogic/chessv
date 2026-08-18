using Archipelago.MultiClient.Net.Helpers;
using ChessV;
using ChessV.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Archipelago.APChessV
{

  [TestClass]
  public class LocationHandlerUnitTests
  {
    TestLocationHandler handler;
    Mock<ILocationCheckHelper> locations;
    Mock<ChessV.Match> match;
    Mock<ChessV.Games.Chess> game;
    Mock<Player> player;

    Mock<Piece> firstPiece;
    Mock<Piece> secondPiece;
    Mock<PieceType> firstPieceType;
    Mock<PieceType> secondPieceType;


    MoveInfo info;

    public LocationHandlerUnitTests()
    {
    }

    [TestInitialize()]
    public void beforeEach()
    {
      locations = new Mock<ILocationCheckHelper>();

      player = new Mock<Player>();
      player.SetupGet(mock => mock.IsHuman).Returns(true);

      game = new Mock<ChessV.Games.Chess>() { CallBase = true };
      InitializeChessGame(game.Object);
      match = new Mock<ChessV.Match>();
      match.SetupGet(mock => mock.Game).Returns(game.Object);
      match.Setup(mock => mock.GetPlayer(0)).Returns(player.Object);

      firstPieceType = new Mock<PieceType>();
      firstPiece = new Mock<Piece>();
      firstPiece.SetupGet(mock => mock.PieceType).Returns(firstPieceType.Object);
      firstPieceType.SetupGet(mock => mock.Name).Returns("Bishop");
      secondPieceType = new Mock<PieceType>();
      secondPiece = new Mock<Piece>();
      secondPiece.SetupGet(mock => mock.PieceType).Returns(secondPieceType.Object);
      secondPieceType.SetupGet(mock => mock.Name).Returns("Pawn");

      handler = new TestLocationHandler();
      handler.Initialize(locations.Object, null);
      handler.StartMatch(match.Object);
    }

    [TestMethod]
    public void updateMoveState_findsNewPiece()
    {
      locations.Setup(locs => locs.GetLocationIdFromName("ChecksMate", It.IsAny<string>()));

      MoveInfo info = GetMoveInfo();
      handler.UpdateMoveState(info);
      info = GetMoveInfo();
      info.FromSquare = game.Object.NotationToSquare("c1");
      info.MoveType = MoveType.StandardCapture;
      handler.HandleMove(info);

      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture Piece Queen's Knight"));
    }

    [TestMethod]
    public void updateMoveState_findsDifferentPiece()
    {
      locations.Setup(locs => locs.GetLocationIdFromName("ChecksMate", It.IsAny<string>()));

      MoveInfo info = GetMoveInfo();
      info.FromSquare = game.Object.NotationToSquare("a1");
      handler.UpdateMoveState(info);
      info = GetMoveInfo();
      info.FromSquare = game.Object.NotationToSquare("c1");
      info.MoveType = MoveType.StandardCapture;
      handler.HandleMove(info);

      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture Piece Queen's Rook"));
    }

    [TestMethod]
    public void updateMoveState_findsMovedPiece()
    {
      locations.Setup(locs => locs.GetLocationIdFromName("ChecksMate", It.IsAny<string>()));

      MoveInfo info = GetMoveInfo();
      handler.UpdateMoveState(info);
      info = GetMoveInfo();
      info.FromSquare = game.Object.NotationToSquare("d1");
      info.ToSquare = game.Object.NotationToSquare("f1");
      handler.UpdateMoveState(info);
      info = GetMoveInfo();
      info.FromSquare = game.Object.NotationToSquare("c1");
      info.ToSquare = game.Object.NotationToSquare("f1");
      info.MoveType = MoveType.StandardCapture;
      handler.HandleMove(info);

      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture Piece Queen's Knight"));
    }

    [TestMethod]
    public void handleMove_countsPawnAndPieceThreatsFromSameAttackerAsFork()
    {
      var chess = CreateEmptyChessGame();
      var attacker = PlacePiece(chess, 0, chess.Knight, "d4");
      PlacePiece(chess, 1, chess.Pawn, "e6");
      PlacePiece(chess, 1, chess.Rook, "f5");

      HandleMoveOn(chess, attacker);

      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Threaten Pawn"), Times.AtLeastOnce);
      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Fork, Sacrificial"), Times.AtLeastOnce);
    }

    [TestMethod]
    public void handleMove_countsDefendedEqualValueTargetTowardSacrificialFork()
    {
      var chess = CreateEmptyChessGame();
      var attacker = PlacePiece(chess, 0, chess.Knight, "d4");
      PlacePiece(chess, 1, chess.Bishop, "f5");
      PlacePiece(chess, 1, chess.Queen, "e6");
      PlacePiece(chess, 1, chess.Rook, "f8");

      HandleMoveOn(chess, attacker);

      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Fork, True"), Times.Never);
      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Fork, Sacrificial"), Times.AtLeastOnce);
    }

    [TestMethod]
    public void handleMove_findsTrueForkWhenDefendedAttackerHasOneMoreValuableAttacker()
    {
      var chess = CreateEmptyChessGame();
      var attacker = PlacePiece(chess, 0, chess.Knight, "d4");
      PlacePiece(chess, 0, chess.Pawn, "c3");
      PlacePiece(chess, 1, chess.Rook, "e6");
      PlacePiece(chess, 1, chess.Rook, "f5");
      PlacePiece(chess, 1, chess.Queen, "d8");

      HandleMoveOn(chess, attacker);

      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Fork, Sacrificial"), Times.AtLeastOnce);
      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Fork, True"), Times.AtLeastOnce);
    }

    private ChessV.Games.Chess CreateEmptyChessGame()
    {
      var chess = new ChessV.Games.Chess();
      InitializeChessGame(chess);
      return chess;
    }

    private void InitializeChessGame(ChessV.Games.Chess chess)
    {
      object[] attributes = typeof(ChessV.Games.Chess).GetCustomAttributes(typeof(GameAttribute), false);
      var gameAttribute = (GameAttribute)attributes[0];
      gameAttribute.GameName = "Archipelago Multiworld";
      chess.Initialize(gameAttribute, null, null);
      chess.ClearGameState();

      var core = ApmwCore.getInstance();
      core.kings = new List<PieceType> { chess.King };
      core.pawns = new HashSet<PieceType> { chess.Pawn };
      core.minors = new HashSet<PieceType> { chess.Bishop, chess.Knight };
      core.majors = new HashSet<PieceType> { chess.Rook };
      core.queens = new HashSet<PieceType> { chess.Queen };
    }

    private Piece PlacePiece(ChessV.Games.Chess chess, int player, PieceType pieceType, string square)
    {
      var piece = new Piece(chess, player, pieceType, chess.NotationToSquare(square));
      chess.AddPiece(piece);
      return piece;
    }

    private void HandleMoveOn(ChessV.Games.Chess chess, Piece attacker)
    {
      match.SetupGet(mock => mock.Game).Returns(chess);
      handler.HandleMove(new MoveInfo
      {
        Player = 0,
        FromSquare = attacker.Square,
        ToSquare = attacker.Square,
        MoveType = MoveType.StandardMove,
        PieceMoved = attacker
      });
    }

    private sealed class TestLocationHandler : LocationHandler
    {
      public new void UpdateMoveState(MoveInfo info)
      {
        base.UpdateMoveState(info);
      }
    }

    private MoveInfo GetMoveInfo()
    {
      MoveInfo info = new MoveInfo();

      info.Player = 0;
      info.FromSquare = game.Object.NotationToSquare("b1");
      info.ToSquare = game.Object.NotationToSquare("d1");
      info.MoveType = MoveType.StandardMove;
      info.PieceMoved = secondPiece.Object;
      info.PieceCaptured = firstPiece.Object;
      return info;
    }
  }
}
