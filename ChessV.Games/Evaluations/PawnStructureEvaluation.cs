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

using ChessV.Games;
using System;
using System.Collections.Generic;

namespace ChessV.Evaluations
{
  public class PawnStructureEvaluation : Evaluation
  {
    // *** PROPERTIES *** //

    //	Stores the weights for the various adjustments
    public PawnStructureAdjustments Adjustments { get; private set; }

    public bool? PassedPawnEvaluation { get; set; }

    public int PawnPromotionRank { get; set; }


    // *** INITIALIZATION *** //

    #region Constructor
    public PawnStructureEvaluation()
    {
      PassedPawnEvaluation = null;
    }
    #endregion

    #region Initialize
    public override void Initialize(Game game)
    {
      base.Initialize(game);
      player0pawns = new int[Board.MAX_FILES + 2];
      player1pawns = new int[Board.MAX_FILES + 2];
      player0backPawn = new int[Board.MAX_FILES + 2];
      player1backPawn = new int[Board.MAX_FILES + 2];
      if (!(game is Games.Abstract.GenericChess))
        throw new Exception("Fatal Error: PawnStructureEvaluation - game must derive from GenericChess");
      chessGame = (Games.Abstract.GenericChess)game;
    }
    #endregion

    #region PostInitialize
    public override void PostInitialize()
    {
      base.PostInitialize();

      //	Find the type numbers of the King and Pawns
      pawnTypeNumbers = new List<int>();
      kingTypeNumber = -1;
      PieceType[] pieceTypes;
      int pieceTypeCount = game.GetPieceTypes(out pieceTypes);
      for (int nPieceType = 0; nPieceType < pieceTypeCount; nPieceType++)
      {
        if (pieceTypes[nPieceType].IsPawn)
          pawnTypeNumbers.Add(pieceTypes[nPieceType].TypeNumber);
        if (pieceTypes[nPieceType] is King &&
          pieceTypes[nPieceType].GetCustomAttributes(typeof(Games.RoyalAttribute)).Length > 0)
          kingTypeNumber = pieceTypes[nPieceType].TypeNumber;
      }
      if (pawnTypeNumbers.Count == 0)
        throw new Exception("Fatal error in PawnStructureEvaluation: No pawn types found");
      //			if( kingTypeNumber == -1 )
      //				throw new Exception( "Fatal error in PawnStructureEvaluation: Royal piece type not found" );
      Adjustments = new PawnStructureAdjustments();

      //	Set up evaluation of passed pawns.  The Game class can enable or 
      //	disable this by setting PassedPawnEvaluation to true or false.
      //	If the Game class doesn't do either, and it is still null, we 
      //	we assume by default that passed pawn evaluation should be enabled 
      //	if the PromotionType is set and that the promotion rank is the last 
      //	rank of the board.  The Game class should set this if that is not 
      //	correct.  For an example, see Mecklenbeck Chess.
      if (PassedPawnEvaluation == null)
      {
        if (chessGame.PromotingType != null)
        {
          PassedPawnEvaluation = true;
          PawnPromotionRank = board.NumRanks - 1;
        }
        else
          PassedPawnEvaluation = false;
      }

      if (PassedPawnEvaluation == true)
      {
        passedPawnBonusByRank = new int[board.NumRanks];
        int rank = board.NumRanks - 1;
        int bonus = 200;
        for (; rank >= PawnPromotionRank - 1; rank--)
          passedPawnBonusByRank[rank] = bonus;
        double scale = 0.6;
        for (; rank >= 0; rank--)
        {
          bonus = (int)(bonus * scale);
          scale = scale * 0.6;
          passedPawnBonusByRank[rank] = Math.Max(bonus, 5);
        }
        pawnDirectionByPlayer = new int[2];
        pawnDirectionByPlayer[0] = game.GetDirectionNumber(new Direction(1, 0));
        pawnDirectionByPlayer[1] = game.PlayerDirection(1, pawnDirectionByPlayer[0]);
        sidewaysDirectionNumbers = new int[2];
        sidewaysDirectionNumbers[0] = game.GetDirectionNumber(new Direction(0, 1));
        sidewaysDirectionNumbers[1] = game.GetDirectionNumber(new Direction(0, -1));
        pawnSupportDirectionNumbers = new int[2, 2];
        pawnSupportDirectionNumbers[0, 0] = game.GetDirectionNumber(new Direction(-1, 1));
        pawnSupportDirectionNumbers[0, 1] = game.GetDirectionNumber(new Direction(-1, -1));
        pawnSupportDirectionNumbers[1, 0] = game.PlayerDirection(1, pawnSupportDirectionNumbers[0, 0]);
        pawnSupportDirectionNumbers[1, 1] = game.PlayerDirection(1, pawnSupportDirectionNumbers[0, 1]);
      }
    }
    #endregion

