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

using System.Text;

namespace ChessV.Boards
{
  public class BoardWithCards : Board
  {
    public int HandSize;

    public BoardWithCards(int nFiles, int nRanks, int handSize, int numPlayers = 2) : base(nFiles, nRanks, nRanks * nFiles + handSize * numPlayers)
    {
      HandSize = handSize;
      for (int player = 0; player < numPlayers; player++)
      {
        for (int handIndex = 1; handIndex <= handSize; handIndex++)
        {
          int square = NumSquares - 1 + handIndex + (handSize * player);
          rankBySquare[square] = player;
          fileBySquare[square] = -handIndex;
        }
      }
    }

    //	Lookup for the notaiton of a square
    public override string GetDefaultSquareNotation(int square)
    {
      if (square < NumRanks * NumFiles)
        return GetFileNotation(GetFile(square)) + GetRankNotation(GetRank(square));
      else
      {
        Location location = SquareToLocation(square);
        StringBuilder result = new StringBuilder();
        result.Append("PKT");
        result.Append(-location.File);
        return result.ToString();
      }
    }

    public override Location SquareToLocation(int square)
    {
      if (square < 0)
        return Location.NullLocation;
      if (square < NumSquares)
        return new Location(rankBySquare[square], fileBySquare[square]);
      else
      {
        int offset = square - NumSquares;
        return new Location(offset / HandSize, -(offset % HandSize) - 1);
      }
    }

    public override int LocationToSquare(Location location)
    {
      if (location.File >= 0)
        return location.File * NumRanks + location.Rank;
      else
        return NumSquares + location.Rank * HandSize - location.File - 1;
    }
  }
}
