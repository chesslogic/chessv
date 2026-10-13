using System;
using System.Linq;
using System.Collections.Generic;
using ChessV.Games.Rules;
using ChessV.Games;
using ChessV.Base;

namespace ChessV.Test
{
  [TestClass]
  [DoNotParallelize]
  public class KingThreatAnalysisTests
  {
    [DataTestMethod]
    [DataRow("7k/6Q1/5K2/8/8/8/8/8 b - - 0 1", false)]
    [DataRow("7k/5Q2/6K1/8/8/8/8/8 b - - 0 1", true)]
    public void MateAndStalemate_ExplainTheImmobileKing(string fen, bool safe)
    {
      Game game = CreateGame(fen);
      KingThreatSnapshot hints = KingThreatAnalysis.Analyze(game, game.Board[Square(game, "h8")]);

      Assert.AreEqual(safe, hints.CurrentSquare.IsSafe);
      Assert.IsTrue(hints.Destinations.Where(h => h.Square != hints.KingSquare).All(h => !h.IsLegalMove),
        string.Join(", ", hints.Destinations.Where(h => h.IsLegalMove)
          .Select(h => game.Board.GetDefaultSquareNotation(h.Square))));
      Assert.IsTrue(hints.Destinations.All(h => h.IsComplete),
        string.Join(", ", hints.Destinations.Where(h => !h.IsComplete)
          .Select(h => $"{game.Board.GetDefaultSquareNotation(h.Square)}:{h.Threats.Count}")));
      Assert.IsTrue(hints.Destinations.Any(h => h.Threats.Count > 0));
    }

    [DataTestMethod]
    [DataRow("4r2k/8/8/8/8/8/8/3K4 w - - 0 1", "d1", "e2", "e8")]
    [DataRow("4r2k/8/8/8/8/8/4r3/4K3 w - - 0 1", "e1", "e2", "e8")]
    [DataRow("4k3/8/8/8/8/8/4r3/3KR3 w - - 0 1", "d1", "d2", "e2")]
    public void DestinationDanger_IncludesProtectedCapturesAndPinnedAttackers(
      string fen, string origin, string target, string attacker)
    {
      Game game = CreateGame(fen);
      KingDestinationHint hint = KingThreatAnalysis.Analyze(game, game.Board[Square(game, origin)])
        .Destinations.Single(h => h.Square == Square(game, target) && !h.IsCastling);

      Assert.IsFalse(hint.IsSafe);
      Assert.IsFalse(hint.IsLegalMove);
      Assert.IsTrue(hint.IsComplete);
      Assert.AreEqual(Square(game, attacker), hint.Threats.Single().SourceSquare);
    }

    [TestMethod]
    public void FriendlyBlocker_IsNotTreatedAsAHypotheticalCapture()
    {
      Game game = CreateGame("7k/8/8/8/8/8/3P4/4K3 w - - 0 1");
      KingDestinationHint hint = KingThreatAnalysis.Analyze(game, game.Board[Square(game, "e1")])
        .Destinations.Single(h => h.Square == Square(game, "d2"));
      Assert.IsTrue(hint.FriendlyOccupied);
      Assert.IsFalse(hint.IsSafe);
      Assert.AreEqual(0, hint.Threats.Count);
    }

    [DataTestMethod]
    [DataRow("4i2k/8/8/8/4P3/8/8/4K3 w - - 0 1", 1)]
    [DataRow("4i2k/8/8/8/8/8/8/4K3 w - - 0 1", 0)]
    [DataRow("4i2k/8/8/8/4P3/4P3/8/4K3 w - - 0 1", 0)]
    public void CannonThreat_RequiresExactlyOneScreen(string fen, int threats)
    {
      Game game = CreateGame(fen);
      KingDestinationHint hint = KingThreatAnalysis.Analyze(game, game.Board[Square(game, "e1")]).CurrentSquare;
      Assert.AreEqual(threats, hint.Threats.Count);
      if (threats > 0)
        Assert.AreEqual(Square(game, "e4"), hint.Threats.Single().Path.ScreenSquare);
    }

    [TestMethod]
    public void CapturingCannonScreen_RemovesTheThreatAtTheDestination()
    {
      Game game = CreateGame("4i2k/8/8/8/4p3/4K3/8/8 w - - 0 1");
      KingDestinationHint hint = KingThreatAnalysis.Analyze(game, game.Board[Square(game, "e3")])
        .Destinations.Single(h => h.Square == Square(game, "e4"));
      Assert.IsTrue(hint.IsSafe);
      Assert.AreEqual(0, hint.Threats.Count);
    }

