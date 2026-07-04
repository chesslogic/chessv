using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.APChessV;

namespace ChessV.Test
{
    [TestClass]
    public class ApmwConfigTests
    {
        private sealed class AlwaysZeroRandom : Random
        {
            public override int Next(int maxValue)
            {
                return 0;
            }
        }

        [TestMethod]
        public void Seed_UsesDeterministicChaosSeedFromSlotData()
        {
            const int chaosSeed = 123456;
            var slotData = new Dictionary<string, object>
            {
                ["piece_types"] = (int)PieceTypes.Chaos,
                ["piece_locations"] = (int)PieceLocations.Chaos,
                [ApmwConfig.DeterministicChaosSeedSlotKeyForTest] = chaosSeed,
            };

            var config = new ApmwConfig();
            config.Instantiate(slotData);
            config.seed();

            var expected = new Random(chaosSeed);
            var firstStageSeeds = CaptureSeeds(config);
            CollectionAssert.AreEqual(
                new[]
                {
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                    expected.Next(),
                },
                firstStageSeeds);

            config.seed();
            CollectionAssert.AreEqual(firstStageSeeds, CaptureSeeds(config));
        }

        [TestMethod]
        public void Distribute_UsesProvidedRandom()
        {
            var result = ApmwConfig.distribute(
                new List<string> { "A", "B", "C" },
                3,
                new AlwaysZeroRandom());

            CollectionAssert.AreEqual(
                new[] { "B", "C", "A" },
                Enumerable.Range(0, 3).Select(index => result[index]).ToArray());
        }

