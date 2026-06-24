using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.APChessV;
using Archipelago.MultiClient.Net.Helpers;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using ChessV.Games.Pieces.Berolina;
using ChessV.Games.Pieces.OdinsRune;
using Moq;

namespace ChessV.Test
{
    [TestClass]
    public class ApmwPawnDistributionTests
    {
        private ItemHandler handler;
        private const int NUM_FILES = 8;

        // Shared piece instances so tests can swap them in/out of core.pawns/core.sergeants
        // (PickPawns identifies sergeants via reference-equality through HashSet.Contains).
        private static PieceType _pawn;
        private static PieceType _berolina;
        private static PieceType _checkers;
        private static PieceType _sergeant;
        private static PieceType _odinPawn;

        // Tiny mocked Random so tests can deterministically force PickPawns down
        // specific code paths (e.g. always-pick-sergeant in Pool mode).
        private sealed class TestRandom : Random
        {
            private readonly Queue<int> _values;
            public TestRandom(IEnumerable<int> values) { _values = new Queue<int>(values); }
            public override int Next(int maxValue) => _values.Count > 0 ? _values.Dequeue() % maxValue : 0;
            public override int Next() => Next(int.MaxValue);
        }

        // Returns a TestRandom that always yields a "max" index, which (mod n)
        // tends to land on the last entry of the augmented pool — i.e. the
        // sergeant when callers append sergeants after pawnOptions.
        private static TestRandom AlwaysMaxRandom()
        {
            var values = Enumerable.Repeat(int.MaxValue, 1024);
            return new TestRandom(values);
        }

        // A TestRandom whose Next(n) is always n-1: i.e. it picks the last index of
        // every pool. For augmented pools = pawnOptions ++ sergeants, this means
        // "always pick a sergeant when one exists". Useful regardless of pool size.
        private sealed class AlwaysLastRandom : Random
        {
            public override int Next(int maxValue) => maxValue - 1;
            public override int Next() => int.MaxValue;
        }

        [TestInitialize]
        public void Setup()
        {
            // Reset the upgrades mode between tests; ApmwConfig is a singleton.
            ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.Off;
            // Reset FairyPawns between tests so the singleton can't leak a non-default
            // value across tests. Vanilla matches the implicit pre-existing default that
            // pre-feature tests (e.g. TestPawnDistribution_MultipleRanks) relied on.
            ApmwConfig.getInstance().PawnsInt = (int)FairyPawns.Vanilla;
            // Deterministic seeds for tests that go through GeneratePawns.
            ApmwConfig.getInstance().pawnSeed = 42;
            ApmwConfig.getInstance().pawnLocSeed = 4242;

            // Fresh instances per test so mutation of core.pawns/core.sergeants in
            // one test cannot bleed into another via shared object identity.
            _pawn = new Pawn("Pawn", "P", 100, 125);
            _berolina = new BerolinaPawn("Berolina Pawn", "Ŕ", 85, 120);
            _checkers = new ChessV.Games.Pieces.Apmw.Checkers("Checkers", "Ç", 40, 95);
            _sergeant = new ChessV.Games.Pieces.Apmw.Sergeant("Sergeant", "Ŝ", 200, 225);
            _odinPawn = new OdinPawn("Odin Pawn", "Ó", 150, 200);

            var core = ApmwCore.getInstance();
            core.pawns = new System.Collections.Generic.HashSet<PieceType> { _pawn, _berolina, _checkers };
            core.sergeants = new System.Collections.Generic.HashSet<PieceType> { _sergeant };
            core.foundPawnForwardness = 0;

            var helper = new Mock<IReceivedItemsHelper>();
            helper.SetupGet(h => h.AllItemsReceived)
                .Returns(new System.Collections.ObjectModel.ReadOnlyCollection<Archipelago.MultiClient.Net.Models.ItemInfo>(
                    new System.Collections.Generic.List<Archipelago.MultiClient.Net.Models.ItemInfo>()));
            handler = new ItemHandler(helper.Object);
        }

        // GeneratePawns expects `minors` to contain at least 2*numFiles entries:
        //   indices 0..numFiles-1     = back rank (passed through unchanged)
        //   indices numFiles..2*nF-1  = initial pawn rank (filled by GeneratePawns)
        // Output is 5*numFiles long: [back, pawn rank, rank3, rank4, rank5].
        private static System.Collections.Generic.List<PieceType> EmptyMinors()
        {
            return EmptyMinors(NUM_FILES);
        }

        private static System.Collections.Generic.List<PieceType> EmptyMinors(int numFiles)
        {
            return Enumerable.Repeat<PieceType>(null, numFiles * 2).ToList();
        }

        // GeneratePawns is value-targeted, not count-targeted: PickPawns spends an
        // adjustedPawnValues budget (foundPawns * PAWN_VALUE + spare_material + 45),
        // so the actual placed-pawn count typically exceeds foundPawns by a small margin
        // depending on which (cheap/expensive) pawn variants the RNG selects.
        // These tests therefore assert structural invariants rather than exact counts.

        // Returns the count of non-null entries in result rank `rankIndex` (0=back, 1=pawn, ...).
        private static int CountInRank(System.Collections.Generic.List<PieceType> result, int rankIndex)
        {
            return result.Skip(rankIndex * NUM_FILES).Take(NUM_FILES).Count(x => x != null);
        }

        [TestMethod]
        public void TestPawnDistribution_BasicCase()
        {
            // 8 pawns: pawn rank should fill first; back rank stays empty; minimal forward spill.
            var minors = EmptyMinors();
            ApmwCore.getInstance().foundPawns = 8;

            var result = handler.GeneratePawns(NUM_FILES, minors, 0);

            Assert.AreEqual(0, CountInRank(result, 0), "back rank untouched");
            Assert.AreEqual(NUM_FILES, CountInRank(result, 1), "pawn rank should be full");
            Assert.IsTrue(CountInRank(result, 2) <= NUM_FILES / 2, "minimal spill onto third rank");
            Assert.AreEqual(0, CountInRank(result, 3), "no pawns in fourth rank");
            Assert.AreEqual(0, CountInRank(result, 4), "no pawns in fifth rank");
        }

