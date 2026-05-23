using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Archipelago.APChessV;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using ChessV.Base;
using ChessV.Games;
using Moq;

namespace ChessV.Test
{
    internal enum ApmwFuzzStage
    {
        Configuration,
        ItemHandlerGeneration,
        StandardBoardStartup,
        SuperBoardStartup,
        MoveGeneration,
        InvariantValidation,
    }

    internal static class ApmwFuzzStages
    {
        public static ApmwFuzzStage BoardStartup(bool isSuperSized)
        {
            return isSuperSized ? ApmwFuzzStage.SuperBoardStartup : ApmwFuzzStage.StandardBoardStartup;
        }

        public static string GetName(ApmwFuzzStage stage)
        {
            return ApmwFuzzCase.TargetStages.FromStage(stage);
        }
    }

    internal sealed class ApmwFuzzGenerationResult
    {
        public ApmwFuzzGenerationResult(
            int numFiles,
            Dictionary<KeyValuePair<int, int>, PieceType> playerPieceSet,
            string promotionTypes,
            IList<PieceType> pocketPieces)
        {
            NumFiles = numFiles;
            PlayerPieceSet = new ReadOnlyDictionary<KeyValuePair<int, int>, PieceType>(
                playerPieceSet ?? throw new ArgumentNullException(nameof(playerPieceSet)));
            PromotionTypes = promotionTypes ?? string.Empty;
            PocketPieces = new ReadOnlyCollection<PieceType>(
                (pocketPieces ?? throw new ArgumentNullException(nameof(pocketPieces))).ToList());
        }

        public ApmwFuzzStage Stage { get { return ApmwFuzzStage.ItemHandlerGeneration; } }

        public string TargetStage { get { return ApmwFuzzStages.GetName(Stage); } }

        public int NumFiles { get; }

        public IReadOnlyDictionary<KeyValuePair<int, int>, PieceType> PlayerPieceSet { get; }

        public string PromotionTypes { get; }

        public IReadOnlyList<PieceType> PocketPieces { get; }
    }

    internal sealed class ApmwFuzzStartupResult
    {
        public ApmwFuzzStartupResult(string gameName, int expectedNumFiles, bool isSuperSized, Game game)
        {
            GameName = gameName ?? throw new ArgumentNullException(nameof(gameName));
            ExpectedNumFiles = expectedNumFiles;
            IsSuperSized = isSuperSized;
            Stage = ApmwFuzzStages.BoardStartup(isSuperSized);
            Game = game ?? throw new ArgumentNullException(nameof(game));
        }

        public ApmwFuzzStage Stage { get; }

        public string TargetStage { get { return ApmwFuzzStages.GetName(Stage); } }

        public string GameName { get; }

        public int ExpectedNumFiles { get; }

        public bool IsSuperSized { get; }

        public Game Game { get; }
    }

    internal sealed class ApmwFuzzScope : IDisposable
    {
        private const int FuzzSlotId = 1;
        private const string FuzzSlotName = "ApmwFuzzScope";
        private const long FirstFuzzItemId = 100000;

        private static readonly object SyncRoot = new object();

        private bool disposed;
        private bool lockTaken;

