using ChessV.Base;
using ChessV.Games;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Archipelago.APChessV
{
  internal sealed class ApmwLocationProfile
  {
    private static readonly string[] LegacyStageOrder =
    {
      "8x8",
      "10x8",
      "10x10",
      "12x10",
      "12x12",
    };

    private static readonly string[] OrderedProgressive6x8StageOrder =
    {
      "6x8",
      "8x8",
      "10x8",
      "10x10",
      "12x10",
      "12x12",
    };

    private static readonly Dictionary<string, string> CheckmateNames =
      new Dictionary<string, string>(StringComparer.Ordinal)
      {
        { "6x8", "Checkmate 6x8" },
        { "8x8", "Checkmate Minima" },
        { "10x8", "Checkmate Maxima" },
        { "10x10", "Checkmate 10x10" },
        { "12x10", "Checkmate 12x10" },
        { "12x12", "Checkmate 12x12" },
      };

    private ApmwLocationProfile(int files, int ranks)
    {
      Files = files;
      Ranks = ranks;
      StageId = files + "x" + ranks;
      StageIndex = Array.IndexOf(OrderedProgressive6x8StageOrder, StageId);
      if (StageIndex < 0)
        throw new ArgumentOutOfRangeException(nameof(files), "Unsupported APMW location geometry " + StageId + ".");
    }

    public string StageId { get; }
    public int StageIndex { get; }
    public int Files { get; }
    public int Ranks { get; }
    public int CpuPawnCount { get { return Files; } }
    public int CpuNonKingCount { get { return Files - 1; } }
    public int MaximumAnyCaptureCount { get { return CpuPawnCount + CpuNonKingCount - 1; } }
    public int CenterLeftFile { get { return Files / 2 - 1; } }
    public int CenterRightFile { get { return Files / 2; } }
    public int CenterLowerRank { get { return Ranks / 2 - 1; } }
    public int CenterUpperRank { get { return Ranks / 2; } }
    public string CheckmateLocation { get { return CheckmateNames[StageId]; } }
    public bool IsFinalStage { get { return StageId == "12x12"; } }

    public static ApmwLocationProfile For(int files, int ranks)
    {
      return new ApmwLocationProfile(files, ranks);
    }

    public IReadOnlyList<string> CheckmateLocationsThroughStage()
    {
      return CheckmateLocationsThroughStage(Goal.OrderedProgressive);
    }

    public IReadOnlyList<string> CheckmateLocationsThroughStage(Goal goal)
    {
      string[] stageOrder = ApmwGoalSemantics.UsesSixByEightOpening(goal)
        ? OrderedProgressive6x8StageOrder
        : LegacyStageOrder;
      int stageIndex = Array.IndexOf(stageOrder, StageId);
      if (stageIndex < 0)
      {
        throw new InvalidOperationException(
          "APMW location geometry " + StageId + " is not part of goal " + goal + ".");
      }
      return stageOrder
        .Take(stageIndex + 1)
        .Select(stage => CheckmateNames[stage])
        .ToList()
        .AsReadOnly();
    }

    public int HomeRank(int player)
    {
      return player == 0 ? 0 : Ranks - 1;
    }

    public bool IsCenter(int file, int rank)
    {
      return (file == CenterLeftFile || file == CenterRightFile) &&
        (rank == CenterLowerRank || rank == CenterUpperRank);
    }
  }

  public class CaptureLookup
  {
    public static Dictionary<string, string> SixByEightNames =
      new Dictionary<string, string>()
      {
        { "A", "Queen's Knight" },
        { "B", "Queen's Bishop" },
        { "C", "Queen's Rook" },
        { "D", "Checkmate 6x8" }, // not used
        { "E", "King's Bishop" },
        { "F", "King's Knight" },
      };

    public static Dictionary<string, string> MinimaNames =
      new Dictionary<string, string>()
      {
        { "A", "Queen's Rook" },
        { "B", "Queen's Knight" },
        { "C", "Queen's Bishop" },
        { "D", "Queen" },
        { "E", "Checkmate Minima" }, // not used
        { "F", "King's Bishop" },
        { "G", "King's Knight" },
        { "H", "King's Rook" }
      };
    public static Dictionary<string, string> MaximaNames =
      new Dictionary<string, string>()
      {
        { "A", "Queen's Rook" },
        { "B", "Queen's Knight" },
        { "C", "Queen's Attendant" },
        { "D", "Queen's Bishop" },
        { "E", "Queen" },
        { "F", "Checkmate Maxima" }, // not used
        { "G", "King's Bishop" },
        { "H", "King's Attendant" },
        { "I", "King's Knight" },
        { "J", "King's Rook" },
      };
    public static Dictionary<string, string> TwelveFileNames =
      new Dictionary<string, string>()
      {
        { "A", "Queen's Rook" },
        { "B", "Queen's Outer Attendant" },
        { "C", "Queen's Knight" },
        { "D", "Queen's Attendant" },
        { "E", "Queen's Bishop" },
        { "F", "Queen" },
        { "G", "Checkmate 12x10" }, // not used
        { "H", "King's Bishop" },
        { "I", "King's Attendant" },
        { "J", "King's Knight" },
        { "K", "King's Outer Attendant" },
        { "L", "King's Rook" },
      };

    public string fileToLocation(int numFiles, string fileNotation)
    {
      string qualifiedName = fileToSubname(numFiles, fileNotation);
      return "Capture Piece " + qualifiedName;
    }

    private string fileToSubname(int numFiles, string fileNotation)
    {
      if (fileNotation == null)
        throw new ArgumentNullException(nameof(fileNotation));

      Dictionary<string, string> names;
      switch (numFiles)
      {
        case 6:
          names = SixByEightNames;
          break;
        case 8:
          names = MinimaNames;
          break;
        case 10:
          names = MaximaNames;
          break;
        case 12:
          names = TwelveFileNames;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(numFiles), "Unsupported APMW capture width.");
      }

      if (!names.TryGetValue(fileNotation, out string subname))
        throw new ArgumentOutOfRangeException(nameof(fileNotation), "Unsupported APMW capture file.");
      return subname;
    }
  }
}
