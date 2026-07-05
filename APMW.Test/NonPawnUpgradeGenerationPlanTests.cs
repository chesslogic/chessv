using System.Collections.Generic;
using System.Linq;
using Archipelago.APChessV;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ChessV.Test
{
    /// <summary>
    /// Focused unit coverage for <see cref="NonPawnUpgradeGeneration.Plan(ApmwCore, ApmwConfig)"/>
    /// (Legacy mode's non-pawn upgrade planner), bypassing the full <see cref="ItemHandler"/>
    /// pipeline -- mirrors FundamentalSlotGraduationPlannerTests' style. Specifically proves the
    /// per-unit weighted-draw restructuring (sharing WeightedTieBreak with
    /// FundamentalSlotGraduationPlanner) actually resolves genuine ties -- two actions sharing
    /// both priority and SourceFamily -- via seeded weighted competition instead of one
    /// deterministically claiming its whole target budget before the other gets a turn, as the
    /// old fixed-declaration-order sequential loop did.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class NonPawnUpgradeGenerationPlanTests
    {
        [TestInitialize]
        public void Setup()
        {
            ResetSingletons();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ResetSingletons();
        }

        [TestMethod]
        public void Plan_TiedSharedSourceActionsSplitScarceSupplyDifferentlyAcrossSeeds()
        {
            // 4 minors must cover two competing targets (3 major-budget + 3 jack-budget = 6
            // desired, only 4 available) -- genuine scarcity where the split is forced to vary.
            // Before the per-unit restructuring, MinorToMajor (earlier in ValidPieceUpgradeActions
            // declaration order) would deterministically claim its full budget of 3 first every
            // time, always leaving MinorToJack exactly 1 -- regardless of seed.
            var priorities = new Dictionary<string, int>
            {
                [ApmwConstants.PieceUpgradeActions.MinorToMajor] = 10,
                [ApmwConstants.PieceUpgradeActions.MinorToJack] = 10,
            };
            JObject priorityMap = JObject.FromObject(priorities);

            var splits = new HashSet<(int Major, int Jack)>();
            for (int i = 0; i < 30; i++)
            {
                ApmwConfig config = ConfigurePlanner(priorityMap, pocketSeed: 4001 + i, pawnSeed: 5507 + i * 7);
                ApmwCore core = ConfigureCore(minors: 4, majors: 3, jacks: 3);

                NonPawnGenerationPlan plan = NonPawnUpgradeGeneration.Plan(core, config);
                int majorUpgrades = RequestedUpgradesFor(plan, ApmwConstants.PieceUpgradeActions.MinorToMajor);
                int jackUpgrades = RequestedUpgradesFor(plan, ApmwConstants.PieceUpgradeActions.MinorToJack);

                Assert.AreEqual(
                    4,
                    majorUpgrades + jackUpgrades,
                    "all 4 scarce minors must be allocated to exactly one of the two tied actions");
                Assert.IsTrue(majorUpgrades >= 1 && majorUpgrades <= 3, "major upgrades must respect its own target budget of 3");
                Assert.IsTrue(jackUpgrades >= 1 && jackUpgrades <= 3, "jack upgrades must respect its own target budget of 3");
                splits.Add((majorUpgrades, jackUpgrades));
            }

            Assert.IsTrue(
                splits.Count > 1,
                "seeded tie-breaking should split the scarce shared source differently across many different seeds, got: "
                    + string.Join(", ", splits));
        }

        [TestMethod]
        public void Plan_ZeroProportionExcludesActionFromTiedSharedSourceDraw()
        {
            // Deterministic counterpart to the statistical test above: an explicit weight of 0
            // must fully exclude minor-to-jack from every contested draw, so minor-to-major (the
            // only nonzero-weight competitor) claims every scarce minor, on every seed -- no
            // flakiness. Major's own target budget (10) is deliberately set above the total minor
            // supply (4) so major can never itself run dry mid-run -- see the companion
            // "last resort" test below for what happens when the zero-weight action's competitor
            // *does* run out first.
            var priorities = new Dictionary<string, int>
            {
                [ApmwConstants.PieceUpgradeActions.MinorToMajor] = 10,
                [ApmwConstants.PieceUpgradeActions.MinorToJack] = 10,
            };
            var proportions = new Dictionary<string, double>
            {
                [ApmwConstants.PieceUpgradeActions.MinorToJack] = 0,
            };
            JObject priorityMap = JObject.FromObject(priorities);
            JObject proportionMap = JObject.FromObject(proportions);

            for (int i = 0; i < 15; i++)
            {
                ApmwConfig config = ConfigurePlanner(
                    priorityMap,
                    pocketSeed: 7001 + i,
                    pawnSeed: 8209 + i * 7,
                    proportions: proportionMap);
                ApmwCore core = ConfigureCore(minors: 4, majors: 10, jacks: 3);

                NonPawnGenerationPlan plan = NonPawnUpgradeGeneration.Plan(core, config);
                int majorUpgrades = RequestedUpgradesFor(plan, ApmwConstants.PieceUpgradeActions.MinorToMajor);
                int jackUpgrades = RequestedUpgradesFor(plan, ApmwConstants.PieceUpgradeActions.MinorToJack);

                Assert.AreEqual(4, majorUpgrades, "seed index " + i);
                Assert.AreEqual(0, jackUpgrades, "seed index " + i);
            }
        }

        [TestMethod]
        public void Plan_ZeroProportionActionStillAppliesAsLastResortOnceItsCompetitorIsExhausted()
        {
            // Discovered/locked-in design property: a proportion of 0 only ever means "always
            // lose while a competitor is still viable" -- it is not the same as priority <= 0
            // ("fully disabled"). Once minor-to-major (weight 1) exhausts its own target budget
            // of 3, minor-to-jack becomes the sole viable action for the 4th remaining minor and
            // -- despite its weight of 0 -- still applies, rather than leaving that minor
            // stranded/unconverted. This is deterministic (no seed dependence) because minor-to-
            // major wins every contested draw with probability 1 (its competitor's weight is
            // always exactly 0 whenever both are viable).
            var priorities = new Dictionary<string, int>
            {
                [ApmwConstants.PieceUpgradeActions.MinorToMajor] = 10,
                [ApmwConstants.PieceUpgradeActions.MinorToJack] = 10,
            };
            var proportions = new Dictionary<string, double>
            {
                [ApmwConstants.PieceUpgradeActions.MinorToJack] = 0,
            };
            JObject priorityMap = JObject.FromObject(priorities);
            JObject proportionMap = JObject.FromObject(proportions);

            for (int i = 0; i < 15; i++)
            {
                ApmwConfig config = ConfigurePlanner(
                    priorityMap,
                    pocketSeed: 9001 + i,
                    pawnSeed: 2609 + i * 7,
                    proportions: proportionMap);
                ApmwCore core = ConfigureCore(minors: 4, majors: 3, jacks: 3);

                NonPawnGenerationPlan plan = NonPawnUpgradeGeneration.Plan(core, config);
                int majorUpgrades = RequestedUpgradesFor(plan, ApmwConstants.PieceUpgradeActions.MinorToMajor);
                int jackUpgrades = RequestedUpgradesFor(plan, ApmwConstants.PieceUpgradeActions.MinorToJack);

                Assert.AreEqual(3, majorUpgrades, "seed index " + i);
                Assert.AreEqual(1, jackUpgrades, "seed index " + i);
            }
        }

        private static int RequestedUpgradesFor(NonPawnGenerationPlan plan, string actionName)
        {
            var planned = plan.UpgradeActions.FirstOrDefault(action => action.Metadata.ActionName == actionName);
            return planned == null ? 0 : planned.RequestedUpgrades;
        }

        private static ApmwConfig ConfigurePlanner(
            JObject priorities,
            int pocketSeed,
            int pawnSeed,
            JObject proportions = null)
        {
            ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
            builder.PocketSeed = pocketSeed;
            builder.PawnSeed = pawnSeed;

            Dictionary<string, object> slotData = builder.Build().BuildSlotData();
            slotData[ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure;
            slotData[ApmwConstants.SlotKeyPieceUpgradePreferences] = priorities;
            if (proportions != null)
                slotData[ApmwConstants.SlotKeyPieceUpgradeProportions] = proportions;

            ApmwConfig config = ApmwConfig.getInstance();
            config.Instantiate(slotData);
            config.seed();
            return config;
        }

        private static ApmwCore ConfigureCore(int minors, int majors, int jacks)
        {
            ApmwCore core = ApmwCore.getInstance();
            core.foundMinors = minors;
            core.foundMajors = majors;
            core.foundJacks = jacks;
            core.foundQueens = 0;
            core.foundAmazons = 0;
            return core;
        }

        private static void ResetSingletons()
        {
            ApmwCore._instance = null;
            ApmwConfig._instance = null;
        }
    }
}
