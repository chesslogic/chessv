
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

namespace ChessV
{
  public struct BitBoard
  {
    // *** CONSTRUCTION *** //

    #region Constructors
    public BitBoard(int nBits)
    {
      bits2 = 0;
      bits1 = 0;
      bits0 = 0;
      bitCount = 0;
      capacity = (nBits - 1) / 64;
    }

    public BitBoard(int capacity, UInt64 bits0, UInt64 bits1 = 0UL, UInt64 bits2 = 0UL)
    {
      this.bits0 = bits0;
      this.bits1 = bits1;
      this.bits2 = bits2;
      this.capacity = capacity;
      bitCount = -1;
    }
    #endregion


    // *** PROPERTIES *** //

    #region BitCount
    public int BitCount
    {
      get
      {
        if (bitCount < 0)
        {
          //	The bitCount will be set to -1 in those cases where 
          //	we don't know the value, so we must now recalcualte it.
          //	This is the SWAR-PopCount routine from the chess programming wiki:
          //	http://chessprogramming.wikispaces.com/Population+Count#SWAR-Popcount

          bitCount = 0;
          UInt64 x;
          switch (capacity)
          {
            case 2:
              x = bits2;
              x = x - ((x >> 1) & k1); /* put count of each 2 bits into those 2 bits */
              x = (x & k2) + ((x >> 2) & k2); /* put count of each 4 bits into those 4 bits */
              x = (x + (x >> 4)) & k4; /* put count of each 8 bits into those 8 bits */
              x = (x * kf) >> 56; /* returns 8 most significant bits of x + (x<<8) + (x<<16) + (x<<24) + ...  */
              bitCount += (int)x;
              goto case 1;

            case 1:
              x = bits1;
              x = x - ((x >> 1) & k1); /* put count of each 2 bits into those 2 bits */
              x = (x & k2) + ((x >> 2) & k2); /* put count of each 4 bits into those 4 bits */
              x = (x + (x >> 4)) & k4; /* put count of each 8 bits into those 8 bits */
              x = (x * kf) >> 56; /* returns 8 most significant bits of x + (x<<8) + (x<<16) + (x<<24) + ...  */
              bitCount += (int)x;
              goto case 0;

            case 0:
              x = bits0;
              x = x - ((x >> 1) & k1); /* put count of each 2 bits into those 2 bits */
              x = (x & k2) + ((x >> 2) & k2); /* put count of each 4 bits into those 4 bits */
              x = (x + (x >> 4)) & k4; /* put count of each 8 bits into those 8 bits */
              x = (x * kf) >> 56; /* returns 8 most significant bits of x + (x<<8) + (x<<16) + (x<<24) + ...  */
              bitCount += (int)x;
              break;
          }
        }
        return bitCount;
      }
    }
    #endregion

    #region LSB
    public int LSB
    {
      get
      //	returns least significant bit
      {
        switch (capacity)
        {
          case 2:
            if (bits0 != 0)
              return index64[((bits0 & (UInt64)(-((Int64)bits0))) * debruijn64) >> 58];
            if (bits1 != 0)
              return index64[((bits1 & (UInt64)(-((Int64)bits1))) * debruijn64) >> 58] + 64;
            if (bits2 != 0)
              return index64[((bits2 & (UInt64)(-((Int64)bits2))) * debruijn64) >> 58] + 128;
            return -1;

          case 1:
            if (bits0 != 0)
              return index64[((bits0 & (UInt64)(-((Int64)bits0))) * debruijn64) >> 58];
            if (bits1 != 0)
              return index64[((bits1 & (UInt64)(-((Int64)bits1))) * debruijn64) >> 58] + 64;
            return -1;

          case 0:
            if (bits0 != 0)
              return index64[((bits0 & (UInt64)(-((Int64)bits0))) * debruijn64) >> 58];
            return -1;
        }
        throw new Exception("Unsupported bitboard capacity");
      }
    }
    #endregion

    #region IsEmpty
    public bool IsEmpty
    {
      get
      {
        return bits0 == 0 && bits1 == 0 && bits2 == 0;
      }
    }
    #endregion


    // *** OPERATIONS *** //

    #region Clear
    public void Clear()
    {
      bits2 = 0;
      bits1 = 0;
      bits0 = 0;
      bitCount = 0;
    }
    #endregion

    #region SetAll
    public void SetAll()
    {
      bits2 = 0xFFFFFFFFFFFFFFFFUL;
      bits1 = 0xFFFFFFFFFFFFFFFFUL;
      bits0 = 0xFFFFFFFFFFFFFFFFUL;
      bitCount = capacity * 64;
    }
    #endregion

