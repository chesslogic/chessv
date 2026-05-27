using System;
using System.Collections.Generic;
using System.Linq;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using ChessV.Games.Pieces.Apmw;
using ChessV.Manager;

namespace ChessV.Test
{
  //  Regression tests pinning down basic setup behavior of the APMW
  //  ("Archipelago Multiworld") variant. These assert that the game can
  //  be constructed and initialized with only the default (non-randomized)
  //  ApmwCore providers, that the Board ends up 8x8 with all predefined
  //  directions wired up, and that the root MoveList cursors are in a
  //  clean state after setup and after Reset().
  //
  //  Test hooks used (added in this change-set):
  //    - Game.RootMoveListForTest / Game.GenerateMovesForTest
  //    - MoveList.PickupCursorForTest / MoveList.DropCursorForTest
  //  These are narrow, read-only seams that avoid reflection in tests.
  [TestClass]
  public class ApmwSetupTests
  {
    private const string GameName = "Archipelago Multiworld";

    //  ApmwCore is a process-wide singleton whose providers are set by
    //  whatever happens to construct an ApmwChessGame first. Reset the
    //  providers to their documented defaults before each test so we do
    //  not depend on test execution order.
    [TestInitialize]
    public void ResetApmwCore()
    {
      ApmwCore core = ApmwCore.getInstance();
      core.foundPockets = -1;
      core.foundPocketRange = -1;
      core.foundPocketGems = -1;
      core.foundPawns = -1;
      core.foundMinors = -1;
      core.foundMajors = -1;
      core.foundJacks = -1;
      core.foundQueens = -1;
      core.foundConsuls = -1;
      core.foundKingPromotions = -1;
      core.foundPawnForwardness = -1;
      core.isGrand = false;
      core.foundArmy = null;

      //  Build a minimal but legal starting setup so that the game can
      //  actually reach postInitialize/LoadFEN. We place a King and two
      //  Rooks on the back rank (ApmwChess uses rank=4 as the back line
      //  in the starting-position dictionary — see ApmwChess.cs) and a
      //  full row of Pawns on rank=3. Pieces are separate instances with
      //  matching notations so the FEN serializer round-trips them into
      //  the game's own piece types.
      PieceType king = new King("King", "K", 325, 325);
      PieceType rook = new Rook("Rook", "R", 500, 550);
      PieceType pawn = new Pawn("Pawn", "P", 100, 125);

      Dictionary<KeyValuePair<int, int>, PieceType> starters =
        new Dictionary<KeyValuePair<int, int>, PieceType>();
      for (int f = 0; f < 8; f++)
        starters[new KeyValuePair<int, int>(3, f)] = pawn;
      starters[new KeyValuePair<int, int>(4, 0)] = rook;
      starters[new KeyValuePair<int, int>(4, 4)] = king;
      starters[new KeyValuePair<int, int>(4, 7)] = rook;

      core.PlayerPieceSetProvider = (numFiles) => (starters, "QRNB");
      core.PlayerPocketPiecesProvider = () => new List<PieceType>();
      core.GeriProvider = () => 1;
      core.EngineWeakeningProvider = () => 0;
    }

    private static ApmwChessGame CreateAndInitializeApmw()
    {
      //  Use the real Manager so the initialization path matches the live
      //  app (ChessV.GUI.MainForm.startGame -> Manager.CreateGame).
      Manager.Manager mgr = new Manager.Manager();
      Game game = mgr.CreateGame(GameName);
      Assert.IsNotNull(game, "Manager.CreateGame returned null for " + GameName);
      Assert.IsInstanceOfType(game, typeof(ApmwChessGame),
        "Expected Archipelago Multiworld to resolve to ApmwChessGame");
      return (ApmwChessGame) game;
    }

    // (a) Construction alone must not throw. No Initialize, no Manager.
    [TestMethod]
    public void Construct_DoesNotThrow()
    {
      ApmwChessGame game = new ApmwChessGame();
      Assert.IsNotNull(game);
      //  Kings/Pawns/etc. are instance-level collections populated in the
      //  ctor; verify they at least exist so later NRE bugs would fail here.
      Assert.IsNotNull(game.Kings);
      Assert.IsNotNull(game.Pawns);
    }

