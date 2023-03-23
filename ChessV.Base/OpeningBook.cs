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
using System.IO;

namespace ChessV
{
	public class OpeningBook
	{
		// *** CONSTRUCTION *** //

		public OpeningBook( BinaryReader instream )
		{
			book = new Dictionary<UInt64, List<Int32>>();
			while( true )
			{
				//	each entry begins with the hashcode of a position
				UInt64 positionHash = instream.ReadUInt64();
				//	if this is null, that signals the end of the book
				if( positionHash == 0 )
					break;
				//	create a list of move hashes to store the moves 
				//	which are to be randonly chosen from for this position
				List<Int32> movelist = new List<Int32>();
				Int32 movehash = instream.ReadInt32();
				while( movehash != 0 )
				{
					movelist.Add( movehash );
					movehash = instream.ReadInt32();
				}
			}
			random = new Random();
		}


		// *** OPERATIONS *** //

		public bool Lookup( UInt64 positionHash, out Int32 moveHash )
		{
			moveHash = 0;
			if( book.ContainsKey( positionHash ) )
			{
				int numberOfMoves = book[positionHash].Count;
				moveHash = book[positionHash][random.Next( numberOfMoves )];
				return true;
			}
			return false;
		}


		// *** PROTECTED DATA *** //

		protected Dictionary<UInt64, List<Int32>> book;
		protected Random random;
	}
}
