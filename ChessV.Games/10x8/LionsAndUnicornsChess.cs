﻿
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

using ChessV.Evaluations;

namespace ChessV.Games
{
	//**********************************************************************
	//
	//                    LionsAndUnicornsChess
	//
	//    This class implements David Paulowich's Lions and Unicorns Chess.
	//    It adds two new types, the Lion (Betza's HFD) and the Unicorn 
	//    (Bishop+Knightrider) on an 8x10 board.

	[Game("Lions and Unicorns Chess", typeof(Geometry.Rectangular), 10, 8,
		  Invented = "2005",
		  InventedBy = "David Paulowich",
		  Tags = "Chess Variant")]
	[Appearance(ColorScheme="Lesotho", NumberOfSquareColors=3)]
	public class LionsAndUnicornsChess: Abstract.Generic10x8
	{
		// *** PIECE TYPES *** //

		public PieceType Unicorn;
		public PieceType Chancellor;
		public PieceType Lion;


		// *** CONSTRUCTION *** //

		public LionsAndUnicornsChess(): 
			base
				( /* symmetry = */ new MirrorSymmetry() )
		{
		}


		// *** INITIALIZATION *** //

		#region SetGameVariables
		public override void SetGameVariables()
		{
			base.SetGameVariables();
			NumberOfSquareColors = 3;
			Array = "lrcbqkburl/nppppppppn/10/10/10/10/NPPPPPPPPN/LRCBQKBURL";
			PawnDoubleMove = true;
			EnPassant = true;
			PromotionRule.Value = "Standard";
			PromotionTypes = "QCU";
			Castling.Value = "Close-Rook";
		}
		#endregion

		#region AddPieceTypes
		public override void AddPieceTypes()
		{
			base.AddPieceTypes();
			AddChessPieceTypes();
			AddPieceType( Unicorn = new Unicorn( "Unicorn", "U", 1050, 1125 ) );
			AddPieceType( Chancellor = new Chancellor( "Chancellor", "C", 1050, 1125 ) );
			AddPieceType( Lion = new Lion( "Lion", "L", 400, 400 ) );
		}
		#endregion

		#region AddEvaluations
		public override void AddEvaluations()
		{
			base.AddEvaluations();
			if( Chancellor != null && Chancellor.Enabled )
				RookTypeEval.AddRookOn7thBonus( Chancellor, King, 2, 8 );
			if( Lion != null && Lion.Enabled )
				OutpostEval.AddOutpostBonus( Lion, 12, 4, 8, 6 );
		}
		#endregion
	}
}
