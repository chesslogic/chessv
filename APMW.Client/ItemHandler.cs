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

      try
      {
        core.foundPockets = items.Count(
          (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive Pocket");
      } catch (Exception e)
      {
        ArchipelagoClient.getInstance().nonSessionMessages.Add(e.ToString());
      }
      core.foundPocketRange = Math.Min(6, items.Count(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive Pocket Range"));
      core.foundPocketGems = items.Count(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive Pocket Gems");
      core.GeriProvider = () => items.Any(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Play as White") ? 0 : 1;
      core.EngineWeakeningProvider = () => Math.Min(5, items.Count(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive Engine ELO Lobotomy"));
      core.foundPockets = Math.Min(12, items.Count(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive Pocket"));
      core.foundPawns = items.Count(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive Pawn");
      core.foundMinors = items.Count(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive Minor Piece");
      core.foundMajors = items.Count(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive Major Piece");
      core.foundJacks = items.Count(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive Jack");
      core.foundQueens = items.Count(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive Major To Queen");
      core.foundPawnForwardness = items.Count(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive Pawn Forwardness");
      core.foundConsuls = Math.Min(2, items.Count(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive Consul"));
      core.foundKingPromotions = Math.Min(2, items.Count(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Progressive King Promotion"));
      core.isGrand = items.Any(
        (item) => ReceivedItemsHelper.GetItemName(item.ItemId, "ChecksMate") == "Super-Size Me");
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

    private List<PieceType> PickPawns(Random randomPieces, int adjustedPawnValues, int remainingPawnSpaces)
    {
      List<PieceType> pawnOptions = setupPawnOptions();
      // Add more pawns until we have enough
      List<PieceType> workingPawns = new List<PieceType>();
      while (workingPawns.Count < remainingPawnSpaces && adjustedPawnValues > 0)
      {
        var upgrade = adjustedPawnValues <= PAWN_VALUE ? PawnUpgrade.Min : PawnUpgrade.Core;
        workingPawns.Add(GetNextPawn(randomPieces, pawnOptions, upgrade));
        adjustedPawnValues -= workingPawns.Last().MidgameValue;
      }
      // TODO(chesslogic): Add an Option not to upgrade pawns. For now, we'll always maximize material value.
      UpgradePawns(randomPieces, adjustedPawnValues, pawnOptions, workingPawns);
      return workingPawns;
    }

    private void UpgradePawns(Random randomPieces, int adjustedPawnValues, List<PieceType> pawnOptions, List<PieceType> workingPawns)
    {
      // Find the lowest value piece that can be upgraded, then replace it with a higher-value piece
      var miniIndexes = new Queue<int>(workingPawns.Select((item, index) => new { Piece = item, Index = index })
        .Where(item => item.Piece.MidgameValue < WEAK_VALUE)
        .OrderBy(item => item.Piece.MidgameValue)
        .Select(item => item.Index));
      while (adjustedPawnValues > 0 && miniIndexes.Count > 0)
      {
        // Find the indexes of the lowest value piece that can be upgraded
        var index = miniIndexes.Dequeue();
        // Remove that piece, returning its value to the pool
        adjustedPawnValues += workingPawns[index].MidgameValue;
        // Replace it with a higher-value piece
        workingPawns[index] = GetNextPawn(randomPieces, pawnOptions, PawnUpgrade.Best);
        // Subtract the new piece's value from the pool
        adjustedPawnValues -= workingPawns[index].MidgameValue;
      }
      // If we still have value to distribute, start upgrading pawns to sergeants or minors
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

      List<PieceType> workingPawns = PickPawns(randomPieces, adjustedPawnValues, remainingPawnSpaces);

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
      List<PieceType> outer = queens.Skip(numFiles).Take(numFiles - 2).ToList();
      List<PieceType> left = queens.Take(numFiles / 2).ToList();
      List<PieceType> right = queens.Skip(numFiles / 2 + 1).Take(numFiles / 2 - 1).ToList();
      // full row: 1 empty space, then 6 potential major pieces, then 1 empty space
      outer = outer.Prepend(null).Append(null).ToList();

      Random randomPieces = new Random(ApmwConfig.getInstance().minorSeed);
      Random randomLocations = new Random(ApmwConfig.getInstance().minorLocSeed);

      int startingPieces =
        ApmwCore.getInstance().foundMajors
        + ApmwCore.getInstance().foundConsuls
        + ApmwCore.getInstance().foundJacks;
      int totalPieces = startingPieces + ApmwCore.getInstance().foundMinors;

      int limit = ApmwConfig.getInstance().minorTypeLimit;
      int player = ApmwCore.getInstance().GeriProvider();
      int parity = left.Count((piece) => piece != null) - right.Count((piece) => piece != null);
      // this ends at 7 instead of 8 because the King occupies 1 space, thus 0..6 not 0..7
      for (int i = startingPieces; i < Math.Min(numFiles - 1, totalPieces); i++)
      {
        var piece = choosePiece(ref minors, randomPieces, chosenPieces, limit);
        if (piece != null)
        {
          promoPieces.Add(piece.Notation[player]);
          spare_material += MINOR_VALUE - piece.MidgameValue; // Track difference from expected minor value
        }
        parity = placeOnBackRank(new List<int>(), left, right, randomLocations, parity, i, piece);
      }
      for (int i = Math.Max(numFiles - 1, startingPieces); i < Math.Min(numFiles * 2 - 1, totalPieces); i++)
      {
        var piece = choosePiece(ref minors, randomPieces, chosenPieces, limit);
        if (piece != null)
        {
          promoPieces.Add(piece.Notation[player]);
          spare_material += MINOR_VALUE - piece.MidgameValue; // Track difference from expected minor value
        }
        chooseIndexAndPlace(outer, randomLocations, piece);
      }

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
      var core = ApmwCore.getInstance();

      HashSet<string> promoPieces = new HashSet<string>();
      Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
      List<PieceType> queens = ApmwCore.getInstance().queens.ToList();
      queens = filterPiecesByArmy(queens);
      int kingIndex = numFiles / 2; // king is on E file

      Random random = new Random(ApmwConfig.getInstance().queenSeed);

      int limit = ApmwConfig.getInstance().queenTypeLimit;
      int player = ApmwCore.getInstance().GeriProvider();
      int numKings = ApmwCore.getInstance().foundConsuls;
      int numQueens = ApmwCore.getInstance().foundQueens;
      int remainingMajors = order.Count - numQueens;
      for (int i = order.Count - 1; i >= remainingMajors && i >= 0; i--)
      {
        var piece = choosePiece(ref queens, random, chosenPieces, limit);
        if (piece != null)
        {
          promoPieces.Add(piece.Notation[player]);
          spare_material += QUEEN_VALUE - piece.MidgameValue; // Track difference from expected queen value
          if (order[i] < kingIndex)
            majors[order[i]] = piece;
          else
            majors[order[i] + 1] = piece;
        }
      }
      promotions.Add(string.Join("", promoPieces));
      return majors;
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
      List<PieceType> outer = Enumerable.Repeat<PieceType>(null, numFiles - 2).ToList();
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
      for (int i = numKings; i < Math.Min(numFiles - 1, numNonMinorPieces); i++)
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
      for (int i = numFiles - 1; i < Math.Min(numFiles * 2 - 1, numNonMinorPieces); i++)
      {
        PieceType piece = null;
        if (i < numJacks)
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
        order.Add(chooseIndexAndPlace(outer, randomLocations, piece) + 8);
      }
      spare_material += Math.Max(0, numNonMinorPieces - numFiles * 2) * MAJOR_VALUE;

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
      var index = 0;
      var skips = random.Next(items.Count(item => item == null));
      while (items[index] != null || skips > 0)
      {
        if (items[index] == null)
          skips--;
        index++;
      }
      items[index] = piece;
      return index;
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
      List<PieceType> newPieces = new List<PieceType>();
      List<int> army = ApmwConfig.getInstance().Army;
      if (army.Count == 0)
        return pieces.ToList();
      HashSet<PieceType> armiesPieces = new HashSet<PieceType>();
      for (int i = 0; i < army.Count; i++)
        armiesPieces = armiesPieces.Concat(ApmwCore.getInstance().armies[army[i]]).ToHashSet();
      foreach (var piece in pieces)
        if (armiesPieces.Contains(piece))
          newPieces.Add(piece);
      return newPieces;
    }
  }
}