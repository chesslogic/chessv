
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
	[Game("Relative Royalty Chess", typeof(Geometry.Rectangular), 8, 8,
		  Invented = "2017",
		  InventedBy = "various",
		  Tags = "Chess Variant")]
	public class RelativeRoyaltyChess: Chess
	{
		// *** INITIALIZATION *** //

		#region SetGameVariables
		public override void SetGameVariables()
		{
			base.SetGameVariables();
			Array = "rnbqkknr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKKNR";
		}
		#endregion

		#region AddRules
		public override void AddRules()
		{
			base.AddRules();
			ReplaceRule( FindRule( typeof(Rules.CheckmateRule) ), new Rules.MultiKing.RelativeRoyaltyCheckmateRule( King ) );
		}
		#endregion

		#region AddEvaluations
		public override void AddEvaluations()
		{
			base.AddEvaluations();

			//	We need to remove the LowMaterialEvaluation for now. 
			//	It doesn't understand multiple kings, so all the logic to 
			//	detect draws by insufficient material, etc, won't do 
			//	the right thing.
			RemoveEvaluation( typeof( LowMaterialEvaluation ) );
		}
		#endregion
	}
}
