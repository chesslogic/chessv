using Archipelago.MultiClient.Net.Helpers;
using ChessV;
using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using static Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper;

namespace Archipelago.APChessV
{
  public class ItemHandler
  {

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
      var core = ApmwCore.getInstance();

      ApmwConfig.getInstance().seed();
      List<string> promotions = new List<string>();
      List<int> order;
      // indices 0..7 are the back rank - 8..9 can be pieces if the first rank fills up
      List<PieceType> withMajors = GenerateMajors(numFiles, out order, promotions);
      // replace some or all majors with queens
      List<PieceType> withQueens = SubstituteQueens(numFiles, withMajors, order, promotions);
      // then add minor pieces until out of space
      List<PieceType> withMinors = GenerateMinors(numFiles, withQueens, promotions);
      List<PieceType> withPawns = GeneratePawns(numFiles, withMinors);

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

    private PieceType GetNextPawn(Random randomSource)
    {
      var config = ApmwConfig.getInstance();
      var standardPawn = ApmwCore.getInstance().pawns.First(item => item.Notation[0].Equals("P"));
      var berolinaPawn = ApmwCore.getInstance().pawns.First(item => item.Notation[0].Equals("Z"));
      var checkersPawn = ApmwCore.getInstance().pawns.First(item => item.Name.Equals("Checkers"));

      switch (config.Pawns)
      {
        case FairyPawns.Mixed:
          return ApmwCore.getInstance().pawns.ElementAt(randomSource.Next(ApmwCore.getInstance().pawns.Count));
        case FairyPawns.AnyPawn:
          return randomSource.Next(2) == 0 ? standardPawn : berolinaPawn;
        case FairyPawns.AnyFairy:
          return randomSource.Next(2) == 0 ? berolinaPawn : checkersPawn;
        case FairyPawns.AnyClassical:
          return randomSource.Next(2) == 0 ? standardPawn : checkersPawn;
        case FairyPawns.Vanilla:
          return standardPawn;
        case FairyPawns.Berolina:
          return berolinaPawn;
        case FairyPawns.Checkers:
          return checkersPawn;
        default:
          return standardPawn;
      }
    }

    private void FillPawnRank(List<PieceType> targetRank, int numFiles, int startIndex, int totalChessmen, 
        Random randomPieces, Random randomLocations)
    {
      for (int i = startIndex; i < Math.Min(numFiles * (startIndex/numFiles + 1), totalChessmen); i++)
      {
        var piece = GetNextPawn(randomPieces);
        chooseIndexAndPlace(targetRank, randomLocations, piece);
      }
    }

    public List<PieceType> GeneratePawns(int numFiles, List<PieceType> minors)
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
      int totalChessmen = core.foundPawns + startingPieces;

      // Fill each rank
      FillPawnRank(pawnRank, numFiles, startingPieces, totalChessmen, randomPieces, randomLocations);
      FillPawnRank(thirdRank, numFiles, numFiles, totalChessmen, randomPieces, randomLocations);
      FillPawnRank(fourthRank, numFiles, numFiles * 2, totalChessmen, randomPieces, randomLocations);
      FillPawnRank(finalRank, numFiles, numFiles * 3, totalChessmen, randomPieces, randomLocations);