    [TestMethod]
    public void CheckersWrappedLanding_UsesTheActualJumpRoute()
    {
      Game game = CreateGame("7k/8/8/8/1e6/K7/8/8 w - - 0 1");
      CaptureThreat threat = KingThreatAnalysis.Analyze(game, game.Board[Square(game, "a3")])
        .CurrentSquare.Threats.Single();
      CollectionAssert.AreEqual(new[] { Square(game, "b4"), Square(game, "a3"), Square(game, "h2") },
        threat.Path.Squares.ToArray());
      Assert.IsTrue(threat.Path.WrapsFiles);
    }

    [TestMethod]
    public void FailedOccupancyQueries_RestoreIdentityHashesCountersAndBitboards()
    {
      Game game = CreateGame("4r2k/8/8/8/8/8/4r3/4K3 w - - 0 1");
      Piece king = game.Board[Square(game, "e1")];
      Piece victim = game.Board[Square(game, "e2")];
      int settled = 0, played = 0, undone = 0;
      game.PositionSettled += _ => settled++;
      game.MovePlayed += _ => played++;
      game.MoveTakenBack += () => undone++;
      string before = State(game);
      int rootCursor = game.RootMoveListForTest.MoveCursor;

      Assert.ThrowsException<InvalidOperationException>(() =>
        game.Board.InspectRelocation<int>(king, victim.Square, () => throw new InvalidOperationException("probe")));
      var replacement = new Piece(game, king.Player, king.PieceType, -1);
      Assert.ThrowsException<InvalidOperationException>(() =>
        game.Board.InspectReplacement<int>(replacement, victim.Square, () => throw new InvalidOperationException("probe")));
      KingThreatAnalysis.Analyze(game, king);

      Assert.AreEqual(before, State(game));
      Assert.AreSame(king, game.Board[Square(game, "e1")]);
      Assert.AreSame(victim, game.Board[Square(game, "e2")]);
      Assert.AreEqual(-1, replacement.Square);
      Assert.AreEqual(rootCursor, game.RootMoveListForTest.MoveCursor);
      Assert.AreEqual(0, settled + played + undone);
    }

    internal static string State(Game game)
    {
      var state = new List<string>
      {
        game.FEN.ToString(), game.Board.HashCode.ToString(), game.Board.PawnHashCode.ToString(),
        game.Board.MaterialHashCode.ToString(), game.Result.ToString(), game.CurrentSide.ToString(),
        game.Ply.ToString(), game.GameMoveNumber.ToString(), game.GameTurnNumber.ToString(),
        game.Board.GetMidgameMaterialEval(0).ToString(), game.Board.GetMidgameMaterialEval(1).ToString()
      };
      for (int player = 0; player < game.NumPlayers; player++)
        for (int type = 0; type < game.NPieceTypes; type++)
        {
          BitBoard bits = game.Board.GetPieceTypeBitboard(player, type);
          while (!bits.IsEmpty)
          {
            int square = bits.LSB;
            state.Add($"bit:{player}:{type}:{square}");
            bits.ClearBit(square);
          }
        }
      state.AddRange(game.GetPieceList().Select(p => $"{p.Square}:{p.TypeNumber}:{p.MoveCount}"));
      var rule = game.FindRule(typeof(CheckmateRule), true) as CheckmateRule;
      if (rule != null)
        state.AddRange(rule.RoyalPieces.SelectMany(p => p ?? new HashSet<Piece>())
          .Select(p => $"royal:{p.Player}:{p.Square}:{p.TypeNumber}").OrderBy(p => p));
      return string.Join("|", state);
    }

