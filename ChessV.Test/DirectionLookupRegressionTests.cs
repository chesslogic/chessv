using System;
using ChessV;
using ChessV.Base;

namespace ChessV.Test
{
  // Regression tests for the "unregistered Direction" bug, originally
  // hypothesized by the user and confirmed by the agent that authored this
  // file:
  //
  // Game.DirectionLookup(MoveInfo) and Game.DirectionLookup(Movement) at
  // ChessV.Base/Game.cs:1296-1300 used to unconditionally index
  //     playerDirections[move.Player, Board.DirectionLookup(from, to)]
  // but Board.DirectionLookup (Board.cs:271) returns -1 whenever no
  // *registered* direction connects from->to. The matrix is built in
  // buildDirectionLookupMatrix (Board.cs:562-594) by walking each registered
  // direction step-by-step and stamping the *first* direction that lands on
  // each target. Composite displacements that are not themselves a registered
  // direction (e.g. the (rank+6, file+2) displacement of a 3-jump Checkers
  // chain a1->c3->e5->c7) get no entry and stay at -1.
  //
  // Consumers that iterate generated moves and call this lookup would then
  // crash with IndexOutOfRangeException:
  //   ChessV.Games\Rules\Move50Rule.cs:110
  //   ChessV.Games\Rules\Apmw\MixedEnPassantRule.cs:62
  //   ChessV.Games\Rules\EnPassantRule.cs:152
  //   ChessV.Games\Rules\Berolina\BerolinaEnPassantRule.cs:150
  //
  // The fix forwards the -1 sentinel from Board.DirectionLookup instead of
  // indexing playerDirections with it. These tests now PIN the safe
  // behavior: -1 is returned, no exception is thrown.
  [TestClass]
  public class DirectionLookupRegressionTests
  {
    private static CheckersCannonTestGame CreateGame()
    {
      var game = new CheckersCannonTestGame();
      var attrs = typeof(CheckersCannonTestGame)
        .GetCustomAttributes(typeof(GameAttribute), inherit: false);
      Assert.IsTrue(attrs.Length > 0);
      var gameAttr = (GameAttribute)attrs[0];
      game.Initialize(gameAttr, null, null);
      return game;
    }

    private static int Sq(Game g, string notation) => g.Board.DefaultNotationToSquare(notation);

    // (a) Direct call: hand-construct a Movement whose displacement is not on
    // any registered direction (a1->c7 has rank/file delta (6,2), not a
    // primary compass direction nor a knight leap, and certainly not
    // anything Chess+Checkers+Cannon registers). Game.DirectionLookup must
    // forward the -1 sentinel instead of indexing playerDirections[player, -1].
    [TestMethod]
    public void DirectionLookup_HandCraftedCompositeDisplacement_ReturnsMinusOne()
    {
      var g = CreateGame();
      g.LoadFEN("8/8/8/8/8/8/8/8 w - - 0 1");

      int from = Sq(g, "a1");
      int to = Sq(g, "c7");

      Assert.AreEqual(-1, g.Board.DirectionLookup(from, to),
        "Sanity: Board.DirectionLookup must return -1 for composite displacement (a1->c7).");

      var movement = new Movement(from, to, 0, MoveType.StandardMove);
      Assert.AreEqual(-1, g.DirectionLookup(movement),
        "Game.DirectionLookup(Movement) must forward the -1 sentinel instead of " +
        "indexing playerDirections[player, -1].");
    }

    // (a') Same as above but via the MoveInfo overload (Game.cs:1296).
    [TestMethod]
    public void DirectionLookup_MoveInfoOverload_CompositeDisplacement_ReturnsMinusOne()
    {
      var g = CreateGame();
      g.LoadFEN("8/8/8/8/8/8/8/8 w - - 0 1");

      MoveInfo move = new MoveInfo
      {
        FromSquare = Sq(g, "a1"),
        ToSquare = Sq(g, "c7"),
        Player = 0,
        MoveType = MoveType.StandardMove,
      };

      Assert.AreEqual(-1, g.DirectionLookup(ref move),
        "Game.DirectionLookup(ref MoveInfo) must forward the -1 sentinel.");
    }

