using Archipelago.MultiClient.Net.Helpers;
using ChessV;
using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using static Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper;

namespace Archipelago.APChessV
{
  internal static class ApmwEffectiveMaxima
  {
    public const int PlayAsWhite = 1;
    public const int AIIntelligenceMalus = 5;
    public const int Pocket = 12;
    public const int PocketRange = 6;
    public const int KingPromotion = 2;
    public const int Consul = 2;
    public const int Pawn = 60;
    public const int PawnForwardness = 13;
    public const int Minor = 15;
    public const int Major = 11;
    public const int MajorToQueen = 9;
    public const int Jack = 9;
    public const int Chessmen = 107;
    public const int Material = 321;
    public const int Castler = 2;
    public const int BoardFiles = 2;
    public const int BoardRanks = 2;
  }

  public class ItemHandler
  {
    public ItemHandler(IReceivedItemsHelper receivedItemsHelper)
      : this(receivedItemsHelper, null)
    {
    }

    internal ItemHandler(
      IReceivedItemsHelper receivedItemsHelper,
      IApmwProjectionBackend projectionBackend)
    {
      ReceivedItemsHelper = receivedItemsHelper;
      this.projectionBackend = projectionBackend ?? new CurrentCSharpProjectionBackend();

      irHandler = (helper) => this.Hook();
      ReceivedItemsHelper.ItemReceived += irHandler;
      isHooked = true;
      this.Hook();

      // Save original providers before overwriting global state
      ApmwCore core = ApmwCore.getInstance();
      originalPlayerPieceSetProvider = core.PlayerPieceSetProvider;
      originalGeometryAwarePlayerPieceSetProvider =
        core.GeometryAwarePlayerPieceSetProvider;
      originalPlayerPocketPiecesProvider = core.PlayerPocketPiecesProvider;

      // overwrite global state
      core.PlayerPieceSetProvider = (numFiles) => GeneratePlayerPieceSet(numFiles);
      if (ApmwConfig.getInstance().UsesCurrentContract)
      {
        core.GeometryAwarePlayerPieceSetProvider =
          (numFiles, numRanks) => GenerateProjectedPlayerPieceSet(numFiles, numRanks);
      }
      core.PlayerPocketPiecesProvider = () => GeneratePocketItems();
    }

    private readonly IReceivedItemsHelper ReceivedItemsHelper;
    private readonly IApmwProjectionBackend projectionBackend;
    private readonly ItemReceivedHandler irHandler;
    private bool isHooked;
    private Func<int, (Dictionary<KeyValuePair<int, int>, PieceType>, string)> originalPlayerPieceSetProvider;
    private Func<int, int, (Dictionary<KeyValuePair<int, int>, PieceType>, string)>
      originalGeometryAwarePlayerPieceSetProvider;
    private Func<List<PieceType>> originalPlayerPocketPiecesProvider;

    public event EventHandler ReceivedItemsChanged;
    public ApmwGeometryUnlockSnapshot GeometryUnlocks { get; private set; } =
      ApmwGeometryUnlockSnapshot.Empty;