    [TestMethod]
    public void EveryApmwGeometry_AnalyzesPromotedKingsWithoutChangingProgress()
    {
      ApmwCore previous = ApmwCore._instance;
      try
      {
        var manager = new ChessV.Manager.Manager();
        foreach (ApmwGeometryProfile profile in ApmwProfiles.Stages)
          foreach (int promotion in new[] { 1, 2 })
          {
            var core = new ApmwCore
            {
              foundPockets = 0, foundPocketRange = 0, foundPocketGems = 0, foundPawns = 0,
              foundMinors = 0, foundMajors = 0, foundJacks = 0, foundQueens = 0,
              foundAmazons = 0, foundConsuls = 0, foundKingPromotions = promotion,
              foundPawnForwardness = 0, foundChessmen = 0, foundMaterialBudget = 0,
              foundCastlers = 0, foundPlayAsWhite = 1, GeriProvider = () => 0
            };
            ApmwCore._instance = core;
            core.PlayerPieceSetProvider = files =>
            {
              var pieces = new Dictionary<KeyValuePair<int, int>, PieceType>();
              for (int file = 0; file < files; file++)
                pieces.Add(new KeyValuePair<int, int>(profile.PlayerPawnRank, file),
                  core.pawns.Single(p => p.Name == "Pawn"));
              pieces.Add(new KeyValuePair<int, int>(profile.PlayerBackRank, profile.KingFile), core.kings[0]);
              pieces.Add(new KeyValuePair<int, int>(profile.PlayerBackRank, 0),
                core.majors.Single(p => p.Name == "Rook"));
              pieces.Add(new KeyValuePair<int, int>(profile.PlayerBackRank, files - 1),
                core.majors.Single(p => p.Name == "Rook"));
              pieces.Add(new KeyValuePair<int, int>(profile.PlayerPawnRank - 1, files / 2), core.kings[promotion]);
              return (pieces, "QRNB");
            };
            var game = (ApmwChessGame)manager.CreateGame(profile.GameName, null);
            int notifications = 0;
            core.NewMoveSetup.Add(_ => notifications++);
            core.NewMovePlayed.Add(_ => notifications++);
            core.MatchFinished.Add(_ => notifications++);
            game.PositionSettled += _ => notifications++;
            string before = State(game);
            Piece[] kings = KingThreatAnalysis.GetKings(game, 0).ToArray();
            Assert.AreEqual(2, kings.Length, profile.StageId);
            Piece promoted = kings.Single(p => p.PieceType == core.kings[promotion]);
            KingThreatSnapshot snapshot = KingThreatAnalysis.Analyze(game, promoted);

            Assert.IsTrue(snapshot.Destinations.Count > 9, profile.StageId + " promoted movement");
            Assert.IsTrue(snapshot.Destinations.All(h => h.Square >= 0 && h.Square < game.Board.NumSquares));
            Assert.IsFalse(snapshot.Destinations.Any(h => h.IsCastling), "Only the primary king may castle.");
            int count = game.GetRootMoves(out MoveInfo[] legal, promoted);
            foreach (MoveInfo move in legal.Take(count))
              Assert.IsTrue(snapshot.Destinations.Any(h => h.Square == move.ToSquare && h.IsLegalMove));
            Assert.AreEqual(before, State(game), profile.StageId);
            Assert.AreEqual(0, notifications);
            Assert.AreEqual(promotion, core.foundKingPromotions);
          }
      }
      finally
      {
        ApmwCore._instance = previous;
      }
    }

    [TestMethod]
    public void VacatingKingOrigin_RevealsDestinationAttackWithoutMovingTheKing()
    {
      CheckersCannonTestGame game = CreateGame("4r2k/8/8/8/8/8/4K3/8 w - - 0 1");
      Piece king = game.Board[Square(game, "e2")];
      ulong hash = game.Board.HashCode;
      int moves = game.RootMoveListForTest.Count;

      KingThreatSnapshot hints = KingThreatAnalysis.Analyze(game, king);

      KingDestinationHint destination = hints.Destinations.Single(h => h.Square == Square(game, "e1"));
      Assert.AreEqual(Square(game, "e8"), destination.Threats.Single().SourceSquare);
      Assert.AreSame(king, game.Board[Square(game, "e2")]);
      Assert.IsNull(game.Board[Square(game, "e1")]);
      Assert.AreEqual(hash, game.Board.HashCode);
      Assert.AreEqual(moves, game.RootMoveListForTest.Count);
      Assert.AreEqual(0, game.GameMoveNumber);
    }

    internal static CheckersCannonTestGame CreateGame(string fen)
    {
      var game = new CheckersCannonTestGame();
      var attribute = (GameAttribute)typeof(CheckersCannonTestGame)
        .GetCustomAttributes(typeof(GameAttribute), false).Single();
      game.Initialize(attribute, null, null);
      game.LoadFEN(fen);
      return game;
    }

    [TestMethod]
    public void CheckersCaptureChain_ExplainsIntermediateKingCaptureAndLandingSquares()
    {
      CheckersCannonTestGame game = CreateGame("7k/8/1e6/2P5/8/4K3/8/8 w - - 0 1");
      Piece king = game.Board[Square(game, "e3")];

      CaptureThreat threat = KingThreatAnalysis.Analyze(game, king).CurrentSquare.Threats
        .Single(t => t.SourceSquare == Square(game, "b6"));

      Assert.IsNotNull(threat.Path);
      CollectionAssert.AreEqual(new[] { "b6", "c5", "d4", "e3", "f2" }
        .Select(n => Square(game, n)).ToArray(), threat.Path.Squares.ToArray());
    }

    internal static int Square(Game game, string notation)
    {
      return game.Board.DefaultNotationToSquare(notation);
    }
  }
}
