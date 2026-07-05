using ChessV;
using ChessV.Games.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessV.PieceAnalysis
{
  public sealed class MobilityStatistics
  {
    public MobilityStatistics(
      ConstructedPiece sourcePiece,
      double averageDirectionsAttacked,
      double averageSafeChecks,
      IReadOnlyDictionary<int, double> mobilityByDensityPercent)
    {
      SourcePiece = sourcePiece;
      AverageDirectionsAttacked = averageDirectionsAttacked;
      AverageSafeChecks = averageSafeChecks;
      MobilityByDensityPercent = mobilityByDensityPercent;
    }

    public ConstructedPiece SourcePiece { get; private set; }
    public double AverageDirectionsAttacked { get; private set; }
    public double AverageSafeChecks { get; private set; }
    public IReadOnlyDictionary<int, double> MobilityByDensityPercent { get; private set; }
  }

  public sealed class MobilityAnalyzer
  {
    public static readonly int[] DefaultDensityPercents = new[] { 50, 45, 40, 35, 30, 25, 20, 15, 10 };

    public IReadOnlyList<MobilityStatistics> Analyze(
      IEnumerable<ConstructedPiece> constructedPieces,
      int files,
      int ranks,
      IReadOnlyList<int> densityPercents)
    {
      List<ConstructedPiece> pieceList = constructedPieces.ToList();
      if (pieceList.Count == 0)
        return new List<MobilityStatistics>();

      List<MobilityStatistics> results = new List<MobilityStatistics>();
      foreach (ConstructedPiece constructedPiece in pieceList)
        results.Add(Analyze(constructedPiece, files, ranks, densityPercents));

      return results;
    }

    public MobilityStatistics Analyze(
      ConstructedPiece constructedPiece,
      int files,
      int ranks,
      IReadOnlyList<int> densityPercents)
    {
      GameAttribute attr = new GameAttribute(string.Empty, typeof(Geometry.Rectangular), files, ranks);
      UndefinedGame.PieceTypeList = new List<PieceType> { constructedPiece.Piece };
      UndefinedGame game = new UndefinedGame(files, ranks);
      game.Initialize(attr, null, null);

      Dictionary<int, double> mobilityByDensity = new Dictionary<int, double>();
      double averageDirectionsAttacked = 0.0;
      double averageSafeChecks = 0.0;

      foreach (int densityPercent in densityPercents)
      {
        double density = 1.0 - (densityPercent / 100.0);
        constructedPiece.Piece.CalculateMobilityStatistics(game, density);
        mobilityByDensity[densityPercent] = constructedPiece.Piece.AverageMobility;
        if (densityPercent == densityPercents[0])
        {
          averageDirectionsAttacked = constructedPiece.Piece.AverageDirectionsAttacked;
          averageSafeChecks = constructedPiece.Piece.AverageSafeChecks;
        }
      }

      return new MobilityStatistics(
        constructedPiece,
        averageDirectionsAttacked,
        averageSafeChecks,
        mobilityByDensity);
    }
  }
}