    #region SetVariation
    public override void SetVariation(int randomness)
    {
      if (randomness > 0)
      {
        int[] randomAdjustments = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        if (randomness == 1)
          randomAdjustments = new int[] { -2, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2 };
        else if (randomness == 2)
          randomAdjustments = new int[] { -2, -2, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 2 };
        else if (randomness == 3)
          randomAdjustments = new int[] { -3, -2, -2, -1, -1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 2, 2, 3 };
        Adjustments.IsolatedMidgame += randomAdjustments[Program.Random.Next(16)];
        Adjustments.IsolatedEndgame += randomAdjustments[Program.Random.Next(16)];
        Adjustments.BackwardMidgame += randomAdjustments[Program.Random.Next(16)];
        Adjustments.BackwardEndgame += randomAdjustments[Program.Random.Next(16)];
        Adjustments.WeakExposedMidgame += randomAdjustments[Program.Random.Next(16)];
        Adjustments.WeakExposedEndgame += randomAdjustments[Program.Random.Next(16)];
        Adjustments.DoubledMidgame += randomAdjustments[Program.Random.Next(16)];
        Adjustments.DoubledEndgame += randomAdjustments[Program.Random.Next(16)];
        Adjustments.PassedMidgame += randomAdjustments[Program.Random.Next(16)];
        Adjustments.PassedEndgame += randomAdjustments[Program.Random.Next(16)];
      }
    }
    #endregion


    // *** EVENT HANDLERS *** //

    #region ReleaseMemoryAllocations
    public override void ReleaseMemoryAllocations()
    {
      base.ReleaseMemoryAllocations();
      pawnHashTable = null;
      player0pawns = null;
      player1pawns = null;
      player0backPawn = null;
      player1backPawn = null;
      passedPawnBonusByRank = null;
      pawnDirectionByPlayer = null;
      sidewaysDirectionNumbers = null;
      pawnSupportDirectionNumbers = null;
    }
    #endregion

