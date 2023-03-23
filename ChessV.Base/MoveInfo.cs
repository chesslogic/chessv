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
	public struct MoveInfo
	{
		private Int32 tagOrPromotionType { get; set; }

		public MoveType MoveType { get; set; }
		public Int32 Player { get; set; }
		public Int32 FromSquare { get; set; }
		public Int32 ToSquare { get; set; }
		public Int32 Tag { get { return tagOrPromotionType; } set { tagOrPromotionType = value; } }
		public Int32 PromotionType { get { return tagOrPromotionType; } set { tagOrPromotionType = value; } }
		public Int32 OriginalType { get; set; }
		public Int32 PickupCursor { get; set; }
		public Int32 DropCursor { get; set; }
		public Int32 Evaluation { get; set; }
		public Piece PieceMoved { get; set; }
		public Piece PieceCaptured { get; set; }

		public static bool operator ==( MoveInfo m1, MoveInfo m2 )
		{ return m1.MoveType == m2.MoveType && m1.FromSquare == m2.FromSquare && m1.ToSquare == m2.ToSquare && m1.tagOrPromotionType == m2.tagOrPromotionType; }

		public static bool operator !=( MoveInfo m1, MoveInfo m2 )
		{ return m1.MoveType != m2.MoveType || m1.FromSquare != m2.FromSquare || m1.ToSquare != m2.ToSquare || m1.tagOrPromotionType != m2.tagOrPromotionType; }

		public static implicit operator Movement( MoveInfo mi )
		{
			return new Movement( mi.FromSquare, mi.ToSquare, mi.Player, mi.MoveType, mi.tagOrPromotionType );
		}

		public UInt32 Hash
		{
			get
			{ return (uint) FromSquare + (uint) (ToSquare << 8) + (uint) (tagOrPromotionType << 16) + ((uint) MoveType << 24) + ((uint) Player << 31); }
		}

		public static implicit operator UInt32( MoveInfo mi )
		{ return mi.Hash; }

		public override bool Equals( object obj )
		{
			if( obj is MoveInfo )
				return Equals( (MoveInfo) obj );
			return false;
		}

		public bool Equals( MoveInfo other )
		{ return this == other; }

		public override int GetHashCode()
		{ return (int) Hash; }
	}
}
