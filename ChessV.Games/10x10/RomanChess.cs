
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

namespace ChessV.Games
{
	[Game("Roman Chess", typeof(Geometry.Rectangular), 10, 10,
		  Invented = "1999",
		  InventedBy = "Mark and Eric Woodall",
		  Tags = "Chess Variant")]
	public class RomanChess: Abstract.Generic10x10
	{
		// *** PIECE TYPES *** //

		public PieceType Archer;


		// *** CONSTRUCTION *** //

		public RomanChess():
			base
				 ( /* symmetry = */ new MirrorSymmetry() )
		{
		}

		// *** INITIALIZATION *** //

		#region SetGameVariables
		public override void SetGameVariables()
		{
			base.SetGameVariables();
			Array = "rnabqkbanr/pppppppppp/10/10/10/10/10/10/PPPPPPPPPP/RNABQKBANR";
			PawnMultipleMove.Value = "Double";
			EnPassant = true;
			Castling.Value = "Standard";
			PromotionRule.Value = "Standard";
			PromotionTypes = "QRNBA";
		}
		#endregion

		#region AddPieceTypes
		public override void AddPieceTypes()
		{
			base.AddPieceTypes();
			AddChessPieceTypes();
			AddPieceType( Archer = new General( "Archer", "A", 325, 375 ) );
		}
		#endregion
	}
}