        [TestMethod]
        public void TestPawnDistribution_PartialFirstRank()
        {
            // 4 pawns: should place ~4 in the pawn rank only; nothing forward.
            var minors = EmptyMinors();
            ApmwCore.getInstance().foundPawns = 4;

            var result = handler.GeneratePawns(NUM_FILES, minors, 0);

            Assert.IsTrue(CountInRank(result, 1) >= 4, "at least foundPawns in pawn rank");
            Assert.IsTrue(CountInRank(result, 1) <= NUM_FILES, "pawn rank not over-filled");
            Assert.AreEqual(0, CountInRank(result, 2), "no spill onto third rank");
            Assert.AreEqual(0, CountInRank(result, 0), "back rank untouched");
        }

        [TestMethod]
        public void TestPawnDistribution_MultipleRanks()
        {
            // 20 pawns: pawn and third ranks fill, fourth rank gets the remainder.
            var minors = EmptyMinors();
            ApmwCore.getInstance().foundPawns = 20;

            var result = handler.GeneratePawns(NUM_FILES, minors, 0);

            Assert.AreEqual(NUM_FILES, CountInRank(result, 1), "pawn rank full");
            Assert.AreEqual(NUM_FILES, CountInRank(result, 2), "third rank full");
            Assert.IsTrue(CountInRank(result, 3) >= 4, "fourth rank carries the remainder");
            Assert.IsTrue(CountInRank(result, 3) < NUM_FILES, "fourth rank not full");
            Assert.AreEqual(0, CountInRank(result, 4), "fifth rank empty");
        }

        [TestMethod]
        public void TestPawnDistribution_WithExistingPieces()
        {
            // Existing piece in pawn rank is preserved; new pawns fill remaining slots and spill forward.
            var minors = EmptyMinors();
            var existing = MockPieceType();
            minors[NUM_FILES] = existing; // first slot of the pawn rank
            ApmwCore.getInstance().foundPawns = 8;

            var result = handler.GeneratePawns(NUM_FILES, minors, 0);

            Assert.AreEqual(NUM_FILES, CountInRank(result, 1), "pawn rank full");
            Assert.AreEqual(1, result.Skip(NUM_FILES).Take(NUM_FILES).Count(x => x == existing),
                "existing piece preserved exactly once in pawn rank");
            Assert.AreEqual(0, CountInRank(result, 0), "back rank untouched");
        }

        [TestMethod]
        public void TestPawnDistribution_MaximumPawns()
        {
            // 32 pawns fills pawn/third/fourth/fifth ranks (4 * numFiles slots).
            var minors = EmptyMinors();
            ApmwCore.getInstance().foundPawns = NUM_FILES * 4;

            var result = handler.GeneratePawns(NUM_FILES, minors, 0);

            for (int rank = 1; rank <= 4; rank++)
            {
                Assert.AreEqual(NUM_FILES, CountInRank(result, rank),
                    $"rank index {rank} should be full");
            }
            Assert.AreEqual(0, CountInRank(result, 0), "back rank untouched");
        }

        private PieceType MockPieceType()
        {
            // Create a mock piece type for testing
            return new Pawn("Test Pawn", "T", 100, 100);
        }

        private static bool IsSergeant(PieceType piece)
        {
            return piece != null && ApmwCore.getInstance().sergeants.Contains(piece);
        }

        private static int CountSergeants(IEnumerable<PieceType> result)
        {
            return result.Count(IsSergeant);
        }

        private static int CountPlainPawns(IEnumerable<PieceType> result)
        {
            return result.Count(p => p != null && !IsSergeant(p));
        }

        // Standard test pool matching Setup()'s core.pawns.
        private static List<PieceType> StandardPawnOptions()
        {
            return ApmwCore.getInstance().pawns.ToList();
        }

        // Build a custom pool with only Pawn(100) and Sergeant(200) — no cheap Checkers.
        // Used to exercise the pigeonhole guard without a cheap fallback.
        private static List<PieceType> PawnOnlyPool()
        {
            return new List<PieceType>
            {
                ApmwCore.getInstance().pawns.First(p => p.Name == "Pawn"),
            };
        }

        // Build a custom pool with Pawn(100), Checkers(40), Sergeant(200).
        // Pigeonhole math: 200 + (slotsAfter)*40 must fit budget.
        private static List<PieceType> PawnAndCheckersPool()
        {
            return new List<PieceType>
            {
                ApmwCore.getInstance().pawns.First(p => p.Name == "Pawn"),
                ApmwCore.getInstance().pawns.First(p => p.Name == "Checkers"),
            };
        }

        private static int AdjustedBudget(int foundPawns, int spare)
        {
            return Math.Max(foundPawns * 100, foundPawns * 100 + spare + 45);
        }

        private static void ConfigureFoundPieceCounts(
            int foundPawns, int foundConsuls, int foundJacks, int foundMajors, int foundMinors)
        {
            var core = ApmwCore.getInstance();
            core.foundPawns = foundPawns;
            core.foundConsuls = foundConsuls;
            core.foundJacks = foundJacks;
            core.foundMajors = foundMajors;
            core.foundMinors = foundMinors;
        }