      int remainingForwardness = core.foundPawnForwardness;
      foreach ((List<PieceType>, List<PieceType>) ranks in new List<(List<PieceType>, List<PieceType>)> {
        (pawnRank, thirdRank), (thirdRank, fourthRank), (fourthRank, finalRank),
        (pawnRank, thirdRank), (thirdRank, fourthRank),
        (pawnRank, thirdRank)
      })
      {
        List<int> possibleForwardPawnPositions = new List<int>();
        for (int i = 0; i < ranks.Item2.Count; i++)
          if (ranks.Item2[i] == null && ranks.Item1[i] != null && ranks.Item1[i].Name.Contains("Pawn"))
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

    public List<PieceType> GenerateMinors(int numFiles, List<PieceType> queens, List<string> promotions)
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

      int startingPieces = ApmwCore.getInstance().foundMajors + ApmwCore.getInstance().foundConsuls;
      int totalPieces = startingPieces + ApmwCore.getInstance().foundMinors;

      int limit = ApmwConfig.getInstance().minorTypeLimit;
      int player = ApmwCore.getInstance().GeriProvider();
      int parity = left.Count((piece) => piece != null) - right.Count((piece) => piece != null);
      // this ends at 7 instead of 8 because the King occupies 1 space, thus 0..6 not 0..7
      for (int i = startingPieces; i < Math.Min(numFiles - 1, totalPieces); i++)
      {
        var piece = choosePiece(ref minors, randomPieces, chosenPieces, limit);
        promoPieces.Add(piece.Notation[player]);
        parity = placeOnBackRank(new List<int>(), left, right, randomLocations, parity, i, piece);
      }
      for (int i = Math.Max(numFiles - 1, startingPieces); i < Math.Min(numFiles * 2 - 1, totalPieces); i++)
      {
        var piece = choosePiece(ref minors, randomPieces, chosenPieces, limit);
        promoPieces.Add(piece.Notation[player]);
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

    public List<PieceType> SubstituteQueens(int numFiles, List<PieceType> majors, List<int> order, List<string> promotions)
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
        promoPieces.Add(piece.Notation[player]);
        if (order[i] < kingIndex)
          majors[order[i]] = piece;
        else
          majors[order[i] + 1] = piece;
      }
      promotions.Add(string.Join("", promoPieces));
      return majors;
    }

    public List<PieceType> GenerateMajors(int numFiles, out List<int> order, List<string> promotions)
    {
      var core = ApmwCore.getInstance();

      HashSet<string> promoPieces = new HashSet<string>();
      Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
      order = new List<int>();
      List<PieceType> majors = ApmwCore.getInstance().majors.ToList();
      majors = filterPiecesByArmy(majors);
      // Initialize lists with appropriate size based on board size
      List<PieceType> outer = Enumerable.Repeat<PieceType>(null, numFiles - 2).ToList();
      List<PieceType> left = Enumerable.Repeat<PieceType>(null, numFiles / 2).ToList();
      List<PieceType> right = Enumerable.Repeat<PieceType>(null, numFiles / 2 - 1).ToList();

      Random randomPieces = new Random(ApmwConfig.getInstance().majorSeed);
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
      int numNonMinorPieces = ApmwCore.getInstance().foundMajors + numKings;
      for (int i = numKings; i < Math.Min(numFiles - 1, numNonMinorPieces); i++)
      {
        PieceType piece = null;
        if (i < numNonMinorPieces - queensToBe)
        {
          piece = choosePiece(ref majors, randomPieces, chosenPieces, limit);
          if (piece != null)
            promoPieces.Add(piece.Notation[player]);
        }
        else
          randomPieces.Next();
        parity = placeOnBackRank(order, left, right, randomLocations, parity, i, piece);
      }
      for (int i = numFiles - 1; i < numNonMinorPieces; i++)
      {
        PieceType piece = null;
        if (i < numNonMinorPieces - queensToBe)
        {
          piece = choosePiece(ref majors, randomPieces, chosenPieces, limit);
          if (piece != null)
            promoPieces.Add(piece.Notation[player]);
        }
        else
          randomPieces.Next();
        order.Add(chooseIndexAndPlace(outer, randomLocations, piece) + 8);
      }

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
      for (int i = 0; i < 3; i++)
      {
        Random randomPieces = new Random(ApmwConfig.getInstance().pocketChoiceSeed[i]);
        if (pockets[i] == 0)
          pocketPieces.Add(null);
        else if (pockets[i] == 1)
          pocketPieces.Add(GetNextPawn(randomPieces));
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