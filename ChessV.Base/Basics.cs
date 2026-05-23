
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
using System.Collections.Generic;

namespace ChessV
{
  public delegate void ThinkingCallback(Dictionary<string, string> info);

  public delegate void InitializationHelper(Game game, object helperObject = null);

  #region MoveEventResponse enumeration
  public enum MoveEventResponse
  {
    MoveOk = 1,
    Handled = 2,
    NotHandled = 3,
    IllegalMove = 4,
    GameWon = 5,
    GameLost = 6,
    GameDrawn = 7
  }
  #endregion

  #region MoveNotation enumemartion
  public enum MoveNotation
  {
    XBoard,
    StandardAlgebraic,
    Descriptive,
    MoveSelectionText
  }
  #endregion

  #region MoveType enumeration
  public enum MoveType : uint
  {
    //	Invalid condition:
    Invalid = 0,

    //	Bits that identify move properties:
    CaptureProperty = 2,
    MultiMoveProperty = 4,
    ReplacementCaptureProperty = 8,
    BaroqueCaptureProperty = 16,
    PromotionProperty = 32,
    DropOrReplaceProperty = 64,

    //	Actual move types:
    StandardMove = 1,
    StandardCapture = CaptureProperty | ReplacementCaptureProperty,
    MoveWithPromotion = StandardMove | PromotionProperty,
    CaptureWithPromotion = StandardCapture | PromotionProperty,
    MoveReplace = StandardMove | DropOrReplaceProperty | PromotionProperty,
    CaptureReplace = StandardCapture | DropOrReplaceProperty | PromotionProperty,
    BaroqueCapture = CaptureProperty | BaroqueCaptureProperty,
    ExtraCapture = CaptureProperty | ReplacementCaptureProperty | BaroqueCaptureProperty,
    Castling = MultiMoveProperty | 1,
    EnPassant = BaroqueCapture | 1,
    Drop = DropOrReplaceProperty,
    Replace = DropOrReplaceProperty | 1,
    MoveRelay = MultiMoveProperty,
    Swap = MultiMoveProperty,
    NullMove = ReplacementCaptureProperty | BaroqueCaptureProperty,
    Pass = ReplacementCaptureProperty | BaroqueCaptureProperty | 1,

    //	Abstract base type:
    CustomMove = ReplacementCaptureProperty | BaroqueCaptureProperty | MultiMoveProperty
  }
  #endregion

  #region Location
  public struct Location
  {
    public int Rank { get; set; }
    public int File { get; set; }

    public Location(int rank, int file) : this()
    { Rank = rank; File = file; }

    public bool IsNull
    { get { return Rank < 0; } }

    public static bool operator ==(Location l1, Location l2)
    { return l1.Rank == l2.Rank && l1.File == l2.File; }

    public static bool operator !=(Location l1, Location l2)
    { return l1.Rank != l2.Rank || l1.File != l2.File; }

    public bool Equals(Location other)
    { return Rank == other.Rank && File == other.File; }

    public override bool Equals(object obj)
    {
      if (obj is Location)
        return Equals((Location)obj);
      return false;
    }

    public override int GetHashCode()
    { return Rank << 8 | File; }

    public static Location NullLocation = new Location(-1, -1);
  }
  #endregion

  public delegate bool ConditionalLocationDelegate(Location loc);

  #region Direction
  public struct Direction
  {
    public int RankOffset { get; set; }
    public int FileOffset { get; set; }

    public Direction(int rankOffset, int fileOffset) : this()
    { RankOffset = rankOffset; FileOffset = fileOffset; }

    public static bool operator ==(Direction d1, Direction d2)
    { return d1.RankOffset == d2.RankOffset && d1.FileOffset == d2.FileOffset; }

    public static bool operator !=(Direction d1, Direction d2)
    { return d1.RankOffset != d2.RankOffset || d1.FileOffset != d2.FileOffset; }

    public bool Equals(Direction other)
    { return RankOffset == other.RankOffset && FileOffset == other.FileOffset; }

    public override bool Equals(object obj)
    {
      if (obj is Direction)
        return Equals((Direction)obj);
      return false;
    }

    public override int GetHashCode()
    { return RankOffset << 8 | FileOffset; }
  }
  #endregion

  #region PredefinedDirections
  public static class PredefinedDirections
  {
    public const int N = 0;
    public const int S = 1;
    public const int E = 2;
    public const int W = 3;
    public const int NE = 4;
    public const int SW = 5;
    public const int NW = 6;
    public const int SE = 7;
  }
  #endregion

  #region SpecialAttacks
  public enum SpecialAttacks
  {
    None = 0,
    CannonCapture = 1,
    RifleCapture = 2
  }
  #endregion

  #region PV class
  public class PV
  {
    public UInt64[] MoveHashes { get; set; }

    public void Initialize()
    {
      MoveHashes = new UInt64[Game.MAX_PLY];
    }

    public UInt64 this[int moveNumber]
    {
      get
      { return MoveHashes[moveNumber]; }

      set
      { MoveHashes[moveNumber] = value; }
    }

    public void CopyTo(PV targetPV)
    {
      for (int x = 0; x < Game.MAX_PLY; x++)
        targetPV[x] = MoveHashes[x];
    }
  }
  #endregion

  #region SearchStack struct
  public struct SearchStack
  {
    public bool IsInCheck { get; set; }
    public int Eval { get; set; }
    public PV PV;

    public void Initialize()
    {
      PV = new PV();
      PV.Initialize();
    }
  }
  #endregion

  #region Pickup struct
  public struct Pickup
  {
    public Piece Piece { get; set; }
    public int Square { get; set; }
  }
  #endregion

  #region Drop struct
  public struct Drop
  {
    public Piece Piece { get; set; }
    public int Square { get; set; }
    public PieceType NewType { get; set; }
  }
  #endregion

  #region PerftResults class
  public class PerftResults
  {
    public int Nodes { get; set; }
    public int Captures { get; set; }
    public int EnPassants { get; set; }
    public int Castles { get; set; }
    public int Promotions { get; set; }

    public PerftResults()
    { Nodes = 0; Captures = 0; EnPassants = 0; Castles = 0; Promotions = 0; }
  }
  #endregion
}
