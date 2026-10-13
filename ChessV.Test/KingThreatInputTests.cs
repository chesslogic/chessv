using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using ChessV.Base;
using ChessV.Games;
using ChessV.Games.Rules;
using ChessV.GUI;

namespace ChessV.Test
{
  [TestClass]
  public class KingThreatInputTests
  {
    [TestMethod]
    public void TerminalUndo_KeepsKingInspectionReadOnlyAfterResultIsCleared()
    {
      RunSta(() =>
      {
        var game = CreateChess();
        game.PlayMoves("f2f3 e7e5 g2g4 d8h4", MoveNotation.XBoard);
        using var control = new InputBoard();
        control.Initialize(game, game.Board, CreateTheme());
        control.RefreshKingHints(true, game.NoMoveSide);
        Assert.IsFalse(control.Presentation.KingHints.Destinations.Any(hint => hint.IsCastling));
        game.UndoMove();
        game.Result = new Result();
        control.RefreshKingHints(true, 0);
        int history = game.GameMoveNumber;
        ulong hash = game.Board.HashCode;
        Point king = Center(control, game, "e8");
        Point destination = Center(control, game, "e7");

        control.Down(king);
        Thread.Sleep(250);
        Application.DoEvents();
        control.ClickAt(king);
        control.Down(destination);
        control.ClickAt(destination);
        control.Up(destination);

        Assert.IsTrue(control.IsTerminalInspection);
        Assert.IsNull(control.Presentation.PieceLifted);
        Assert.AreEqual(history, game.GameMoveNumber);
        Assert.AreEqual(hash, game.Board.HashCode);
        Assert.IsNotNull(control.Presentation.KingHints);
      });
    }

    [TestMethod]
    public void ZeroMoveKing_CanBeInspectedWithoutStartingADrag()
    {
      RunSta(() =>
      {
        var game = CreateChess();
        game.PlayMoves("f2f3 e7e5 g2g4 d8h4", MoveNotation.XBoard);
        game.Result = new Result();
        using var control = new InputBoard();
        control.Initialize(game, game.Board, CreateTheme());
        control.Down(Center(control, game, "e1"));
        KingThreatSnapshot snapshot = control.Presentation.KingHints;
        Assert.IsNotNull(snapshot);
        Thread.Sleep(250);
        Application.DoEvents();
        foreach (string square in new[] { "d1", "e1", "f1", "d2", "e2", "f2" })
          control.MoveTo(Center(control, game, square));
        Assert.AreSame(snapshot, control.Presentation.KingHints);
        Assert.IsNull(control.Presentation.PieceLifted);
      });
    }

    [TestMethod]
    public void RefreshCancelsPendingPickup_AndHintsDoNotChangeRotatedHitMapping()
    {
      RunSta(() =>
      {
        var game = CreateChess("4r2k/8/8/8/8/8/8/3K4 w - - 0 1");
        using var control = new InputBoard();
        control.Initialize(game, game.Board, CreateTheme());
        control.RotateBoard = true;
        Point center = Center(control, game, "d1");
        Location before = control.GetLocationAtCoordinate(center);
        Assert.AreEqual(game.Board.SquareToLocation(KingThreatAnalysisTests.Square(game, "d1")), before);
        control.Down(center);
        Assert.IsNotNull(control.Presentation.KingHints);
        using Bitmap rendered = control.Presentation.Render(control.HighlightMove, control.RotateBoard);
        Assert.AreEqual(before, control.GetLocationAtCoordinate(center));
        control.RefreshKingHints(false, null);
        Thread.Sleep(250);
        Application.DoEvents();
        Assert.IsNull(control.Presentation.PieceLifted);
        Assert.IsNull(control.Presentation.KingHints);
      });
    }

    [TestMethod]
    public void AbsentKing_HasNoFabricatedHintsAndRestoredKingCanBeInspected()
    {
      RunSta(() =>
      {
        var game = CreateChess();
        int square = KingThreatAnalysisTests.Square(game, "e1");
        Piece king = game.Board.ClearSquare(square);
        using var control = new InputBoard();
        control.Initialize(game, game.Board, CreateTheme());
        control.RefreshKingHints(true, 0);
        Assert.IsNull(control.Presentation.KingHints);
        game.Board.SetSquare(king, square);
        control.RefreshKingHints(true, 0);
        Assert.AreSame(king, control.Presentation.KingHints.King);
      });
    }

    [TestMethod]
    public void ResultWithoutAMove_RefreshesTheFormAndLatchesTerminalInput()
    {
      RunSta(() =>
      {
        var game = CreateChess();
        Match match = CreateMatch(game);
        using var form = new GameForm(game, CreateTheme());
        ((ResigningHuman)match.GetPlayer(0)).Resign();
        var control = (BoardControl)form.Controls.Find("boardControl", true).Single();
        Assert.IsTrue(control.IsTerminalInspection);
        Assert.AreEqual(0, control.Presentation.KingHints.King.Player);
        Assert.AreEqual(0, game.GameMoveNumber);
      });
    }

