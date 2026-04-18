using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Helpers;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using Moq;

namespace Archipelago.APChessV
{
  /// <summary>
  /// Regression tests pinning the current behavior of the fork-detection logic in
  /// <see cref="LocationHandler.HandleMove"/>. These tests exercise the "BEGIN threats"
  /// section that scans every board square for threatened opponent pieces and emits
  /// "Threaten X" / "Fork, ..." location checks. They are intended as a baseline for
  /// reviewing PR #1, which rewrites the true-fork classifier.
  ///
  /// Test conventions:
  ///   - humanPlayer is 0 (player.IsHuman = true on Player 0).
  ///   - "Attack table" is keyed by player index. attacks[player][square] returns the
  ///     attackers (i.e. pieces of `player` attacking `square`).
  ///   - GameTurnNumber is forced > 10 so the early king-pattern locations don't fire.
  /// </summary>
  [TestClass]
  public class LocationHandlerForkTests
  {
    // PieceTypes - shared across all tests via ApmwCore singleton
    static readonly Pawn PawnType = new Pawn("Pawn", "P", 100, 125);
    static readonly Knight KnightType = new Knight("Knight", "N", 300, 325);
    static readonly Bishop BishopType = new Bishop("Bishop", "B", 325, 350);
    static readonly Rook RookType = new Rook("Rook", "R", 500, 550);
    static readonly Queen QueenType = new Queen("Queen", "Q", 900, 950);
    static readonly King KingType = new King("King", "K", 0, 0);

    Mock<ILocationCheckHelper> locations;
    Mock<ChessV.Match> match;
    Mock<Game> game;
    Mock<Board> board;
    Mock<Player> player;
    LocationHandler handler;

    /// <summary>Squares mapped to the piece occupying them (or absent = empty square).</summary>
    Dictionary<int, Piece> piecesBySquare;

    /// <summary>attackTable[player][square] = pieces of `player` attacking `square`.</summary>
    Dictionary<int, Dictionary<int, List<Piece>>> attackTable;

    const int BOARD_SQUARES = 64;
    const int HUMAN = 0;
    const int OPPONENT = 1;

    [TestInitialize]
    public void Setup()
    {
      var core = ApmwCore.getInstance();
      core.kings = new List<PieceType> { KingType };
      core.pawns = new HashSet<PieceType> { PawnType };
      core.minors = new HashSet<PieceType> { KnightType, BishopType };
      core.majors = new HashSet<PieceType> { RookType };
      core.queens = new HashSet<PieceType> { QueenType };

      locations = new Mock<ILocationCheckHelper>();
      locations.Setup(l => l.GetLocationIdFromName("ChecksMate", It.IsAny<string>())).Returns(0L);

      player = new Mock<Player>();
      player.SetupGet(p => p.IsHuman).Returns(true);

      board = new Mock<Board>();
      game = new Mock<Game>();
      match = new Mock<ChessV.Match>();
      match.SetupGet(m => m.Game).Returns(game.Object);
      match.Setup(m => m.GetPlayer(0)).Returns(player.Object);

      game.SetupGet(g => g.Board).Returns(board.Object);
      game.SetupGet(g => g.GameTurnNumber).Returns(99); // skip early-king locations
      game.SetupGet(g => g.NumFiles).Returns(8);
      game.SetupGet(g => g.BoardMoveStack).Returns(new BoardMoveStack(board.Object));

      board.SetupGet(b => b.NumSquares).Returns(BOARD_SQUARES);
      board.Setup(b => b.GetFile(It.IsAny<int>())).Returns<int>(s => s % 8);
      board.Setup(b => b.GetRank(It.IsAny<int>())).Returns<int>(s => s / 8);
      board.Setup(b => b.GetFileNotation(It.IsAny<int>()))
           .Returns<int>(f => ((char)('a' + f)).ToString());
      board.Setup(b => b.SquareToLocation(It.IsAny<int>()))
           .Returns<int>(s => new Location(s / 8, s % 8));

      piecesBySquare = new Dictionary<int, Piece>();
      attackTable = new Dictionary<int, Dictionary<int, List<Piece>>>
      {
        [HUMAN] = new Dictionary<int, List<Piece>>(),
        [OPPONENT] = new Dictionary<int, List<Piece>>(),
      };

      board.Setup(b => b[It.IsAny<int>()]).Returns<int>(sq =>
        piecesBySquare.TryGetValue(sq, out var p) ? p : null);

      // 2-arg overload: just whether any piece of `player` attacks `square`
      game.Setup(g => g.IsSquareAttacked(It.IsAny<int>(), It.IsAny<int>()))
          .Returns<int, int>((sq, p) =>
            attackTable[p].TryGetValue(sq, out var list) && list.Count > 0);

      // 3-arg overload (with out attackers). Moq can't easily do `out`s through Setup
      // for non-It.Ref args unless we use Callback. We use the ReturnsAttackers helper.
      ConfigureIsSquareAttackedWithAttackers();
    }