    public void Hook()
    {
      ItemProgressSnapshot progress = ItemProgressSnapshot.FromReceivedItems(ReceivedItemsHelper);
      ApplyProgressSnapshot(ApmwCore.getInstance(), progress);
      projectionBackend.Invalidate();
      GeometryUnlocks = progress.GeometryUnlocks;
      ReceivedItemsChanged?.Invoke(this, EventArgs.Empty);
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
      core.foundAmazons = progress.FoundAmazons;
      core.foundPawnForwardness = progress.FoundPawnForwardness;
      core.foundConsuls = progress.FoundConsuls;
      core.foundKingPromotions = progress.FoundKingPromotions;
      core.foundChessmen = progress.FoundChessmen;
      core.foundMaterialBudget = progress.FoundMaterialBudget;
      core.foundCastlers = progress.FoundCastlers;
      core.foundPlayAsWhite = progress.FoundPlayAsWhite;
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
      public int FoundAmazons { get; }
      public int FoundPawnForwardness { get; }
      public int FoundConsuls { get; }
      public int FoundKingPromotions { get; }
      public int FoundChessmen { get; }
      public int FoundMaterialBudget { get; }
      public int FoundCastlers { get; }
      public int FoundPlayAsWhite { get; }
      public bool IsGrand { get; }
      public ApmwGeometryUnlockSnapshot GeometryUnlocks { get; }

      private ItemProgressSnapshot(Dictionary<string, int> itemCounts, ApmwConfig config)
      {
        FoundPocketRange = Math.Min(ApmwEffectiveMaxima.PocketRange, Count(itemCounts, ApmwConstants.ProgressiveItems.PocketRange));
        FoundPocketGems = Count(itemCounts, ApmwConstants.ProgressiveItems.PocketGems);
        FoundPlayAsWhite = Math.Min(ApmwEffectiveMaxima.PlayAsWhite, Count(itemCounts, ApmwConstants.ProgressiveItems.PlayAsWhite));
        GeriProviderPlayer = FoundPlayAsWhite > 0 ? 0 : 1;
        EngineWeakening = Math.Min(ApmwEffectiveMaxima.AIIntelligenceMalus, Count(itemCounts, ApmwConstants.ProgressiveItems.AIIntelligenceMalus));
        FoundPockets = Math.Min(ApmwEffectiveMaxima.Pocket, Count(itemCounts, ApmwConstants.ProgressiveItems.Pocket));
        FoundConsuls = Math.Min(ApmwEffectiveMaxima.Consul, Count(itemCounts, ApmwConstants.ProgressiveItems.Consul));
        FoundKingPromotions = Math.Min(ApmwEffectiveMaxima.KingPromotion, Count(itemCounts, ApmwConstants.ProgressiveItems.KingPromotion));
        if (config.UsesFundamentalProgressionItemization)
        {
          FoundPawns = 0;
          FoundMinors = 0;
          FoundMajors = 0;
          FoundJacks = 0;
          FoundQueens = 0;
          FoundAmazons = 0;
          FoundPawnForwardness = 0;
          FoundChessmen = Math.Min(ApmwEffectiveMaxima.Chessmen, Count(itemCounts, ApmwConstants.ProgressiveItems.Chessmen));
          FoundMaterialBudget = Math.Min(ApmwEffectiveMaxima.Material, Count(itemCounts, ApmwConstants.ProgressiveItems.Material)) * config.materialItemValue;
          FoundCastlers = Math.Min(
            ApmwEffectiveMaxima.Castler,
            Math.Min(
              Math.Max(0, config.castlingLocationCount),
              Count(itemCounts, ApmwConstants.ProgressiveItems.Castler)));
        }
        else
        {
          FoundPawns = Math.Min(ApmwEffectiveMaxima.Pawn, Count(itemCounts, ApmwConstants.ProgressiveItems.Pawn));
          FoundMinors = Math.Min(ApmwEffectiveMaxima.Minor, Count(itemCounts, ApmwConstants.ProgressiveItems.MinorPiece));
          FoundMajors = Math.Min(ApmwEffectiveMaxima.Major, Count(itemCounts, ApmwConstants.ProgressiveItems.MajorPiece));
          FoundJacks = Math.Min(ApmwEffectiveMaxima.Jack, Count(itemCounts, ApmwConstants.ProgressiveItems.Jack));
          FoundQueens = Math.Min(ApmwEffectiveMaxima.MajorToQueen, Count(itemCounts, ApmwConstants.ProgressiveItems.MajorToQueen));
          FoundAmazons = config.UsesCurrentContract
            ? 0
            : Count(itemCounts, ApmwConstants.ProgressiveItems.Amazon);
          FoundPawnForwardness = Math.Min(ApmwEffectiveMaxima.PawnForwardness, Count(itemCounts, ApmwConstants.ProgressiveItems.PawnForwardness));
          FoundChessmen = 0;
          FoundMaterialBudget = 0;
          FoundCastlers = 0;
        }
        IsGrand = Any(itemCounts, ApmwConstants.ProgressiveItems.SuperSizeMe);
        GeometryUnlocks = new ApmwGeometryUnlockSnapshot(
          Math.Min(
            ApmwEffectiveMaxima.BoardFiles,
            Count(itemCounts, ApmwConstants.ProgressiveItems.BoardFiles)),
          Math.Min(
            ApmwEffectiveMaxima.BoardRanks,
            Count(itemCounts, ApmwConstants.ProgressiveItems.BoardRanks)),
          IsGrand);
      }

      public static ItemProgressSnapshot FromReceivedItems(IReceivedItemsHelper receivedItemsHelper)
      {
        // TODO(chesslogic): Grouped counts cannot reconstruct acquisition/experiential continuity.
        // Preserving that would require stable received-item identities or an explicit replay
        // contract; see ApmwItemHandlerCharacterizationTests and the v2 roster plan.
        var itemCounts = receivedItemsHelper.AllItemsReceived
          .Select(item => receivedItemsHelper.GetItemName(item.ItemId, ApmwConstants.TrackerName))
          .Where(name => name != null)
          .GroupBy(name => name)
          .ToDictionary(group => group.Key, group => group.Count());

        return new ItemProgressSnapshot(itemCounts, ApmwConfig.getInstance());
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
      core.GeometryAwarePlayerPieceSetProvider =
        originalGeometryAwarePlayerPieceSetProvider;
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

    internal GeneratedRoster GenerateOwnedRoster()
    {
      return projectionBackend.GenerateOwnedRoster();
    }

    internal ActiveRosterProjection ProjectOwnedRoster(ProjectionGeometry geometry)
    {
      return projectionBackend.Project(geometry);
    }

    internal (Dictionary<KeyValuePair<int, int>, PieceType>, string)
      GenerateProjectedPlayerPieceSet(int numFiles, int numRanks)
    {
      ActiveRosterProjection projection = ProjectOwnedRoster(
        ProjectionGeometry.For(numFiles, numRanks));
      var pieces = new Dictionary<KeyValuePair<int, int>, PieceType>();
      int backSourceRank = projection.Geometry.HumanFormationRanks - 1;
      pieces.Add(
        new KeyValuePair<int, int>(
          backSourceRank - projection.PrimaryKingPlacement.Coordinate.RelativeRank,
          projection.PrimaryKingPlacement.Coordinate.File),
        projection.PrimaryKing);

      foreach (ProjectedRosterPiece projectedPiece in projection.ActivePieces)
      {
        if (projectedPiece.ConcretePieceType == null ||
            projectedPiece.Placement == null)
        {
          throw new InvalidOperationException(
            "Active APMW projection pieces require concrete types and placements.");
        }

        ProjectionCoordinate coordinate = projectedPiece.Placement.Coordinate;
        pieces.Add(
          new KeyValuePair<int, int>(
            backSourceRank - coordinate.RelativeRank,
            coordinate.File),
          projectedPiece.ConcretePieceType);
      }

      return (
        pieces,
        string.Concat(projection.ActivePromotionCatalog));
    }

    public ApmwGeometryPreview GetGeometryPreview(int files, int ranks)
    {
      return ApmwGeometryPreview.FromProjection(
        ProjectOwnedRoster(ProjectionGeometry.For(files, ranks)));
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
