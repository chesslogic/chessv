using System.Collections.Generic;
using System.Linq;

namespace ChessV.Games.Pieces.Apmw
{
  [PieceType("Checkers", "APMW Custom Pieces")]
  public class Checkers : PieceType

  {
    public List<PieceType> PromotionTypes { get; private set; }

    public Checkers(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = "CircleLittle") :
      base("Checkers", name, notation, midgameValue, endgameValue, preferredImageName)
    {
      IsPawn = true;
      IsSliced = false;
      AddMoves(this);

      // Customize piece-square-tables for the Checkers piece
      PSTMidgameForwardness = 7;
      PSTEndgameForwardness = 10;
      PSTMidgameInSmallCenter = 6;
    }

    public static new void AddMoves(PieceType type)
    {
      // Forward diagonal moves without capture
      type.StepMoveOnly(new Direction(1, 1));
      type.StepMoveOnly(new Direction(1, -1));
    }

    public override void Initialize(Game game)
    {
      base.Initialize(game);

      // Add custom move generator for multi-captures
      CustomMoveGenerator = GenerateMultiCaptureMoves;
    }

    public void SetPromotionTypes(List<PieceType> availablePromotionTypes)
    {
      PromotionTypes = availablePromotionTypes;
    }

    private bool GenerateMultiCaptureMoves(PieceType pieceType, Piece piece, MoveList moveList, bool capturesOnly)
    {
      if (piece.Square < 0) return false;

      // Start positions for potential captures
      GenerateJumpCaptures(piece.Square, piece.Square, piece.Player, moveList, new List<int>());
      return true;
    }

    private void GenerateJumpCaptures(int startSquare, int currentSquare, int player, MoveList moveList, 
                                    List<int> jumpedSquares)
    {
      int[] directions = new int[] { 
        PredefinedDirections.NE, 
        PredefinedDirections.NW
      };

      foreach (var dir in directions)
      {
        int jumpOver = Game.Board.NextSquare(player, dir, currentSquare);
        if (jumpOver < 0) continue;

        // Get the landing square, allowing for wrapping
        int landingSquare = GetWrappingLandingSquare(jumpOver, dir, player);
        if (landingSquare < 0) continue;

        Piece capturedPiece = Game.Board[jumpOver];
        Piece landingPiece = Game.Board[landingSquare];
        if (capturedPiece != null && capturedPiece.Player != player && landingPiece == null)
        {
          var nextJumps = new List<int>(jumpedSquares) { jumpOver };

          int rank = Game.Board.GetRank(landingSquare);
          MoveType moveType;
          if (rank == 0 || rank == 7)
          {
            moveType = MoveType.ExtraCapture | MoveType.PromotionProperty;
            foreach (PieceType promoteTo in PromotionTypes)
            {
              ListForSkipCapture(startSquare, moveList, landingSquare, nextJumps, promoteTo, moveType);
            }
          }
          else
          {
            moveType = MoveType.ExtraCapture;
            ListForSkipCapture(startSquare, moveList, landingSquare, nextJumps, null, moveType);

            // Recursively look for additional captures from the landing square
            int enemyPawnRank = 6 - (player * 5);
            if (nextJumps.Count < 3 && rank != enemyPawnRank)
              GenerateJumpCaptures(startSquare, landingSquare, player, moveList, nextJumps);
          }
        }
      }
    }

    private int GetWrappingLandingSquare(int jumpOverSquare, int direction, int player)
    {
      // Get the standard next square first
      int standardLanding = Game.Board.NextSquare(player, direction, jumpOverSquare);
      
      // If it's valid, return it
      if (standardLanding >= 0) return standardLanding;

      // If we're jumping over a piece on file 0 or NUM_FILES-1, we need to wrap
      int jumpOverFile = Game.Board.GetFile(jumpOverSquare);
      if (jumpOverFile == 0 || jumpOverFile == Game.Board.NumFiles - 1)
      {
        // Calculate the wrapped file
        int jumpOverRank = Game.Board.GetRank(jumpOverSquare);
        int directionOffset = direction == PredefinedDirections.NE ? 1 : -1;
        int wrappedFile = (jumpOverFile + directionOffset + Game.Board.NumFiles) % Game.Board.NumFiles;
        
        // Calculate the wrapped rank based on player and direction
        int wrappedRank = jumpOverRank + (player == 0 ? 1 : -1);
        
        // If the wrapped rank is valid, return the wrapped square
        if (wrappedRank >= 0 && wrappedRank < Game.Board.NumRanks)
        {
          return Game.Board.LocationToSquare(new Location(wrappedRank, wrappedFile));
        }
      }
      
      return -1;
    }

    private void ListForSkipCapture(int startSquare, MoveList moveList, int landingSquare, List<int> nextJumps, PieceType promoteTo, MoveType moveType)
    {
      // Create move for this capture chain
      moveList.BeginMoveAdd(moveType, startSquare, landingSquare, nextJumps.Count > 0 ? nextJumps[0] : 0);

      // Pick up the moving piece
      Piece pickedPiece = moveList.AddPickup(startSquare);

      // Pick up all captured pieces in the chain
      foreach (int square in nextJumps)
        moveList.AddPickup(square);

      if (moveType.HasFlag(MoveType.PromotionProperty))
        moveList.AddDrop(pickedPiece, landingSquare, promoteTo);
      else
        moveList.AddDrop(pickedPiece, landingSquare);

      // Evaluation increases with number of captures
      int materialGain = nextJumps.Sum(sq => Game.Board[sq].PieceType.MidgameValue);
      moveList.EndMoveAdd(3000 + materialGain + (nextJumps.Count * 500));
    }
  }
}