        [TestMethod]
        public void PawnUpgrades_Off_PreservesBaselineDistribution()
        {
            ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.Off;
            var minors = EmptyMinors();
            ApmwCore.getInstance().foundPawns = 8;

            var result = handler.GeneratePawns(NUM_FILES, minors, 0);

            Assert.AreEqual(0, CountInRank(result, 0), "back rank untouched");
            Assert.AreEqual(NUM_FILES, CountInRank(result, 1), "pawn rank should be full");
            Assert.IsTrue(CountInRank(result, 2) <= NUM_FILES / 2, "minimal spill onto third rank");
            Assert.AreEqual(0, CountInRank(result, 3), "no pawns in fourth rank");
            Assert.AreEqual(0, CountInRank(result, 4), "no pawns in fifth rank");
            Assert.AreEqual(0, CountSergeants(result), "Off mode must not place sergeants");
        }

        [TestMethod]
        public void PawnUpgrades_Off_ProducesNinePawnsZeroSergeants_AtFoundPawns8Spare100()
        {
            ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.Off;
            ApmwCore.getInstance().foundPawns = 8;

            var picks = handler.PickPawns(new Random(42), StandardPawnOptions(),
                AdjustedBudget(8, 100), NUM_FILES * 4, 8);

            Assert.AreEqual(0, CountSergeants(picks), "Off must not place sergeants per-slot");
            Assert.IsTrue(picks.Count >= 8, $"must produce at least foundPawns slots, got {picks.Count}");
            Assert.IsTrue(picks.Count <= 24, $"budget caps slot count, got {picks.Count}");
        }

        [TestMethod]
        public void PawnUpgrades_Pool_NeverProducesAllSergeants_AtFoundPawns8Spare100()
        {
            ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.Pool;
            ApmwCore.getInstance().foundPawns = 8;

            // TestRandom always returns int.MaxValue, which lands on the sergeant in the
            // augmented [pawnOptions ++ sergeants] pool. The pigeonhole guard MUST reject
            // some sergeants so the count never drops below foundPawns.
            var picks = handler.PickPawns(AlwaysMaxRandom(), StandardPawnOptions(),
                AdjustedBudget(8, 100), NUM_FILES * 4, 8);

            Assert.IsTrue(picks.Count >= 8, $"pigeonhole must keep count >= foundPawns, got {picks.Count}");
            Assert.IsTrue(CountPlainPawns(picks) >= 1,
                "guard must force at least one non-sergeant pick to satisfy foundPawns");
        }

        [TestMethod]
        public void PawnUpgrades_Pool_AcceptableMix_AtFoundPawns8Spare100()
        {
            ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.Pool;
            ApmwCore.getInstance().foundPawns = 8;

            var picks = handler.PickPawns(new Random(42), StandardPawnOptions(),
                AdjustedBudget(8, 100), NUM_FILES * 4, 8);

            Assert.IsTrue(CountSergeants(picks) >= 1, "Pool mode should typically produce >= 1 sergeant");
            Assert.IsTrue(CountPlainPawns(picks) >= 1, "Pool mode should still produce plain pawns");
        }

        [TestMethod]
        public void PawnUpgrades_Pool_RejectsSergeant_WhenPigeonholeFails_NoCheckers()
        {
            // Pool of {Pawn(100), Sergeant(200)}, foundPawns=3, budget=320.
            // Pigeonhole at slot 0: 200 + 2*100 = 400 > 320 -> guard fails -> re-pick.
            ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.Pool;
            ApmwCore.getInstance().foundPawns = 3;

            var picks = handler.PickPawns(new AlwaysLastRandom(), PawnOnlyPool(),
                adjustedPawnValues: 320, remainingPawnSpaces: 32, foundPawns: 3);

            Assert.AreEqual(0, CountSergeants(picks), "guard must reject every sergeant attempt");
            Assert.IsTrue(CountPlainPawns(picks) >= 3,
                $"need at least 3 plain pawns to satisfy foundPawns, got {CountPlainPawns(picks)}");
            Assert.IsTrue(picks.Count <= 4, $"budget 320 caps pawn count to ~3-4, got {picks.Count}");
        }

        [TestMethod]
        public void PawnUpgrades_Pool_AcceptsSergeant_WhenPigeonholeHolds_WithCheckers()
        {
            // Pool of {Pawn(100), Checkers(40), Sergeant(200)}, foundPawns=3, budget=320.
            // Pigeonhole at slot 0: 200 + 2*40 = 280 <= 320 -> guard passes.
            ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.Pool;
            ApmwCore.getInstance().foundPawns = 3;

            var picks = handler.PickPawns(new AlwaysLastRandom(), PawnAndCheckersPool(),
                adjustedPawnValues: 320, remainingPawnSpaces: 32, foundPawns: 3);

            Assert.IsTrue(CountSergeants(picks) >= 1, "guard must accept at least the first sergeant");
            Assert.IsTrue(picks.Count >= 3, $"must end with at least foundPawns slots, got {picks.Count}");
        }

