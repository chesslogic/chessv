using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Archipelago.APChessV;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using static Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper;

namespace ChessV.Test
{
    [TestClass]
    [DoNotParallelize]
    public class ApmwItemHandlerCharacterizationTests
    {
        private const int NumFiles = 8;
        private ItemHandler handler;

        [TestInitialize]
        public void Setup()
        {
            ResetSingletons();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (handler != null)
            {
                handler.Unhook();
                handler = null;
            }

            ResetSingletons();
        }

        [TestMethod]
        public void Hook_MapsItemCountsAndCapsCoreFields()
        {
            var helper = new MutableReceivedItemsHelper()
                .Add(ApmwConstants.ProgressiveItems.Pocket, 15)
                .Add(ApmwConstants.ProgressiveItems.PocketRange, 9)
                .Add(ApmwConstants.ProgressiveItems.PocketGems, 4)
                .Add(ApmwConstants.ProgressiveItems.PlayAsWhite)
                .Add(ApmwConstants.ProgressiveItems.AIIntelligenceMalus, 7)
                .Add(ApmwConstants.ProgressiveItems.Pawn, 8)
                .Add(ApmwConstants.ProgressiveItems.MinorPiece, 5)
                .Add(ApmwConstants.ProgressiveItems.MajorPiece, 4)
                .Add(ApmwConstants.ProgressiveItems.Jack, 3)
                .Add(ApmwConstants.ProgressiveItems.MajorToQueen, 2)
                .Add(ApmwConstants.ProgressiveItems.Amazon, 4)
                .Add(ApmwConstants.ProgressiveItems.PawnForwardness, 6)
                .Add(ApmwConstants.ProgressiveItems.Consul, 5)
                .Add(ApmwConstants.ProgressiveItems.KingPromotion, 4)
                .Add(ApmwConstants.ProgressiveItems.SuperSizeMe);

            var core = ApmwCore.getInstance();
            var originalPieceProvider = core.PlayerPieceSetProvider;
            var originalPocketProvider = core.PlayerPocketPiecesProvider;

            handler = new ItemHandler(helper);

            Assert.AreEqual(12, core.foundPockets, "pockets cap at 12");
            Assert.AreEqual(6, core.foundPocketRange, "pocket range caps at 6");
            Assert.AreEqual(4, core.foundPocketGems);
            Assert.AreEqual(8, core.foundPawns);
            Assert.AreEqual(5, core.foundMinors);
            Assert.AreEqual(4, core.foundMajors);
            Assert.AreEqual(3, core.foundJacks);
            Assert.AreEqual(2, core.foundQueens);
            Assert.AreEqual(4, core.foundAmazons);
            Assert.AreEqual(6, core.foundPawnForwardness);
            Assert.AreEqual(2, core.foundConsuls, "consuls cap at 2");
            Assert.AreEqual(2, core.foundKingPromotions, "king promotions cap at 2");
            Assert.IsTrue(core.isGrand);
            Assert.AreEqual(0, core.GeriProvider(), "Play as White selects player 0");
            Assert.AreEqual(5, core.EngineWeakeningProvider(), "AI malus caps at 5");
            Assert.AreNotSame(originalPieceProvider, core.PlayerPieceSetProvider);
            Assert.AreNotSame(originalPocketProvider, core.PlayerPocketPiecesProvider);
        }

        [TestMethod]
        public void Unhook_StopsItemReceivedFromRefreshingHookedFields()
        {
            var helper = new MutableReceivedItemsHelper()
                .Add(ApmwConstants.ProgressiveItems.Pawn);
            handler = new ItemHandler(helper);

            Assert.AreEqual(1, ApmwCore.getInstance().foundPawns);

            helper.Add(ApmwConstants.ProgressiveItems.Pawn);
            helper.RaiseItemReceived();
            Assert.AreEqual(2, ApmwCore.getInstance().foundPawns, "event should refresh before unhook");

            handler.Unhook();
            helper.Add(ApmwConstants.ProgressiveItems.Pawn);
            helper.RaiseItemReceived();

            Assert.AreEqual(2, ApmwCore.getInstance().foundPawns, "event should not refresh after unhook");
        }

