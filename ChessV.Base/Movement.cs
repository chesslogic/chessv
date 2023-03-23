﻿
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

using System;
using System.Collections.Generic;

namespace ChessV
{
	/************************************************************************

                                  Movement

	The Movement class stores enough information to uniquely identify a move
	from any valid position of any supported game.  It does not provide
	enough information to actually make the move - that requires a MoveInfo.
	The Movement is used for quickly comparing moves for equivalence and 
	recording them where space is tight (hashtable and killer move list for 
	example.)  It stores several values packed into a single 32-bit 
	integer ("Hash") and later unpacked.  The Movement class provides 
	functions for performing this packing and unpacking.

	The pieces of information stored are: from square number, to square 
	number, number of side moving, indication of the type of move, and a 
	'tag' value that represents different things for different types of 
	moves (such as the type of piece to promote to.)

    ************************************************************************/

	public class Movement
	{
		// *** PROPERTIES *** //

		#region Properties 
		public int FromSquare { get; private set; }
		public int ToSquare { get; private set; }
		public int Tag { get; private set; }
		private uint playerAndType { get; set; }

		public MoveType MoveType 
		{ get { return (MoveType) (playerAndType & 127u); } }

		public int Player
		{ get { return (int) ((playerAndType & 128u) >> 7); } }

		public UInt32 Hash
		{ get { return (uint) FromSquare + (uint) (ToSquare << 8) + (uint) (Tag << 16) + (playerAndType << 24); } }
		#endregion


		// *** CONSTRUCTION *** //

		#region Constructors
		public Movement()
		{
			playerAndType = (uint) MoveType.Invalid;
		}

		public Movement( int from, int to, int player, MoveType type, int tag = 0 )
		{
			FromSquare = from;
			ToSquare = to;
			playerAndType = ((uint) player << 7) | (uint) type;
			Tag = tag;
		}

		public Movement( UInt32 movehash )
		{
			FromSquare = GetFromSquareFromHash( movehash );
			ToSquare = GetToSquareFromHash( movehash );
			playerAndType = (uint) GetMoveTypeFromHash( movehash ) | (uint) (GetPlayerFromHash( movehash ) << 7);
			Tag = GetTagFromHash( movehash );
		}
		#endregion


		// *** HASH FUNCTIONS *** //

		#region GetFromSquareFromHash
		public static int GetFromSquareFromHash( UInt32 movehash )
		{
			return (int) (movehash & 0x000000FF); 
		}
		#endregion

		#region GetToSquareFromHash
		public static int GetToSquareFromHash( UInt32 movehash )
		{
			return (int) ((movehash & 0x0000FF00) >> 8); 
		}
		#endregion

		#region GetPlayerFromHash
		public static int GetPlayerFromHash( UInt32 movehash )
		{
			return (int) ((movehash >> 31) & 1); 
		}
		#endregion

		#region GetTagFromHash
		public static int GetTagFromHash( UInt32 movehash )
		{
			return (int) ((movehash & 0x00FF0000) >> 16); 
		}
		#endregion

		#region GetMoveTypeFromHash
		public static MoveType GetMoveTypeFromHash( UInt32 movehash )
		{
			return (MoveType) ((movehash & 0x7F000000) >> 24); 
		}
		#endregion

		#region GetHashCode
		public override int GetHashCode()
		{
			return (int) Hash;
		}
		#endregion


		// *** OPERATOR OVERLOADS *** //

		#region Operator Overloads
		public override bool Equals( object obj )
		{
			if( obj == null )
				return false;
			Movement other = obj as Movement;
			if( ((object) other) == null )
				return false;

			return 
				FromSquare == other.FromSquare &&
				ToSquare == other.ToSquare &&
				playerAndType == other.playerAndType &&
				Tag == other.Tag;
		}

		public static bool operator ==( Movement m1, Movement m2 )
		{
			if( System.Object.ReferenceEquals( m1, m2 ) )
				return true;
			if( ((object) m1) == null || ((object) m2) == null )
				return false;

			return
				m1.FromSquare == m2.FromSquare && 
				m1.ToSquare == m2.ToSquare && 
				m1.playerAndType == m2.playerAndType && 
				m1.Tag == m2.Tag;
		}

		public static bool operator !=( Movement m1, Movement m2 )
		{
			if( System.Object.ReferenceEquals( m1, m2 ) )
				return false;
			if( ((object) m1) == null || ((object) m2) == null )
				return true;

			return
				m1.FromSquare != m2.FromSquare || 
				m1.ToSquare != m2.ToSquare || 
				m1.playerAndType != m2.playerAndType || 
				m1.Tag != m2.Tag;
		}

		public static bool operator ==( Movement m1, UInt32 hash )
		{
			if( ((object) m1) == null )
				return false;

			return m1.Hash == hash;
		}

		public static bool operator !=( Movement m1, UInt32 hash )
		{
			if( ((object) m1) == null )
				return true;

			return m1.Hash != hash;
		}
		#endregion
	}
}