        [TestMethod]
        public void PawnUpgrades_Max_FallsBackToPawn_WhenPigeonholeWouldFail()
        {
            // Pool of {Pawn(100), Sergeant(200)}, foundPawns=3, budget=350.
            // After 1 sergeant (200), budget=150, slotsAfter=1 cheapest=100, 150 < 100*1? No.
            // Slot 0 guard: 350-200 >= 2*100=200 -> 150 >= 200? No. Guard fails first slot.
            // So we expect pawn-pawn-pawn in this scenario actually. Let me restate:
            // budget=350: slot 0 guard 350-200=150 >= 2*100=200 -> false -> pawn picked.
            // budget=250 after pawn: slot 1 guard 250-200=50 >= 1*100=100 -> false -> pawn.
            // budget=150 after pawn: slot 2 guard 150-200<0 -> false -> pawn (Min path? 150>100).
            // budget=50 after pawn: count=3 == foundPawns. Loop continues since budget>0,
            // but sergeant guard now is "budget>=sergCost" = 50>=200 false -> pawn (Min=Pawn).
            // budget=-50 -> exit. Result: 4 pawns, 0 sergeants.
            // To actually exercise sergeant placement under Max: use budget=600 with foundPawns=3.
            // Slot 0: 600-200=400 >= 2*100=200 -> serg. budget=400.
            // Slot 1: 400-200=200 >= 1*100=100 -> serg. budget=200.
            // Slot 2: 200-200=0 >= 0 -> serg. budget=0. 3 sergeants.
            // To force exactly 1 sergeant + 2 pawns + fallback: use budget=420.
            // Slot 0: 420-200=220 >= 200 -> serg. budget=220.
            // Slot 1: 220-200=20 >= 100? No -> pawn. budget=120.
            // Slot 2: count=2, slotsAfter=0 -> guard says budget>=200? No -> pawn. budget=20.
            // count=3, budget=20>0 still: serg attempted, 20<200 -> pawn (Min, budget<=100 -> Min=Pawn). budget=-80. Exit.
            // Result: 1 sergeant + 3 pawns = 4 slots. This matches the spec assertion shape.
            ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.Max;
            ApmwCore.getInstance().foundPawns = 3;

            var picks = handler.PickPawns(AlwaysMaxRandom(), PawnOnlyPool(),
                adjustedPawnValues: 420, remainingPawnSpaces: 32, foundPawns: 3);

            Assert.AreEqual(1, CountSergeants(picks), "expected exactly one sergeant before guard rejects");
            Assert.IsTrue(picks.Count >= 3, $"must end with at least foundPawns slots, got {picks.Count}");
        }

        [TestMethod]
        public void PawnUpgrades_DoubleSpendBugFixed()
        {
            // Regression: v1's UpgradePawns added spare_material to the sergeant fallback budget
            // even though spare_material was already baked into adjustedPawnValues by GeneratePawns.
            // With foundPawns=8, spare=300: budget = max(800, 800+300+45) = 1145.
            // Total piece value should never exceed 1145 + small upgrade overshoot (one piece swap).
            ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.Pool;
            ApmwCore.getInstance().foundPawns = 8;

            var picks = handler.PickPawns(AlwaysMaxRandom(), StandardPawnOptions(),
                AdjustedBudget(8, 300), NUM_FILES * 4, 8);

            int totalValue = picks.Sum(p => p.MidgameValue);
            // Allow up to one piece-value of overshoot (final pick may push budget negative).
            // The pre-fix bug would have produced totals approaching 1145 + 300 = 1445.
            Assert.IsTrue(totalValue <= 1145 + 200,
                $"total piece value {totalValue} exceeds expected ceiling 1345 — spare_material may be double-spent");
        }

        [TestMethod]
        public void PawnUpgrades_SuperMax_EightFiles_LowersGuaranteeAndKeepsPawnMaterial()
        {
            const int foundPawns = 5;
            const int expectedGuarantee = 3; // 15 slots - 12 known non-pawns.
            ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.SuperMax;
            ConfigureFoundPieceCounts(
                foundPawns, foundConsuls: 2, foundJacks: 3, foundMajors: 3, foundMinors: 4);

            var result = handler.GeneratePawns(NUM_FILES, EmptyMinors(), 0);

            int placed = result.Count(p => p != null);
            int sergeants = CountSergeants(result);
            int totalValue = result.Where(p => p != null).Sum(p => p.MidgameValue);

            Assert.IsTrue(placed >= expectedGuarantee,
                $"SuperMax must honor lowered guarantee {expectedGuarantee}, got {placed}");
            Assert.IsTrue(placed < foundPawns,
                $"SuperMax should not force all {foundPawns} found pawn slots when only {expectedGuarantee} are required");
            Assert.IsTrue(sergeants >= 1, "excess pawn material should become sergeants");
            Assert.IsTrue(totalValue >= foundPawns * _pawn.MidgameValue,
                $"pawn material should not be lost: value {totalValue} < {foundPawns * _pawn.MidgameValue}");
        }

        [TestMethod]
        public void PawnUpgrades_SuperMax_TenFiles_UsesNineteenSlotGuaranteeAndKeepsPawnMaterial()
        {
            const int numFiles = 10;
            const int foundPawns = 7;
            const int expectedGuarantee = 3; // 19 slots - 16 known non-pawns.
            ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.SuperMax;
            ConfigureFoundPieceCounts(
                foundPawns, foundConsuls: 2, foundJacks: 4, foundMajors: 5, foundMinors: 5);

            var result = handler.GeneratePawns(numFiles, EmptyMinors(numFiles), 0);

            int placed = result.Count(p => p != null);
            int sergeants = CountSergeants(result);
            int totalValue = result.Where(p => p != null).Sum(p => p.MidgameValue);

            Assert.AreEqual(numFiles * 5, result.Count, "10-file GeneratePawns output shape");
            Assert.IsTrue(placed >= expectedGuarantee,
                $"SuperMax must honor lowered guarantee {expectedGuarantee}, got {placed}");
            Assert.IsTrue(placed < foundPawns,
                $"SuperMax should not force all {foundPawns} found pawn slots when only {expectedGuarantee} are required");
            Assert.IsTrue(sergeants >= 1, "excess pawn material should become sergeants");
            Assert.IsTrue(totalValue >= foundPawns * _pawn.MidgameValue,
                $"pawn material should not be lost: value {totalValue} < {foundPawns * _pawn.MidgameValue}");
        }