        [TestMethod]
        public void Constructor_InstallsProvidersThatDelegateToCurrentHandler()
        {
            ConfigureStableGeneration();

            var helper = new MutableReceivedItemsHelper()
                .Add(ApmwConstants.ProgressiveItems.Pocket, 3)
                .Add(ApmwConstants.ProgressiveItems.Pawn, 8)
                .Add(ApmwConstants.ProgressiveItems.MinorPiece, 2)
                .Add(ApmwConstants.ProgressiveItems.MajorPiece, 2)
                .Add(ApmwConstants.ProgressiveItems.MajorToQueen);
            handler = new ItemHandler(helper);

            var core = ApmwCore.getInstance();
            var directPieceSet = handler.generatePlayerPieceSet(NumFiles);
            var providerPieceSet = core.PlayerPieceSetProvider(NumFiles);
            Assert.AreEqual(
                PieceSetSignature(directPieceSet.Item1),
                PieceSetSignature(providerPieceSet.Item1));
            Assert.AreEqual(directPieceSet.Item2, providerPieceSet.Item2);

            var directPockets = handler.generatePocketItems();
            var providerPockets = core.PlayerPocketPiecesProvider();
            CollectionAssert.AreEqual(PieceNames(directPockets), PieceNames(providerPockets));
        }

        [TestMethod]
        public void Generation_RespectsArmyFilteringForPiecesAndPockets()
        {
            var fuzzCase = ApmwFuzzCase.DefaultStandard().With(builder =>
            {
                builder.CaseName = "characterization-army-filter";
                builder.ArmyIndexes = new[] { 1 };
                builder.PocketCount = 12;
                builder.PocketLimitByPocket = 4;
                builder.PawnCount = 8;
                builder.MinorPieceCount = 4;
                builder.MajorPieceCount = 4;
                builder.JackCount = 1;
                builder.MajorToQueenCount = 1;
            });

            using (var scope = ApmwFuzzScope.Configure(fuzzCase))
            {
                var result = scope.RunItemHandlerGeneration();
                var core = ApmwCore.getInstance();
                var selectedArmy = core.armies[1];

                var constrainedPieces = result.PlayerPieceSet.Values
                    .Where(piece => IsArmyConstrainedGeneratedPiece(core, piece))
                    .ToList();
                Assert.IsTrue(constrainedPieces.Count > 0, "case should place army-filtered generated pieces");
                foreach (var piece in constrainedPieces)
                    Assert.IsTrue(selectedArmy.Contains(piece), "generated piece escaped selected army: " + piece.Name);

                Assert.AreEqual(3, result.PocketPieces.Count);
                Assert.IsTrue(result.PocketPieces.All(piece => piece != null), "12 pockets should fill all pocket slots");
                foreach (var piece in result.PocketPieces)
                    Assert.IsTrue(selectedArmy.Contains(piece), "pocket piece escaped selected army: " + piece.Name);
            }
        }

        [TestMethod]
        public void Generation_SuperMaxUsesItemHandlerCountsForEffectivePawnGuarantee()
        {
            var fuzzCase = ApmwFuzzCase.DefaultStandard().With(builder =>
            {
                builder.CaseName = "characterization-supermax-item-handler-counts";
                builder.FairyChessPawns = FairyPawns.Vanilla;
                builder.FairyChessPawnUpgrades = FairyPawnUpgrades.SuperMax;
                builder.PawnCount = 32;
                builder.MinorPieceCount = 8;
                builder.MajorPieceCount = 4;
                builder.JackCount = 2;
                builder.MajorToQueenCount = 0;
                builder.ConsulCount = 1;
            });

            using (var scope = ApmwFuzzScope.Configure(fuzzCase))
            {
                var result = scope.RunItemHandlerGeneration();
                var core = ApmwCore.getInstance();

                Assert.AreEqual(FairyPawnUpgrades.SuperMax, ApmwConfig.getInstance().PawnUpgrades);
                Assert.AreEqual(FairyPawns.Vanilla, ApmwConfig.getInstance().Pawns);
                Assert.AreEqual(32, core.foundPawns);
                Assert.AreEqual(8, core.foundMinors);
                Assert.AreEqual(4, core.foundMajors);
                Assert.AreEqual(2, core.foundJacks);
                Assert.AreEqual(1, core.foundConsuls);

                int boardLocationNeeds = (result.NumFiles == 10 ? 19 : 15) -
                    core.foundConsuls -
                    core.foundJacks -
                    core.foundMajors -
                    core.foundMinors;
                int effectivePawnGuarantee = Math.Min(core.foundPawns, Math.Max(0, boardLocationNeeds));
                Assert.AreEqual(0, effectivePawnGuarantee);

                var generatedPieces = result.PlayerPieceSet.Values.ToList();
                int generatedPawnFamilyCount = generatedPieces.Count(piece =>
                    core.pawns.Contains(piece) || core.sergeants.Contains(piece));
                Assert.IsTrue(
                    CountFrom(generatedPieces, core.sergeants) > 0,
                    "SuperMax should allow Sergeant/Odin Pawn after the item-handler counts reduce the pawn guarantee.");
                Assert.IsTrue(
                    generatedPawnFamilyCount < core.foundPawns,
                    "SuperMax should not require every collected pawn item to become a pawn-family slot.");
            }
        }

