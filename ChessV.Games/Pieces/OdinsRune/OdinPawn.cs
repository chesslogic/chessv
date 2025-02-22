
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

using System.Collections.Generic;

namespace ChessV.Games.Pieces.OdinsRune
{
  public class OdinPawn : PieceType
  {
    public OdinPawn(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
      base("Odin Pawn", name, notation, midgameValue, endgameValue, preferredImageName)
    {
      AddMoves(this);
    }

    public static new void AddMoves(PieceType type)
    {
      Ferz.AddMoves(type);

      //	add the multi-path move
      MoveCapability move = MoveCapability.Step(new Direction(2, 0));
      MovePathInfo movePath = new MovePathInfo();
      movePath.AddPath(new List<Direction>() { new Direction(1, 1), new Direction(1, -1) });
      movePath.AddPath(new List<Direction>() { new Direction(1, -1), new Direction(1, 1) });
      move.PathInfo = movePath;
      type.AddMoveCapability(move);
    }
  }
}