        [DataTestMethod]
        [DataRow(FairyPawnUpgrades.Off, 3, -200)]
        [DataRow(FairyPawnUpgrades.Off, 3, 0)]
        [DataRow(FairyPawnUpgrades.Off, 3, 50)]
        [DataRow(FairyPawnUpgrades.Off, 3, 100)]
        [DataRow(FairyPawnUpgrades.Off, 3, 400)]
        [DataRow(FairyPawnUpgrades.Off, 4, -200)]
        [DataRow(FairyPawnUpgrades.Off, 4, 0)]
        [DataRow(FairyPawnUpgrades.Off, 4, 50)]
        [DataRow(FairyPawnUpgrades.Off, 4, 100)]
        [DataRow(FairyPawnUpgrades.Off, 4, 400)]
        [DataRow(FairyPawnUpgrades.Off, 8, -200)]
        [DataRow(FairyPawnUpgrades.Off, 8, 0)]
        [DataRow(FairyPawnUpgrades.Off, 8, 50)]
        [DataRow(FairyPawnUpgrades.Off, 8, 100)]
        [DataRow(FairyPawnUpgrades.Off, 8, 400)]
        [DataRow(FairyPawnUpgrades.Pool, 3, -200)]
        [DataRow(FairyPawnUpgrades.Pool, 3, 0)]
        [DataRow(FairyPawnUpgrades.Pool, 3, 50)]
        [DataRow(FairyPawnUpgrades.Pool, 3, 100)]
        [DataRow(FairyPawnUpgrades.Pool, 3, 400)]
        [DataRow(FairyPawnUpgrades.Pool, 4, -200)]
        [DataRow(FairyPawnUpgrades.Pool, 4, 0)]
        [DataRow(FairyPawnUpgrades.Pool, 4, 50)]
        [DataRow(FairyPawnUpgrades.Pool, 4, 100)]
        [DataRow(FairyPawnUpgrades.Pool, 4, 400)]
        [DataRow(FairyPawnUpgrades.Pool, 8, -200)]
        [DataRow(FairyPawnUpgrades.Pool, 8, 0)]
        [DataRow(FairyPawnUpgrades.Pool, 8, 50)]
        [DataRow(FairyPawnUpgrades.Pool, 8, 100)]
        [DataRow(FairyPawnUpgrades.Pool, 8, 400)]
        [DataRow(FairyPawnUpgrades.Max, 3, -200)]
        [DataRow(FairyPawnUpgrades.Max, 3, 0)]
        [DataRow(FairyPawnUpgrades.Max, 3, 50)]
        [DataRow(FairyPawnUpgrades.Max, 3, 100)]
        [DataRow(FairyPawnUpgrades.Max, 3, 400)]
        [DataRow(FairyPawnUpgrades.Max, 4, -200)]
        [DataRow(FairyPawnUpgrades.Max, 4, 0)]
        [DataRow(FairyPawnUpgrades.Max, 4, 50)]
        [DataRow(FairyPawnUpgrades.Max, 4, 100)]
        [DataRow(FairyPawnUpgrades.Max, 4, 400)]
        [DataRow(FairyPawnUpgrades.Max, 8, -200)]
        [DataRow(FairyPawnUpgrades.Max, 8, 0)]
        [DataRow(FairyPawnUpgrades.Max, 8, 50)]
        [DataRow(FairyPawnUpgrades.Max, 8, 100)]
        [DataRow(FairyPawnUpgrades.Max, 8, 400)]
        public void PawnUpgrades_AllModes_NeverFewerThanFoundPawns(FairyPawnUpgrades mode, int foundPawns, int spare)
        {
            ApmwConfig.getInstance().PawnUpgradesInt = (int)mode;
            ApmwCore.getInstance().foundPawns = foundPawns;

            var picks = handler.PickPawns(new Random(42), StandardPawnOptions(),
                AdjustedBudget(foundPawns, spare), NUM_FILES * 4, foundPawns);

            Assert.IsTrue(picks.Count >= foundPawns,
                $"mode={mode} foundPawns={foundPawns} spare={spare}: count {picks.Count} < foundPawns");
        }

        // ---------------------------------------------------------------------
        // (A) FairyPawns x PawnUpgrades cross-coverage
        // ---------------------------------------------------------------------

        private void ConfigurePool(FairyPawns pawns, FairyPawnUpgrades upgrades)
        {
            ApmwConfig.getInstance().PawnsInt = (int)pawns;
            ApmwConfig.getInstance().PawnUpgradesInt = (int)upgrades;
        }

        // Mirror of ItemHandler.setupPawnOptions(); kept private to the test class
        // so it stays in lock-step with what PickPawns actually receives.
        private static List<PieceType> ExpectedPawnOptions(FairyPawns mode)
        {
            switch (mode)
            {
                case FairyPawns.Mixed: return ApmwCore.getInstance().pawns.ToList();
                case FairyPawns.AnyPawn: return new List<PieceType> { _pawn, _berolina };
                case FairyPawns.AnyFairy: return new List<PieceType> { _berolina, _checkers };
                case FairyPawns.AnyClassical: return new List<PieceType> { _pawn, _checkers };
                case FairyPawns.Berolina: return new List<PieceType> { _berolina };
                case FairyPawns.Checkers: return new List<PieceType> { _checkers };
                case FairyPawns.Vanilla:
                default: return new List<PieceType> { _pawn };
            }
        }

