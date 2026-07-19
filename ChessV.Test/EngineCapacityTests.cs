using System;
using System.Collections.Generic;
using ChessV.Boards;
using ChessV.Games.Abstract;

namespace ChessV.Test
{
  [Game("Engine Capacity Test Variant",
      typeof(Geometry.Rectangular), 12, 12,
      Template = true)]
  public class EngineCapacityTestGame : GenericChess
  {
    public EngineCapacityTestGame()
      : base(12, 12, new MirrorSymmetry())
    {
    }

    public override void SetGameVariables()
    {
      base.SetGameVariables();
      PromotionRule.Value = "None";
      PromotionTypes = "";
      EnPassant = false;
      Array = "5k6/12/12/12/12/12/12/12/12/12/12/5K6";
      FENStart = Array + " w - - 0 1";
    }
  }

  [Game("Burst Move Generation Capacity Test Variant",
      typeof(Geometry.Rectangular), 12, 12,
      Template = true)]
  public class BurstMoveGenerationTestGame : GenericChess
  {
    public PieceType BurstPiece;

    public BurstMoveGenerationTestGame()
      : base(12, 12, new MirrorSymmetry())
    {
    }

    public override void SetGameVariables()
    {
      base.SetGameVariables();
      PromotionRule.Value = "None";
      PromotionTypes = "";
      EnPassant = false;
      Array = "5k6/12/12/12/12/12/12/12/12/12/12/Z4K6";
      FENStart = Array + " w - - 0 1";
    }

    public override void AddPieceTypes()
    {
      base.AddPieceTypes();
      AddPieceType(BurstPiece = new BurstCapacityPiece());
    }
  }

  public sealed class BurstCapacityPiece : PieceType
  {
    public const int GENERATED_MOVES = MoveList.INITIAL_CAPACITY + 500;

    public BurstCapacityPiece()
      : base("Burst Capacity Piece", "Burst Capacity Piece", "Z", 100, 100)
    {
      IsSliced = false;
    }

    public override void Initialize(Game game)
    {
      base.Initialize(game);
      CustomMoveGenerator = GenerateBurst;
    }

    private bool GenerateBurst(PieceType pieceType, Piece piece, MoveList moves, bool capturesOnly)
    {
      if (capturesOnly)
        return false;

      int target = -1;
      for (int square = 0; square < Game.Board.NumSquares; square++)
        if (Game.Board[square] == null)
        {
          target = square;
          break;
        }

      if (target < 0)
        return false;

      for (int index = 0; index < GENERATED_MOVES; index++)
        if (!moves.AddMove(piece.Square, target, direct: true))
          throw new InvalidOperationException($"Burst move {index} was not appended.");
      return false;
    }
  }

  [TestClass]
  public class EngineCapacityTests
  {
    private static EngineCapacityTestGame CreateGame()
    {
      var game = new EngineCapacityTestGame();
      object[] attributes = typeof(EngineCapacityTestGame)
        .GetCustomAttributes(typeof(GameAttribute), inherit: false);
      game.Initialize((GameAttribute)attributes[0], null, null);
      return game;
    }

