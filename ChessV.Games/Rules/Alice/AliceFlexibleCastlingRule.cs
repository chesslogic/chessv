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
  public class AliceFlexibleCastlingRule : FlexibleCastlingRule
  {
    // *** CONSTRUCTION *** //

    public AliceFlexibleCastlingRule() { }
    public AliceFlexibleCastlingRule(CastlingRule templateRule) : base(templateRule) { }


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
            if (castlingMoves[Game.CurrentSide, x].KingFromSquare < castlingMoves[Game.CurrentSide, x].KingToSquare)
            {
              bool squaresEmpty = true;
              for (int file = Board.GetFile(castlingMoves[Game.CurrentSide, x].KingFromSquare) + 1;
                squaresEmpty && (file <= Board.GetFile(castlingMoves[Game.CurrentSide, x].KingToSquare) ||
                file <= Board.GetFile(castlingMoves[Game.CurrentSide, x].OtherFromSquare)); file++)
              {
                int sq = file * Board.NumRanks + Board.GetRank(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                if (sq != castlingMoves[Game.CurrentSide, x].OtherFromSquare && Board[sq] != null)
                  squaresEmpty = false;
              }
              if (squaresEmpty)
              {
                bool squaresAttacked = false;
                for (int file = Board.GetFile(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                  !squaresAttacked && file <= Board.GetFile(castlingMoves[Game.CurrentSide, x].KingToSquare); file++)
                {
                  int sq = file * Board.NumRanks + Board.GetRank(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                  if (Game.IsSquareAttacked(sq, Game.CurrentSide ^ 1))
                    squaresAttacked = true;
                }
                if (!squaresAttacked)
                {
                  if (castlingMoves[Game.CurrentSide, x].KingToSquare < nSquaresFirstBoard)
                  {
                    //	moving from first board to second board
                    if (Board[castlingMoves[Game.CurrentSide, x].KingToSquare + nSquaresFirstBoard] == null &&
                      Board[castlingMoves[Game.CurrentSide, x].KingToSquare - Board.NumRanks + nSquaresFirstBoard] == null)
                    {
                      list.BeginMoveAdd(MoveType.Castling, castlingMoves[Game.CurrentSide, x].KingFromSquare,
                        castlingMoves[Game.CurrentSide, x].KingToSquare + nSquaresFirstBoard);
                      Piece king = list.AddPickup(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                      Piece other = list.AddPickup(castlingMoves[Game.CurrentSide, x].OtherFromSquare);
                      list.AddDrop(king, castlingMoves[Game.CurrentSide, x].KingToSquare + nSquaresFirstBoard, null);
                      list.AddDrop(other, castlingMoves[Game.CurrentSide, x].KingToSquare - Board.NumRanks + nSquaresFirstBoard, null);
                      list.EndMoveAdd(1000);
                    }
                  }
                  else
                  {
                    //	moving from second board to first board
                    if (Board[castlingMoves[Game.CurrentSide, x].KingToSquare - nSquaresFirstBoard] == null &&
                      Board[castlingMoves[Game.CurrentSide, x].OtherToSquare - nSquaresFirstBoard] == null)
                    {
                      list.BeginMoveAdd(MoveType.Castling, castlingMoves[Game.CurrentSide, x].KingFromSquare,
                        castlingMoves[Game.CurrentSide, x].KingToSquare - nSquaresFirstBoard);
                      Piece king = list.AddPickup(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                      Piece other = list.AddPickup(castlingMoves[Game.CurrentSide, x].OtherFromSquare);
                      list.AddDrop(king, castlingMoves[Game.CurrentSide, x].KingToSquare - nSquaresFirstBoard, null);
                      list.AddDrop(other, castlingMoves[Game.CurrentSide, x].KingToSquare - Board.NumRanks - nSquaresFirstBoard, null);
                      list.EndMoveAdd(1000);
                    }
                  }

                  for (int file = Board.GetFile(castlingMoves[Game.CurrentSide, x].KingToSquare) + 1;
                    !squaresAttacked && file < Board.GetFile(castlingMoves[Game.CurrentSide, x].OtherFromSquare); file++)
                  {
                    int sq = file * Board.NumRanks + Board.GetRank(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                    squaresAttacked = Game.IsSquareAttacked(sq, Game.CurrentSide ^ 1);
                    if (!squaresAttacked)
                    {
                      if (castlingMoves[Game.CurrentSide, x].KingToSquare < nSquaresFirstBoard)
                      {
                        //	moving from first board to second board
                        if (Board[sq + nSquaresFirstBoard] == null &&
                          Board[sq - Board.NumRanks + nSquaresFirstBoard] == null)
                        {
                          list.BeginMoveAdd(MoveType.Castling, castlingMoves[Game.CurrentSide, x].KingFromSquare, sq + nSquaresFirstBoard);
                          Piece king = list.AddPickup(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                          Piece other = list.AddPickup(castlingMoves[Game.CurrentSide, x].OtherFromSquare);
                          list.AddDrop(king, sq + nSquaresFirstBoard, null);
                          list.AddDrop(other, sq - Board.NumRanks + nSquaresFirstBoard, null);
                          list.EndMoveAdd(1000);
                        }
                      }
                      else
                      {
                        //	moving from second board to first board
                        if (Board[sq - nSquaresFirstBoard] == null &&
                          Board[sq - Board.NumRanks - nSquaresFirstBoard] == null)
                        {
                          list.BeginMoveAdd(MoveType.Castling, castlingMoves[Game.CurrentSide, x].KingFromSquare, sq - nSquaresFirstBoard);
                          Piece king = list.AddPickup(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                          Piece other = list.AddPickup(castlingMoves[Game.CurrentSide, x].OtherFromSquare);
                          list.AddDrop(king, sq - nSquaresFirstBoard, null);
                          list.AddDrop(other, sq - Board.NumRanks - nSquaresFirstBoard, null);
                          list.EndMoveAdd(1000);
                        }
                      }
                    }
                  }
                }
              }
            }
            else
            {
              bool squaresEmpty = true;
              for (int file = Board.GetFile(castlingMoves[Game.CurrentSide, x].KingFromSquare) - 1;
                squaresEmpty && (file >= Board.GetFile(castlingMoves[Game.CurrentSide, x].KingToSquare) ||
                file >= Board.GetFile(castlingMoves[Game.CurrentSide, x].OtherFromSquare)); file--)
              {
                int sq = file * Board.NumRanks + Board.GetRank(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                if (sq != castlingMoves[Game.CurrentSide, x].OtherFromSquare && Board[sq] != null)
                  squaresEmpty = false;
              }
              if (squaresEmpty)
              {
                bool squaresAttacked = false;
                for (int file = Board.GetFile(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                  !squaresAttacked && file >= Board.GetFile(castlingMoves[Game.CurrentSide, x].KingToSquare); file--)
                {
                  int sq = file * Board.NumRanks + Board.GetRank(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                  if (Game.IsSquareAttacked(sq, Game.CurrentSide ^ 1))
                    squaresAttacked = true;
                }
                if (!squaresAttacked)
                {
                  if (castlingMoves[Game.CurrentSide, x].KingToSquare < nSquaresFirstBoard)
                  {
                    //	moving from first board to second board
                    if (Board[castlingMoves[Game.CurrentSide, x].KingToSquare + nSquaresFirstBoard] == null &&
                      Board[castlingMoves[Game.CurrentSide, x].KingToSquare + Board.NumRanks + nSquaresFirstBoard] == null)
                    {
                      list.BeginMoveAdd(MoveType.Castling, castlingMoves[Game.CurrentSide, x].KingFromSquare,
                        castlingMoves[Game.CurrentSide, x].KingToSquare + nSquaresFirstBoard);
                      Piece king = list.AddPickup(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                      Piece other = list.AddPickup(castlingMoves[Game.CurrentSide, x].OtherFromSquare);
                      list.AddDrop(king, castlingMoves[Game.CurrentSide, x].KingToSquare + nSquaresFirstBoard, null);
                      list.AddDrop(other, castlingMoves[Game.CurrentSide, x].KingToSquare + Board.NumRanks + nSquaresFirstBoard, null);
                      list.EndMoveAdd(1000);
                    }
                  }
                  else
                  {
                    //	moving from second board to first board
                    if (Board[castlingMoves[Game.CurrentSide, x].KingToSquare - nSquaresFirstBoard] == null &&
                      Board[castlingMoves[Game.CurrentSide, x].KingToSquare + Board.NumRanks - nSquaresFirstBoard] == null)
                    {
                      list.BeginMoveAdd(MoveType.Castling, castlingMoves[Game.CurrentSide, x].KingFromSquare,
                        castlingMoves[Game.CurrentSide, x].KingToSquare + nSquaresFirstBoard);
                      Piece king = list.AddPickup(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                      Piece other = list.AddPickup(castlingMoves[Game.CurrentSide, x].OtherFromSquare);
                      list.AddDrop(king, castlingMoves[Game.CurrentSide, x].KingToSquare - nSquaresFirstBoard, null);
                      list.AddDrop(other, castlingMoves[Game.CurrentSide, x].KingToSquare + Board.NumRanks - nSquaresFirstBoard, null);
                      list.EndMoveAdd(1000);
                    }
                  }

                  for (int file = Board.GetFile(castlingMoves[Game.CurrentSide, x].KingFromSquare) - 1;
                    !squaresAttacked && file > Board.GetFile(castlingMoves[Game.CurrentSide, x].OtherFromSquare); file--)
                  {
                    int sq = file * Board.NumRanks + Board.GetRank(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                    squaresAttacked = Game.IsSquareAttacked(sq, Game.CurrentSide ^ 1);
                    if (!squaresAttacked)
                    {
                      if (castlingMoves[Game.CurrentSide, x].KingToSquare < nSquaresFirstBoard)
                      {
                        //	moving from first board to second board
                        if (Board[sq + nSquaresFirstBoard] == null &&
                          Board[sq + Board.NumRanks + nSquaresFirstBoard] == null)
                        {
                          list.BeginMoveAdd(MoveType.Castling, castlingMoves[Game.CurrentSide, x].KingFromSquare, sq + nSquaresFirstBoard);
                          Piece king = list.AddPickup(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                          Piece other = list.AddPickup(castlingMoves[Game.CurrentSide, x].OtherFromSquare);
                          list.AddDrop(king, sq + nSquaresFirstBoard, null);
                          list.AddDrop(other, sq + Board.NumRanks + nSquaresFirstBoard, null);
                          list.EndMoveAdd(1000);
                        }
                      }
                      else
                      {
                        //	moving from second board to first board
                        if (Board[sq - nSquaresFirstBoard] == null &&
                          Board[sq + Board.NumRanks - nSquaresFirstBoard] == null)
                        {
                          list.BeginMoveAdd(MoveType.Castling, castlingMoves[Game.CurrentSide, x].KingFromSquare, sq - nSquaresFirstBoard);
                          Piece king = list.AddPickup(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                          Piece other = list.AddPickup(castlingMoves[Game.CurrentSide, x].OtherFromSquare);
                          list.AddDrop(king, sq - nSquaresFirstBoard, null);
                          list.AddDrop(other, sq + Board.NumRanks - nSquaresFirstBoard, null);
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
      }
    }
  }
}
