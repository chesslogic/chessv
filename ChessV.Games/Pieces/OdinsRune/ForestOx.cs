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

namespace ChessV.Games.Pieces.OdinsRune
{
  public class ForestOx : PieceType
  {
    public ForestOx(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Forest Ox", name, notation, midgameValue, endgameValue, preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      Knight.AddMoves(type);
      type.CustomMoveGenerator = CustomMoveGenerationHandler;
    }

    public static bool CustomMoveGenerationHandler(PieceType pieceType, Piece piece, MoveList moveList, bool capturesOnly)
    {
      MoveCapability[] moves;
      int nMoves = pieceType.GetMoveCapabilities(out moves);
      for (int nMove = 0; nMove < nMoves; nMove++)
      {
        MoveCapability move = moves[nMove];
        int step = 1;
        int nextSquare = piece.Board.NextSquare(piece.Player, move.NDirection, piece.Square);
        while (nextSquare >= 0 && step <= move.MaxSteps)
        {
          Piece pieceOnSquare = piece.Board[nextSquare];
          if (pieceOnSquare != null)
          {
            if (step >= move.MinSteps && move.CanCapture && pieceOnSquare.Player != piece.Player)
            {
              moveList.AddCapture(piece.Square, nextSquare);
              for (int x = 0; x < 8; x++)
              {
                int targetSquare = piece.Board.NextSquare(x, nextSquare);
                if (targetSquare >= 0 && piece.Board[targetSquare] != null && piece.Board[targetSquare].Player != piece.Player)
                {
                  moveList.BeginMoveAdd(MoveType.ExtraCapture, piece.Square, nextSquare, targetSquare);
                  moveList.AddPickup(piece.Square);
                  moveList.AddPickup(nextSquare);
                  moveList.AddPickup(targetSquare);
                  moveList.AddDrop(piece, nextSquare);
                  moveList.EndMoveAdd(4000 + piece.Board[nextSquare].PieceType.MidgameValue + piece.Board[targetSquare].PieceType.MidgameValue);
                }
              }
            }
            nextSquare = -1;
          }
          else
          {
            if (step >= move.MinSteps && !move.MustCapture)
            {
              if (!capturesOnly)
                moveList.AddMove(piece.Square, nextSquare);
              for (int x = 0; x < 8; x++)
              {
                int targetSquare = piece.Board.NextSquare(x, nextSquare);
                if (targetSquare >= 0 && piece.Board[targetSquare] != null && piece.Board[targetSquare].Player != piece.Player)
                {
                  moveList.BeginMoveAdd(MoveType.ExtraCapture, piece.Square, nextSquare, targetSquare);
                  moveList.AddPickup(piece.Square);
                  moveList.AddPickup(targetSquare);
                  moveList.AddDrop(piece, nextSquare);
                  moveList.EndMoveAdd(3000 + piece.Board[targetSquare].PieceType.MidgameValue);
                }
              }
            }
            nextSquare = piece.Board.NextSquare(piece.Player, move.NDirection, nextSquare);
            step++;
          }
        }
      }

      return false;
    }
  }
}