    [TestMethod]
    public void LoadingFenWithinACompletedMatch_DoesNotClearTheTerminalLatch()
    {
      RunSta(() =>
      {
        var game = CreateChess();
        Match match = CreateMatch(game);
        using var form = new GameForm(game, CreateTheme());
        ((ResigningHuman)match.GetPlayer(0)).Resign();
        game.ClearGameState();
        foreach (CheckmateRule rule in game.GetRules().OfType<CheckmateRule>())
          foreach (var kings in rule.RoyalPieces)
            kings?.Clear();
        game.Result = new Result();
        game.LoadFEN("4r2k/8/8/8/8/8/8/3K4 w - - 0 1");

        var control = (BoardControl)form.Controls.Find("boardControl", true).Single();
        Assert.IsTrue(control.IsTerminalInspection);
        Assert.AreEqual(KingThreatAnalysisTests.Square(game, "d1"), control.Presentation.KingHints.KingSquare);
        Assert.AreSame(match, game.Match);
        match.Finished?.Invoke(match);
        Assert.IsNotNull(control.Presentation.KingHints);
        Assert.AreEqual(KingThreatAnalysisTests.Square(game, "d1"), control.Presentation.KingHints.KingSquare);
      });
    }

    [TestMethod]
    public void LeavingDuringPendingPickup_PreservesTheLiftedPiecePosition()
    {
      RunSta(() =>
      {
        var game = CreateChess("4r2k/8/8/8/8/8/8/3K4 w - - 0 1");
        using var control = new InputBoard();
        control.Initialize(game, game.Board, CreateTheme());
        Point king = Center(control, game, "d1");
        control.Down(king);
        control.Up(king);
        control.ClickAt(king);
        control.LeaveBoard();
        Thread.Sleep(250);
        Application.DoEvents();

        Assert.AreSame(game.Board[KingThreatAnalysisTests.Square(game, "d1")],
          control.Presentation.PieceLifted);
        Assert.AreEqual(king, control.Presentation.MouseCoordinate);
      });
    }

    [DataTestMethod]
    [DataRow("c1", true)]
    [DataRow("e1", false)]
    public void StandardKingPickup_PreservesLegalAndIllegalDestinations(string target, bool legal)
    {
      RunSta(() =>
      {
        var game = CreateChess("4r2k/8/8/8/8/8/8/3K4 w - - 0 1");
        CreateMatch(game);
        using var control = new InputBoard();
        control.Initialize(game, game.Board, CreateTheme());
        Piece king = game.Board[KingThreatAnalysisTests.Square(game, "d1")];
        control.Down(Center(control, game, "d1"));
        Assert.IsNotNull(control.Presentation.KingHints);
        Thread.Sleep(250);
        Application.DoEvents();
        Point destination = Center(control, game, target);
        control.MoveTo(destination);
        control.Up(destination);

        Assert.AreEqual(legal ? 1 : 0, game.GameMoveNumber);
        Assert.AreEqual(KingThreatAnalysisTests.Square(game, legal ? target : "d1"), king.Square);
        Assert.IsNull(control.Presentation.PieceLifted);
        Assert.IsNull(control.Presentation.KingHints);
      });
    }

