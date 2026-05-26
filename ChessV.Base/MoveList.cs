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
  // MoveList is a compact, shared buffer with several intentionally separate
  // roles. Keep these cursor contracts distinct when changing callers:
  //
  //   Role                 Main callers                         Cursor contract
  //   -------------------  -----------------------------------  ---------------------------------------------
  //   Generation buffer    Game.generateMoves, pieces/rules     Reset clears entries; Add* appends candidates.
  //   Candidate check      AddMove/AddCapture/EndMoveAddCore    Legal Make/Unmake checks must leave state even.
  //   Search iterator      Search.cs                            Restart keeps entries; MakeNextMove selects one.
  //   Root move source     GetRootMoves, GUI, Game.MakeMove     Count/cursors describe candidates, not history.
  //   History source       Game.MakeMove, BoardMoveStack        Played history copies ranges from a root move.
  public class MoveList
  {
    // *** PROPERTIES *** //

    public Board Board { get; private set; }

    public Game Game { get; private set; }

    // This is the maximum number of moves that can be stored, but the implementation of nonstandard moves ("Special Moves")
    // means that the actual number of turns we can look forward can be less. A pickup and drop is 2 moves, in 1 ply.
    public const int MAX_MOVES = 2048;
    public const int MAX_MOVES_TRY_STOP_DELTA = 100;
    public const int MAX_MOVES_HARD_STOP_DELTA = 50;
    public const int MAX_MOVES_CRASH_STOP_DELTA = 5;

    public bool LegalMovesOnly { get; set; }

    public int Count { get { return moveCursor; } }

    public int MoveCursor
    { get { return moveCursor; } }

    // *** TEST HOOKS *** //
    // The following accessors are intentionally narrow, read-only views of the
    // internal cursors and arrays. They exist so that regression tests can
    // snapshot/inspect MoveList state without needing reflection. They are not
    // used by production code paths.
    public int PickupCursorForTest { get { return pickupCursor; } }
    public int DropCursorForTest { get { return dropCursor; } }
    public Pickup GetPickupForTest(int index) { return pickups[index]; }
    public Drop GetDropForTest(int index) { return drops[index]; }
    public MoveInfo GetMoveForTest(int index) { return moves[index]; }

    // Test hooks that expose the protected Make/Undo primitives so unit
    // tests can drive partial Make sequences and verify the defensive
    // rollback logic in MakeMove without having to go through a custom
    // Rule that throws inside Game.MoveBeingMade.
    public void PerformPickupForTest(int index) { PerformPickup(index); }
    public void PerformDropForTest(int index) { PerformDrop(index); }
    public void UndoPickupForTest(int index) { UndoPickup(index); }
    public void UndoDropForTest(int index) { UndoDrop(index); }
    public void SetPickupForTest(int index, Pickup p) { pickups[index] = p; }
    public void SetDropForTest(int index, Drop d) { drops[index] = d; }
    public void RollbackPartialApplyForTest(
      int firstPickup, int lastAppliedPickup,
      int firstDrop, int lastAppliedDrop)
    {
      RollbackPartialApply(firstPickup, lastAppliedPickup, firstDrop, lastAppliedDrop);
    }

    // The current move being made, if any
    public MoveInfo CurrentMove
    { get { return currentMoveIndex >= 0 && currentMoveIndex < moveCursor ? moves[currentMoveIndex] : default(MoveInfo); } }


    // *** CONSTRUCTION *** //

    #region Constructor
    public MoveList
      (Board board,
        SearchStack[] searchStack,
        UInt64[] killers1,
        UInt64[] killers2,
        UInt32[,,] historyCounters,
        UInt32[,,] butterflyCounters,
        int ply)
    {
      if (nullMoves == null)
      {
        nullMoves = new MoveInfo[2];
        nullMoves[0].MoveType = MoveType.NullMove;
        nullMoves[0].Player = 0;
        nullMoves[1].MoveType = MoveType.NullMove;
        nullMoves[1].Player = 1;
      }

      Board = board;
      Game = board.Game;
      LegalMovesOnly = false;
      moves = new MoveInfo[MAX_MOVES];
      pickups = new Pickup[MAX_MOVES];
      drops = new Drop[MAX_MOVES];
      moveOrder = new int[MAX_MOVES];
      this.searchStack = searchStack;
      this.killers1 = killers1;
      this.killers2 = killers2;
      this.historyCounters = historyCounters;
      this.butterflyCounters = butterflyCounters;
      this.ply = ply;
      Reset();
    }
    #endregion


    // *** OPERATIONS *** //

    #region Reset
    public void Reset(UInt64 hashtableMoveHash = 0, UInt64 countermove = 0)
    {
      // Reset clears generated entries and any currently selected move.
      moveCursor = 0;
      pickupCursor = 0;
      dropCursor = 0;
      currentMoveIndex = -1;
      triedMovesCursor = -1;
      this.hashtableMoveHash = hashtableMoveHash;
      this.countermove = countermove;
    }
    #endregion

    #region Restart
    public void Restart(UInt64 pvMove)
    {
      // Restart preserves generated entries but rewinds search iteration.
      currentMoveIndex = -1;
      triedMovesCursor = -1;
      if (pvMove != 0)
      {
        //	multi-pv mode - find the specified PV move and 
        //	maximize its evaluation so it is tried first
        for (int x = 0; x < moveCursor; x++)
          if (moves[x].Hash == pvMove)
            moves[x].Evaluation = 99999;
      }
    }
    #endregion

    #region ReorderMoves
    public void ReorderMoves(Dictionary<UInt64, int> moveScores)
    {
      for (int x = 0; x < moveCursor; x++)
        moves[x].Evaluation = moveScores[moves[x].Hash];
    }
    #endregion

    #region GetMoves
    public int GetMoves(out MoveInfo[] moves)
    { moves = this.moves; return moveCursor; }
    #endregion

    #region FindMove
    public MoveInfo FindMove(UInt64 movehash)
    {
      for (int x = 0; x < moveCursor; x++)
        if (moves[x].Hash == movehash)
          return moves[x];
      throw new Exception("Move not found");
    }
    #endregion

    #region MakeNextMove
    public bool MakeNextMove(int minCaptureValue = 0)
    {
      if (triedMovesCursor == -1)
        triedMovesCursor = moveCursor;
      bool succeeded = false;
      while (!succeeded && triedMovesCursor > 0)
      {
        int bestMoveIndex = 0;
        int bestMoveEval = moves[moveOrder[0]].Evaluation;
        for (int x = 1; x < triedMovesCursor; x++)
          if (moves[moveOrder[x]].Evaluation > bestMoveEval)
          {
            bestMoveIndex = x;
            bestMoveEval = moves[moveOrder[x]].Evaluation;
          }
        int captureVal = 0;
        if (minCaptureValue != 0 && (moves[moveOrder[bestMoveIndex]].MoveType & MoveType.CaptureProperty) != 0)
        {
          captureVal = moves[moveOrder[bestMoveIndex]].PieceCaptured.PieceType.MidgameValue -
            moves[moveOrder[bestMoveIndex]].PieceMoved.PieceType.GetMidgamePST(Board.PlayerSquare(moves[moveOrder[bestMoveIndex]].PieceMoved.Player, moves[moveOrder[bestMoveIndex]].FromSquare)) +
            moves[moveOrder[bestMoveIndex]].PieceMoved.PieceType.GetMidgamePST(Board.PlayerSquare(moves[moveOrder[bestMoveIndex]].PieceMoved.Player, moves[moveOrder[bestMoveIndex]].ToSquare)) +
            moves[moveOrder[bestMoveIndex]].PieceCaptured.PieceType.GetMidgamePST(Board.PlayerSquare(moves[moveOrder[bestMoveIndex]].PieceCaptured.Player, moves[moveOrder[bestMoveIndex]].PieceCaptured.Square));
          if ((moves[moveOrder[bestMoveIndex]].MoveType & MoveType.PromotionProperty) != 0)
            captureVal += Board.Game.GetPieceType(moves[moveOrder[bestMoveIndex]].PromotionType).MidgameValue -
              moves[moveOrder[bestMoveIndex]].PieceMoved.PieceType.MidgameValue;
          if (moves[moveOrder[bestMoveIndex]].MoveType == MoveType.ExtraCapture)
            captureVal += Board[moves[moveOrder[bestMoveIndex]].Tag].MidgameValue;
        }
        if ((moves[moveOrder[bestMoveIndex]].MoveType != MoveType.StandardCapture &&
           moves[moveOrder[bestMoveIndex]].MoveType != MoveType.EnPassant) || captureVal >= minCaptureValue)
        {
          bool tryMove = true;
          if (minCaptureValue != 0 && moves[moveOrder[bestMoveIndex]].MoveType == MoveType.StandardCapture)
            //	check the SEE score and skip if this is a losing capture
            tryMove = !Board.Game.StaticExchangeEvaluation || Board.Game.SEE_GE(moves[moveOrder[bestMoveIndex]].FromSquare, moves[moveOrder[bestMoveIndex]].ToSquare, 0);
          if (tryMove)
          {
            try
            {
              succeeded = MakeMove(moveOrder[bestMoveIndex]);
            }
            catch (Exception ex)
            {
              throw new Exception(
                string.Format("Error making move {0} ({1}) during move {2} ({3}) considering move {4} ({5})!",
                  moveOrder[bestMoveIndex],
                  moves[moveOrder[bestMoveIndex]].ToString(),
                  currentMoveIndex,
                  moves[currentMoveIndex].ToString(),
                  moveCursor,
                  moves[moveCursor].ToString()),
                ex
              );
            }
            currentMoveIndex = moveOrder[bestMoveIndex];
            if (!succeeded)
              UnmakeMove();
          }
        }
        int tempOrder = moveOrder[triedMovesCursor - 1];
        moveOrder[triedMovesCursor - 1] = moveOrder[bestMoveIndex];
        moveOrder[bestMoveIndex] = tempOrder;
        triedMovesCursor--;
      }
      return succeeded;
    }
    #endregion

    #region PerformPickup
    protected void PerformPickup(int index)
    {
      try
      {
        pickups[index].Piece = Board.ClearSquare(pickups[index].Square);
      }
      catch (InvalidBoardStateException ex)
      {
        // Add move context AND the unified move-generation context stack
        // to the exception. The base message already includes the board
        // dump from Board.ClearSquare; we append the context stack so the
        // caller can see WHICH rule and WHICH nested generation cycle
        // triggered the failing pickup.
        throw new InvalidBoardStateException(
          ex.Message + MoveGenerationContext.FormatContextStack(),
          ex.Square,
          ex.SquareNotation,
          ex.Game,
          moves[moveCursor]);
      }
      catch (Exception ex)
      {
        throw new Exception(
          string.Format("Error performing pickup at {0} ({1}) during move {2} ({3}) following move {4} ({5})!{6}",
            pickups[index].Square,
            Board.GetDefaultSquareNotation(pickups[index].Square),
            moveCursor,
            moves[moveCursor].ToString(),
            moveCursor - 1,
            moveCursor > 0 ? moves[moveCursor - 1].ToString() : "none",
            MoveGenerationContext.FormatContextStack()),
          ex
        );
      }
    }
    #endregion

    #region PerformDrop
    protected void PerformDrop(int index)
    {
      Piece piece = drops[index].Piece;
      int square = drops[index].Square;
      if (drops[index].NewType != null)
      {
        PieceType oldType = piece.PieceType;
        piece.PieceType = drops[index].NewType;
        piece.TypeNumber = piece.PieceType.TypeNumber;
        drops[index].NewType = oldType;
      }
      piece.MoveCount++;
      Board.SetSquare(piece, square);
    }
    #endregion

    #region UndoPickup
    protected void UndoPickup(int index)
    {
      // Defensive guard: if the recorded Piece is null then PerformPickup
      // either never ran for this slot or threw before assigning. Restoring
      // null would silently corrupt the board (Board.SetSquare with null
      // throws elsewhere) and hide the real cause. Surface the failure
      // with full context so the caller can diagnose the partial Make.
      if (pickups[index].Piece == null)
      {
        int sq = pickups[index].Square;
        string sqNotation;
        try { sqNotation = Board != null ? Board.GetDefaultSquareNotation(sq) : sq.ToString(); }
        catch { sqNotation = sq.ToString(); }
        string msg = string.Format(
          "UndoPickup called on pickup #{0} (square {1}) whose Piece is null " +
          "— likely PerformPickup never ran (failed move?) or the pickup index " +
          "is stale. This means a Make failed mid-way and the board is corrupted. " +
          "moveCursor={2} pickupCursor={3} dropCursor={4}{5}",
          index, sqNotation, moveCursor, pickupCursor, dropCursor,
          MoveGenerationContext.FormatContextStack());
        throw new InvalidBoardStateException(msg, sq, sqNotation, Board != null ? Board.Game : null);
      }
      // Strictly restore the picked-up piece back to its original square
      Board.SetSquare(pickups[index].Piece, pickups[index].Square);
    }
    #endregion

    #region UndoDrop
    protected void UndoDrop(int index)
    {
      // Strictly clear the dropped piece and restore its pre-drop state
      Board.ClearSquare(drops[index].Square);
      drops[index].Piece.MoveCount--;
      if (drops[index].NewType != null)
      {
        PieceType oldType = drops[index].NewType;
        PieceType newType = drops[index].Piece.PieceType;
        drops[index].Piece.PieceType = oldType;
        drops[index].Piece.TypeNumber = oldType.TypeNumber;
        drops[index].NewType = newType;
      }
    }
    #endregion

    #region AddMove
    public bool AddMove(int fromSquare, int toSquare, bool direct = false)
    {
      // Check for move list overflow before proceeding
      if (moveCursor >= MAX_MOVES - MAX_MOVES_CRASH_STOP_DELTA)
      {
        return false;
      }

      if (!direct && Board.Game.MoveBeingGenerated(this, fromSquare, toSquare, MoveType.StandardMove))
        return true; // Move was handled, consider it successful

      if (Game.DeduplicateMoves)
        //	Game requires move deduplication, so we must check to 
        //	ensure that we don't have this move already
        for (int x = 0; x < moveCursor; x++)
          if (moves[x].MoveType == MoveType.StandardMove &&
            moves[x].FromSquare == fromSquare &&
            moves[x].ToSquare == toSquare)
            //	we have this move already so don't add it again
            return true; // Already have it, consider successful

      Piece pieceBeingMoved = Board[fromSquare];
      if (pieceBeingMoved == null)
      {
        return false; // No piece to move, return false instead of throwing
      }

      //	initialize pickups and drops
      pickups[pickupCursor].Piece = null;
      pickups[pickupCursor++].Square = fromSquare;
      drops[dropCursor].NewType = null;
      drops[dropCursor].Piece = pieceBeingMoved;
      drops[dropCursor++].Square = toSquare;

      //	initialize move order array
      moveOrder[moveCursor] = moveCursor;

      //	initialize moveInfo in moves
      moves[moveCursor].MoveType = MoveType.StandardMove;
      moves[moveCursor].Player = pieceBeingMoved.Player;
      moves[moveCursor].FromSquare = fromSquare;
      moves[moveCursor].ToSquare = toSquare;
      moves[moveCursor].PickupCursor = pickupCursor;
      moves[moveCursor].DropCursor = dropCursor;
      moves[moveCursor].PieceMoved = pieceBeingMoved;
      moves[moveCursor].PieceCaptured = null;
      moves[moveCursor].Tag = 0;
      moves[moveCursor].OriginalType = pieceBeingMoved.PieceType.TypeNumber;

      //	determine the move evaluation (for move ordering)
      if (moves[moveCursor].Hash == searchStack[1].PV[ply])
        moves[moveCursor].Evaluation = 50000;
      else if (moves[moveCursor].Hash == hashtableMoveHash)
        moves[moveCursor].Evaluation = 40000;
      else if (moves[moveCursor].Hash == killers1[ply] || moves[moveCursor].Hash == killers2[ply])
        moves[moveCursor].Evaluation = 2000;
      else
      {
        ulong history = (ulong)historyCounters[pieceBeingMoved.Player, pieceBeingMoved.TypeNumber, toSquare];
        if (history > 0)
          moves[moveCursor].Evaluation = (int)(history * 500ul / Board.Game.CurrentMaxHistoryScore /
            butterflyCounters[pieceBeingMoved.Player, pieceBeingMoved.TypeNumber, toSquare]);
        else
          moves[moveCursor].Evaluation = pieceBeingMoved.PieceType.GetMidgamePST(toSquare) -
            pieceBeingMoved.PieceType.GetMidgamePST(fromSquare) - 25;
      }
      if (moves[moveCursor].Hash == countermove)
        moves[moveCursor].Evaluation += 150;
      moveCursor++;

      if (LegalMovesOnly)
      {
        // Synthetic diagnostic frame: AddMove runs Make/Unmake without
        // going through BeginMoveAdd/EndMoveAdd, so push our own.
        MoveGenerationContext.Push(
          ruleNameOverride: "(AddMove)",
          ply: ply,
          moveType: MoveType.StandardMove,
          fromSquare: fromSquare,
          toSquare: toSquare,
          pickupCursor: pickupCursor,
          dropCursor: dropCursor,
          moveCursor: moveCursor,
          boardHash: Board != null ? Board.HashCode : 0UL);
        try
        {
          // Make/Unmake round-trip invariant: capture pre-state if armed.
          // Off-path is a single bool check; no allocation when disabled.
          int firstPickup = pickupCursor - 1;
          int firstDrop = dropCursor - 1;
          ulong preHash = 0;
          Piece[] preSnapshot = null;
          if (DebugFlags.AssertMakeUnmakeRoundTrip)
          {
            preHash = Board.HashCode;
            if (DebugFlags.VerboseDiagnostics)
              preSnapshot = SnapshotBoardSquares();
          }

          bool legal = MakeMove(moveCursor - 1);
          UnmakeMove(moveCursor - 1);

          if (DebugFlags.AssertMakeUnmakeRoundTrip && Board.HashCode != preHash)
          {
            ThrowMakeUnmakeRoundTripViolation(
              moveIndex: moveCursor - 1,
              firstPickup: firstPickup,
              endPickup: pickupCursor,
              firstDrop: firstDrop,
              endDrop: dropCursor,
              preHash: preHash,
              postHash: Board.HashCode,
              preSnapshot: preSnapshot);
          }

          if (!legal)
          {
            moveCursor--;
            pickupCursor--;
            dropCursor--;
          }
        }
        finally
        {
          MoveGenerationContext.Pop();
        }
      }
      
      return true; // Successfully added move
    }

    public bool AddMove(string fromSquareNotation, string toSquareNotation, bool direct = false)
    {
      return AddMove(Board.Game.NotationToSquare(fromSquareNotation), Board.Game.NotationToSquare(toSquareNotation), direct);
    }
    #endregion

    #region AddCapture
    public bool AddCapture(int fromSquare, int toSquare, bool direct = false)
    {
      // Check for move list overflow - return false instead of throwing
      if (moveCursor >= MAX_MOVES - MAX_MOVES_CRASH_STOP_DELTA)
      {
        // Log the issue if possible but don't crash
        return false;
      }

      if (!direct && Board.Game.MoveBeingGenerated(this, fromSquare, toSquare, MoveType.StandardCapture))
        return true; // Move was handled, consider it successful

      if (Game.DeduplicateMoves)
        //	Game requires move deduplication, so we must check to 
        //	ensure that we don't have this move already
        for (int x = 0; x < moveCursor; x++)
          if (moves[x].MoveType == MoveType.StandardCapture &&
            moves[x].FromSquare == fromSquare &&
            moves[x].ToSquare == toSquare)
            //	we have this move already so don't add it again
            return true; // Already have it, consider successful

      Piece pieceBeingMoved = Board[fromSquare];
      Piece pieceBeingCaptured = Board[toSquare];

      if (pieceBeingMoved == null || pieceBeingCaptured == null)
      {
        // Log the issue but don't crash - just skip this move
        return false;
      }

      //	initialize pickups and drops
      pickups[pickupCursor].Piece = null;
      pickups[pickupCursor++].Square = fromSquare;
      pickups[pickupCursor].Piece = null;
      pickups[pickupCursor++].Square = toSquare;
      drops[dropCursor].NewType = null;
      drops[dropCursor].Piece = pieceBeingMoved;
      drops[dropCursor++].Square = toSquare;

      //	initialize move order array
      moveOrder[moveCursor] = moveCursor;

      //	initialize moveInfo in moves
      moves[moveCursor].MoveType = MoveType.StandardCapture;
      moves[moveCursor].Player = pieceBeingMoved.Player;
      moves[moveCursor].FromSquare = fromSquare;
      moves[moveCursor].ToSquare = toSquare;
      moves[moveCursor].PickupCursor = pickupCursor;
      moves[moveCursor].DropCursor = dropCursor;
      moves[moveCursor].PieceMoved = pieceBeingMoved;
      moves[moveCursor].PieceCaptured = pieceBeingCaptured;
      moves[moveCursor].Tag = 0;
      moves[moveCursor].OriginalType = pieceBeingMoved.PieceType.TypeNumber;

      //	determine the move evaluation (for move ordering)
      moves[moveCursor].Evaluation = (pieceBeingCaptured.PieceType.MidgameValue >= pieceBeingMoved.PieceType.MidgameValue ? 3000 :
        (!Board.Game.SimpleMoveGeneration || Board.Game.SEE_GE(fromSquare, toSquare, 0) ? 3000 : 100)) +
        (pieceBeingCaptured.PieceType.MidgameValue / 2) - (pieceBeingMoved.PieceType.MidgameValue / 32);
      if (moves[moveCursor].Hash == searchStack[1].PV[ply])
        moves[moveCursor].Evaluation = 50000;
      else if (moves[moveCursor].Hash == hashtableMoveHash)
        moves[moveCursor].Evaluation = 40000;
      else if (moves[moveCursor].Hash == countermove)
        moves[moveCursor].Evaluation += 250;
      moveCursor++;

      if (LegalMovesOnly)
      {
        // Synthetic diagnostic frame: AddCapture runs Make/Unmake
        // without going through BeginMoveAdd/EndMoveAdd, so push our own.
        MoveGenerationContext.Push(
          ruleNameOverride: "(AddCapture)",
          ply: ply,
          moveType: MoveType.StandardCapture,
          fromSquare: fromSquare,
          toSquare: toSquare,
          pickupCursor: pickupCursor,
          dropCursor: dropCursor,
          moveCursor: moveCursor,
          boardHash: Board != null ? Board.HashCode : 0UL);
        try
        {
          // AddCapture pushed 2 pickups (from, to) and 1 drop above.
          int firstPickup = pickupCursor - 2;
          int firstDrop = dropCursor - 1;
          ulong preHash = 0;
          Piece[] preSnapshot = null;
          if (DebugFlags.AssertMakeUnmakeRoundTrip)
          {
            preHash = Board.HashCode;
            if (DebugFlags.VerboseDiagnostics)
              preSnapshot = SnapshotBoardSquares();
          }

          bool legal = MakeMove(moveCursor - 1);
          UnmakeMove(moveCursor - 1);

          if (DebugFlags.AssertMakeUnmakeRoundTrip && Board.HashCode != preHash)
          {
            ThrowMakeUnmakeRoundTripViolation(
              moveIndex: moveCursor - 1,
              firstPickup: firstPickup,
              endPickup: pickupCursor,
              firstDrop: firstDrop,
              endDrop: dropCursor,
              preHash: preHash,
              postHash: Board.HashCode,
              preSnapshot: preSnapshot);
          }

          if (!legal)
          {
            moveCursor--;
            pickupCursor -= 2;
            dropCursor--;
          }
        }
        finally
        {
          MoveGenerationContext.Pop();
        }
      }
      
      return true; // Successfully added capture
    }

    public bool AddCapture(string fromSquareNotation, string toSquareNotation, bool direct = false)
    {
      return AddCapture(Board.Game.NotationToSquare(fromSquareNotation), Board.Game.NotationToSquare(toSquareNotation), direct);
    }
    #endregion

    #region AddRifleCapture
    public bool AddRifleCapture(int fromSquare, int toSquare, bool direct = false)
    {
      // Check for move list overflow
      if (moveCursor >= MAX_MOVES - MAX_MOVES_CRASH_STOP_DELTA)
      {
        return false;
      }

      if (!direct && Board.Game.MoveBeingGenerated(this, fromSquare, toSquare, MoveType.BaroqueCapture))
        return true;

      Piece pieceBeingMoved = Board[fromSquare];
      Piece pieceBeingCaptured = Board[toSquare];

      if (pieceBeingMoved == null || pieceBeingCaptured == null)
      {
        return false;
      }

      //	initialize pickup
      pickups[pickupCursor].Piece = null;
      pickups[pickupCursor++].Square = toSquare;

      //	initialize move order array
      moveOrder[moveCursor] = moveCursor;

      //	initialize moveInfo in moves
      moves[moveCursor].MoveType = MoveType.BaroqueCapture;
      moves[moveCursor].Player = pieceBeingMoved.Player;
      moves[moveCursor].FromSquare = fromSquare;
      moves[moveCursor].ToSquare = toSquare;
      moves[moveCursor].PickupCursor = pickupCursor;
      moves[moveCursor].DropCursor = dropCursor;
      moves[moveCursor].PieceMoved = pieceBeingMoved;
      moves[moveCursor].PieceCaptured = pieceBeingCaptured;
      moves[moveCursor].Tag = toSquare;
      moves[moveCursor].OriginalType = pieceBeingMoved.PieceType.TypeNumber;
      moves[moveCursor].Evaluation = 6000 +
        pieceBeingCaptured.PieceType.MidgameValue;
      if (moves[moveCursor].Hash == searchStack[1].PV[ply])
        moves[moveCursor].Evaluation = 50000;
      else if (moves[moveCursor].Hash == hashtableMoveHash)
        moves[moveCursor].Evaluation = 40000;
      moveCursor++;

      if (LegalMovesOnly)
      {
        bool legal = MakeMove(moveCursor - 1);
        UnmakeMove(moveCursor - 1);
        if (!legal)
        {
          moveCursor--;
          pickupCursor--;
          return false;
        }
      }
      
      return true;
    }
    #endregion

    #region BeginMoveAdd
    public bool BeginMoveAdd
      (MoveType moveType,
        int fromSquare,
        int toSquare,
        int tag = 0)
    {
      // Check for move list overflow before proceeding - return false instead of throwing
      if (moveCursor >= MAX_MOVES - MAX_MOVES_HARD_STOP_DELTA)
      {
        return false;
      }

      // Validate square bounds - return false instead of throwing
      if (fromSquare != -1 && (fromSquare < 0 || fromSquare >= Board.NumSquaresExtended))
      {
        return false;
      }
      if (toSquare < 0 || toSquare >= Board.NumSquaresExtended)
      {
        return false;
      }

      moves[moveCursor].MoveType = moveType;
      moves[moveCursor].Tag = tag;

      if (fromSquare != -1)
      {
        Piece pieceBeingMoved = Board[fromSquare];
        if (pieceBeingMoved == null)
        {
          // No frame pushed yet; safe to bail without affecting the diag stack.
          return false;
        }
        moves[moveCursor].Player = pieceBeingMoved.Player;
        moves[moveCursor].FromSquare = fromSquare;
        moves[moveCursor].ToSquare = toSquare;
        moves[moveCursor].OriginalType = pieceBeingMoved.PieceType.TypeNumber;
        moves[moveCursor].PieceMoved = pieceBeingMoved;
      }
      else
      {
        moves[moveCursor].Player = Board.Game.CurrentSide;
        moves[moveCursor].FromSquare = 0;
        moves[moveCursor].ToSquare = 0;
        moves[moveCursor].OriginalType = -1;
        moves[moveCursor].PieceMoved = null;
      }

      //	initialize move order array
      moveOrder[moveCursor] = moveCursor;

      //	store temporary cursor values, in case this move turns out to be 
      //	illegal, in which case we need to restore the original values
      tempPickupCursor = pickupCursor;
      tempDropCursor = dropCursor;

      // Push a diagnostic frame so any crash escaping PerformPickup or
      // Board.ClearSquare during this move's Make/Unmake validation is
      // attributable to the rule + ply + cursor state that started it.
      // Matched by a Pop in every exit of EndMoveAdd. If EndMoveAdd is
      // never reached because the caller abandons the move without
      // calling EndMoveAdd, the frame leaks for the duration of the
      // current generation pass; this is acceptable because in-product
      // callers always pair the two, and tests can call Reset().
      MoveGenerationContext.Push(
        ruleNameOverride: null,
        ply: ply,
        moveType: moveType,
        fromSquare: fromSquare,
        toSquare: toSquare,
        pickupCursor: pickupCursor,
        dropCursor: dropCursor,
        moveCursor: moveCursor,
        boardHash: Board != null ? Board.HashCode : 0UL);

      return true;
    }
    #endregion

    #region SetMoveTag
    public void SetMoveTag(int tag)
    {
      moves[moveCursor].Tag = tag;
    }
    #endregion

    #region AddPickup
    public Piece AddPickup(int square)
    {
      // Validate square bounds - return null instead of throwing
      if (square < 0 || square >= Board.NumSquaresExtended)
      {
        return null;
      }

      // Check pickup capacity - return null instead of throwing
      if (pickupCursor >= MAX_MOVES - MAX_MOVES_CRASH_STOP_DELTA)
      {
        return null;
      }

      Piece pieceOnSquare = Board[square];
      if (pieceOnSquare == null)
      {
        return null;
      }

      pickups[pickupCursor].Piece = null;
      pickups[pickupCursor++].Square = square;
      moves[moveCursor].PieceCaptured = pieceOnSquare;
      if (pieceOnSquare.Player != Board.Game.CurrentSide)
        moves[moveCursor].PieceCaptured = pieceOnSquare;
      return pieceOnSquare;
    }
    #endregion

    #region AddDrop
    public bool AddDrop
      (Piece piece,
        int square,
        PieceType newType)
    {
      // Validate inputs - return false instead of throwing
      if (piece == null)
      {
        return false;
      }
      if (square < 0 || square >= Board.NumSquaresExtended)
      {
        return false;
      }

      // Check drop capacity - return false instead of throwing
      if (dropCursor >= MAX_MOVES - MAX_MOVES_CRASH_STOP_DELTA)
      {
        return false;
      }

      if (moves[moveCursor].PieceCaptured == piece)
        moves[moveCursor].PieceCaptured = null;
      drops[dropCursor].NewType = newType;
      drops[dropCursor].Piece = piece;
      drops[dropCursor++].Square = square;
      if (newType != null)
        moves[moveCursor].PromotionType = newType.TypeNumber;
        
      return true;
    }

    public bool AddDrop
      (Piece piece,
        int square)
    {
      return AddDrop(piece, square, null);
    }
    #endregion

    #region EndMoveAdd
    public bool EndMoveAdd(int evaluation)
    {
      try
      {
        return EndMoveAddCore(evaluation);
      }
      finally
      {
        // Pair with the Push in BeginMoveAdd. Always runs, even when an
        // exception escapes from MakeMove/PerformPickup, so the per-thread
        // diag stack does not leak across generation passes.
        MoveGenerationContext.Pop();
      }
    }

    private bool EndMoveAddCore(int evaluation)
    {
      // Validate that we have consistent pickup and drop counts
      int pickupCount = pickupCursor - tempPickupCursor;
      int dropCount = dropCursor - tempDropCursor;
      
      // For most moves, we should have equal pickups and drops, but some special moves might differ
      if (pickupCount == 0 && dropCount == 0)
      {
        return false;
      }

      moves[moveCursor].PickupCursor = pickupCursor;
      moves[moveCursor].DropCursor = dropCursor;
      moves[moveCursor].Evaluation = evaluation;
      if (moves[moveCursor].Hash == searchStack[1].PV[ply])
        moves[moveCursor].Evaluation = 50000;
      else if (moves[moveCursor].Hash == hashtableMoveHash)
        moves[moveCursor].Evaluation = 40000;
      else if (moves[moveCursor].Hash == countermove)
        moves[moveCursor].Evaluation += 250;
      moveCursor++;

      // Final bounds check - return false instead of throwing
      if (moveCursor >= MAX_MOVES)
      {
        moveCursor--; // Rollback the increment
        pickupCursor = tempPickupCursor;
        dropCursor = tempDropCursor;
        return false;
      }

      if (LegalMovesOnly)
      {
        int firstPickup = tempPickupCursor;
        int firstDrop = tempDropCursor;
        ulong preHash = 0;
        Piece[] preSnapshot = null;
        if (DebugFlags.AssertMakeUnmakeRoundTrip)
        {
          preHash = Board.HashCode;
          if (DebugFlags.VerboseDiagnostics)
            preSnapshot = SnapshotBoardSquares();
        }

        // Phase 3.6: snapshot the cursors right BEFORE MakeMove so we can
        // detect any nested write to tempPickupCursor / tempDropCursor that
        // would silently corrupt the rollback below. MakeMove+UnmakeMove
        // must be balanced and may not bump the global cursors; if they
        // do, our rollback target is no longer trustworthy.
        int preMakePickupCursor = pickupCursor;
        int preMakeDropCursor = dropCursor;
        int preMakeTempPickupCursor = tempPickupCursor;
        int preMakeTempDropCursor = tempDropCursor;

        bool legal = MakeMove(moveCursor - 1);
        UnmakeMove(moveCursor - 1);

        if (DebugFlags.AssertMakeUnmakeRoundTrip && Board.HashCode != preHash)
        {
          ThrowMakeUnmakeRoundTripViolation(
            moveIndex: moveCursor - 1,
            firstPickup: firstPickup,
            endPickup: pickupCursor,
            firstDrop: firstDrop,
            endDrop: dropCursor,
            preHash: preHash,
            postHash: Board.HashCode,
            preSnapshot: preSnapshot);
        }

        if (DebugFlags.AssertMakeUnmakeRoundTrip &&
            (pickupCursor != preMakePickupCursor || dropCursor != preMakeDropCursor ||
             tempPickupCursor != preMakeTempPickupCursor ||
             tempDropCursor != preMakeTempDropCursor))
        {
          throw new InvalidBoardStateException(
            $"MakeMove/UnmakeMove perturbed MoveList cursors: " +
            $"pickupCursor {preMakePickupCursor}->{pickupCursor}, " +
            $"dropCursor {preMakeDropCursor}->{dropCursor}, " +
            $"tempPickupCursor {preMakeTempPickupCursor}->{tempPickupCursor}, " +
            $"tempDropCursor {preMakeTempDropCursor}->{tempDropCursor}. " +
            $"Cursor state must be invariant across a balanced Make+Unmake. " +
            MoveGenerationContext.FormatContextStack(),
            square: 0,
            squareNotation: "<n/a>",
            game: Board?.Game);
        }

        if (!legal)
        {
          moveCursor--;
          pickupCursor = tempPickupCursor;
          dropCursor = tempDropCursor;
          return false;
        }
      }
      
      return true;
    }
    #endregion

    #region Capacity Management
    
    /// <summary>
    /// Check if we can safely add more moves of a given type
    /// </summary>
    public bool CanAddMoves(int moveTypeCount = 1)
    {
      return moveCursor + moveTypeCount <= MAX_MOVES - MAX_MOVES_HARD_STOP_DELTA;
    }
    
    /// <summary>
    /// Check if we're approaching capacity and should prioritize important moves
    /// </summary>
    public bool ShouldPrioritizeMoves()
    {
      return moveCursor >= MAX_MOVES - MAX_MOVES_TRY_STOP_DELTA;
    }
    
    /// <summary>
    /// Check if we should skip lower-priority move generation
    /// </summary>
    public bool ShouldSkipLowPriorityMoves()
    {
      return moveCursor >= MAX_MOVES - MAX_MOVES_HARD_STOP_DELTA;
    }
    
    /// <summary>
    /// Get remaining capacity for moves
    /// </summary>
    public int RemainingCapacity()
    {
      return MAX_MOVES - moveCursor;
    }
    
    /// <summary>
    /// Check if a move would be a duplicate (for better deduplication)
    /// </summary>
    public bool IsDuplicateMove(MoveType moveType, int fromSquare, int toSquare)
    {
      for (int x = 0; x < moveCursor; x++)
      {
        if (moves[x].MoveType == moveType &&
            moves[x].FromSquare == fromSquare &&
            moves[x].ToSquare == toSquare)
        {
          return true;
        }
      }
      return false;
    }
    
    /// <summary>
    /// Safely add a move with automatic capacity checking and deduplication
    /// Returns: true if move was added, false if skipped due to capacity or duplication
    /// </summary>
    public bool TryAddMove(int fromSquare, int toSquare, bool skipIfDuplicate = true)
    {
      if (!CanAddMoves(1))
        return false;
        
      if (skipIfDuplicate && IsDuplicateMove(MoveType.StandardMove, fromSquare, toSquare))
        return true; // Consider this successful since we already have the move
        
      return AddMove(fromSquare, toSquare, false);
    }
    
    /// <summary>
    /// Safely add a capture with automatic capacity checking and deduplication
    /// Returns: true if capture was added, false if skipped due to capacity or duplication
    /// </summary>
    public bool TryAddCapture(int fromSquare, int toSquare, bool skipIfDuplicate = true)
    {
      if (!CanAddMoves(1))
        return false;
        
      if (skipIfDuplicate && IsDuplicateMove(MoveType.StandardCapture, fromSquare, toSquare))
        return true; // Consider this successful since we already have the move
        
      return AddCapture(fromSquare, toSquare, false);
    }
    
    #endregion

    #region MakeMove
    public bool MakeMove(int index)
    {
      bool succeeded = false;
      if (index >= 0 && index < moveCursor)
      {
        // determine the pickup/drop ranges for this move
        int firstPickup = 0;
        int firstDrop = 0;
        if (index > 0)
        {
          firstPickup = moves[index - 1].PickupCursor;
          firstDrop = moves[index - 1].DropCursor;
        }

        //	NOTE: do NOT write tempPickupCursor / tempDropCursor here.
        //	Those fields are owned by BeginMoveAdd as a snapshot for the
        //	EndMoveAddCore rollback path, and EndMoveAddCore calls into
        //	this MakeMove for its legality check. Aliasing the snapshot
        //	from inside MakeMove silently corrupted the rollback target
        //	for illegal moves, leaving pickupCursor/dropCursor at the
        //	post-Make values and causing later AddMove calls to overlap
        //	prior pickup ranges (see Phase 2 diagnosis of the
        //	2026-02-12 Checkers crash). MakeMove itself does not need
        //	these fields - rollback within MakeMove is bounded by the
        //	local lastAppliedPickup / lastAppliedDrop counters below.

        // Track the last successfully-applied pickup/drop so that a catch
        // path knows EXACTLY how much state to roll back. If PerformPickup
        // throws on index N, then pickups [firstPickup..N-1] are applied
        // and pickups [N..end) are not (and may have stale/null Piece
        // refs from prior MoveLists), so the rollback must NOT iterate
        // past lastAppliedPickup.
        int lastAppliedPickup = firstPickup - 1;
        int lastAppliedDrop = firstDrop - 1;

        try
        {
          // Apply all pickups for this move
          for (int pickup = firstPickup; pickup < moves[index].PickupCursor; pickup++)
          {
            PerformPickup(pickup);
            lastAppliedPickup = pickup;
          }

          // Apply all drops for this move
          for (int drop = firstDrop; drop < moves[index].DropCursor; drop++)
          {
            PerformDrop(drop);
            lastAppliedDrop = drop;
          }

          //	ok, now pass message to the Game class, so it can update any info
          //	it may need to as a result of this move.  this also gives the Game
          //	class the chance to return false, indicating that the move is illegal
          succeeded = Board.Game.MoveBeingMade(moves[index]);
        }
        catch (InvalidBoardStateException ex)
        {
          if (DebugFlags.ThrowOnInvariantViolation)
          {
            throw new InvalidBoardStateException(
              ex.Message,
              ex.Square,
              ex.SquareNotation,
              ex.Game,
              moves[index]);
          }
          // Swallow and mark as failed move in non-throw mode, but first
          // roll back any pickups/drops we already applied so the board
          // is restored to its pre-MakeMove state. Without this, a later
          // UnmakeMove would compound the corruption.
          RollbackPartialApply(firstPickup, lastAppliedPickup, firstDrop, lastAppliedDrop);
          succeeded = false;
        }
        catch (Exception ex)
        {
          if (DebugFlags.ThrowOnInvariantViolation)
          {
            throw new Exception(
              string.Format("Error making move {0} ({1}) during move {2} ({3}) considering move {4} ({5})!",
                index,
                moves[index].ToString(),
                currentMoveIndex,
                moves[currentMoveIndex].ToString(),
                moveCursor,
                moves[moveCursor].ToString()),
              ex
            );
          }
          RollbackPartialApply(firstPickup, lastAppliedPickup, firstDrop, lastAppliedDrop);
          succeeded = false;
        }
      }
      return succeeded;
    }

    // Rolls back any pickups/drops applied by MakeMove before a failure.
    // Iterates in REVERSE (LIFO) so each Undo step sees the same board
    // state its corresponding Perform step left behind. Bounded by the
    // last successfully-applied index so we never call UndoPickup on a
    // slot whose Piece is null (which would trip the Phase 3 guard).
    // Secondary exceptions during rollback are swallowed (with optional
    // diagnostic output) because the board may already be too corrupt
    // to fully restore — propagating here would mask the original
    // failure that the caller is already handling.
    private void RollbackPartialApply(int firstPickup, int lastAppliedPickup,
                                       int firstDrop, int lastAppliedDrop)
    {
      try
      {
        for (int drop = lastAppliedDrop; drop >= firstDrop; drop--)
          UndoDrop(drop);
        for (int pickup = lastAppliedPickup; pickup >= firstPickup; pickup--)
          UndoPickup(pickup);
      }
      catch (Exception rollbackEx)
      {
        if (DebugFlags.VerboseDiagnostics)
        {
          Console.Error.WriteLine(
            "MoveList.RollbackPartialApply: secondary exception while undoing " +
            "partial Make state (pickups[{0}..{1}] drops[{2}..{3}]): {4}",
            firstPickup, lastAppliedPickup, firstDrop, lastAppliedDrop, rollbackEx);
        }
      }
    }

    public bool MakeMove(MoveInfo move)
    {
      for (int x = 0; x < moveCursor; x++)
        if (moves[x] == move)
          return MakeMove(x);
      return false;
    }
    #endregion

    #region UnmakeMove
    protected void UnmakeMove(int index)
    {
      Board.Game.MoveBeingUnmade(moves[index]);

      int firstDrop = 0;
      int firstPickup = 0;
      if (index > 0)
      {
        firstPickup = moves[index - 1].PickupCursor;
        firstDrop = moves[index - 1].DropCursor;
      }

      //	undo all drops
      for (int drop = firstDrop; drop < moves[index].DropCursor; drop++)
        UndoDrop(drop);

      //	undo all pickups
      for (int pickup = firstPickup; pickup < moves[index].PickupCursor; pickup++)
        UndoPickup(pickup);
    }

    public void UnmakeMove()
    {
      if (currentMoveIndex < 0)
      {
        RecoverableDiagnostics.Report(new RecoverableDiagnostic(
          "MoveList.UnmakeMove",
          "MoveList.UnmakeMove() was called without a selected search move.",
          string.Format(
            "The parameterless UnmakeMove overload is for search iteration after MakeNextMove(). " +
            "No board mutation was attempted. currentMoveIndex={0}, moveCursor={1}, pickupCursor={2}, dropCursor={3}.",
            currentMoveIndex, moveCursor, pickupCursor, dropCursor),
          Board != null ? Board.Game : null));
        return;
      }
      if (currentMoveIndex >= moveCursor)
        throw new InvalidOperationException(
          string.Format("MoveList.UnmakeMove() selected move index {0} is outside generated move count {1}.",
            currentMoveIndex, moveCursor));
      UnmakeMove(currentMoveIndex);
    }
    #endregion

    #region MakeNullMove
    public void MakeNullMove()
    {
      Board.Game.MoveBeingMade(nullMoves[Board.Game.CurrentSide]);
    }
    #endregion

    #region UnmakeNullMove
    public void UnmakeNullMove()
    {
      Board.Game.MoveBeingUnmade(nullMoves[Board.Game.CurrentSide ^ 1]);
    }
    #endregion

    #region CopyMoveToGameHistory
    public void CopyMoveToGameHistory(List<Pickup> gamePickups, List<Drop> gameDrops, MoveInfo move)
    {
      for (int index = 0; index < moveCursor; index++)
        if (moves[index] == move)
        {
          int firstPickup = 0;
          int firstDrop = 0;
          if (index > 0)
          {
            firstPickup = moves[index - 1].PickupCursor;
            firstDrop = moves[index - 1].DropCursor;
          }
          for (int pickup = firstPickup; pickup < moves[index].PickupCursor; pickup++)
            gamePickups.Add(pickups[pickup]);
          for (int drop = firstDrop; drop < moves[index].DropCursor; drop++)
            gameDrops.Add(drops[drop]);
          return;
        }
      throw new Exception("fatal error in MoveList::CopyMoveToGameHistory");
    }
    #endregion

    #region Validate
    public void Validate()
    {
      //	make sure each move is unique
      for (int x = 0; x < moveCursor; x++)
        for (int y = 0; y < moveCursor; y++)
          if (x != y && moves[x] == moves[y])
            throw new Exception("Invalid move list.");
    }
    #endregion


    #region Make/Unmake round-trip invariant helpers
    // Snapshot every square's Piece reference. Only called when both
    // AssertMakeUnmakeRoundTrip and VerboseDiagnostics are on, so the
    // allocation cost is opt-in. Returns null if Board is unavailable.
    private Piece[] SnapshotBoardSquares()
    {
      if (Board == null)
        return null;
      int n = Board.NumSquaresExtended;
      Piece[] snapshot = new Piece[n];
      for (int sq = 0; sq < n; sq++)
        snapshot[sq] = Board[sq];
      return snapshot;
    }

    // Build and throw the round-trip invariant violation. Centralized so all
    // three Make/Unmake call sites (AddMove, AddCapture, EndMoveAddCore) emit
    // a consistent message that includes pickup/drop ranges and (when
    // VerboseDiagnostics is on) per-square diffs vs. a pre-MakeMove snapshot.
    private void ThrowMakeUnmakeRoundTripViolation(
      int moveIndex,
      int firstPickup,
      int endPickup,
      int firstDrop,
      int endDrop,
      ulong preHash,
      ulong postHash,
      Piece[] preSnapshot)
    {
      MoveInfo mi = moves[moveIndex];
      var sb = new System.Text.StringBuilder();
      sb.AppendLine("Make/Unmake round-trip invariant violated: Board.HashCode was not restored after UnmakeMove.");
      sb.AppendLine($"  Move index: {moveIndex}");
      sb.AppendLine($"  MoveType: {mi.MoveType}");
      sb.AppendLine($"  FromSquare: {mi.FromSquare}, ToSquare: {mi.ToSquare}");
      sb.AppendLine($"  Pickup range: [{firstPickup}..{endPickup})");
      sb.AppendLine($"  Drop range: [{firstDrop}..{endDrop})");
      sb.AppendLine($"  Pre-MakeMove HashCode:  0x{preHash:X16}");
      sb.AppendLine($"  Post-UnmakeMove HashCode: 0x{postHash:X16}");

      if (DebugFlags.VerboseDiagnostics && preSnapshot != null && Board != null)
      {
        int n = Math.Min(preSnapshot.Length, Board.NumSquaresExtended);
        int diffs = 0;
        sb.AppendLine("  Per-square differences (pre vs post):");
        for (int sq = 0; sq < n; sq++)
        {
          Piece before = preSnapshot[sq];
          Piece after = Board[sq];
          if (!ReferenceEquals(before, after))
          {
            string beforeDesc = before == null ? "(empty)" : before.PieceType.Name + ":" + before.Player;
            string afterDesc = after == null ? "(empty)" : after.PieceType.Name + ":" + after.Player;
            sb.AppendLine($"    sq {sq} ({Board.GetDefaultSquareNotation(sq)}): {beforeDesc} -> {afterDesc}");
            diffs++;
            if (diffs >= 32)
            {
              sb.AppendLine("    ... (further differences truncated)");
              break;
            }
          }
        }
        if (diffs == 0)
          sb.AppendLine("    (none — only hash differs; possible Zobrist/side-to-move desync)");
      }

      sb.Append(MoveGenerationContext.FormatContextStack());

      // Defensive throw: BuildErrorMessage will read mi.PieceMoved.Square and
      // call Board.GetDefaultSquareNotation(square) on whatever we pass. After
      // a corrupting Make/Unmake round-trip those values can be out-of-range,
      // so we sanitize square to a safe value, do not pass a MoveInfo (so the
      // formatter skips the corrupted PieceMoved access), and fall back to a
      // plain Exception if construction still throws.
      int boardN = Board != null ? Board.NumSquaresExtended : 0;
      int safeSquare = (Board != null && mi.FromSquare >= 0 && mi.FromSquare < boardN) ? mi.FromSquare : 0;
      string safeNotation;
      try
      {
        safeNotation = Board != null ? Board.GetDefaultSquareNotation(safeSquare) : safeSquare.ToString();
      }
      catch
      {
        safeNotation = safeSquare.ToString();
      }

      InvalidBoardStateException toThrow;
      try
      {
        toThrow = new InvalidBoardStateException(sb.ToString(), safeSquare, safeNotation, Game);
      }
      catch (Exception buildEx)
      {
        throw new Exception(sb.ToString() + Environment.NewLine +
          "(Secondary failure while constructing InvalidBoardStateException: " + buildEx.Message + ")");
      }
      throw toThrow;
    }
    #endregion


    // *** PROTECTED DATA MEMBERS *** //

    protected MoveInfo[] moves;
    protected int moveCursor;
    protected Pickup[] pickups;
    protected int pickupCursor;
    protected Drop[] drops;
    protected int dropCursor;
    protected int[] moveOrder;
    protected int currentMoveIndex;
    protected SearchStack[] searchStack;
    protected UInt64[] killers1;
    protected UInt64[] killers2;
    protected UInt32[,,] historyCounters;
    protected UInt32[,,] butterflyCounters;
    protected int ply;
    protected UInt64 hashtableMoveHash;
    protected UInt64 countermove;
    private int tempPickupCursor;
    private int tempDropCursor;
    private int triedMovesCursor;

    protected static MoveInfo[] nullMoves;

  }
}