    // (b) After initialization the board is the documented 8x8.
    [TestMethod]
    public void Initialize_BoardIs8x8()
    {
      ApmwChessGame game = CreateAndInitializeApmw();
      Assert.AreEqual(8, game.Board.NumFiles, "NumFiles");
      Assert.AreEqual(8, game.Board.NumRanks, "NumRanks");
      Assert.AreEqual(64, game.Board.NumSquares, "NumSquares");
    }

    // (c) Every registered move capability on every registered piece type
    // must name a direction that Board.NextSquare actually wires up (i.e.
    // resolves to a neighbor in [0, NumSquares) for SOME central square,
    // for SOME player). A direction number that returns -1 everywhere is
    // effectively a noop and will manifest as missing moves or silent
    // crashes later.
    [TestMethod]
    public void Initialize_EveryRegisteredMoveDirection_IsWiredUp()
    {
      ApmwChessGame game = CreateAndInitializeApmw();
      Board board = game.Board;
      int center = board.LocationToSquare(new Location(3, 4)); // e4 (rank 3, file 4)

      for (int t = 0; t < game.NPieceTypes; t++)
      {
        PieceType type = game.GetPieceType(t);
        MoveCapability[] caps;
        int n = type.GetMoveCapabilities(out caps);
        for (int i = 0; i < n; i++)
        {
          int dir = caps[i].NDirection;
          bool wiredForSomePlayer = false;
          for (int player = 0; player < 2 && !wiredForSomePlayer; player++)
          {
            //  Try the central square and its four diagonal neighbors so
            //  that even if e4 happens to be off-board for some exotic
            //  (future) direction we have a few fallbacks.
            int[] trySquares = new int[]
            {
              center,
              board.LocationToSquare(new Location(3, 3)), // d4
              board.LocationToSquare(new Location(4, 4)), // e5
              board.LocationToSquare(new Location(3, 5)), // f4
              board.LocationToSquare(new Location(2, 4)), // e3
            };
            foreach (int sq in trySquares)
            {
              int next = board.NextSquare(player, dir, sq);
              if (next >= 0 && next < board.NumSquares)
              {
                wiredForSomePlayer = true;
                break;
              }
            }
          }
          Assert.IsTrue(wiredForSomePlayer,
            string.Format("PieceType '{0}' capability #{1} direction {2} ({3}) is not wired up for either player from any central square.",
              type.Name, i, dir, caps[i].Direction));
        }
      }
    }

    // (d) The eight compass directions Checkers and the standard pieces
    // use must resolve to valid neighbors from e4. If any returns -1 we
    // have reproduced suspected bug #1 from the task description.
    [TestMethod]
    public void Initialize_StandardCompassDirectionsFromE4_AreNeighbors()
    {
      ApmwChessGame game = CreateAndInitializeApmw();
      Board board = game.Board;
      int e4 = board.LocationToSquare(new Location(3, 4));

      var directionsToCheck = new (int nDir, string name)[]
      {
        (PredefinedDirections.N,  "N"),
        (PredefinedDirections.S,  "S"),
        (PredefinedDirections.E,  "E"),
        (PredefinedDirections.W,  "W"),
        (PredefinedDirections.NE, "NE"),
        (PredefinedDirections.NW, "NW"),
        (PredefinedDirections.SE, "SE"),
        (PredefinedDirections.SW, "SW"),
      };

      foreach (var (nDir, name) in directionsToCheck)
      {
        int next = board.NextSquare(nDir, e4);
        Assert.IsTrue(next >= 0 && next < board.NumSquares,
          string.Format("Board.NextSquare({0}, e4) returned {1}; e4 is in the center of an 8x8 board and MUST have a neighbor in this direction.", name, next));
        //  Sanity: the neighbor should be king-distance 1 from e4.
        Assert.AreEqual(1, board.GetDistance(e4, next),
          string.Format("Board.NextSquare({0}, e4) -> square {1} is not a king-adjacent square.", name, next));
      }
    }

