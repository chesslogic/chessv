
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

using ChessV.Games.Rules.Apmw;
using ChessV.Games.Rules.Extinction;
using System;
using System.Collections.Generic;

namespace ChessV.Games.Rules
{
  public class CastlingRule : Rule
  {
    const int MAX_CASTLING_MOVES = 16;

    public sealed class Restriction
    {
      public int Square { get; }
      public bool Occupied { get; }
      public bool Attacked { get; }

      internal Restriction(int square, bool occupied, bool attacked)
      {
        Square = square;
        Occupied = occupied;
        Attacked = attacked;
      }
    }

    public IReadOnlyList<Restriction> GetRestrictions(Piece king)
    {
      if (king == null || king.Game != Game)
        throw new ArgumentException("Castling inspection requires this game's king.", nameof(king));
      var restrictions = new Dictionary<int, Restriction>();
      // Flexible and Alice castling have different transit semantics.
      if (GetType() != typeof(CastlingRule))
        return new List<Restriction>().AsReadOnly();
      int privileges = gameHistoryPrivs[Game.GameMoveNumber];
      for (int index = 0; index < nCastlingMoves[king.Player]; index++)
      {
        CastlingMove move = castlingMoves[king.Player, index];
        if (move.KingFromSquare != king.Square || (move.RequiredPriv & privileges) == 0)
          continue;
        foreach (int square in RequiredEmptySquares(move))
          if (Board[square] != null)
            restrictions[square] = new Restriction(square, true, false);
        if (hasCheckmateRule)
          foreach (int square in KingPathSquares(move))
            if (Game.IsSquareAttacked(square, king.Player ^ 1))
              restrictions[square] = new Restriction(square,
                restrictions.ContainsKey(square) && restrictions[square].Occupied, true);
      }
      return new List<Restriction>(restrictions.Values).AsReadOnly();
    }

    private IEnumerable<int> RequiredEmptySquares(CastlingMove move)
    {
      int minimum = Math.Min(Math.Min(move.KingFromSquare, move.KingToSquare),
        Math.Min(move.OtherFromSquare, move.OtherToSquare));
      int maximum = Math.Max(Math.Max(move.KingFromSquare, move.KingToSquare),
        Math.Max(move.OtherFromSquare, move.OtherToSquare));
      for (int file = Board.GetFile(minimum); file <= Board.GetFile(maximum); file++)
      {
        int square = file * Board.NumRanks + Board.GetRank(minimum);
        if (square != move.KingFromSquare && square != move.OtherFromSquare)
          yield return square;
      }
    }

    private IEnumerable<int> KingPathSquares(CastlingMove move)
    {
      int firstFile = Board.GetFile(move.KingFromSquare);
      int lastFile = Board.GetFile(move.KingToSquare);
      int step = firstFile <= lastFile ? 1 : -1;
      for (int file = firstFile; ; file += step)
      {
        yield return file * Board.NumRanks + Board.GetRank(move.KingFromSquare);
        if (file == lastFile)
          break;
      }
    }

    #region CastlingMove helper struct
    protected struct CastlingMove
    {
      public int KingFromSquare;
      public int KingToSquare;
      public int OtherFromSquare;
      public int OtherToSquare;
      public int RequiredPriv;
      /** A character representing one privilege to castle, e.g. K for white's kingside castle */
      public char PrivChar;

      public CastlingMove(int kingfrom, int kingto, int otherfrom, int otherto, int priv, char privChar)
      { KingFromSquare = kingfrom; KingToSquare = kingto; OtherFromSquare = otherfrom; OtherToSquare = otherto; RequiredPriv = priv; PrivChar = privChar; }
    }
    #endregion


    // *** CONSTRUCTION *** //

    #region Construction
    public CastlingRule()
    {
      privLookup = new Dictionary<char, int>();
    }

    public CastlingRule(CastlingRule template)
    {
      Initialize(template.Game);
      privLookup = template.privLookup;
      allPrivsPerPlayer = template.allPrivsPerPlayer;
      allPrivString = template.allPrivString;
      nextPriv = template.nextPriv;
      castlingMoves = template.castlingMoves;
      nCastlingMoves = template.nCastlingMoves;
    }
    #endregion


    // *** INITIALIZATION *** //