    #region AdjustEvaluation
    public override void AdjustEvaluation(ref int midgameEval, ref int endgameEval)
    {
      // *** CHECK PAWN STRUCTURE HASH TABLE *** //

      if (pawnHashTable == null)
      {
        pawnHashTable = new PawnHashEntry[65536]; // 2^16 slots (4 MB)
        for (int x = 0; x < 65536; x++)
          pawnHashTable[x].PassedPawns = new BitBoard(board.NumSquares);
      }

      game.Statistics.PawnHashLookups++;
      int slot = (int)(game.Board.PawnHashCode & 0x000000000000FFFFUL);
      if (pawnHashTable[slot].HashCode == game.Board.PawnHashCode)
      {
        midgameEval += pawnHashTable[slot].MidgameAdjustment;
        endgameEval += pawnHashTable[slot].EndgameAdjustment;
        if (PassedPawnEvaluation == true)
          EvaluatePassedPawns(pawnHashTable[slot].PassedPawns, ref midgameEval, ref endgameEval);
        chessGame.PawnStructureInfo = pawnHashTable[slot];
        game.Statistics.PawnHashHits++;
        return;
      }


      // *** DETERMINE BASIC PAWN INFO *** //

      int midgameAdjustment = 0;
      int endgameAdjustment = 0;

      //  reset information
      pawnHashTable[slot].PassedPawns.Clear();
      for (int file = 0; file < board.NumFiles + 2; file++)
      {
        player0pawns[file] = 0;
        player1pawns[file] = 0;
        player0backPawn[file] = 15;
        player1backPawn[file] = 0;
      }

      //  loop through player 0's pawns for each pawn type
      BitBoard p0pawns = new BitBoard(board.NumSquares);
      foreach (int pawnTypeNumber in pawnTypeNumbers)
      {
        BitBoard typePawns = board.GetPieceTypeBitboard(0, pawnTypeNumber);
        p0pawns = p0pawns | typePawns;
      }
      while (p0pawns.BitCount > 0)
      {
        int square = p0pawns.ExtractLSB();
        int file = board.GetFile(square);
        int rank = board.GetRank(square);
        player0pawns[file + 1]++;
        if (rank < player0backPawn[file + 1])
          player0backPawn[file + 1] = rank;
      }

      //  loop through player 1's pawns for each pawn type
      BitBoard p1pawns = new BitBoard(board.NumSquares);
      foreach (int pawnTypeNumber in pawnTypeNumbers)
      {
        BitBoard typePawns = board.GetPieceTypeBitboard(1, pawnTypeNumber);
        p1pawns = p1pawns | typePawns;
      }
      while (p1pawns.BitCount > 0)
      {
        int square = p1pawns.ExtractLSB();
        int file = board.GetFile(square);
        int rank = board.GetRank(square);
        player1pawns[file + 1]++;
        if (rank > player1backPawn[file + 1])
          player1backPawn[file + 1] = rank;
      }

      //	Check for cylindrical board.  If we are playing with a 
      //	cylindrical board, we need to consider the edges connected.
      if (board is Boards.CylindricalBoard)
      {
        player0pawns[0] = player0pawns[board.NumFiles];
        player0backPawn[0] = player0backPawn[board.NumFiles];
        player0pawns[board.NumFiles + 1] = player0pawns[1];
        player0backPawn[board.NumFiles + 1] = player0backPawn[1];
        player1pawns[0] = player1pawns[board.NumFiles];
        player1backPawn[0] = player1backPawn[board.NumFiles];
        player1pawns[board.NumFiles + 1] = player1pawns[1];
        player1backPawn[board.NumFiles + 1] = player1backPawn[1];
      }


      // *** APPLY THIS INFO TO EACH PAWN TO DETERMINE STATUS *** //

      //  loop through player 0's pawns
      p0pawns = new BitBoard(board.NumSquares);
      foreach (int pawnTypeNumber in pawnTypeNumbers)
      {
        BitBoard typePawns = board.GetPieceTypeBitboard(0, pawnTypeNumber);
        p0pawns = p0pawns | typePawns;
      }
      while (p0pawns)
      {
        int square = p0pawns.ExtractLSB();
        int pawnFile = board.GetFile(square);
        int pawnRank = board.GetRank(square);
        bool isolated = false;
        bool backward = false;

        if (player0pawns[pawnFile] == 0 &&
    player0pawns[pawnFile + 2] == 0)
        {
          //	isolated pawn
          midgameAdjustment -= Adjustments.IsolatedMidgame;
          endgameAdjustment -= Adjustments.IsolatedEndgame;
          isolated = true;
        }
        if (player0backPawn[pawnFile] > pawnRank &&
    player0backPawn[pawnFile + 2] > pawnRank)
        {
          //	backward pawn
          midgameAdjustment -= Adjustments.BackwardMidgame;
          endgameAdjustment -= Adjustments.BackwardEndgame;
          backward = true;
        }
        if (player1pawns[pawnFile + 1] == 0)
        {
          //	penalize weak, exposed pawns
          if (backward)
          {
            midgameAdjustment -= Adjustments.WeakExposedMidgame;
            endgameAdjustment -= Adjustments.WeakExposedEndgame;
          }
          if (isolated)
          {
            midgameAdjustment -= Adjustments.WeakExposedMidgame;
            endgameAdjustment -= Adjustments.WeakExposedEndgame;
          }
        }
        if (player0backPawn[pawnFile + 1] < pawnRank)
        {
          //	doubled, trippled, etc.
          midgameAdjustment -= Adjustments.DoubledMidgame;
          endgameAdjustment -= Adjustments.DoubledEndgame;
        }
        if (pawnRank >= player1backPawn[pawnFile + 1] &&
    pawnRank >= player1backPawn[pawnFile] &&
    pawnRank >= player1backPawn[pawnFile + 2] &&
  (player0pawns[pawnFile + 1] == 1 ||
   pawnRank > player0backPawn[pawnFile + 1]))
        {
          //	passed pawn
          pawnHashTable[slot].PassedPawns.SetBit(square);
          //					midgameAdjustment += Adjustments.PassedMidgame;
          //					endgameAdjustment += Adjustments.PassedEndgame;
          //					if( !isolated )
          //					{
          //						midgameAdjustment += Adjustments.PassedNotIsolatedMidgame;
          //						endgameAdjustment += Adjustments.PassedNotIsolatedEndgame;
          //					}
        }
      }

      //  loop through player 1's pawns
      p1pawns = new BitBoard(board.NumSquares);
      foreach (int pawnTypeNumber in pawnTypeNumbers)
      {
        BitBoard typePawns = board.GetPieceTypeBitboard(1, pawnTypeNumber);
        p1pawns = p1pawns | typePawns;
      }
      while (p1pawns)
      {
        int square = p1pawns.ExtractLSB();
        int pawnFile = board.GetFile(square);
        int pawnRank = board.GetRank(square);

        bool isolated = false;
        bool backward = false;

        if (player1pawns[pawnFile] == 0 &&
    player1pawns[pawnFile + 2] == 0)
        {
          //	isolated pawn
          midgameAdjustment += Adjustments.IsolatedMidgame;
          endgameAdjustment += Adjustments.IsolatedEndgame;
          isolated = true;
        }
        if (player1backPawn[pawnFile] < pawnRank &&
    player1backPawn[pawnFile + 2] < pawnRank)
        {
          //	backward pawn
          midgameAdjustment += Adjustments.BackwardMidgame;
          endgameAdjustment += Adjustments.BackwardEndgame;
          backward = true;
        }
        if (player0pawns[pawnFile + 1] == 0)
        {
          //	penalize weak, exposed pawns
          if (backward)
          {
            midgameAdjustment += Adjustments.WeakExposedMidgame;
            endgameAdjustment += Adjustments.WeakExposedEndgame;
          }
          if (isolated)
          {
            midgameAdjustment += Adjustments.WeakExposedMidgame;
            endgameAdjustment += Adjustments.WeakExposedEndgame;
          }
        }
        if (player1backPawn[pawnFile + 1] > pawnRank)
        {
          //	doubled, trippled, etc.
          midgameAdjustment += Adjustments.DoubledMidgame;
          endgameAdjustment += Adjustments.DoubledEndgame;
        }
        if (pawnRank <= player0backPawn[pawnFile + 1] &&
    pawnRank <= player0backPawn[pawnFile] &&
    pawnRank <= player0backPawn[pawnFile + 2] &&
  (player1pawns[pawnFile + 1] == 1 ||
   pawnRank < player1backPawn[pawnFile + 1]))
        {
          //	passed pawn
          pawnHashTable[slot].PassedPawns.SetBit(square);
          //					midgameAdjustment -= Adjustments.PassedMidgame;
          //					endgameAdjustment -= Adjustments.PassedEndgame;
          //					if( !isolated )
          //					{
          //						midgameAdjustment -= Adjustments.PassedNotIsolatedMidgame;
          //						endgameAdjustment -= Adjustments.PassedNotIsolatedEndgame;
          //					}
        }
      }
      if (PassedPawnEvaluation == true)
        EvaluatePassedPawns(pawnHashTable[slot].PassedPawns, ref midgameEval, ref endgameEval);

      //	store this information in the pawn hash table
      pawnHashTable[slot].HashCode = board.PawnHashCode;
      pawnHashTable[slot].MidgameAdjustment = midgameAdjustment;
      pawnHashTable[slot].EndgameAdjustment = endgameAdjustment;
      for (int file = 0; file < board.NumFiles; file++)
      {
        pawnHashTable[slot].SetBackPawnRank(0, file, player0backPawn[file + 1]);
        pawnHashTable[slot].SetBackPawnRank(1, file, player1backPawn[file + 1]);
      }
      chessGame.PawnStructureInfo = pawnHashTable[slot];

      //	adjust the actual evaluations accordingly
      midgameEval += midgameAdjustment;
      endgameEval += endgameAdjustment;
    }
    #endregion

