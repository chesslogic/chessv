using System.Collections.Generic;

namespace ChessV.Games.Rules.Apmw
{
  public class ApmwFirstTurnRule : Rule
  {
    private CheckmateRule CheckmateRule;
    private int hashKeyIndex;

    public override void Initialize(Game game)
    {
      base.Initialize(game);
      CheckmateRule = game.FindRule(typeof(CheckmateRule), true) as CheckmateRule;
    }

    public override void GenerateSpecialMoves(MoveList list, bool capturesOnly, int ply)
    {
      // Only apply this rule in the first two turns (ply <= 3)
      if (Game.GameTurnNumber > 2)
        return;

      // Get the current player's king square
      int kingSquare = -1;
      BitBoard royalPieces = Board.GetPieceTypeBitboard(Game.CurrentSide, CheckmateRule.RoyalPieceTypes[0].TypeNumber);
      if (royalPieces.BitCount > 0)
        kingSquare = royalPieces.LSB;
      else
        return;

      // If we're not in check, no need to generate counter captures
      if (!Game.IsSquareAttacked(kingSquare, Game.CurrentSide ^ 1))
        return;

      // Find all attacking pieces
      HashSet<int> attackingSquares = new HashSet<int>();
      for (int square = 0; square < Board.NumSquares; square++)
      {
        Piece piece = Board[square];
        if (piece == null || piece.Player != (Game.CurrentSide ^ 1))
          continue;

        // Check if this piece is attacking the king by generating its moves
        MoveCapability[] moves;
        int nMoves = piece.PieceType.GetMoveCapabilities(out moves);
        for (int nMove = 0; nMove < nMoves; nMove++)
        {
          MoveCapability move = moves[nMove];
          if (!move.CanCapture)
            continue;

          int step = 1;
          int nextSquare = Board.NextSquare(piece.Player, move.NDirection, piece.Square);
          while (nextSquare >= 0 && step <= move.MaxSteps)
          {
            if (nextSquare == kingSquare)
            {
              attackingSquares.Add(piece.Square);
              break;
            }
            else if (Board[nextSquare] != null)
              break;

            nextSquare = Board.NextSquare(piece.Player, move.NDirection, nextSquare);
            step++;
          }
        }
      }

      // For each attacking piece, check if it can be captured normally by any of our pieces
      foreach (int attackingSquare in attackingSquares)
      {
        bool canBeCapturedNormally = false;
        for (int fromSquare = 0; fromSquare < Board.NumSquares && !canBeCapturedNormally; fromSquare++)
        {
          Piece piece = Board[fromSquare];
          if (piece == null || piece.Player != Game.CurrentSide)
            continue;

          // This is the direction we need to move to reach the attacker, which has to exist for the engine to allow this move
          int attackDirection = Board.DirectionLookup(fromSquare, attackingSquare);
          if (attackDirection < 0)
            continue;

          MoveCapability[] moves;
          int nMoves = piece.PieceType.GetMoveCapabilities(out moves);
          for (int nMove = 0; nMove < nMoves; nMove++)
          {
            MoveCapability move = moves[nMove];
            if (!move.CanCapture)
              continue;

            // This is the direction the piece would move according to its capability
            int moveDirection = Game.PlayerDirection(piece.Player, move.NDirection);
            
            // Check if this move can actually reach the attacking piece
            int nextSquare = Board.NextSquare(moveDirection, fromSquare);
            int step = 1;
            bool pathClear = true;

            // Follow the path of the move
            while (nextSquare >= 0 && step <= move.MaxSteps)
            {
              if (nextSquare == attackingSquare)
              {
                // Found a valid capture path
                if (pathClear && step >= move.MinSteps)
                {
                  canBeCapturedNormally = true;
                  break;
                }
              }
              else if (Board[nextSquare] != null)
              {
                pathClear = false;
                break;
              }
              nextSquare = Board.NextSquare(moveDirection, nextSquare);
              step++;
            }

            if (canBeCapturedNormally)
              break;
          }
        }

        // If this attacker can't be captured normally, generate special moves for all our pieces
        if (!canBeCapturedNormally)
        {
          for (int fromSquare = 0; fromSquare < Board.NumSquares; fromSquare++)
          {
            Piece piece = Board[fromSquare];
            if (piece == null || piece.Player != Game.CurrentSide)
              continue;

            // Validate that this is a legal direction for some piece type so the game doesn't crash
            int direction = Board.DirectionLookup(fromSquare, attackingSquare);
            if (direction < 0)
              continue;

            // Add a special capture move
            int capturedPieceValue = Board[attackingSquare].PieceType.MidgameValue;
            list.BeginMoveAdd(MoveType.BaroqueCapture, fromSquare, attackingSquare);
            Piece movingPiece = list.AddPickup(fromSquare);
            Piece capturedPiece = list.AddPickup(attackingSquare);
            list.AddDrop(movingPiece, attackingSquare);
            // Give these moves a high priority since they prevent check
            list.EndMoveAdd(1000 + capturedPieceValue);
          }
        }
      }
    }

    public override void GetNotesForPieceType(PieceType type, List<string> notes)
    {
      notes.Add("first turn counter capture");
    }
  }
}