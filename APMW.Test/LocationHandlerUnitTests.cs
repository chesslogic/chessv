using Archipelago.MultiClient.Net.Helpers;
using ChessV;
using ChessV.Base;
using Moq;

namespace Archipelago.APChessV
{

  [TestClass]
  public class LocationHandlerUnitTests
  {
    LocationHandler handler;
    Mock<ILocationCheckHelper> locations;
    Mock<ChessV.Match> match;
    Mock<Game> game;
    Mock<Board> board;
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

      board = new Mock<Board>();
      game = new Mock<Game>();
      match = new Mock<ChessV.Match>();
      match.SetupGet(mock => mock.Game).Returns(game.Object);
      match.Setup(mock => mock.GetPlayer(0)).Returns(player.Object);
      game.SetupGet(mock => mock.Board).Returns(board.Object);
      game.SetupGet(mock => mock.GameTurnNumber).Returns(1);
      game.SetupGet(mock => mock.NumFiles).Returns(8);
      game.SetupGet(mock => mock.BoardMoveStack).Returns(new BoardMoveStack(board.Object));
      board.Setup(mock => mock.GetRank(It.IsAny<int>())).Returns(0);
      board.Setup(mock => mock.GetFile(0)).Returns(0);
      board.Setup(mock => mock.GetFile(1)).Returns(1);
      board.Setup(mock => mock.GetFile(2)).Returns(2);
      board.Setup(mock => mock.GetFile(3)).Returns(3);
      board.Setup(mock => mock.GetFile(4)).Returns(4);
      board.Setup(mock => mock.GetFile(5)).Returns(5);
      board.Setup(mock => mock.GetFileNotation(0)).Returns("a");
      board.Setup(mock => mock.GetFileNotation(1)).Returns("b");
      board.Setup(mock => mock.GetFileNotation(2)).Returns("c");
      board.Setup(mock => mock.GetFileNotation(3)).Returns("d");
      board.Setup(mock => mock.GetFileNotation(4)).Returns("e");
      board.Setup(mock => mock.GetFileNotation(5)).Returns("f");

      firstPieceType = new Mock<PieceType>();
      firstPiece = new Mock<Piece>();
      firstPiece.SetupGet(mock => mock.PieceType).Returns(firstPieceType.Object);
      firstPieceType.SetupGet(mock => mock.Name).Returns("Bishop");
      secondPieceType = new Mock<PieceType>();
      secondPiece = new Mock<Piece>();
      secondPiece.SetupGet(mock => mock.PieceType).Returns(secondPieceType.Object);
      secondPieceType.SetupGet(mock => mock.Name).Returns("Pawn");

      //board.Setup(mock => mock.GetFile(1)).Returns(0);

      var core = ApmwCore.getInstance();
      core.kings = new System.Collections.Generic.List<PieceType>();
      core.pawns = new System.Collections.Generic.HashSet<PieceType>();
      core.minors = new System.Collections.Generic.HashSet<PieceType>();
      core.majors = new System.Collections.Generic.HashSet<PieceType>();
      core.queens = new System.Collections.Generic.HashSet<PieceType>();

      handler = new LocationHandler(locations.Object, match.Object);
    }

    [TestMethod]
    public void updateMoveState_findsNewPiece()
    {
      locations.Setup(locs => locs.GetLocationIdFromName("ChecksMate", It.IsAny<string>()));

      MoveInfo info = GetMoveInfo();
      handler.UpdateMoveState(info);
      info = GetMoveInfo();
      info.FromSquare = 2;
      info.MoveType = MoveType.StandardCapture;
      handler.HandleMove(info);

      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture Piece Queen's Knight"));
    }

    [TestMethod]
    public void updateMoveState_findsDifferentPiece()
    {
      locations.Setup(locs => locs.GetLocationIdFromName("ChecksMate", It.IsAny<string>()));

      MoveInfo info = GetMoveInfo();
      info.FromSquare = 0;
      handler.UpdateMoveState(info);
      info = GetMoveInfo();
      info.FromSquare = 2;
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
      info.FromSquare = 3;
      info.ToSquare = 5;
      handler.UpdateMoveState(info);
      info = GetMoveInfo();
      info.FromSquare = 2;
      info.ToSquare = 5;
      info.MoveType = MoveType.StandardCapture;
      handler.HandleMove(info);

      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture Piece Queen's Knight"));
    }

    private MoveInfo GetMoveInfo()
    {
      MoveInfo info = new MoveInfo();

      info.Player = 0;
      info.FromSquare = 1;
      info.ToSquare = 3;
      info.MoveType = MoveType.StandardMove;
      info.PieceMoved = secondPiece.Object;
      info.PieceCaptured = firstPiece.Object;
      return info;
    }
  }
}
