/***************************************************************************

                                 ChessV

                  COPYRIGHT (C) 2012-2017 BY GREG STRONG

This file is part of ChessV.  ChessV is free software; you can redistribute
it and/or modify it under the terms of the GNU General Public License as 
published by the Free Software Foundation, either version 3 of the License, 
or (at your option) any later version.

ChessV is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or 
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for 
more details; the file 'COPYING' contains the License text, but if for
some reason you need a copy, please visit <http://www.gnu.org/licenses/>.

****************************************************************************/

namespace ChessV.Games.Rules.Alice
{
  public class AliceRule : Rule
  {
    public PieceType RoyalType { get; set; }

    public override MoveEventResponse MoveBeingGenerated(MoveList moves, int from, int to, MoveType type)
    {
      int nSquaresOnFirstBoard = Board.NumSquares / 2;
      
      // Validate input parameters
      if (from < 0 || from >= Board.NumSquaresExtended || to < 0 || to >= Board.NumSquaresExtended)
      {
        return MoveEventResponse.NotHandled;
      }
      
      if (type == MoveType.StandardMove)
      {
        if (to >= nSquaresOnFirstBoard)
        {
          int targetSquare = to - nSquaresOnFirstBoard;
          // Validate target square is within bounds
          if (targetSquare >= 0 && targetSquare < Board.NumSquaresExtended && Board[targetSquare] == null)
            moves.AddMove(from, targetSquare, true);
        }
        else
        {
          int targetSquare = to + nSquaresOnFirstBoard;
          // Validate target square is within bounds
          if (targetSquare >= 0 && targetSquare < Board.NumSquaresExtended && Board[targetSquare] == null)
            moves.AddMove(from, targetSquare, true);
        }
        return MoveEventResponse.Handled;
      }
      else if (type == MoveType.StandardCapture)
      {
        if (to >= nSquaresOnFirstBoard)
        {
          int targetSquare = to - nSquaresOnFirstBoard;
          // Validate target square is within bounds
          if (targetSquare >= 0 && targetSquare < Board.NumSquaresExtended)
          {
            if (moves.BeginMoveAdd(MoveType.StandardCapture, from, targetSquare))
            {
              Piece pieceBeingMoved = moves.AddPickup(from);
              Piece pieceBeingCaptured = moves.AddPickup(to);
              if (pieceBeingMoved != null && pieceBeingCaptured != null)
              {
                moves.AddDrop(pieceBeingMoved, targetSquare);
                moves.EndMoveAdd(3000 +
                  pieceBeingCaptured.PieceType.MidgameValue -
                  (pieceBeingMoved.PieceType.MidgameValue / 16));
              }
            }
          }
        }
        else
        {
          int targetSquare = to + nSquaresOnFirstBoard;
          // Validate target square is within bounds
          if (targetSquare >= 0 && targetSquare < Board.NumSquaresExtended)
          {
            if (moves.BeginMoveAdd(MoveType.StandardCapture, from, targetSquare))
            {
              Piece pieceBeingMoved = moves.AddPickup(from);
              Piece pieceBeingCaptured = moves.AddPickup(to);
              if (pieceBeingMoved != null && pieceBeingCaptured != null)
              {
                moves.AddDrop(pieceBeingMoved, targetSquare);
                moves.EndMoveAdd(3000 +
                  pieceBeingCaptured.PieceType.MidgameValue -
                  (pieceBeingMoved.PieceType.MidgameValue / 16));
              }
            }
          }
        }
        return MoveEventResponse.Handled;
      }
      return MoveEventResponse.NotHandled;
    }

    public override MoveEventResponse MoveBeingMade(MoveInfo move, int ply)
    {
      if (move.PieceMoved != null && move.PieceMoved.PieceType == RoyalType)
      {
        //	Make sure that the king didn't move into check on the board 
        //	he was moving from.  Since the actual destination square is on 
        //	the other board, this wouldn't be detected automatically.
        int nSquaresOnFirstBoard = Board.NumSquares / 2;
        if (move.ToSquare >= nSquaresOnFirstBoard)
        {
          int checkSquare = move.ToSquare - nSquaresOnFirstBoard;
          if (checkSquare >= 0 && checkSquare < Board.NumSquaresExtended && 
              Game.IsSquareAttacked(checkSquare, move.Player ^ 1))
            return MoveEventResponse.IllegalMove;
        }
        else
        {
          int checkSquare = move.ToSquare + nSquaresOnFirstBoard;
          if (checkSquare >= 0 && checkSquare < Board.NumSquaresExtended && 
              Game.IsSquareAttacked(checkSquare, move.Player ^ 1))
            return MoveEventResponse.IllegalMove;
        }
      }

      return MoveEventResponse.NotHandled;
    }
  }
}
