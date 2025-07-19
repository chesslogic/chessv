using System.Collections.Generic;
using System.Linq;

namespace ChessV.Games.Pieces.Apmw
{
  [PieceType("Checkers", "APMW Custom Pieces")]
  public class Checkers : PieceType

  {
    public List<PieceType> PromotionTypes { get; private set; }
    private const int MAX_CAPTURE_DEPTH = 8; // Limit recursive capture depth

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
      if (piece.Square < 0 || piece.Square >= Game.Board.NumSquaresExtended) return false;

      // Check if we're approaching move list limits - be more conservative for Checkers
      if (moveList.Count > MoveList.MAX_MOVES - 200)
      {
        // Stop generating moves if we're close to the limit
        return true;
      }

      int initialMoveCount = moveList.Count;
      
      // Start positions for potential captures
      GenerateJumpCaptures(piece.Square, piece.Square, piece.Player, moveList, new List<int>(), 0);
      
      // Log if we generated an excessive number of moves
      int movesGenerated = moveList.Count - initialMoveCount;
      if (movesGenerated > 50)
      {
        // We generated too many moves, this might be contributing to crashes
        // Consider this a sign that the position is too complex for safe analysis
        return true;
      }
      
      return true;
    }

    private void GenerateJumpCaptures(int startSquare, int currentSquare, int player, MoveList moveList, 
                                    List<int> jumpedSquares, int depth)
    {
      // Prevent infinite recursion and excessive move generation
      if (depth >= MAX_CAPTURE_DEPTH || jumpedSquares.Count >= MAX_CAPTURE_DEPTH)
        return;

      // Check move list capacity
      if (moveList.Count > MoveList.MAX_MOVES - 50)
        return;

      // Validate current square
      if (currentSquare < 0 || currentSquare >= Game.Board.NumSquares)
        return;

      int[] directions = new int[] { 
        PredefinedDirections.NE, 
        PredefinedDirections.NW
      };

      foreach (var dir in directions)
      {
        int jumpOver = Game.Board.NextSquare(player, dir, currentSquare);
        if (jumpOver < 0 || jumpOver >= Game.Board.NumSquares) continue;

        // Skip if we've already jumped over this square in this sequence
        if (jumpedSquares.Contains(jumpOver)) continue;

        // Get the landing square, allowing for wrapping
        int landingSquare = GetWrappingLandingSquare(jumpOver, dir, player);
        if (landingSquare < 0 || landingSquare >= Game.Board.NumSquares) continue;

        Piece capturedPiece = Game.Board[jumpOver];
        Piece landingPiece = Game.Board[landingSquare];
        
        // Validate the capture
        if (capturedPiece != null && capturedPiece.Player != player && landingPiece == null)
        {
          var nextJumps = new List<int>(jumpedSquares) { jumpOver };

          int rank = Game.Board.GetRank(landingSquare);
          MoveType moveType;
          if (rank == 0 || rank == Game.Board.NumRanks - 1)
          {
            moveType = MoveType.ExtraCapture | MoveType.PromotionProperty;
            if (PromotionTypes != null)
            {
              foreach (PieceType promoteTo in PromotionTypes)
              {
                ListForSkipCapture(startSquare, moveList, landingSquare, nextJumps, promoteTo, moveType);
              }
            }
          }
          else
          {
            moveType = MoveType.ExtraCapture;
            ListForSkipCapture(startSquare, moveList, landingSquare, nextJumps, null, moveType);

            // Recursively look for additional captures from the landing square
            int enemyPawnRank = 6 - (player * 5);
            if (nextJumps.Count < MAX_CAPTURE_DEPTH && rank != enemyPawnRank && depth < MAX_CAPTURE_DEPTH - 1)
              GenerateJumpCaptures(startSquare, landingSquare, player, moveList, nextJumps, depth + 1);
          }
        }
      }
    }

    private int GetWrappingLandingSquare(int jumpOverSquare, int direction, int player)
    {
      // Validate input square
      if (jumpOverSquare < 0 || jumpOverSquare >= Game.Board.NumSquares)
        return -1;

      // Get the standard next square first
      int standardLanding = Game.Board.NextSquare(player, direction, jumpOverSquare);
      
      // If it's valid, return it
      if (standardLanding >= 0 && standardLanding < Game.Board.NumSquares) 
        return standardLanding;

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
        
        // Validate the wrapped coordinates
        if (wrappedRank >= 0 && wrappedRank < Game.Board.NumRanks && 
            wrappedFile >= 0 && wrappedFile < Game.Board.NumFiles)
        {
          int wrappedSquare = Game.Board.LocationToSquare(new Location(wrappedRank, wrappedFile));
          // Double-check the wrapped square is valid
          if (wrappedSquare >= 0 && wrappedSquare < Game.Board.NumSquares)
            return wrappedSquare;
        }
      }
      
      return -1;
    }

    private void ListForSkipCapture(int startSquare, MoveList moveList, int landingSquare, List<int> nextJumps, PieceType promoteTo, MoveType moveType)
    {
      // Validate squares before creating the move
      if (startSquare < 0 || startSquare >= Game.Board.NumSquares ||
          landingSquare < 0 || landingSquare >= Game.Board.NumSquares ||
          nextJumps.Any(sq => sq < 0 || sq >= Game.Board.NumSquares))
        return;

      // Additional validation: ensure all jumped squares actually contain capturable pieces
      foreach (int square in nextJumps)
      {
        Piece piece = Game.Board[square];
        if (piece == null || piece.Player == Game.Board[startSquare].Player)
          return; // Invalid capture sequence
      }

      // Check move list capacity before adding
      if (moveList.Count >= MoveList.MAX_MOVES - 10)
        return;

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
      int materialGain = nextJumps.Sum(sq => {
        Piece capturedPiece = Game.Board[sq];
        return capturedPiece?.PieceType?.MidgameValue ?? 0;
      });
      moveList.EndMoveAdd(3000 + materialGain + (nextJumps.Count * 500));
    }
  }
}