    // (d) Matrix proof: the directionLookup matrix, after Board init, has -1
    // entries for composite displacements, AND playerDirections[0, -1]
    // throws IndexOutOfRangeException. This proves the chain that produces
    // the bug regardless of which Game subclass is in play.
    [TestMethod]
    public void DirectionLookupMatrix_CompositeDisplacementIsMinusOne_AndPlayerDirectionsThrows()
    {
      var g = CreateGame();
      g.LoadFEN("8/8/8/8/8/8/8/8 w - - 0 1");

      int a1 = Sq(g, "a1");
      int c7 = Sq(g, "c7");
      Assert.AreEqual(-1, g.Board.DirectionLookup(a1, c7),
        "directionLookup[a1,c7] must be -1 (composite displacement).");

      // Confirm that PlayerDirection(player, -1) throws - this is the exact
      // expression evaluated inside Game.DirectionLookup.
      Assert.ThrowsException<IndexOutOfRangeException>(() =>
      {
        int _ = g.PlayerDirection(0, -1);
      });
    }

    // (c) End-to-end: generate a real Checkers 3-jump chain whose endpoints
    // form a composite displacement, then call game.DirectionLookup on the
    // generated MoveInfo. This is the most authentic reproduction - the
    // crash that real consumers (Move50Rule etc.) would hit when iterating
    // generated moves after such a chain is in the move list.
    //
    // Layout (white to move, white Checkers at a1, white King at h1, black
    // King at g8 to keep him out of the way; black Checkers screens at b2,
    // d4, d6 so a1 can chain a1->c3->e5->c7):
    //   8: 6k.
    //   7: ...
    //   6: ...e....
    //   5: ...
    //   4: ...e....
    //   3: ...
    //   2: .e......
    //   1: E......K
    [TestMethod]
    public void DirectionLookup_OnGeneratedCheckersTripleJump_ReturnsMinusOne()
    {
      var g = CreateGame();
      g.LoadFEN("6k1/8/3e4/8/3e4/8/1e6/E6K w - - 0 1");

      g.GenerateMovesForTest(0);
      var ml = g.RootMoveListForTest;

      int a1 = Sq(g, "a1");
      int c7 = Sq(g, "c7");

      // Find the triple-jump terminating at c7. Checkers move generation
      // emits each landing-step as a separate MoveInfo with FromSquare = the
      // *original* origin (a1) so that the human-visible move starts where
      // the piece started; the chain end-state is the move whose ToSquare
      // is c7.
      MoveInfo? tripleJump = null;
      for (int i = 0; i < ml.MoveCursor; i++)
      {
        var mv = ml.GetMoveForTest(i);
        if (mv.FromSquare == a1 && mv.ToSquare == c7)
        {
          tripleJump = mv;
          break;
        }
      }

      Assert.IsTrue(tripleJump.HasValue,
        "Expected a generated Checkers triple-jump a1->c3->e5->c7. If this " +
        "fails the layout no longer produces the chain (check the FEN); the " +
        "subsequent assertion would otherwise be vacuous.");

      // Sanity: the displacement really is composite/unregistered.
      Assert.AreEqual(-1, g.Board.DirectionLookup(a1, c7),
        "a1->c7 must remain unregistered for this test to be meaningful.");

      MoveInfo move = tripleJump.Value;
      Assert.AreEqual(-1, g.DirectionLookup(ref move),
        "A generated multi-jump's MoveInfo, fed into Game.DirectionLookup " +
        "(which Move50Rule / EnPassantRule / MixedEnPassantRule / " +
        "BerolinaEnPassantRule all do during post-move processing), must " +
        "now return -1 rather than throw IndexOutOfRangeException.");
    }

    // (b) Indirect via a real rule. Move50Rule.MoveBeingMade calls
    // Game.DirectionLookup(move) when the moved piece type is in the
    // tracked list AND requiredDirection != -1. We could install a
    // Move50Rule that tracks Checkers with a non-(-1) requiredDirection and
    // play the triple-jump above to trigger MoveBeingMade. That requires
    // hooking the rule into a freshly-constructed game (AddRule must be
    // called inside AddRules, before postInitialize), running a full
    // MakeMove pipeline including the game-state machinery, and waiting
    // for the rule callback. This is plumbing the existing test scaffold
    // does not expose (no public MakeMove for an arbitrary MoveInfo on a
    // composed game, and Move50Rule's parameterization isn't exposed via
    // the test variant). The direct-call test (a) plus the end-to-end
    // generated-move test above already prove the crash signature; an
    // additional rule-driven repro would not add evidence beyond what
    // those provide.
    [TestMethod]
    [Ignore("Direct + generated-move tests already pin the bug. Adding a " +
            "Move50Rule-driven repro would require building a custom Game " +
            "with the rule wired via AddRules and a public MakeMove path, " +
            "neither of which the current test scaffold exposes.")]
    public void Move50Rule_OnCheckersMultiJump_Throws()
    {
    }
  }
}
