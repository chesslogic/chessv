using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessV.PieceAnalysis
{
  public sealed class PieceWarning
  {
    public PieceWarning(string type, string severity, string message)
    {
      Type = type;
      Severity = severity;
      Message = message;
    }

    public string Type { get; private set; }
    public string Severity { get; private set; }
    public string Message { get; private set; }
  }

  public sealed class PercentileBreakdown
  {
    public double? AverageMobilityAtDensity10 { get; set; }
    public double? AverageDirectionsAttacked { get; set; }
    public double? AverageSafeChecks { get; set; }
  }

  public sealed class PiecePercentiles
  {
    public PercentileBreakdown VsTierPeers { get; set; }
    public PercentileBreakdown VsAllPieces { get; set; }
  }

  public sealed class AssessedPiece
  {
    public AssessedPiece(MobilityStatistics statistics)
    {
      Statistics = statistics;
      Percentiles = new PiecePercentiles();
      Warnings = new List<PieceWarning>();
    }

    public MobilityStatistics Statistics { get; private set; }
    public PiecePercentiles Percentiles { get; set; }
    public List<PieceWarning> Warnings { get; private set; }
  }

  public sealed class MaterialAssessment
  {
    private sealed class MobilityThreshold
    {
      public MobilityThreshold(double minDensity10, double maxDensity10, double? minSafeChecks = null, double? maxSafeChecks = null)
      {
        MinDensity10 = minDensity10;
        MaxDensity10 = maxDensity10;
        MinSafeChecks = minSafeChecks;
        MaxSafeChecks = maxSafeChecks;
      }

      public double MinDensity10 { get; private set; }
      public double MaxDensity10 { get; private set; }
      public double? MinSafeChecks { get; private set; }
      public double? MaxSafeChecks { get; private set; }
    }

    private sealed class OutlierObservation
    {
      public OutlierObservation(int densityPercent, string direction, double value, double lowFence, double highFence)
      {
        DensityPercent = densityPercent;
        Direction = direction;
        Value = value;
        LowFence = lowFence;
        HighFence = highFence;
      }

      public int DensityPercent { get; private set; }
      public string Direction { get; private set; }
      public double Value { get; private set; }
      public double LowFence { get; private set; }
      public double HighFence { get; private set; }
    }

    // Seeded from the current 8x8 APMW roster's observed density-10 mobility values.
    private static readonly IReadOnlyDictionary<PieceTier, MobilityThreshold> mobilityThresholds =
      new Dictionary<PieceTier, MobilityThreshold>
      {
        { PieceTier.Minor, new MobilityThreshold(2.0, 9.5, minSafeChecks: 2.55) },
        { PieceTier.Major, new MobilityThreshold(8.0, 13.0) },
        { PieceTier.Jack, new MobilityThreshold(8.5, 16.5) },
        { PieceTier.Queen, new MobilityThreshold(7.0, 22.0) },
        { PieceTier.Amazon, new MobilityThreshold(13.5, 30.0) },
        { PieceTier.Pawn, new MobilityThreshold(0.5, 2.5) },
        { PieceTier.Weak, new MobilityThreshold(0.5, 2.0) }
      };

    public IReadOnlyList<AssessedPiece> Assess(
      IReadOnlyList<MobilityStatistics> assessedStatistics,
      IReadOnlyList<MobilityStatistics> referenceStatistics)
    {
      List<AssessedPiece> assessedPieces = assessedStatistics.Select(statistics => new AssessedPiece(statistics)).ToList();
      List<MobilityStatistics> allReferencePieces = referenceStatistics.ToList();
      Dictionary<PieceTier, List<MobilityStatistics>> referenceByTier = allReferencePieces
        .Where(statistics => statistics.SourcePiece.Tier.HasValue)
        .GroupBy(statistics => statistics.SourcePiece.Tier.Value)
        .ToDictionary(group => group.Key, group => group.OrderBy(stat => stat.SourcePiece.RequestedName, StringComparer.Ordinal).ToList());

      foreach (AssessedPiece assessedPiece in assessedPieces)
      {
        MobilityStatistics statistics = assessedPiece.Statistics;
        List<MobilityStatistics> tierReferencePieces = new List<MobilityStatistics>();
        if (statistics.SourcePiece.Tier.HasValue)
          referenceByTier.TryGetValue(statistics.SourcePiece.Tier.Value, out tierReferencePieces);

        assessedPiece.Percentiles = new PiecePercentiles
        {
          VsTierPeers = CalculatePercentiles(statistics, tierReferencePieces),
          VsAllPieces = CalculatePercentiles(statistics, allReferencePieces)
        };

        AddTierOutlierWarnings(assessedPiece, tierReferencePieces);
        AddAbsoluteThresholdWarning(assessedPiece);
        AddValueTierMismatchWarning(assessedPiece);
      }

      return assessedPieces;
    }

    private static void AddTierOutlierWarnings(AssessedPiece assessedPiece, List<MobilityStatistics> tierReferencePieces)
    {
      if (tierReferencePieces == null)
        return;

      List<MobilityStatistics> peers = tierReferencePieces
        .Where(peer => !string.Equals(peer.SourcePiece.RequestedName, assessedPiece.Statistics.SourcePiece.RequestedName, StringComparison.OrdinalIgnoreCase))
        .ToList();

      if (peers.Count < 4)
        return;

      List<OutlierObservation> mobilityOutliers = new List<OutlierObservation>();
      foreach (KeyValuePair<int, double> entry in assessedPiece.Statistics.MobilityByDensityPercent.OrderBy(item => item.Key))
      {
        OutlierObservation observation = GetIqrOutlier(
          peers.Select(peer => peer.MobilityByDensityPercent[entry.Key]).ToList(),
          entry.Value,
          entry.Key);
        if (observation != null)
          mobilityOutliers.Add(observation);
      }

      if (mobilityOutliers.Count > 0)
      {
        OutlierObservation example = mobilityOutliers[0];
        assessedPiece.Warnings.Add(new PieceWarning(
          "statistical_outlier",
          "warning",
          string.Format(
            "AverageMobility is {0} the same-tier IQR fence at densities [{1}]. Example: density {2} value {3:F2}, expected within [{4:F2}, {5:F2}] from {6} peer samples.",
            example.Direction,
            string.Join(", ", mobilityOutliers.Select(item => item.DensityPercent.ToString()).OrderBy(text => text, StringComparer.Ordinal)),
            example.DensityPercent,
            example.Value,
            example.LowFence,
            example.HighFence,
            peers.Count)));
      }

      MaybeAddIqrWarning(
        assessedPiece.Warnings,
        peers.Select(peer => peer.AverageDirectionsAttacked).ToList(),
        assessedPiece.Statistics.AverageDirectionsAttacked,
        "averageDirectionsAttacked",
        "AverageDirectionsAttacked");

      MaybeAddIqrWarning(
        assessedPiece.Warnings,
        peers.Select(peer => peer.AverageSafeChecks).ToList(),
        assessedPiece.Statistics.AverageSafeChecks,
        "averageSafeChecks",
        "AverageSafeChecks");
    }

    private static void AddAbsoluteThresholdWarning(AssessedPiece assessedPiece)
    {
      PieceTier? tier = assessedPiece.Statistics.SourcePiece.Tier;
      if (!tier.HasValue || !mobilityThresholds.ContainsKey(tier.Value))
        return;

      MobilityThreshold threshold = mobilityThresholds[tier.Value];
      double mobilityAtDensity10 = assessedPiece.Statistics.MobilityByDensityPercent[10];
      if (mobilityAtDensity10 < threshold.MinDensity10 || mobilityAtDensity10 > threshold.MaxDensity10)
      {
        assessedPiece.Warnings.Add(new PieceWarning(
          "absolute_threshold",
          "warning",
          string.Format(
            "{0} density-10 mobility {1:F2} is outside the configured {2} tier range [{3:F2}, {4:F2}].",
            assessedPiece.Statistics.SourcePiece.RequestedName,
            mobilityAtDensity10,
            tier.Value,
            threshold.MinDensity10,
            threshold.MaxDensity10)));
      }

      double averageSafeChecks = assessedPiece.Statistics.AverageSafeChecks;
      if ((threshold.MinSafeChecks.HasValue && averageSafeChecks < threshold.MinSafeChecks.Value) ||
        (threshold.MaxSafeChecks.HasValue && averageSafeChecks > threshold.MaxSafeChecks.Value))
      {
        assessedPiece.Warnings.Add(new PieceWarning(
          "absolute_threshold",
          "warning",
          string.Format(
            "{0} averageSafeChecks {1:F2} is outside the configured {2} tier range [{3}, {4}].",
            assessedPiece.Statistics.SourcePiece.RequestedName,
            averageSafeChecks,
            tier.Value,
            threshold.MinSafeChecks.HasValue ? threshold.MinSafeChecks.Value.ToString("F2") : "-inf",
            threshold.MaxSafeChecks.HasValue ? threshold.MaxSafeChecks.Value.ToString("F2") : "+inf")));
      }
    }

    private static void AddValueTierMismatchWarning(AssessedPiece assessedPiece)
    {
      PieceTier? tier = assessedPiece.Statistics.SourcePiece.Tier;
      if (!tier.HasValue)
        return;

      double averageValue = (assessedPiece.Statistics.SourcePiece.MidgameValue + assessedPiece.Statistics.SourcePiece.EndgameValue) / 2.0;
      double assignedDifference = Math.Abs(averageValue - TierReferenceData.TierMaterialValues[tier.Value]);
      KeyValuePair<PieceTier, int> closestTier = TierReferenceData.TierMaterialValues
        .OrderBy(candidate => Math.Abs(averageValue - candidate.Value))
        .ThenBy(candidate => candidate.Value)
        .First();
      double closestDifference = Math.Abs(averageValue - closestTier.Value);

      if (closestTier.Key != tier.Value && assignedDifference - closestDifference >= 35.0)
      {
        assessedPiece.Warnings.Add(new PieceWarning(
          "value_tier_mismatch",
          "info",
          string.Format(
            "{0} averages {1:F1} material, which is closer to the {2} baseline ({3}) than its assigned {4} baseline ({5}).",
            assessedPiece.Statistics.SourcePiece.RequestedName,
            averageValue,
            closestTier.Key,
            closestTier.Value,
            tier.Value,
            TierReferenceData.TierMaterialValues[tier.Value])));
      }
    }

    private static PercentileBreakdown CalculatePercentiles(MobilityStatistics target, IReadOnlyList<MobilityStatistics> cohort)
    {
      if (cohort == null || cohort.Count == 0)
        return new PercentileBreakdown();

      return new PercentileBreakdown
      {
        AverageMobilityAtDensity10 = CalculatePercentile(cohort.Select(piece => piece.MobilityByDensityPercent[10]).ToList(), target.MobilityByDensityPercent[10]),
        AverageDirectionsAttacked = CalculatePercentile(cohort.Select(piece => piece.AverageDirectionsAttacked).ToList(), target.AverageDirectionsAttacked),
        AverageSafeChecks = CalculatePercentile(cohort.Select(piece => piece.AverageSafeChecks).ToList(), target.AverageSafeChecks)
      };
    }

    private static double CalculatePercentile(IReadOnlyList<double> values, double target)
    {
      double lessThan = values.Count(value => value < target);
      double equalTo = values.Count(value => Math.Abs(value - target) < 0.0000001);
      return ((lessThan + (0.5 * equalTo)) / values.Count) * 100.0;
    }

    private static void MaybeAddIqrWarning(
      IList<PieceWarning> warnings,
      IList<double> peerValues,
      double candidateValue,
      string metricKey,
      string metricDisplayName)
    {
      OutlierObservation observation = GetIqrOutlier(peerValues, candidateValue, 0);
      if (observation == null)
        return;

      warnings.Add(new PieceWarning(
        "statistical_outlier",
        "warning",
        string.Format(
          "{0} is {1} the same-tier IQR fence for {2}: value {3:F2}, expected within [{4:F2}, {5:F2}] from {6} peer samples.",
          metricKey,
          observation.Direction,
          metricDisplayName,
          observation.Value,
          observation.LowFence,
          observation.HighFence,
          peerValues.Count)));
    }

    private static OutlierObservation GetIqrOutlier(IList<double> peerValues, double candidateValue, int densityPercent)
    {
      if (peerValues.Count < 4)
        return null;

      List<double> sortedValues = peerValues.OrderBy(value => value).ToList();
      double q1 = GetPercentile(sortedValues, 25.0);
      double q3 = GetPercentile(sortedValues, 75.0);
      double iqr = q3 - q1;
      if (iqr < 0.0000001)
        return null;

      double lowFence = q1 - (1.5 * iqr);
      double highFence = q3 + (1.5 * iqr);
      if (candidateValue >= lowFence && candidateValue <= highFence)
        return null;

      return new OutlierObservation(
        densityPercent,
        candidateValue < lowFence ? "below" : "above",
        candidateValue,
        lowFence,
        highFence);
    }

    private static double GetPercentile(IReadOnlyList<double> sortedValues, double percentile)
    {
      if (sortedValues.Count == 1)
        return sortedValues[0];

      double rank = (percentile / 100.0) * (sortedValues.Count - 1);
      int lowerIndex = (int)Math.Floor(rank);
      int upperIndex = (int)Math.Ceiling(rank);
      if (lowerIndex == upperIndex)
        return sortedValues[lowerIndex];

      double weight = rank - lowerIndex;
      return sortedValues[lowerIndex] + ((sortedValues[upperIndex] - sortedValues[lowerIndex]) * weight);
    }
  }
}