    #region Initialize
    public override void Initialize(Game game)
    {
      base.Initialize(game);
      castlingMoves = new CastlingMove[game.NumPlayers, MAX_CASTLING_MOVES];
      nCastlingMoves = new int[game.NumPlayers];
      searchStackPrivs = new int[Game.MAX_PLY];
      gameHistoryPrivs = new int[Game.MAX_GAME_LENGTH];
      allPrivsPerPlayer = new int[game.NumPlayers];
      allPrivString = "";
      nextPriv = 1;
      privEraseMap = new int[game.Board.NumSquaresExtended];

      //	Hook up MoveBeingPlayed event handler
      game.MoveBeingPlayed += MoveBeingPlayedHandler;
    }
    #endregion

    #region PostInitialize
    public override void PostInitialize()
    {
      hashKeyIndex = Game.HashKeys.TakeKeys(nextPriv - 1);
      hasCheckmateRule = Game.FindRule(typeof(CheckmateRule), true) != null;
      for (int square = 0; square < Game.Board.NumSquares; square++)
      {
        int flagvalue = -1;
        for (int player = 0; player < Game.NumPlayers; player++)
        {
          for (int move = 0; move < nCastlingMoves[player]; move++)
          {
            if (castlingMoves[player, move].KingFromSquare == square ||
              castlingMoves[player, move].OtherFromSquare == square)
              //	moving from this square erases this castling priv
              flagvalue = flagvalue & ~castlingMoves[player, move].RequiredPriv;
          }
        }
        privEraseMap[square] = flagvalue;
      }
      for (int square = Game.Board.NumSquares; square < Game.Board.NumSquaresExtended; square++)
        privEraseMap[square] = -1;
    }
    #endregion

    #region ClearGameState
    public override void ClearGameState()
    {
      for (int x = 0; x < Game.MAX_PLY; x++)
        searchStackPrivs[x] = 0;
      for (int x = 0; x < Game.MAX_GAME_LENGTH; x++)
        gameHistoryPrivs[x] = 0;
    }
    #endregion


    // *** OPERATIONS *** //

    #region AddCastlingMove
    public void AddCastlingMove(int player, int kingfrom, int kingto, int otherfrom, int otherto, char privChar)
    {
      int priv = 0;
      if (privLookup.ContainsKey(privChar))
        priv = privLookup[privChar];
      else
      {
        priv = nextPriv;
        nextPriv = nextPriv << 1;
        privLookup.Add(privChar, priv);
      }
      if (allPrivString.IndexOf(privChar) < 0)
        allPrivString += privChar;
      allPrivsPerPlayer[player] |= priv;
      castlingMoves[player, nCastlingMoves[player]++] = new CastlingMove(kingfrom, kingto, otherfrom, otherto, priv, privChar);
    }
    #endregion


    // *** OVERRIDES *** //

    #region GetPositionHashCode
    public override ulong GetPositionHashCode(int ply)
    {
      int castlingPriv = ply == 1 ? gameHistoryPrivs[Game.GameMoveNumber] : searchStackPrivs[ply - 1];
      return HashKeys.Keys[hashKeyIndex + castlingPriv];
    }
    #endregion

    #region SetDefaultsInFEN
    public override void SetDefaultsInFEN(FEN fen)
    {
      if (fen["castling"] == "#default")
        fen["castling"] = allPrivString;
    }
    #endregion

    #region PositionLoaded
    public override void PositionLoaded(FEN fen)
    {
      searchStackPrivs[0] = 0;
      string castling = fen["castling"];
      if (castling != "-")
      {
        foreach (char c in castling)
          if (privLookup.ContainsKey(c))
            searchStackPrivs[0] |= privLookup[c];
          else
            throw new Exceptions.FENParseFailureException("castling", fen["castling"],
              "Invalid character in FEN castling privileges: " + c);
      }
      gameHistoryPrivs[Game.GameMoveNumber] = searchStackPrivs[0];
    }
    #endregion

    #region SavePositionToFEN
    public override void SavePositionToFEN(FEN fen)
    {
      int castlingPriv = gameHistoryPrivs[Game.GameMoveNumber];
      string privString = "";
      foreach (var pair in privLookup)
        if ((pair.Value & castlingPriv) != 0)
          privString += pair.Key;
      if (privString == "")
        privString = "-";
      fen["castling"] = privString;
    }
    #endregion

