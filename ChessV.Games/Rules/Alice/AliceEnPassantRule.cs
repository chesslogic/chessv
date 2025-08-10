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
  public class AliceEnPassantRule : EnPassantRule
  {
    // *** CONSTRUCTION *** //

    public AliceEnPassantRule(PieceType pawnType, int nDirection) : base(pawnType, nDirection) { }
    public AliceEnPassantRule(EnPassantRule templateRule) : base(templateRule) { }


    public override MoveEventResponse MoveBeingMade(MoveInfo move, int ply)
    {
      //	if the current side to move is also the next side to move then 
      //	we return here - en passant is not possible so we would not 
      //	want to set the ep square
      if (Game.CurrentSide == Game.NextSide)
        return MoveEventResponse.NotHandled;
      epSquares[ply] = 0;
      if (ply == 1)
        gameHistory[Game.GameMoveNumber] = 0;
      if (move.PieceMoved != null && move.PieceMoved.PieceType == PawnType)
      {
        int nSquaresFirstBoard = Board.NumSquares / 2;
        int virtualFromSquare =
            move.FromSquare > nSquaresFirstBoard
          ? move.FromSquare - nSquaresFirstBoard
          : move.FromSquare + nSquaresFirstBoard;
        if (Board.GetDistance(virtualFromSquare, move.ToSquare) > 1)
        {
          //	check to see if there are any pawn attackers - even if a pawn makes a 
          //	multi-step move, we don't set the e.p. square unless there is a pawn 
          //	that can actually take it.  this is an important consideration because of 
          //	the hashing/matching of board positions.  we don't want an identical board 
          //	position to be considered different just because a pawn made a multi-step 
          //	move - the fact that a multi-step move was made is only significant because 
          //	of the availability of an e.p. capture
          int epsquare = Board.NextSquare(Game.PlayerDirection(move.Player, NDirection), virtualFromSquare);

          //	we loop here to accomodate large-board games where a pawn can make a move 
          //	of more than two steps and still be captured e.p. on any square passed over
          while (Board[epsquare] == null)
          {
            for (int ndir = 0; ndir < nAttackDirections; ndir++)
            {
              int nextSquare = Board.NextSquare(attackDirections[move.Player, ndir], epsquare);
              if (nextSquare >= 0)
              {
                Piece piece = Board[nextSquare];
                if (piece != null && piece.PieceType == PawnType && piece.Player != move.Player)
                {
                  //	en passant capture is possible
                  epSquares[ply] = epsquare;
                  return MoveEventResponse.MoveOk;
                }
              }
            }
            epsquare = Board.NextSquare(Game.PlayerDirection(move.Player, NDirection), epsquare);
          }
        }
      }
      return MoveEventResponse.NotHandled;
    }

    public override void GenerateSpecialMoves(MoveList list, bool capturesOnly, int ply)
    {
      int epSquare = ply == 1 ? gameHistory[Game.GameMoveNumber] : epSquares[ply - 1];
      if (epSquare > 0)
      {
        int nd = Game.PlayerDirection(Game.CurrentSide ^ 1, NDirection);
        int sq = Board.NextSquare(nd, epSquare);
        Piece pawn = Board[sq];
        for (int ndir = 0; ndir < nAttackDirections; ndir++)
        {
          int nextSquare = Board.NextSquare(attackDirections[Game.CurrentSide ^ 1, ndir], epSquare);
          if (nextSquare >= 0)
          {
            Piece piece = Board[nextSquare];
            if (piece != null && piece.PieceType == PawnType && piece.Player == Game.CurrentSide)
            {
              //	find square of pawn being captured.  we may have to make several 
              //	steps for large-board games where pawns make more than two steps 
              //	and can still be captured e.p.
              int captureSquare = Board.NextSquare(nd, epSquare);
              while (Board[captureSquare] == null)
                captureSquare = Board.NextSquare(nd, captureSquare);

              //	this piece can capture en passant 
              int nSquaresFirstBoard = Board.NumSquares / 2;
              if (epSquare > nSquaresFirstBoard)
              {
                list.BeginMoveAdd(MoveType.EnPassant, nextSquare, epSquare - nSquaresFirstBoard);
                list.AddPickup(nextSquare);
                list.AddPickup(captureSquare);
                list.AddDrop(piece, epSquare - nSquaresFirstBoard, null);
                list.EndMoveAdd(120);
              }
              else
              {
                list.BeginMoveAdd(MoveType.EnPassant, nextSquare, epSquare + nSquaresFirstBoard);
                list.AddPickup(nextSquare);
                list.AddPickup(captureSquare);
                list.AddDrop(piece, epSquare + nSquaresFirstBoard, null);
                list.EndMoveAdd(120);
              }
            }
          }
        }
      }
    }
  }
}