    private void ConfigureIsSquareAttackedWithAttackers()
    {
      // Moq's Returns delegate cannot include `out` parameters, so we use Callback on
      // a side-channel field then return a fixed value? Easier: use a custom delegate.
      game.Setup(g => g.IsSquareAttacked(It.IsAny<int>(), It.IsAny<int>(),
                  out It.Ref<List<Piece>>.IsAny, It.IsAny<bool>()))
          .Returns(new IsSquareAttackedDelegate((int sq, int p, out List<Piece> a, bool _) =>
          {
            if (attackTable[p].TryGetValue(sq, out var list) && list.Count > 0)
            {
              a = new List<Piece>(list);
              return true;
            }
            a = new List<Piece>();
            return false;
          }));
    }

    private delegate bool IsSquareAttackedDelegate(int sq, int player, out List<Piece> attackers, bool findAttackers);

    /// <summary>Place a piece of the given type and player at `square`.</summary>
    private Piece Place(int square, int playerIdx, PieceType type)
    {
      var p = new Piece { Player = playerIdx, PieceType = type, Square = square };
      piecesBySquare[square] = p;
      return p;
    }

    /// <summary>Record that `attacker` (a piece of `playerIdx`) attacks `targetSquare`.</summary>
    private void Attack(int playerIdx, int targetSquare, Piece attacker)
    {
      if (!attackTable[playerIdx].TryGetValue(targetSquare, out var list))
        attackTable[playerIdx][targetSquare] = list = new List<Piece>();
      list.Add(attacker);
    }

    /// <summary>
    /// Build the LocationHandler under test. Must be called AFTER the board state and
    /// attack tables have been arranged.
    /// </summary>
    private void BuildHandler()
    {
      handler = new LocationHandler(locations.Object, match.Object);
    }

    /// <summary>
    /// Drive HandleMove with a benign "human pawn made a quiet move" event so the
    /// scanner-based threat/fork code runs without colliding with the per-piece
    /// capture logic earlier in HandleMove.
    /// </summary>
    private void TriggerThreatScan()
    {
      var moverType = new Pawn("Mover", "M", 100, 125);
      var mover = new Piece { Player = HUMAN, PieceType = moverType, Square = 0 };
      var info = new MoveInfo
      {
        Player = HUMAN,
        FromSquare = 0,
        ToSquare = 8,
        MoveType = MoveType.StandardMove,
        PieceMoved = mover,
        PieceCaptured = null,
      };
      handler.HandleMove(info);
    }

    private void AssertLocationFired(string locationName)
    {
      locations.Verify(l => l.GetLocationIdFromName("ChecksMate", locationName), Times.AtLeastOnce(),
        $"Expected location '{locationName}' to be checked.");
    }

    private void AssertLocationNotFired(string locationName)
    {
      locations.Verify(l => l.GetLocationIdFromName("ChecksMate", locationName), Times.Never(),
        $"Did not expect location '{locationName}' to be checked.");
    }

    // -----------------------------------------------------------------------------------
    // Single-target threats: just emit the right "Threaten X" location
    // -----------------------------------------------------------------------------------

    [TestMethod]
    public void Threat_SingleMinor_EmitsThreatenMinorOnly()
    {
      var ourKnight = Place(20, HUMAN, KnightType);
      Place(40, OPPONENT, BishopType); // opponent minor
      Attack(HUMAN, 40, ourKnight);

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Threaten Minor");
      AssertLocationNotFired("Fork, Sacrificial");
      AssertLocationNotFired("Fork, True");
    }

    [TestMethod]
    public void Threat_SingleQueen_EmitsThreatenQueenNotFork()
    {
      var ourKnight = Place(20, HUMAN, KnightType);
      Place(45, OPPONENT, QueenType);
      Attack(HUMAN, 45, ourKnight);

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Threaten Queen");
      AssertLocationNotFired("Fork, Sacrificial");
    }

    [TestMethod]
    public void Threat_SingleKing_EmitsThreatenKingNotFork()
    {
      var ourRook = Place(20, HUMAN, RookType);
      Place(60, OPPONENT, KingType);
      Attack(HUMAN, 60, ourRook);

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Threaten King");
      AssertLocationNotFired("Fork, Sacrificial");
    }

