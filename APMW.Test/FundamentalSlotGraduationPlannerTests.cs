using System;
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
    /// Focused unit coverage for <see cref="FundamentalSlotGraduationPlanner"/> itself (bypassing
    /// the full <see cref="ItemHandler"/> pipeline), proving the properties the redesign was
    /// specifically meant to guarantee: determinism, prefix-stability (recomputing with a larger
    /// budget only extends a smaller-budget roster, never reshuffles it), strict priority-contract
    /// correctness (including falling back past an eligible-but-unaffordable higher-priority
    /// action), Castler-lock exclusion, and genuine seeded randomization on ties.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class FundamentalSlotGraduationPlannerTests
    {
        private const int NumFiles = 8;

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
        public void Plan_IsDeterministic_ForIdenticalInputs()
        {
            ApmwConfig config = ConfigurePlanner(PlannedChainPriorities());
            ApmwCore core = ConfigureCore(chessmen: 10, material: 4000);

            PieceGenerationAllocation first = FundamentalSlotGraduationPlanner.Plan(core, config, NumFiles);
            PieceGenerationAllocation second = FundamentalSlotGraduationPlanner.Plan(core, config, NumFiles);

            AssertAllocationsEqual(first, second);
        }

        /// <summary>
        /// KNOWN LIMITATION -- documents a real gap discovered while writing this coverage,
        /// flagged to @chesslogic rather than silently patched (see plan.md's "Discovered gap"
        /// section for the full writeup and options). Growing *Material* alone (previous test)
        /// is safely prefix-stable: every slot shares the same fixed-size pool, so more material
        /// can only push that same pool further, never less far. But growing *Chessmen* changes
        /// the size of the pool competing for that material: every slot starts at Pawn and tier
        /// transitions are processed as "waves" (all eligible slots at the current highest
        /// priority/affordable action advance together before the scan considers the next tier),
        /// so adding more slots to a *fixed* material budget dilutes how far the whole group gets
        /// -- including slots that "existed" in a smaller-Chessmen run. This is a real player-
        /// visible regression risk: receiving one more Chessmen item (with no new Material) can
        /// make previously-generated Queens/Amazons disappear, replaced by lower tiers, purely
        /// because more slots are now sharing the same budget. With DistinctPriorities (no ties)
        /// and material=5000 at the same seed: 5 Chessmen fully resolves to 3 Queens + 2 Amazons
        /// (all material spent reaching the top of the chain for every slot); 9 Chessmen resolves
        /// to 2 Majors + 7 Jacks and *zero* Queens/Amazons -- a complete reversal, not merely a
        /// smaller extension. This test pins today's actual (reshuffling) behavior so any future
        /// fix to this is a deliberate, visible change rather than a silent regression.
        /// </summary>
        [TestMethod]
        public void Plan_GrowingChessmenWithFixedMaterial_CurrentlyDilutesSharedBudgetAcrossAllSlots()
        {
            JObject priorities = DistinctPriorities();

            ApmwConfig smallerConfig = ConfigurePlanner(priorities, pocketSeed: 4242, pawnSeed: 1717);
            ApmwCore smallerCore = ConfigureCore(chessmen: 5, material: 5000);
            PieceGenerationAllocation smaller = FundamentalSlotGraduationPlanner.Plan(smallerCore, smallerConfig, NumFiles);

            ApmwConfig largerConfig = ConfigurePlanner(priorities, pocketSeed: 4242, pawnSeed: 1717);
            ApmwCore largerCore = ConfigureCore(chessmen: 9, material: 5000);
            PieceGenerationAllocation larger = FundamentalSlotGraduationPlanner.Plan(largerCore, largerConfig, NumFiles);

            Assert.AreEqual(3, smaller.NonPawnCount(NonPawnPieceFamily.Queen));
            Assert.AreEqual(2, smaller.NonPawnCount(NonPawnPieceFamily.Amazon));

            Assert.AreEqual(2, larger.NonPawnCount(NonPawnPieceFamily.Major));
            Assert.AreEqual(7, larger.NonPawnCount(NonPawnPieceFamily.Jack));
            Assert.AreEqual(
                0,
                larger.NonPawnCount(NonPawnPieceFamily.Queen) + larger.NonPawnCount(NonPawnPieceFamily.Amazon),
                "documents today's actual behavior: the smaller run's 3 Queens + 2 Amazons are completely gone " +
                    "once more Chessmen slots dilute the same fixed Material budget -- see class-level remarks");
        }

        [TestMethod]
        public void Plan_WithDistinctPriorities_GrowingMaterialExtendsRatherThanReshufflesRoster()
        {
            JObject priorities = DistinctPriorities();

            ApmwConfig smallerConfig = ConfigurePlanner(priorities, pocketSeed: 8123, pawnSeed: 3391);
            ApmwCore smallerCore = ConfigureCore(chessmen: 6, material: 600);
            PieceGenerationAllocation smaller = FundamentalSlotGraduationPlanner.Plan(smallerCore, smallerConfig, NumFiles);

            ApmwConfig largerConfig = ConfigurePlanner(priorities, pocketSeed: 8123, pawnSeed: 3391);
            ApmwCore largerCore = ConfigureCore(chessmen: 6, material: 3000);
            PieceGenerationAllocation larger = FundamentalSlotGraduationPlanner.Plan(largerCore, largerConfig, NumFiles);

            AssertAppliedCountsExtend(smaller, larger, chessmenSmaller: 6, chessmenLarger: 6);
        }

        [TestMethod]
        public void Plan_PrefersHigherPriorityActionEvenWhenCheaperLowerPriorityActionWouldFitMoreOften()
        {
            // pawn-to-minor(10, cost 200) drains every slot to Minor first; minor-to-jack(6, cost
            // 400) then outranks the cheaper minor-to-major(5, cost 185) despite the latter being
            // affordable more often -- priority must dominate cost-driven "fits more often" logic.
            var priorities = new Dictionary<string, int>
            {
                [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 10,
                [ApmwConstants.PieceUpgradeActions.MinorToJack] = 6,
                [ApmwConstants.PieceUpgradeActions.MinorToMajor] = 5,
            };
            ApmwConfig config = ConfigurePlanner(JObject.FromObject(priorities));
            ApmwCore core = ConfigureCore(chessmen: 4, material: 4 * 200 + 400); // exactly one minor-to-jack after draining

            PieceGenerationAllocation allocation = FundamentalSlotGraduationPlanner.Plan(core, config, NumFiles);

            Assert.AreEqual(0, allocation.PawnSlots);
            Assert.AreEqual(3, allocation.NonPawnCount(NonPawnPieceFamily.Minor));
            Assert.AreEqual(
                0,
                allocation.NonPawnCount(NonPawnPieceFamily.Major),
                "the cheaper, lower-priority action must not fire ahead of the higher-priority one");
            Assert.AreEqual(1, allocation.NonPawnCount(NonPawnPieceFamily.Jack));
            Assert.AreEqual(0, allocation.InitialSpareMaterial);
        }

        [TestMethod]
        public void Plan_FallsBackPastEligibleButUnaffordableTopPriorityAction()
        {
            // Once all 3 slots reach Major (pawn-to-minor + minor-to-major both fully afforded),
            // major-to-queen(10, cost 415) is eligible (3 Majors exist) but too expensive for the
            // 215 material left over -- the scan must fall back to the cheaper, lower-priority
            // major-to-jack(5, cost 215) rather than stalling just because the top pick is unaffordable.
            var priorities = new Dictionary<string, int>
            {
                [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 20,
                [ApmwConstants.PieceUpgradeActions.MinorToMajor] = 15,
                [ApmwConstants.PieceUpgradeActions.MajorToQueen] = 10,
                [ApmwConstants.PieceUpgradeActions.MajorToJack] = 5,
            };
            ApmwConfig config = ConfigurePlanner(JObject.FromObject(priorities));
            ApmwCore core = ConfigureCore(chessmen: 3, material: 3 * (200 + 185) + 215);

            PieceGenerationAllocation allocation = FundamentalSlotGraduationPlanner.Plan(core, config, NumFiles);

            Assert.AreEqual(0, allocation.PawnSlots);
            Assert.AreEqual(0, allocation.NonPawnCount(NonPawnPieceFamily.Minor));
            Assert.AreEqual(2, allocation.NonPawnCount(NonPawnPieceFamily.Major));
            Assert.AreEqual(1, allocation.NonPawnCount(NonPawnPieceFamily.Jack));
            Assert.AreEqual(
                0,
                allocation.NonPawnCount(NonPawnPieceFamily.Queen),
                "unaffordable higher-priority action must not block cheaper lower-priority progress");
            Assert.AreEqual(0, allocation.InitialSpareMaterial);
        }

        [TestMethod]
        public void Plan_LockedCastlerMajorsAreNeverFurtherUpgradedOrConsumed()
        {
            var priorities = new Dictionary<string, int>
            {
                [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 10,
                [ApmwConstants.PieceUpgradeActions.MinorToMajor] = 9,
                [ApmwConstants.PieceUpgradeActions.MajorToQueen] = 8,
            };
            ApmwConfig config = ConfigurePlanner(JObject.FromObject(priorities));
            ApmwCore core = ConfigureCore(chessmen: 4, material: 10000, castlers: 2); // ample budget to try to upgrade everything

            PieceGenerationAllocation allocation = FundamentalSlotGraduationPlanner.Plan(core, config, NumFiles);

            Assert.AreEqual(2, allocation.LockedMajorCount);
            Assert.AreEqual(0, allocation.PawnSlots);
            Assert.AreEqual(0, allocation.NonPawnCount(NonPawnPieceFamily.Minor));
            Assert.AreEqual(
                2,
                allocation.NonPawnCount(NonPawnPieceFamily.Major),
                "only the 2 locked Castler majors should remain at Major; non-locked slots must keep advancing");
            Assert.AreEqual(2, allocation.NonPawnCount(NonPawnPieceFamily.Queen));
        }

        [TestMethod]
        public void Plan_MajorSurvivorsDoNotReserveExtraDirectSlotsDuringGeneration()
        {
            var priorities = new Dictionary<string, int>
            {
                [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 10,
                [ApmwConstants.PieceUpgradeActions.MinorToMajor] = 9,
            };
            ApmwConfig config = ConfigurePlanner(JObject.FromObject(priorities));
            ApmwCore core = ConfigureCore(chessmen: 10, material: 10 * (ItemGenerationValues.Minor + (ItemGenerationValues.Major - ItemGenerationValues.Minor)));
            new ApmwChessGame().earlyPopulatePieceTypes();

            PieceGenerationAllocation allocation = FundamentalSlotGraduationPlanner.Plan(core, config, NumFiles);
            var generated = PlayerPieceSetGeneration.Generate(NumFiles).Item1.Values.ToList();

            Assert.AreEqual(10, allocation.NonPawnCount(NonPawnPieceFamily.Major), "planner should resolve all 10 slots to Major");
            Assert.AreEqual(10, CountNonKingPieces(generated), "generation must not create extra physical pieces beyond the planned Chessmen slots");
            Assert.AreEqual(10, CountFrom(generated, core.majors), "Major survivors should come from substitution, not additional direct Major placements");
        }

        [TestMethod]
        public void Plan_TieBetweenActionsAtEqualPriorityProducesSeedDependentVariation()
        {
            // pawn-to-minor and minor-to-major share priority 5 -- a genuine tie (different
            // FromTiers) resolved by seeded weighted random choice, not list/config order.
            var priorities = new Dictionary<string, int>
            {
                [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 5,
                [ApmwConstants.PieceUpgradeActions.MinorToMajor] = 5,
            };
            JObject priorityMap = JObject.FromObject(priorities);

            var outcomes = new HashSet<(int Minor, int Major)>();
            for (int i = 0; i < 40; i++)
            {
                ApmwConfig config = ConfigurePlanner(priorityMap, pocketSeed: 1009 + i, pawnSeed: 2003 + i * 7);
                ApmwCore core = ConfigureCore(chessmen: 12, material: 1200); // stops partway through the chain

                PieceGenerationAllocation allocation = FundamentalSlotGraduationPlanner.Plan(core, config, NumFiles);
                int minor = allocation.NonPawnCount(NonPawnPieceFamily.Minor);
                int major = allocation.NonPawnCount(NonPawnPieceFamily.Major);

                Assert.AreEqual(
                    12,
                    allocation.PawnSlots + minor + major,
                    "every slot must be accounted for in exactly one tier");
                outcomes.Add((minor, major));
            }

            Assert.IsTrue(
                outcomes.Count > 1,
                "seeded tie-breaking should explore more than one outcome across many different seeds, got: "
                    + string.Join(", ", outcomes));
        }

        private static ApmwConfig ConfigurePlanner(JObject priorities, int pocketSeed = 5011, int pawnSeed = 9077)
        {
            ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
            builder.PocketSeed = pocketSeed;
            builder.PawnSeed = pawnSeed;

            Dictionary<string, object> slotData = builder.Build().BuildSlotData();
            slotData[ApmwConstants.SlotKeyProgressionItemization] = "fundamental";
            slotData[ApmwConstants.SlotKeyMaterialItemValue] = ApmwConfig.DefaultMaterialItemValue;
            slotData[ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure;
            slotData[ApmwConstants.SlotKeyPieceUpgradePreferences] = priorities;

            ApmwConfig config = ApmwConfig.getInstance();
            config.Instantiate(slotData);
            config.seed();
            return config;
        }

        private static ApmwCore ConfigureCore(int chessmen, int material, int castlers = 0)
        {
            ApmwCore core = ApmwCore.getInstance();
            core.foundChessmen = chessmen;
            core.foundMaterialBudget = material;
            core.foundCastlers = castlers;
            core.foundConsuls = 0;
            core.foundKingPromotions = 0;
            core.IgnoreCastlersReceived = false;
            return core;
        }

        private static JObject PlannedChainPriorities()
        {
            // @chesslogic's planned final action list (tier-transition subset only -- new-pawn/
            // better-pawn are outside FundamentalSlotGraduationPlanner's scope pending the
            // deferred pawn-quality-unification decision).
            return JObject.FromObject(new Dictionary<string, int>
            {
                [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 6,
                [ApmwConstants.PieceUpgradeActions.MinorToMajor] = 3,
                [ApmwConstants.PieceUpgradeActions.MajorToJack] = 2,
                [ApmwConstants.PieceUpgradeActions.MinorToJack] = 2,
                [ApmwConstants.PieceUpgradeActions.MajorToQueen] = 1,
                [ApmwConstants.PieceUpgradeActions.JackToQueen] = 1,
                [ApmwConstants.PieceUpgradeActions.QueenToAmazon] = 1,
            });
        }

        private static JObject DistinctPriorities()
        {
            // Every action gets its own priority (no ties) so the whole run is fully
            // deterministic -- isolating prefix-stability from the seeded tie-breaking RNG.
            return JObject.FromObject(new Dictionary<string, int>
            {
                [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 7,
                [ApmwConstants.PieceUpgradeActions.MinorToMajor] = 6,
                [ApmwConstants.PieceUpgradeActions.MajorToJack] = 5,
                [ApmwConstants.PieceUpgradeActions.MinorToJack] = 4,
                [ApmwConstants.PieceUpgradeActions.MajorToQueen] = 3,
                [ApmwConstants.PieceUpgradeActions.JackToQueen] = 2,
                [ApmwConstants.PieceUpgradeActions.QueenToAmazon] = 1,
            });
        }

        private static void AssertAllocationsEqual(PieceGenerationAllocation a, PieceGenerationAllocation b)
        {
            Assert.AreEqual(a.PawnSlots, b.PawnSlots);
            Assert.AreEqual(a.InitialSpareMaterial, b.InitialSpareMaterial);
            Assert.AreEqual(a.LockedMajorCount, b.LockedMajorCount);
            foreach (NonPawnPieceFamily family in Enum.GetValues(typeof(NonPawnPieceFamily)))
                Assert.AreEqual(a.NonPawnCount(family), b.NonPawnCount(family), "family " + family);

            CollectionAssert.AreEquivalent(
                AppliedActionCounts(a).Select(pair => pair.Key + "=" + pair.Value).ToList(),
                AppliedActionCounts(b).Select(pair => pair.Key + "=" + pair.Value).ToList());
        }

        // pawn-to-minor's applied count is never exposed via UpgradeActions (it seeds the very
        // first placeholder rather than substituting an existing one -- see BuildNonPawnPlan), so
        // it's derived here from how many slots ever left the Pawn tier.
        private static Dictionary<string, int> AppliedActionCounts(PieceGenerationAllocation allocation, int chessmenCount)
        {
            Dictionary<string, int> counts = AppliedActionCounts(allocation);
            int pawnToMinorCount = chessmenCount - allocation.PawnSlots - allocation.LockedMajorCount;
            if (pawnToMinorCount > 0)
                counts[ApmwConstants.PieceUpgradeActions.PawnToMinor] = pawnToMinorCount;
            return counts;
        }

        private static Dictionary<string, int> AppliedActionCounts(PieceGenerationAllocation allocation)
        {
            return allocation.PrecomputedNonPawnPlan.UpgradeActions
                .ToDictionary(action => action.Metadata.ActionName, action => action.RequestedUpgrades);
        }

        private static void AssertAppliedCountsExtend(
            PieceGenerationAllocation smaller,
            PieceGenerationAllocation larger,
            int chessmenSmaller,
            int chessmenLarger)
        {
            Dictionary<string, int> smallerCounts = AppliedActionCounts(smaller, chessmenSmaller);
            Dictionary<string, int> largerCounts = AppliedActionCounts(larger, chessmenLarger);

            foreach (KeyValuePair<string, int> pair in smallerCounts)
            {
                int largerValue;
                largerCounts.TryGetValue(pair.Key, out largerValue);
                Assert.IsTrue(
                    largerValue >= pair.Value,
                    string.Format(
                        "action {0}: smaller-budget count {1} should never exceed larger-budget count {2} (recomputing with more Chessmen/Material must only extend, never reshuffle, the smaller roster)",
                        pair.Key,
                        pair.Value,
                        largerValue));
            }
        }

        private static void ResetSingletons()
        {
            ApmwCore._instance = null;
            ApmwConfig._instance = null;
        }

        private static int CountFrom(IEnumerable<PieceType> pieces, ISet<PieceType> set)
        {
            return pieces.Count(piece => piece != null && set.Contains(piece));
        }

        private static int CountNonKingPieces(IEnumerable<PieceType> pieces)
        {
            var kings = ApmwCore.getInstance().kings;
            return pieces.Count(piece => piece != null && (kings == null || !kings.Contains(piece)));
        }
    }
}
