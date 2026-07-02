using ChessV;
using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Archipelago.APChessV
{
  internal static class ItemGenerationValues
  {
    public const int Major = 485;
    public const int Minor = 300;
    public const int Pawn = 100;
    public const int Weak = 75;
    public const int Jack = 700;
    public const int Queen = 900;
    public const int Amazon = 1300;
  }

  internal readonly struct BoardCoordinate
  {
    public BoardCoordinate(int rank, int file)
    {
      Rank = rank;
      File = file;
    }

    public int Rank { get; }
    public int File { get; }

    public KeyValuePair<int, int> ToKeyValuePair()
    {
      return new KeyValuePair<int, int>(Rank, File);
    }
  }

  internal sealed class PieceSetLayout
  {
    private PieceSetLayout(
      List<PieceType> leftBackRank,
      PieceType centerBackRank,
      List<PieceType> rightBackRank,
      List<PieceType> outerRank)
    {
      LeftBackRank = leftBackRank;
      CenterBackRank = centerBackRank;
      RightBackRank = rightBackRank;
      OuterRank = outerRank;
    }

    public List<PieceType> LeftBackRank { get; }
    public PieceType CenterBackRank { get; set; }
    public List<PieceType> RightBackRank { get; }
    public List<PieceType> OuterRank { get; }

    public int BackRankPieceCount
    {
      get { return LeftBackRank.Count(piece => piece != null) + RightBackRank.Count(piece => piece != null); }
    }

    public int AvailableBackRankSpaces
    {
      get { return LeftBackRank.Count(piece => piece == null) + RightBackRank.Count(piece => piece == null); }
    }

    public int AvailableOuterRankSpaces
    {
      get { return OuterRank.Count(piece => piece == null); }
    }

    public static PieceSetLayout Empty(int numFiles, PieceType centerBackRank)
    {
      return new PieceSetLayout(
        Enumerable.Repeat<PieceType>(null, numFiles / 2).ToList(),
        centerBackRank,
        Enumerable.Repeat<PieceType>(null, numFiles / 2 - 1).ToList(),
        Enumerable.Repeat<PieceType>(null, numFiles).ToList());
    }

    public static PieceSetLayout FromPieceList(int numFiles, List<PieceType> pieces)
    {
      return new PieceSetLayout(
        pieces.Take(numFiles / 2).ToList(),
        pieces[numFiles / 2],
        pieces.Skip(numFiles / 2 + 1).Take(numFiles / 2 - 1).ToList(),
        pieces.Skip(numFiles).Take(numFiles).ToList());
    }

    public List<PieceType> ToPieceList()
    {
      List<PieceType> output = new List<PieceType>();
      output.AddRange(LeftBackRank);
      output.Add(CenterBackRank);
      output.AddRange(RightBackRank);
      output.AddRange(OuterRank);
      return output;
    }
  }

  internal static class PieceMaterialAccounting
  {
    public static void RecordPromotion(
      HashSet<string> promotionPieces,
      PieceType piece,
      int player,
      int expectedMaterial,
      ref int spareMaterial)
    {
      if (piece == null)
        return;

      promotionPieces.Add(piece.Notation[player]);
      spareMaterial += expectedMaterial - piece.MidgameValue;
    }
  }

  internal static class PlayerPieceSetGeneration
  {
    public static (Dictionary<KeyValuePair<int, int>, PieceType>, string) Generate(int numFiles)
    {
      int spareMaterial = 0;

      ApmwConfig.getInstance().seed();
      List<string> promotions = new List<string>();
      List<int> order;
      // The first numFiles entries are the back rank; overflow majors use the next rank.
      List<PieceType> withMajors = MajorPieceGeneration.Generate(numFiles, out order, promotions, ref spareMaterial);
      // replace some or all majors with queens
      List<PieceType> withQueens = QueenGeneration.Substitute(numFiles, withMajors, order, promotions, ref spareMaterial);
      // replace distinct reserved major slots with amazons
      List<PieceType> withAmazons = AmazonGeneration.Substitute(numFiles, withQueens, order, promotions, ref spareMaterial);
      // then add minor pieces until out of space
      List<PieceType> withMinors = MinorPieceGeneration.Generate(numFiles, withAmazons, promotions, ref spareMaterial);
      List<PieceType> withPawns = PawnGeneration.GeneratePawns(numFiles, withMinors, spareMaterial);

      Dictionary<KeyValuePair<int, int>, PieceType> pieces = new Dictionary<KeyValuePair<int, int>, PieceType>();
      for (int rankIndex = 0; rankIndex < 5; rankIndex++)
        for (int fileIndex = 0; fileIndex < numFiles; fileIndex++)
        {
          PieceType piece = withPawns[rankIndex * numFiles + fileIndex];
          if (piece != null)
          {
            var coordinate = new BoardCoordinate(4 - rankIndex, fileIndex);
            pieces.Add(coordinate.ToKeyValuePair(), piece);
          }
        }

      return (pieces, string.Join("", promotions));
    }
  }

  internal static class PawnGeneration
  {
    private enum PawnUpgrade
    {
      Core,
      Min,
      Best,
      Sergeant
    }

    public static List<PieceType> SetupPawnOptions()
    {
      var config = ApmwConfig.getInstance();
      var standardPawn = ApmwCore.getInstance().pawns.First(item => item.Notation[0].Equals("P"));
      var berolinaPawn = ApmwCore.getInstance().pawns.First(item => item.Notation[0].Equals("Ŕ"));
      var checkersPawn = ApmwCore.getInstance().pawns.First(item => item.Name.Equals("Checkers"));

      switch (config.Pawns)
      {
        case FairyPawns.Mixed:
          return ApmwCore.getInstance().pawns.ToList();
        case FairyPawns.AnyPawn:
          return new List<PieceType>() { standardPawn, berolinaPawn };
        case FairyPawns.AnyFairy:
          return new List<PieceType>() { berolinaPawn, checkersPawn };
        case FairyPawns.AnyClassical:
          return new List<PieceType>() { standardPawn, checkersPawn };
        case FairyPawns.Vanilla:
          return new List<PieceType>() { standardPawn };
        case FairyPawns.Berolina:
          return new List<PieceType>() { berolinaPawn };
        case FairyPawns.Checkers:
          return new List<PieceType>() { checkersPawn };
        default:
          return new List<PieceType>() { standardPawn };
      }
    }

    public static PieceType GetNextPawn(Random randomSource, List<PieceType> options)
    {
      return GetNextPawn(randomSource, options, PawnUpgrade.Core);
    }

    private static PieceType GetNextPawn(Random randomSource, List<PieceType> options, PawnUpgrade upgrade)
    {
      List<PieceType> limited;
      switch (upgrade)
      {
        case PawnUpgrade.Sergeant:
          if (ApmwConfig.getInstance().Pawns == FairyPawns.Vanilla)
            return ApmwCore.getInstance().sergeants.First(s => s.Name == "Sergeant");
          else
            return ApmwCore.getInstance().sergeants.ElementAt(
              randomSource.Next(
                ApmwCore.getInstance().sergeants.Count));
        case PawnUpgrade.Best:
          limited = options.Where(item => item.MidgameValue >= ItemGenerationValues.Weak).ToList();
          if (limited.Count == 0)
            return GetNextPawn(randomSource, options, PawnUpgrade.Sergeant);
          return limited.ElementAt(randomSource.Next(limited.Count));
        case PawnUpgrade.Min:
          limited = options.Where(item => item.MidgameValue <= options.Min(item => item.MidgameValue)).ToList();
          return limited.ElementAt(randomSource.Next(limited.Count));
        case PawnUpgrade.Core:
        default:
          return options.ElementAt(randomSource.Next(options.Count));
      }
    }

    private static void FillPawnRank(List<PieceType> targetRank, int numFiles, int startIndex, Queue<PieceType> adjustedPawns,
        Random randomLocations)
    {
      for (int i = startIndex;
        i < numFiles * (startIndex / numFiles + 1)
          && adjustedPawns.Count > 0
          && targetRank.Count(item => item == null) > 0;
        i++)
      {
        var piece = adjustedPawns.Dequeue();
        PiecePlacement.ChooseIndexAndPlace(targetRank, randomLocations, piece);
      }
    }

    public static List<PieceType> PickPawns(Random randomPieces, int adjustedPawnValues, int remainingPawnSpaces, int foundPawns)
    {
      return PickPawns(randomPieces, SetupPawnOptions(), adjustedPawnValues, remainingPawnSpaces, foundPawns);
    }

    public static List<PieceType> PickPawns(Random randomPieces, List<PieceType> pawnOptions, int adjustedPawnValues, int remainingPawnSpaces, int foundPawns)
    {
      var mode = ApmwConfig.getInstance().PawnUpgrades;
      var sergeants = ApmwCore.getInstance().sergeants.ToList();
      List<PieceType> workingPawns = new List<PieceType>();
      while (workingPawns.Count < remainingPawnSpaces && adjustedPawnValues > 0)
      {
        PieceType picked;
        switch (mode)
        {
          case FairyPawnUpgrades.Pool:
            picked = PickPawnPoolMode(randomPieces, pawnOptions, sergeants, adjustedPawnValues, foundPawns, workingPawns.Count);
            break;
          case FairyPawnUpgrades.Max:
          case FairyPawnUpgrades.SuperMax:
            picked = PickPawnMaxMode(randomPieces, pawnOptions, adjustedPawnValues, foundPawns, workingPawns.Count);
            break;
          case FairyPawnUpgrades.Off:
          default:
          {
            var upgrade = adjustedPawnValues <= ItemGenerationValues.Pawn ? PawnUpgrade.Min : PawnUpgrade.Core;
            picked = GetNextPawn(randomPieces, pawnOptions, upgrade);
            break;
          }
        }
        if (picked == null) break;
        workingPawns.Add(picked);
        adjustedPawnValues -= picked.MidgameValue;
      }
      // TODO(chesslogic): Add an Option not to upgrade pawns. For now, we'll always maximize material value.
      UpgradePawns(randomPieces, adjustedPawnValues, pawnOptions, workingPawns, mode);
      return workingPawns;
    }

    private static PieceType PickPawnPoolMode(Random randomPieces, List<PieceType> pawnOptions, List<PieceType> sergeants,
        int budget, int foundPawns, int currentCount)
    {
      var augmented = pawnOptions.Concat(sergeants).ToList();
      if (augmented.Count == 0) return null;
      var candidate = augmented[randomPieces.Next(augmented.Count)];
      if (!sergeants.Contains(candidate)) return candidate;
      bool allowed = currentCount >= foundPawns
        ? budget >= candidate.MidgameValue
        : PigeonholeAllowsSergeant(budget, foundPawns, currentCount, candidate.MidgameValue, pawnOptions);
      if (allowed) return candidate;
      // Sergeant rejected; re-pick uniformly from non-sergeant options.
      var nonSerg = pawnOptions.Where(p => !sergeants.Contains(p)).ToList();
      if (nonSerg.Count == 0) return null;
      return GetNextPawn(randomPieces, nonSerg, FallbackUpgrade(budget, foundPawns, currentCount));
    }

    private static PieceType PickPawnMaxMode(Random randomPieces, List<PieceType> pawnOptions,
        int budget, int foundPawns, int currentCount)
    {
      var sergeant = GetNextPawn(randomPieces, pawnOptions, PawnUpgrade.Sergeant);
      bool allowed = currentCount >= foundPawns
        ? budget >= sergeant.MidgameValue
        : PigeonholeAllowsSergeant(budget, foundPawns, currentCount, sergeant.MidgameValue, pawnOptions);
      if (allowed) return sergeant;
      return GetNextPawn(randomPieces, pawnOptions, FallbackUpgrade(budget, foundPawns, currentCount));
    }

    // Slot-aware fallback selector: if remaining budget per still-required slot cannot
    // afford a pawn-value piece, force the cheapest option so the count guarantee holds.
    private static PawnUpgrade FallbackUpgrade(int budget, int foundPawns, int currentCount)
    {
      if (budget <= ItemGenerationValues.Pawn) return PawnUpgrade.Min;
      int slotsLeft = Math.Max(1, foundPawns - currentCount);
      if (currentCount < foundPawns && budget / slotsLeft <= ItemGenerationValues.Pawn) return PawnUpgrade.Min;
      return PawnUpgrade.Core;
    }

    private static bool PigeonholeAllowsSergeant(
        int budget, int foundPawns, int currentCount, int sergeantCost,
        List<PieceType> pawnOptions)
    {
      var sergeants = ApmwCore.getInstance().sergeants;
      int slotsStillNeededAfter = Math.Max(0, foundPawns - currentCount - 1);
      if (slotsStillNeededAfter == 0) return budget >= sergeantCost;
      var nonSergeantOptions = pawnOptions.Where(p => !sergeants.Contains(p)).ToList();
      if (nonSergeantOptions.Count == 0) return false;
      int cheapestNonSergeant = nonSergeantOptions.Min(p => p.MidgameValue);
      return (budget - sergeantCost) >= slotsStillNeededAfter * cheapestNonSergeant;
    }

    private static void UpgradePawns(Random randomPieces, int adjustedPawnValues, List<PieceType> pawnOptions,
        List<PieceType> workingPawns, FairyPawnUpgrades mode)
    {
      // Best-upgrade pass: replace the cheapest sub-WEAK_VALUE pieces with Best-tier alternatives.
      var miniIndexes = new Queue<int>(workingPawns.Select((item, index) => new { Piece = item, Index = index })
        .Where(item => item.Piece.MidgameValue < ItemGenerationValues.Weak)
        .OrderBy(item => item.Piece.MidgameValue)
        .Select(item => item.Index));
      while (adjustedPawnValues > 0 && miniIndexes.Count > 0)
      {
        var index = miniIndexes.Dequeue();
        adjustedPawnValues += workingPawns[index].MidgameValue;
        workingPawns[index] = GetNextPawn(randomPieces, pawnOptions, PawnUpgrade.Best);
        adjustedPawnValues -= workingPawns[index].MidgameValue;
      }
      // Pool/Max/SuperMax already placed sergeants per-slot in PickPawns; only Off uses the legacy fallback.
      if (mode != FairyPawnUpgrades.Off) return;
      UpgradeRemainingPawnsToSergeants(randomPieces, adjustedPawnValues, pawnOptions, workingPawns);
    }

    private static void UpgradeRemainingPawnsToSergeants(Random randomPieces, int adjustedPawnValues, List<PieceType> pawnOptions, List<PieceType> workingPawns)
    {
      var sergeantIndexes = new Queue<int>(workingPawns.Select((item, index) => new { Piece = item, Index = index })
        .OrderBy(item => item.Piece.MidgameValue)
        .Select(item => item.Index));
      while (adjustedPawnValues > 0 && sergeantIndexes.Count > 0)
      {
        var index = sergeantIndexes.Dequeue();
        adjustedPawnValues += workingPawns[index].MidgameValue;
        workingPawns[index] = GetNextPawn(randomPieces, pawnOptions, PawnUpgrade.Sergeant);
        adjustedPawnValues -= workingPawns[index].MidgameValue;
      }
    }

    internal static int SuperMaxPawnGuarantee(int numFiles, int foundPawns, int foundConsuls, int foundJacks, int foundMajors, int foundMinors)
    {
      int boardLocationNeeds = (numFiles == 10 ? 19 : 15) - foundConsuls - foundJacks - foundMajors - foundMinors;
      return Math.Min(foundPawns, Math.Max(0, boardLocationNeeds));
    }

    public static List<PieceType> GeneratePawns(int numFiles, List<PieceType> minors, int spareMaterial)
    {
      var core = ApmwCore.getInstance();
      List<PieceType> thirdRank = Enumerable.Repeat<PieceType>(null, numFiles).ToList();
      List<PieceType> fourthRank = Enumerable.Repeat<PieceType>(null, numFiles).ToList();
      List<PieceType> finalRank = Enumerable.Repeat<PieceType>(null, numFiles).ToList();
      List<PieceType> pawnRank = minors.Skip(numFiles).ToList();

      Random randomPieces = new Random(ApmwConfig.getInstance().pawnSeed);
      Random randomLocations = new Random(ApmwConfig.getInstance().pawnLocSeed);
      int startingPieces = pawnRank.Count((item) => item != null);
      int remainingPawnSpaces = 4 * numFiles - startingPieces;

      int adjustedPawnValues = Math.Max(
        core.foundPawns * ItemGenerationValues.Pawn,
        core.foundPawns * ItemGenerationValues.Pawn + spareMaterial + 45);

      int pawnGuarantee = core.foundPawns;
      if (ApmwConfig.getInstance().PawnUpgrades == FairyPawnUpgrades.SuperMax)
        pawnGuarantee = SuperMaxPawnGuarantee(numFiles, core.foundPawns, core.foundConsuls, core.foundJacks, core.foundMajors, core.foundMinors);

      List<PieceType> workingPawns = PickPawns(randomPieces, adjustedPawnValues, remainingPawnSpaces, pawnGuarantee);

      Queue<PieceType> adjustedPawns = new Queue<PieceType>(workingPawns);
      // Fill each rank
      FillPawnRank(pawnRank, numFiles, startingPieces, adjustedPawns, randomLocations);
      FillPawnRank(thirdRank, numFiles, numFiles, adjustedPawns, randomLocations);
      FillPawnRank(fourthRank, numFiles, numFiles * 2, adjustedPawns, randomLocations);
      FillPawnRank(finalRank, numFiles, numFiles * 3, adjustedPawns, randomLocations);

      int remainingForwardness = core.foundPawnForwardness;
      foreach (var rankAdvance in new List<(List<PieceType> SourceRank, List<PieceType> TargetRank)> {
        (pawnRank, thirdRank), (thirdRank, fourthRank), (fourthRank, finalRank),
        (pawnRank, thirdRank), (thirdRank, fourthRank),
        (pawnRank, thirdRank)
      })
      {
        List<int> possibleForwardPawnPositions = new List<int>();
        for (int i = 0; i < rankAdvance.TargetRank.Count; i++)
          if (rankAdvance.TargetRank[i] == null && rankAdvance.SourceRank[i] != null && rankAdvance.SourceRank[i].IsPawn)
            possibleForwardPawnPositions.Add(i);
        for (
          int i = randomLocations.Next(possibleForwardPawnPositions.Count);
          remainingForwardness-- > 0 && possibleForwardPawnPositions.Count > 0;
          i = randomLocations.Next(possibleForwardPawnPositions.Count))
        {
          // swap backward with forward
          rankAdvance.TargetRank[possibleForwardPawnPositions[i]] = rankAdvance.SourceRank[possibleForwardPawnPositions[i]];
          rankAdvance.SourceRank[possibleForwardPawnPositions[i]] = null;
          possibleForwardPawnPositions.RemoveAt(i);
        }
      }

      List<PieceType> output = new List<PieceType>();
      output.AddRange(minors.Take(numFiles));
      output.AddRange(pawnRank);
      output.AddRange(thirdRank);
      output.AddRange(fourthRank);
      output.AddRange(finalRank);

      return output;
    }
  }

  internal static class MinorPieceGeneration
  {
    public static List<PieceType> Generate(int numFiles, List<PieceType> queens, List<string> promotions, ref int spareMaterial)
    {
      var core = ApmwCore.getInstance();

      HashSet<string> promotionPieces = new HashSet<string>();
      Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
      List<PieceType> minors = core.minors.ToList();
      minors = ArmyPieceFilter.Filter(minors);
      PieceSetLayout layout = PieceSetLayout.FromPieceList(numFiles, queens);

      Random randomPieces = new Random(ApmwConfig.getInstance().minorSeed);
      Random randomLocations = new Random(ApmwConfig.getInstance().minorLocSeed);

      int limit = ApmwConfig.getInstance().minorTypeLimit;
      int player = core.GeriProvider();
      int parity = layout.LeftBackRank.Count((piece) => piece != null) - layout.RightBackRank.Count((piece) => piece != null);
      int backRankPieces = layout.BackRankPieceCount;
      int availableBackRankSpaces = layout.AvailableBackRankSpaces;
      int availableOuterSpaces = layout.AvailableOuterRankSpaces;
      int minorsToPlace = Math.Min(core.foundMinors, availableBackRankSpaces + availableOuterSpaces);
      int backRankMinorsToPlace = Math.Min(availableBackRankSpaces, minorsToPlace);

      for (int i = 0; i < backRankMinorsToPlace; i++)
      {
        var piece = PieceChoice.Choose(ref minors, randomPieces, chosenPieces, limit);
        PieceMaterialAccounting.RecordPromotion(promotionPieces, piece, player, ItemGenerationValues.Minor, ref spareMaterial);
        parity = PiecePlacement.PlaceOnBackRank(new List<int>(), layout, randomLocations, parity, backRankPieces + i, piece);
      }
      for (int i = backRankMinorsToPlace; i < minorsToPlace; i++)
      {
        var piece = PieceChoice.Choose(ref minors, randomPieces, chosenPieces, limit);
        PieceMaterialAccounting.RecordPromotion(promotionPieces, piece, player, ItemGenerationValues.Minor, ref spareMaterial);
        PiecePlacement.ChooseIndexAndPlace(layout.OuterRank, randomLocations, piece);
      }
      spareMaterial += Math.Max(0, core.foundMinors - minorsToPlace) * ItemGenerationValues.Minor;

      promotions.Add(string.Join("", promotionPieces));

      return layout.ToPieceList();
    }
  }

  internal static class QueenGeneration
  {
    public static List<PieceType> Substitute(int numFiles, List<PieceType> majors, List<int> order, List<string> promotions, ref int spareMaterial)
    {
      var core = ApmwCore.getInstance();
      return MajorUpgradeSubstitution.Substitute(
        numFiles,
        majors,
        order,
        promotions,
        core.queens,
        Math.Max(0, core.foundQueens),
        0,
        ApmwConfig.getInstance().queenSeed,
        ApmwConfig.getInstance().queenTypeLimit,
        ItemGenerationValues.Queen,
        ref spareMaterial);
    }
  }

  internal static class AmazonGeneration
  {
    public static List<PieceType> Substitute(int numFiles, List<PieceType> majors, List<int> order, List<string> promotions, ref int spareMaterial)
    {
      var core = ApmwCore.getInstance();
      return MajorUpgradeSubstitution.Substitute(
        numFiles,
        majors,
        order,
        promotions,
        core.amazons,
        Math.Max(0, core.foundAmazons),
        Math.Max(0, core.foundQueens),
        ApmwConfig.getInstance().queenSeed,
        ApmwConfig.getInstance().queenTypeLimit,
        ItemGenerationValues.Amazon,
        ref spareMaterial);
    }
  }

  internal static class MajorUpgradeSubstitution
  {
    public static List<PieceType> Substitute(
      int numFiles,
      List<PieceType> majors,
      List<int> order,
      List<string> promotions,
      IEnumerable<PieceType> upgradePieces,
      int upgradesToSubstitute,
      int reservedAfter,
      int seed,
      int limit,
      int expectedMaterial,
      ref int spareMaterial)
    {
      HashSet<string> promotionPieces = new HashSet<string>();
      Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
      List<PieceType> pieces = ArmyPieceFilter.Filter(upgradePieces);
      Random random = new Random(seed);
      var core = ApmwCore.getInstance();
      int player = core.GeriProvider();

      int majorSlotStart = Math.Min(order.Count, Math.Max(0, core.foundJacks));
      int endExclusive = Math.Min(order.Count, Math.Max(majorSlotStart, order.Count - Math.Max(0, reservedAfter)));
      int startInclusive = Math.Max(majorSlotStart, endExclusive - upgradesToSubstitute);
      for (int i = endExclusive - 1; i >= startInclusive; i--)
      {
        var piece = PieceChoice.Choose(ref pieces, random, chosenPieces, limit);
        if (piece != null)
        {
          PieceMaterialAccounting.RecordPromotion(promotionPieces, piece, player, expectedMaterial, ref spareMaterial);
          majors[MajorOrderIndexToPieceSetIndex(numFiles, order[i], majors.Count)] = piece;
        }
      }
      promotions.Add(string.Join("", promotionPieces));
      return majors;
    }

    public static int MajorOrderIndexToPieceSetIndex(int numFiles, int orderIndex, int pieceSetCount)
    {
      int kingIndex = numFiles / 2;
      int pieceSetIndex =
        orderIndex < kingIndex ? orderIndex :
        orderIndex < numFiles - 1 ? orderIndex + 1 :
        orderIndex;
      if (pieceSetIndex < 0 || pieceSetIndex >= pieceSetCount)
        throw new InvalidOperationException(
          "Major order index " + orderIndex + " mapped outside generated piece set of size " + pieceSetCount + ".");
      return pieceSetIndex;
    }
  }

  internal static class MajorPieceGeneration
  {
    public static List<PieceType> Generate(int numFiles, out List<int> order, List<string> promotions, ref int spareMaterial)
    {
      var core = ApmwCore.getInstance();

      HashSet<string> promotionPieces = new HashSet<string>();
      Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
      order = new List<int>();
      List<PieceType> majors = core.majors.ToList();
      List<PieceType> jacks = core.jacks.ToList();
      majors = ArmyPieceFilter.Filter(majors);
      jacks = ArmyPieceFilter.Filter(jacks);
      PieceSetLayout layout = PieceSetLayout.Empty(numFiles, null);

      Random randomPieces = new Random(ApmwConfig.getInstance().majorSeed);
      Random randomJackPieces = new Random(ApmwConfig.getInstance().majorSeed);
      Random randomLocations = new Random(ApmwConfig.getInstance().majorLocSeed);

      int limit = ApmwConfig.getInstance().majorTypeLimit;
      int majorUpgradesToBe = Math.Max(0, core.foundQueens) + Math.Max(0, core.foundAmazons);
      int player = core.GeriProvider();
      int parity = 0;

      int numKings = core.foundConsuls;
      if (numKings > 0)
      {
        List<PieceType> kings = core.kings;
        // Center the king on D file for 8x8 or E file for 10x10
        int centerFile = (numFiles / 2) - 1;
        layout.LeftBackRank[centerFile] = kings[0];
        if (numKings > 1)
        {
          // Place second king on E file for 8x8 or F file for 10x10
          layout.RightBackRank[0] = kings[0];
        }
      }

      int numJacks = core.foundJacks;
      int numNonMinorPieces = core.foundMajors + numKings + numJacks;
      int backRankCapacity = numFiles - 1;
      int outerRankCapacity = numFiles;
      int placementCapacity = backRankCapacity + outerRankCapacity;
      var piecePicker = new MajorPiecePicker(
        majors,
        jacks,
        randomPieces,
        randomJackPieces,
        chosenPieces,
        limit,
        player,
        promotionPieces,
        numKings,
        numJacks,
        numNonMinorPieces,
        majorUpgradesToBe);

      for (int placementIndex = numKings; placementIndex < Math.Min(placementCapacity, numNonMinorPieces); placementIndex++)
      {
        PieceType piece = piecePicker.Pick(placementIndex, ref spareMaterial);
        if (placementIndex < backRankCapacity)
          parity = PiecePlacement.PlaceOnBackRank(order, layout, randomLocations, parity, placementIndex, piece);
        else
          PlaceOnOuterRank(order, layout, randomLocations, piece, numFiles);
      }
      spareMaterial += Math.Max(0, numNonMinorPieces - placementCapacity) * ItemGenerationValues.Major;

      layout.CenterBackRank = core.kings[core.foundKingPromotions];
      promotions.Add(string.Join("", promotionPieces));

      return layout.ToPieceList();
    }

    private static void PlaceOnOuterRank(
      List<int> order,
      PieceSetLayout layout,
      Random randomLocations,
      PieceType piece,
      int numFiles)
    {
      bool innerSpillSpaceAvailable = layout.OuterRank.Skip(1).Take(numFiles - 2).Any(item => item == null);
      int placedIndex = innerSpillSpaceAvailable
        ? PiecePlacement.ChooseIndexAndPlace(layout.OuterRank, randomLocations, piece, 1, numFiles - 1)
        : PiecePlacement.ChooseIndexAndPlace(layout.OuterRank, randomLocations, piece);
      order.Add(placedIndex + numFiles);
    }

    private sealed class MajorPiecePicker
    {
      private readonly Random randomPieces;
      private readonly Random randomJackPieces;
      private readonly Dictionary<PieceType, int> chosenPieces;
      private readonly int limit;
      private readonly int player;
      private readonly HashSet<string> promotionPieces;
      private readonly int numKings;
      private readonly int numJacks;
      private readonly int numNonMinorPieces;
      private readonly int majorUpgradesToBe;
      private List<PieceType> majors;
      private List<PieceType> jacks;

      public MajorPiecePicker(
        List<PieceType> majors,
        List<PieceType> jacks,
        Random randomPieces,
        Random randomJackPieces,
        Dictionary<PieceType, int> chosenPieces,
        int limit,
        int player,
        HashSet<string> promotionPieces,
        int numKings,
        int numJacks,
        int numNonMinorPieces,
        int majorUpgradesToBe)
      {
        this.majors = majors;
        this.jacks = jacks;
        this.randomPieces = randomPieces;
        this.randomJackPieces = randomJackPieces;
        this.chosenPieces = chosenPieces;
        this.limit = limit;
        this.player = player;
        this.promotionPieces = promotionPieces;
        this.numKings = numKings;
        this.numJacks = numJacks;
        this.numNonMinorPieces = numNonMinorPieces;
        this.majorUpgradesToBe = majorUpgradesToBe;
      }

      public PieceType Pick(int placementIndex, ref int spareMaterial)
      {
        if (placementIndex < numJacks + numKings)
          return PickPromotion(ref jacks, randomJackPieces, ItemGenerationValues.Jack, ref spareMaterial);
        if (placementIndex < numNonMinorPieces - majorUpgradesToBe)
          return PickPromotion(ref majors, randomPieces, ItemGenerationValues.Major, ref spareMaterial);

        randomPieces.Next();
        return null;
      }

      private PieceType PickPromotion(
        ref List<PieceType> pieces,
        Random random,
        int expectedMaterial,
        ref int spareMaterial)
      {
        PieceType piece = PieceChoice.Choose(ref pieces, random, chosenPieces, limit);
        PieceMaterialAccounting.RecordPromotion(promotionPieces, piece, player, expectedMaterial, ref spareMaterial);
        return piece;
      }
    }
  }

  internal static class PocketItemGeneration
  {
    public static List<PieceType> Generate()
    {
      int foundPockets = ApmwCore.getInstance().foundPockets;
      var pockets = ApmwConfig.getInstance().generatePocketValues(foundPockets);
      List<PieceType> pocketPieces = new List<PieceType>();
      List<PieceType> pawnOptions = PawnGeneration.SetupPawnOptions();
      for (int i = 0; i < 3; i++)
      {
        Random randomPieces = new Random(ApmwConfig.getInstance().pocketChoiceSeed[i]);
        if (pockets[i] == 0)
          pocketPieces.Add(null);
        else if (pockets[i] == 1)
          pocketPieces.Add(PawnGeneration.GetNextPawn(randomPieces, pawnOptions));
        else
        {
          // TODO(chesslogic): Try to remove very low material pieces like Gardener, unless it leaves set empty
          HashSet<PieceType> pieceSet = ApmwCore.getInstance().pocketSets[pockets[i] - 1];
          List<PieceType> pocketOptions = ArmyPieceFilter.Filter(pieceSet);
          int index = randomPieces.Next(pocketOptions.Count);
          pocketPieces.Add(pocketOptions[index]);
        }
      }
      return pocketPieces;
    }
  }

  internal static class PieceChoice
  {
    public static PieceType Choose(ref List<PieceType> pieces, Random randomPieces, Dictionary<PieceType, int> chosenPieces, int limit)
    {
      if (limit <= 0)
        return pieces[randomPieces.Next(pieces.Count)];
      int index = randomPieces.Next(pieces.Count);
      PieceType piece = pieces[index];
      if (!chosenPieces.ContainsKey(pieces[index]))
        chosenPieces[pieces[index]] = 0;
      if (++chosenPieces[pieces[index]] >= limit && pieces.Count > 1)
        pieces.RemoveAt(index);
      return piece;
    }
  }

  internal static class PiecePlacement
  {
    public static int PlaceOnBackRank(
      List<int> order,
      PieceSetLayout layout,
      Random random,
      int parity,
      int placementIndex,
      PieceType piece)
    {
      return PlaceOnBackRank(order, layout.LeftBackRank, layout.RightBackRank, random, parity, placementIndex, piece);
    }

    public static int PlaceOnBackRank(
      List<int> order,
      List<PieceType> left,
      List<PieceType> right,
      Random random,
      int parity,
      int placementIndex,
      PieceType piece)
    {
      int side;
      // The left back-rank side has one more slot than the right side.
      if (placementIndex >= right.Count * 2 || placementIndex >= left.Count * 2)
      {
        side = right.Count(item => item == null) - left.Count(item => item == null);
        parity = 0;
      }
      // if we need to choose a side, it should be random
      else if (parity == 0)
      {
        parity = random.Next(2) * 2 - 1;
        side = -parity;
      }
      // we chose the other side last time, let's go somewhere new
      else
      {
        side = parity;
        parity = 0;
      }

      if (side <= 0)
      {
        order.Add(ChooseIndexAndPlace(left, random, piece));
      }
      else
      {
        order.Add(ChooseIndexAndPlace(right, random, piece) + left.Count);
      }

      return parity;
    }

    public static int ChooseIndexAndPlace(List<PieceType> items, Random random, PieceType piece)
    {
      return ChooseIndexAndPlace(items, random, piece, 0, items.Count);
    }

    public static int ChooseIndexAndPlace(List<PieceType> items, Random random, PieceType piece, int startIndex, int endIndex)
    {
      if (items == null)
        throw new ArgumentNullException(nameof(items));
      if (random == null)
        throw new ArgumentNullException(nameof(random));
      if (startIndex < 0 || endIndex > items.Count || startIndex >= endIndex)
        throw new ArgumentOutOfRangeException(nameof(startIndex), "Placement range must be within the target list.");

      int emptyCount = items.Skip(startIndex).Take(endIndex - startIndex).Count(item => item == null);
      if (emptyCount <= 0)
        throw new InvalidOperationException(
          "No space to place piece in " + string.Join(", ", items.Select(item => item?.Notation[0])));

      var skips = random.Next(emptyCount);
      for (int index = startIndex; index < endIndex; index++)
      {
        if (items[index] != null)
          continue;
        if (skips-- > 0)
          continue;
        items[index] = piece;
        return index;
      }

      throw new InvalidOperationException(
        "No space to place piece in " + string.Join(", ", items.Select(item => item?.Notation[0])));
    }
  }

  internal static class ArmyPieceFilter
  {
    public static List<PieceType> Filter(IEnumerable<PieceType> pieces)
    {
      List<PieceType> originalPieces = pieces.ToList();
      List<int> army = ApmwConfig.getInstance().Army;
      if (army.Count == 0)
        return originalPieces;
      HashSet<PieceType> armiesPieces = new HashSet<PieceType>();
      for (int i = 0; i < army.Count; i++)
        armiesPieces = armiesPieces.Concat(ApmwCore.getInstance().armies[army[i]]).ToHashSet();
      List<PieceType> newPieces = new List<PieceType>();
      foreach (var piece in originalPieces)
        if (armiesPieces.Contains(piece))
          newPieces.Add(piece);
      return newPieces.Count > 0 ? newPieces : originalPieces;
    }
  }
}
