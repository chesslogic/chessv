using Archipelago.MultiClient.Net.Helpers;
using ChessV;
using ChessV.Base;
using ChessV.Games.Pieces.Apmw;
using ChessV.Games.Pieces.Berolina;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using static Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper;

namespace Archipelago.APChessV
{
  public class ItemHandler
  {
    private static readonly int MAJOR_VALUE = 485;
    private static readonly int MINOR_VALUE = 300;
    private static readonly int PAWN_VALUE = 100;
    private static readonly int WEAK_VALUE = 75;
    private static readonly int JACK_VALUE = 700;
    private static readonly int QUEEN_VALUE = 900;

    public ItemHandler(IReceivedItemsHelper receivedItemsHelper)
    {
      ReceivedItemsHelper = receivedItemsHelper;

      irHandler = (helper) => this.Hook();
      ReceivedItemsHelper.ItemReceived += irHandler;
      this.Hook();

      // overwrite global state
      ApmwCore.getInstance().PlayerPieceSetProvider = (numFiles) => generatePlayerPieceSet(numFiles);
      ApmwCore.getInstance().PlayerPocketPiecesProvider = () => generatePocketItems();
    }

    private IReceivedItemsHelper ReceivedItemsHelper;
    private ItemReceivedHandler irHandler;

    public void Hook()
    {
      var items = ReceivedItemsHelper.AllItemsReceived;

      var core = ApmwCore.getInstance();

      int Count(string name) => items.Count(
        item => ReceivedItemsHelper.GetItemName(item.ItemId, ApmwConstants.TrackerName) == name);
      bool Any(string name) => items.Any(
        item => ReceivedItemsHelper.GetItemName(item.ItemId, ApmwConstants.TrackerName) == name);

      try
      {
        core.foundPockets = Count(ApmwConstants.ProgressiveItems.Pocket);
      } catch (Exception e)
      {
        ArchipelagoClient.getInstance().nonSessionMessages.Add(e.ToString());
      }
      core.foundPocketRange = Math.Min(6, Count(ApmwConstants.ProgressiveItems.PocketRange));
      core.foundPocketGems = Count(ApmwConstants.ProgressiveItems.PocketGems);
      core.GeriProvider = () => Any(ApmwConstants.ProgressiveItems.PlayAsWhite) ? 0 : 1;
      core.EngineWeakeningProvider = () => Math.Min(5, Count(ApmwConstants.ProgressiveItems.AIIntelligenceMalus));
      core.foundPockets = Math.Min(12, Count(ApmwConstants.ProgressiveItems.Pocket));
      core.foundPawns = Count(ApmwConstants.ProgressiveItems.Pawn);
      core.foundMinors = Count(ApmwConstants.ProgressiveItems.MinorPiece);
      core.foundMajors = Count(ApmwConstants.ProgressiveItems.MajorPiece);
      core.foundJacks = Count(ApmwConstants.ProgressiveItems.Jack);
      core.foundQueens = Count(ApmwConstants.ProgressiveItems.MajorToQueen);
      core.foundPawnForwardness = Count(ApmwConstants.ProgressiveItems.PawnForwardness);
      core.foundConsuls = Math.Min(2, Count(ApmwConstants.ProgressiveItems.Consul));
      core.foundKingPromotions = Math.Min(2, Count(ApmwConstants.ProgressiveItems.KingPromotion));
      core.isGrand = Any(ApmwConstants.ProgressiveItems.SuperSizeMe);
    }

    public void Unhook()
    {
      ReceivedItemsHelper.ItemReceived -= irHandler;
    }

    ///////////////////////
    /// GENERATE PIECES ///
    ///////////////////////

