
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

using ChessV.Base;
using System;
using System.Linq;

namespace ChessV.Games.Rules.Cards
{
  public class CardDropRule : Rule
  {
    protected int handSize; // TODO(chesslogic): why is this 32
    protected int[] pocketSquares;

    protected static Game game;

    public int Range { get; }

    public CardDropRule(int range = 6)
    {
      Range = range;
    }

    public override void Initialize(Game game)
    {
      base.Initialize(game);
      handSize = (game.Board.NumSquaresExtended - game.Board.NumSquares) / 2;
      pocketSquares = new int[handSize * 2];
      for (int player = 0; player < game.NumPlayers; player++)
      {
        for (int card = 1; card <= handSize; card++)
        {
          int index = player * handSize + card - 1;
          pocketSquares[index] = Board.LocationToSquare(new Location(player, -card));
        }
      }
    }

    public override void PostInitialize()
    {

    }

    public override void PositionLoaded(FEN fen)
    {
      int squares = 0;
      foreach (char c in fen["pieces in hand"].ToCharArray())
      {
        if (c >= '0' && c <= '9')
          squares += (int)(c - '0');
        else if (c != '-' && c != '@')
        {
          PieceType type = Game.GetTypeByNotation(c.ToString());
          int player = squares / handSize;
          int file = -(squares % handSize + 1);
          squares++;
          Location loc = new Location(player, file); // TODO(chesslogic): order of operations?
          if (type != null)
          {
            Piece piece = new Piece(Game, player, type, loc);
            Board.Game.AddPiece(piece);
          }
        }
      }
    }

    public override MoveEventResponse MoveBeingUnmade(MoveInfo info, int ply)
    {
      Player movingPlayer = Game.CurrentPlayer;
      if (movingPlayer != null)
      {
        movingPlayer = info.Player == movingPlayer.Side ? movingPlayer : movingPlayer.Opponent;
        if (!pocketSquares.Contains(info.FromSquare))
          return MoveEventResponse.NotHandled;
        if (movingPlayer.Side != info.Player)
          movingPlayer = movingPlayer.Opponent;
        movingPlayer.GemsSpent -= info.PieceMoved.MidgameValue / 100;
        return MoveEventResponse.Handled;
      }
      // TODO(chesslogic): try NotHandled
      return MoveEventResponse.Handled;
    }

    public override MoveEventResponse MoveBeingMade(MoveInfo info, int something)
    {
      Player movingPlayer = Game.CurrentPlayer;
      if (movingPlayer != null)
      {
        movingPlayer = info.Player == movingPlayer.Side ? movingPlayer : movingPlayer.Opponent;
        if (!pocketSquares.Contains(info.FromSquare))
          return MoveEventResponse.NotHandled;
        if (movingPlayer.Side != info.Player)
          movingPlayer = movingPlayer.Opponent;
        if (movingPlayer.Gems < info.PieceMoved.MidgameValue / 100)
          return MoveEventResponse.IllegalMove;
        movingPlayer.GemsSpent += info.PieceMoved.MidgameValue / 100;
        return MoveEventResponse.Handled;
      }
      return MoveEventResponse.Handled;
    }

    public override void GenerateSpecialMoves(MoveList list, bool capturesOnly, int ply)
    {
      if (!capturesOnly)
      {
        for (int card = 0; card < handSize; card++)
        {
          int pocketSquare = pocketSquares[Game.CurrentSide * handSize + card];
          Piece pieceInPocket = Board[pocketSquare];
          if (pieceInPocket != null)
          {
            int gems;
            if (Game.Match != null)
            {
              Player movingPlayer = Game.Match.GetPlayer(list.CurrentMove.Player);
              //if (Game.CurrentPlayer.Side != list.CurrentMove.Player)
              //{
              //	movingPlayer = movingPlayer.Opponent;
              //}
              gems = movingPlayer.Gems;
            }
            else
            {
              gems = list.CurrentMove.Player + ApmwCore.getInstance().foundPocketGems;
            }
            if (gems < pieceInPocket.MidgameValue / 100)
            {
              continue;
            }

            for (int square = 0; square < Board.NumSquares; square++)
            {
              var rankFromPlayer = Game.CurrentSide == 0 ? Board.GetRank(square) : 7 - Board.GetRank(square);
              var inRange = rankFromPlayer <= Range;
              if (inRange && Board[square] == null)
              {
                list.BeginMoveAdd(MoveType.Drop, pocketSquare, square);
                Piece piece = list.AddPickup(pocketSquare);
                list.AddDrop(piece, square, pieceInPocket.PieceType);
                list.EndMoveAdd(piece.PieceType.GetMidgamePST(square) - 10);
              }
            }
          }
        }
      }
    }

    public override MoveEventResponse DescribeMove(MoveInfo move, MoveNotation format, ref string description)
    {
      if (move.FromSquare < Board.NumSquares)
        return MoveEventResponse.NotHandled;

      description = String.Join(" ",
        Board.GetDefaultSquareNotation(move.FromSquare),
        Board.GetDefaultSquareNotation(move.ToSquare),
        move.PieceMoved.PieceType.Notation[move.Player]);
      return MoveEventResponse.Handled;
    }
  }
}