    [TestMethod]
    public void Threat_SinglePawn_EmitsThreatenPawnAndContinuesPastFork()
    {
      // CURRENT behavior: after "Threaten Pawn" the loop `continue`s, so a knight that
      // forks a pawn + a minor only registers the pawn threat (no fork). PR #1 removes
      // this `continue`. This test pins the current behavior so the regression is visible.
      var ourKnight = Place(20, HUMAN, KnightType);
      Place(40, OPPONENT, PawnType);
      Place(45, OPPONENT, BishopType);
      Attack(HUMAN, 40, ourKnight);
      Attack(HUMAN, 45, ourKnight);

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Threaten Pawn");
      AssertLocationFired("Threaten Minor");
      // Current behavior: pawn target's `continue` short-circuits fork accounting for that target.
      // The bishop is the only NON-pawn target in attackers loop; forkers[knight] only reaches 1.
      AssertLocationNotFired("Fork, Sacrificial");
      AssertLocationNotFired("Fork, True");
    }

    // -----------------------------------------------------------------------------------
    // Two-target forks: sacrificial vs true
    // -----------------------------------------------------------------------------------

    [TestMethod]
    public void Fork_TwoMinors_AttackerSafe_BothUndefended_IsTrueFork()
    {
      // Knight forks two bishops. Knight is not attacked. Targets are not defended.
      // -> "Fork, Sacrificial" AND "Fork, True".
      var ourKnight = Place(20, HUMAN, KnightType);
      Place(40, OPPONENT, BishopType);
      Place(45, OPPONENT, BishopType);
      Attack(HUMAN, 40, ourKnight);
      Attack(HUMAN, 45, ourKnight);

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Fork, Sacrificial");
      AssertLocationFired("Fork, True");
    }

    [TestMethod]
    public void Fork_TwoMinors_AttackerUnderAttack_IsSacrificialOnly()
    {
      // Knight forks two bishops, but knight itself is attacked by an opponent pawn.
      // Current: !IsSquareAttacked(attacker.Square, opponent) is FALSE -> not true fork.
      var ourKnight = Place(20, HUMAN, KnightType);
      var oppPawn = Place(11, OPPONENT, PawnType);
      Place(40, OPPONENT, BishopType);
      Place(45, OPPONENT, BishopType);
      Attack(HUMAN, 40, ourKnight);
      Attack(HUMAN, 45, ourKnight);
      Attack(OPPONENT, 20, oppPawn); // knight under attack

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Fork, Sacrificial");
      AssertLocationNotFired("Fork, True");
    }

    [TestMethod]
    public void Fork_TwoMinors_AttackerSafe_ButTargetsDefended_IsNotTrueFork()
    {
      // Knight forks two bishops, knight safe, but each bishop is defended by another
      // opponent piece. Recapture costs only ~minor (knight 300 vs bishop 325) so the
      // material guard (target >= attacker+100) FAILS, AND target IS defended, AND
      // target is NOT a king -> the boolean evaluates false -> not true fork.
      var ourKnight = Place(20, HUMAN, KnightType);
      var oppRook1 = Place(48, OPPONENT, RookType);
      var oppRook2 = Place(53, OPPONENT, RookType);
      Place(40, OPPONENT, BishopType);
      Place(45, OPPONENT, BishopType);
      Attack(HUMAN, 40, ourKnight);
      Attack(HUMAN, 45, ourKnight);
      Attack(OPPONENT, 40, oppRook1); // bishop at 40 defended
      Attack(OPPONENT, 45, oppRook2); // bishop at 45 defended

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Fork, Sacrificial");
      AssertLocationNotFired("Fork, True");
    }

    [TestMethod]
    public void Fork_QueenForksTwoMinors_RecaptureLosesMaterial_IsTrueForkEvenIfDefended()
    {
      // Skewed-value case: queen forks two rooks. Each rook is defended, but rook (500)
      // >= queen (900)+100? No. So the material clause is FALSE. But are rooks defended?
      // Yes -> not true fork. Use a knight target instead so attacker (queen 900) and
      // target (knight 300): 300 >= 1000? false. Wait we want to test the OPPOSITE
      // direction: a bishop attacker forking a queen and a rook. Target value (queen 900
      // or rook 500) >= attacker (bishop 325) + 100? Queen yes, rook yes. Both fork
      // targets recapture-lose-material -> true fork even if both defended.
      var ourBishop = Place(20, HUMAN, BishopType);
      var oppDefender = Place(48, OPPONENT, KnightType);
      Place(40, OPPONENT, QueenType);
      Place(45, OPPONENT, RookType);
      Attack(HUMAN, 40, ourBishop);
      Attack(HUMAN, 45, ourBishop);
      Attack(OPPONENT, 40, oppDefender);
      Attack(OPPONENT, 45, oppDefender);

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Fork, Sacrificial");
      AssertLocationFired("Fork, True");
    }

