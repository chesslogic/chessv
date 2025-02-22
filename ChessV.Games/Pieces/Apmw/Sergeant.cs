/***************************************************************************

                                 ChessV

                  COPYRIGHT (C) 2012-2019 BY GREG STRONG

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

namespace ChessV.Games.Pieces.Apmw
{
    [PieceType("Sergeant", "Chess")]
    public class Sergeant : PieceType
    {
        public Sergeant(string name, string notation, int midgameValue, int endgameValue, string preferredImageName = null) :
            base("Sergeant", name, notation, midgameValue, endgameValue, preferredImageName)
        {
            IsPawn = true;
            IsSliced = false;
            AddMoves(this);

            // Customize piece-square-tables for the Sergeant
            PSTMidgameForwardness = 7;
            PSTEndgameForwardness = 10;
            PSTMidgameInSmallCenter = 6;
        }

        public static new void AddMoves(PieceType type)
        {
            // Regular Pawn moves
            type.StepMoveOnly(new Direction(1, 0));
            type.StepCaptureOnly(new Direction(1, 1));
            type.StepCaptureOnly(new Direction(1, -1));

            // Berolina Pawn moves
            type.StepMoveOnly(new Direction(1, 1));
            type.StepMoveOnly(new Direction(1, -1));
            type.StepCaptureOnly(new Direction(1, 0));
        }
    }
}
