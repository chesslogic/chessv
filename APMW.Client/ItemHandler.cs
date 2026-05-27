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
      isHooked = true;
      this.Hook();

      // Save original providers before overwriting global state
      ApmwCore core = ApmwCore.getInstance();
      originalPlayerPieceSetProvider = core.PlayerPieceSetProvider;
      originalPlayerPocketPiecesProvider = core.PlayerPocketPiecesProvider;

      // overwrite global state
      core.PlayerPieceSetProvider = (numFiles) => GeneratePlayerPieceSet(numFiles);
      core.PlayerPocketPiecesProvider = () => GeneratePocketItems();
    }

    private readonly IReceivedItemsHelper ReceivedItemsHelper;
    private readonly ItemReceivedHandler irHandler;
    private bool isHooked;
    private Func<int, (Dictionary<KeyValuePair<int, int>, PieceType>, string)> originalPlayerPieceSetProvider;
    private Func<List<PieceType>> originalPlayerPocketPiecesProvider;

    public void Hook()
    {
      ItemProgressSnapshot progress = ItemProgressSnapshot.FromReceivedItems(ReceivedItemsHelper);
      ApplyProgressSnapshot(ApmwCore.getInstance(), progress);
    }

    private static void ApplyProgressSnapshot(ApmwCore core, ItemProgressSnapshot progress)
    {
      core.foundPocketRange = progress.FoundPocketRange;
      core.foundPocketGems = progress.FoundPocketGems;
      core.GeriProvider = () => progress.GeriProviderPlayer;
      core.EngineWeakeningProvider = () => progress.EngineWeakening;
      core.foundPockets = progress.FoundPockets;
      core.foundPawns = progress.FoundPawns;
      core.foundMinors = progress.FoundMinors;
      core.foundMajors = progress.FoundMajors;
      core.foundJacks = progress.FoundJacks;
      core.foundQueens = progress.FoundQueens;
      core.foundPawnForwardness = progress.FoundPawnForwardness;
      core.foundConsuls = progress.FoundConsuls;
      core.foundKingPromotions = progress.FoundKingPromotions;
      core.isGrand = progress.IsGrand;
    }

    private sealed class ItemProgressSnapshot
    {
      public int FoundPocketRange { get; }
      public int FoundPocketGems { get; }
      public int GeriProviderPlayer { get; }
      public int EngineWeakening { get; }
      public int FoundPockets { get; }
      public int FoundPawns { get; }
      public int FoundMinors { get; }
      public int FoundMajors { get; }
      public int FoundJacks { get; }
      public int FoundQueens { get; }
      public int FoundPawnForwardness { get; }
      public int FoundConsuls { get; }
      public int FoundKingPromotions { get; }
      public bool IsGrand { get; }

      private ItemProgressSnapshot(Dictionary<string, int> itemCounts)
      {
        FoundPocketRange = Math.Min(6, Count(itemCounts, ApmwConstants.ProgressiveItems.PocketRange));
        FoundPocketGems = Count(itemCounts, ApmwConstants.ProgressiveItems.PocketGems);
        GeriProviderPlayer = Any(itemCounts, ApmwConstants.ProgressiveItems.PlayAsWhite) ? 0 : 1;
        EngineWeakening = Math.Min(5, Count(itemCounts, ApmwConstants.ProgressiveItems.AIIntelligenceMalus));
        FoundPockets = Math.Min(12, Count(itemCounts, ApmwConstants.ProgressiveItems.Pocket));
        FoundPawns = Count(itemCounts, ApmwConstants.ProgressiveItems.Pawn);
        FoundMinors = Count(itemCounts, ApmwConstants.ProgressiveItems.MinorPiece);
        FoundMajors = Count(itemCounts, ApmwConstants.ProgressiveItems.MajorPiece);
        FoundJacks = Count(itemCounts, ApmwConstants.ProgressiveItems.Jack);
        FoundQueens = Count(itemCounts, ApmwConstants.ProgressiveItems.MajorToQueen);
        FoundPawnForwardness = Count(itemCounts, ApmwConstants.ProgressiveItems.PawnForwardness);
        FoundConsuls = Math.Min(2, Count(itemCounts, ApmwConstants.ProgressiveItems.Consul));
        FoundKingPromotions = Math.Min(2, Count(itemCounts, ApmwConstants.ProgressiveItems.KingPromotion));
        IsGrand = Any(itemCounts, ApmwConstants.ProgressiveItems.SuperSizeMe);
      }

      public static ItemProgressSnapshot FromReceivedItems(IReceivedItemsHelper receivedItemsHelper)
      {
        var itemCounts = receivedItemsHelper.AllItemsReceived
          .Select(item => receivedItemsHelper.GetItemName(item.ItemId, ApmwConstants.TrackerName))
          .Where(name => name != null)
          .GroupBy(name => name)
          .ToDictionary(group => group.Key, group => group.Count());

        return new ItemProgressSnapshot(itemCounts);
      }

      private static int Count(Dictionary<string, int> itemCounts, string name)
      {
        return itemCounts.TryGetValue(name, out int count) ? count : 0;
      }

      private static bool Any(Dictionary<string, int> itemCounts, string name)
      {
        return Count(itemCounts, name) > 0;
      }
    }

    public void Unhook()
    {
      if (!isHooked)
        return;

      ReceivedItemsHelper.ItemReceived -= irHandler;
      isHooked = false;

      // Restore original providers
      ApmwCore core = ApmwCore.getInstance();
      core.PlayerPieceSetProvider = originalPlayerPieceSetProvider;
      core.PlayerPocketPiecesProvider = originalPlayerPocketPiecesProvider;
    }

    ///////////////////////
    /// GENERATE PIECES ///
    ///////////////////////

    public (Dictionary<KeyValuePair<int, int>, PieceType>, string) generatePlayerPieceSet(int numFiles)
    {
      return GeneratePlayerPieceSet(numFiles);
    }

    internal (Dictionary<KeyValuePair<int, int>, PieceType>, string) GeneratePlayerPieceSet(int numFiles)
    {
      return PlayerPieceSetGeneration.Generate(numFiles);
    }

    public List<PieceType> GeneratePawns(int numFiles, List<PieceType> minors, int spare_material)
    {
      return PawnGeneration.GeneratePawns(numFiles, minors, spare_material);
    }

    public List<PieceType> GenerateMinors(int numFiles, List<PieceType> queens, List<string> promotions, ref int spare_material)
    {
      return MinorPieceGeneration.Generate(numFiles, queens, promotions, ref spare_material);
    }

    public List<PieceType> SubstituteQueens(int numFiles, List<PieceType> majors, List<int> order, List<string> promotions, ref int spare_material)
    {
      return QueenGeneration.Substitute(numFiles, majors, order, promotions, ref spare_material);
    }

    public List<PieceType> GenerateMajors(int numFiles, out List<int> order, List<string> promotions, ref int spare_material)
    {
      return MajorPieceGeneration.Generate(numFiles, out order, promotions, ref spare_material);
    }

    public List<PieceType> generatePocketItems()
    {
      return GeneratePocketItems();
    }

    internal List<PieceType> GeneratePocketItems()
    {
      return PocketItemGeneration.Generate();
    }

    internal List<PieceType> PickPawns(Random randomPieces, int adjustedPawnValues, int remainingPawnSpaces, int foundPawns)
    {
      return PawnGeneration.PickPawns(randomPieces, adjustedPawnValues, remainingPawnSpaces, foundPawns);
    }

    internal List<PieceType> PickPawns(Random randomPieces, List<PieceType> pawnOptions, int adjustedPawnValues, int remainingPawnSpaces, int foundPawns)
    {
      return PawnGeneration.PickPawns(randomPieces, pawnOptions, adjustedPawnValues, remainingPawnSpaces, foundPawns);
    }
  }
}
