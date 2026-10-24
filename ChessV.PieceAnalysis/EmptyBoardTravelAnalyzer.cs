using ChessV;
using System.Collections.Generic;
using System.Linq;

namespace ChessV.PieceAnalysis
{
  public sealed class EmptyBoardTravelStatistics
  {
    public string Model { get { return "empty_board_noncapturing_player_0"; } }
    public string UnsupportedReason { get; set; }
    public double? ReachableSquareFraction { get; set; }
    public double? AverageMovesToReachableSquare { get; set; }
    public double? HomeToFarRankReachableFraction { get; set; }
    public double? AverageMovesHomeToFarRank { get; set; }
  }

  internal static class EmptyBoardTravelAnalyzer
  {
    public static EmptyBoardTravelStatistics Analyze(PieceType piece, Game game)
    {
      var result = new EmptyBoardTravelStatistics();
      MoveCapability[] moves;
      int count = piece.GetMoveCapabilities(out moves);
      if (piece.CustomMoveGenerator != null ||
          moves.Take(count).Any(move => move.SpecialAttacks != 0 || move.PathInfo != null))
      {
        result.UnsupportedReason = "Custom moves, special attacks, or movement paths require a separate movement model.";
        return result;
      }

      Board board = game.Board;
      var destinations = new List<int>[board.NumSquares];
      for (int square = 0; square < board.NumSquares; square++)
      {
        var reachable = new HashSet<int>();
        for (int index = 0; index < count; index++)
        {
          MoveCapability move = moves[index];
          if (move.MustCapture ||
              (move.ConditionalBySquare != null && !move.ConditionalBySquare[0][square]))
            continue;

          int target = board.NextSquare(move.NDirection, square);
          for (int steps = 1; target >= 0 && steps <= move.MaxSteps; steps++)
          {
            if (steps >= move.MinSteps)
              reachable.Add(target);
            target = board.NextSquare(move.NDirection, target);
          }
        }
        destinations[square] = reachable.ToList();
      }

      long reachablePairs = 0;
      long totalMoves = 0;
      long farRankPairs = 0;
      long farRankMoves = 0;
      var queue = new int[board.NumSquares];
      for (int source = 0; source < board.NumSquares; source++)
      {
        int[] distances = Enumerable.Repeat(-1, board.NumSquares).ToArray();
        distances[source] = 0;
        int head = 0;
        int tail = 0;
        queue[tail++] = source;
        while (head < tail)
        {
          int current = queue[head++];
          foreach (int target in destinations[current])
          {
            if (distances[target] >= 0)
              continue;
            distances[target] = distances[current] + 1;
            queue[tail++] = target;
          }
        }

        for (int target = 0; target < board.NumSquares; target++)
        {
          if (target == source || distances[target] < 0)
            continue;
          reachablePairs++;
          totalMoves += distances[target];
          if (board.GetRank(source) == 0 && board.GetRank(target) == board.NumRanks - 1)
          {
            farRankPairs++;
            farRankMoves += distances[target];
          }
        }
      }

      result.ReachableSquareFraction = board.NumSquares > 1
        ? (double)reachablePairs / ((long)board.NumSquares * (board.NumSquares - 1))
        : 0.0;
      result.AverageMovesToReachableSquare = reachablePairs > 0 ? (double)totalMoves / reachablePairs : (double?)null;
      result.HomeToFarRankReachableFraction = (double)farRankPairs / (board.NumFiles * board.NumFiles);
      result.AverageMovesHomeToFarRank = farRankPairs > 0 ? (double)farRankMoves / farRankPairs : (double?)null;
      return result;
    }
  }
}
