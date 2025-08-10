using System.Collections.Generic;

namespace ChessV.Games.Rules.Apmw
{
  public class MixedEnPassantRule : EnPassantRule
  {
    public MixedEnPassantRule(EnPassantRule templateRule) : base(templateRule)
    {
      // TODO(chesslogic): maybe this doesn't work
    }

    #region Initialize
    public override void Initialize(Game game)
    {
      base.Initialize(game);
      hashKeyIndex = game.HashKeys.TakeKeys(game.Board.NumSquaresExtended);
      epSquares = new int[Game.MAX_PLY];
      for (int x = 0; x < Game.MAX_PLY; x++)
        epSquares[x] = 0;
      gameHistory = new int[Game.MAX_GAME_LENGTH];
      gameHistory[0] = 0;
      List<int> attackdirs = new List<int>();
      MoveCapability[] moveCapabilities;
      foreach (PieceType pawnType in ((IMultipawnGame)Game).Pawns)
      {
        int nMoveCapabilities = pawnType.GetMoveCapabilities(out moveCapabilities);
        for (int nMoveCap = 0; nMoveCap < nMoveCapabilities; nMoveCap++)
        {
          MoveCapability move = moveCapabilities[nMoveCap];
          if (move.CanCapture)
            attackdirs.Add(move.NDirection);
        }
      }
      nAttackDirections = attackdirs.Count;
      attackDirections = new int[game.NumPlayers, nAttackDirections];
      for (int ndir = 0; ndir < nAttackDirections; ndir++)
        for (int player = 0; player < game.NumPlayers; player++)
          attackDirections[player, ndir] = game.PlayerDirection(player, attackdirs[ndir]);

      //	Hook up MoveBeingPlayed event handler
      game.MoveBeingPlayed += MoveBeingPlayedHandler;
    }
    #endregion

    #region MoveBeingMade
    public override MoveEventResponse MoveBeingMade(MoveInfo move, int ply)
    {
      epSquares[ply] = 0;
      if (ply == 1)
        gameHistory[Game.GameMoveNumber] = 0;
      //	if the current side to move is also the next side to move then 
      //	we return here - en passant is not possible so we would not 
      //	want to set the ep square
      if (Game.CurrentSide == Game.NextSide)
        return MoveEventResponse.NotHandled;
      // Exclude ExtraCapture moves (like Checkers multi-jumps) from triggering en passant
      if (move.MoveType == MoveType.ExtraCapture || (move.MoveType & MoveType.ExtraCapture) == MoveType.ExtraCapture)
        return MoveEventResponse.NotHandled;
      if (move.PieceMoved != null && ((IMultipawnGame)Game).Pawns.Contains(move.PieceMoved.PieceType))
        if (move.FromSquare >= Board.NumSquares) // pocket pawns would check outside of board
          return MoveEventResponse.NotHandled;
        else if (Board.DirectionLookup(move.FromSquare, move.ToSquare) == Game.PlayerDirection(move.Player, NDirection) &&
          Board.GetDistance(move.FromSquare, move.ToSquare) > 1)
        {
          //	check to see if there are any pawn attackers - even if a pawn makes a 
          //	multi-step move, we don't set the e.p. square unless there is a pawn 
          //	that can actually take it.  this is an important consideration because of 
          //	the hashing/matching of board positions.  we don't want an identical board 
          //	position to be considered different just because a pawn made a multi-step 
          //	move - the fact that a multi-step move was made is only significant because 
          //	of the availability of an e.p. capture
          int epsquare = Board.NextSquare(Game.PlayerDirection(move.Player, NDirection), move.FromSquare);

          //	we loop here to accomodate large-board games where a pawn can make a move 
          //	of more than two steps and still be captured e.p. on any square passed over
          while (epsquare >= 0 && epsquare < Board.NumSquares && Board[epsquare] == null)
          {
            for (int ndir = 0; ndir < nAttackDirections; ndir++)
            {
              int nextSquare = Board.NextSquare(attackDirections[move.Player, ndir], epsquare);
              if (nextSquare >= 0)
              {
                Piece piece = Board[nextSquare];
                if (piece != null && ((IMultipawnGame)Game).Pawns.Contains(piece.PieceType) && piece.Player != move.Player)
                {
                  //	en passant capture is possible
                  epSquares[ply] = epsquare;
                  if (ply == 1)
                    gameHistory[Game.GameMoveNumber] = epsquare;
                  return MoveEventResponse.MoveOk;
                }
              }
            }
            int nextEp = Board.NextSquare(Game.PlayerDirection(move.Player, NDirection), epsquare);
            if (nextEp < 0)
              break;
            epsquare = nextEp;
          }
        }
      return MoveEventResponse.NotHandled;
    }
    #endregion

    #region GenerateSpecialMoves
    public override void GenerateSpecialMoves(MoveList list, bool capturesOnly, int ply)
    {
      int epSquare = ply == 1
        ? (Game.GameMoveNumber == 0 ? epSquares[0] : gameHistory[Game.GameMoveNumber - 1])
        : epSquares[ply - 1];
      if (epSquare > 0)
      {
        int nd = Game.PlayerDirection(Game.CurrentSide ^ 1, NDirection);
        for (int ndir = 0; ndir < nAttackDirections; ndir++)
        {
          int nextSquare = Board.NextSquare(attackDirections[Game.CurrentSide ^ 1, ndir], epSquare);
          if (nextSquare >= 0)
          {
            Piece piece = Board[nextSquare];
            if (piece != null && ((IMultipawnGame)Game).Pawns.Contains(piece.PieceType) && piece.Player == Game.CurrentSide)
            {
              //	find square of pawn being captured.  we may have to make several 
              //	steps for large-board games where pawns make more than two steps 
              //	and can still be captured e.p.
              int captureSquare = Board.NextSquare(nd, epSquare);
              int steps = 1;
              // Add maximum step limit and validate capture square
              while (Board[captureSquare] == null && steps < Board.NumRanks && captureSquare >= 0)
              {
                int nextCaptureSquare = Board.NextSquare(nd, captureSquare);
                if (nextCaptureSquare < 0)
                    break;
                captureSquare = nextCaptureSquare;
                steps++;
              }

              // Verify there is actually a pawn to capture
              Piece capturedPiece = Board[captureSquare];
              if (captureSquare >= 0 && capturedPiece != null && 
                  ((IMultipawnGame)Game).Pawns.Contains(capturedPiece.PieceType) &&
                  capturedPiece.Player == (Game.CurrentSide ^ 1))
              {
                //	this piece can capture en passant 
                list.BeginMoveAdd(MoveType.EnPassant, nextSquare, epSquare);
                list.AddPickup(nextSquare);
                list.AddPickup(captureSquare);
                list.AddDrop(piece, epSquare, null);

                list.EndMoveAdd(3000);
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
      if (((IMultipawnGame)Game).Pawns.Contains(type))
        notes.Add("en passant");
    }
    #endregion
  }
}
