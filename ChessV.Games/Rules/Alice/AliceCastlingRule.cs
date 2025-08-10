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
  public class AliceCastlingRule : CastlingRule
  {
    // *** CONSTRUCTION *** //

    public AliceCastlingRule() { }
    public AliceCastlingRule(CastlingRule templateRule) : base(templateRule) { }


    // *** OVERRIDES *** //

    public override void GenerateSpecialMoves(MoveList list, bool capturesOnly, int ply)
    {
      if (!capturesOnly)
      {
        int nSquaresFirstBoard = Board.NumSquares / 2;
        int castlingPriv = ply == 1 ? gameHistoryPrivs[Game.GameMoveNumber] : searchStackPrivs[ply - 1];
        for (int x = 0; x < nCastlingMoves[Game.CurrentSide]; x++)
        {
          if ((castlingMoves[Game.CurrentSide, x].RequiredPriv & castlingPriv) != 0)
          {
            //	make sure final landing squares on the other board are free
            if (castlingMoves[Game.CurrentSide, x].KingFromSquare < nSquaresFirstBoard)
            {
              //	going from first board to second
              if (Board[castlingMoves[Game.CurrentSide, x].KingToSquare + nSquaresFirstBoard] != null ||
                Board[castlingMoves[Game.CurrentSide, x].OtherToSquare + nSquaresFirstBoard] != null)
                //	occupied - castling not possible
                continue;
            }
            else
            {
              //	going from second board to first
              if (Board[castlingMoves[Game.CurrentSide, x].KingToSquare - nSquaresFirstBoard] != null ||
                Board[castlingMoves[Game.CurrentSide, x].OtherToSquare - nSquaresFirstBoard] != null)
                //	occupied - castling not possible
                continue;
            }
            //	find the left-most square that needs to be free on the board castling FROM
            int minSquare = castlingMoves[Game.CurrentSide, x].KingFromSquare;
            minSquare = castlingMoves[Game.CurrentSide, x].KingToSquare < minSquare ? castlingMoves[Game.CurrentSide, x].KingToSquare : minSquare;
            minSquare = castlingMoves[Game.CurrentSide, x].OtherFromSquare < minSquare ? castlingMoves[Game.CurrentSide, x].OtherFromSquare : minSquare;
            minSquare = castlingMoves[Game.CurrentSide, x].OtherToSquare < minSquare ? castlingMoves[Game.CurrentSide, x].OtherToSquare : minSquare;
            //	find the right-most square that needs to be free on the board castling FROM
            int maxSquare = castlingMoves[Game.CurrentSide, x].KingFromSquare;
            maxSquare = castlingMoves[Game.CurrentSide, x].KingToSquare > maxSquare ? castlingMoves[Game.CurrentSide, x].KingToSquare : maxSquare;
            maxSquare = castlingMoves[Game.CurrentSide, x].OtherFromSquare > maxSquare ? castlingMoves[Game.CurrentSide, x].OtherFromSquare : maxSquare;
            maxSquare = castlingMoves[Game.CurrentSide, x].OtherToSquare > maxSquare ? castlingMoves[Game.CurrentSide, x].OtherToSquare : maxSquare;
            //	make sure the squares are empty
            bool squaresEmpty = true;
            for (int file = Board.GetFile(minSquare); squaresEmpty && file <= Board.GetFile(maxSquare); file++)
            {
              int sq = file * Board.NumRanks + Board.GetRank(minSquare);
              if (sq != castlingMoves[Game.CurrentSide, x].KingFromSquare &&
                sq != castlingMoves[Game.CurrentSide, x].OtherFromSquare &&
                Board[sq] != null)
                //	the path is blocked by a piece other than those involved in castling
                squaresEmpty = false;
            }
            if (squaresEmpty)
            {
              //	make sure squares the King is passes through are not attacked
              bool squaresAttacked = false;
              if (castlingMoves[Game.CurrentSide, x].KingFromSquare < castlingMoves[Game.CurrentSide, x].KingToSquare)
              {
                for (int file = Board.GetFile(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                  !squaresAttacked && file <= Board.GetFile(castlingMoves[Game.CurrentSide, x].KingToSquare); file++)
                {
                  int sq = file * Board.NumRanks + Board.GetRank(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                  if (Game.IsSquareAttacked(sq, Game.CurrentSide ^ 1))
                    squaresAttacked = true;
                }
              }
              else
              {
                for (int file = Board.GetFile(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                  !squaresAttacked && file >= Board.GetFile(castlingMoves[Game.CurrentSide, x].KingToSquare); file--)
                {
                  int sq = file * Board.NumRanks + Board.GetRank(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                  if (Game.IsSquareAttacked(sq, Game.CurrentSide ^ 1))
                    squaresAttacked = true;
                }
              }

              if (!squaresAttacked)
              {
                //	required squares are empty and not attacked so the move is legal - add it
                if (castlingMoves[Game.CurrentSide, x].KingFromSquare < nSquaresFirstBoard)
                {
                  //	going from first board to second
                  list.BeginMoveAdd(MoveType.Castling, castlingMoves[Game.CurrentSide, x].KingFromSquare,
                    castlingMoves[Game.CurrentSide, x].KingToSquare + nSquaresFirstBoard);
                  Piece king = list.AddPickup(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                  Piece other = list.AddPickup(castlingMoves[Game.CurrentSide, x].OtherFromSquare);
                  list.AddDrop(king, castlingMoves[Game.CurrentSide, x].KingToSquare + nSquaresFirstBoard, null);
                  list.AddDrop(other, castlingMoves[Game.CurrentSide, x].OtherToSquare + nSquaresFirstBoard, null);
                  list.EndMoveAdd(1000);
                }
                else
                {
                  //	going from second board to first
                  list.BeginMoveAdd(MoveType.Castling, castlingMoves[Game.CurrentSide, x].KingFromSquare,
                    castlingMoves[Game.CurrentSide, x].KingToSquare - nSquaresFirstBoard);
                  Piece king = list.AddPickup(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                  Piece other = list.AddPickup(castlingMoves[Game.CurrentSide, x].OtherFromSquare);
                  list.AddDrop(king, castlingMoves[Game.CurrentSide, x].KingToSquare - nSquaresFirstBoard, null);
                  list.AddDrop(other, castlingMoves[Game.CurrentSide, x].OtherToSquare - nSquaresFirstBoard, null);
                  list.EndMoveAdd(1000);
                }
              }
            }
          }
        }
      }
    }
  }
}
