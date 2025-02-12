using System.Collections.Generic;

namespace ChessV.Games.Pieces.Apmw
{
  [PieceType("Checkers", "Chess")]
  public class Checkers : PieceType

  {
    public Checkers(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = "Wizard") :
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

      // Capture moves - these will be 2 squares in each direction
      MoveCapability captureNE = new MoveCapability();
      captureNE.MinSteps = 2;
      captureNE.MaxSteps = 2;
      captureNE.MustCapture = true;
      captureNE.CanCapture = true;
      captureNE.Direction = new Direction(1, 1);
      captureNE.SpecialAttacks = SpecialAttacks.CannonCapture;
      type.AddMoveCapability(captureNE);

      MoveCapability captureNW = new MoveCapability();
      captureNW.MinSteps = 2;
      captureNW.MaxSteps = 2;
      captureNW.MustCapture = true;
      captureNW.CanCapture = true;
      captureNW.Direction = new Direction(1, -1);
      captureNW.SpecialAttacks = SpecialAttacks.CannonCapture;
      type.AddMoveCapability(captureNW);
    }

    public override void Initialize(Game game)
    {
      base.Initialize(game);

      // Add custom move generator for multi-captures
      CustomMoveGenerator = GenerateMultiCaptureMoves;
    }

    private bool GenerateMultiCaptureMoves(PieceType pieceType, Piece piece, MoveList moveList, bool capturesOnly)
    {
      if (capturesOnly)
      {
        // Start positions for potential captures
        GenerateJumpCaptures(piece.Square, piece.Player, moveList, new HashSet<int>());
        return true;
      }
      else
      {
        // Generate non-capture moves
        int[] directions = new int[] {
        PredefinedDirections.NE,
        PredefinedDirections.NW
      };

        foreach (var dir in directions)
        {
          int nextSquare = Game.Board.NextSquare(Game.PlayerDirection(piece.Player, dir), piece.Square);
          if (nextSquare >= 0 && Game.Board[nextSquare] == null)
          {
            moveList.AddMove(piece.Square, nextSquare);
          }
        }
      }
      return false;
    }

    private void GenerateJumpCaptures(int fromSquare, int player, MoveList moveList, HashSet<int> visitedSquares)
    {
      visitedSquares.Add(fromSquare);
      int[] directions = new int[] { 
        PredefinedDirections.NE, 
        PredefinedDirections.NW
      };

      foreach (var dir in directions)
      {
        int jumpOver = Game.Board.NextSquare(Game.PlayerDirection(player, dir), fromSquare);
        if (jumpOver < 0) continue;

        int landingSquare = Game.Board.NextSquare(Game.PlayerDirection(player, dir), jumpOver);
        if (landingSquare < 0) continue;

        Piece capturedPiece = Game.Board[jumpOver];
        if (capturedPiece != null && capturedPiece.Player != player && 
          Game.Board[landingSquare] == null && !visitedSquares.Contains(landingSquare))
        {
          // Create move for this capture
          moveList.BeginMoveAdd(MoveType.ExtraCapture, fromSquare, landingSquare, jumpOver);
          moveList.AddPickup(fromSquare);  // Pick up the moving piece
          moveList.AddPickup(jumpOver);    // Pick up the captured piece
          moveList.AddDrop(Game.Board[fromSquare], landingSquare);  // Drop the moving piece
          moveList.EndMoveAdd(3000 + capturedPiece.PieceType.MidgameValue);  // Higher eval for captures

          // Recursively look for additional captures
          GenerateJumpCaptures(landingSquare, player, moveList, visitedSquares);
        }
      }

      visitedSquares.Remove(fromSquare);
    }
  }
}
