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

using System.Collections.Generic;

namespace ChessV.Games.Rules.Extinction
{
	//**********************************************************************
	//
	//                        ExtinctionRule
	//
	//    This rule adds the victory condition used in Extinction Chess 
	//    as well as Kinglet.  Constructor takes a string of notations of 
	//    all the types that will cause the player to lose if the last 
	//    piece of the type is captured.

	public class ExtinctionRule: Rule
	{
		// *** CONSTRUCTION *** //

		public ExtinctionRule( string types )
		{
			extinctionTypesNotation = types;
		}


		// *** INITIALIZATION *** //

		public override void PostInitialize()
		{
			base.PostInitialize();
			List<PieceType> types = Game.ParseTypeListFromString( extinctionTypesNotation );
			extinctionTypeNumbers = new List<int>();
			foreach( PieceType type in types )
				extinctionTypeNumbers.Add( type.TypeNumber );
		}


		// *** OVERRIDES *** //

		public override MoveEventResponse TestForWinLossDraw( int currentPlayer, int ply )
		{
			foreach( int typeNumber in extinctionTypeNumbers )
			{
				if( Board.GetPieceTypeBitboard( currentPlayer, typeNumber ).BitCount == 0 )
					return MoveEventResponse.GameLost;
				if( Board.GetPieceTypeBitboard( currentPlayer ^ 1, typeNumber ).BitCount == 0 )
					return MoveEventResponse.GameWon;
			}
			return MoveEventResponse.NotHandled;
		}

		public override void GetNotesForPieceType( PieceType type, List<string> notes )
		{
			if( extinctionTypeNumbers.Contains( type.TypeNumber ) )
				notes.Add( "extinction loses" );
		}


		// *** PROTECTED DATA MEMBERS *** //

		protected string extinctionTypesNotation;
		protected List<int> extinctionTypeNumbers;
	}
}