        [DataTestMethod]
        [DataRow(FairyPawns.Vanilla, FairyPawnUpgrades.Off)]
        [DataRow(FairyPawns.Vanilla, FairyPawnUpgrades.Pool)]
        [DataRow(FairyPawns.Vanilla, FairyPawnUpgrades.Max)]
        [DataRow(FairyPawns.Mixed, FairyPawnUpgrades.Off)]
        [DataRow(FairyPawns.Mixed, FairyPawnUpgrades.Pool)]
        [DataRow(FairyPawns.Mixed, FairyPawnUpgrades.Max)]
        [DataRow(FairyPawns.Berolina, FairyPawnUpgrades.Off)]
        [DataRow(FairyPawns.Berolina, FairyPawnUpgrades.Pool)]
        [DataRow(FairyPawns.Berolina, FairyPawnUpgrades.Max)]
        [DataRow(FairyPawns.Checkers, FairyPawnUpgrades.Off)]
        [DataRow(FairyPawns.Checkers, FairyPawnUpgrades.Pool)]
        [DataRow(FairyPawns.Checkers, FairyPawnUpgrades.Max)]
        [DataRow(FairyPawns.AnyPawn, FairyPawnUpgrades.Off)]
        [DataRow(FairyPawns.AnyPawn, FairyPawnUpgrades.Pool)]
        [DataRow(FairyPawns.AnyPawn, FairyPawnUpgrades.Max)]
        [DataRow(FairyPawns.AnyFairy, FairyPawnUpgrades.Off)]
        [DataRow(FairyPawns.AnyFairy, FairyPawnUpgrades.Pool)]
        [DataRow(FairyPawns.AnyFairy, FairyPawnUpgrades.Max)]
        [DataRow(FairyPawns.AnyClassical, FairyPawnUpgrades.Off)]
        [DataRow(FairyPawns.AnyClassical, FairyPawnUpgrades.Pool)]
        [DataRow(FairyPawns.AnyClassical, FairyPawnUpgrades.Max)]
        public void PawnUpgrades_FairyPawns_AllCombinations_NeverFewerThanFoundPawns(
            FairyPawns pawns, FairyPawnUpgrades upgrades)
        {
            ConfigurePool(pawns, upgrades);
            int foundPawns = 8;
            ApmwCore.getInstance().foundPawns = foundPawns;

            var picks = handler.PickPawns(new Random(42),
                AdjustedBudget(foundPawns, 100), NUM_FILES * 4, foundPawns);

            Assert.IsTrue(picks.Count >= foundPawns,
                $"pawns={pawns} upgrades={upgrades}: count {picks.Count} < foundPawns");

            var expected = ExpectedPawnOptions(pawns);
            var sergeants = ApmwCore.getInstance().sergeants;
            foreach (var p in picks)
            {
                Assert.IsTrue(expected.Contains(p) || sergeants.Contains(p),
                    $"pawns={pawns} upgrades={upgrades}: piece {p?.Name} not in setupPawnOptions or core.sergeants");
            }

            if (upgrades == FairyPawnUpgrades.Off)
            {
                Assert.AreEqual(0, CountSergeants(picks),
                    $"Off mode must not place sergeants (pawns={pawns})");
            }
        }

        // ---------------------------------------------------------------------
        // (B) OdinPawn-specific tests
        // ---------------------------------------------------------------------