    public (Dictionary<KeyValuePair<int, int>, PieceType>, string) generatePlayerPieceSet(int numFiles)
    {
      ApmwCore core = ApmwCore.getInstance();

      int spare_material = 0;

      ApmwConfig.getInstance().seed();
      List<string> promotions = new List<string>();
      List<int> order;
      // indices 0..7 are the back rank - 8..9 can be pieces if the first rank fills up
      List<PieceType> withMajors = GenerateMajors(numFiles, out order, promotions, ref spare_material);
      // replace some or all majors with queens
      List<PieceType> withQueens = SubstituteQueens(numFiles, withMajors, order, promotions, ref spare_material);
      // then add minor pieces until out of space
      List<PieceType> withMinors = GenerateMinors(numFiles, withQueens, promotions, ref spare_material);
      List<PieceType> withPawns = GeneratePawns(numFiles, withMinors, spare_material);

      Dictionary<KeyValuePair<int, int>, PieceType> pieces = new Dictionary<KeyValuePair<int, int>, PieceType>();
      for (int i = 0; i < 5; i++)
        for (int j = 0; j < numFiles; j++)
          if (withPawns[i * numFiles + j] != null)
            pieces.Add(new KeyValuePair<int, int>(4 - i, j), withPawns[i * numFiles + j]);

      return (pieces, String.Join("", promotions));
    }

    ///////////////////////
    /// GENERATE PIECES ///
    ///////////////////////

    private List<PieceType> setupPawnOptions()
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

    enum PawnUpgrade
    {
      Core,
      Min,
      Best,
      Sergeant
    }

    private PieceType GetNextPawn(Random randomSource, List<PieceType> options, PawnUpgrade upgrade = PawnUpgrade.Core)
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
          limited = options.Where(item => item.MidgameValue >= WEAK_VALUE).ToList();
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

    private void FillPawnRank(List<PieceType> targetRank, int numFiles, int startIndex, Queue<PieceType> adjustedPawns, 
        Random randomLocations)
    {
      for (int i = startIndex;
        i < numFiles * (startIndex/numFiles + 1)
          && adjustedPawns.Count > 0
          && targetRank.Where(item => item == null).Count() > 0;
        i++)
      {
        var piece = adjustedPawns.Dequeue();
        chooseIndexAndPlace(targetRank, randomLocations, piece);
      }
    }

    internal List<PieceType> PickPawns(Random randomPieces, int adjustedPawnValues, int remainingPawnSpaces, int foundPawns)
    {
      return PickPawns(randomPieces, setupPawnOptions(), adjustedPawnValues, remainingPawnSpaces, foundPawns);
    }

