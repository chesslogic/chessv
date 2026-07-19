using Archipelago.MultiClient.Net.Helpers;
using ChessV;
using ChessV.Base;
using Moq;
using System.Collections.Generic;
using System.Linq;

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
    public void locationProfile_scalesCaptureBandsAndCheckmatesThroughTwelveByTwelve()
    {
      ApmwLocationProfile profile = ApmwLocationProfile.For(12, 12);

      Assert.AreEqual(12, profile.CpuPawnCount);
      Assert.AreEqual(11, profile.CpuNonKingCount);
      Assert.AreEqual(22, profile.MaximumAnyCaptureCount);
      Assert.AreEqual("Checkmate 12x12", profile.CheckmateLocation);
      CollectionAssert.AreEqual(
        new[]
        {
          "Checkmate Minima",
          "Checkmate Maxima",
          "Checkmate 10x10",
          "Checkmate 12x10",
          "Checkmate 12x12",
        },
        profile.CheckmateLocationsThroughStage().ToArray());
    }

    [TestMethod]
    public void goalStage_preservesLegacyTenByEightAndUsesCurrentTwelveByTwelve()
    {
      Assert.IsTrue(LocationHandler.IsGoalStage(
        ApmwLocationProfile.For(10, 8),
        Goal.Progressive,
        hasGeometryContract: false));
      Assert.IsFalse(LocationHandler.IsGoalStage(
        ApmwLocationProfile.For(10, 8),
        Goal.Progressive,
        hasGeometryContract: true));
      Assert.IsTrue(LocationHandler.IsGoalStage(
        ApmwLocationProfile.For(12, 12),
        Goal.Progressive,
        hasGeometryContract: true));
      Assert.IsFalse(LocationHandler.IsGoalStage(
        ApmwLocationProfile.For(12, 12),
        Goal.Progressive,
        hasGeometryContract: false));
      Assert.IsTrue(LocationHandler.IsCaptureEverythingStage(
        ApmwLocationProfile.For(10, 8),
        Goal.Progressive,
        hasGeometryContract: false));
      Assert.IsTrue(LocationHandler.IsCaptureEverythingStage(
        ApmwLocationProfile.For(12, 10),
        Goal.Progressive,
        hasGeometryContract: true));
      Assert.IsFalse(LocationHandler.IsCaptureEverythingStage(
        ApmwLocationProfile.For(10, 10),
        Goal.Progressive,
        hasGeometryContract: true));
    }

    [TestMethod]
    public void captureLookup_mapsTwelveFileOuterAttendants()
    {
      var lookup = new CaptureLookup();

      Assert.AreEqual(
        "Capture Piece Queen's Outer Attendant",
        lookup.fileToLocation(12, "B"));
      Assert.AreEqual(
        "Capture Piece King's Outer Attendant",
        lookup.fileToLocation(12, "K"));
      Assert.AreEqual(
        "Capture Piece King's Rook",
        lookup.fileToLocation(12, "L"));
    }

    [TestMethod]
    public void handleMove_usesTwelveFileOriginalBackRankMapping()
    {
      ConfigureBoard(12, 10);
      MoveInfo capture = GetMoveInfo();
      capture.FromSquare = 24;
      capture.ToSquare = 10;
      capture.MoveType = MoveType.StandardCapture;

      handler.HandleMove(capture);

      locations.Verify(locs => locs.GetLocationIdFromName(
        "ChecksMate",
        "Capture Piece King's Outer Attendant"));
    }

    [TestMethod]
    public void findSecondaryMove_tracksActualInnerCastlerDestination()
    {
      object king = new object();
      object outerCastler = new object();
      object innerCastler = new object();
      var before = new Dictionary<int, object>
      {
        [1] = outerCastler,
        [4] = king,
        [8] = innerCastler,
      };
      var after = new Dictionary<int, object>
      {
        [1] = outerCastler,
        [5] = innerCastler,
        [6] = king,
      };

      LocationHandler.SecondaryMove moved = LocationHandler.FindSecondaryMove(
        before,
        after,
        primaryFromSquare: 4);

      Assert.IsNotNull(moved);
      Assert.AreEqual(8, moved.FromSquare);
      Assert.AreEqual(5, moved.ToSquare);
    }

    [TestMethod]
    public void findSecondaryMove_rejectsAmbiguousMultiPieceMoves()
    {
      object king = new object();
      object first = new object();
      object second = new object();
      var before = new Dictionary<int, object>
      {
        [0] = king,
        [1] = first,
        [2] = second,
      };
      var after = new Dictionary<int, object>
      {
        [1] = second,
        [2] = first,
        [3] = king,
      };

      Assert.IsNull(LocationHandler.FindSecondaryMove(
        before,
        after,
        primaryFromSquare: 0));
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

    private void ConfigureBoard(int files, int ranks)
    {
      game.SetupGet(mock => mock.NumFiles).Returns(files);
      board.SetupGet(mock => mock.NumSquares).Returns(files * ranks);
      board.Setup(mock => mock.GetRank(It.IsAny<int>())).Returns<int>(square => square / files);
      board.Setup(mock => mock.GetFile(It.IsAny<int>())).Returns<int>(square => square % files);
      board.Setup(mock => mock.GetFileNotation(It.IsAny<int>()))
        .Returns<int>(file => ((char)('a' + file)).ToString());
    }
  }
}