    [TestMethod]
    public void PlayerRoster_GrowsPast64_AndCapturePromotionRoundTrips()
    {
      EngineCapacityTestGame game = CreateGame();
      var whitePawns = new List<Piece>();

      for (int square = 0; square < game.Board.NumSquares && whitePawns.Count < 70; square++)
      {
        if (game.Board[square] != null)
          continue;
        var pawn = new Piece(game, 0, game.Pawn, square);
        game.AddPiece(pawn);
        whitePawns.Add(pawn);
      }

      int captureSquare = FindEmptySquare(game);
      var blackPawn = new Piece(game, 1, game.Pawn, captureSquare);
      game.AddPiece(blackPawn);

      Assert.AreEqual(71, game.GetPieceList(0).Count);
      Assert.AreEqual(2, game.GetPieceList(1).Count);
      game.Board.Validate();

      MoveList moves = game.RootMoveListForTest;
      moves.LegalMovesOnly = false;
      moves.Reset();
      Piece movingPawn = whitePawns[0];
      int fromSquare = movingPawn.Square;
      ulong beforeCaptureHash = game.Board.HashCode;
      ulong beforeCaptureMaterialHash = game.Board.MaterialHashCode;

      Assert.IsTrue(moves.AddCapture(fromSquare, captureSquare, direct: true));
      Assert.IsTrue(moves.MakeMove(0));
      moves.UnmakeMoveForTest(0);

      Assert.AreEqual(beforeCaptureHash, game.Board.HashCode);
      Assert.AreEqual(beforeCaptureMaterialHash, game.Board.MaterialHashCode);
      Assert.AreSame(movingPawn, game.Board[fromSquare]);
      Assert.AreSame(blackPawn, game.Board[captureSquare]);

      moves.Reset();
      Assert.IsTrue(moves.BeginMoveAdd(
        MoveType.CaptureWithPromotion,
        fromSquare,
        captureSquare));
      Assert.AreSame(movingPawn, moves.AddPickup(fromSquare));
      Assert.AreSame(blackPawn, moves.AddPickup(captureSquare));
      Assert.IsTrue(moves.AddDrop(movingPawn, captureSquare, game.King));
      Assert.IsTrue(moves.EndMoveAdd(4000));

      ulong beforePromotionHash = game.Board.HashCode;
      ulong beforePromotionMaterialHash = game.Board.MaterialHashCode;
      Assert.IsTrue(moves.MakeMove(0));
      Assert.AreSame(game.King, movingPawn.PieceType);
      moves.UnmakeMoveForTest(0);

      Assert.AreSame(game.Pawn, movingPawn.PieceType);
      Assert.AreEqual(beforePromotionHash, game.Board.HashCode);
      Assert.AreEqual(beforePromotionMaterialHash, game.Board.MaterialHashCode);
      Assert.AreSame(movingPawn, game.Board[fromSquare]);
      Assert.AreSame(blackPawn, game.Board[captureSquare]);
      game.Board.Validate();
    }

    [TestMethod]
    public void MoveList_GrowsPastLegacy2048_WithoutLosingCursorRanges()
    {
      EngineCapacityTestGame game = CreateGame();
      MoveList moves = game.RootMoveListForTest;
      moves.LegalMovesOnly = false;
      moves.Reset();

      Piece king = game.GetPieceList(0)[0];
      int fromSquare = king.Square;
      int toSquare = FindEmptySquare(game);
      const int generatedMoveCount = MoveList.INITIAL_CAPACITY + 500;

      for (int index = 0; index < generatedMoveCount; index++)
        Assert.IsTrue(moves.AddMove(fromSquare, toSquare, direct: true), $"Move {index} was not appended.");

      Assert.AreEqual(generatedMoveCount, moves.Count);
      Assert.AreEqual(generatedMoveCount, moves.PickupCursorForTest);
      Assert.AreEqual(generatedMoveCount, moves.DropCursorForTest);
      Assert.IsTrue(moves.MoveCapacityForTest > MoveList.INITIAL_CAPACITY);
      Assert.IsTrue(moves.PickupCapacityForTest > MoveList.INITIAL_CAPACITY);
      Assert.IsTrue(moves.DropCapacityForTest > MoveList.INITIAL_CAPACITY);
      Assert.AreEqual(generatedMoveCount, moves.GetMoveForTest(generatedMoveCount - 1).PickupCursor);
      Assert.AreEqual(generatedMoveCount, moves.GetMoveForTest(generatedMoveCount - 1).DropCursor);

      ulong beforeHash = game.Board.HashCode;
      Assert.IsTrue(moves.MakeMove(generatedMoveCount - 1));
      moves.UnmakeMoveForTest(generatedMoveCount - 1);
      Assert.AreEqual(beforeHash, game.Board.HashCode);
      Assert.AreSame(king, game.Board[fromSquare]);
      Assert.IsNull(game.Board[toSquare]);
    }

