
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
  //**********************************************************************
  //
  //                             Piece
  //
  //    Extends the GenericPiece class to give the Piece some knowledge 
  //    of its context in the game being played.  

  public class Piece : GenericPiece
  {
    // *** PROPERTIES *** //

    //	Reference to the Game the piece is participating in
    public Game Game { get; private set; }

    //	Reference to the Board the piece is on, or would be on 
    //	if not presently captured or off-board
    public Board Board { get; private set; }

    //	The game-specific type number of the type of this piece.
    //	All piece types used in a given game are given a sequential 
    //	number for efficient lookup and identification
    public int TypeNumber { get; set; }

    //	The zero-based square number if on-board presently or -1
    public int Square { get; set; }

    //	The Location of the piece if presently on-board or will 
    //	throw an exception otherwise
    public Location Location
    {
      get { return Board.SquareToLocation(Square); }
      set { Square = Board.LocationToSquare(value); }
    }

    //	The total number of moves the piece has made this game
    public int MoveCount { get; set; }

    public int MidgameValue
    { get { return PieceType.MidgameValue; } }

    public int EndgameValue
    { get { return PieceType.EndgameValue; } }


    // *** CONSTRUCTION *** //

    // TEST ONLY
    public Piece() { }

    #region Constructors
    public Piece(Game game, GenericPiece genericPiece, int square) :
      base(genericPiece)
    {
      Game = game;
      Board = game.Board;
      Square = square;
      MoveCount = 0;
      TypeNumber = Game.GetPieceTypeNumber(genericPiece.PieceType);
    }

    public Piece(Game game, int player, PieceType type, int square) :
      base(player, type)
    {
      Game = game;
      Board = game.Board;
      Player = player;
      PieceType = type;
      Square = square;
      TypeNumber = Game.GetPieceTypeNumber(type);
      MoveCount = 0;
    }

    public Piece(Game game, int player, PieceType type, Location location) :
      base(player, type)
    {
      Game = game;
      Board = game.Board;
      Player = player;
      PieceType = type;
      Location = location;
      MoveCount = 0;
      Square = Board.LocationToSquare(location);
      TypeNumber = Game.GetPieceTypeNumber(type);
    }
    #endregion


    // *** OVERLOADED OPERATORS *** //

    #region Overloaded operators
    public static bool operator ==(Piece p1, Piece p2)
    { return System.Object.ReferenceEquals(p1, p2); }

    public static bool operator !=(Piece p1, Piece p2)
    { return !System.Object.ReferenceEquals(p1, p2); }

    public new bool Equals(GenericPiece other)
    { return System.Object.ReferenceEquals(this, other); }

    public override bool Equals(object obj)
    {
      if (obj is Piece)
        return Equals((Piece)obj);
      return false;
    }

    public override int GetHashCode()
    { return this.Square + this.TypeNumber << 10 + MoveCount << 17; }
    #endregion


    // *** OPERATIONS *** //

    #region GetAttackRange
    //	Look up maximum attack range in the given direction
    public int GetAttackRange(int nDirection)
    { return PieceType.AttackRangePerDirection[Game.PlayerDirection(Player, nDirection)]; }
    #endregion

    #region GetCannonAttackRange
    public int GetCannonAttackRange(int nDireciton)
    { return PieceType.CannonAttackRangePerDirection[Game.PlayerDirection(Player, nDireciton)]; }
    #endregion

    #region GenerateMoves
    //	Generate moves for this piece and add to specified MoveList
    public void GenerateMoves(MoveList list, bool capturesOnly)
    {
      GenerateMoves(PieceType, list, capturesOnly);
    }

    //	Generate moves for this piece assuming it is of the given 
    //	type (which it usually is.)  This function is useful for polymorphic 
    //	piece types that can move as other types of pieces.
    public void GenerateMoves(PieceType pieceType, MoveList list, bool capturesOnly)
    {
      //	if the piece type has a custom move generator, run that first.
      //	it will determine if we proceed to standard generation
      if (pieceType.CustomMoveGenerator != null)
        if (!pieceType.CustomMoveGenerator(pieceType, this, list, capturesOnly))
          //	custom generator returned false, so we don't 
          //	proceed to standard move generation
          return;

      MoveCapability[] moves;
      int nMoves = pieceType.GetMoveCapabilities(out moves);
      for (int nMove = 0; nMove < nMoves; nMove++)
        if (moves[nMove].ConditionalBySquare == null || moves[nMove].ConditionalBySquare[Player][Square])
          GenerateMovesForCapability(pieceType.SimpleMoveGeneration, ref moves[nMove], list, capturesOnly);
    }

    #region GenerateMovesForCapability
    public void GenerateMovesForCapability(bool simpleMoveGeneration, ref MoveCapability move, MoveList list, bool capturesOnly)
    {
      List<int> inspectionPath = list.IsInspection ? new List<int> { Square } : null;
      bool isLeap = move.PathInfo == null &&
        Math.Max(Math.Abs(move.Direction.RankOffset), Math.Abs(move.Direction.FileOffset)) > 1;
      if (simpleMoveGeneration)
      {
        #region Handle simple move generation
        int step = 1;
        int nextSquare = Game.Board.NextSquare(Player, move.NDirection, Square);
        while (nextSquare >= 0 && step <= move.MaxSteps)
        {
          inspectionPath?.Add(nextSquare);
          Piece pieceOnSquare = Board[nextSquare];
          if (pieceOnSquare != null)
          {
            if (step >= move.MinSteps && move.CanCapture && pieceOnSquare.Player != Player)
              AddInspectedCapture(list, nextSquare, inspectionPath, -1, isLeap);
            nextSquare = -1;
          }
          else
          {
            if (step >= move.MinSteps && !move.MustCapture && !capturesOnly)
              list.AddMove(Square, nextSquare);
            nextSquare = Game.Board.NextSquare(Player, move.NDirection, nextSquare);
            step++;
          }
        }
        #endregion
      }
      else
      {
        #region Handle pieces with the more complex moves
        int step = 1;
        bool passedScreen = false;
        int screenSquare = -1;
        int nextSquare = Game.Board.NextSquare(Player, move.NDirection, Square);
        while (nextSquare >= 0 && step <= move.MaxSteps)
        {
          inspectionPath?.Add(nextSquare);
          Piece pieceOnSquare = Board[nextSquare];

          #region Handle pieces with move paths
          if (move.PathInfo != null)
          {
            if (pieceOnSquare == null && capturesOnly)
              //	don't bother trying to generate this non-capture
              return;

            if (pieceOnSquare != null && pieceOnSquare.Player == Player)
              //	can't capture own piece
              return;

            //	ensure this move is supported
            if (step > 1 || move.SpecialAttacks != 0 || move.PathInfo.AllowMultiCapture == true)
              throw new Exception("Piece type " + PieceType.Name + " has an unsupported movement capability");
            //	iterate through the paths to see if one is available
            foreach (List<int> path in move.PathInfo.PathNDirections)
            {
              int pathSquare = Square;
              List<int> inspectedRoute = list.IsInspection ? new List<int> { Square } : null;
              //	iterate through each step in this path to see if it is clear
              bool pathIsClear = true;
              foreach (int nDirection in path)
              {
                pathSquare = Game.Board.NextSquare(Player, nDirection, pathSquare);
                inspectedRoute?.Add(pathSquare);
                if (pathSquare < 0 || (Board[pathSquare] != null && Board[pathSquare] != pieceOnSquare))
                {
                  //	this path is obstructed
                  pathIsClear = false;
                  break;
                }
              }
              if (pathIsClear)
              {
                if (pieceOnSquare != null && move.CanCapture)
                  AddInspectedCapture(list, nextSquare, inspectedRoute, -1, false);
                else if (pieceOnSquare == null)
                  list.AddMove(Square, nextSquare);
                //	we can return now; no need to try other paths
                return;
              }
            }
            //	if we are here, all paths are blocked
            return;
          }
          #endregion

          if (pieceOnSquare != null)
          {
            if (step >= move.MinSteps && pieceOnSquare.Player != Player &&
              (move.CanCapture ||
               ((move.SpecialAttacks & SpecialAttacks.CannonCapture) != 0 && passedScreen)))
            {
              AddInspectedCapture(list, nextSquare, inspectionPath, screenSquare, isLeap,
                move.SpecialAttacks == SpecialAttacks.RifleCapture);
            }
            if ((move.SpecialAttacks & SpecialAttacks.CannonCapture) != 0 && !passedScreen)
            {
              passedScreen = true;
              screenSquare = nextSquare;
              nextSquare = Game.Board.NextSquare(Player, move.NDirection, nextSquare);
            }
            else
              nextSquare = -1;
          }
          else
          {
            if (step >= move.MinSteps && !move.MustCapture && !capturesOnly && !passedScreen)
              list.AddMove(Square, nextSquare);
            nextSquare = Game.Board.NextSquare(Player, move.NDirection, nextSquare);
            step++;
          }
        }

        #endregion
      }
    }
    #endregion
    #endregion

    private void AddInspectedCapture(MoveList list, int toSquare, List<int> path,
      int screenSquare, bool isLeap, bool rifle = false)
    {
      int firstMove = list.Count;
      if (rifle)
        list.AddRifleCapture(Square, toSquare);
      else
        list.AddCapture(Square, toSquare);
      if (list.IsInspection)
        list.RecordCapturePath(firstMove, new CapturePath(path, screenSquare, isLeap));
    }
  }
}
