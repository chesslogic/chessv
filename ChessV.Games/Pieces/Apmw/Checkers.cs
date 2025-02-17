using System.Collections.Generic;
using System.Linq;

namespace ChessV.Games.Pieces.Apmw
{
  [PieceType("Checkers", "Chess")]
  public class Checkers : PieceType

  {
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

    private bool GenerateMultiCaptureMoves(PieceType pieceType, Piece piece, MoveList moveList, bool capturesOnly)
    {
      if (piece.Square < 0) return false;

      if (!capturesOnly)
      {
        // Generate non-capture moves using the standard move generator
        return true;
      }

      // Start positions for potential captures
      GenerateJumpCaptures(piece.Square, piece.Square, piece.Player, moveList, new List<int>());
      return true;
    }

    private void GenerateJumpCaptures(int startSquare, int currentSquare, int player, MoveList moveList, 
                                    List<int> jumpedSquares)
    {
      int[] directions = new int[] { 
        PredefinedDirections.NE, 
        PredefinedDirections.NW,
        PredefinedDirections.SE,
        PredefinedDirections.SW
      };

      foreach (var dir in directions)
      {
        int jumpOver = Game.Board.NextSquare(player, dir, currentSquare);
        if (jumpOver < 0) continue;

        int landingSquare = Game.Board.NextSquare(player, dir, jumpOver);
        if (landingSquare < 0) continue;

        Piece capturedPiece = Game.Board[jumpOver];
        if (capturedPiece != null && capturedPiece.Player != player && Game.Board[landingSquare] == null)
        {
          var nextJumps = new List<int>(jumpedSquares) { jumpOver };

          // Create move for this capture
          moveList.BeginMoveAdd(MoveType.BaroqueCapture, startSquare, landingSquare);
          //var thisPiece = moveList.AddPickup(startSquare);
          
          // Add all captured pieces
          int lastSquare = startSquare;
          foreach (int square in nextJumps)
          {
            moveList.AddCapture(lastSquare, square);
            lastSquare = square;
            // moveList.AddPickup(square);
          }
          
          moveList.AddMove(lastSquare, landingSquare);
          
          // Evaluation increases with number of captures
          int materialGain = nextJumps.Sum(sq => Game.Board[sq].PieceType.MidgameValue);
          moveList.EndMoveAdd(3000 + materialGain + (nextJumps.Count * 500));

          // Recursively look for additional captures from the landing square
          if (nextJumps.Count < 3)
            GenerateJumpCaptures(startSquare, landingSquare, player, moveList, nextJumps);
        }
      }
    }
  }
}