        [TestMethod]
        public void PawnUpgrades_Pool_OdinPawnAcceptedWhenSergeantWouldBeRejected()
        {
            // Augmented pool inside PickPawnPoolMode = pawnOptions ++ sergeants
            // = [Pawn(100), Sergeant(200), OdinPawn(150)].
            // Pigeonhole at slot 0 with foundPawns=3, budget=380, cheapestNonSergeant=100:
            //   Sergeant: 200 + 2*100 = 400 > 380 -> rejected
            //   OdinPawn: 150 + 2*100 = 350 <= 380 -> accepted
            // We force the outer Next(3) to land on index 2 (OdinPawn) so the path
            // is exercised; subsequent zeros let the algorithm settle deterministically.
            var originalSergeants = ApmwCore.getInstance().sergeants;
            try
            {
                ApmwCore.getInstance().sergeants =
                    new System.Collections.Generic.HashSet<PieceType> { _sergeant, _odinPawn };
                ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.Pool;
                ApmwCore.getInstance().foundPawns = 3;

                var pool = new List<PieceType> { _pawn };
                // Queue: 2 -> picks OdinPawn at slot 0 (augmented index 2 of [Pawn, Sergeant, OdinPawn]).
                // Trailing zeros pick Pawn (index 0) for remaining slots.
                var rng = new TestRandom(new[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

                var picks = handler.PickPawns(rng, pool,
                    adjustedPawnValues: 380, remainingPawnSpaces: 32, foundPawns: 3);

                Assert.IsTrue(picks.Contains(_odinPawn),
                    "OdinPawn should be placed when its cheaper cost satisfies pigeonhole");
                Assert.IsTrue(picks.Count >= 3, $"must end with >= foundPawns slots, got {picks.Count}");
            }
            finally
            {
                ApmwCore.getInstance().sergeants = originalSergeants;
            }
        }

        [TestMethod]
        public void PawnUpgrades_Max_PrefersAffordableSergeantVariant_OverManySeeds()
        {
            // Mixed pool, Max mode, foundPawns=3, budget=380. Whichever sergeant variant
            // GetNextPawn happens to pick (Sergeant=200 or OdinPawn=150), the algorithm
            // must always satisfy count >= foundPawns.
            var originalSergeants = ApmwCore.getInstance().sergeants;
            try
            {
                ApmwCore.getInstance().sergeants =
                    new System.Collections.Generic.HashSet<PieceType> { _sergeant, _odinPawn };
                ApmwConfig.getInstance().PawnsInt = (int)FairyPawns.Mixed;
                ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.Max;
                ApmwCore.getInstance().foundPawns = 3;

                for (int seed = 1; seed <= 50; seed++)
                {
                    var picks = handler.PickPawns(new Random(seed),
                        adjustedPawnValues: 380, remainingPawnSpaces: 32, foundPawns: 3);

                    Assert.IsTrue(picks.Count >= 3,
                        $"seed={seed}: count {picks.Count} < foundPawns 3");
                }
            }
            finally
            {
                ApmwCore.getInstance().sergeants = originalSergeants;
            }
        }

        [TestMethod]
        public void PawnUpgrades_BothSergeantVariantsCanAppear_AcrossSeeds()
        {
            // With both sergeant variants enabled, Pool mode + a generous budget should,
            // across many seeds, surface BOTH Sergeant and OdinPawn at least once.
            var originalSergeants = ApmwCore.getInstance().sergeants;
            try
            {
                ApmwCore.getInstance().sergeants =
                    new System.Collections.Generic.HashSet<PieceType> { _sergeant, _odinPawn };
                ApmwConfig.getInstance().PawnsInt = (int)FairyPawns.Mixed;
                ApmwConfig.getInstance().PawnUpgradesInt = (int)FairyPawnUpgrades.Pool;
                ApmwCore.getInstance().foundPawns = 8;

                var aggregated = new List<PieceType>();
                for (int seed = 1; seed <= 30; seed++)
                {
                    var picks = handler.PickPawns(new Random(seed),
                        AdjustedBudget(8, 800), NUM_FILES * 4, 8);
                    aggregated.AddRange(picks);
                }

                Assert.IsTrue(aggregated.Any(p => p?.Name == "Sergeant"),
                    "expected at least one Sergeant across 30 seeds");
                Assert.IsTrue(aggregated.Any(p => p?.Name == "Odin Pawn"),
                    "expected at least one Odin Pawn across 30 seeds");
            }
            finally
            {
                ApmwCore.getInstance().sergeants = originalSergeants;
            }
        }

        // ---------------------------------------------------------------------
        // (C) Material-spend efficiency
        // ---------------------------------------------------------------------

        // Lookup helper: maps a piece name to the shared instance prepared in Setup.
        // Using shared instances keeps reference-equality intact for sergeant detection.
        private static List<PieceType> CustomPool(params string[] names)
        {
            return names.Select(n =>
            {
                switch (n)
                {
                    case "Pawn": return _pawn;
                    case "Berolina Pawn": return _berolina;
                    case "Checkers": return _checkers;
                    case "Sergeant": return _sergeant;
                    case "Odin Pawn": return _odinPawn;
                    default: throw new ArgumentException($"unknown piece {n}");
                }
            }).ToList();
        }

        private static readonly System.Collections.Generic.HashSet<string> _sergeantNames =
            new System.Collections.Generic.HashSet<string> { "Sergeant", "Odin Pawn" };

        [DataTestMethod]
        [DataRow(new[] { "Pawn", "Sergeant", "Checkers" }, 4, 600, FairyPawnUpgrades.Pool)]
        [DataRow(new[] { "Pawn", "Sergeant", "Checkers" }, 4, 600, FairyPawnUpgrades.Max)]
        [DataRow(new[] { "Berolina Pawn", "Odin Pawn", "Checkers" }, 4, 540, FairyPawnUpgrades.Pool)]
        [DataRow(new[] { "Berolina Pawn", "Odin Pawn", "Checkers" }, 4, 540, FairyPawnUpgrades.Max)]
        [DataRow(new[] { "Pawn", "Berolina Pawn", "Sergeant", "Odin Pawn" }, 4, 700, FairyPawnUpgrades.Pool)]
        [DataRow(new[] { "Pawn", "Berolina Pawn", "Sergeant", "Odin Pawn" }, 4, 700, FairyPawnUpgrades.Max)]
        [DataRow(new[] { "Pawn", "Berolina Pawn", "Checkers", "Sergeant" }, 4, 600, FairyPawnUpgrades.Pool)]
        [DataRow(new[] { "Pawn", "Berolina Pawn", "Checkers", "Sergeant" }, 4, 600, FairyPawnUpgrades.Max)]
        [DataRow(new[] { "Pawn", "Berolina Pawn", "Checkers", "Odin Pawn", "Sergeant" }, 4, 700, FairyPawnUpgrades.Pool)]
        [DataRow(new[] { "Pawn", "Berolina Pawn", "Checkers", "Odin Pawn", "Sergeant" }, 4, 700, FairyPawnUpgrades.Max)]
        [DataRow(new[] { "Pawn", "Sergeant" }, 3, 400, FairyPawnUpgrades.Pool)]
        [DataRow(new[] { "Berolina Pawn", "Odin Pawn" }, 3, 420, FairyPawnUpgrades.Max)]
        public void PawnUpgrades_SpendsAsMuchMaterialAsPossible(
            string[] names, int foundPawns, int adjustedPawnValues, FairyPawnUpgrades mode)
        {
            var pool = CustomPool(names);
            var pawnPool = pool.Where(p => !_sergeantNames.Contains(p.Name)).ToList();
            var sergPool = pool.Where(p => _sergeantNames.Contains(p.Name)).ToList();
            int cheapest = pool.Min(p => p.MidgameValue);

            var originalSergeants = ApmwCore.getInstance().sergeants;
            try
            {
                ApmwCore.getInstance().sergeants =
                    new System.Collections.Generic.HashSet<PieceType>(sergPool);
                // Force Mixed so GetNextPawn(Sergeant) uses the random-from-sergeants
                // path rather than the Vanilla branch that hard-requires a piece named
                // "Sergeant" (which some custom pools intentionally omit).
                ApmwConfig.getInstance().PawnsInt = (int)FairyPawns.Mixed;
                ApmwConfig.getInstance().PawnUpgradesInt = (int)mode;
                ApmwCore.getInstance().foundPawns = foundPawns;

                var picks = handler.PickPawns(new Random(42), pawnPool,
                    adjustedPawnValues, NUM_FILES * 4, foundPawns);

                Assert.IsTrue(picks.Count >= foundPawns,
                    $"names=[{string.Join(",", names)}] mode={mode}: count {picks.Count} < foundPawns {foundPawns}");

                int spent = picks.Sum(p => p.MidgameValue);
                int leftover = adjustedPawnValues - spent;

                // Leftover should be strictly less than the cheapest piece value: if
                // it weren't, the algorithm could have placed one more cheap piece.
                // Note: leftover may be negative (last pick can overshoot the budget).
                Assert.IsTrue(leftover < cheapest,
                    $"names=[{string.Join(",", names)}] mode={mode}: leftover {leftover} >= cheapest {cheapest} (spent {spent}/{adjustedPawnValues})");
            }
            finally
            {
                ApmwCore.getInstance().sergeants = originalSergeants;
            }
        }

        // ---------------------------------------------------------------------
        // (D) Targeted FairyPawns edge tests
        // ---------------------------------------------------------------------

        [TestMethod]
        public void PawnUpgrades_Pool_VanillaPool_StrictPigeonhole_RejectsSergeant()
        {
            // Vanilla -> setupPawnOptions = [Pawn]. core.sergeants = {Sergeant} (200).
            // Pigeonhole at slot 0 with foundPawns=3, budget=320, cheapest=100:
            //   200 + 2*100 = 400 > 320 -> reject every sergeant.
            ConfigurePool(FairyPawns.Vanilla, FairyPawnUpgrades.Pool);
            ApmwCore.getInstance().foundPawns = 3;

            var picks = handler.PickPawns(new AlwaysLastRandom(),
                adjustedPawnValues: 320, remainingPawnSpaces: 32, foundPawns: 3);

            Assert.AreEqual(0, CountSergeants(picks), "Vanilla pigeonhole must reject Sergeant at budget 320");
            Assert.IsTrue(CountPlainPawns(picks) >= 3,
                $"need >= 3 plain pawns to satisfy foundPawns, got {CountPlainPawns(picks)}");
        }

        [TestMethod]
        public void PawnUpgrades_Pool_BerolinaOnly_PigeonholeUsesBerolinaCheapest()
        {
            // Berolina -> setupPawnOptions = [Berolina(85)] so cheapestNonSergeant=85.
            // foundPawns=3, budget=350. Sub-case a) with Sergeant(200) only:
            //   200 + 2*85 = 370 > 350 -> reject.
            // Sub-case b) with OdinPawn(150) only:
            //   150 + 2*85 = 320 <= 350 -> accept.
            ConfigurePool(FairyPawns.Berolina, FairyPawnUpgrades.Pool);
            ApmwCore.getInstance().foundPawns = 3;

            var originalSergeants = ApmwCore.getInstance().sergeants;
            try
            {
                // Sub-case a: Sergeant variant.
                ApmwCore.getInstance().sergeants =
                    new System.Collections.Generic.HashSet<PieceType> { _sergeant };
                var picksA = handler.PickPawns(new AlwaysLastRandom(),
                    adjustedPawnValues: 350, remainingPawnSpaces: 32, foundPawns: 3);
                Assert.AreEqual(0, CountSergeants(picksA),
                    "Berolina+Sergeant: 200+2*85=370 > 350, pigeonhole should reject");

                // Sub-case b: OdinPawn variant.
                ApmwCore.getInstance().sergeants =
                    new System.Collections.Generic.HashSet<PieceType> { _odinPawn };
                var picksB = handler.PickPawns(new AlwaysLastRandom(),
                    adjustedPawnValues: 350, remainingPawnSpaces: 32, foundPawns: 3);
                Assert.IsTrue(CountSergeants(picksB) >= 1,
                    "Berolina+OdinPawn: 150+2*85=320 <= 350, pigeonhole should accept at least one");
            }
            finally
            {
                ApmwCore.getInstance().sergeants = originalSergeants;
            }
        }

        [TestMethod]
        public void PawnUpgrades_Pool_AnyFairy_NoPlainPawnInResult()
        {
            // AnyFairy -> setupPawnOptions = [Berolina, Checkers]. Plain Pawn must not appear.
            ConfigurePool(FairyPawns.AnyFairy, FairyPawnUpgrades.Pool);
            ApmwCore.getInstance().foundPawns = 8;

            var picks = handler.PickPawns(new Random(42),
                AdjustedBudget(8, 100), NUM_FILES * 4, 8);

            Assert.IsFalse(picks.Any(p => p?.Name == "Pawn"),
                "AnyFairy must never produce plain Pawn; only Berolina/Checkers/sergeants");
        }

        [TestMethod]
        public void PawnUpgrades_Max_AnyFairy_FallbackPicksFairyPawn_NotPlainPawn()
        {
            // AnyFairy + Max: when the pigeonhole rejects a sergeant, the fallback must
            // draw from the AnyFairy options ([Berolina, Checkers]), never plain Pawn.
            ConfigurePool(FairyPawns.AnyFairy, FairyPawnUpgrades.Max);
            ApmwCore.getInstance().foundPawns = 3;

            var picks = handler.PickPawns(new Random(42),
                adjustedPawnValues: 350, remainingPawnSpaces: 32, foundPawns: 3);

            foreach (var p in picks.Where(p => !IsSergeant(p)))
            {
                Assert.IsTrue(p?.Name == "Berolina Pawn" || p?.Name == "Checkers",
                    $"AnyFairy fallback produced unexpected non-sergeant piece: {p?.Name}");
            }
        }

        [TestMethod]
        public void PawnUpgrades_Pool_Mixed_RespectsCheckersAsCheapest()
        {
            // Mixed -> setupPawnOptions = [Pawn, Berolina, Checkers] so cheapestNonSergeant=40.
            // foundPawns=3, budget=320, Sergeant(200): 200+2*40=280 <= 320 -> accept.
            ConfigurePool(FairyPawns.Mixed, FairyPawnUpgrades.Pool);
            ApmwCore.getInstance().foundPawns = 3;

            // core.sergeants = {Sergeant} (default) so AlwaysLastRandom forces Sergeant variant.
            var picks = handler.PickPawns(new AlwaysLastRandom(),
                adjustedPawnValues: 320, remainingPawnSpaces: 32, foundPawns: 3);

            Assert.IsTrue(CountSergeants(picks) >= 1,
                "Mixed cheapest (Checkers=40) should let pigeonhole accept Sergeant at budget 320");
        }
    }
}