        public ApmwFuzzScope(ApmwFuzzCase fuzzCase)
        {
            if (fuzzCase == null)
                throw new ArgumentNullException(nameof(fuzzCase));

            Monitor.Enter(SyncRoot, ref lockTaken);

            try
            {
                Case = fuzzCase;
                ConfigureProductionState();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public static ApmwFuzzScope Configure(ApmwFuzzCase fuzzCase)
        {
            return new ApmwFuzzScope(fuzzCase);
        }

        public ApmwFuzzCase Case { get; }

        public ItemHandler Handler { get; private set; }

        public int NumFiles { get { return Case.IsSuperSized ? 10 : 8; } }

        public ApmwFuzzGenerationResult RunItemHandlerGeneration()
        {
            ThrowIfDisposed();
            ConfigureProductionState();
            PreparePieceTypesForGeneration();

            var playerPieceSet = Handler.generatePlayerPieceSet(NumFiles);
            var pocketPieces = Handler.generatePocketItems();

            return new ApmwFuzzGenerationResult(
                NumFiles,
                playerPieceSet.Item1,
                playerPieceSet.Item2,
                pocketPieces);
        }

        public (Dictionary<KeyValuePair<int, int>, PieceType> PlayerPieceSet, string PromotionTypes) GeneratePlayerPieceSet()
        {
            ThrowIfDisposed();
            ConfigureProductionState();
            PreparePieceTypesForGeneration();

            var playerPieceSet = Handler.generatePlayerPieceSet(NumFiles);
            return (playerPieceSet.Item1, playerPieceSet.Item2);
        }

        public IReadOnlyList<PieceType> GeneratePocketItems()
        {
            ThrowIfDisposed();
            ConfigureProductionState();
            PreparePieceTypesForGeneration();
            EnsureConfigSeeded();

            return new ReadOnlyCollection<PieceType>(Handler.generatePocketItems());
        }

        public ApmwFuzzStartupResult RunGameStartup()
        {
            ThrowIfDisposed();
            ConfigureProductionState();
            return new ApmwFuzzStartupResult(
                Case.GameName,
                NumFiles,
                Case.IsSuperSized,
                new ChessV.Manager.Manager().CreateGame(Case.GameName));
        }

        public Game CreateGame()
        {
            return RunGameStartup().Game;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            try
            {
                if (Handler != null)
                {
                    Handler.Unhook();
                    Handler = null;
                }
            }
            finally
            {
                ResetSingletons();

                if (lockTaken)
                {
                    lockTaken = false;
                    Monitor.Exit(SyncRoot);
                }
            }
        }

        private static Mock<IReceivedItemsHelper> CreateReceivedItemsHelper(ApmwFuzzCase fuzzCase)
        {
            var itemNamesById = BuildItemNamesById(fuzzCase);
            var items = new ReadOnlyCollection<ItemInfo>(BuildReceivedItems(fuzzCase, itemNamesById));

            var helper = new Mock<IReceivedItemsHelper>();
            helper.SetupGet(h => h.AllItemsReceived).Returns(items);
            helper.Setup(h => h.GetItemName(It.IsAny<long>(), It.IsAny<string>()))
                .Returns<long, string>((itemId, _) =>
                    itemNamesById.TryGetValue(itemId, out string itemName) ? itemName : null);

            return helper;
        }

        private static Dictionary<long, string> BuildItemNamesById(ApmwFuzzCase fuzzCase)
        {
            return fuzzCase.BuildItemCounts()
                .Select((item, index) => new { item.ItemName, ItemId = FirstFuzzItemId + index })
                .ToDictionary(item => item.ItemId, item => item.ItemName);
        }

        private static List<ItemInfo> BuildReceivedItems(
            ApmwFuzzCase fuzzCase,
            IReadOnlyDictionary<long, string> itemNamesById)
        {
            var itemIdsByName = itemNamesById.ToDictionary(item => item.Value, item => item.Key);
            var items = new List<ItemInfo>();

            foreach (var item in fuzzCase.BuildPositiveItemCounts())
            {
                if (itemIdsByName.TryGetValue(item.ItemName, out long itemId))
                    AddItems(items, itemId, item.Count);
            }

            return items;
        }

        private static void AddItems(List<ItemInfo> items, long itemId, int count)
        {
            for (int i = 0; i < count; i++)
                items.Add(CreateItemInfo(itemId));
        }

        private static ItemInfo CreateItemInfo(long itemId)
        {
            return new ItemInfo(
                new NetworkItem
                {
                    Item = itemId,
                    Player = FuzzSlotId,
                },
                ApmwConstants.TrackerName,
                ApmwConstants.TrackerName,
                null,
                new PlayerInfo(
                    0,
                    FuzzSlotId,
                    FuzzSlotName,
                    FuzzSlotName,
                    ApmwConstants.TrackerName,
                    new NetworkSlot[0],
                    new int[0]));
        }

        private static void ResetSingletons()
        {
            ApmwCore._instance = null;
            ApmwConfig._instance = null;
        }

        private void ConfigureProductionState()
        {
            if (Handler != null)
            {
                Handler.Unhook();
                Handler = null;
            }

            ResetSingletons();

            ApmwCore._instance = new ApmwCore();
            ApmwConfig._instance = new ApmwConfig();
            ApmwConfig.getInstance().Instantiate(Case.BuildSlotData());

            Handler = new ItemHandler(CreateReceivedItemsHelper(Case).Object);
        }

        private void PreparePieceTypesForGeneration()
        {
            ApmwChessGame generationGame = Case.IsSuperSized
                ? new ApmwGrandChess()
                : new ApmwChessGame();
            generationGame.earlyPopulatePieceTypes();
        }

        private static void EnsureConfigSeeded()
        {
            if (ApmwConfig.getInstance().pocketSeed == -1)
                ApmwConfig.getInstance().seed();
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ApmwFuzzScope));
        }
    }
}
