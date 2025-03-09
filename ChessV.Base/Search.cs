
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
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ChessV
{
  public enum NodeType
  {
    PV,
    All,
    Cut
  }

  public partial class Game
  {
    // *********************************** //
    // ***                             *** //
    // ***       INTERNAL ENGINE       *** //
    // ***                             *** //
    // *********************************** //


    // *** PUBLIC PROPERTIES *** //

    public int TTSizeInMB { get; set; }
    public int Weakening { get; set; }
    public ulong CurrentMaxHistoryScore { get; protected set; }

    #region Think
    public List<Movement> Think(TimeControl timeControl, int multiPV = 1)
    {
      List<Movement> moves;
      thinkStartTime = DateTime.Now;
      this.timeControl = timeControl;
      IsThinking = true;
      abortSearch = false;


      // *** INITIALIZATION *** //

      #region Initialization
      //	clear killer moves
      for (int x = 0; x < MAX_PLY; x++)
      {
        killers1[x] = 0;
        killers2[x] = 0;
      }

      //	clear history counters
      for (int p = 0; p < NumPlayers; p++)
        for (int x = 0; x < NPieceTypes; x++)
          for (int y = 0; y < Board.NumSquaresExtended; y++)
          {
            historyCounters[p, x, y] = 0;
            butterflyCounters[p, x, y] = 1;
          }

      //	clear countermoves
      for (int x = 0; x < Board.NumSquaresExtended; x++)
        for (int y = 0; y < Board.NumSquaresExtended; y++)
          countermoves[x, y] = 0;

      //	reset statistics
      Statistics.Reset();
      Statistics.SearchStartTime = DateTime.Now;
      Statistics.SearchStartTime = DateTime.Now;

      //	create hashtable (transposition table)
      if (hashtable == null)
      {
        hashtable = new Hashtable();
        hashtable.SetSize(TTSizeInMB);
        if (Variation == 0)
          weakeningHashShift = 12;
        else
          weakeningHashShift = 13 + Program.Random.Next(12);
      }

      //	Initialize arrays for collecting PV(s)
      PV[] PVs = new PV[multiPV];
      for (int x = 0; x < multiPV; x++)
      {
        PVs[x] = new PV();
        PVs[x].Initialize();
      }
      int[] pvScores = new int[multiPV];
      Dictionary<UInt32, PV> lookupPVbyExcludedMoves = new Dictionary<uint, PV>();
      searchStack[1].Eval = Evaluate();

      //	If we are in multi-pv mode, we will use this movesToExclude list to 
      //	accumulate the moves selected from previous PVs and exclude them from 
      //	consideration in later PVs.
      List<UInt32> movesToExclude = multiPV > 1 ? new List<UInt32>() : null;
      #endregion


      // *** DETERMINE TIME ALLOCATION *** //

      #region Determine Time Allocation
      maxSearchTime = -1;
      absoluteMaxSearchTime = -1;
      exactMaxTime = -1;
      if (timeControl != null)
      {
        if (timeControl.TimePerMove != 0)
        {
          exactMaxTime = timeControl.TimePerMove;
        }
        else
        {
          if (timeControl.MovesLeft != 0)
          {
            // *** TOURNAMENT TIME CONTROLS *** //

            if (timeControl.MovesLeft == 1)
            {
              maxSearchTime = timeControl.ActiveTimeLeft / 2;
              absoluteMaxSearchTime = Math.Min(timeControl.ActiveTimeLeft / 2, timeControl.ActiveTimeLeft - 500);
            }
            else
            {
              maxSearchTime = timeControl.ActiveTimeLeft / Math.Min(timeControl.MovesLeft, 20);
              absoluteMaxSearchTime = Math.Min(4 * timeControl.ActiveTimeLeft / timeControl.MovesLeft, timeControl.ActiveTimeLeft / 3);
            }
          }
          else
          {
            // *** SUDDEN DEATH TIME CONTROL *** //

            if (timeControl.TimeIncrement > 0)
            {
              maxSearchTime = timeControl.TimeIncrement + timeControl.ActiveTimeLeft /
                Math.Min(StartingPieceCount[0] + StartingPieceCount[1] - 24,
                          Board.GetPlayerPieceBitboard(0).BitCount + Board.GetPlayerPieceBitboard(1).BitCount + 2);
              absoluteMaxSearchTime = Math.Max(timeControl.ActiveTimeLeft / 6, timeControl.TimeIncrement - 100);
            }
            else
            {
              maxSearchTime = timeControl.ActiveTimeLeft /
                Math.Min(StartingPieceCount[0] + StartingPieceCount[1] - 10,
                      Board.GetPlayerPieceBitboard(0).BitCount + Board.GetPlayerPieceBitboard(1).BitCount + 6);
              absoluteMaxSearchTime = timeControl.ActiveTimeLeft / 6;
            }
          }
        }
      }
      #endregion

      try
      {
        int alpha = -INFINITY;
        int beta = INFINITY;
        int score = -INFINITY;
        int delta = -INFINITY;
        int scorePreviousIteration = -INFINITY;
        CurrentMaxHistoryScore = 1;

        // *** ITERATIVE DEEPENING LOOP *** //

        #region Iterative Deepening Loop
        bool deepen = true;
        for (idepth = ONEPLY; deepen && !abortSearch; idepth += ONEPLY)
        {
          if (movesToExclude != null)
            movesToExclude.Clear();

          // *** MULTI-PV LOOP *** //

          #region Multi-PV Loop
          for (int pvIndex = 0; pvIndex < multiPV && !abortSearch; pvIndex++)
          {
            //	Reset aspiration window size
            if (idepth >= 5 * ONEPLY)
            {
              delta = 35;
              alpha = Math.Max(pvScores[pvIndex] - delta, -INFINITY);
              beta = Math.Min(pvScores[pvIndex] + delta, INFINITY);
            }

            UInt32 exclusionKey = 0;
            if (movesToExclude != null)
            {
              //	Determine key based on moves to exclude
              foreach (UInt32 movehash in movesToExclude)
                exclusionKey = exclusionKey ^ (movehash * 17);
              //	Lookup PV in case we've already run an iteration 
              //	with these particular moves excluded before
              if (lookupPVbyExcludedMoves.ContainsKey(exclusionKey))
                lookupPVbyExcludedMoves[exclusionKey].CopyTo(SearchStack[1].PV);
            }

            while (true)
            {
              score = SearchRoot(alpha, beta, idepth, movesToExclude);

              if (abortSearch)
                break;

              if (score <= alpha)
              {
                beta = (alpha + beta) / 2;
                alpha = Math.Max(score - delta, -INFINITY);
              }
              else if (score >= beta)
              {
                alpha = (alpha + beta) / 2;
                beta = Math.Min(score + delta, INFINITY);
              }
              else
                break;

              delta += delta; // delta / 4 + 5;
            }

            pvScores[pvIndex] = score;

            if (!abortSearch)
            {
              //	completed search - update current PV
              SearchStack[1].PV.CopyTo(PVs[pvIndex]);

              if (multiPV > 1)
              {
                movesToExclude.Add(PVs[pvIndex][1]);
                lookupPVbyExcludedMoves[exclusionKey] = new PV();
                lookupPVbyExcludedMoves[exclusionKey].Initialize();
                PVs[pvIndex].CopyTo(lookupPVbyExcludedMoves[exclusionKey]);
              }

              #region Call ThinkingCallback to update display
              if (ThinkingCallback != null)
              {
                Int64 nodesUsed = Statistics.Nodes;
                TimeSpan timeUsed = DateTime.Now - thinkStartTime;
                StringBuilder pv = new StringBuilder(80);
                pv.Append(DescribeMove(searchStack[1].PV[1], MoveNotation.StandardAlgebraic));
                for (int x = 2; searchStack[1].PV[x] != 0; x++)
                {
                  pv.Append(" ");
                  pv.Append(DescribeMove(searchStack[1].PV[x], MoveNotation.StandardAlgebraic));
                }
                Dictionary<string, string> searchinfo = new Dictionary<string, string>();
                searchinfo["Depth"] = (idepth / ONEPLY).ToString();
                searchinfo["Score"] = FormatScoreForDisplay(score);
                searchinfo["Time"] =
                    timeUsed.Hours > 0
                  ? timeUsed.Hours.ToString() + ":" + timeUsed.Minutes.ToString("D2") + ":" + timeUsed.Seconds.ToString("D2")
                  : timeUsed.Minutes.ToString() + ":" + timeUsed.Seconds.ToString("D2");
                searchinfo["TimeXB"] = Convert.ToInt32(timeUsed.TotalMilliseconds * 10.0).ToString(); // time in centiseconds for xboard
                searchinfo["Nodes"] = nodesUsed.ToString("N0");
                searchinfo["NodesXB"] = nodesUsed.ToString(); // nodes with no , separators for xboard
                searchinfo["PV"] = pv.ToString();
                ThinkingCallback(searchinfo);
              }
              #endregion
            }
            else
              //	search aborted due to timeout or node limit reached
              deepen = false;
          }
          #endregion

          #region Track recent best move history
          if (idepth / ONEPLY <= 5)
            previousBestMoves[idepth / ONEPLY - 1] = PVs[0][1];
          else
          {
            for (int x = 0; x < 4; x++)
              previousBestMoves[x] = previousBestMoves[x + 1];
            previousBestMoves[4] = PVs[0][1];
          }
          #endregion

          #region Determine whether to perform another iteration
          //	handle "Quick Hint" - timeControl is NULL, we search to 4 ply
          if (timeControl == null)
            deepen = idepth <= 3 * ONEPLY;
          else
          {
            deepen = (idepth / ONEPLY) < MAX_PLY - 1;
            if (!timeControl.Infinite)
            {
              long timeUsed = (long)(DateTime.Now - thinkStartTime).TotalMilliseconds;
              if (exactMaxTime > 0)
              {
                if (timeUsed > exactMaxTime * 2 / 3)
                  deepen = false;
              }
              else
              {
                int timeUseAgressiveness = (timeControl.TimeIncrement > 0 ? 36 : 34);
                //	if the score has dropped a lot since last iteration, use time more agressively
                if (score < scorePreviousIteration - 150)
                  timeUseAgressiveness += 8;
                //	if we are ahead of our opponent (in an absolute sense) use time less agressively
                else if (score > 150)
                  timeUseAgressiveness -= Math.Min((score - 150) / 10, 8);
                //	use time more agressively if the best move has been changing over recent iterations
                uint bestMove = PVs[0][1];
                for (int x = Math.Min(idepth / ONEPLY - 2, 3); x >= 0; x--)
                  if (previousBestMoves[x] != bestMove)
                  {
                    timeUseAgressiveness = timeUseAgressiveness * 6 / 5;
                    bestMove = previousBestMoves[x];
                  }
                //	now that we have decided how agressively to use time, 
                //	decide if we should perform another iteration
                if (timeUsed > maxSearchTime * timeUseAgressiveness / 120 || timeUsed > absoluteMaxSearchTime)
                  deepen = false;
              }
            }
            //	handle fixed depth search
            if (timeControl.PlyLimit > 0 && deepen)
              deepen = idepth < timeControl.PlyLimit * ONEPLY;
            //	handle fixed node search
            else if (timeControl.NodeLimit > 0 && deepen)
              deepen = Statistics.Nodes * 2 < timeControl.NodeLimit;
          }
          #endregion

          scorePreviousIteration = score;
        }
        #endregion

        hashtable.NextGeneration();

        //	Add all moves for the current player from the PV to a 
        //	Movement list and return.  Normally there will be only one 
        //	move, but there could be more for multi-move variants.
        moves = new List<Movement>();
        for (int x = 1; PVs[0][x] != 0 && Movement.GetPlayerFromHash(PVs[0][x]) == CurrentSide; x++)
          moves.Add(new Movement(PVs[0][x]));
      }
      catch (Exception ex)
      {
        IsThinking = false;
        throw new Exceptions.ChessVException(this, ex);
      }
      IsThinking = false;
      return moves;
    }
    #endregion

    #region SearchRoot
    public int SearchRoot(int alpha, int beta, int depth, List<UInt32> movesToExclude = null)
    {
      //	track counts of moves executed:
      int moveNumber = 0; // count of all moves
      int normalMoveCount = 0; // count of non-captures

      //	positional extension (typically check extension)
      int extension = getExtension(1);
      if (depth < ONEPLY)
        depth += extension;

      //	track number of nodes per move for better move ordering
      Dictionary<UInt32, int> nodesPerMove = new Dictionary<UInt32, int>();

      //	no need to regenerate moves at the root - just restart the move selection
      moveLists[1].Restart(movesToExclude != null ? 0 : searchStack[1].PV[1]);

      // *** MOVE LOOP *** //
      int movingSide = CurrentSide;
      int originalAlpha = alpha;
      while (!abortSearch && moveLists[1].MakeNextMove())
      {
        Statistics.Nodes++;
        MoveInfo currentMove = moveLists[1].CurrentMove;
        //	Check to see if this is an excluded move.  If it is, just 
        //	undo and continue.  The excluded moves are used for multi-pv
        if (movesToExclude != null && movesToExclude.Contains(currentMove))
        {
          moveLists[1].UnmakeMove();
          nodesPerMove[currentMove.Hash] = nodesPerMove.GetValueOrDefault(currentMove.Hash);
          // TODO(chesslogic): why is this recording an identical ToSquare, FromSquare, and PromotionType?
          continue;
        }
        //	Store current node count so we can determine how many total 
        //	nodes were used during consideration of this move.  We will 
        //	use the info for better move ordering on the next iteration.
        long startNodeCount = Statistics.Nodes;
        SearchPath[1] = currentMove;
        if ((currentMove.MoveType & MoveType.CaptureProperty) == 0)
          normalMoveCount++;
        int score;
        if (moveNumber < 1)
        {
          if (CurrentSide != movingSide)
            score = -SearchPV(-beta, -alpha, depth - ONEPLY, 2);
          else
            score = SearchPV(alpha, beta, depth - ONEPLY, 2);
        }
        else
        {
          //	Late Move Reductions
          int reduction = depth >= 2 * ONEPLY && moveNumber > 4 && normalMoveCount > 1 && currentMove.MoveType == MoveType.StandardMove && extension == 0 ?
            (Math.Min(Math.Max(depth - 2, 0) / 4, Math.Max(moveNumber - 4, 0) / 3) + Math.Min(Math.Max(depth - 2, 0) / 5, Math.Max(moveNumber - 2, 0) * 2 / 3) + (moveNumber / 16) + 0) : 0;

          if (reduction > 0 && Weakening > 0)
            reduction += Math.Min((moveNumber - 3) / 4, Weakening / 4 + 1);
          if (CurrentSide != movingSide)
            score = -Search(-alpha, depth - ONEPLY - reduction, 2, true, NodeType.Cut);
          else
            score = Search(beta, depth - ONEPLY - reduction, 2, true, NodeType.Cut);
          if (reduction > 0 && score > alpha)
            //	re-search without reduction
            if (CurrentSide != movingSide)
              score = -Search(-alpha, depth - ONEPLY, 2, true, NodeType.Cut);
            else
              score = Search(beta, depth - ONEPLY, 2, true, NodeType.Cut);
          if (score > alpha)
            //	we found a new best move and have a new PV
            if (CurrentSide != movingSide)
              score = -SearchPV(-beta, -alpha, depth - ONEPLY, 2);
            else
              score = SearchPV(alpha, beta, depth - ONEPLY, 2);
        }
        moveLists[1].UnmakeMove();

        //	did we run out of time?
        if (abortSearch)
          break;

        //	store the number of nodes considered for this move
        nodesPerMove[currentMove.Hash] = Math.Max(nodesPerMove.GetValueOrDefault(currentMove.Hash), (int)((Statistics.Nodes - startNodeCount) / depth));
        // TODO(chesslogic): why is this recording an identical ToSquare, FromSquare, and PromotionType?

        //	if this move is better than alpha, we have a new PV
        if (score > alpha)
        {
          alpha = score;
          updatePV(1);
        }

        moveNumber++;
      }

      //	update move evaluations for better ordering on the next iteration
      if (!abortSearch && depth >= 3 * ONEPLY && (movesToExclude == null || movesToExclude.Count == 0))
        moveLists[1].ReorderMoves(nodesPerMove);

      return alpha;
    }
    #endregion

    #region SearchPV
    public int SearchPV(int alpha, int beta, int depth, int ply)
    {
      //	test for end-of-game
      MoveEventResponse response = TestForWinLossDraw(CurrentSide);
      if (response != MoveEventResponse.NotHandled)
      {
        if (response == MoveEventResponse.GameDrawn)
          return 0;
        if (response == MoveEventResponse.GameWon)
          return INFINITY - ply;
        if (response == MoveEventResponse.GameLost)
          return -INFINITY + ply;
      }

      //	bookkeeping, time check, etc., every 1024 nodes
      if (Statistics.Nodes % 1024 == 0)
      {
        doBookkeeping();
        if (abortSearch)
          return 0;
      }

      //	positional extension (typically check extension only)
      int extension = getExtension(ply);
      if (depth < ONEPLY || ply < idepth * 2)
        depth += extension;

      //	enter q-search when out of depth
      if (depth < ONEPLY)
        return QSearch(alpha, beta, 0, ply);

      searchStack[ply].PV[ply] = 0;
      searchStack[ply + 1].PV[ply] = 0;

      // *** TRANSPOSITION TABLE CHECK *** //
      TTHashEntry hash = new TTHashEntry();
      UInt32 hashtableMove = 0;
      if (hashtable.Lookup(GetPositionHashCode(ply), ref hash))
      {
        //	If this position has been stored in the hashtable with a 
        //	preferred move, we'll go ahead and try that move first.
        //	Regardless of the depth associated with the stored hash, 
        //	this is still the best information available about best move.
        hashtableMove = hash.MoveHash;
        //	At a PV node, besides potentially offering a move to be 
        //	searched first, the hashtable entry is only good if the 
        //	stored hash is of type Exact and the stored depth is 
        //	greater that or equal to what we need.  (In practice, 
        //	this almost never happens.)
        if (hash.Depth >= depth && hash.Type == TTHashEntry.HashType.Exact)
        {
          if (hash.Score >= beta)
            saveKiller(ply, hash.MoveHash);
          return scoreFromHashtable(hash.Score, ply);
        }
      }

      int eval = Evaluate();
      searchStack[ply].Eval = eval;

      //	at max depth?
      if (depth == MAX_PLY - 1)
        return eval;

      // *** FUTILITY PRUNING - CHILD NODE *** //
      bool improving = ply < 3 || searchStack[ply].Eval > searchStack[ply - 2].Eval;
      if (depth < 4 * ONEPLY &&
        eval < INFINITY - MAX_PLY &&
        eval - futilityMargin(depth, improving) >= beta)
        return eval;

      // *** INTERNAL ITERATIVE DEEPENING *** //
      bool useIID =
        /* we have no move from the hashtable ... */ Movement.GetMoveTypeFromHash(hashtableMove) == MoveType.Invalid &&
        /* ... and we have at least 5 ply remaining */ depth >= 5 * ONEPLY &&
        /* ... and we are not in check */ extension == 0;
      if (useIID)
      {
        SearchPV(alpha, beta, depth - 2 * ONEPLY, ply);
        hashtableMove = SearchStack[ply].PV[ply];
      }

      //	generate moves
      generateMoves(CurrentSide, ply, hashtableMove);

      // *** MOVE LOOP *** //
      int score = -INFINITY;
      int bestScore = -INFINITY;
      int originalAlpha = alpha;
      int moveNumber = 0;
      int normalMoveCount = 0; // non-captures
      int movingSide = CurrentSide;
      while (alpha < beta && moveLists[ply].MakeNextMove())
      {
        Statistics.Nodes++;
        MoveInfo currentMove = moveLists[ply].CurrentMove;

        //	Weakening - if enabled, handle moves we are "blind" to
        if (Weakening > 0)
          if ((int)((Board.HashCode & (0xFFUL << weakeningHashShift)) >> weakeningHashShift) < Weakening * 2)
          {
            moveLists[ply].UnmakeMove();
            continue;
          }

        SearchPath[ply] = currentMove;
        if (currentMove.MoveType == MoveType.StandardMove)
          normalMoveCount++;

        if (moveNumber == 0)
        {
          //	FIRST MOVE - SEARCH AT FULL WIDTH  //

          if (CurrentSide != movingSide)
            score = -SearchPV(-beta, -alpha, depth - ONEPLY, ply + 1);
          else
            score = SearchPV(alpha, beta, depth - ONEPLY, ply + 1);
          //	undo the move
          moveLists[ply].UnmakeMove();
        }
        else
        {
          //	ALL MOVES AFTER FIRST ARE SEARCHED WITH ZERO WIDTH  //

          //	Late Move Reductions (LMR) - calculate reduction (if any)
          bool reduce =
            /* leave at least 1 full ply and ... */ depth >= 2 * ONEPLY &&
            /* ... we have searched at least 5 moves */ moveNumber > 5 &&
            /* ... we have searched at least 1 normal move */ normalMoveCount > 1 &&
            /* ... is a normal move (not capture or promotion) */ currentMove.MoveType == MoveType.StandardMove &&
            /* ... we are not in check (or other extension) */ extension == 0 &&
            /* ... this is not a killer move */ //currentMove != killers1[ply] && currentMove != killers2[ply] &&
            /* ... this is not the recorded counter-move */ currentMove != countermoves[SearchPath[ply - 1].FromSquare, SearchPath[ply - 1].ToSquare];
          int reduction = reduce
            ? (Math.Min(Math.Max(depth - 2, 0) / 4, Math.Max(moveNumber - 4, 0) / 3) +
               Math.Min(Math.Max(depth - 2, 0) / 5, Math.Max(moveNumber - 2, 0) * 2 / 3) + (moveNumber / 16))
            : 0;
          //	decrease reduction for moves that escape a capture
          //					if( currentMove.MoveType == MoveType.StandardMove && reduction >= 2 &&
          //						SEE( currentMove.ToSquare, currentMove.FromSquare, 80 ) )
          //						reduction -= 2;

          //	less or no reduction for historically successful moves
          if (reduction > 0)
          {
            ulong x = historyCounters[currentMove.Player, currentMove.PieceMoved.TypeNumber, currentMove.ToSquare] /
              Math.Max(butterflyCounters[currentMove.Player, currentMove.PieceMoved.TypeNumber, currentMove.ToSquare], 1);
            if (x > 0)
            {
              reduction--;
              if (x > CurrentMaxHistoryScore / (uint)(idepth / ONEPLY))
                reduction = 0;
            }
          }

          if (reduction > 0 && Weakening > 0)
            reduction += Math.Min((moveNumber - 3) / 4, Weakening / 4 + 1);

          int reducedDepth = reduction > 0 ? Math.Max(depth - ONEPLY - reduction, ONEPLY) : depth - ONEPLY;
          int actualReduction = depth - ONEPLY - reducedDepth;
          //	After the first, all moves are searched with zero-width.  The idea 
          //	here is that we only need to prove that they are worse than the move 
          //	we already searched.  If we are incorrect about this, we'll need 
          //	to repeat the search with a full window.
          if (CurrentSide != movingSide)
            score = -Search(-alpha, reducedDepth, ply + 1, true, NodeType.Cut);
          else
            score = Search(beta, reducedDepth, ply + 1, true, NodeType.Cut);

          if (actualReduction > 0 && score > alpha)
            //	The move appears to be better than the one we already searched, but 
            //	we're not sure becuase we reduced.  Re-search without reduction.
            if (CurrentSide != movingSide)
              score = -Search(-alpha, depth - ONEPLY, ply + 1, true, NodeType.Cut);
            else
              score = Search(beta, depth - ONEPLY, ply + 1, true, NodeType.Cut);

          if (score > alpha && score < beta)
            //	Our zero-window search was incorrect - the fact that the value 
            //	of alpha has increased means the first move was not the best, 
            //	we have a new PV, and need to re-search the move with full alpha-beta.
            if (CurrentSide != movingSide)
              score = -SearchPV(-beta, -alpha, depth - ONEPLY, ply + 1);
            else
              score = SearchPV(alpha, beta, depth - ONEPLY, ply + 1);

          //					if( currentMove.MoveType == MoveType.StandardMove && depth > 2*ONEPLY && score < beta )
          //						butterflyCounters[currentMove.Player, currentMove.PieceMoved.TypeNumber, currentMove.ToSquare] += (ushort) (depth / ONEPLY);

          //	undo the move
          moveLists[ply].UnmakeMove();
        }

        if (abortSearch)
          //	we ran out of time and can't trust the results of this search
          return 0;

        if (score > bestScore)
        {
          bestScore = score;
          if (score > alpha)
          {
            alpha = score;
            updatePV(ply);

            if (score >= beta)
            {
              //	update killer moves
              saveKiller(ply, ref currentMove);
              //	update history counters
              if (depth > 2 * ONEPLY)
                updateHistoryCounters(depth, ref currentMove);
              //	update countermove
              if (ply > 1)
                countermoves[SearchPath[ply - 1].FromSquare, SearchPath[ply - 1].ToSquare] = currentMove.Hash;
            }
          }
        }

        moveNumber++;
      }

      if (moveNumber == 0)
      {
        //	If we found no legal moves, call NoMovesResult which will 
        //	in turn call the NoMovesResult for each rule object until 
        //	one of them returns a result (typically the CheckmateRule.)
        MoveEventResponse result = NoMovesResult(CurrentSide);
        if (result == MoveEventResponse.GameWon)
          return INFINITY - ply;
        if (result == MoveEventResponse.GameLost)
          return -INFINITY + ply;
        return 0;
      }

      //	Update the Transposition Table
      if (bestScore < originalAlpha)
        hashtable.Store(GetPositionHashCode(ply), scoreToHashtable(bestScore, ply), depth, 0, TTHashEntry.HashType.UpperBound);
      else if (bestScore >= beta)
        hashtable.Store(GetPositionHashCode(ply), scoreToHashtable(bestScore, ply), depth, searchStack[ply].PV[ply], TTHashEntry.HashType.LowerBound);
      else
        hashtable.Store(GetPositionHashCode(ply), scoreToHashtable(bestScore, ply), depth, searchStack[ply].PV[ply], TTHashEntry.HashType.Exact);

      return bestScore;
    }
    #endregion

    #region Search
    public int Search(int beta, int depth, int ply, bool tryNullMove, NodeType nodeType)
    {
      //	test for end-of-game
      MoveEventResponse response = TestForWinLossDraw(CurrentSide);
      if (response != MoveEventResponse.NotHandled)
      {
        if (response == MoveEventResponse.GameDrawn)
          return 0;
        if (response == MoveEventResponse.GameWon)
          return INFINITY - ply;
        if (response == MoveEventResponse.GameLost)
          return -INFINITY + ply;
      }

      //	bookkeeping, time check, etc., every 1024 nodes
      if (Statistics.Nodes % 1024 == 0)
      {
        doBookkeeping();
        if (abortSearch)
          return 0;
      }

      //	positional extension (typically check extension only)
      int extension = 0;
      if (depth < ONEPLY || ply < idepth * 2)
      {
        extension = getExtension(ply);
        depth += extension;
      }

      //	enter q-search when out of depth
      if (depth < ONEPLY)
        return QSearch(beta - 1, beta, 0, ply);

      searchStack[ply].PV[ply] = 0;
      searchStack[ply + 1].PV[ply] = 0;

      // *** TRANSPOSITION TABLE CHECK *** //
      TTHashEntry hash = new TTHashEntry();
      UInt32 hashtableMove = 0;
      if (hashtable.Lookup(GetPositionHashCode(ply), ref hash))
      {
        hashtableMove = hash.MoveHash;
        if ((hash.Depth >= depth ||
           hash.Score >= Math.Max(INFINITY - 100, beta) ||
           hash.Score < Math.Min(-INFINITY + 100, beta)) &&
          ((hash.Type == TTHashEntry.HashType.LowerBound && hash.Score >= beta) ||
           (hash.Type == TTHashEntry.HashType.UpperBound && hash.Score < beta) ||
            hash.Type == TTHashEntry.HashType.Exact))
        {
          if (hash.Score >= beta)
          {
            //	update killer moves
            saveKiller(ply, hash.MoveHash);
            //	update history counters
            if (depth > 2 * ONEPLY)
              updateHistoryCounters(depth, hash.MoveHash);
            //	update countermove
            if (ply > 1)
              countermoves[SearchPath[ply - 1].FromSquare, SearchPath[ply - 1].ToSquare] = hash.MoveHash;
            //	update PV
            updatePV(ply);
          }
          return scoreFromHashtable(hash.Score, ply);
        }
      }

      int eval = Evaluate();
      searchStack[ply].Eval = eval;

      //	at max depth?
      if (depth == MAX_PLY - 1)
        return eval;

      // *** RAZORING *** //
      if (depth < 3 * ONEPLY + Weakening / 5 &&
        // hashtableMove == 0 && 
        extension == 0 &&
        eval + razorMargin[depth / ONEPLY] - ((Weakening / 3) * (24 - (depth * 2))) <= beta - 1)
      {
        if (depth <= ONEPLY)
          return QSearch(beta - 1, beta, 0, ply);

        int rAlpha = beta - razorMargin[depth / ONEPLY] - 1;
        int val = QSearch(rAlpha, rAlpha + 1, 0, ply);
        if (val <= rAlpha)
          return val;
      }

      // *** FUTILITY PRUNING - CHILD NODE *** //
      bool improving = ply < 3 || searchStack[ply].Eval > searchStack[ply - 2].Eval;
      if (depth < 4 * ONEPLY &&
        eval < INFINITY - MAX_PLY &&
        eval - futilityMargin(depth, improving) >= beta)
        return eval;

      // *** NULL MOVE *** //

      bool nullMoveMatesUs = false;
      //	we will disable the null move if we are too close to the end 
      //	to prevent serious mistakes in zugzuang positions, or if we 
      //	are extending (meaning we're probably in check)
      tryNullMove &= extension == 0 &&
        Board.GetPlayerPieceBitboard(0).BitCount >= 2 &&
        Board.GetPlayerPieceBitboard(1).BitCount >= 2 &&
        Board.GetPlayerPieceBitboard(0).BitCount + Board.GetPlayerPieceBitboard(1).BitCount >= 5 &&
        beta < INFINITY - 100 && beta > -INFINITY + 100;
      //	determine size of reduction depending on remaining depth
      int nullReduction = (depth / ONEPLY >= 7 ? 3 : 2) * ONEPLY;
      //	if we have the depth remaining and are close enough to beta, make the null move
      if (tryNullMove && depth >= nullReduction + ONEPLY + ONEPLY &&
        Board.GetMidgameMaterialEval() + (nodeType == NodeType.All ? 350 : 500) > beta)
      {
        int nullMoveSide = CurrentSide;
        //	make the null move
        moveLists[ply].MakeNullMove();
        SearchPath[ply] = new Movement(0, 0, 0, MoveType.NullMove);
        //	search with reduced depth
        int nullScore;
        if (CurrentSide != nullMoveSide)
          nullScore = -Search(-(beta - 1), depth - ONEPLY - nullReduction, ply + 1, false, nodeType);
        else
          //	this alternative exists for games with multiple moves such as Marseillais Chess
          nullScore = Search(beta, depth - ONEPLY - nullReduction, ply + 1, false, nodeType);
        //	take back the null move
        moveLists[ply].UnmakeNullMove();
        //	If null score exceeded beta, we'll quit searching and just return 
        //	beta.  The thinking is that if the current position is so strong that 
        //	we can do nothing and it is still good enough to generate a beta 
        //	cutoff, then there is no point in searching because surely there is 
        //	some move that is better than doing nothing which would also generate 
        //	beta cut-off.  In zugzuang positions, this assumption is incorrect!
        if (nullScore >= beta)
        {
          //	do not return unproven mate scores
          if (nullScore > INFINITY - MAX_PLY)
            nullScore = beta;
          return nullScore;
        }

        if (nullScore < -INFINITY + MAX_PLY)
          nullMoveMatesUs = true;
      }


      // *** INTERNAL ITERATIVE DEEPENING (deactivated - seems not useful at non-pv node) *** //
      bool useIID = false &&
        /* we have no move from the hashtable and ... */ Movement.GetMoveTypeFromHash(hashtableMove) == MoveType.Invalid &&
        /* ... we have at least 7 ply remaining */ depth >= 7 * ONEPLY &&
        /* ... we are not in check */ extension == 0 &&
        /* ... we're at a likely CUT node */ nodeType == NodeType.Cut &&
        /* ... evaluation is close enough to beta */ eval + (depth * 4) + 100 >= beta;
      if (useIID)
      {
        Search(beta, depth - 4 * ONEPLY, ply, false, NodeType.Cut);
        if (hashtable.Lookup(GetPositionHashCode(ply), ref hash))
          hashtableMove = hash.MoveHash;
      }


      int score = -INFINITY;
      int bestScore = -INFINITY;
      int moveNumber = 0; // count all moves
      int normalMoveCount = 0; // count non-captures

      //	Generate moves
      generateMoves(CurrentSide, ply, hashtableMove);

      //	Eligible for Pruning
      bool pruningEligible =
        /* at a frontier or pre-frontier node and */ depth - ONEPLY < 3 * ONEPLY &&
        /* ... not already in the endgame and */ Board.GetEndgameMaterialEval(CurrentSide) >= 375 &&
        /* ... not in check (or other extension) */ extension == 0;

      // *** MOVE LOOP *** //
      int movingSide = CurrentSide;
      while (score < beta && moveLists[ply].MakeNextMove())
      {
        Statistics.Nodes++;
        MoveInfo currentMove = moveLists[ply].CurrentMove;

        //	Weakening - if enabled, handle moves we are "blind" to
        if (Weakening > 0)
          if ((int)((Board.HashCode & (0xFFUL << weakeningHashShift)) >> weakeningHashShift) < Weakening * 2)
          {
            moveLists[ply].UnmakeMove();
            continue;
          }

        SearchPath[ply] = currentMove;
        if (currentMove.MoveType == MoveType.StandardMove)
          normalMoveCount++;

        // *** PRUNING *** //

        if (pruningEligible)
        {
          bool prune = false;

          //	Fuility Pruning
          if ( /* ... we've searched at least one move */ moveNumber > 1 &&
            /* ... this is a not capture or promotion */ currentMove.MoveType == MoveType.StandardMove &&
            /* ... move leaves us far enough below beta */
            eval + Board.CalculateStandardMovePST(currentMove.FromSquare, currentMove.ToSquare) +
            (depth < 2 * ONEPLY ? (currentMove.PieceMoved.PieceType.IsPawn ? 50 : 30) : 200) < beta)
          {
            //	If we are at a frontier node, we'll go ahead and do an actual eval on the new position
            //	to ensure it is below beta.  (Our evaluation function is remarkably fast since it doesn't
            //	consider very much.)  At a pre-frontier node, we are happy since we are so far below beta.
            int newEval = (depth < 2 * ONEPLY ? Evaluate() : beta - 1);
            if ( /* we are still far enough below beta and ... */ newEval < beta &&
              /* ... not passed pawn push or other restriction */ CanPruneMove(currentMove))
              prune = true;

          }
          //	Late Move Pruning
          if (!prune && depth < 3 * ONEPLY &&
            /* ... we've searched enough moves */ moveNumber > (depth < 2 * ONEPLY ? 14 : 18) + (improving ? 3 : 0) &&
            /* ... this is a not capture or promotion */ currentMove.MoveType == MoveType.StandardMove)
            prune = true;

          //	Perform pruning
          if ( /* if we are eligible for pruning and ... */ prune &&
            /* ... not a passed pawn move or other restriction and */ CanPruneMove(currentMove) &&
            /* ... and make sure this move doesn't give check */ getExtension(ply + 1) == 0)
          {
            moveLists[ply].UnmakeMove();
            continue;
          }
        }

        // *** LATE MOVE REDUCTIONS *** //

        bool reduce =
          /* leave at least 1 full ply and ... */ depth >= 2 * ONEPLY &&
          /* ... we have searched at least 4 moves */ moveNumber > 4 &&
          /* ... we have searched at least 1 normal (non-capture) move */ normalMoveCount > 1 &&
          /* ... is a normal move (not capture or promotion) */ currentMove.MoveType == MoveType.StandardMove &&
          /* ... null move doesn't mate us */ !nullMoveMatesUs &&
          /* ... we are not in check (or other extension) */ extension == 0 &&
          /* ... this is not a killer move */ //currentMove != killers1[ply] && currentMove != killers2[ply] &&
          /* ... this is not the recorded counter-move */ currentMove != countermoves[SearchPath[ply - 1].FromSquare, SearchPath[ply - 1].ToSquare];

        int reduction = !reduce ? 0 :
          //	calculate size of reduction
          (Math.Min(Math.Max(depth - 2, 0) / 3, Math.Max(moveNumber - 2, 0) / 3) +
           Math.Min(Math.Max(depth - 2, 0) / 5, Math.Max(moveNumber - 2, 0) * 2 / 3) +
           (moveNumber / 16) + (nodeType == NodeType.Cut ? 2 : 0) + (improving ? 0 : 1));
        //	decrease reduction for moves that escape a capture
        //				if( nodeType != NodeType.Cut && currentMove.MoveType == MoveType.StandardMove && reduction >= 2 && 
        //					SEE( currentMove.ToSquare, currentMove.FromSquare, 80 ) )
        //					reduction -= 2;

        //	less or no reduction for historically successful moves
        if (reduction > 0)
        {
          ulong x = historyCounters[currentMove.Player, currentMove.PieceMoved.TypeNumber, currentMove.ToSquare] /
            Math.Max(butterflyCounters[currentMove.Player, currentMove.PieceMoved.TypeNumber, currentMove.ToSquare], 1);
          if (x > 0)
          {
            reduction--;
            if (x > CurrentMaxHistoryScore / (uint)(idepth / ONEPLY))
              reduction = 0;
          }
        }

        //	potentially increase reduction if Weakening is in effect
        if (reduction > 0 && Weakening > 0)
          reduction += Math.Min((moveNumber - 3) / 4, Weakening / 4 + 1);

        //	calculate new depth (making sure we never reduce so much that we don't leave one full ply
        int reducedDepth = reduction > 0 ? Math.Max(depth - ONEPLY - reduction, ONEPLY) : depth - ONEPLY;
        int actualReduction = depth - ONEPLY - reducedDepth;

        // *** DEEPEN SEARCH RECURSIVELY *** //

        if (CurrentSide != movingSide)
          score = -Search(-(beta - 1), reducedDepth, ply + 1, true, nodeType == NodeType.Cut ? NodeType.All : NodeType.Cut);
        else
          //	this alternative exists for games with multiple moves such as Marseillais Chess
          score = Search(beta, reducedDepth, ply + 1, true, nodeType == NodeType.Cut ? NodeType.All : NodeType.Cut);

        //	if we reduced and failed high ...
        if (actualReduction > 0 && score >= beta)
          //	re-search without reduction
          if (CurrentSide != movingSide)
            score = -Search(-(beta - 1), depth - ONEPLY, ply + 1, true, nodeType == NodeType.Cut ? NodeType.All : NodeType.Cut);
          else
            score = Search(beta, depth - ONEPLY, ply + 1, true, nodeType == NodeType.Cut ? NodeType.All : NodeType.Cut);

        if (currentMove.MoveType == MoveType.StandardMove && depth > 2 * ONEPLY && score < beta && nodeType == NodeType.Cut)
          //	update butterfly counter of unsuccessful moves
          butterflyCounters[currentMove.Player, currentMove.PieceMoved.TypeNumber, currentMove.ToSquare] += (ushort)(depth / ONEPLY);

        //	Undo the move
        moveLists[ply].UnmakeMove();

        if (abortSearch)
          //	we ran out of time and can't trust the results of this search
          return 0;

        if (score > bestScore)
        {
          bestScore = score;
          if (score >= beta)
          {
            //	update killer moves
            saveKiller(ply, ref currentMove);
            //	update history counters
            if (depth > 2 * ONEPLY)
              updateHistoryCounters(depth, ref currentMove);
            //	update countermove
            if (ply > 1)
              countermoves[SearchPath[ply - 1].FromSquare, SearchPath[ply - 1].ToSquare] = currentMove.Hash;
            //	update PV
            updatePV(ply);
            //	store lower bound in hash table
            hashtable.Store(GetPositionHashCode(ply), scoreToHashtable(score, ply), depth, currentMove.Hash, TTHashEntry.HashType.LowerBound);
          }
          else
            //	store uppder bound in hash table
            hashtable.Store(GetPositionHashCode(ply), scoreToHashtable(score, ply), depth, 0, TTHashEntry.HashType.UpperBound);
        }

        moveNumber++;
      }

      if (moveNumber == 0)
      {
        //	If we found no legal moves, call NoMovesResult which will 
        //	in turn call the NoMovesResult for each rule object until 
        //	one of them returns a result (typically the CheckmateRule.)
        MoveEventResponse result = NoMovesResult(CurrentSide);
        if (result == MoveEventResponse.GameWon)
          return INFINITY - ply;
        if (result == MoveEventResponse.GameLost)
          return -INFINITY + ply;
        return 0;
      }

      return bestScore;
    }
    #endregion

    #region QSearch
    public int QSearch(int alpha, int beta, int depth, int ply, int recaptureSquare = -1)
    {
      //	test for end-of-game
      MoveEventResponse response = TestForWinLossDraw(CurrentSide);
      if (response != MoveEventResponse.NotHandled)
      {
        if (response == MoveEventResponse.GameDrawn)
          return 0;
        if (response == MoveEventResponse.GameWon)
          return INFINITY - ply;
        if (response == MoveEventResponse.GameLost)
          return -INFINITY + ply;
      }

      searchStack[ply].PV[ply] = 0;
      searchStack[ply + 1].PV[ply] = 0;

      // *** TRANSPOSITION TABLE CHECK *** //
      TTHashEntry hash = new TTHashEntry();
      UInt32 hashtableMove = 0;
      if (hashtable.Lookup(GetPositionHashCode(ply), ref hash))
      {
        hashtableMove = hash.MoveHash;
        if ((hash.Type == TTHashEntry.HashType.LowerBound && hash.Score >= beta) ||
          (hash.Type == TTHashEntry.HashType.UpperBound && hash.Score < beta))
        {
          return scoreFromHashtable(hash.Score, ply);
        }
      }

      //	bookkeeping, time check, etc., every 4096 nodes
      if (Statistics.Nodes % 4096 == 0)
      {
        doBookkeeping();
        if (abortSearch)
          return 0;
      }
      bool pvNode = alpha != beta - 1;
      bool inCheck = getExtension(ply) > 0;
      int oldAlpha = alpha;
      int eval = Evaluate();
      //	after 8 plies of QSearch, allow stand pat even if we are 
      //	in check to prevent search explosion in some variants where 
      //	this can be a real problem (like Gross Chess)
      if (inCheck && depth < -8 * ONEPLY)
        eval = -INFINITY;
      int score = eval;


      //	max depth?
      // TODO(chesslogic): I think I can collect the "most steps" seen in a special move this game and limit by that instead of - 1
      if (depth == MAX_PLY - 1)
        return score;

      //	stand pat?
      if (score >= beta)
        return score;

      int bestScore = score;
      if (bestScore > alpha)
        alpha = bestScore;

      //	Generate moves
      generateMoves(CurrentSide, ply, 0, !inCheck);

      // *** MOVE LOOP *** //
      int movingSide = CurrentSide;
      while (alpha < beta && moveLists[ply].MakeNextMove(inCheck ? 0 : /* delta pruning threshold: */ alpha - eval - 50))
      {
        Statistics.Nodes++;
        Statistics.QNodes++;
        MoveInfo currentMove = moveLists[ply].CurrentMove;

        //	After 4 plies of QSearch, consider re-captures only
        if (!inCheck && depth < -4 * ONEPLY && currentMove.ToSquare != recaptureSquare)
        {
          moveLists[ply].UnmakeMove();
          continue;
        }

        //	Weakening - if enabled, handle moves we are "blind" to
        if (Weakening > 0)
          if ((int)((Board.HashCode & (0xFFUL << weakeningHashShift)) >> weakeningHashShift) < Weakening * 2)
          {
            moveLists[ply].UnmakeMove();
            continue;
          }

        SearchPath[ply] = currentMove;

        if (CurrentSide != movingSide)
          score = -QSearch(-beta, -alpha, depth - ONEPLY, ply + 1, currentMove.ToSquare);
        else
          score = QSearch(alpha, beta, depth - ONEPLY, ply + 1, currentMove.ToSquare);
        moveLists[ply].UnmakeMove();

        if (abortSearch)
          //	we ran out of time and can't trust the results of this search
          return 0;

        if (score > bestScore)
        {
          bestScore = score;
          if (score > alpha)
          {
            alpha = score;
            if (pvNode)
              updatePV(ply);
          }
        }
      }

      //	return checkmate score if we are in check and found no escape
      if (inCheck && bestScore == -INFINITY)
        return -INFINITY + ply;

      //	Update Transposition Table
      if (alpha - beta != 1)
      {
        if (bestScore < beta)
        {
          if (bestScore > eval)
            hashtable.Store(GetPositionHashCode(ply), scoreToHashtable(score, ply), 0, 0,
              pvNode && bestScore > oldAlpha ? TTHashEntry.HashType.Exact : TTHashEntry.HashType.UpperBound);
        }
        else
          hashtable.Store(GetPositionHashCode(ply), scoreToHashtable(score, ply), 0, 0, TTHashEntry.HashType.LowerBound);
      }

      return bestScore;
    }
    #endregion

    protected int scoreFromHashtable(int score, int ply)
    {
      if (score >= INFINITY - MAX_PLY)
        return score - ply;
      if (score <= -INFINITY + MAX_PLY)
        return score + ply;
      return score;
    }

    protected int scoreToHashtable(int score, int ply)
    {
      if (score >= INFINITY - MAX_PLY)
        return score + ply;
      if (score <= -INFINITY + MAX_PLY)
        return score - ply;
      return score;
    }

    protected int getExtension(int ply)
    {
      int extension = 0;
      foreach (Rule rule in rulesHandlingSearchExtensions)
        extension += rule.PositionalSearchExtension(CurrentSide, ply);
      return extension > ONEPLY ? ONEPLY : extension;
    }

    protected void saveKiller(int ply, ref MoveInfo move)
    {
      if (move.MoveType == MoveType.StandardMove)
      {
        if (move.Hash != killers1[ply])
        {
          killers2[ply] = killers1[ply];
          killers1[ply] = move.Hash;
        }
      }
    }

    protected void saveKiller(int ply, UInt32 movehash)
    {
      if (Movement.GetMoveTypeFromHash(movehash) == MoveType.StandardMove)
      {
        if (movehash != killers1[ply])
        {
          killers2[ply] = killers1[ply];
          killers1[ply] = movehash;
        }
      }
    }

    protected void updateHistoryCounters(int depth, ref MoveInfo move)
    {
      if (move.MoveType == MoveType.StandardMove)
      {
        historyCounters[move.PieceMoved.Player, move.PieceMoved.TypeNumber, move.ToSquare] +=
          (UInt32)(((depth / ONEPLY) + 2) * ((depth / ONEPLY) + 1));
        CurrentMaxHistoryScore = Math.Max(CurrentMaxHistoryScore, historyCounters[move.PieceMoved.Player, move.PieceMoved.TypeNumber, move.ToSquare] /
          butterflyCounters[move.PieceMoved.Player, move.PieceMoved.TypeNumber, move.ToSquare]);
      }
    }

    protected void updateHistoryCounters(int depth, UInt32 movehash)
    {
      if (Movement.GetMoveTypeFromHash(movehash) != MoveType.StandardMove)
        return;
      int player = Movement.GetPlayerFromHash(movehash);
      Piece piece = Board[Movement.GetFromSquareFromHash(movehash)];
      if (piece == null)
        //	this could happen if the hashtable move isn't valid becase of hash collision
        return;
      int pieceTypeNumber = piece.TypeNumber;
      int toSquare = Movement.GetToSquareFromHash(movehash);
      historyCounters[player, pieceTypeNumber, toSquare] +=
        (UInt32)(((depth / ONEPLY) + 2) * ((depth / ONEPLY) + 1));
      CurrentMaxHistoryScore = Math.Max(CurrentMaxHistoryScore, historyCounters[player, pieceTypeNumber, toSquare] /
        butterflyCounters[player, pieceTypeNumber, toSquare]);
    }

    protected int futilityMargin(int depth, bool improving)
    {
      return improving
        ? 75 * depth / ONEPLY
        : 125 * depth / ONEPLY;
    }

    protected void perft(int ply, int depth, PerftResults results, StreamWriter log)
    {
      if (depth == 0)
      {
        results.Nodes++;

        if (log != null)
        {
          StringBuilder sbr = new StringBuilder(64);
          sbr.Append(DescribeMove(moveLists[1].CurrentMove, MoveNotation.StandardAlgebraic));
          for (int p = 2; p < ply; p++)
          {
            sbr.Append(' ');
            sbr.Append(DescribeMove(moveLists[p].CurrentMove, MoveNotation.StandardAlgebraic));
          }
          log.WriteLine(sbr);
        }

        return;
      }

      generateMoves(CurrentSide, ply, 0);
      while (moveLists[ply].MakeNextMove())
      {
        MoveInfo currentMove = moveLists[ply].CurrentMove;
        Board.Validate();
        if (depth == 1)
        {

          if (currentMove.MoveType == MoveType.StandardCapture)
            results.Captures++;
          else if (currentMove.MoveType == MoveType.Castling)
            results.Castles++;
          else if (currentMove.MoveType == MoveType.EnPassant)
          {
            results.Captures++;
            results.EnPassants++;
          }
        }
        perft(ply + 1, depth - 1, results, log);
        moveLists[ply].UnmakeMove();
        Board.Validate();
      }
    }

    protected void updatePV(int ply)
    {
      int p;
      SearchStack[ply].PV[ply] = moveLists[ply].CurrentMove;
      for (p = ply + 1; searchStack[ply + 1].PV[p] != 0; p++)
        searchStack[ply].PV[p] = searchStack[ply + 1].PV[p];
      searchStack[ply].PV[p] = searchStack[ply + 1].PV[p];
    }

    protected void doBookkeeping()
    {
      //	handle Windows events so the GUI doesn't stall
      Application.DoEvents();

      //	perform time check
      if (timeControl != null && !timeControl.Infinite)
      {
        long timeUsed = (long)(DateTime.Now - thinkStartTime).TotalMilliseconds;
        if ((absoluteMaxSearchTime > 0 && timeUsed > absoluteMaxSearchTime) ||
          (exactMaxTime > 0 && timeUsed > exactMaxTime) ||
          (timeControl.NodeLimit > 0 && Statistics.Nodes > timeControl.NodeLimit))
          //	we must about the search
          abortSearch = true;
      }
    }

    public void AbortSearch()
    {
      abortSearch = true;
    }

    public PerftResults Perft(int depth, StreamWriter log = null)
    {
      PerftResults results = new PerftResults();
      perft(1, depth, results, log);
      return results;
    }

    public void Cleanup()
    {
      hashtable = null;
    }

    public string FormatScoreForDisplay(int score)
    {
      if (score > INFINITY - MAX_PLY)
      {
        if (score == INFINITY - 2)
          return "Mate";
        return "M" + ((INFINITY - (score + 2)) / 2);
      }
      if (score < -INFINITY + MAX_PLY)
        return "-M" + ((INFINITY + (score - 1)) / 2);
      return ((double)score / 100.0).ToString("F2");
    }

    protected int[] razorMargin;
    protected int weakeningHashShift;
    protected uint[] previousBestMoves = new uint[5];
  }
}