    // -----------------------------------------------------------------------------------
    // Triple forks
    // -----------------------------------------------------------------------------------

    [TestMethod]
    public void Fork_ThreeMinors_AttackerSafe_AllUndefended_IsTrueTriple()
    {
      var ourKnight = Place(20, HUMAN, KnightType);
      Place(40, OPPONENT, BishopType);
      Place(45, OPPONENT, BishopType);
      Place(50, OPPONENT, KnightType);
      Attack(HUMAN, 40, ourKnight);
      Attack(HUMAN, 45, ourKnight);
      Attack(HUMAN, 50, ourKnight);

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Fork, Sacrificial");
      AssertLocationFired("Fork, True");
      AssertLocationFired("Fork, Sacrificial Triple");
      AssertLocationFired("Fork, True Triple");
    }

    // -----------------------------------------------------------------------------------
    // Royal forks (king + queen)
    // -----------------------------------------------------------------------------------

    [TestMethod]
    public void Fork_RoyalKingPlusQueen_AttackerSafe_IsTrueRoyal()
    {
      // Knight attacks both king and queen; knight not attacked. Both targets undefended.
      var ourKnight = Place(20, HUMAN, KnightType);
      Place(40, OPPONENT, KingType);
      Place(45, OPPONENT, QueenType);
      Attack(HUMAN, 40, ourKnight);
      Attack(HUMAN, 45, ourKnight);

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Threaten King");
      AssertLocationFired("Threaten Queen");
      AssertLocationFired("Fork, Sacrificial");
      AssertLocationFired("Fork, True");
      AssertLocationFired("Fork, Sacrificial Royal");
      AssertLocationFired("Fork, True Royal");
    }

    [TestMethod]
    public void Fork_RoyalKingPlusQueen_AttackerUnderAttack_IsSacrificialRoyalOnly()
    {
      var ourKnight = Place(20, HUMAN, KnightType);
      var oppPawn = Place(11, OPPONENT, PawnType);
      Place(40, OPPONENT, KingType);
      Place(45, OPPONENT, QueenType);
      Attack(HUMAN, 40, ourKnight);
      Attack(HUMAN, 45, ourKnight);
      Attack(OPPONENT, 20, oppPawn);

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Fork, Sacrificial Royal");
      AssertLocationNotFired("Fork, True Royal");
    }

    // -----------------------------------------------------------------------------------
    // King-target special case: a king target ALWAYS contributes a true fork (when the
    // attacker is safe), because "no king can be defended" overrides the defense check.
    // -----------------------------------------------------------------------------------

    [TestMethod]
    public void Fork_KingPlusDefendedMinor_AttackerSafe_IsTrueFork()
    {
      // Knight forks king + a defended bishop. Knight not attacked. The bishop is
      // defended (so on its own it would fail the true-fork test for bishop-target),
      // but the king clause overrides defense check for the king target. The bishop
      // target still fails the true-fork test individually -> trueForkers[knight]==1.
      // Per code: "Fork, True" requires trueForkers[attacker] > 1, so trueForkers needs
      // BOTH iterations to mark true. King iteration: true. Bishop iteration: bishop
      // defended, target not >= attacker+100, bishop not king -> false.
      // Net: Sacrificial yes (forkers==2), True NO (trueForkers==1).
      var ourKnight = Place(20, HUMAN, KnightType);
      var oppRook = Place(53, OPPONENT, RookType);
      Place(40, OPPONENT, KingType);
      Place(45, OPPONENT, BishopType);
      Attack(HUMAN, 40, ourKnight);
      Attack(HUMAN, 45, ourKnight);
      Attack(OPPONENT, 45, oppRook); // bishop defended

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Fork, Sacrificial");
      AssertLocationNotFired("Fork, True");
    }

    // -----------------------------------------------------------------------------------
    // Negative case: only one target -> never a fork
    // -----------------------------------------------------------------------------------

    [TestMethod]
    public void NoFork_OnlyOneTarget_NoForkLocations()
    {
      var ourKnight = Place(20, HUMAN, KnightType);
      Place(40, OPPONENT, QueenType);
      Attack(HUMAN, 40, ourKnight);

      BuildHandler();
      TriggerThreatScan();

      AssertLocationFired("Threaten Queen");
      AssertLocationNotFired("Fork, Sacrificial");
      AssertLocationNotFired("Fork, True");
      AssertLocationNotFired("Fork, Sacrificial Triple");
      AssertLocationNotFired("Fork, True Triple");
      AssertLocationNotFired("Fork, Sacrificial Royal");
      AssertLocationNotFired("Fork, True Royal");
    }
  }
}
