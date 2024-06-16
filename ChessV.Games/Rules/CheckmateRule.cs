
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
using System.Linq;

namespace ChessV.Games.Rules
{
  public class CheckmateRule : Rule
  {
    // *** PROPERTIES *** //

    public MoveEventResponse StalemateResult { get; set; }

    public HashSet<Piece>[] RoyalPieces;

    public PieceType[] RoyalPieceTypes { get; private set; }

    public PieceType RoyalPieceType { get { return RoyalPieceTypes.First(); } }


    // *** CONSTRUCTION ** //

    public CheckmateRule(PieceType royalPieceType)
    {
      RoyalPieceTypes = new PieceType[1] { royalPieceType };
      StalemateResult = MoveEventResponse.GameDrawn;
    }

    public CheckmateRule(PieceType[] royalPieceTypes)
    {
      RoyalPieceTypes = royalPieceTypes;
      StalemateResult = MoveEventResponse.GameDrawn;
    }

    public override void Initialize(Game game)
    {
      RoyalPieces = new HashSet<Piece>[game.NumPlayers];
      base.Initialize(game);
      foreach (PieceType royalType in RoyalPieceTypes)
        if (royalType.FindCustomAttributes(typeof(RoyalAttribute)).Count == 0)
          royalType.AddAttribute(new RoyalAttribute());
    }

    // *** OVERRIDES *** //

    public override void RuleRemoved()
    {
      foreach (PieceType royalType in RoyalPieceTypes)
        royalType.RemoveCustomAttributes(typeof(RoyalAttribute));
    }

    public override void PositionLoaded(FEN fen)
    {
      //	scan the board for the royal pieces
      for (int player = 0; player < Game.NumPlayers; player++)
      {
        List<Piece> piecelist = Game.GetPieceList(player);
        foreach (Piece piece in piecelist)
          if (RoyalPieceTypes.Contains(piece.PieceType))
          {
            if (RoyalPieces[player] == null)
              RoyalPieces[player] = new HashSet<Piece>();
            RoyalPieces[player].Add(piece);
          }
      }
    }

    public override MoveEventResponse MoveBeingMade(MoveInfo move, int ply)
    {
      //	Assert that this move doesn't capture a royal piece, 
      //	otherwise we have a fundamental problem!
      if (move.PieceCaptured != null &&
          RoyalPieces.Any(playerRoyals => playerRoyals.Contains(move.PieceCaptured)))
        throw new Exception("Fatal error in CheckmateRule - Royal piece captured");
      return IllegalCheckMoves(move);
    }

    protected MoveEventResponse IllegalCheckMoves(MoveInfo move)
    {
      //	Make sure that as a result of this move, no moving player's
      //	royal piece is attacked.  If it is, this move is illegal.
      foreach (Piece royalPiece in RoyalPieces[move.Player])
        if (royalPiece != null && Game.IsSquareAttacked(royalPiece.Square, move.Player ^ 1))
          return MoveEventResponse.IllegalMove;
      return MoveEventResponse.NotHandled;
    }

    public override MoveEventResponse NoMovesResult(int currentPlayer, int ply)
    {
      if (RoyalPieces[currentPlayer].Where(p => p != null).Count() > 1)
        return 0;
      Piece royalPiece = RoyalPieces[currentPlayer].First(p => p != null);
      //	No moves - if the royal piece is attacked, the game is lost;
      //	Otherwise, return the StalemateResult
      if (Game.IsSquareAttacked(royalPiece.Square, currentPlayer ^ 1))
        return MoveEventResponse.GameLost;
      return StalemateResult;
    }

    public override int PositionalSearchExtension(int currentPlayer, int ply)
    {
      if (RoyalPieces[currentPlayer].Where(p => p != null).Count() > 1)
        return 0;
      Piece royalPiece = RoyalPieces[currentPlayer].First(p => p != null);
      if (Game.IsSquareAttacked(royalPiece.Square, currentPlayer ^ 1))
        //	king is in check - extend by one ply
        return Game.ONEPLY;
      return 0;
    }

    public override void GetNotesForPieceType(PieceType type, List<string> notes)
    {
      if (RoyalPieceTypes.Contains(type))
        notes.Add("royal");
    }
  }
}
