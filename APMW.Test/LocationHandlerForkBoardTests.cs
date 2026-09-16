using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Archipelago.APChessV
{
  [TestClass]
  [DoNotParallelize]
  public class LocationHandlerForkBoardTests
  {
    private const string PinnedRookDefender = "8/8/8/4b3/5q2/6k1/1R2N2r/K7";

    [TestMethod]
    [TestCategory("ForkBoard")]
    [TestCategory("ForkRegression")]
    public async Task SafePawnAttackingMutuallyDefendedRooks_CompletesTrueFork()
    {
      using var fixture = ForkBoardFixture.Create("k7/8/8/2r1r3/3P4/8/8/7K");
      CollectionAssert.AreEquivalent(new[] { "Pawn@d4" }, fixture.Attackers("c5", 0));
      CollectionAssert.AreEquivalent(new[] { "Pawn@d4" }, fixture.Attackers("e5", 0));
      CollectionAssert.AreEquivalent(new[] { "Rook@e5" }, fixture.Attackers("c5", 1));
      CollectionAssert.AreEquivalent(new[] { "Rook@c5" }, fixture.Attackers("e5", 1));
      Assert.AreEqual(0, fixture.Attackers("d4", 1).Length);

      var completed = await fixture.ScanAsync("d4");

      Assert.IsTrue(completed.Contains("Fork, Sacrificial"));
      Assert.IsTrue(completed.Contains("Fork, True"));
      Assert.IsFalse(completed.Contains("Fork, True Royal"));
    }

    [TestMethod]
    [TestCategory("ForkBoard")]
    [TestCategory("ForkRegression")]
    public async Task SafeQueenAttackingMutuallyDefendedRooks_CompletesOnlySacrificialFork()
    {
      using var fixture = ForkBoardFixture.Create("k7/8/8/2r1r3/3Q4/8/8/7K");
      CollectionAssert.AreEquivalent(new[] { "Queen@d4" }, fixture.Attackers("c5", 0));
      CollectionAssert.AreEquivalent(new[] { "Queen@d4" }, fixture.Attackers("e5", 0));
      CollectionAssert.AreEquivalent(new[] { "Rook@e5" }, fixture.Attackers("c5", 1));
      CollectionAssert.AreEquivalent(new[] { "Rook@c5" }, fixture.Attackers("e5", 1));
      Assert.AreEqual(0, fixture.Attackers("d4", 1).Length);

      var completed = await fixture.ScanAsync("d4");

      // Preserve the baseline sacrificial contract independently of true-fork eligibility.
      Assert.IsTrue(completed.Contains("Fork, Sacrificial"));
      Assert.IsFalse(completed.Contains("Fork, True"));
      Assert.IsFalse(completed.Contains("Fork, Sacrificial Royal"));
    }

    [TestMethod]
    [TestCategory("ForkBoard")]
    [TestCategory("ForkRegression")]
    public async Task RookThenPawnAttackingRoyalForker_DoesNotCompleteTrueRoyal()
    {
      using var fixture = ForkBoardFixture.Create("3r4/8/2k1q3/4p3/3N4/8/8/3Q3K");
      CollectionAssert.AreEquivalent(new[] { "Knight@d4" }, fixture.Attackers("c6", 0));
      CollectionAssert.AreEquivalent(new[] { "Knight@d4" }, fixture.Attackers("e6", 0));
      CollectionAssert.AreEquivalent(new[] { "Queen@d1" }, fixture.Attackers("d4", 0));
      CollectionAssert.AreEqual(new[] { "Rook@d8" }, fixture.Attackers("d4", 1, findAll: false));
      CollectionAssert.AreEquivalent(new[] { "Rook@d8", "Pawn@e5" }, fixture.Attackers("d4", 1));

      var completed = await fixture.ScanAsync("d4");

      Assert.IsTrue(completed.Contains("Fork, Sacrificial"));
      Assert.IsTrue(completed.Contains("Fork, Sacrificial Royal"));
      Assert.IsFalse(completed.Contains("Fork, True"));
      Assert.IsFalse(completed.Contains("Fork, True Royal"));
    }

    [TestMethod]
    [TestCategory("ForkBoard")]
    [TestCategory("ForkRegression")]
    public async Task TwoRooksAttackingQueenDefendedRoyalForker_DoNotCompleteTrueRoyal()
    {
      // White Kh1, Qd1, Nd4; black Kc6, Qe6, Rd8, Ra4.
      using var fixture = ForkBoardFixture.Create("3r4/8/2k1q3/8/r2N4/8/8/3Q3K");
      CollectionAssert.AreEquivalent(new[] { "Knight@d4" }, fixture.Attackers("c6", 0));
      CollectionAssert.AreEquivalent(new[] { "Knight@d4" }, fixture.Attackers("e6", 0));
      CollectionAssert.AreEquivalent(new[] { "Queen@d1" }, fixture.Attackers("d4", 0));
      CollectionAssert.AreEquivalent(new[] { "Rook@d8", "Rook@a4" }, fixture.Attackers("d4", 1));
      Assert.AreEqual(300, fixture.Game.Board["d4"].MidgameValue);
      Assert.AreEqual(950, fixture.Game.Board["d1"].MidgameValue);
      Assert.AreEqual(500, fixture.Game.Board["d8"].MidgameValue);
      Assert.AreEqual(500, fixture.Game.Board["a4"].MidgameValue);

      var completed = await fixture.ScanAsync("d4");

      Assert.IsTrue(completed.Contains("Fork, Sacrificial Royal"));
      Assert.IsFalse(completed.Contains("Fork, True"));
      Assert.IsFalse(completed.Contains("Fork, True Royal"));
    }

    [TestMethod]
    [TestCategory("ForkBoard")]
    [TestCategory("ForkRegression")]
    public async Task RoyalForkerWithRookDefenderPinnedToLastKing_DoesNotCompleteTrueRoyal()
    {
      using var fixture = ForkBoardFixture.Create(PinnedRookDefender);
      CollectionAssert.AreEquivalent(new[] { "Knight@e2" }, fixture.Attackers("g3", 0));
      CollectionAssert.AreEquivalent(new[] { "Knight@e2" }, fixture.Attackers("f4", 0));
      CollectionAssert.AreEquivalent(new[] { "Rook@h2" }, fixture.Attackers("e2", 1));
      CollectionAssert.AreEquivalent(new[] { "Rook@b2" }, fixture.Attackers("e2", 0));

      var completed = await fixture.ScanAsync("e2");

      Assert.IsTrue(completed.Contains("Fork, Sacrificial Royal"));
      Assert.IsFalse(completed.Contains("Fork, True"));
      Assert.IsFalse(completed.Contains("Fork, True Royal"));
    }

    [TestMethod]
    [TestCategory("ForkBoard")]
    [TestCategory("ForkRegression")]
    public void PinnedRookRecapturingRoyalForker_AllowsLastKingToBeCaptured()
    {
      using var fixture = ForkBoardFixture.Create(PinnedRookDefender);

      fixture.PlayMoves("h2e2 b2e2 e5a1");

      Assert.AreEqual("Bishop", fixture.Game.Board["a1"].PieceType.Name);
      Assert.AreEqual(1, fixture.Game.Board["a1"].Player);
      Assert.IsFalse(Enumerable.Range(0, fixture.Game.Board.NumSquares)
        .Select(square => fixture.Game.Board[square])
        .Any(piece => piece != null && piece.Player == 0 && piece.PieceType.Name == "King"));
    }

    [TestMethod]
    [TestCategory("ForkBoard")]
    [TestCategory("ForkKnownLimitation")]
    public async Task KnownLimitation_SafeKnightForkCompletesTrueDespiteLastKingThreatAfterReply()
    {
      using var fixture = ForkBoardFixture.Create("7k/8/8/1b3b2/3N4/8/K7/8");
      CollectionAssert.AreEquivalent(new[] { "Knight@d4" }, fixture.Attackers("b5", 0));
      CollectionAssert.AreEquivalent(new[] { "Knight@d4" }, fixture.Attackers("f5", 0));
      Assert.AreEqual(0, fixture.Attackers("d4", 1).Length);
      Assert.AreEqual(0, fixture.Attackers("b5", 1).Length);
      Assert.AreEqual(0, fixture.Attackers("f5", 1).Length);

      var completed = await fixture.ScanAsync("d4");

      // This records an existing limitation, not a claim that the fork is tactically safe.
      Assert.IsTrue(completed.Contains("Fork, Sacrificial"));
      Assert.IsTrue(completed.Contains("Fork, True"));
      fixture.PlayMoves("b5c4 d4f5 c4a2");
      Assert.AreEqual("Bishop", fixture.Game.Board["a2"].PieceType.Name);
      Assert.AreEqual(1, fixture.Game.Board["a2"].Player);
      Assert.IsFalse(Enumerable.Range(0, fixture.Game.Board.NumSquares)
        .Select(square => fixture.Game.Board[square])
        .Any(piece => piece != null && piece.Player == 0 && piece.PieceType.Name == "King"));
    }
  }
}