        [TestMethod]
        public void EarlyPopulate_PlacesHeraldAndAmazonInAmazonFamilyOnly()
        {
            ConfigureStableGeneration();
            var core = ApmwCore.getInstance();

            Assert.IsFalse(core.queens.Any(piece => piece.Name == "Herald"), "Herald should no longer be a queen-family piece");
            Assert.IsTrue(core.amazons.Any(piece => piece.Name == "Herald"), "Herald should be an amazon-family piece");
            Assert.IsTrue(core.amazons.Any(piece => piece.Name == "Amazon"), "Amazon should be an amazon-family piece");
        }

        [TestMethod]
        public void MajorQueenMinorGeneration_PlacesPiecesAndTracksPromotionStrings()
        {
            ConfigureStableGeneration();
            handler = new ItemHandler(new MutableReceivedItemsHelper());
            var core = ApmwCore.getInstance();
            core.foundConsuls = 0;
            core.foundKingPromotions = 0;
            core.foundMajors = 2;
            core.foundQueens = 1;
            core.foundJacks = 0;
            core.foundMinors = 4;

            int spareMaterial = 0;
            var promotions = new List<string>();
            List<int> order;

            var majors = handler.GenerateMajors(NumFiles, out order, promotions, ref spareMaterial);
            Assert.AreEqual(NumFiles * 2, majors.Count);
            Assert.AreSame(core.kings[0], majors[NumFiles / 2], "king remains centered");
            Assert.AreEqual(2, order.Count, "one major and one queen-reserved slot are ordered");
            Assert.AreEqual(1, CountFrom(majors, core.majors));

            var withQueens = handler.SubstituteQueens(NumFiles, majors, order, promotions, ref spareMaterial);
            Assert.AreEqual(1, CountFrom(withQueens, core.majors));
            Assert.AreEqual(1, CountFrom(withQueens, core.queens));

            var withMinors = handler.GenerateMinors(NumFiles, withQueens, promotions, ref spareMaterial);
            Assert.AreSame(core.kings[0], withMinors[NumFiles / 2], "king remains centered after minors");
            Assert.AreEqual(1, CountFrom(withMinors, core.majors));
            Assert.AreEqual(1, CountFrom(withMinors, core.queens));
            Assert.AreEqual(4, CountFrom(withMinors, core.minors));
            Assert.AreEqual(7, withMinors.Take(NumFiles).Count(piece => piece != null));
            Assert.AreEqual(0, withMinors.Skip(NumFiles).Count(piece => piece != null));
            Assert.AreEqual(3, promotions.Count, "majors, queens, and minors each append a promotion segment");

            string promotionText = string.Concat(promotions);
            int player = core.GeriProvider();
            foreach (var piece in withMinors.Where(piece => piece != null && !core.kings.Contains(piece)).Distinct())
                StringAssert.Contains(promotionText, piece.Notation[player]);
        }

        [TestMethod]
        public void Generation_NeutralAmazonUpgradeDoesNotConsumePositiveQueenUpgrade()
        {
            var fuzzCase = ApmwFuzzCase.DefaultStandard().With(builder =>
            {
                builder.CaseName = "characterization-amazon-queen-upgrades";
                builder.PawnCount = 8;
                builder.ArmyIndexes = new int[0];
                builder.MinorPieceCount = 0;
                builder.MajorPieceCount = 4;
                builder.JackCount = 0;
                builder.MajorToQueenCount = 1;
                builder.AmazonCount = 2;
                builder.QueenPieceLimitByType = 1;
                builder.MajorSeed = 710;
                builder.QueenSeed = 3;
            });

            using (var scope = ApmwFuzzScope.Configure(fuzzCase))
            {
                var result = scope.GeneratePlayerPieceSet();
                var core = ApmwCore.getInstance();
                var generated = result.PlayerPieceSet.Values.ToList();
                var amazonPieces = generated.Where(piece => core.amazons.Contains(piece)).ToList();
                var queenPieces = generated.Where(piece => core.queens.Contains(piece)).ToList();

                Assert.AreEqual(3, CountFrom(generated, core.majors), "major-to-queen consumes one existing major");
                Assert.AreEqual(1, queenPieces.Count, "neutral queen-to-amazon should not consume the positive-priority queen upgrade");
                Assert.AreEqual(0, amazonPieces.Count, "neutral source-less amazon upgrades should become pawn material instead of direct amazons");

                Assert.AreEqual(
                    result.PlayerPieceSet.Count,
                    result.PlayerPieceSet.Keys.Distinct().Count(),
                    "upgrade planning must occupy distinct board coordinates");

                foreach (var piece in queenPieces.Distinct())
                    StringAssert.Contains(result.PromotionTypes, piece.Notation[core.GeriProvider()]);
            }
        }