    #region MoveBeingPlayedHandler
    public void MoveBeingPlayedHandler(MoveInfo move)
    {
      gameHistoryPrivs[Game.GameMoveNumber] = searchStackPrivs[1];
      searchStackPrivs[0] = searchStackPrivs[1];
    }
    #endregion

    #region MoveBeingMade
    public override MoveEventResponse MoveBeingMade(MoveInfo move, int ply)
    {
      //	remove privs associated with the move from and to squares
      int privsToErase = privEraseMap[move.FromSquare] & privEraseMap[move.ToSquare];
      //	if this move also has a baroque capture on another square (for example, 
      //	a capture-by-advance) then we need to remove any priv associated with that square too
      if ((move.MoveType & MoveType.CaptureProperty) != 0 && (move.MoveType & MoveType.BaroqueCaptureProperty) != 0)
        privsToErase &= privEraseMap[move.Tag];
      searchStackPrivs[ply] =
        (ply == 1 ? gameHistoryPrivs[Game.GameMoveNumber] : searchStackPrivs[ply - 1]) & privsToErase;
      if (ply == 1)
        gameHistoryPrivs[Game.GameMoveNumber + 1] = searchStackPrivs[1];
      return MoveEventResponse.MoveOk;
    }
    #endregion

    #region GenerateSpecialMoves
    public override void GenerateSpecialMoves(MoveList list, bool capturesOnly, int ply)
    {
      if (!capturesOnly)
      {
        int castlingPriv = ply == 1 ? gameHistoryPrivs[Game.GameMoveNumber] : searchStackPrivs[ply - 1];
        for (int x = 0; x < nCastlingMoves[Game.CurrentSide]; x++)
        {
          if ((castlingMoves[Game.CurrentSide, x].RequiredPriv & castlingPriv) != 0)
          {
            bool squaresEmpty = true;
            foreach (int square in RequiredEmptySquares(castlingMoves[Game.CurrentSide, x]))
              if (Board[square] != null)
              {
                squaresEmpty = false;
                break;
              }
            if (squaresEmpty)
            {
              //	make sure squares the King is passes through are not attacked
              bool squaresAttacked = false;
              if (hasCheckmateRule)
                foreach (int square in KingPathSquares(castlingMoves[Game.CurrentSide, x]))
                  if (Game.IsSquareAttacked(square, Game.CurrentSide ^ 1))
                  {
                    squaresAttacked = true;
                    break;
                  }

              if (!squaresAttacked)
              {
                //	required squares are empty and not attacked so the move is legal - add it
                if (Board[castlingMoves[Game.CurrentSide, x].KingFromSquare] == null)
                  throw new Exception("No king to pick up from "
                  + castlingMoves[Game.CurrentSide, x].KingFromSquare
                  + " at "
                  + Board.GetDefaultSquareNotation(castlingMoves[Game.CurrentSide, x].KingFromSquare)
                  + "!");
                list.BeginMoveAdd(MoveType.Castling, castlingMoves[Game.CurrentSide, x].KingFromSquare,
                  castlingMoves[Game.CurrentSide, x].KingToSquare);
                Piece king = list.AddPickup(castlingMoves[Game.CurrentSide, x].KingFromSquare);
                Piece other = list.AddPickup(castlingMoves[Game.CurrentSide, x].OtherFromSquare);
                list.AddDrop(king, castlingMoves[Game.CurrentSide, x].KingToSquare, null);
                list.AddDrop(other, castlingMoves[Game.CurrentSide, x].OtherToSquare, null);
                list.EndMoveAdd(100);
              }
            }
          }
        }
      }
    }
    #endregion

    #region GetNotesForPieceType
    public override void GetNotesForPieceType(PieceType type, List<string> notes)
    {
      if (Board[castlingMoves[0, 0].KingFromSquare] != null &&
        Board[castlingMoves[0, 0].KingFromSquare].PieceType == type)
        notes.Add("can castle");
    }
    #endregion


    // *** PROTECTED DATA MEMBERS *** //

    protected CastlingMove[,] castlingMoves;
    protected int[] nCastlingMoves;
    protected int[] searchStackPrivs;
    protected int[] gameHistoryPrivs;
    protected int[] allPrivsPerPlayer;
    protected Dictionary<char, int> privLookup;
    protected int nextPriv;
    protected int[] privEraseMap;
    protected string allPrivString;
    protected bool hasCheckmateRule;
    protected int hashKeyIndex;
  }
}
