
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

using System.Collections.Generic;
using System;

namespace ChessV
{
  public class BoardMoveStack
  {
    public Board Board { get; private set; }

    public int MoveCount
    { get { return moves.Count; } }

    public MoveInfo GetMove(int movenum)
    {
      return moves[movenum];
    }

    public BoardMoveStack(Board board)
    {
      Board = board;

      pickups = new List<Pickup>();
      drops = new List<Drop>();
      moves = new List<MoveInfo>();
    }

    public void MakingMove(MoveList movelist, MoveInfo moveinfo)
    {
      MoveInfo newmove = moveinfo;
      movelist.CopyMoveToGameHistory(pickups, drops, moveinfo);
      newmove.PickupCursor = pickups.Count;
      newmove.DropCursor = drops.Count;
      moves.Add(newmove);
    }

    public void UnmakeMove()
    {

      if (moves.Count > 0)
      {
        Board.Game.MoveBeingUnmade(moves[moves.Count - 1]);

        int pickupCursor = 0;
        int dropCursor = 0;
        if (moves.Count > 1)
        {
          pickupCursor = moves[moves.Count - 2].PickupCursor;
          dropCursor = moves[moves.Count - 2].DropCursor;
        }
        //	undo all drops
        for (int x = drops.Count; x > dropCursor; x--)
          UndoDrop();
        //	undo all pickups
        for (int x = pickups.Count; x > pickupCursor; x--)
          UndoPickup();
        moves.RemoveAt(moves.Count - 1);
      }
    }

    protected void UndoPickup()
    {
      if (pickups.Count == 0)
      {
        if (DebugFlags.ThrowOnInvariantViolation)
          throw new Exception("BoardMoveStack.UndoPickup: No pickups to undo!");
        return;
      }
      
      var pickup = pickups[pickups.Count - 1];
      
      // Check if there's actually a piece to restore before attempting to restore it
      if (pickup.Piece == null)
      {
        pickups.RemoveAt(pickups.Count - 1); // maintain state
        return;
      }
      
      // Check if the target square is already occupied
      if (Board[pickup.Square] != null)
      {
        if (DebugFlags.ThrowOnInvariantViolation)
        {
          var existingPiece = Board[pickup.Square];
          throw new Exception($"BoardMoveStack.UndoPickup: Cannot restore {pickup.Piece.PieceType.Name} (Player {pickup.Piece.Player}) to square {Board.GetDefaultSquareNotation(pickup.Square)} - already occupied by {existingPiece.PieceType.Name} (Player {existingPiece.Player})");
        }
        // Skip restoring to avoid double occupancy, but still drop the pickup to keep cursors consistent
        pickups.RemoveAt(pickups.Count - 1);
        return;
      }
      
      Board.SetSquare(pickup.Piece, pickup.Square);
      pickups.RemoveAt(pickups.Count - 1);
    }

    protected void UndoDrop()
    {
      if (drops.Count == 0)
      {
        if (DebugFlags.ThrowOnInvariantViolation)
          throw new Exception("BoardMoveStack.UndoDrop: No drops to undo!");
        return;
      }
      
      var drop = drops[drops.Count - 1];
      
      // Check if there's actually a piece to clear before attempting to clear it
      if (Board[drop.Square] == null)
      {
        if (DebugFlags.ThrowOnInvariantViolation)
          throw new Exception($"BoardMoveStack.UndoDrop: No piece found at square {Board.GetDefaultSquareNotation(drop.Square)} to clear. Expected to find {drop.Piece.PieceType.Name} (Player {drop.Piece.Player}). Current board state around square:\n{GetBoardStateAroundSquare(drop.Square)}");
        // Nothing to clear; consider it already undone
        drops.RemoveAt(drops.Count - 1);
        return;
      }
      
      var pieceAtSquare = Board[drop.Square];
      if (pieceAtSquare != drop.Piece)
      {
        if (DebugFlags.ThrowOnInvariantViolation)
          throw new Exception($"BoardMoveStack.UndoDrop: Expected to find {drop.Piece.PieceType.Name} (Player {drop.Piece.Player}) at square {Board.GetDefaultSquareNotation(drop.Square)}, but found {pieceAtSquare.PieceType.Name} (Player {pieceAtSquare.Player}) instead. Current board state around square:\n{GetBoardStateAroundSquare(drop.Square)}");
        // Best-effort: do not clear a different piece; consider this drop already reconciled
        drops.RemoveAt(drops.Count - 1);
        return;
      }
      
      Board.ClearSquare(drop.Square);
      drop.Piece.MoveCount--;
      if (drop.NewType != null)
      {
        PieceType oldType = drop.NewType;
        drop.Piece.PieceType = oldType;
        drop.Piece.TypeNumber = oldType.TypeNumber;
      }
      drops.RemoveAt(drops.Count - 1);
    }
    
    private string GetBoardStateAroundSquare(int square)
    {
      var sb = new System.Text.StringBuilder();
      var location = Board.SquareToLocation(square);
      for (int r = location.Rank - 1; r <= location.Rank + 1; r++)
      {
        for (int f = location.File - 1; f <= location.File + 1; f++)
        {
          if (r >= 0 && r < Board.NumRanks && f >= 0 && f < Board.NumFiles)
          {
            int sq = Board.LocationToSquare(new ChessV.Location(r, f));
            var piece = Board[sq];
            string pieceInfo = piece != null ? $"{piece.PieceType.Name[0]}{piece.Player}" : "..";
            sb.Append($"{Board.GetDefaultSquareNotation(sq)}:{pieceInfo} ");
          }
        }
        sb.AppendLine();
      }
      return sb.ToString();
    }

    protected List<Pickup> pickups;
    protected List<Drop> drops;
    protected List<MoveInfo> moves;
  }
}