    #region GetBit
    public int GetBit(int bitnumber)
    {
      switch (bitnumber / 64)
      {
        case 2:
          return (int)(bits2 >> (bitnumber % 64)) & 1;

        case 1:
          return (int)(bits1 >> (bitnumber % 64)) & 1;

        case 0:
          return (int)(bits0 >> (bitnumber % 64)) & 1;
      }
      throw new ArgumentOutOfRangeException("bitnumber", "Argument out of range in BitBoard.GetBit function");
    }
    #endregion

    #region IsBitSet
    public bool IsBitSet(int bitnumber)
    {
      switch (bitnumber / 64)
      {
        case 2:
          return ((bits2 >> (bitnumber % 64)) & 1) != 0UL;

        case 1:
          return ((bits1 >> (bitnumber % 64)) & 1) != 0UL;

        case 0:
          return ((bits0 >> (bitnumber % 64)) & 1) != 0UL;
      }
      throw new ArgumentOutOfRangeException("bitnumber", "Argument out of range in BitBoard.GetBit function");
    }
    #endregion

    #region SetBit
    public void SetBit(int bitnumber)
    {
      switch (bitnumber / 64)
      {
        case 2:
          if (bitCount >= 0)
            bitCount += ((int)(bits2 >> (bitnumber % 64)) & 1) ^ 1;
          bits2 |= 1UL << (bitnumber % 64);
          break;

        case 1:
          if (bitCount >= 0)
            bitCount += ((int)(bits1 >> (bitnumber % 64)) & 1) ^ 1;
          bits1 |= 1UL << (bitnumber % 64);
          break;

        case 0:
          if (bitCount >= 0)
            bitCount += ((int)(bits0 >> (bitnumber % 64)) & 1) ^ 1;
          bits0 |= 1UL << (bitnumber % 64);
          break;
      }
    }
    #endregion

    #region ClearBit
    public void ClearBit(int bitnumber)
    {
      switch (bitnumber / 64)
      {
        case 2:
          if (bitCount >= 0)
            bitCount -= ((int)(bits2 >> (bitnumber % 64)) & 1);
          bits2 &= 0xFFFFFFFFFFFFFFFFUL ^ (1UL << (bitnumber % 64));
          break;

        case 1:
          if (bitCount >= 0)
            bitCount -= ((int)(bits1 >> (bitnumber % 64)) & 1);
          bits1 &= 0xFFFFFFFFFFFFFFFFUL ^ (1UL << (bitnumber % 64));
          break;

        case 0:
          if (bitCount >= 0)
            bitCount -= ((int)(bits0 >> (bitnumber % 64)) & 1);
          bits0 &= 0xFFFFFFFFFFFFFFFFUL ^ (1UL << (bitnumber % 64));
          break;
      }
    }
    #endregion

    #region ExtractLSB
    public int ExtractLSB()
    //	returns and clears the least significant bit
    {
      if (bitCount > 0)
        bitCount--;
      else if (bitCount == 0)
        throw new Exception("BitBoard.ExtractLSB: no bits to extract");

      int returnval;
      switch (capacity)
      {
        case 2:
          if (bits0 != 0)
          {
            returnval = index64[((bits0 & (UInt64)(-((Int64)bits0))) * debruijn64) >> 58];
            bits0 = bits0 & (UInt64)((Int64)bits0 - 1);
            return returnval;
          }
          if (bits1 != 0)
          {
            returnval = index64[((bits1 & (UInt64)(-((Int64)bits1))) * debruijn64) >> 58] + 64;
            bits1 = bits1 & (UInt64)((Int64)bits1 - 1);
            return returnval;
          }
          if (bits2 != 0)
          {
            returnval = index64[((bits2 & (UInt64)(-((Int64)bits2))) * debruijn64) >> 58] + 128;
            bits2 = bits2 & (UInt64)((Int64)bits2 - 1);
            return returnval;
          }
          return -1;

        case 1:
          if (bits0 != 0)
          {
            returnval = index64[((bits0 & (UInt64)(-((Int64)bits0))) * debruijn64) >> 58];
            bits0 = bits0 & (UInt64)((Int64)bits0 - 1);
            return returnval;
          }
          if (bits1 != 0)
          {
            returnval = index64[((bits1 & (UInt64)(-((Int64)bits1))) * debruijn64) >> 58] + 64;
            bits1 = bits1 & (UInt64)((Int64)bits1 - 1);
            return returnval;
          }
          return -1;

        case 0:
          if (bits0 != 0)
          {
            returnval = index64[((bits0 & (UInt64)(-((Int64)bits0))) * debruijn64) >> 58];
            bits0 = bits0 & (UInt64)((Int64)bits0 - 1);
            return returnval;
          }
          return -1;
      }
      throw new Exception("Unsupported bitboard capacity");
    }
    #endregion