        [TestMethod]
        public void Generation_NeutralNonPawnUpgradesDoNotPreemptDirectMajorJackMinorPlacement()
        {
            handler = ConfigureStableGenerationWithPieceUpgradePriorities(
                new MutableReceivedItemsHelper()
                    .Add(ApmwConstants.ProgressiveItems.MinorPiece)
                    .Add(ApmwConstants.ProgressiveItems.MajorPiece)
                    .Add(ApmwConstants.ProgressiveItems.Jack),
                new JObject());

            var generated = handler.generatePlayerPieceSet(NumFiles).Item1.Values.ToList();

            Assert.AreEqual(1, CountFamily(generated, "minor"), "neutral minor-to-major should not consume direct minor placement");
            Assert.AreEqual(1, CountFamily(generated, "major"), "neutral major-to-jack/minor-to-major should not consume direct major placement");
            Assert.AreEqual(1, CountFamily(generated, "jack"), "neutral jack target should remain a direct jack");
        }

        [DataTestMethod]
        [DataRow(ApmwConstants.PieceUpgradeActions.MinorToMajor, ApmwConstants.ProgressiveItems.MinorPiece, ApmwConstants.ProgressiveItems.MajorPiece, "minor", "major")]
        [DataRow(ApmwConstants.PieceUpgradeActions.MajorToJack, ApmwConstants.ProgressiveItems.MajorPiece, ApmwConstants.ProgressiveItems.Jack, "major", "jack")]
        [DataRow(ApmwConstants.PieceUpgradeActions.MinorToJack, ApmwConstants.ProgressiveItems.MinorPiece, ApmwConstants.ProgressiveItems.Jack, "minor", "jack")]
        [DataRow(ApmwConstants.PieceUpgradeActions.MajorToQueen, ApmwConstants.ProgressiveItems.MajorPiece, ApmwConstants.ProgressiveItems.MajorToQueen, "major", "queen")]
        [DataRow(ApmwConstants.PieceUpgradeActions.JackToQueen, ApmwConstants.ProgressiveItems.Jack, ApmwConstants.ProgressiveItems.MajorToQueen, "jack", "queen")]
        public void Generation_NonPawnUpgradeActionsConvertConfiguredSourceToTarget(
            string actionName,
            string sourceItemName,
            string targetItemName,
            string sourceFamily,
            string targetFamily)
        {
            handler = ConfigureStableGenerationWithPieceUpgradePriorities(
                new MutableReceivedItemsHelper()
                    .Add(sourceItemName)
                    .Add(targetItemName),
                PriorityMapWith(actionName, 10));

            var result = handler.generatePlayerPieceSet(NumFiles);
            var core = ApmwCore.getInstance();
            var generated = result.Item1.Values.ToList();
            var targetPieces = generated.Where(piece => FamilyContains(core, targetFamily, piece)).ToList();

            Assert.AreEqual(0, CountFamily(generated, sourceFamily), actionName + " should consume its source piece");
            Assert.AreEqual(
                1,
                targetPieces.Count,
                actionName + " should create one target piece and no duplicate direct target; generated: " +
                    string.Join(", ", generated.Select(piece => piece.Name)));
            foreach (var piece in targetPieces.Distinct())
                StringAssert.Contains(result.Item2, piece.Notation[core.GeriProvider()], actionName + " should add target notation to promotions");
        }