    // (e) MoveList cursor baseline: per the task description, after the
    // game setup completes but before any move is played, the root move
    // list's cursors should all be zero. In the current ChessV design,
    // Game.Initialize() internally calls LoadFEN() which in turn calls
    // generateMoves(), so pickup/drop/move cursors are NON-zero by the
    // time Initialize returns. Whether that is intended or a leaked-
    // state bug (see _refs/2025-09-12.txt, _refs/2026-02-12.txt — both
    // stack traces point at a stale pickups[] entry) is exactly what
    // this task is trying to pin down, so we keep the strict assertion
    // and mark the test [Ignore] until that design question is
    // answered. TODO(chesslogic): unignore once the intended post-
    // Initialize state is decided.
    [TestMethod]
    //[Ignore("Repros suspected MoveList leftover-cursor bug; see _refs/2025-09-12.txt and _refs/2026-02-12.txt. After Game.Initialize() the root MoveList has candidate moves already generated, so cursors are not zero. Unignore once it is decided whether that is a bug.")]
    public void Initialize_RootMoveListCursors_AreZero()
    {
      ApmwChessGame game = CreateAndInitializeApmw();
      MoveList root = game.RootMoveListForTest;
      Assert.IsNotNull(root, "RootMoveListForTest was null after Initialize");
      Assert.AreEqual(0, root.MoveCursor,        "moveCursor should be 0 at baseline");
      Assert.AreEqual(0, root.PickupCursorForTest, "pickupCursor should be 0 at baseline");
      Assert.AreEqual(0, root.DropCursorForTest,   "dropCursor should be 0 at baseline");
    }

    // (e') Contract version of (e) that does not depend on the disputed
    // post-Initialize state: after the root MoveList is explicitly
    // Reset(), all three cursors are zero. This is the invariant we
    // actually rely on in the move-generation pipeline.
    [TestMethod]
    public void Initialize_ThenResetRootMoveList_AllCursorsZero()
    {
      ApmwChessGame game = CreateAndInitializeApmw();
      MoveList root = game.RootMoveListForTest;
      Assert.IsNotNull(root, "RootMoveListForTest was null after Initialize");

      root.Reset();

      Assert.AreEqual(0, root.MoveCursor,          "moveCursor after Reset");
      Assert.AreEqual(0, root.PickupCursorForTest, "pickupCursor after Reset");
      Assert.AreEqual(0, root.DropCursorForTest,   "dropCursor after Reset");
    }

    // (f) Running move generation for the initial position and then
    // resetting the MoveList must leave all cursors back at zero. This
    // pins down suspected bug #2 (lingering cursor state on the
    // MoveList after a generation pass). We do NOT call LoadFEN again
    // here because Initialize() already loaded FENStart and LoadFEN
    // does not ClearBoard first.
    [TestMethod]
    public void GenerateMovesForInitialPosition_ThenReset_ResetsAllCursorsToZero()
    {
      ApmwChessGame game = CreateAndInitializeApmw();
      MoveList root = game.RootMoveListForTest;
      Assert.IsNotNull(root);

      //  Force a fresh generation pass against the initial position
      //  (mirrors what happens each turn in the GUI pipeline).
      root.Reset();
      game.GenerateMovesForTest(game.CurrentSide);

      //  At this point we expect at least one legal move to have been
      //  generated — the side to move must have something to play.
      Assert.IsTrue(root.MoveCursor > 0,
        "Move generation for the initial position produced zero moves; something is very wrong with setup.");

      root.Reset();
      Assert.AreEqual(0, root.MoveCursor,          "moveCursor not reset to 0");
      Assert.AreEqual(0, root.PickupCursorForTest, "pickupCursor not reset to 0");
      Assert.AreEqual(0, root.DropCursorForTest,   "dropCursor not reset to 0");
    }
  }
}