    #region GetData
    public UInt64 GetData(int bitGroup)
    {
      switch (bitGroup)
      {
        case 0:
          return bits0;

        case 1:
          return bits1;

        case 2:
          return bits2;
      }
      //	we should never get here
      return 0;
    }
    #endregion

    #region SetData
    public void SetData(UInt64 b0, UInt64 b1, UInt64 b2, int bitct = -1)
    {
      bits0 = b0;
      bits1 = b1;
      bits2 = b2;
      bitCount = bitct;
    }
    #endregion


    // *** OVERLOADED OPERATORS *** //

    #region Overloaded Operators
    public bool this[int bitnumber]
    {
      get
      { return IsBitSet(bitnumber); }
    }

    public static implicit operator bool(BitBoard bb)
    {
      return bb.bitCount >= 0
        ? bb.bitCount > 0
        : ((bb.capacity == 0 && bb.bits0 != 0UL) ||
           (bb.capacity == 1 && (bb.bits0 != 0UL || bb.bits1 != 0UL)) ||
           (bb.capacity == 2 && (bb.bits0 != 0UL || bb.bits1 != 0UL || bb.bits2 != 0UL)));
    }

    public static BitBoard operator <<(BitBoard bb, int shift)
    {
      switch (bb.capacity)
      {
        case 0:
          return new BitBoard(0, bb.bits0 << shift);

        case 1:
          return new BitBoard(1, bb.bits0 << shift, (bb.bits1 << shift) | (bb.bits0 >> (64 - shift)));

        case 2:
          return new BitBoard(2, bb.bits0 << shift, (bb.bits1 << shift) | (bb.bits0 >> (64 - shift)),
            (bb.bits2 << shift) | (bb.bits1 >> (64 - shift)));
      }
      throw new Exception("Unsupported bitboard capacity");
    }

    public static BitBoard operator >>(BitBoard bb, int shift)
    {
      switch (bb.capacity)
      {
        case 0:
          return new BitBoard(0, bb.bits0 >> shift);

        case 1:
          return new BitBoard(1, (bb.bits0 >> shift) | (bb.bits1 << (64 - shift)), bb.bits1 >> shift);

        case 2:
          return new BitBoard(2, (bb.bits0 >> shift) | (bb.bits1 << (64 - shift)), (bb.bits1 >> shift) | (bb.bits2 << (64 - shift)), bb.bits2 >> shift);
      }
      throw new Exception("Unsupported bitboard capacity");
    }

    public static BitBoard operator &(BitBoard b1, BitBoard b2)
    {
      return new BitBoard(b1.capacity, b1.bits0 & b2.bits0, b1.bits1 & b2.bits1, b1.bits2 & b2.bits2);
    }

    public static BitBoard operator |(BitBoard b1, BitBoard b2)
    {
      return new BitBoard(b1.capacity, b1.bits0 | b2.bits0, b1.bits1 | b2.bits1, b1.bits2 | b2.bits2);
    }
    #endregion


    // *** INTERNAL DATA *** //

    private UInt64 bits2;
    private UInt64 bits1;
    private UInt64 bits0;
    private Int32 bitCount;
    private Int32 capacity;


    // *** HELPER CONSTANTS *** //

    private const UInt64 k1 = 0x5555555555555555UL; /*  -1/3   */
    private const UInt64 k2 = 0x3333333333333333UL; /*  -1/5   */
    private const UInt64 k4 = 0x0f0f0f0f0f0f0f0fUL; /*  -1/17  */
    private const UInt64 kf = 0x0101010101010101UL; /*  -1/255 */

    private static int[] index64 = {
      63,  0, 58,  1, 59, 47, 53,  2,
      60, 39, 48, 27, 54, 33, 42,  3,
      61, 51, 37, 40, 49, 18, 28, 20,
      55, 30, 34, 11, 43, 14, 22,  4,
      62, 57, 46, 52, 38, 26, 32, 41,
      50, 36, 17, 19, 29, 10, 13, 21,
      56, 45, 25, 31, 35, 16,  9, 12,
      44, 24, 15,  8, 23,  7,  6,  5
    };

    private const UInt64 debruijn64 = 0x07EDD5E59A4E28C2UL;
  }
}