        [TestMethod]
        public void Generation_QueenToAmazonConsumesQueenCreatedByEarlierUpgrade()
        {
            handler = ConfigureStableGenerationWithPieceUpgradePriorities(
                new MutableReceivedItemsHelper()
                    .Add(ApmwConstants.ProgressiveItems.MajorPiece)
                    .Add(ApmwConstants.ProgressiveItems.MajorToQueen)
                    .Add(ApmwConstants.ProgressiveItems.Amazon),
                PriorityMapWith(
                    ApmwConstants.PieceUpgradeActions.MajorToQueen,
                    20,
                    new Dictionary<string, int>
                    {
                        [ApmwConstants.PieceUpgradeActions.QueenToAmazon] = 10,
                        [ApmwConstants.PieceUpgradeActions.MinorToMajor] = -1,
                    }));

            var result = handler.generatePlayerPieceSet(NumFiles);
            var core = ApmwCore.getInstance();
            var generated = result.Item1.Values.ToList();
            var amazonPieces = generated.Where(piece => core.amazons.Contains(piece)).ToList();

            Assert.AreEqual(0, CountFamily(generated, "major"), "major-to-queen should consume the original major");
            Assert.AreEqual(0, CountFamily(generated, "queen"), "queen-to-amazon should consume the intermediate queen");
            Assert.AreEqual(1, amazonPieces.Count, "queen-to-amazon should create one amazon-family target");
            foreach (var piece in amazonPieces.Distinct())
                StringAssert.Contains(result.Item2, piece.Notation[core.GeriProvider()]);
        }

        [DataTestMethod]
        [DataRow(ApmwConstants.PieceUpgradeActions.MajorToJack, "major", "minor")]
        [DataRow(ApmwConstants.PieceUpgradeActions.MinorToJack, "minor", "major")]
        public void Generation_JackUpgradePriorityControlsWhichSourceConsumesFoundJacks(
            string preferredAction,
            string consumedSourceFamily,
            string remainingSourceFamily)
        {
            string otherAction = preferredAction == ApmwConstants.PieceUpgradeActions.MajorToJack
                ? ApmwConstants.PieceUpgradeActions.MinorToJack
                : ApmwConstants.PieceUpgradeActions.MajorToJack;
            handler = ConfigureStableGenerationWithPieceUpgradePriorities(
                new MutableReceivedItemsHelper()
                    .Add(ApmwConstants.ProgressiveItems.MinorPiece)
                    .Add(ApmwConstants.ProgressiveItems.MajorPiece)
                    .Add(ApmwConstants.ProgressiveItems.Jack),
                PriorityMapWith(
                    preferredAction,
                    10,
                    new Dictionary<string, int>
                    {
                        [otherAction] = 5,
                        [ApmwConstants.PieceUpgradeActions.MinorToMajor] = -1,
                    }));

            var generated = handler.generatePlayerPieceSet(NumFiles).Item1.Values.ToList();

            Assert.AreEqual(1, CountFamily(generated, "jack"), "foundJacks should only create one upgraded jack");
            Assert.AreEqual(0, CountFamily(generated, consumedSourceFamily), preferredAction + " should consume the higher-priority source");
            Assert.AreEqual(1, CountFamily(generated, remainingSourceFamily), otherAction + " should not also consume foundJacks");
        }

        [DataTestMethod]
        [DataRow(ApmwConstants.PieceUpgradeActions.MajorToQueen, "major", "jack")]
        [DataRow(ApmwConstants.PieceUpgradeActions.JackToQueen, "jack", "major")]
        public void Generation_QueenUpgradePriorityControlsWhichSourceConsumesFoundQueens(
            string preferredAction,
            string consumedSourceFamily,
            string remainingSourceFamily)
        {
            string otherAction = preferredAction == ApmwConstants.PieceUpgradeActions.MajorToQueen
                ? ApmwConstants.PieceUpgradeActions.JackToQueen
                : ApmwConstants.PieceUpgradeActions.MajorToQueen;
            handler = ConfigureStableGenerationWithPieceUpgradePriorities(
                new MutableReceivedItemsHelper()
                    .Add(ApmwConstants.ProgressiveItems.MajorPiece)
                    .Add(ApmwConstants.ProgressiveItems.Jack)
                    .Add(ApmwConstants.ProgressiveItems.MajorToQueen),
                PriorityMapWith(
                    preferredAction,
                    10,
                    new Dictionary<string, int>
                    {
                        [otherAction] = 5,
                        [ApmwConstants.PieceUpgradeActions.MinorToMajor] = -1,
                        [ApmwConstants.PieceUpgradeActions.MajorToJack] = -1,
                        [ApmwConstants.PieceUpgradeActions.MinorToJack] = -1,
                        [ApmwConstants.PieceUpgradeActions.QueenToAmazon] = -1,
                    }));

            var generated = handler.generatePlayerPieceSet(NumFiles).Item1.Values.ToList();

            Assert.AreEqual(1, CountFamily(generated, "queen"), "foundQueens should only create one upgraded queen");
            Assert.AreEqual(0, CountFamily(generated, consumedSourceFamily), preferredAction + " should consume the higher-priority source");
            Assert.AreEqual(1, CountFamily(generated, remainingSourceFamily), otherAction + " should not also consume foundQueens");
        }