    internal List<PieceType> PickPawns(Random randomPieces, List<PieceType> pawnOptions, int adjustedPawnValues, int remainingPawnSpaces, int foundPawns)
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
            picked = PickPawnMaxMode(randomPieces, pawnOptions, sergeants, adjustedPawnValues, foundPawns, workingPawns.Count);
            break;
          case FairyPawnUpgrades.Off:
          default:
          {
            var upgrade = adjustedPawnValues <= PAWN_VALUE ? PawnUpgrade.Min : PawnUpgrade.Core;
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

    private PieceType PickPawnPoolMode(Random randomPieces, List<PieceType> pawnOptions, List<PieceType> sergeants,
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

    private PieceType PickPawnMaxMode(Random randomPieces, List<PieceType> pawnOptions, List<PieceType> sergeants,
        int budget, int foundPawns, int currentCount)
    {
      var sergeant = GetNextPawn(randomPieces, pawnOptions, PawnUpgrade.Sergeant);
      bool allowed = currentCount >= foundPawns
        ? budget >= sergeant.MidgameValue
        : PigeonholeAllowsSergeant(budget, foundPawns, currentCount, sergeant.MidgameValue, pawnOptions);
      if (allowed) return sergeant;
      return GetNextPawn(randomPieces, pawnOptions, FallbackUpgrade(budget, foundPawns, currentCount));
    }

    // Slot-aware fallback selector: if remaining budget per still-required slot won't
    // afford a PAWN_VALUE piece, force the cheapest option so the count guarantee holds.
    private PawnUpgrade FallbackUpgrade(int budget, int foundPawns, int currentCount)
    {
      if (budget <= PAWN_VALUE) return PawnUpgrade.Min;
      int slotsLeft = Math.Max(1, foundPawns - currentCount);
      if (currentCount < foundPawns && budget / slotsLeft <= PAWN_VALUE) return PawnUpgrade.Min;
      return PawnUpgrade.Core;
    }

    private bool PigeonholeAllowsSergeant(
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

    private void UpgradePawns(Random randomPieces, int adjustedPawnValues, List<PieceType> pawnOptions,
        List<PieceType> workingPawns, FairyPawnUpgrades mode)
    {
      // Best-upgrade pass: replace the cheapest sub-WEAK_VALUE pieces with Best-tier alternatives.
      var miniIndexes = new Queue<int>(workingPawns.Select((item, index) => new { Piece = item, Index = index })
        .Where(item => item.Piece.MidgameValue < WEAK_VALUE)
        .OrderBy(item => item.Piece.MidgameValue)
        .Select(item => item.Index));
      while (adjustedPawnValues > 0 && miniIndexes.Count > 0)
      {
        var index = miniIndexes.Dequeue();
        adjustedPawnValues += workingPawns[index].MidgameValue;
        workingPawns[index] = GetNextPawn(randomPieces, pawnOptions, PawnUpgrade.Best);
        adjustedPawnValues -= workingPawns[index].MidgameValue;
      }
      // Pool/Max already placed sergeants per-slot in PickPawns; only Off uses the legacy fallback.
      if (mode != FairyPawnUpgrades.Off) return;
      UpgradeRemainingPawnsToSergeants(randomPieces, adjustedPawnValues, pawnOptions, workingPawns);
    }

    private void UpgradeRemainingPawnsToSergeants(Random randomPieces, int adjustedPawnValues, List<PieceType> pawnOptions, List<PieceType> workingPawns)
    {
      var sergeantIndexes = new Queue<int>(workingPawns.Select((item, index) => new { Piece = item, Index = index })
        .OrderBy(item => item.Piece.MidgameValue)
        .Select(item => item.Index));
      while (adjustedPawnValues > 0 && sergeantIndexes.Count > 0)
      {
        // Find the indexes of the lowest value piece that can be upgraded
        var index = sergeantIndexes.Dequeue();
        // Remove that piece, returning its value to the pool
        adjustedPawnValues += workingPawns[index].MidgameValue;
        // Replace it with a higher-value piece
        workingPawns[index] = GetNextPawn(randomPieces, pawnOptions, PawnUpgrade.Sergeant);
        // Subtract the new piece's value from the pool
        adjustedPawnValues -= workingPawns[index].MidgameValue;
      }
    }

    public List<PieceType> GeneratePawns(int numFiles, List<PieceType> minors, int spare_material)
    {
      var core = ApmwCore.getInstance();
      List<PieceType> pawns = core.pawns.ToList();
      List<PieceType> thirdRank = Enumerable.Repeat<PieceType>(null, numFiles).ToList();
      List<PieceType> fourthRank = Enumerable.Repeat<PieceType>(null, numFiles).ToList();
      List<PieceType> finalRank = Enumerable.Repeat<PieceType>(null, numFiles).ToList();
      List<PieceType> pawnRank = minors.Skip(numFiles).ToList();

      Random randomPieces = new Random(ApmwConfig.getInstance().pawnSeed);
      Random randomLocations = new Random(ApmwConfig.getInstance().pawnLocSeed);

      int startingPieces = pawnRank.Count((item) => item != null);
      int remainingPawnSpaces = 4 * numFiles - startingPieces;
      int totalChessmen = core.foundPawns + startingPieces;

      int adjustedPawnValues = Math.Max(
        core.foundPawns * PAWN_VALUE,
        core.foundPawns * PAWN_VALUE + spare_material + 45);

      List<PieceType> workingPawns = PickPawns(randomPieces, adjustedPawnValues, remainingPawnSpaces, core.foundPawns);

      Queue<PieceType> adjustedPawns = new Queue<PieceType>(workingPawns);
      // Fill each rank
      FillPawnRank(pawnRank, numFiles, startingPieces, adjustedPawns, randomLocations);
      FillPawnRank(thirdRank, numFiles, numFiles, adjustedPawns, randomLocations);
      FillPawnRank(fourthRank, numFiles, numFiles * 2, adjustedPawns, randomLocations);
      FillPawnRank(finalRank, numFiles, numFiles * 3, adjustedPawns, randomLocations);

      int remainingForwardness = core.foundPawnForwardness;
      foreach ((List<PieceType>, List<PieceType>) ranks in new List<(List<PieceType>, List<PieceType>)> {
        (pawnRank, thirdRank), (thirdRank, fourthRank), (fourthRank, finalRank),
        (pawnRank, thirdRank), (thirdRank, fourthRank),
        (pawnRank, thirdRank)
      })
      {
        List<int> possibleForwardPawnPositions = new List<int>();
        for (int i = 0; i < ranks.Item2.Count; i++)
          if (ranks.Item2[i] == null && ranks.Item1[i] != null && ranks.Item1[i].IsPawn)
            possibleForwardPawnPositions.Add(i);
        for (
          int i = randomLocations.Next(possibleForwardPawnPositions.Count);
          remainingForwardness-- > 0 && possibleForwardPawnPositions.Count > 0;
          i = randomLocations.Next(possibleForwardPawnPositions.Count))
        {
          // swap backward with forward
          ranks.Item2[possibleForwardPawnPositions[i]] = ranks.Item1[possibleForwardPawnPositions[i]];
          ranks.Item1[possibleForwardPawnPositions[i]] = null;
          // setup for next iteration (TODO(chesslogic): could move this into the for loop syntax)
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

    public List<PieceType> GenerateMinors(int numFiles, List<PieceType> queens, List<string> promotions, ref int spare_material)
    {
      var core = ApmwCore.getInstance();

      HashSet<string> promoPieces = new HashSet<string>();
      Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
      List<PieceType> minors = ApmwCore.getInstance().minors.ToList();
      minors = filterPiecesByArmy(minors);
      List<PieceType> outer = queens.Skip(numFiles).Take(numFiles).ToList();
      List<PieceType> left = queens.Take(numFiles / 2).ToList();
      List<PieceType> right = queens.Skip(numFiles / 2 + 1).Take(numFiles / 2 - 1).ToList();

      Random randomPieces = new Random(ApmwConfig.getInstance().minorSeed);
      Random randomLocations = new Random(ApmwConfig.getInstance().minorLocSeed);

      int limit = ApmwConfig.getInstance().minorTypeLimit;
      int player = ApmwCore.getInstance().GeriProvider();
      int parity = left.Count((piece) => piece != null) - right.Count((piece) => piece != null);
      int backRankPieces = left.Count(piece => piece != null) + right.Count(piece => piece != null);
      int availableBackRankSpaces = left.Count(piece => piece == null) + right.Count(piece => piece == null);
      int availableOuterSpaces = outer.Count(piece => piece == null);
      int minorsToPlace = Math.Min(core.foundMinors, availableBackRankSpaces + availableOuterSpaces);
      int backRankMinorsToPlace = Math.Min(availableBackRankSpaces, minorsToPlace);

      for (int i = 0; i < backRankMinorsToPlace; i++)
      {
        var piece = choosePiece(ref minors, randomPieces, chosenPieces, limit);
        if (piece != null)
        {
          promoPieces.Add(piece.Notation[player]);
          spare_material += MINOR_VALUE - piece.MidgameValue; // Track difference from expected minor value
        }
        parity = placeOnBackRank(new List<int>(), left, right, randomLocations, parity, backRankPieces + i, piece);
      }
      for (int i = backRankMinorsToPlace; i < minorsToPlace; i++)
      {
        var piece = choosePiece(ref minors, randomPieces, chosenPieces, limit);
        if (piece != null)
        {
          promoPieces.Add(piece.Notation[player]);
          spare_material += MINOR_VALUE - piece.MidgameValue; // Track difference from expected minor value
        }
        chooseIndexAndPlace(outer, randomLocations, piece);
      }
      spare_material += Math.Max(0, core.foundMinors - minorsToPlace) * MINOR_VALUE;

      List<PieceType> output = new List<PieceType>();
      output.AddRange(left);
      output.Add(queens[numFiles / 2]);
      output.AddRange(right);
      output.AddRange(outer);
      promotions.Add(string.Join("", promoPieces));

      return output;
    }

    public List<PieceType> SubstituteQueens(int numFiles, List<PieceType> majors, List<int> order, List<string> promotions, ref int spare_material)
    {
      HashSet<string> promoPieces = new HashSet<string>();
      Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
      List<PieceType> queens = ApmwCore.getInstance().queens.ToList();
      queens = filterPiecesByArmy(queens);

      Random random = new Random(ApmwConfig.getInstance().queenSeed);

      int limit = ApmwConfig.getInstance().queenTypeLimit;
      int player = ApmwCore.getInstance().GeriProvider();
      int numQueens = ApmwCore.getInstance().foundQueens;
      int remainingMajors = order.Count - numQueens;
      for (int i = order.Count - 1; i >= remainingMajors && i >= 0; i--)
      {
        var piece = choosePiece(ref queens, random, chosenPieces, limit);
        if (piece != null)
        {
          promoPieces.Add(piece.Notation[player]);
          spare_material += QUEEN_VALUE - piece.MidgameValue; // Track difference from expected queen value
          majors[MajorOrderIndexToPieceSetIndex(numFiles, order[i], majors.Count)] = piece;
        }
      }
      promotions.Add(string.Join("", promoPieces));
      return majors;
    }

    private static int MajorOrderIndexToPieceSetIndex(int numFiles, int orderIndex, int pieceSetCount)
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

    public List<PieceType> GenerateMajors(int numFiles, out List<int> order, List<string> promotions, ref int spare_material)
    {
      var core = ApmwCore.getInstance();

      HashSet<string> promoPieces = new HashSet<string>();
      Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
      order = new List<int>();
      List<PieceType> majors = ApmwCore.getInstance().majors.ToList();
      List<PieceType> jacks = ApmwCore.getInstance().jacks.ToList();
      majors = filterPiecesByArmy(majors);
      jacks = filterPiecesByArmy(jacks);
      // Initialize lists with appropriate size based on board size
      List<PieceType> outer = Enumerable.Repeat<PieceType>(null, numFiles).ToList();
      List<PieceType> left = Enumerable.Repeat<PieceType>(null, numFiles / 2).ToList();
      List<PieceType> right = Enumerable.Repeat<PieceType>(null, numFiles / 2 - 1).ToList();

      Random randomPieces = new Random(ApmwConfig.getInstance().majorSeed);
      Random randomJackPieces = new Random(ApmwConfig.getInstance().majorSeed);
      Random randomLocations = new Random(ApmwConfig.getInstance().majorLocSeed);

      int limit = ApmwConfig.getInstance().majorTypeLimit;
      int queensToBe = ApmwCore.getInstance().foundQueens;
      int player = ApmwCore.getInstance().GeriProvider();
      int parity = 0;

      int numKings = ApmwCore.getInstance().foundConsuls;
      if (numKings > 0)
      {
        List<PieceType> kings = ApmwCore.getInstance().kings;
        // Center the king on D file for 8x8 or E file for 10x10
        int centerFile = (numFiles / 2) - 1;
        left[centerFile] = kings[0];
        if (numKings > 1)
        {
          // Place second king on E file for 8x8 or F file for 10x10
          right[0] = kings[0];
        }
      }

      // this ends at 7 instead of 8 because the King always occupies 1 space, thus 0..6 not 0..7
      int numJacks = ApmwCore.getInstance().foundJacks;
      int numNonMinorPieces = ApmwCore.getInstance().foundMajors + numKings + numJacks;
      int backRankCapacity = numFiles - 1;
      int outerRankCapacity = numFiles;
      int placementCapacity = backRankCapacity + outerRankCapacity;
      for (int i = numKings; i < Math.Min(backRankCapacity, numNonMinorPieces); i++)
      {
        PieceType piece = null;
        if (i < numJacks + numKings)
        {
          piece = choosePiece(ref jacks, randomJackPieces, chosenPieces, limit);
          promoPieces.Add(piece.Notation[player]);
          spare_material += JACK_VALUE - piece.MidgameValue; // Track difference from expected major value
        }
        else if (i < numNonMinorPieces - queensToBe)
        {
          piece = choosePiece(ref majors, randomPieces, chosenPieces, limit);
          if (piece != null)
          {
            promoPieces.Add(piece.Notation[player]);
            spare_material += MAJOR_VALUE - piece.MidgameValue; // Track difference from expected major value
          }
        }
        else
          randomPieces.Next();
        parity = placeOnBackRank(order, left, right, randomLocations, parity, i, piece);
      }
      for (int i = backRankCapacity; i < Math.Min(placementCapacity, numNonMinorPieces); i++)
      {
        PieceType piece = null;
        if (i < numJacks + numKings)
        {
          piece = choosePiece(ref jacks, randomJackPieces, chosenPieces, limit);
          promoPieces.Add(piece.Notation[player]);
          spare_material += JACK_VALUE - piece.MidgameValue; // Track difference from expected major value
        }
        else if (i < numNonMinorPieces - queensToBe)
        {
          piece = choosePiece(ref majors, randomPieces, chosenPieces, limit);
          if (piece != null)
          {
            promoPieces.Add(piece.Notation[player]);
            spare_material += MAJOR_VALUE - piece.MidgameValue; // Track difference from expected major value
          }
        }
        else
          randomPieces.Next();
        bool innerSpillSpaceAvailable = outer.Skip(1).Take(numFiles - 2).Any(item => item == null);
        int placedIndex = innerSpillSpaceAvailable
          ? chooseIndexAndPlace(outer, randomLocations, piece, 1, numFiles - 1)
          : chooseIndexAndPlace(outer, randomLocations, piece);
        order.Add(placedIndex + numFiles);
      }
      spare_material += Math.Max(0, numNonMinorPieces - placementCapacity) * MAJOR_VALUE;

      List<PieceType> output = new List<PieceType>();
      output.AddRange(left);
      output.Add(ApmwCore.getInstance().kings[ApmwCore.getInstance().foundKingPromotions]);
      output.AddRange(right);
      output.AddRange(outer);
      promotions.Add(string.Join("", promoPieces));

      return output;
    }

    private PieceType choosePiece(ref List<PieceType> pieces, Random randomPieces, Dictionary<PieceType, int> chosenPieces, int limit)
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

    private static int placeOnBackRank(
      List<int> order,
      List<PieceType> left,
      List<PieceType> right,
      Random random,
      int parity,
      int i,
      PieceType piece)
    {
      int side;
      // there are 4 spaces on the left (queenside) vs 3 on right (kingside)
      if (i >= right.Count * 2 || i >= left.Count * 2)
      {
        side = right.Count(item => item == null) - left.Count(item => item == null); // 3 - 4 = -1
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
        order.Add(chooseIndexAndPlace(left, random, piece));
      }
      else
      {
        order.Add(chooseIndexAndPlace(right, random, piece) + left.Count); // left.Count == 4
      }

      return parity;
    }

    private static int chooseIndexAndPlace(List<PieceType> items, Random random, PieceType piece)
    {
      return chooseIndexAndPlace(items, random, piece, 0, items.Count);
    }

    private static int chooseIndexAndPlace(List<PieceType> items, Random random, PieceType piece, int startIndex, int endIndex)
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

    public List<PieceType> generatePocketItems()
    {
      int foundPockets = ApmwCore.getInstance().foundPockets;
      var pockets = ApmwConfig.getInstance().generatePocketValues(foundPockets);
      List<PieceType> pocketPieces = new List<PieceType>();
      List<PieceType> pawnOptions = setupPawnOptions();
      for (int i = 0; i < 3; i++)
      {
        Random randomPieces = new Random(ApmwConfig.getInstance().pocketChoiceSeed[i]);
        if (pockets[i] == 0)
          pocketPieces.Add(null);
        else if (pockets[i] == 1)
          pocketPieces.Add(GetNextPawn(randomPieces, pawnOptions));
        else
        {
          // TODO(chesslogic): Try to remove very low material pieces like Gardener, unless it leaves set empty
          HashSet<PieceType> setOfPieceType = ApmwCore.getInstance().pocketSets[pockets[i] - 1];
          List<PieceType> listOfPieceType = filterPiecesByArmy(setOfPieceType);
          int index = randomPieces.Next(listOfPieceType.Count);
          pocketPieces.Add(listOfPieceType[index]);
        }
      }
      return pocketPieces;
    }

    private List<PieceType> filterPiecesByArmy(IEnumerable<PieceType> pieces)
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