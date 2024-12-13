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
      ApmwCore.getInstance().PlayerPieceSetProvider = () => generatePlayerPieceSet();
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

    public (Dictionary<KeyValuePair<int, int>, PieceType>, string) generatePlayerPieceSet()
    {
      var core = ApmwCore.getInstance();
      bool isGrand = core.isGrand;
      var numFiles = isGrand ? 10 : 8;

      ApmwConfig.getInstance().seed();
      List<string> promotions = new List<string>();
      List<int> order;
      // indices 0..7 are the back rank - 8..9 can be pieces if the first rank fills up
      List<PieceType> withMajors = GenerateMajors(out order, promotions);
      // replace some or all majors with queens
      List<PieceType> withQueens = SubstituteQueens(withMajors, order, promotions);
      // then add minor pieces until out of space
      List<PieceType> withMinors = GenerateMinors(withQueens, promotions);
      List<PieceType> withPawns = GeneratePawns(withMinors);

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

    public List<PieceType> GeneratePawns(List<PieceType> minors)
    {
      var core = ApmwCore.getInstance();
      bool isGrand = core.isGrand;
      var numFiles = isGrand ? 10 : 8;

      List<PieceType> pawns = ApmwCore.getInstance().pawns.ToList();
      List<PieceType> thirdRank = new List<PieceType>() { null, null, null, null, null, null, null, null };
      List<PieceType> fourthRank = new List<PieceType>() { null, null, null, null, null, null, null, null };
      List<PieceType> finalRank = new List<PieceType>() { null, null, null, null, null, null, null, null };
      List<PieceType> pawnRank = minors.Skip(numFiles).ToList();

      if (isGrand)
      {
        thirdRank = thirdRank.Prepend(null).Append(null).ToList();
        fourthRank = fourthRank.Prepend(null).Append(null).ToList();
        finalRank = finalRank.Prepend(null).Append(null).ToList();
      }

      Random randomPieces = new Random(ApmwConfig.getInstance().pawnSeed);
      Random randomLocations = new Random(ApmwConfig.getInstance().pawnLocSeed);

      int startingPieces = pawnRank.Count((item) => item != null);
      int totalChessmen = ApmwCore.getInstance().foundPawns + startingPieces;

      for (int i = startingPieces; i < Math.Min(numFiles, totalChessmen); i++)
      {
        PieceType piece;
        if (ApmwConfig.getInstance().Pawns == FairyPawns.Vanilla)
          piece = pawns.Find(item => item.Notation[0].Equals("P"));
        else if (ApmwConfig.getInstance().Pawns == FairyPawns.Berolina)
          piece = pawns.Find(item => !item.Notation[0].Equals("P"));
        else
          piece = pawns[randomPieces.Next(pawns.Count)];

        chooseIndexAndPlace(pawnRank, randomLocations, piece);
      }
      for (int i = numFiles; i < Math.Min(numFiles * 2, totalChessmen); i++)
      {
        PieceType piece;
        if (ApmwConfig.getInstance().Pawns == FairyPawns.Vanilla)
          piece = pawns.Find(item => item.Notation[0].Equals("P"));
        else if (ApmwConfig.getInstance().Pawns == FairyPawns.Berolina)
          piece = pawns.Find(item => !item.Notation[0].Equals("P"));
        else
          piece = pawns[randomPieces.Next(pawns.Count)];

        chooseIndexAndPlace(thirdRank, randomLocations, piece);
      }
      for (int i = numFiles * 2; i < Math.Min(numFiles * 3, totalChessmen); i++)
      {
        PieceType piece;
        if (ApmwConfig.getInstance().Pawns == FairyPawns.Vanilla)
          piece = pawns.Find(item => item.Notation[0].Equals("P"));
        else if (ApmwConfig.getInstance().Pawns == FairyPawns.Berolina)
          piece = pawns.Find(item => !item.Notation[0].Equals("P"));
        else
          piece = pawns[randomPieces.Next(pawns.Count)];

        chooseIndexAndPlace(fourthRank, randomLocations, piece);
      }
      for (int i = numFiles * 3; i < Math.Min(numFiles * 4, totalChessmen); i++)
      {
        PieceType piece;
        if (ApmwConfig.getInstance().Pawns == FairyPawns.Vanilla)
          piece = pawns.Find(item => item.Notation[0].Equals("P"));
        else if (ApmwConfig.getInstance().Pawns == FairyPawns.Berolina)
          piece = pawns.Find(item => !item.Notation[0].Equals("P"));
        else
          piece = pawns[randomPieces.Next(pawns.Count)];

        chooseIndexAndPlace(finalRank, randomLocations, piece);
      }

      int remainingForwardness = ApmwCore.getInstance().foundPawnForwardness;
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

    public List<PieceType> GenerateMinors(List<PieceType> queens, List<string> promotions)
    {
      var core = ApmwCore.getInstance();
      bool isGrand = core.isGrand;
      var numFiles = isGrand ? 10 : 8;

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

    public List<PieceType> SubstituteQueens(List<PieceType> majors, List<int> order, List<string> promotions)
    {
      var core = ApmwCore.getInstance();
      bool isGrand = core.isGrand;
      var numFiles = isGrand ? 10 : 8;

      /*
      IEnumerable<int> majorIndexes =
        majors.Select((major, index) => major != null ? index : -1)
            .Where(index => index != -1);
      */

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

    public List<PieceType> GenerateMajors(out List<int> order, List<string> promotions)
    {
      var core = ApmwCore.getInstance();
      bool isGrand = core.isGrand;
      var numFiles = isGrand ? 10 : 8;

      HashSet<string> promoPieces = new HashSet<string>();
      Dictionary<PieceType, int> chosenPieces = new Dictionary<PieceType, int>();
      order = new List<int>();
      List<PieceType> majors = ApmwCore.getInstance().majors.ToList();
      majors = filterPiecesByArmy(majors);
      List<PieceType> outer = new List<PieceType>() { null, null, null, null, null, null };
      List<PieceType> left = new List<PieceType>() { null, null, null, null };
      List<PieceType> right = new List<PieceType>() { null, null, null };

      if (isGrand)
      {
        left = left.Append(null).ToList();
        right = right.Append(null).ToList();
        outer = outer.Append(null).Append(null).ToList();
      }

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
        left[numFiles / 2 - 1] = kings[0];
        if (numKings > 1)
        {
          right[0] = kings[0];
          // TODO(chesslogic): I think this was fixed when I started counting null spaces
          // parity = -1; // pick left side first!
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
        if (pockets[i] == 0)
        {
          pocketPieces.Add(null);
          continue;
        }
        HashSet<PieceType> setOfPieceType = ApmwCore.getInstance().pocketSets[pockets[i] - 1];
        List<PieceType> listOfPieceType;
        if (pockets[i] == 1)
        {
          listOfPieceType = new List<PieceType>();
          List<PieceType> pawns = ApmwCore.getInstance().pawns.ToList();
          if (ApmwConfig.getInstance().Pawns != FairyPawns.Berolina)
            listOfPieceType.Add(pawns.Find(item => item.Notation[0].Equals("P")));
          if (ApmwConfig.getInstance().Pawns != FairyPawns.Vanilla)
            listOfPieceType.Add(pawns.Find(item => !item.Notation[0].Equals("P")));
        }
        else
          listOfPieceType = filterPiecesByArmy(setOfPieceType);

        Random random = new Random(ApmwConfig.getInstance().pocketChoiceSeed[i]);
        int index = random.Next(listOfPieceType.Count);
        pocketPieces.Add(listOfPieceType[index]);
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