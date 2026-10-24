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
      IReadOnlyDictionary<int, double> mobilityByDensityPercent,
      int files = 8,
      int ranks = 8,
      EmptyBoardTravelStatistics emptyBoardTravel = null,
      IReadOnlyList<string> analysisLimitations = null)
    {
      SourcePiece = sourcePiece;
      AverageDirectionsAttacked = averageDirectionsAttacked;
      AverageSafeChecks = averageSafeChecks;
      MobilityByDensityPercent = mobilityByDensityPercent;
      Files = files;
      Ranks = ranks;
      EmptyBoardTravel = emptyBoardTravel;
      AnalysisLimitations = analysisLimitations ?? new List<string>();
    }

    public ConstructedPiece SourcePiece { get; private set; }
    public double AverageDirectionsAttacked { get; private set; }
    public double AverageSafeChecks { get; private set; }
    public IReadOnlyDictionary<int, double> MobilityByDensityPercent { get; private set; }
    public int Files { get; private set; }
    public int Ranks { get; private set; }
    public EmptyBoardTravelStatistics EmptyBoardTravel { get; private set; }
    public IReadOnlyList<string> AnalysisLimitations { get; private set; }
  }

  public sealed class MobilityAnalyzer
  {
    private sealed class AnalysisGame : UndefinedGame
    {
      private readonly PieceType analyzedPiece;

      public AnalysisGame(int files, int ranks, PieceType analyzedPiece) : base(files, ranks)
      {
        this.analyzedPiece = analyzedPiece;
      }

      public override void AddPieceTypes()
      {
        King = analyzedPiece.Notation[0] == "K" ? analyzedPiece : new ChessV.Games.King("King", "K", 0, 0);
        Pawn = analyzedPiece.Notation[0] == "P" ? analyzedPiece : new ChessV.Games.Pawn("Pawn", "P", 100, 125);
        CastlingType = King;
        AddPieceType(King);
        AddPieceType(Pawn);
        if (analyzedPiece != King && analyzedPiece != Pawn)
          AddPieceType(analyzedPiece);
      }
    }

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
      UndefinedGame game = new AnalysisGame(files, ranks, constructedPiece.Piece);
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
        mobilityByDensity,
        files,
        ranks,
        EmptyBoardTravelAnalyzer.Analyze(constructedPiece.Piece, game),
        FindAnalysisLimitations(constructedPiece.Piece));
    }

    private static IReadOnlyList<string> FindAnalysisLimitations(PieceType piece)
    {
      var limitations = new List<string>();
      MoveCapability[] moves;
      int count = piece.GetMoveCapabilities(out moves);
      if (piece.CustomMoveGenerator != null)
        limitations.Add("Directional mobility does not include custom move generation.");
      if (moves.Take(count).Any(move => move.SpecialAttacks != 0))
        limitations.Add("Directional mobility does not model special capture rules or required screens.");
      if (moves.Take(count).Any(move => move.PathInfo != null))
        limitations.Add("Directional mobility does not check movement paths.");
      if (moves.Take(count).Any(move => !move.CanCapture))
        limitations.Add("Directional mobility excludes move-only capabilities.");
      return limitations;
    }
  }
}
