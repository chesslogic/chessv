using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ChessV.PieceAnalysis
{
  public sealed class BoardOutput
  {
    public int Files { get; set; }
    public int Ranks { get; set; }
  }

  public sealed class UnresolvedPieceOutput
  {
    public string Name { get; set; }
    public string Reason { get; set; }
  }

  public sealed class PieceOutput
  {
    public string Name { get; set; }
    public string Notation { get; set; }
    public string Tier { get; set; }
    public string TierSource { get; set; }
    public int? MidgameValue { get; set; }
    public int? EndgameValue { get; set; }
    public double? AverageDirectionsAttacked { get; set; }
    public double? AverageSafeChecks { get; set; }
    public IDictionary<string, double> MobilityByDensityPercent { get; set; }
    public PiecePercentiles Percentiles { get; set; }
    public IList<PieceWarning> Warnings { get; set; }
    public string ConstructionError { get; set; }
  }

  public sealed class AnalysisOutput
  {
    public BoardOutput Board { get; set; }
    public IList<int> DensityPercents { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public IList<PieceOutput> Pieces { get; set; }
    public IList<UnresolvedPieceOutput> UnresolvedPieces { get; set; }
  }

  internal sealed class Program
  {
    private sealed class Options
    {
      public Options()
      {
        PieceNames = new List<string>();
      }

      public List<string> PieceNames { get; private set; }
      public bool AllPieces { get; set; }
      public PieceTier? TierHint { get; set; }
      public int Files { get; set; }
      public int Ranks { get; set; }
      public string OutputPath { get; set; }
    }

    internal static int Main(string[] args)
    {
      try
      {
        return Run(args);
      }
      catch (ArgumentException ex)
      {
        Console.Error.WriteLine(ex.Message);
        return 1;
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine(ex.GetBaseException().Message);
        return 1;
      }
    }

    private static int Run(string[] args)
    {
      Options options = ParseArguments(args);
      PieceCatalog catalog = new PieceCatalog(TierReferenceData.Entries);
      MobilityAnalyzer analyzer = new MobilityAnalyzer();
      IReadOnlyList<int> densityPercents = MobilityAnalyzer.DefaultDensityPercents;

      List<ConstructedPiece> requestedConstructedPieces = new List<ConstructedPiece>();
      List<PieceConstructionFailure> unresolvedRequestedPieces = new List<PieceConstructionFailure>();
      HashSet<string> includedPieceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      List<string> requestedNames = GetRequestedPieceNames(options, catalog);
      foreach (string requestedName in requestedNames)
      {
        Type pieceType;
        if (!catalog.TryResolvePieceType(requestedName, out pieceType))
        {
          unresolvedRequestedPieces.Add(new PieceConstructionFailure(requestedName, null, "Piece type was not found in ChessV.Games."));
          continue;
        }

        if (!includedPieceNames.Add(pieceType.Name))
          continue;

        try
        {
          string ignoredTierHintMessage;
          ConstructedPiece constructedPiece = catalog.ConstructPiece(pieceType, options.TierHint, out ignoredTierHintMessage);
          if (!string.IsNullOrEmpty(ignoredTierHintMessage))
            Console.Error.WriteLine(ignoredTierHintMessage);
          requestedConstructedPieces.Add(constructedPiece);
        }
        catch (Exception ex)
        {
          unresolvedRequestedPieces.Add(new PieceConstructionFailure(requestedName, pieceType, ex.GetBaseException().Message));
        }
      }

      List<PieceOutput> failedAllPieceOutputs = new List<PieceOutput>();
      if (options.AllPieces)
      {
        requestedConstructedPieces.Clear();
        includedPieceNames.Clear();
        unresolvedRequestedPieces.Clear();

        foreach (Type pieceType in catalog.GetAllConstructiblePieceTypes())
        {
          if (!includedPieceNames.Add(pieceType.Name))
            continue;

          try
          {
            string ignoredTierHintMessage;
            ConstructedPiece constructedPiece = catalog.ConstructPiece(pieceType, null, out ignoredTierHintMessage);
            requestedConstructedPieces.Add(constructedPiece);
          }
          catch (Exception ex)
          {
            TierReferenceEntry referenceEntry;
            string tier = TierReferenceData.TryGetEntry(pieceType.Name, out referenceEntry) ? referenceEntry.Tier.ToString() : null;
            failedAllPieceOutputs.Add(new PieceOutput
            {
              Name = pieceType.Name,
              Notation = referenceEntry != null ? referenceEntry.Notation : null,
              Tier = tier,
              TierSource = referenceEntry != null ? referenceEntry.TierSource : null,
              MidgameValue = referenceEntry != null ? referenceEntry.MidgameValue : (int?)null,
              EndgameValue = referenceEntry != null ? referenceEntry.EndgameValue : (int?)null,
              AverageDirectionsAttacked = null,
              AverageSafeChecks = null,
              MobilityByDensityPercent = null,
              Percentiles = null,
              Warnings = new List<PieceWarning>(),
              ConstructionError = ex.GetBaseException().Message
            });
          }
        }
      }

      if (requestedConstructedPieces.Count == 0 && failedAllPieceOutputs.Count == 0)
        throw new ArgumentException("No pieces could be analyzed.");

      List<ConstructedPiece> referenceConstructedPieces = new List<ConstructedPiece>();
      foreach (TierReferenceEntry entry in TierReferenceData.Entries.Values.OrderBy(item => item.PieceTypeName, StringComparer.Ordinal))
      {
        Type pieceType;
        if (!catalog.TryResolvePieceType(entry.PieceTypeName, out pieceType))
          continue;

        string ignoredTierHintMessage;
        referenceConstructedPieces.Add(catalog.ConstructPiece(pieceType, null, out ignoredTierHintMessage));
      }

      List<MobilityStatistics> requestedStatistics = new List<MobilityStatistics>();
      foreach (ConstructedPiece constructedPiece in requestedConstructedPieces)
      {
        try
        {
          requestedStatistics.Add(analyzer.Analyze(constructedPiece, options.Files, options.Ranks, densityPercents));
        }
        catch (Exception ex)
        {
          if (options.AllPieces)
          {
            failedAllPieceOutputs.Add(CreateFailedPieceOutput(constructedPiece, ex.GetBaseException().Message));
          }
          else
          {
            unresolvedRequestedPieces.Add(new PieceConstructionFailure(constructedPiece.RequestedName, constructedPiece.PieceType, ex.GetBaseException().Message));
          }
        }
      }

      IReadOnlyList<MobilityStatistics> referenceStatistics = analyzer.Analyze(referenceConstructedPieces, options.Files, options.Ranks, densityPercents);
      MaterialAssessment assessment = new MaterialAssessment();
      IReadOnlyList<AssessedPiece> assessedPieces = assessment.Assess(requestedStatistics, referenceStatistics);

      List<PieceOutput> pieceOutputs = assessedPieces
        .Select(assessedPiece => ToPieceOutput(assessedPiece))
        .OrderBy(piece => piece.Name, StringComparer.Ordinal)
        .ToList();
      pieceOutputs.AddRange(failedAllPieceOutputs.OrderBy(piece => piece.Name, StringComparer.Ordinal));

      AnalysisOutput output = new AnalysisOutput
      {
        Board = new BoardOutput { Files = options.Files, Ranks = options.Ranks },
        DensityPercents = densityPercents.ToList(),
        GeneratedAtUtc = DateTime.UtcNow,
        Pieces = pieceOutputs,
        UnresolvedPieces = unresolvedRequestedPieces
          .OrderBy(piece => piece.RequestedName, StringComparer.Ordinal)
          .Select(piece => new UnresolvedPieceOutput { Name = piece.RequestedName, Reason = piece.Reason })
          .ToList()
      };

      string json = JsonConvert.SerializeObject(
        output,
        Formatting.Indented,
        new JsonSerializerSettings
        {
          ContractResolver = new DefaultContractResolver
          {
            NamingStrategy = new CamelCaseNamingStrategy()
          },
          NullValueHandling = NullValueHandling.Include
        });

      if (string.IsNullOrEmpty(options.OutputPath))
      {
        Console.Out.Write(json);
      }
      else
      {
        File.WriteAllText(options.OutputPath, json, Encoding.UTF8);
      }

      return 0;
    }

    private static PieceOutput CreateFailedPieceOutput(ConstructedPiece constructedPiece, string reason)
    {
      return new PieceOutput
      {
        Name = constructedPiece.PieceType.Name,
        Notation = constructedPiece.Notation,
        Tier = constructedPiece.Tier.HasValue ? constructedPiece.Tier.Value.ToString() : null,
        TierSource = constructedPiece.TierSource,
        MidgameValue = constructedPiece.MidgameValue,
        EndgameValue = constructedPiece.EndgameValue,
        AverageDirectionsAttacked = null,
        AverageSafeChecks = null,
        MobilityByDensityPercent = null,
        Percentiles = null,
        Warnings = new List<PieceWarning>(),
        ConstructionError = reason
      };
    }

    private static PieceOutput ToPieceOutput(AssessedPiece assessedPiece)
    {
      MobilityStatistics statistics = assessedPiece.Statistics;
      return new PieceOutput
      {
        Name = statistics.SourcePiece.PieceType.Name,
        Notation = statistics.SourcePiece.Piece.Notation != null && statistics.SourcePiece.Piece.Notation.Length > 0
          ? statistics.SourcePiece.Piece.Notation[0]
          : statistics.SourcePiece.Notation,
        Tier = statistics.SourcePiece.Tier.HasValue ? statistics.SourcePiece.Tier.Value.ToString() : null,
        TierSource = statistics.SourcePiece.TierSource,
        MidgameValue = statistics.SourcePiece.Piece.MidgameValue,
        EndgameValue = statistics.SourcePiece.Piece.EndgameValue,
        AverageDirectionsAttacked = statistics.AverageDirectionsAttacked,
        AverageSafeChecks = statistics.AverageSafeChecks,
        MobilityByDensityPercent = statistics.MobilityByDensityPercent
          .OrderBy(entry => entry.Key)
          .ToDictionary(entry => entry.Key.ToString(), entry => entry.Value),
        Percentiles = assessedPiece.Percentiles,
        Warnings = assessedPiece.Warnings,
        ConstructionError = null
      };
    }

    private static List<string> GetRequestedPieceNames(Options options, PieceCatalog catalog)
    {
      if (options.AllPieces)
        return catalog.GetAllConstructiblePieceTypes().Select(type => type.Name).ToList();

      if (options.PieceNames.Count > 0)
        return options.PieceNames.ToList();

      return catalog.GetDefaultRosterPieceNames().ToList();
    }

    private static Options ParseArguments(string[] args)
    {
      Options options = new Options
      {
        Files = 8,
        Ranks = 8
      };

      for (int index = 0; index < args.Length; index++)
      {
        switch (args[index])
        {
          case "--piece":
            options.PieceNames.Add(ReadRequiredValue(args, ref index, "--piece"));
            break;
          case "--all":
            options.AllPieces = true;
            break;
          case "--tier":
            string rawTier = ReadRequiredValue(args, ref index, "--tier");
            PieceTier parsedTier;
            if (!TierReferenceData.TryParseTier(rawTier, out parsedTier))
              throw new ArgumentException("Invalid --tier value. Expected Weak, Pawn, Minor, Major, Jack, Queen, or Amazon.");
            options.TierHint = parsedTier;
            break;
          case "--files":
            options.Files = ParsePositiveInteger(ReadRequiredValue(args, ref index, "--files"), "--files");
            break;
          case "--ranks":
            options.Ranks = ParsePositiveInteger(ReadRequiredValue(args, ref index, "--ranks"), "--ranks");
            break;
          case "--out":
            options.OutputPath = ReadRequiredValue(args, ref index, "--out");
            break;
          default:
            throw new ArgumentException(string.Format("Unknown argument: {0}", args[index]));
        }
      }

      return options;
    }

    private static string ReadRequiredValue(string[] args, ref int index, string optionName)
    {
      if (index + 1 >= args.Length)
        throw new ArgumentException(string.Format("Missing value for {0}.", optionName));

      index++;
      return args[index];
    }

    private static int ParsePositiveInteger(string rawValue, string optionName)
    {
      int value;
      if (!int.TryParse(rawValue, out value) || value <= 0)
        throw new ArgumentException(string.Format("{0} must be a positive integer.", optionName));
      return value;
    }
  }
}
