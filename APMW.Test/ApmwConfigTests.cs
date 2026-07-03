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
        [DataRow(FairyPawnUpgrades.Off, "new-pawn,more-pawn,better-pawn,major-to-queen")]
        [DataRow(FairyPawnUpgrades.Pool, "new-pawn,pool-pawn-upgrade,more-pawn,better-pawn,major-to-queen")]
        [DataRow(FairyPawnUpgrades.Max, "new-pawn,better-pawn,more-pawn,major-to-queen")]
        [DataRow(FairyPawnUpgrades.SuperMax, "new-pawn,better-pawn,more-pawn,major-to-queen")]
        [DataRow(FairyPawnUpgrades.Configure, "new-pawn,more-pawn,better-pawn,major-to-queen")]
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
                new[] { "better-pawn", "new-pawn", "major-to-queen" },
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
                new[] { "new-pawn", "pool-pawn-upgrade", "more-pawn", "better-pawn", "major-to-queen" },
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
                new[] { "new-pawn", "more-pawn", "better-pawn", "major-to-queen" },
                config.PieceUpgradePreferences);
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
