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

namespace ChessV.Games
{
	[Game("Opulent Chess", typeof(Geometry.Rectangular), 10, 10,
		  XBoardName = "opulent",
		  Invented = "2005",
		  InventedBy = "Greg Strong",
		  Tags = "Chess Variant",
		  GameDescription1 = "Expanded version of Grand Chess with 10 piece types",
		  GameDescription2 = "Features four types of pieces with nearly equal value")]
	[Appearance(ColorScheme = "Marmoor Quadraut")]
	public class OpulentChess: GrandChess
	{
		// *** PIECE TYPES *** //

		public PieceType Wizard;
		public PieceType Lion;


		// *** CONSTRUCTION *** //

		public OpulentChess()
		{
		}

		// *** INITIALIZATION *** //

		#region SetGameVariables
		public override void SetGameVariables()
		{
			base.SetGameVariables();
			Array = "rw6wr/clnbqkbnla/pppppppppp/10/10/10/10/PPPPPPPPPP/CLNBQKBNLA/RW6WR";
		}
		#endregion

		#region AddPieceTypes
		public override void AddPieceTypes()
		{
			base.AddPieceTypes();
			AddPieceType( Wizard = new Wizard( "Wizard", "W", 460, 460 ) );
			AddPieceType( Lion = new Lion( "Lion", "L", 475, 475 ) );
			//	Add the Wazir-move to the Knight
			Wazir.AddMoves( Knight );
			Knight.MidgameValue = 475;
			Knight.EndgameValue = 475;
			Knight.PreferredImage = "Knight Wazir";
			//	Set the name for the Chancellor (called Marshall in Grand Chess)
			Marshall.Name = "Chancellor";
			Marshall.SetNotation( "C" );
			//	Set the name for the Archbishop (called a Cardinal in Grand Chess)
			Cardinal.Name = "Archbishop";
			Cardinal.SetNotation( "A" );
		}
		#endregion

		#region AddEvaluations
		public override void AddEvaluations()
		{
			base.AddEvaluations();
			var developentEval =  (Evaluations.Grand.GrandChessDevelopmentEvaluation)
				FindEvaluation( typeof(Evaluations.Grand.GrandChessDevelopmentEvaluation) );
			developentEval.HeavyPieceThreshold = 700;
		}
		#endregion
	}
}
