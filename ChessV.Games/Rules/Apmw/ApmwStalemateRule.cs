using System.Collections.Generic;
using System.Linq;

namespace ChessV.Games.Rules.Apmw
{
  public class ApmwStalemateRule : CheckmateRule
  {
    private HashSet<Piece>[] startingRoyalPieces;

    public ApmwStalemateRule(PieceType[] royalPieceTypes) : base(royalPieceTypes)
    {
      StalemateResult = MoveEventResponse.GameDrawn;
    }

    public override void Initialize(Game game)
    {
      base.Initialize(game);
    }


    public override void PositionLoaded(FEN fen)
    {
      base.PositionLoaded(fen);
      startingRoyalPieces = new HashSet<Piece>[2];
      startingRoyalPieces[0] = new HashSet<Piece>(RoyalPieces[0]);
      startingRoyalPieces[1] = new HashSet<Piece>(RoyalPieces[1]);
    }

    public override MoveEventResponse MoveBeingMade(MoveInfo move, int ply)
    {
      if (Game == null || Game.Match == null)
        return base.IllegalCheckMoves(move);
      Player player = Game.Match.GetPlayer(move.Player);
      if (player is HumanPlayer)
        return MoveEventResponse.NotHandled;
      if (move.MoveType.HasFlag(MoveType.CaptureProperty))
        if (RoyalPieces[player.Side ^ 1].Contains(move.PieceCaptured))
            if (RoyalPieces[player.Side ^ 1].Remove(move.PieceCaptured))
              return MoveEventResponse.NotHandled;
      if (RoyalPieces[move.Player ^ 1].Count > 1 ||
          !move.MoveType.HasFlag(MoveType.CaptureProperty) || move.PieceCaptured != RoyalPieces[move.Player ^ 1].First())
        return base.IllegalCheckMoves(move);
      return MoveEventResponse.NotHandled;
    }

    public override MoveEventResponse MoveBeingUnmade(MoveInfo move, int ply)
    {
      if (Game == null || Game.Match == null)
        return base.IllegalCheckMoves(move);
      Player player = Game.Match.GetPlayer(move.Player);
      if (player is not HumanPlayer)
        if (move.MoveType.HasFlag(MoveType.CaptureProperty) && startingRoyalPieces[player.Side ^ 1].Contains(move.PieceCaptured))
          RoyalPieces[player.Side ^ 1].Add(move.PieceCaptured);
      return MoveEventResponse.NotHandled;
    }

    public override MoveEventResponse NoMovesResult(int currentPlayer, int ply)
    {
      MoveEventResponse response = base.NoMovesResult(currentPlayer, ply);
      if (response != StalemateResult)
        return response;
      Player player = Game.Match.GetPlayer(currentPlayer);
      if (player is HumanPlayer)
        return MoveEventResponse.GameLost;
      return MoveEventResponse.GameWon;
    }

    public override int PositionalSearchExtension(int currentPlayer, int ply)
    {
      if (ply < 20)
        return base.PositionalSearchExtension(currentPlayer, ply);
      return 0;
    }
  }
}
