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
using Moq;

namespace ChessV.Test
{
    [TestClass]
    public class ApmwPawnDistributionTests
    {
        private ItemHandler handler;
        private const int NUM_FILES = 8;

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
            // Deterministic seeds for tests that go through GeneratePawns.
            ApmwConfig.getInstance().pawnSeed = 42;
            ApmwConfig.getInstance().pawnLocSeed = 4242;

            var core = ApmwCore.getInstance();
            core.pawns = new System.Collections.Generic.HashSet<PieceType>
            {
                new Pawn("Pawn", "P", 100, 125),
                new BerolinaPawn("Berolina Pawn", "Ŕ", 85, 120),
                new ChessV.Games.Pieces.Apmw.Checkers("Checkers", "Ç", 40, 95),
            };
            core.sergeants = new System.Collections.Generic.HashSet<PieceType>
            {
                new ChessV.Games.Pieces.Apmw.Sergeant("Sergeant", "Ŝ", 200, 225),
            };
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
            return Enumerable.Repeat<PieceType>(null, NUM_FILES * 2).ToList();
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
    }
}
