
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
  public class MoveList
  {
    // *** PROPERTIES *** //

    public Board Board { get; private set; }

    public Game Game { get; private set; }

    // This is the maximum number of moves that can be stored, but the implementation of nonstandard moves ("Special Moves")
    // means that the actual number of turns we can look forward can be less. A pickup and drop is 2 moves, in 1 ply.
    public const int MAX_MOVES = 2048;

    public bool LegalMovesOnly { get; set; }

    public int Count { get { return moveCursor; } }

    public MoveInfo CurrentMove { get { return moves[currentMoveIndex]; } }


    // *** CONSTRUCTION *** //

    #region Constructor
    public MoveList
      (Board board,
        SearchStack[] searchStack,
        UInt32[] killers1,
        UInt32[] killers2,
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
    public void Reset(UInt32 hashtableMoveHash = 0, UInt32 countermove = 0)
    {
      moveCursor = 0;
      pickupCursor = 0;
      dropCursor = 0;
      triedMovesCursor = -1;
      this.hashtableMoveHash = hashtableMoveHash;
      this.countermove = countermove;
    }
    #endregion

    #region Restart
    public void Restart(UInt32 pvMove)
    {
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
    public void ReorderMoves(Dictionary<UInt32, int> moveScores)
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
    public MoveInfo FindMove(Int32 movehash)
    {
      for (int x = 0; x < moveCursor; x++)
        if (moves[x] == movehash)
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
            succeeded = MakeMove(moveOrder[bestMoveIndex]);
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
      pickups[index].Piece = Board.ClearSquare(pickups[index].Square);
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
      Board.SetSquare(pickups[index].Piece, pickups[index].Square);
    }
    #endregion

    #region UndoDrop
    protected void UndoDrop(int index)
    {
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
    public void AddMove(int fromSquare, int toSquare, bool direct = false)
    {
      if (!direct && Board.Game.MoveBeingGenerated(this, fromSquare, toSquare, MoveType.StandardMove))
        return;

      if (Game.DeduplicateMoves)
        //	Game requires move deduplication, so we must check to 
        //	ensure that we don't have this move already
        for (int x = 0; x < moveCursor; x++)
          if (moves[x].MoveType == MoveType.StandardMove &&
            moves[x].FromSquare == fromSquare &&
            moves[x].ToSquare == toSquare)
            //	we have this move already so don't add it again
            return;

      Piece pieceBeingMoved = Board[fromSquare];

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
        bool legal = MakeMove(moveCursor - 1);
        UnmakeMove(moveCursor - 1);
        if (!legal)
        {
          moveCursor--;
          pickupCursor--;
          dropCursor--;
        }
      }
    }

    public void AddMove(string fromSquareNotation, string toSquareNotation, bool direct = false)
    {
      AddMove(Board.Game.NotationToSquare(fromSquareNotation), Board.Game.NotationToSquare(toSquareNotation), direct);
    }
    #endregion

    #region AddCapture
    public void AddCapture(int fromSquare, int toSquare, bool direct = false)
    {
      if (!direct && Board.Game.MoveBeingGenerated(this, fromSquare, toSquare, MoveType.StandardCapture))
        return;

      if (Game.DeduplicateMoves)
        //	Game requires move deduplication, so we must check to 
        //	ensure that we don't have this move already
        for (int x = 0; x < moveCursor; x++)
          if (moves[x].MoveType == MoveType.StandardCapture &&
            moves[x].FromSquare == fromSquare &&
            moves[x].ToSquare == toSquare)
            //	we have this move already so don't add it again
            return;

      Piece pieceBeingMoved = Board[fromSquare];
      Piece pieceBeingCaptured = Board[toSquare];

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
        bool legal = MakeMove(moveCursor - 1);
        UnmakeMove(moveCursor - 1);
        if (!legal)
        {
          moveCursor--;
          pickupCursor -= 2;
          dropCursor--;
        }
      }
    }

    public void AddCapture(string fromSquareNotation, string toSquareNotation, bool direct = false)
    {
      AddCapture(Board.Game.NotationToSquare(fromSquareNotation), Board.Game.NotationToSquare(toSquareNotation), direct);
    }
    #endregion

    #region AddRifleCapture
    public void AddRifleCapture(int fromSquare, int toSquare, bool direct = false)
    {
      if (!direct && Board.Game.MoveBeingGenerated(this, fromSquare, toSquare, MoveType.BaroqueCapture))
        return;

      Piece pieceBeingMoved = Board[fromSquare];
      Piece pieceBeingCaptured = Board[toSquare];

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
        }
      }
    }
    #endregion

    #region BeginMoveAdd
    public void BeginMoveAdd
      (MoveType moveType,
        int fromSquare,
        int toSquare,
        int tag = 0)
    {
      moves[moveCursor].MoveType = moveType;
      moves[moveCursor].Tag = tag;

      if (fromSquare != -1)
      {
        moves[moveCursor].Player = Board[fromSquare].Player;
        moves[moveCursor].FromSquare = fromSquare;
        moves[moveCursor].ToSquare = toSquare;
        moves[moveCursor].OriginalType = Board[fromSquare].PieceType.TypeNumber;
        moves[moveCursor].PieceMoved = Board[fromSquare];
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
      pickups[pickupCursor].Piece = null;
      pickups[pickupCursor++].Square = square;
      Piece pieceOnSquare = Board[square];
      moves[moveCursor].PieceCaptured = pieceOnSquare;
      if (pieceOnSquare.Player != Board.Game.CurrentSide)
        moves[moveCursor].PieceCaptured = pieceOnSquare;
      return pieceOnSquare;
    }
    #endregion

    #region AddDrop
    public void AddDrop
      (Piece piece,
        int square,
        PieceType newType)
    {
      if (moves[moveCursor].PieceCaptured == piece)
        moves[moveCursor].PieceCaptured = null;
      drops[dropCursor].NewType = newType;
      drops[dropCursor].Piece = piece;
      drops[dropCursor++].Square = square;
      if (newType != null)
        moves[moveCursor].PromotionType = newType.TypeNumber;
    }

    public void AddDrop
      (Piece piece,
        int square)
    {
      AddDrop(piece, square, null);
    }
    #endregion

    #region EndMoveAdd
    public void EndMoveAdd(int evaluation)
    {
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

      if (LegalMovesOnly)
      {
        bool legal = MakeMove(moveCursor - 1);
        UnmakeMove(moveCursor - 1);
        if (!legal)
        {
          moveCursor--;
          pickupCursor = tempPickupCursor;
          dropCursor = tempDropCursor;
        }
      }
    }
    #endregion

    #region MakeMove
    public bool MakeMove(int index)
    {
      int firstPickup = 0;
      int firstDrop = 0;
      if (index > 0)
      {
        firstPickup = moves[index - 1].PickupCursor;
        firstDrop = moves[index - 1].DropCursor;
      }
      int nCaptures = (moves[index].PickupCursor - firstPickup) - (moves[index].DropCursor - firstDrop);

      //	perform all pickups
      for (int pickup = firstPickup; pickup < moves[index].PickupCursor; pickup++)
      {
        PerformPickup(pickup);
        if (moves[index].PickupCursor - pickup <= nCaptures)
        {
        }
      }

      //	perform all drops (normally only one, but Castling is one exception)
      for (int drop = firstDrop; drop < moves[index].DropCursor; drop++)
        PerformDrop(drop);

      //	ok, now pass message to the Game class, so it can update any info
      //	it may need to as a result of this move.  this also gives the Game
      //	class the chance to return false, indicating that the move is illegal
      return Board.Game.MoveBeingMade(moves[index]);
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
    protected UInt32[] killers1;
    protected UInt32[] killers2;
    protected UInt32[,,] historyCounters;
    protected UInt32[,,] butterflyCounters;
    protected int ply;
    protected UInt32 hashtableMoveHash;
    protected UInt32 countermove;
    private int tempPickupCursor;
    private int tempDropCursor;
    private int triedMovesCursor;

    protected static MoveInfo[] nullMoves;

  }
}
