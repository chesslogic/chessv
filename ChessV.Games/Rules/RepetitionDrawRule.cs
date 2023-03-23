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

using System;

namespace ChessV.Games.Rules
{
	public class RepetitionDrawRule: Rule
	{
		protected UInt64[] gameHistoryHashes;
		protected UInt64[] searchStackHashes;

		public RepetitionDrawRule()
		{
		}

		public override void Initialize( Game game )
		{
			base.Initialize( game );
			gameHistoryHashes = new UInt64[Game.MAX_GAME_LENGTH];
			searchStackHashes = new UInt64[Game.MAX_PLY];
			Game.MoveBeingPlayed += MoveBeingPlayedHandler;
		}

		public override void PostInitialize()
		{
			base.PostInitialize();
		}

		void MoveBeingPlayedHandler( MoveInfo move )
		{
			UInt64 hash = Game.GetPositionHashCode( 2 );
			gameHistoryHashes[Game.GameMoveNumber] = hash;
		}

		public override MoveEventResponse MoveMade( MoveInfo move, int ply )
		{
			UInt64 hash = Game.GetPositionHashCode( ply );
			searchStackHashes[ply] = hash;
			return MoveEventResponse.MoveOk;
		}

		public override MoveEventResponse TestForWinLossDraw( int currentPlayer, int ply )
		{
			int count = 1;
			UInt64 hash = Game.GetPositionHashCode( ply );
			for( int x = ply - 1; x > 0; x-- )
				if( searchStackHashes[x] == hash )
					count++;
			for( int y = Game.GameMoveNumber - 1; count < 3 && y > 0; y-- )
				if( gameHistoryHashes[y] == hash )
					count++;
			if( count >= 3 )
				return MoveEventResponse.GameDrawn;
			return MoveEventResponse.NotHandled;
		}
	}
}