        [TestMethod]
        public void Generation_DisabledNonPawnUpgradeLeavesDirectTargetAndSourcePieces()
        {
            handler = ConfigureStableGenerationWithPieceUpgradePriorities(
                new MutableReceivedItemsHelper()
                    .Add(ApmwConstants.ProgressiveItems.MinorPiece)
                    .Add(ApmwConstants.ProgressiveItems.MajorPiece),
                PriorityMapWith(
                    ApmwConstants.PieceUpgradeActions.MinorToMajor,
                    -1));

            var generated = handler.generatePlayerPieceSet(NumFiles).Item1.Values.ToList();

            Assert.AreEqual(1, CountFamily(generated, "minor"), "disabled minor-to-major should not consume the minor");
            Assert.AreEqual(1, CountFamily(generated, "major"), "disabled minor-to-major should leave the major item as a direct piece");
        }

        [TestMethod]
        public void Generation_SourceLessUpgradeConvertsTargetBudgetToPawnMaterial()
        {
            handler = ConfigureStableGenerationWithPieceUpgradePriorities(
                new MutableReceivedItemsHelper()
                    .Add(ApmwConstants.ProgressiveItems.MajorToQueen),
                PriorityMapWith(ApmwConstants.PieceUpgradeActions.MajorToQueen, 10));

            var result = handler.generatePlayerPieceSet(NumFiles);
            var core = ApmwCore.getInstance();
            var generated = result.Item1.Values.ToList();

            Assert.AreEqual(0, CountFamily(generated, "queen"), "source-less major-to-queen should not create a direct queen");
            Assert.IsTrue(
                generated.Count(piece => piece != null && (core.pawns.Contains(piece) || core.sergeants.Contains(piece))) > 0,
                "source-less upgrade material should be available to generated pawns");
        }

        [TestMethod]
        public void Generation_AmazonFamilyUpgradesDoNotCreateCastlingRookPrivileges()
        {
            handler = ConfigureStableGenerationWithPieceUpgradePriorities(
                new MutableReceivedItemsHelper()
                    .Add(ApmwConstants.ProgressiveItems.MajorPiece)
                    .Add(ApmwConstants.ProgressiveItems.MajorToQueen)
                    .Add(ApmwConstants.ProgressiveItems.Amazon),
                PriorityMapWith(
                    ApmwConstants.PieceUpgradeActions.MajorToQueen,
                    20,
                    new Dictionary<string, int>
                    {
                        [ApmwConstants.PieceUpgradeActions.QueenToAmazon] = 10,
                        [ApmwConstants.PieceUpgradeActions.MinorToMajor] = -1,
                    }),
                false);

            var game = (ApmwChessGame)new ChessV.Manager.Manager().CreateGame(ApmwConstants.GameNameStandard);
            var generatedResult = handler.generatePlayerPieceSet(NumFiles);
            var core = ApmwCore.getInstance();
            var amazonBackRankFiles = generatedResult.Item1
                .Where(item => item.Key.Key == 4 && core.amazons.Contains(item.Value))
                .Select(item => item.Key.Value)
                .ToArray();

            Assert.IsTrue(amazonBackRankFiles.Length > 0, "at least one amazon-family upgrade should be on the castling rank");
            foreach (var piece in generatedResult.Item1.Values.Where(piece => core.amazons.Contains(piece)))
            {
                Assert.IsFalse(core.majors.Contains(piece), piece.Name + " should not be a major-family castling rook");
                Assert.IsFalse(core.jacks.Contains(piece), piece.Name + " should not be a jack-family castling rook");
            }

            string castleRooks = (string)game.GetCustomProperty("CastleRooks");
            foreach (int file in amazonBackRankFiles)
                Assert.IsFalse(
                    castleRooks.Contains(char.ToUpper((char)('a' + file))),
                    "amazon-family upgrade file should not receive custom castling rights: " + file);
        }

