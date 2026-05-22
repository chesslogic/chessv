using System.Collections.Generic;
using System.Collections.ObjectModel;
using Archipelago.APChessV;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using ChessV.Base;
using Moq;
using Newtonsoft.Json.Linq;

namespace ChessV.Test
{
    internal static class ApmwSeed23348465130110326631TestSupport
    {
        internal const string SeedName = "23348465130110326631";
        internal const int ChecksMateSlotId = 91;
        internal const string ChecksMateSlotName = "HakknivChess1";
        internal const string ReportedGameName = "Archipelago Multiworld Super-Sized";

        private const long ProgressivePawnItemId = 1000;
        private const long ProgressivePocketRangeItemId = 1001;
        private const long ProgressiveAIIntelligenceMalusItemId = 1002;
        private const long ProgressivePocketItemId = 1003;
        private const long ProgressivePawnForwardnessItemId = 1004;
        private const long ProgressivePocketGemsItemId = 1005;
        private const long ProgressiveMinorPieceItemId = 1006;
        private const long VictoryItemId = 1007;
        private const long ProgressiveMajorPieceItemId = 1008;
        private const long PlayAsWhiteItemId = 1009;
        private const long SuperSizeMeItemId = 1010;
        private const long ProgressiveConsulItemId = 1011;
        private const long ProgressiveKingPromotionItemId = 1012;

        internal static void ConfigureReportedState()
        {
            ApmwCore._instance = new ApmwCore();
            ApmwConfig._instance = new ApmwConfig();
            ApmwConfig.getInstance().Instantiate(BuildSlotData());

            var itemNamesById = BuildItemNamesById();
            var helper = new Mock<IReceivedItemsHelper>();
            helper.SetupGet(h => h.AllItemsReceived)
                .Returns(new ReadOnlyCollection<ItemInfo>(BuildReceivedItems()));
            helper.Setup(h => h.GetItemName(It.IsAny<long>(), It.IsAny<string>()))
                .Returns<long, string>((itemId, _) =>
                    itemNamesById.TryGetValue(itemId, out string itemName) ? itemName : null);

            _ = new ItemHandler(helper.Object);
        }

        internal static Game CreateReportedGame()
        {
            ConfigureReportedState();
            if (!ApmwCore.getInstance().isGrand)
                throw new System.InvalidOperationException("Reported state did not configure Super-Size Me.");
            return new ChessV.Manager.Manager().CreateGame(ReportedGameName);
        }

        private static Dictionary<string, object> BuildSlotData()
        {
            return new Dictionary<string, object>
            {
                ["goal"] = 1,
                ["piece_locations"] = 1,
                ["piece_types"] = 1,
                ["fairy_chess_army"] = 0,
                ["fairy_chess_pawns"] = 1,
                ["army"] = new JArray(0, 1, 2, 3, 4, 5, 6),
                ["pocket_seed"] = 2005800662,
                ["pawn_seed"] = 372563268,
                ["minor_seed"] = 2034783094,
                ["major_seed"] = 319842325,
                ["queen_seed"] = 226887457,
                ["minor_piece_limit_by_type"] = 0,
                ["major_piece_limit_by_type"] = 0,
                ["queen_piece_limit_by_type"] = 0,
                ["pocket_limit_by_pocket"] = 4,
                ["required_chess_client_version"] = ApmwConstants.ClientVersion,
            };
        }

        private static Dictionary<long, string> BuildItemNamesById()
        {
            return new Dictionary<long, string>
            {
                [ProgressivePawnItemId] = ApmwConstants.ProgressiveItems.Pawn,
                [ProgressivePocketRangeItemId] = ApmwConstants.ProgressiveItems.PocketRange,
                [ProgressiveAIIntelligenceMalusItemId] = ApmwConstants.ProgressiveItems.AIIntelligenceMalus,
                [ProgressivePocketItemId] = ApmwConstants.ProgressiveItems.Pocket,
                [ProgressivePawnForwardnessItemId] = ApmwConstants.ProgressiveItems.PawnForwardness,
                [ProgressivePocketGemsItemId] = ApmwConstants.ProgressiveItems.PocketGems,
                [ProgressiveMinorPieceItemId] = ApmwConstants.ProgressiveItems.MinorPiece,
                [VictoryItemId] = "Victory",
                [ProgressiveMajorPieceItemId] = ApmwConstants.ProgressiveItems.MajorPiece,
                [PlayAsWhiteItemId] = ApmwConstants.ProgressiveItems.PlayAsWhite,
                [SuperSizeMeItemId] = ApmwConstants.ProgressiveItems.SuperSizeMe,
                [ProgressiveConsulItemId] = ApmwConstants.ProgressiveItems.Consul,
                [ProgressiveKingPromotionItemId] = ApmwConstants.ProgressiveItems.KingPromotion,
            };
        }

        private static List<ItemInfo> BuildReceivedItems()
        {
            var items = new List<ItemInfo>();
            AddItems(items, ProgressivePawnItemId, 14);
            AddItems(items, ProgressivePocketRangeItemId, 9);
            AddItems(items, ProgressiveAIIntelligenceMalusItemId, 5);
            AddItems(items, ProgressivePocketItemId, 9);
            AddItems(items, ProgressivePawnForwardnessItemId, 11);
            AddItems(items, ProgressivePocketGemsItemId, 11);
            AddItems(items, ProgressiveMinorPieceItemId, 13);
            AddItems(items, VictoryItemId, 1);
            AddItems(items, ProgressiveMajorPieceItemId, 4);
            AddItems(items, PlayAsWhiteItemId, 1);
            AddItems(items, SuperSizeMeItemId, 1);
            AddItems(items, ProgressiveConsulItemId, 1);
            AddItems(items, ProgressiveKingPromotionItemId, 2);
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
                    Player = ChecksMateSlotId,
                },
                ApmwConstants.TrackerName,
                ApmwConstants.TrackerName,
                null,
                new PlayerInfo(
                    0,
                    ChecksMateSlotId,
                    ChecksMateSlotName,
                    ChecksMateSlotName,
                    ApmwConstants.TrackerName,
                    new NetworkSlot[0],
                    new int[0]));
        }
    }
}