    [TestMethod]
    public void GameGeneration_DoesNotSilentlyStopAtLegacy2048()
    {
      var game = new BurstMoveGenerationTestGame();
      object[] attributes = typeof(BurstMoveGenerationTestGame)
        .GetCustomAttributes(typeof(GameAttribute), inherit: false);

      game.Initialize((GameAttribute)attributes[0], null, null);

      Assert.IsTrue(game.RootMoveListForTest.Count >= BurstCapacityPiece.GENERATED_MOVES);
      Assert.IsTrue(game.RootMoveListForTest.MoveCapacityForTest > MoveList.INITIAL_CAPACITY);
    }

    [TestMethod]
    public void MoveList_GuardsItsExplicitHardLimit()
    {
      EngineCapacityTestGame game = CreateGame();
      MoveList moves = game.RootMoveListForTest;
      moves.LegalMovesOnly = false;
      moves.Reset();
      int fromSquare = game.GetPieceList(0)[0].Square;
      int toSquare = FindEmptySquare(game);

      Assert.AreEqual(16 * 1024, MoveList.MAX_MOVES);
      for (int index = 0; index < MoveList.MAX_MOVES; index++)
        Assert.IsTrue(moves.AddMove(fromSquare, toSquare, direct: true));

      Assert.AreEqual(MoveList.MAX_MOVES, moves.MoveCapacityForTest);
      Assert.AreEqual(MoveList.MAX_MOVES, moves.PickupCapacityForTest);
      Assert.AreEqual(MoveList.MAX_MOVES, moves.DropCapacityForTest);

      InvalidOperationException moveException = Assert.ThrowsException<InvalidOperationException>(
        () => moves.AddMove(fromSquare, toSquare, direct: true));
      InvalidOperationException pickupException = Assert.ThrowsException<InvalidOperationException>(
        () => moves.EnsurePickupCapacityForTest(MoveList.MAX_MOVES + 1));
      InvalidOperationException dropException = Assert.ThrowsException<InvalidOperationException>(
        () => moves.EnsureDropCapacityForTest(MoveList.MAX_MOVES + 1));

      StringAssert.Contains(moveException.Message, "guarded limit");
      StringAssert.Contains(pickupException.Message, "guarded limit");
      StringAssert.Contains(dropException.Message, "guarded limit");
      Assert.IsFalse(moves.CanAddMoves());
    }

    [TestMethod]
    public void BoardWithCards_12x12PlusSixSquares_FitsBitBoardAndMoveEncoding()
    {
      var board = new BoardWithCards(12, 12, handSize: 3);
      Assert.AreEqual(144, board.NumSquares);
      Assert.AreEqual(150, board.NumSquaresExtended);

      int lastSquare = board.NumSquaresExtended - 1;
      Location lastLocation = board.SquareToLocation(lastSquare);
      Assert.AreEqual(1, lastLocation.Rank);
      Assert.AreEqual(-3, lastLocation.File);
      Assert.AreEqual(lastSquare, board.LocationToSquare(lastLocation));

      var bitBoard = new BitBoard(board.NumSquaresExtended);
      bitBoard.SetBit(lastSquare);
      Assert.AreEqual(1, bitBoard.GetBit(lastSquare));
      Assert.AreEqual(lastSquare, bitBoard.ExtractLSB());

      var movement = new Movement(lastSquare, lastSquare - 1, 1, MoveType.StandardMove);
      var roundTrip = new Movement(movement.Hash);
      Assert.AreEqual(lastSquare, roundTrip.FromSquare);
      Assert.AreEqual(lastSquare - 1, roundTrip.ToSquare);
    }

    [TestMethod]
    public void UnsupportedExtendedBoardCapacity_FailsBeforeAllocation()
    {
      NotSupportedException exception = Assert.ThrowsException<NotSupportedException>(
        () => new BoardWithCards(12, 12, handSize: 25));

      StringAssert.Contains(exception.Message, "BitBoard supports at most 192 squares");
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => new BitBoard(BitBoard.MAX_BITS + 1));
    }

    private static int FindEmptySquare(Game game)
    {
      for (int square = 0; square < game.Board.NumSquares; square++)
        if (game.Board[square] == null)
          return square;
      Assert.Fail("Test setup requires an empty square.");
      return -1;
    }
  }
}