        [TestMethod]
        public void GenerateMajors_ConvertsUnplacedMajorCapacityToSpareMaterial()
        {
            ConfigureStableSlotDataOnly();
            handler = new ItemHandler(new MutableReceivedItemsHelper());
            var major = new TestPiece("Exact Major", "M", 485);
            var core = ApmwCore.getInstance();
            core.kings = new List<PieceType>
            {
                new TestPiece("King", "K", 325),
                new TestPiece("Mounted King", "W", 700),
                new TestPiece("Hyper King", "Z", 1175),
            };
            core.majors = new HashSet<PieceType> { major };
            core.jacks = new HashSet<PieceType>();
            core.queens = new HashSet<PieceType>();
            core.armies = new List<HashSet<PieceType>>();
            ApmwConfig.getInstance().Army = new List<int>();
            core.foundConsuls = 0;
            core.foundKingPromotions = 0;
            core.foundMajors = 17;
            core.foundQueens = 0;
            core.foundJacks = 0;

            int spareMaterial = 0;
            List<int> order;
            var generated = handler.GenerateMajors(NumFiles, out order, new List<string>(), ref spareMaterial);

            Assert.AreEqual(15, CountFrom(generated, core.majors), "8x8 generation has 15 non-king major slots");
            Assert.AreEqual(2 * 485, spareMaterial);
        }

        [TestMethod]
        public void ItemGenerationAndPockets_AreRepeatableForStableSeeds()
        {
            var fuzzCase = ApmwFuzzCase.DefaultStandard().With(builder =>
            {
                builder.CaseName = "characterization-repeatable-seeds";
                builder.PocketCount = 6;
                builder.PawnCount = 8;
                builder.MinorPieceCount = 3;
                builder.MajorPieceCount = 3;
                builder.JackCount = 1;
                builder.MajorToQueenCount = 1;
                builder.PocketSeed = 501;
                builder.PawnSeed = 502;
                builder.MinorSeed = 503;
                builder.MajorSeed = 504;
                builder.QueenSeed = 505;
            });

            using (var scope = ApmwFuzzScope.Configure(fuzzCase))
            {
                var firstPieces = scope.GeneratePlayerPieceSet();
                var firstPockets = scope.GeneratePocketItems();
                var secondPieces = scope.GeneratePlayerPieceSet();
                var secondPockets = scope.GeneratePocketItems();

                Assert.AreEqual(PieceSetSignature(firstPieces.PlayerPieceSet), PieceSetSignature(secondPieces.PlayerPieceSet));
                Assert.AreEqual(firstPieces.PromotionTypes, secondPieces.PromotionTypes);
                CollectionAssert.AreEqual(PieceNames(firstPockets), PieceNames(secondPockets));
            }
        }

        private static void ConfigureStableGeneration()
        {
            ConfigureStableSlotDataOnly();
            new ApmwChessGame().earlyPopulatePieceTypes();
        }