        [DataTestMethod]
        [DataRow(FairyPawnUpgrades.Off, "new-pawn,more-pawn,better-pawn,major-to-queen,pool-pawn-upgrade,minor-to-major,major-to-jack,minor-to-jack,jack-to-queen,queen-to-amazon")]
        [DataRow(FairyPawnUpgrades.Pool, "new-pawn,pool-pawn-upgrade,more-pawn,better-pawn,major-to-queen,minor-to-major,major-to-jack,minor-to-jack,jack-to-queen,queen-to-amazon")]
        [DataRow(FairyPawnUpgrades.Max, "new-pawn,better-pawn,more-pawn,major-to-queen,pool-pawn-upgrade,minor-to-major,major-to-jack,minor-to-jack,jack-to-queen,queen-to-amazon")]
        [DataRow(FairyPawnUpgrades.SuperMax, "new-pawn,better-pawn,more-pawn,major-to-queen,pool-pawn-upgrade,minor-to-major,major-to-jack,minor-to-jack,jack-to-queen,queen-to-amazon")]
        [DataRow(FairyPawnUpgrades.Configure, "new-pawn,more-pawn,better-pawn,major-to-queen,pool-pawn-upgrade,minor-to-major,major-to-jack,minor-to-jack,jack-to-queen,queen-to-amazon")]
        public void Instantiate_DerivesPieceUpgradePreferencesFromLegacyMode(
            FairyPawnUpgrades legacyMode,
            string expectedCsv)
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)legacyMode,
            });

            CollectionAssert.AreEqual(
                expectedCsv.Split(','),
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void Instantiate_UsesResolvedPieceUpgradePreferencesWhenPresent()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Pool,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    new JArray("better-pawn", "not-real", "new-pawn", "major-to-queen"),
            });

            CollectionAssert.AreEqual(
                new[]
                {
                    "better-pawn",
                    "new-pawn",
                    "major-to-queen",
                    "more-pawn",
                    "pool-pawn-upgrade",
                    "minor-to-major",
                    "major-to-jack",
                    "minor-to-jack",
                    "jack-to-queen",
                    "queen-to-amazon",
                },
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void Instantiate_DerivesLegacyPreferencesWhenResolvedListIsEmpty()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Pool,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] = new JArray(),
            });

            CollectionAssert.AreEqual(
                new[] { "new-pawn", "pool-pawn-upgrade", "more-pawn", "better-pawn", "major-to-queen", "minor-to-major", "major-to-jack", "minor-to-jack", "jack-to-queen", "queen-to-amazon" },
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void Instantiate_FallsBackToOffPreferencesWhenResolvedListHasNoValidNames()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Pool,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] = new JArray("not-real", "also-not-real"),
            });

            CollectionAssert.AreEqual(
                new[] { "new-pawn", "more-pawn", "better-pawn", "major-to-queen", "pool-pawn-upgrade", "minor-to-major", "major-to-jack", "minor-to-jack", "jack-to-queen", "queen-to-amazon" },
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void Instantiate_AcceptsFuturePieceUpgradePreferenceActionsAndFiltersInvalidNames()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    new JArray(
                        "minor-to-major",
                        "not-real",
                        "major-to-jack",
                        "minor-to-jack",
                        "jack-to-queen",
                        "queen-to-amazon"),
            });

            CollectionAssert.AreEqual(
                new[]
                {
                    "minor-to-major",
                    "major-to-jack",
                    "minor-to-jack",
                    "jack-to-queen",
                    "queen-to-amazon",
                    "new-pawn",
                    "more-pawn",
                    "better-pawn",
                    "pool-pawn-upgrade",
                    "major-to-queen",
                },
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void PieceUpgradePreferenceHelpers_ReflectResolvedOrderAndMajorToQueen()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    new JArray("new-pawn", "better-pawn", "more-pawn"),
            });

            Assert.IsTrue(config.IsPieceUpgradeActionPreferredBefore(
                ApmwConstants.PieceUpgradeActions.BetterPawn,
                ApmwConstants.PieceUpgradeActions.MorePawn));
            Assert.IsTrue(config.MajorToQueenUpgradeEnabled);
            Assert.AreEqual(0, config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.MajorToQueen].Priority);

            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    new JArray("new-pawn", "major-to-queen"),
            });

            Assert.IsTrue(config.MajorToQueenUpgradeEnabled);
        }

        [TestMethod]
        public void Instantiate_ResolvesPieceUpgradePriorityMapWithNeutralOmissionsAndDisabledActions()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure,
                [ApmwConstants.SlotKeyPieceUpgradePreferences] =
                    JObject.FromObject(new Dictionary<string, int>
                    {
                        [ApmwConstants.PieceUpgradeActions.BetterPawn] = 2,
                        [ApmwConstants.PieceUpgradeActions.MajorToQueen] = -1,
                        ["not-real"] = 100,
                    }),
            });

            Assert.IsTrue(config.IsPieceUpgradeActionEnabled(ApmwConstants.PieceUpgradeActions.NewPawn));
            Assert.IsFalse(config.IsPieceUpgradeActionEnabled(ApmwConstants.PieceUpgradeActions.MajorToQueen));
            Assert.AreEqual(0, config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.NewPawn].Priority);
            Assert.AreEqual(2, config.PieceUpgradeActions[ApmwConstants.PieceUpgradeActions.BetterPawn].Priority);
            Assert.IsTrue(config.IsPieceUpgradeActionPreferredBefore(
                ApmwConstants.PieceUpgradeActions.BetterPawn,
                ApmwConstants.PieceUpgradeActions.NewPawn));
            Assert.IsFalse(config.PieceUpgradeActions.ContainsKey("not-real"));
            CollectionAssert.AreEqual(
                new[]
                {
                    ApmwConstants.PieceUpgradeActions.BetterPawn,
                    ApmwConstants.PieceUpgradeActions.NewPawn,
                    ApmwConstants.PieceUpgradeActions.MorePawn,
                    ApmwConstants.PieceUpgradeActions.PoolPawnUpgrade,
                    ApmwConstants.PieceUpgradeActions.MinorToMajor,
                    ApmwConstants.PieceUpgradeActions.MajorToJack,
                    ApmwConstants.PieceUpgradeActions.MinorToJack,
                    ApmwConstants.PieceUpgradeActions.JackToQueen,
                    ApmwConstants.PieceUpgradeActions.QueenToAmazon,
                },
                config.PieceUpgradePreferences);
        }

        [TestMethod]
        public void Instantiate_DefaultsToLegacyProgressionItemization()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>());

            Assert.AreEqual(ProgressionItemization.Legacy, config.ProgressionItemization);
            Assert.IsFalse(config.UsesFundamentalProgressionItemization);
            Assert.AreEqual(ApmwConfig.DefaultMaterialItemValue, config.materialItemValue);
            Assert.AreEqual(ApmwConfig.DefaultCastlingLocationCount, config.castlingLocationCount);
        }

        [DataTestMethod]
        [DataRow("fundamental")]
        [DataRow("Fundamental")]
        [DataRow((int)ProgressionItemization.Fundamental)]
        public void Instantiate_ParsesFundamentalProgressionItemization(object rawItemization)
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyProgressionItemization] = rawItemization,
                [ApmwConstants.SlotKeyMaterialItemValue] = 500,
                [ApmwConstants.SlotKeyCastlingLocationCount] = 1,
            });

            Assert.AreEqual(ProgressionItemization.Fundamental, config.ProgressionItemization);
            Assert.IsTrue(config.UsesFundamentalProgressionItemization);
            Assert.AreEqual(500, config.materialItemValue);
            Assert.AreEqual(1, config.castlingLocationCount);
        }

        [TestMethod]
        public void Instantiate_InvalidProgressionItemizationFallsBackToLegacyAndClampsCounts()
        {
            var config = new ApmwConfig();
            config.Instantiate(new Dictionary<string, object>
            {
                [ApmwConstants.SlotKeyProgressionItemization] = "future-mode",
                [ApmwConstants.SlotKeyMaterialItemValue] = 0,
                [ApmwConstants.SlotKeyCastlingLocationCount] = -3,
            });

            Assert.AreEqual(ProgressionItemization.Legacy, config.ProgressionItemization);
            Assert.IsFalse(config.UsesFundamentalProgressionItemization);
            Assert.AreEqual(1, config.materialItemValue);
            Assert.AreEqual(0, config.castlingLocationCount);
        }

        private static int[] CaptureSeeds(ApmwConfig config)
        {
            return new[]
            {
                config.pocketSeed,
                config.pawnSeed,
                config.minorSeed,
                config.majorSeed,
                config.queenSeed,
                config.pawnLocSeed,
                config.minorLocSeed,
                config.majorLocSeed,
                config.queenLocSeed,
            };
        }
    }
}