    #region GetNotesForPieceType
    public override void GetNotesForPieceType(PieceType type, List<string> notes)
    {
      if (pawnTypeNumbers.Contains(type.TypeNumber))
        notes.Add("pawn structure evaluation");
    }
    #endregion


    // *** HELPER FUNCTIONS *** //

    #region EvaluatePassedPawns
    protected void EvaluatePassedPawns
      (BitBoard passedPawns,
        ref int midgameEval,
        ref int endgameEval)
    {
      while (passedPawns)
      {
        int square = passedPawns.ExtractLSB();
        int player = board[square].Player;
        int rank = board.GetRank(board.PlayerSquare(player, square));
        //	the starting bonus, based on rank
        int mgBonus = passedPawnBonusByRank[rank];
        int egBonus = mgBonus;
        //	in endgame, adjust bonus based on distance to kings
        if (kingTypeNumber >= 0)
        {
          int blockSquare = board.NextSquare(pawnDirectionByPlayer[player], square);
          int distanceToPromotion = PawnPromotionRank - rank;
          if (distanceToPromotion <= 4)
          {
            int ourKing = board.GetPieceTypeBitboard(player, kingTypeNumber).LSB;
            int theirKing = board.GetPieceTypeBitboard(player ^ 1, kingTypeNumber).LSB;
            int scaleByRank = (5 - distanceToPromotion) * (5 - distanceToPromotion) + 2;
            egBonus += Math.Min(board.GetDistance(blockSquare, theirKing), 5) * scaleByRank * 5;
            egBonus -= Math.Min(board.GetDistance(blockSquare, ourKing), 5) * scaleByRank * 2;
            if (distanceToPromotion > 1)
              egBonus -= Math.Min(board.GetDistance(board.NextSquare(pawnDirectionByPlayer[player], blockSquare), ourKing), 5) * scaleByRank;
          }
        }
        if (player == 0)
        {
          midgameEval += mgBonus;
          endgameEval += egBonus;
        }
        else
        {
          midgameEval -= mgBonus;
          endgameEval -= egBonus;
        }
      }
    }
    #endregion


    // *** PROTECTED DATA MEMBERS *** //

    protected List<int> pawnTypeNumbers;
    protected int kingTypeNumber;
    protected int[] player0pawns;
    protected int[] player1pawns;
    protected int[] player0backPawn;
    protected int[] player1backPawn;
    protected PawnHashEntry[] pawnHashTable;
    protected int[] passedPawnBonusByRank;
    protected int[] pawnDirectionByPlayer;
    protected int[] sidewaysDirectionNumbers;
    protected int[,] pawnSupportDirectionNumbers;
    protected Games.Abstract.GenericChess chessGame;
  }
}