        private static ItemHandler ConfigureStableGenerationWithPieceUpgradePriorities(
            MutableReceivedItemsHelper receivedItems,
            JObject pieceUpgradePriorities,
            bool populatePieceTypes = true)
        {
            var slotData = ApmwFuzzCase.DefaultStandard().BuildSlotData();
            slotData[ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure;
            slotData[ApmwConstants.SlotKeyPieceUpgradePreferences] = pieceUpgradePriorities;
            ApmwConfig.getInstance().Instantiate(slotData);
            ApmwConfig.getInstance().seed();
            if (populatePieceTypes)
                new ApmwChessGame().earlyPopulatePieceTypes();
            return new ItemHandler(receivedItems);
        }

        private static JObject PriorityMapWith(string actionName, int priority)
        {
            return PriorityMapWith(actionName, priority, new Dictionary<string, int>());
        }

        private static JObject PriorityMapWith(
            string actionName,
            int priority,
            IDictionary<string, int> additionalPriorities)
        {
            var priorities = new Dictionary<string, int>
            {
                [actionName] = priority,
            };
            foreach (var additionalPriority in additionalPriorities)
                priorities[additionalPriority.Key] = additionalPriority.Value;
            return JObject.FromObject(priorities);
        }

        private static void ConfigureStableSlotDataOnly()
        {
            ApmwConfig.getInstance().Instantiate(ApmwFuzzCase.DefaultStandard().BuildSlotData());
            ApmwConfig.getInstance().seed();
        }

        private static bool IsArmyConstrainedGeneratedPiece(ApmwCore core, PieceType piece)
        {
            return piece != null &&
                (core.minors.Contains(piece) ||
                 core.majors.Contains(piece) ||
                 core.jacks.Contains(piece) ||
                 core.queens.Contains(piece) ||
                 core.amazons.Contains(piece));
        }

        private static int CountFrom(IEnumerable<PieceType> pieces, ISet<PieceType> set)
        {
            return pieces.Count(piece => piece != null && set.Contains(piece));
        }

        private static int CountFamily(IEnumerable<PieceType> pieces, string familyName)
        {
            var core = ApmwCore.getInstance();
            return pieces.Count(piece => FamilyContains(core, familyName, piece));
        }

        private static bool FamilyContains(ApmwCore core, string familyName, PieceType piece)
        {
            if (piece == null)
                return false;

            switch (familyName)
            {
                case "minor":
                    return core.minors.Contains(piece);
                case "major":
                    return core.majors.Contains(piece);
                case "jack":
                    return core.jacks.Contains(piece);
                case "queen":
                    return core.queens.Contains(piece);
                case "amazon":
                    return core.amazons.Contains(piece);
                default:
                    throw new ArgumentOutOfRangeException(nameof(familyName), familyName, "Unknown non-pawn piece family.");
            }
        }

        private static string PieceSetSignature(IReadOnlyDictionary<KeyValuePair<int, int>, PieceType> pieces)
        {
            return string.Join("|", pieces
                .OrderBy(item => item.Key.Key)
                .ThenBy(item => item.Key.Value)
                .Select(item => item.Key.Key + "," + item.Key.Value + ":" + item.Value.Name));
        }

        private static string[] PieceNames(IEnumerable<PieceType> pieces)
        {
            return pieces.Select(piece => piece == null ? "<null>" : piece.Name).ToArray();
        }

        private static void ResetSingletons()
        {
            ApmwCore._instance = null;
            ApmwConfig._instance = null;
        }

        private sealed class TestPiece : PieceType
        {
            public TestPiece(string name, string notation, int value)
                : base(name, name, notation, value, value)
            {
            }
        }

        private sealed class MutableReceivedItemsHelper : IReceivedItemsHelper
        {
            private readonly Dictionary<string, long> itemIdsByName = new Dictionary<string, long>();
            private readonly Dictionary<long, string> itemNamesById = new Dictionary<long, string>();
            private readonly List<ItemInfo> items = new List<ItemInfo>();
            private long nextItemId = 100000;

            public event ItemReceivedHandler ItemReceived;

            public ReadOnlyCollection<ItemInfo> AllItemsReceived
            {
                get { return new ReadOnlyCollection<ItemInfo>(items); }
            }

            public int Index { get { return 0; } }

            public MutableReceivedItemsHelper Add(string itemName, int count = 1)
            {
                if (!itemIdsByName.TryGetValue(itemName, out long itemId))
                {
                    itemId = nextItemId++;
                    itemIdsByName[itemName] = itemId;
                    itemNamesById[itemId] = itemName;
                }

                for (int i = 0; i < count; i++)
                    items.Add(CreateItemInfo(itemId));

                return this;
            }

            public void RaiseItemReceived()
            {
                ItemReceived?.Invoke(null);
            }

            public bool Any()
            {
                return items.Count > 0;
            }

            public ItemInfo DequeueItem()
            {
                if (items.Count == 0)
                    return null;
                var item = items[0];
                items.RemoveAt(0);
                return item;
            }

            public string GetItemName(long itemId, string game)
            {
                return itemNamesById.TryGetValue(itemId, out string itemName) ? itemName : null;
            }

            public ItemInfo PeekItem()
            {
                return items.Count == 0 ? null : items[0];
            }

            private static ItemInfo CreateItemInfo(long itemId)
            {
                return new ItemInfo(
                    new NetworkItem
                    {
                        Item = itemId,
                        Player = 1,
                    },
                    ApmwConstants.TrackerName,
                    ApmwConstants.TrackerName,
                    null,
                    new PlayerInfo(
                        0,
                        1,
                        "ApmwItemHandlerCharacterizationTests",
                        "ApmwItemHandlerCharacterizationTests",
                        ApmwConstants.TrackerName,
                        new NetworkSlot[0],
                        new int[0]));
            }
        }
    }
}
