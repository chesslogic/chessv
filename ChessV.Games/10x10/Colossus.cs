
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

namespace ChessV.Games
{
	[Game("Colossus", typeof(Geometry.Rectangular), 10, 10,
		  Invented = "2010",
		  InventedBy = "Charles Daniel",
		  Tags = "Chess Variant",
		  GameDescription1 = "A large board variant with standard pieces",
		  GameDescription2 = "and twice as many rooks, knights, and bishops")]
	public class Colossus: Abstract.Generic10x10
	{
		// *** CONSTRUCTION *** //

		public Colossus() :
			base
				 ( /* symmetry = */ new MirrorSymmetry() )
		{
		}

		// *** INITIALIZATION *** //

		#region SetGameVariables
		public override void SetGameVariables()
		{
			base.SetGameVariables();
			Array = "r1nb2bn1r/1rnbqkbnr/pppppppppp/10/10/10/10/PPPPPPPPPP/1RNBQKBNR/R1NB2BN1R";
			PromotionTypes = "QRBN";
			PawnMultipleMove.Value = "Fast Pawn";
			EnPassant = true;
			Castling.AddChoice( "Colossus", "King starting on the second rank slides 1-3 spaces short " +
				"or 1-4 spaces long to castle with the piece on the b or i file" );
			Castling.Value = "Colossus";
		}
		#endregion

		#region AddPieceTypes
		public override void AddPieceTypes()
		{
			base.AddPieceTypes();
			AddChessPieceTypes();
		}
		#endregion

		#region AddRules
		public override void AddRules()
		{
			base.AddRules();

			// *** CASTLING *** //

			#region Castling
			if( Castling.Value == "Colossus" )
			{
				//	find the king's start square (must be e1 or f1)
				GenericPiece WhiteKing = new GenericPiece( 0, CastlingType );
				GenericPiece BlackKing = new GenericPiece( 1, CastlingType );
				bool supported = false;
				string kingSquare = null;
				if( StartingPieces["f2"] == WhiteKing )
				{
					kingSquare = "f2";
					if( StartingPieces["f9"] == BlackKing )
						supported = true;
				}
				else if( StartingPieces["e2"] == WhiteKing )
				{
					kingSquare = "e2";
					if( StartingPieces["e9"] == BlackKing )
						supported = true;
				}
				if( !supported )
					throw new Exception( "Can't enable castling rule because King does not start on a supported square" );

				AddFlexibleCastlingRule();
				if( kingSquare == "f2" )
				{
					FlexibleCastlingMove( 0, "f2", "g2", "i2", 'K', true );
					FlexibleCastlingMove( 0, "f2", "e2", "b2", 'Q', true );
					FlexibleCastlingMove( 1, "f9", "g9", "i9", 'k', true );
					FlexibleCastlingMove( 1, "f9", "e9", "b9", 'q', true );
				}
				else
				{
					//	if the King starts on e2, use shredder-notation
					FlexibleCastlingMove( 0, "e2", "d2", "b2", 'B', true );
					FlexibleCastlingMove( 0, "e2", "f2", "i2", 'I', true );
					FlexibleCastlingMove( 1, "e9", "d9", "b9", 'b', true );
					FlexibleCastlingMove( 1, "e9", "f9", "i9", 'i', true );
				}
			}
			#endregion
		}
		#endregion
	}
}
