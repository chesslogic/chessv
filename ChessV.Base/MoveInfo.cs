
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

    public static bool operator ==(MoveInfo m1, MoveInfo m2)
    { return m1.MoveType == m2.MoveType && m1.FromSquare == m2.FromSquare && m1.ToSquare == m2.ToSquare && m1.tagOrPromotionType == m2.tagOrPromotionType; }

    public static bool operator !=(MoveInfo m1, MoveInfo m2)
    { return m1.MoveType != m2.MoveType || m1.FromSquare != m2.FromSquare || m1.ToSquare != m2.ToSquare || m1.tagOrPromotionType != m2.tagOrPromotionType; }

    public static implicit operator Movement(MoveInfo mi)
    {
      return new Movement(mi.FromSquare, mi.ToSquare, mi.Player, mi.MoveType, mi.tagOrPromotionType);
    }

    public UInt64 Hash
    {
      get
      {
        return
          ((UInt64)FromSquare & 0xFFUL) |
          (((UInt64)ToSquare & 0xFFUL) << 8) |
          (((UInt64)tagOrPromotionType & 0xFFFFUL) << 16) |
          (((UInt64)MoveType & 0x7FUL) << 32) |
          (((UInt64)Player & 1UL) << 39);
      }
    }

    public static implicit operator UInt64(MoveInfo mi)
    { return mi.Hash; }

    public override bool Equals(object obj)
    {
      if (obj is MoveInfo)
        return Equals((MoveInfo)obj);
      return false;
    }

    public bool Equals(MoveInfo other)
    { return this == other; }

    public override int GetHashCode()
    { return Hash.GetHashCode(); }

    public override string ToString()
    {
      string fromSquare = $"{(char)('a' + (FromSquare % 8))}{8 - (FromSquare / 8)}";
      string toSquare = $"{(char)('a' + (ToSquare % 8))}{8 - (ToSquare / 8)}";
      if (PieceMoved == null) return string.Format("{0} {1} {2}", MoveType, fromSquare, toSquare);
      return string.Format("{0} {1} {2} {3}", MoveType, fromSquare, toSquare, PieceMoved.PieceType.Notation[Player]);
    }
  }
}
