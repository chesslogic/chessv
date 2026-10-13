using System;
using System.Collections.Generic;
using System.Linq;
using ChessV.Games.Rules;

namespace ChessV.Games
{
  public sealed class KingDestinationHint
  {
    public int Square { get; }
    public bool FriendlyOccupied { get; }
    public bool IsLegalMove { get; }
    public bool EngineAttacked { get; }
    public bool IsCastling { get; }
    public bool IsComplete { get; }
    public IReadOnlyList<CaptureThreat> Threats { get; }

    public bool IsSafe => !FriendlyOccupied && IsComplete && !EngineAttacked && Threats.Count == 0;

    internal KingDestinationHint(int square, bool friendlyOccupied, bool legal,
      bool attacked, IEnumerable<CaptureThreat> threats, bool castling = false)
    {
      Square = square;
      FriendlyOccupied = friendlyOccupied;
      IsLegalMove = legal;
      EngineAttacked = attacked;
      IsCastling = castling;
      Threats = Array.AsReadOnly(threats.ToArray());
      IsComplete = (!attacked || Threats.Count > 0) && Threats.All(t => t.Path != null);
    }
  }

  public sealed class KingThreatSnapshot
  {
    public Piece King { get; }
    public int KingSquare { get; }
    public IReadOnlyList<KingDestinationHint> Destinations { get; }
    public KingDestinationHint CurrentSquare => Destinations.First(h => h.Square == KingSquare && !h.IsCastling);

    internal KingThreatSnapshot(Piece king, IEnumerable<KingDestinationHint> destinations)
    {
      King = king;
      KingSquare = king.Square;
      Destinations = Array.AsReadOnly(destinations.ToArray());
    }
  }

  public static class KingThreatAnalysis
  {
    public static IReadOnlyList<Piece> GetKings(Game game, int player)
    {
      var rule = game.FindRule(typeof(CheckmateRule), true) as CheckmateRule;
      if (rule == null)
        return Array.Empty<Piece>();
      return game.GetPieceList(player).Where(p => p.Square >= 0 && p.Square < game.Board.NumSquares &&
        rule.RoyalPieceTypes.Contains(p.PieceType)).OrderBy(p => p.Square).ToArray();
    }

    public static KingThreatSnapshot Analyze(Game game, Piece king, bool includeCastling = true)
    {
      if (game == null)
        throw new ArgumentNullException(nameof(game));
      if (king == null || king.Game != game || king.Square < 0 || king.Square >= game.Board.NumSquares)
        throw new ArgumentException("Analysis requires a king on this game's board.", nameof(king));

      var legalSquares = new HashSet<int>();
      var squares = new HashSet<int> { king.Square };
      int legalCount = game.GetRootMoves(out MoveInfo[] legalMoves, king);
      foreach (MoveInfo move in legalMoves.Take(legalCount))
      {
        legalSquares.Add(move.ToSquare);
        if (move.MoveType != MoveType.Castling)
          squares.Add(move.ToSquare);
      }
      int count = king.PieceType.GetMoveCapabilities(out MoveCapability[] capabilities);
      for (int index = 0; index < count; index++)
      {
        MoveCapability move = capabilities[index];
        if (move.ConditionalBySquare != null && !move.ConditionalBySquare[king.Player][king.Square])
          continue;
        int square = game.Board.NextSquare(king.Player, move.NDirection, king.Square);
        for (int step = 1; square >= 0 && square < game.Board.NumSquares && step <= move.MaxSteps; step++)
        {
          if (step >= move.MinSteps)
            squares.Add(square);
          if (game.Board[square] != null)
            break;
          square = game.Board.NextSquare(king.Player, move.NDirection, square);
        }
      }
      var hints = new List<KingDestinationHint>();
      var captures = new MoveList(game.Board);
      foreach (int square in squares.OrderBy(s => s))
      {
        bool blocked = square != king.Square && game.Board[square]?.Player == king.Player;
        if (blocked)
          hints.Add(new KingDestinationHint(square, true, false, false, Array.Empty<CaptureThreat>()));
        else
          hints.Add(game.Board.InspectRelocation(king, square,
            () => InspectSquare(game, king, square, legalSquares.Contains(square), captures)));
      }
      if (includeCastling)
        foreach (CastlingRule rule in game.GetRules().OfType<CastlingRule>())
          foreach (CastlingRule.Restriction restriction in rule.GetRestrictions(king))
          {
            if (!restriction.Attacked)
              hints.Add(new KingDestinationHint(restriction.Square, restriction.Occupied,
                false, false, Array.Empty<CaptureThreat>(), true));
            else
            {
              var replacement = new Piece(game, king.Player, king.PieceType, -1);
              hints.Add(game.Board.InspectReplacement(replacement, restriction.Square,
                () => InspectSquare(game, king, restriction.Square, false, captures,
                  true, restriction.Occupied, restriction.Attacked)));
            }
          }
      return new KingThreatSnapshot(king, hints);
    }

    private static KingDestinationHint InspectSquare(Game game, Piece king, int square,
      bool legal, MoveList captures, bool castling = false, bool occupied = false, bool? attacked = null)
    {
      captures.Reset();
      foreach (Piece attacker in game.GetPieceList(king.Player ^ 1))
        if (attacker.Square >= 0 && attacker.Square < game.Board.NumSquares)
          attacker.GenerateMoves(captures, true);
      var threats = captures.GetCapturesOf(square)
        .GroupBy(t => t.SourceSquare + ":" + (t.Path == null ? "unknown" : string.Join(",", t.Path.Squares)))
        .Select(g => g.First()).ToArray();
      return new KingDestinationHint(square, occupied, legal,
        attacked ?? game.IsSquareAttacked(square, king.Player ^ 1), threats, castling);
    }
  }
}
