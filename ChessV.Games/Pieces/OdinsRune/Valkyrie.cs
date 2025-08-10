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
  public class Valkyrie : PieceType
  {
    public Valkyrie(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Valkyrie", name, notation, midgameValue, endgameValue, preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      Queen.AddMoves(type);
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
              moveList.AddCapture(piece.Square, nextSquare);
            else if (step >= move.MinSteps && pieceOnSquare.Player == piece.Player && piece.PieceType != pieceOnSquare.PieceType)
            {
              //	self-capture move allowing relocation of friendly piece
              int currentSquare = piece.Square;
              while (currentSquare != nextSquare)
              {
                moveList.BeginMoveAdd(MoveType.MoveRelay, piece.Square, nextSquare, currentSquare);
                moveList.AddPickup(piece.Square);
                moveList.AddPickup(nextSquare);
                moveList.AddDrop(piece, nextSquare);
                moveList.AddDrop(pieceOnSquare, currentSquare);
                moveList.EndMoveAdd(piece.PieceType.GetMidgamePST(nextSquare) - piece.PieceType.GetMidgamePST(piece.Square) +
                  pieceOnSquare.PieceType.GetMidgamePST(currentSquare) - pieceOnSquare.PieceType.GetMidgamePST(nextSquare));
                currentSquare = piece.Board.NextSquare(piece.Player, move.NDirection, currentSquare);
              }
            }
            nextSquare = -1;
          }
          else
          {
            if (step >= move.MinSteps && !move.MustCapture && !capturesOnly)
              moveList.AddMove(piece.Square, nextSquare);
            nextSquare = piece.Board.NextSquare(piece.Player, move.NDirection, nextSquare);
            step++;
          }
        }
      }
      return false;
    }
  }
}