    [TestMethod]
    [DoNotParallelize]
    public void ApmwHumanKing_CanStillSubmitALegalButDangerousMove()
    {
      RunSta(() =>
      {
        ApmwCore previous = ApmwCore._instance;
        try
        {
          var core = new ApmwCore
          {
            foundPockets = 0, foundPocketRange = 0, foundPocketGems = 0, foundPawns = 0,
            foundMinors = 0, foundMajors = 0, foundJacks = 0, foundQueens = 0,
            foundAmazons = 0, foundConsuls = 0, foundKingPromotions = 0,
            foundPawnForwardness = 0, foundChessmen = 0, foundMaterialBudget = 0,
            foundCastlers = 0, foundPlayAsWhite = 1, GeriProvider = () => 0
          };
          ApmwCore._instance = core;
          ApmwGeometryProfile profile = ApmwProfiles.Stages.Single(p => p.GameName == ApmwProfiles.StandardGameName);
          core.PlayerPieceSetProvider = files =>
            (new Dictionary<KeyValuePair<int, int>, PieceType>
              { { new KeyValuePair<int, int>(profile.PlayerBackRank, profile.KingFile), core.kings[0] } }, "QRNB");
          var manager = new ChessV.Manager.Manager();
          var game = (ApmwChessGame)manager.CreateGame(profile.GameName, null);
          CreateMatch(game);
          game.ClearGameState();
          foreach (CheckmateRule rule in game.GetRules().OfType<CheckmateRule>())
            foreach (var kings in rule.RoyalPieces)
              kings?.Clear();
          game.LoadFEN("4r2k/8/8/8/8/8/8/3K4 w - - - 0 1");
          using var control = new InputBoard();
          control.Initialize(game, game.Board, CreateTheme());
          int target = KingThreatAnalysisTests.Square(game, "e1");
          Piece king = game.Board[KingThreatAnalysisTests.Square(game, "d1")];
          string before = KingThreatAnalysisTests.State(game);
          int notifications = 0;
          core.NewMoveSetup.Add(_ => notifications++);
          core.NewMovePlayed.Add(_ => notifications++);
          core.MatchFinished.Add(_ => notifications++);

          control.Down(Center(control, game, "d1"));
          KingDestinationHint hint = control.Presentation.KingHints.Destinations
            .Single(h => h.Square == target && !h.IsCastling);
          Assert.IsTrue(hint.IsLegalMove);
          Assert.IsFalse(hint.IsSafe);
          Assert.IsTrue(hint.Threats.Count > 0);
          Assert.AreEqual(before, KingThreatAnalysisTests.State(game));
          Assert.AreEqual(0, notifications);
          Thread.Sleep(250);
          Application.DoEvents();
          Point destination = Center(control, game, "e1");
          control.MoveTo(destination);
          control.Up(destination);

          Assert.AreEqual(1, game.GameMoveNumber);
          Assert.AreEqual(target, king.Square);
          Assert.IsNull(control.Presentation.KingHints);
        }
        finally
        {
          ApmwCore._instance = previous;
        }
      });
    }

    private static Match CreateMatch(Game game)
    {
      var factory = new TimerFactory();
      var match = new Match(game, new PGNGame(), null, factory);
      game.Match = match;
      match.SetPlayer(0, new ResigningHuman(factory));
      match.SetPlayer(1, new HumanPlayer(null, factory));
      match.SetTimeControl(new TimeControl("inf"));
      match.Start();
      return match;
    }

    private sealed class ResigningHuman : HumanPlayer
    {
      public ResigningHuman(TimerFactory factory) : base(null, factory) { }
      public void Resign() => forfeit(ResultType.Resignation);
    }

    internal static Chess CreateChess(string fen = null)
    {
      var game = new Chess();
      game.Initialize((GameAttribute)typeof(Chess).GetCustomAttributes(typeof(GameAttribute), false).Single(), null, null);
      if (fen != null)
      {
        game.ClearGameState();
        foreach (CheckmateRule rule in game.GetRules().OfType<CheckmateRule>())
          foreach (var kings in rule.RoyalPieces)
            kings?.Clear();
        game.LoadFEN(fen);
      }
      return game;
    }

    internal static Theme CreateTheme()
    {
      PresentationFactory.Initialize();
      PieceSetLibrary.Initialize();
      var scheme = new ColorScheme { NumberOfColors = 2, BorderColor = Color.Gray,
        TextColor = Color.Black, HighlightColor = Color.Gold };
      scheme.SquareColors.Add(0, Color.Beige);
      scheme.SquareColors.Add(1, Color.BurlyWood);
      scheme.PlayerColors.Add(0, Color.White);
      scheme.PlayerColors.Add(1, Color.Black);
      return new Theme(scheme, PieceSetLibrary.Default, 2, null);
    }

    internal static Point Center(BoardControl control, Game game, string notation)
    {
      var pieces = (PieceSet)control.Theme.PieceSet;
      int size = Math.Max(pieces.Width, pieces.Height) + 6;
      Location location = game.Board.SquareToLocation(KingThreatAnalysisTests.Square(game, notation));
      int column = control.RotateBoard ? game.Board.NumFiles - 1 - location.File : location.File;
      int row = control.RotateBoard ? location.Rank : game.Board.NumRanks - 1 - location.Rank;
      return new Point(24 + column * size + size / 2, 24 + row * size + size / 2);
    }

    internal static void RunSta(Action action)
    {
      Exception error = null;
      var thread = new Thread(() =>
      {
        try { action(); }
        catch (Exception exception) { error = exception; }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      thread.Join();
      if (error != null)
        ExceptionDispatchInfo.Capture(error).Throw();
    }

    internal class InputBoard : BoardControl
    {
      public void Down(Point point) => OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, point.X, point.Y, 0));
      public void Up(Point point) => OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, point.X, point.Y, 0));
      public void ClickAt(Point point) => OnMouseClick(new MouseEventArgs(MouseButtons.Left, 1, point.X, point.Y, 0));
      public void MoveTo(Point point) => OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, point.X, point.Y, 0));
      public void LeaveBoard() => OnMouseLeave(EventArgs.Empty);
    }
  }
}
