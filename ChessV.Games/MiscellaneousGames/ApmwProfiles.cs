using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ChessV.Games
{
  public sealed class ApmwCpuArmyProfile
  {
    public string Name { get; private set; }
    public string BackRank { get; private set; }
    public string PromotionPieces { get; private set; }
    public string AttendantPromotions { get; private set; }
    public bool CornerCastlersAreColorbound { get; private set; }

    internal ApmwCpuArmyProfile(
      string name,
      string backRank,
      string promotionPieces,
      string attendantPromotions,
      bool cornerCastlersAreColorbound = false)
    {
      Name = name;
      BackRank = backRank;
      PromotionPieces = promotionPieces;
      AttendantPromotions = attendantPromotions;
      CornerCastlersAreColorbound = cornerCastlersAreColorbound;
    }

    public string BackRankForPlayer(int player)
    {
      return player == 0 ? BackRank.ToUpperInvariant() : BackRank;
    }
  }

  public sealed class ApmwCastlingPlan
  {
    internal ApmwCastlingPlan(
      int kingFromFile,
      int kingToFile,
      int castlerFromFile,
      int castlerToFile,
      bool kingSide)
    {
      KingFromFile = kingFromFile;
      KingToFile = kingToFile;
      CastlerFromFile = castlerFromFile;
      CastlerToFile = castlerToFile;
      KingSide = kingSide;
    }

    public int KingFromFile { get; private set; }
    public int KingToFile { get; private set; }
    public int CastlerFromFile { get; private set; }
    public int CastlerToFile { get; private set; }
    public bool KingSide { get; private set; }
  }

  public sealed class ApmwGeometryProfile
  {
    private readonly int[] metadataGeometryParameters;
    private readonly string[] fenRankPropertyNames;
    private readonly IReadOnlyDictionary<string, ApmwCpuArmyProfile> cpuArmies;

    public string StageId { get; private set; }
    public string GameName { get; private set; }
    public int Files { get; private set; }
    public int Ranks { get; private set; }
    public int CardSlotsPerPlayer { get; private set; }
    public int KingFile { get; private set; }
    public int QueenSideCornerFile { get { return 0; } }
    public int KingSideCornerFile { get { return Files - 1; } }
    public int HumanFormationRanks { get { return Ranks - 3; } }
    public int PlayerBackRank { get { return HumanFormationRanks - 1; } }
    public int PlayerPawnRank { get { return PlayerBackRank - 1; } }
    public int PlayerOuterRank { get { return PlayerBackRank - 2; } }
    public IReadOnlyList<int> MetadataGeometryParameters
    {
      get { return Array.AsReadOnly((int[])metadataGeometryParameters.Clone()); }
    }
    public IReadOnlyList<string> FenRankPropertyNames
    {
      get { return Array.AsReadOnly((string[])fenRankPropertyNames.Clone()); }
    }
    public IReadOnlyDictionary<string, ApmwCpuArmyProfile> CpuArmies
    {
      get { return cpuArmies; }
    }

    internal ApmwGeometryProfile(
      string gameName,
      int files,
      int ranks,
      int cardSlotsPerPlayer,
      IEnumerable<ApmwCpuArmyProfile> cpuArmies)
    {
      StageId = files + "x" + ranks;
      GameName = gameName;
      Files = files;
      Ranks = ranks;
      CardSlotsPerPlayer = cardSlotsPerPlayer;
      KingFile = files / 2;
      metadataGeometryParameters = new[] { files, ranks, cardSlotsPerPlayer };
      fenRankPropertyNames = CreateFenRankPropertyNames(ranks);

      ApmwCpuArmyProfile[] armies = cpuArmies.ToArray();
      if (armies.Any(profile => profile.BackRank.Length != files))
        throw new InvalidOperationException("Every APMW CPU back rank must match its profile width.");
      this.cpuArmies = new ReadOnlyDictionary<string, ApmwCpuArmyProfile>(
        armies.ToDictionary(profile => profile.Name, StringComparer.Ordinal));
    }

    public ApmwCpuArmyProfile ResolveCpuArmy(string enemyArmy)
    {
      ApmwCpuArmyProfile army;
      if (enemyArmy != null && cpuArmies.TryGetValue(enemyArmy, out army))
        return army;
      return cpuArmies[ApmwProfiles.StandardArmy];
    }

    public string EmptyRow
    {
      get { return Files.ToString(); }
    }

    public string CpuPawnRow(int cpuPlayer)
    {
      return new string(cpuPlayer == 0 ? 'P' : 'p', Files);
    }

    public int HomeRank(int player)
    {
      return player == 0 ? 0 : Ranks - 1;
    }

    public int KingDestinationFile(bool kingSide)
    {
      return KingFile + (kingSide ? 2 : -2);
    }

    public int CastlerDestinationFile(bool kingSide)
    {
      return KingFile + (kingSide ? 1 : -1);
    }

    public ApmwCastlingPlan CreateCastlingPlan(
      int castlerSourceFile,
      bool colorboundCastler)
    {
      if (castlerSourceFile < 0 || castlerSourceFile >= Files ||
          castlerSourceFile == KingFile)
      {
        throw new ArgumentOutOfRangeException(
          "castlerSourceFile",
          "An APMW castler must start on a different file from the king.");
      }

      bool kingSide = castlerSourceFile > KingFile;
      int kingDestination = KingDestinationFile(kingSide);
      int castlerDestination = CastlerDestinationFile(kingSide);

      // A CwDA colorbound castler must remain on its original square color.
      // The king's vacated source file is the established APMW fallback.
      if (colorboundCastler &&
          ((castlerSourceFile - castlerDestination) & 1) != 0)
      {
        castlerDestination = KingFile;
      }

      if (kingDestination < 0 || kingDestination >= Files ||
          castlerDestination < 0 || castlerDestination >= Files ||
          kingDestination == castlerDestination)
      {
        throw new InvalidOperationException(
          string.Format(
            "Invalid APMW castling geometry for {0} from file {1}.",
            StageId,
            castlerSourceFile));
      }

      return new ApmwCastlingPlan(
        KingFile,
        kingDestination,
        castlerSourceFile,
        castlerDestination,
        kingSide);
    }

    public string FenArrayTemplate
    {
      get
      {
        return string.Join(
          "/",
          fenRankPropertyNames.Select(propertyName => "#{" + propertyName + "}"));
      }
    }

    public string[] ComposeFenRows(
      int humanPlayer,
      ApmwCpuArmyProfile cpuArmy,
      IReadOnlyList<string> humanRowsBySourceRank)
    {
      if (humanPlayer != 0 && humanPlayer != 1)
        throw new ArgumentOutOfRangeException("humanPlayer");
      if (cpuArmy == null)
        throw new ArgumentNullException("cpuArmy");
      if (humanRowsBySourceRank == null ||
          humanRowsBySourceRank.Count != HumanFormationRanks)
      {
        throw new ArgumentException(
          "Human formation rows must match the geometry-driven formation rank count.",
          "humanRowsBySourceRank");
      }

      int cpuPlayer = humanPlayer ^ 1;
      var rows = new List<string>(Ranks);
      if (humanPlayer == 0)
      {
        rows.Add(cpuArmy.BackRankForPlayer(cpuPlayer));
        rows.Add(CpuPawnRow(cpuPlayer));
        rows.Add(EmptyRow);
        rows.AddRange(humanRowsBySourceRank);
      }
      else
      {
        rows.AddRange(humanRowsBySourceRank.Reverse());
        rows.Add(EmptyRow);
        rows.Add(CpuPawnRow(cpuPlayer));
        rows.Add(cpuArmy.BackRankForPlayer(cpuPlayer));
      }
      return rows.ToArray();
    }

    public void ValidateMetadata(GameAttribute gameAttribute)
    {
      if (gameAttribute == null)
        throw new InvalidOperationException("APMW requires game metadata before board creation.");
      if (gameAttribute.GeometryType != typeof(Geometry.Rectangular) ||
          gameAttribute.GeometryParameters.Length < 2 ||
          gameAttribute.GeometryParameters[0] != Files ||
          gameAttribute.GeometryParameters[1] != Ranks)
      {
        throw new InvalidOperationException(
          string.Format(
            "APMW profile {0} does not match game metadata.",
            StageId));
      }
    }

    private static string[] CreateFenRankPropertyNames(int ranks)
    {
      if (ranks == 8)
      {
        return new[]
        {
          "BlackPieces",
          "BlackPawns",
          "BlackOuter",
          "BlackFourth",
          "WhiteFourth",
          "WhiteOuter",
          "WhitePawns",
          "WhitePieces",
        };
      }

      return Enumerable.Range(0, ranks)
        .Select(rank => "ApmwRank" + rank)
        .ToArray();
    }
  }

  public static class ApmwProfiles
  {
    public const string StandardGameName = "Archipelago Multiworld";
    public const string GrandGameName = "Archipelago Multiworld Super-Sized";
    public const string TenByTenGameName = "Archipelago Multiworld 10x10";
    public const string TwelveByTenGameName = "Archipelago Multiworld 12x10";
    public const string TwelveByTwelveGameName = "Archipelago Multiworld 12x12";

    public const string StandardArmy = "";
    public const string ColourboundClobberers = "Colourbound Clobberers (Betza)";
    public const string RemarkableRookies = "Remarkable Rookies (Betza)";
    public const string NuttyKnights = "Nutty Knights (Betza)";

    public static ApmwGeometryProfile Standard { get; private set; }
    public static ApmwGeometryProfile Grand { get; private set; }
    public static ApmwGeometryProfile TenByTen { get; private set; }
    public static ApmwGeometryProfile TwelveByTen { get; private set; }
    public static ApmwGeometryProfile TwelveByTwelve { get; private set; }
    public static IReadOnlyList<ApmwGeometryProfile> Stages { get; private set; }

    static ApmwProfiles()
    {
      Standard = new ApmwGeometryProfile(
        StandardGameName,
        8,
        8,
        3,
        CreateEightFileArmies());

      Grand = new ApmwGeometryProfile(
        GrandGameName,
        10,
        8,
        3,
        CreateTenFileArmies());

      TenByTen = new ApmwGeometryProfile(
        TenByTenGameName,
        10,
        10,
        3,
        CreateTenFileArmies());

      TwelveByTen = new ApmwGeometryProfile(
        TwelveByTenGameName,
        12,
        10,
        3,
        CreateTwelveFileArmies());

      TwelveByTwelve = new ApmwGeometryProfile(
        TwelveByTwelveGameName,
        12,
        12,
        3,
        CreateTwelveFileArmies());

      Stages = new ReadOnlyCollection<ApmwGeometryProfile>(
        new[]
        {
          Standard,
          Grand,
          TenByTen,
          TwelveByTen,
          TwelveByTwelve,
        });
    }

    private static IEnumerable<ApmwCpuArmyProfile> CreateEightFileArmies()
    {
      return new[]
      {
        new ApmwCpuArmyProfile(StandardArmy, "rnbqkbnr", "rnbq", ""),
        new ApmwCpuArmyProfile(
          ColourboundClobberers,
          "gxeakexg",
          "gxea",
          "",
          true),
        new ApmwCpuArmyProfile(RemarkableRookies, "stickits", "stic", ""),
        new ApmwCpuArmyProfile(NuttyKnights, "hlmykmlh", "hlmy", ""),
      };
    }

    private static IEnumerable<ApmwCpuArmyProfile> CreateTenFileArmies()
    {
      return new[]
      {
        new ApmwCpuArmyProfile(StandardArmy, "rnabqkbcnr", "rnbq", "ac"),
        new ApmwCpuArmyProfile(
          ColourboundClobberers,
          "gxqeakecxg",
          "gxea",
          "qc",
          true),
        new ApmwCpuArmyProfile(RemarkableRookies, "staickiqts", "stic", "aq"),
        new ApmwCpuArmyProfile(NuttyKnights, "hlamykmclh", "hlmy", "ac"),
      };
    }

    private static IEnumerable<ApmwCpuArmyProfile> CreateTwelveFileArmies()
    {
      return new[]
      {
        new ApmwCpuArmyProfile(StandardArmy, "rjnabqkbcnjr", "rnbq", "acj"),
        new ApmwCpuArmyProfile(
          ColourboundClobberers,
          "gjxqeakecxjg",
          "gxea",
          "qcj",
          true),
        new ApmwCpuArmyProfile(RemarkableRookies, "sjtaickiqtjs", "stic", "aqj"),
        new ApmwCpuArmyProfile(NuttyKnights, "hjlamykmcljh", "hlmy", "acj"),
      };
    }
  }
}
