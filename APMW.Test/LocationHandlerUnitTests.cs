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
      locations.Setup(locs => locs.GetLocationIdFromName("ChecksMate", It.IsAny<string>())).Returns(0L);

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
      board.SetupGet(mock => mock.NumSquares).Returns(64);
      board.Setup(mock => mock.GetRank(It.IsAny<int>())).Returns<int>(square => square / 8);
      board.Setup(mock => mock.GetFile(It.IsAny<int>())).Returns<int>(square => square % 8);
      board.Setup(mock => mock.GetFileNotation(It.IsAny<int>())).Returns<int>(file => ((char)('a' + file)).ToString());

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

    [TestMethod]
    public void moveTakenBack_restoresPieceCaptureCounter()
    {
      MoveInfo info = GetMoveInfo();
      info.FromSquare = 12;
      info.ToSquare = 0;
      info.MoveType = MoveType.StandardCapture;

      PlayMove(info);
      TakeBackLastMove();
      PlayMove(info);

      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture Piece Queen's Rook"), Times.Exactly(2));
      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture Any 2"), Times.Never());
      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture 2 Pieces"), Times.Never());
    }

    [TestMethod]
    public void moveTakenBack_restoresPawnCaptureCounter()
    {
      MoveInfo info = GetMoveInfo();
      info.FromSquare = 16;
      info.ToSquare = 8;
      info.MoveType = MoveType.StandardCapture;

      PlayMove(info);
      TakeBackLastMove();
      PlayMove(info);

      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture Pawn A"), Times.Exactly(2));
      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture Any 2"), Times.Never());
      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture 2 Pawns"), Times.Never());
    }

    [TestMethod]
    public void moveTakenBack_restoresOriginalSquareTracking()
    {
      MoveInfo firstMove = GetMoveInfo();
      firstMove.FromSquare = 1;
      firstMove.ToSquare = 3;
      firstMove.PieceCaptured = null;
      PlayMove(firstMove);

      MoveInfo overwriteMove = GetMoveInfo();
      overwriteMove.FromSquare = 2;
      overwriteMove.ToSquare = 3;
      overwriteMove.PieceCaptured = null;
      PlayMove(overwriteMove);

      TakeBackLastMove();

      MoveInfo capture = GetMoveInfo();
      capture.FromSquare = 5;
      capture.ToSquare = 3;
      capture.MoveType = MoveType.StandardCapture;
      PlayMove(capture);

      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture Piece Queen's Knight"), Times.Once());
      locations.Verify(locs => locs.GetLocationIdFromName("ChecksMate", "Capture Piece Queen's Bishop"), Times.Never());
    }

    private void PlayMove(MoveInfo info)
    {
      handler.SetupMove(info);
      handler.HandleMove(info);
    }

    private void TakeBackLastMove()
    {
      handler.MoveTakenBackForTesting();
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
