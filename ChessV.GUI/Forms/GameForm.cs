
/***************************************************************************

                                 ChessV

                  COPYRIGHT (C) 2012-2019 BY GREG STRONG

This file is part of ChessV.  ChessV is free software; you can redistribute
it and/or modify it under the terms of the GNU General Public License as 
published by the Free Software Foundation, either version 3 of the License, 
or (at your option) any later version.

ChessV is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or 
FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for 
more details; the file 'COPYING' contains the License text, but if for
some reason you need a copy, please visit <http://www.gnu.org/licenses/>.

****************************************************************************/

using ChessV.Games;
using ChessV.Games.Rules;
using ChessV.Games.Rules.Apmw;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ChessV.GUI
{
  public partial class GameForm : Form
  {
    // *** PROPERTIES *** //

    public Game Game { get; protected set; }
    public Theme Theme { get; protected set; }
    public Color WindowBackgroundColor { get; protected set; }
    public bool ReviewMode { get; protected set; }


    // *** DATA MEMBERS *** //

    protected bool gameStarted;
    protected Piece contextMenuPiece;
    protected List<string> moveDescriptions;
    protected int currentMoveNumber;
    protected int sideOnClock;
    protected int reviewCursor;
    protected Stack<MoveInfo> reviewRedoStack;
    private DebugForm debugForm;
    private double? currentEval;
    protected bool engineThinking;
    protected EngineStatisticsForm engineStatsForm;
    protected Result gameResult;


    // *** CONSTRUCTION *** //

    #region Constructor
    public GameForm(Game game)
    {
      Game = game;
      Theme = ThemeFactory.CreateTheme(game);
      moveDescriptions = new List<string>();
      currentMoveNumber = 0;
      gameStarted = false;
      currentEval = null;
      ReviewMode = false;

      InitializeComponent();
      boardControl.Initialize(game, game.Board, Theme, this);
      mbControl.Initialize(game, game.Board, Theme);
      ehControl.Initialize(game, Theme);
      menuitem_ComputerPlays0.Text = "Computer Plays " + game.PlayerNames[0];
      menuitem_ComputerPlays1.Text = "Computer Plays " + game.PlayerNames[1];
      updateTheme();
      game.MovePlayed += MovePlayed;
      game.ThinkingCallback += updateThinking;

      if (game.Match != null)
      {
        game.Match.HumanEnabled += HumanEnabled;
        if (game.Match.GetPlayer(0) != null)
        {
          game.Match.GetPlayer(0).StartedThinking += ThinkingStartedPlayer0;
          game.Match.GetPlayer(0).StoppedThinking += ThinkingStopped;
        }
        if (game.Match.GetPlayer(1) != null)
        {
          game.Match.GetPlayer(1).StartedThinking += ThinkingStartedPlayer1;
          game.Match.GetPlayer(1).StoppedThinking += ThinkingStopped;
        }
      }

      debugForm = new DebugForm(Game.MessageLog, this);

      debugForm.Visible = false;

      game.Match.Deathed += (match, reason) => GameEnded(reason);
    }
    #endregion



    #region ThinkingStartedPlayer0
    public void ThinkingStartedPlayer0(long timeleft)
    {
      sideOnClock = 0;
      engineThinking = Game.ComputerControlled[0];
      if (Game.ComputerControlled[0])
        listThinking1.Items.Clear();
      labelTime0.Text = timeInMillisecondsToString(timeleft);
      if (Game.GetType() == typeof(ApmwChessGame))
      {
        gems_label.Text = "Gems: " + Game.CurrentPlayer.Gems;
      }
      timer.Start();
    }
    #endregion

    #region ThinkingStartedPlayer1
    public void ThinkingStartedPlayer1(long timeleft)
    {
      sideOnClock = 1;
      engineThinking = Game.ComputerControlled[1];
      if (Game.ComputerControlled[1])
        listThinking1.Items.Clear();
      labelTime1.Text = timeInMillisecondsToString(timeleft);
      if (Game.GetType() == typeof(ApmwChessGame))
      {
        gems_label.Text = "Gems: " + Game.CurrentPlayer.Gems;
      }
      timer.Start();
    }
    #endregion

    #region ThinkingStopped
    public void ThinkingStopped()
    {
      if (timer.Enabled)
        timer.Stop();
    }
    #endregion

    #region GameEnded
    public void GameEnded(string message)
    {
      MessageBox.Show(message);
    }
    #endregion

    #region MovePlayed
    public void MovePlayed(MoveInfo move)
    {
      if (!ReviewMode)
      {
        //	update the move list
        int number = Game.GameMoveNumber;
        addMoveToMoveList(number, move, Game.GameTurnNumber);
        //	Check for game end
        if (Game.Result.Type != ResultType.NoResult)
        {
          Game.Cleanup();
          if (!Game.IsAutomatedMatch)
            //	Show message if the game has ended
            MessageBox.Show(Game.Result.VerboseString);
          else
            Close();
        }
        if (currentEval != null)
        {
          ehControl.AddEvaluation(move.Player, (double)currentEval);
          currentEval = null;
        }
        if (!Game.ComputerControlled[sideOnClock])
        {
          if (sideOnClock == 0 && !Game.Match.GetPlayer(0).IsHuman)
          {
            Game.Match.SetPlayerToHuman(0);
            Game.Match.GetPlayer(0).StartedThinking += ThinkingStartedPlayer0;
            Game.Match.GetPlayer(0).StoppedThinking += ThinkingStopped;
          }
          else if (sideOnClock == 1 && !Game.Match.GetPlayer(1).IsHuman)
          {
            Game.Match.SetPlayerToHuman(1);
            Game.Match.GetPlayer(1).StartedThinking += ThinkingStartedPlayer1;
            Game.Match.GetPlayer(1).StoppedThinking += ThinkingStopped;
          }
        }
        bool whiteIsHuman = Game.Match.GetPlayer(0).IsHuman;
        if (null != label_check)
        {
          label_check.Visible = false;
          ApmwStalemateRule rule = (ApmwStalemateRule)Game.FindRule(typeof(ApmwStalemateRule));
          if (rule != null) {
            foreach (Piece king in rule.RoyalPieces[whiteIsHuman ? 0 : 1])
            {
              int square = king.Square;
              if (Game.IsSquareAttacked(square, whiteIsHuman ? 1 : 0))
              {
                label_check.Visible = true;
                label_check.Update();
                break;
              }
            }
          }
        }
      }
    }
    #endregion

    #region ShowContextMenu
    public void ShowContextMenu(Point pt, Piece piece)
    {
      contextMenuPiece = piece;
      menuPieceContext.Show(pt);
    }
    #endregion


    // *** EVENT HANDLERS *** //

    #region menuitem_ComputerPlays0_Click
    private void menuitem_ComputerPlays0_Click(object sender, EventArgs e)
    {
      Game.ComputerControlled[0] = !Game.ComputerControlled[0];
      Game.Match.GetPlayer(0).Name = Game.ComputerControlled[0] ? "ChessV" : "Player";
      labelPlayer0.Text = Game.Match.GetPlayer(0).Name;
      if (Game.ComputerControlled[0])
      {
        Game.Match.SetPlayerToInternalEngine(0);
        Game.Match.GetPlayer(0).StartedThinking += ThinkingStartedPlayer0;
        Game.Match.GetPlayer(0).StoppedThinking += ThinkingStopped;
        if (sideOnClock == 0)
        {
          pictFirst.Image = global::ChessV.GUI.Properties.Resources.icon_gray_first;
          pictPrevious.Image = global::ChessV.GUI.Properties.Resources.icon_gray_previous;
          listThinking1.Items.Clear();
          Application.DoEvents();
          engineThinking = true;
          Game.Match.GetPlayer(0).Go(Game.Match.GetPlayer(0));
        }
      }
      else if (sideOnClock != 0)
      {
        Game.Match.SetPlayerToHuman(0);
        Game.Match.GetPlayer(0).StartedThinking += ThinkingStartedPlayer0;
        Game.Match.GetPlayer(0).StoppedThinking += ThinkingStopped;
      }
    }
    #endregion

    #region menuitem_ComputerPlays1_Click
    private void menuitem_ComputerPlays1_Click(object sender, EventArgs e)
    {
      Game.ComputerControlled[1] = !Game.ComputerControlled[1];
      Game.Match.GetPlayer(1).Name = Game.ComputerControlled[1] ? "ChessV" : "Player";
      labelPlayer1.Text = Game.Match.GetPlayer(1).Name;
      if (Game.ComputerControlled[1])
      {
        Game.Match.SetPlayerToInternalEngine(1);
        Game.Match.GetPlayer(1).StartedThinking += ThinkingStartedPlayer1;
        Game.Match.GetPlayer(1).StoppedThinking += ThinkingStopped;
        if (sideOnClock == 1)
        {
          pictFirst.Image = global::ChessV.GUI.Properties.Resources.icon_gray_first;
          pictPrevious.Image = global::ChessV.GUI.Properties.Resources.icon_gray_previous;
          listThinking1.Items.Clear();
          Application.DoEvents();
          engineThinking = true;
          Game.Match.GetPlayer(1).Go(Game.Match.GetPlayer(1));
        }
      }
      else if (sideOnClock != 1)
      {
        Game.Match.GetPlayer(1).StartedThinking += ThinkingStartedPlayer1;
        Game.Match.GetPlayer(1).StoppedThinking += ThinkingStopped;
        Game.Match.SetPlayerToHuman(1);
      }
    }
    #endregion

    #region GameForm_Load
    private void GameForm_Load(object sender, EventArgs e)
    {
      Text = Game.GameAttribute.GameName;
      // 569 == listMoves.Width;
      // 477 == panelClocks.Width;
      // 5 == splitterPanel.SplitterWidth
      Width = boardControl.Presentation.NativeSize().Width + splitContainer1.SplitterWidth + 474 + 5 + 24;
      boardControl.Size = boardControl.Presentation.NativeSize();
      splitContainer1.SplitterDistance = boardControl.Presentation.NativeSize().Width + 4;
      splitContainer2.SplitterDistance = boardControl.Presentation.NativeSize().Height + 4;
      Height = boardControl.Presentation.NativeSize().Height + 24 + 224 + 10; // menustrip + thinking + 2 borders
      if (Game.GetCustomThemes() == null)
        menuitem_EnableCustomTheme.Visible = false;
      else
      {
        menuitem_EnableCustomTheme.Text = "Enable Custom Theme for " + Game.Name;
        menuitem_EnableCustomTheme.Checked = Theme.CustomThemeName != null;
      }
      if (Game.ComputerControlled[0])
        labelPlayer0.Text = Game.Match.GetPlayer(0).Name;
      else
        labelPlayer0.Text = "Player";
      if (Game.ComputerControlled[1])
        labelPlayer1.Text = Game.Match.GetPlayer(1).Name;
      else
        labelPlayer1.Text = "Player";
      if (Game.GetType() == typeof(ApmwChessGame))
      {
        evaluationTabControl.SelectedIndex = 1;
      }
      Game.MoveTakenBack += moveTakenBack;
      timer.Start();
    }
    #endregion

    #region moveTakenBack
    private void moveTakenBack()
    {
      if (!ReviewMode)
      {
        if (Char.IsDigit(moveDescriptions[moveDescriptions.Count - 1][0]))
          currentMoveNumber--;
        if (moveDescriptions.Count > 0)
          moveDescriptions.RemoveAt(moveDescriptions.Count - 1);
        if (listMoves.Items.Count > 0)
          listMoves.Items.RemoveAt(listMoves.Items.Count - 1);
      }
      mbControl.UpdateUnmatchedPieceLists();
      mbControl.Invalidate();
    }
    #endregion

    #region menuitem_StaticEvaluation_Click
    private void menuitem_StaticEvaluation_Click(object sender, EventArgs e)
    {
      int eval = Game.Evaluate();
      string evalDisplay = Game.FormatScoreForDisplay(eval);
      MessageBox.Show("Eval: " + evalDisplay);
    }
    #endregion

    #region quickAnalysisToolStripMenuItem_Click
    private void quickAnalysisToolStripMenuItem_Click(object sender, EventArgs e)
    {
      listThinking1.Items.Clear();
      Application.DoEvents();
      Game.Think(null);
    }
    #endregion

    #region perftToolStripMenuItem_Click
    private void perftToolStripMenuItem_Click(object sender, EventArgs e)
    {
      PerftForm perftForm = new PerftForm(Game);
      perftForm.ShowDialog();
    }
    #endregion

    #region appearanceToolStripMenuItem_Click
    private void appearanceToolStripMenuItem_Click(object sender, EventArgs e)
    {
      AppearanceSettingsForm form = new AppearanceSettingsForm(Theme);
      if (form.ShowDialog() == DialogResult.OK)
      {
        try
        {
          boardControl.Presentation.UpdateTheme(Theme);
        }
        catch (Exception ex)
        {
          ExceptionForm exform = new ExceptionForm(ex, Game);
          exform.ShowDialog();
          return;
        }
        boardControl.Invalidate();
        mbControl.UpdateTheme(Theme);
        updateTheme();
        Theme.SaveToRegistry(Game.RegistryKey);
      }
    }
    #endregion

    #region optionsToolStripMenuItem_DropDownOpening
    private void optionsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
    {
      menuitem_UncheckeredBoard.Checked = Theme.NSquareColors == 1;
      menuitem_CheckeredBoard.Checked = Theme.NSquareColors == 2;
      menuitem_ThreeColorBoard.Checked = Theme.NSquareColors == 3;
      menuitem_ThreeColorBoard.Enabled =
        (Theme.ColorScheme.SquareTextures != null && Theme.ColorScheme.SquareTextures.ContainsKey(2)) ||
        (Theme.ColorScheme.SquareTextures == null && Theme.ColorScheme.SquareColors.ContainsKey(2));
      menuitem_HighlightComputerMove.Checked = boardControl.HighlightMove;
      menuitem_RotateBoard.Checked = boardControl.RotateBoard;
    }
    #endregion

    #region menuitem_UncheckeredBoard_Click
    private void menuitem_UncheckeredBoard_Click(object sender, EventArgs e)
    {
      if (Theme.NSquareColors != 1)
      {
        Theme.NSquareColors = 1;
        boardControl.Presentation.UpdateTheme(Theme);
        boardControl.Invalidate();
      }
    }
    #endregion

    #region menuitem_CheckeredBoard_Click
    private void menuitem_CheckeredBoard_Click(object sender, EventArgs e)
    {
      if (Theme.NSquareColors != 2)
      {
        Theme.NSquareColors = 2;
        boardControl.Presentation.UpdateTheme(Theme);
        boardControl.Invalidate();
      }
    }
    #endregion

    #region menuitem_ThreeColorBoard_Click
    private void menuitem_ThreeColorBoard_Click(object sender, EventArgs e)
    {
      if (Theme.NSquareColors != 3)
      {
        Theme.NSquareColors = 3;
        boardControl.Presentation.UpdateTheme(Theme);
        boardControl.Invalidate();
      }
    }
    #endregion

    #region menuitem_Exit_Click
    private void menuitem_Exit_Click(object sender, EventArgs e)
    {
      Close();
    }
    #endregion

    #region menu_Game_DropDownOpening
    private void menu_Game_DropDownOpening(object sender, EventArgs e)
    {
      menuitem_ComputerPlays0.Checked = Game.ComputerControlled[0];
      menuitem_ComputerPlays1.Checked = Game.ComputerControlled[1];

      //	Several options are only supported with human 
      //	players and/or the internal engine
      Player player0 = Game.Match.GetPlayer(0);
      Player player1 = Game.Match.GetPlayer(1);
      if ((player0 is HumanPlayer || player0 is InternalEngine) &&
        (player1 is HumanPlayer || player1 is InternalEngine) && !ReviewMode)
      {
        menuitem_ComputerPlays0.Enabled = true;
        menuitem_ComputerPlays1.Enabled = true;
        menuitem_TakeBackMove.Enabled = true;
        menuitem_TakeBackAllMoves.Enabled = true;
      }

      //	Take Back Move and Take Back All Moves are
      //	only available if we have moves to take back
      if (Game.BoardMoveStack.MoveCount == 0 || ReviewMode)
      {
        menuitem_TakeBackMove.Enabled = false;
        menuitem_TakeBackAllMoves.Enabled = false;
      }

      //	Computer Plays toggles are not enabled in Review Mode
      if (ReviewMode)
      {
        menuitem_ComputerPlays0.Enabled = false;
        menuitem_ComputerPlays1.Enabled = false;
      }

      //	Stop Thinking is only available if the 
      //	computer is currently thinking
      menuitem_StopThinking.Enabled = engineThinking;
    }
    #endregion

    #region menuitem_HighlightComputerMove_Click
    private void menuitem_HighlightComputerMove_Click(object sender, EventArgs e)
    {
      boardControl.HighlightMove = !boardControl.HighlightMove;
      boardControl.Invalidate();
    }
    #endregion

    #region menuitem_RotateBoard_Click
    private void menuitem_RotateBoard_Click(object sender, EventArgs e)
    {
      boardControl.RotateBoard = !boardControl.RotateBoard;
      boardControl.Invalidate();
    }
    #endregion

    #region propertiesToolStripMenuItem_Click
    private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      PiecePropertiesForm form = new PiecePropertiesForm(contextMenuPiece, boardControl);
      form.Show();
    }
    #endregion

    #region menuitem_TakeBackMove_Click
    private void menuitem_TakeBackMove_Click(object sender, EventArgs e)
    {
      Game.UndoMove(true);
      boardControl.Presentation.Update();
      boardControl.Invalidate();
    }
    #endregion

    #region menuitem_ShowEngineDebugWindow_Click
    private void menuitem_ShowEngineDebugWindow_Click(object sender, EventArgs e)
    {
      debugForm.Visible = !debugForm.Visible;
    }
    #endregion

    #region menu_Tools_DropDownOpening
    private void menu_Tools_DropDownOpening(object sender, EventArgs e)
    {
      menuitem_ShowEngineDebugWindow.Checked = debugForm.Visible;
    }
    #endregion

    #region timer_Tick
    private void timer_Tick(object sender, EventArgs e)
    {
      if (!gameStarted)
      {
        timer.Stop();
        //	Handle moves already played (if any, e.g., loaded from a saved game)
        if (Game.GameMoveNumber > 0)
        {
          for (int nMove = 0; nMove < Game.GameMoveNumber; nMove++)
          {
            //	add move to the move list
            int turnNumber;
            MoveInfo moveMade = Game.GetHistoricalMove(nMove, out turnNumber);
            addMoveToMoveList(nMove, moveMade, turnNumber);
          }
          //	Check for game end
          if (Game.Result.Type != ResultType.NoResult)
          {
            Game.Cleanup();
            if (!Game.IsAutomatedMatch)
              //	Show message if the game has ended
              MessageBox.Show(Game.Result.VerboseString);
            else
              Close();
          }
        }
        //	Initialize player clock displays
        Game.Match.GetTimeControl(0).Initialize();
        Game.Match.GetTimeControl(1).Initialize();
        updatePlayerClock(0, Game.Match.GetTimeControl(0).TimeLeft);
        updatePlayerClock(1, Game.Match.GetTimeControl(1).TimeLeft);
        //	Start the game.  This will start the clocks and start the 
        //	engine thinking if it is up.
        gameStarted = true;
        Game.StartGame();
        return;
      }
      if (!Game.Match.GetPlayer(sideOnClock).TimeControl.Infinite)
      {
        long timeleft = Game.Match.GetPlayer(sideOnClock).TimeControl.ActiveTimeLeft;
        updatePlayerClock(sideOnClock, timeleft);
      }
      if (engineStatsForm != null && engineThinking)
        engineStatsForm.UpdateStatistics();
    }
    #endregion

    #region menuitem_SaveGame_Click
    private void menuitem_SaveGame_Click(object sender, EventArgs e)
    {
      saveFileDialog.Filter = "Save Game Files (*.sgf)|*.sgf|All Files (*.*)|*.*";
      if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
      {
        TextWriter outputfile = new StreamWriter(saveFileDialog.FileName);
        Game.SaveGame(outputfile);
        outputfile.Close();
      }
    }
    #endregion

    #region GameForm_FormClosing
    private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      debugForm.Close();
      Game.AbortSearch();
      Game.Match.Finished();
    }
    #endregion

    #region menuitem_About_Click
    private void menuitem_About_Click(object sender, EventArgs e)
    {
      AboutForm aboutForm = new AboutForm();
      aboutForm.Show();
    }
    #endregion

    #region menuitem_TakeBackAllMoves_Click
    private void menuitem_TakeBackAllMoves_Click(object sender, EventArgs e)
    {
      while (Game.BoardMoveStack.MoveCount > 0)
      {
        Game.UndoMove(true);
        boardControl.Presentation.Update();
        boardControl.Invalidate();
      }
    }
    #endregion

    #region menuitem_EnableCustomTheme_Click
    private void menuitem_EnableCustomTheme_Click(object sender, EventArgs e)
    {
      menuitem_EnableCustomTheme.Checked = !menuitem_EnableCustomTheme.Checked;
      if (menuitem_EnableCustomTheme.Checked)
      {
        Theme.CustomThemeName = Game.GetCustomThemes()[0];
        updateTheme();
      }
      else
      {
        Theme.CustomThemeName = null;
        updateTheme();
      }
      try
      {
        boardControl.Presentation.UpdateTheme(Theme);
      }
      catch (Exception ex)
      {
        ExceptionForm exform = new ExceptionForm(ex, Game);
        exform.ShowDialog();
        return;
      }
      boardControl.Invalidate();
      Theme.SaveToRegistry(Game.RegistryKey);
    }
    #endregion

    #region menuitem_LoadPositionByFEN_Click
    private void menuitem_LoadPositionByFEN_Click(object sender, EventArgs e)
    {
      LoadFENForm form = new LoadFENForm(Game);
      form.BackColor = WindowBackgroundColor;
      int currentSide = sideOnClock;
      if (form.ShowDialog() == DialogResult.OK)
      {
        boardControl.Initialize(Game, Game.Board, Theme, this);
        boardControl.Invalidate();
        listMoves.Items.Clear();
        mbControl.Invalidate();
        ehControl.Invalidate();
        listThinking1.Items.Clear();
        Game.Match.SetMoves("");
        Player player0 = Game.Match.GetPlayer(0);
        if (player0 is XBoardEngine)
          ((XBoardEngine)player0).SetBoard(Game.FEN.ToString());
        Player player1 = Game.Match.GetPlayer(1);
        if (player1 is XBoardEngine)
          ((XBoardEngine)player1).SetBoard(Game.FEN.ToString());
        if (Game.CurrentSide != sideOnClock)
        {
          Game.Match.PlayerToMove.State = PlayerState.Thinking;
          Game.Match.PlayerToWait.State = PlayerState.Observing;
          if (Game.ComputerControlled[Game.CurrentSide])
          {
            engineThinking = true;
            Game.Match.GetPlayer(Game.CurrentSide).Go(Game.Match.GetPlayer(Game.CurrentSide));
          }
        }
      }
    }
    #endregion

    #region menuitem_StopThinking_Click
    private void menuitem_StopThinking_Click(object sender, EventArgs e)
    {
      if (Game.ComputerControlled[0])
        menuitem_ComputerPlays0_Click(null, null);
      if (Game.ComputerControlled[1])
        menuitem_ComputerPlays1_Click(null, null);
      Game.Match.GetPlayer(sideOnClock).StopThinking();
    }
    #endregion

    #region pictPrevious_Click
    private void pictPrevious_Click(object sender, EventArgs e)
    {
      if (!ReviewMode && !engineThinking && Game.GameMoveNumber > 0)
      {
        ReviewMode = true;
        if (Game.Result.Type != ResultType.NoResult)
        {
          gameResult = Game.Result;
          Game.Result = new Result();
        }
        else
          gameResult = null;
        reviewCursor = Game.GameMoveNumber;
        reviewRedoStack = new Stack<MoveInfo>();
        lblReviewMode.Visible = true;
        pictStop.Image = global::ChessV.GUI.Properties.Resources.icon_black_stop;
        listMoves.SelectedItems.Clear();
        listMoves.Items[Game.GameMoveNumber - 1].Selected = true;
        listMoves.HideSelection = false;
        listMoves.Select();
      }
      else if (reviewCursor > 0)
      {
        if (Game.Result.Type != ResultType.NoResult)
          Game.Result = new Result();
        pictNext.Image = global::ChessV.GUI.Properties.Resources.icon_black_next;
        pictLast.Image = global::ChessV.GUI.Properties.Resources.icon_black_last;
        //	save the most recent move onto the Redo Stack so we can 
        //	redo them in sequence when we exit review mode
        reviewRedoStack.Push(Game.GetHistoricalMove(--reviewCursor));
        //	step back one move
        Game.UndoMove();
        //	if we're not at the beginning, go back one more move and then 
        //	replay it so that the proper squares are highlighted
        if (reviewCursor > 0)
        {
          reviewRedoStack.Push(Game.GetHistoricalMove(reviewCursor - 1));
          Game.UndoMove();
          Game.MakeMove(reviewRedoStack.Pop(), true);
        }
        else
          Game.HighlightSquares.Clear();
        //	refresh the board control
        boardControl.Presentation.Update();
        boardControl.Invalidate();
        //	deselect any moves selected in the move list
        listMoves.SelectedItems.Clear();
        //	select the new current move in the move list
        if (reviewCursor > 0)
          listMoves.Items[reviewCursor - 1].Selected = true;
        else
        {
          listMoves.Items[0].Selected = false;
          pictFirst.Image = global::ChessV.GUI.Properties.Resources.icon_gray_first;
          pictPrevious.Image = global::ChessV.GUI.Properties.Resources.icon_gray_previous;
        }
        listMoves.Select();
      }
    }
    #endregion

    #region pictStop_Click
    private void pictStop_Click(object sender, EventArgs e)
    {
      if (ReviewMode)
      {
        lblReviewMode.Visible = false;
        pictStop.Image = global::ChessV.GUI.Properties.Resources.icon_gray_stop;
        pictFirst.Image = global::ChessV.GUI.Properties.Resources.icon_black_first;
        pictPrevious.Image = global::ChessV.GUI.Properties.Resources.icon_black_previous;
        pictNext.Image = global::ChessV.GUI.Properties.Resources.icon_gray_next;
        pictLast.Image = global::ChessV.GUI.Properties.Resources.icon_gray_last;
        listMoves.HideSelection = true;
        //	replay all queued moves
        while (reviewRedoStack.Count > 0)
        {
          MoveInfo mi = reviewRedoStack.Pop();
          Game.MakeMove(mi, true);
        }
        //	restore game result
        if (gameResult != null)
          Game.Result = gameResult;
        ReviewMode = false;
        boardControl.Presentation.Update();
        boardControl.Invalidate();
        Application.DoEvents();
        boardControl.Select();
      }
    }
    #endregion

    #region pictFirst_Click
    private void pictFirst_Click(object sender, EventArgs e)
    {
      if (!ReviewMode && Game.GameMoveNumber > 0)
      {
        ReviewMode = true;
        if (Game.Result.Type != ResultType.NoResult)
        {
          gameResult = Game.Result;
          Game.Result = new Result();
        }
        else
          gameResult = null;
        reviewCursor = Game.GameMoveNumber;
        reviewRedoStack = new Stack<MoveInfo>();
        lblReviewMode.Visible = true;
        pictFirst.Image = global::ChessV.GUI.Properties.Resources.icon_black_first;
        pictPrevious.Image = global::ChessV.GUI.Properties.Resources.icon_black_previous;
        pictStop.Image = global::ChessV.GUI.Properties.Resources.icon_black_stop;
        pictNext.Image = global::ChessV.GUI.Properties.Resources.icon_black_next;
        pictLast.Image = global::ChessV.GUI.Properties.Resources.icon_black_last;
        listMoves.SelectedItems.Clear();
      }
      if (ReviewMode)
      {
        while (reviewCursor > 0)
        {
          //	save the most recent move onto the Redo Stack so we can 
          //	redo them in sequence when we exit review mode
          reviewRedoStack.Push(Game.GetHistoricalMove(--reviewCursor));
          //	step back one move
          Game.UndoMove();
        }
        Game.HighlightSquares.Clear();
        pictNext.Image = global::ChessV.GUI.Properties.Resources.icon_black_next;
        pictLast.Image = global::ChessV.GUI.Properties.Resources.icon_black_last;
        //	refresh the board control
        boardControl.Presentation.Update();
        boardControl.Invalidate();
        //	deselect any moves selected in the move list
        listMoves.SelectedItems.Clear();
        listMoves.Select();
      }
    }
    #endregion

    #region pictNext_Click
    private void pictNext_Click(object sender, EventArgs e)
    {
      if (ReviewMode && reviewRedoStack.Count > 0)
      {
        //	deselect any moves selected in the move list
        listMoves.SelectedItems.Clear();
        //	make the next move from the redo stack
        Game.MakeMove(reviewRedoStack.Pop(), true);
        reviewCursor++;
        listMoves.Items[reviewCursor - 1].Selected = true;
        pictFirst.Image = global::ChessV.GUI.Properties.Resources.icon_black_first;
        pictPrevious.Image = global::ChessV.GUI.Properties.Resources.icon_black_previous;
        if (reviewRedoStack.Count == 0)
        {
          pictNext.Image = global::ChessV.GUI.Properties.Resources.icon_gray_next;
          pictLast.Image = global::ChessV.GUI.Properties.Resources.icon_gray_last;
        }
        //	refresh the board control
        boardControl.Presentation.Update();
        boardControl.Invalidate();
      }
    }
    #endregion

    #region pictLast_Click
    private void pictLast_Click(object sender, EventArgs e)
    {
      if (ReviewMode && reviewRedoStack.Count > 0)
      {
        //	deselect any moves selected in the move list
        listMoves.SelectedItems.Clear();
        while (reviewRedoStack.Count > 0)
        {
          //	make the next move from the redo stack
          Game.MakeMove(reviewRedoStack.Pop(), true);
          reviewCursor++;
        }
        listMoves.Items[reviewCursor - 1].Selected = true;
        pictFirst.Image = global::ChessV.GUI.Properties.Resources.icon_black_first;
        pictPrevious.Image = global::ChessV.GUI.Properties.Resources.icon_black_previous;
        pictNext.Image = global::ChessV.GUI.Properties.Resources.icon_gray_next;
        pictLast.Image = global::ChessV.GUI.Properties.Resources.icon_gray_last;
        //	refresh the board control
        boardControl.Presentation.Update();
        boardControl.Invalidate();
      }
    }
    #endregion

    #region menuitem_MultiPVAnalysis_Click
    private void menuitem_MultiPVAnalysis_Click(object sender, EventArgs e)
    {
      MultiPVAnalysisForm form = new MultiPVAnalysisForm();
      if (form.ShowDialog() == DialogResult.OK)
      {
        TimeControl fixedDepthTC = new TimeControl();
        fixedDepthTC.Infinite = true;
        fixedDepthTC.PlyLimit = Convert.ToInt32(form.NumDepth);
        engineThinking = true;
        Game.Think(fixedDepthTC, form.NumVariations);
      }
    }
    #endregion

    #region menuitem_ShowEngineStatisticsWindow_Click
    private void menuitem_ShowEngineStatisticsWindow_Click(object sender, EventArgs e)
    {
      engineStatsForm = new EngineStatisticsForm(Game.Statistics);
      engineStatsForm.Show(this);
    }
    #endregion

    #region updatePlayerClock
    private void updatePlayerClock(int player, long timeleft)
    {
      if (timeleft < 0)
      {
        if (player == 0)
        {
          labelTime0.Text = "0:00";
          labelTime0.BackColor = Color.Red;
          labelTime0.ForeColor = Color.Black;
        }
        else
        {
          labelTime1.Text = "0:00";
          labelTime1.BackColor = Color.Red;
          labelTime1.ForeColor = Color.Black;
        }
      }
      else
      {
        string timestring = timeInMillisecondsToString(timeleft);
        if (player == 0)
        {
          labelTime0.Text = timestring;
          labelTime0.BackColor = Color.White;
          labelTime0.ForeColor = Color.Black;
        }
        else
        {
          labelTime1.Text = timestring;
          labelTime1.BackColor = Color.Black;
          labelTime1.ForeColor = Color.White;
        }
      }
    }
    #endregion

    #region updateTheme
    protected void updateTheme()
    {
      //	Determine background color - this is typically the 
      //	first square color (light square color) but if that 
      //	color isn't light enough, we will scale it up to make 
      //	it lighter.  Things look bad if the backgrounds for 
      //	all the tool windows aren't fairly light
      WindowBackgroundColor = Theme.ColorScheme.SquareColors[0];
      int brightness =
        (WindowBackgroundColor.R +
         WindowBackgroundColor.G +
         WindowBackgroundColor.B) / 3;
      if (brightness < 240)
      {
        double scaleFactor = 250.0 / (double)brightness;
        WindowBackgroundColor = Color.FromArgb(
          (int)(WindowBackgroundColor.R + ((255 - WindowBackgroundColor.R) * (scaleFactor - 1.0))),
          (int)(WindowBackgroundColor.G + ((255 - WindowBackgroundColor.G) * (scaleFactor - 1.0))),
          (int)(WindowBackgroundColor.B + ((255 - WindowBackgroundColor.B) * (scaleFactor - 1.0))));
      }
      //	Determine color for the side panel controls - this is almost the 
      //	WindowBackgroundColor we just calculated, but slightly darker
      Color clockPanelColor = Color.FromArgb(
        (int)((WindowBackgroundColor.R * 9 + Theme.ColorScheme.SquareColors[1].R * 1) / 10),
        (int)((WindowBackgroundColor.G * 9 + Theme.ColorScheme.SquareColors[1].G * 1) / 10),
        (int)((WindowBackgroundColor.B * 9 + Theme.ColorScheme.SquareColors[1].B * 1) / 10));
      //	Ensure the clockPanelColor is sufficiently different that the window background color,
      //	otherwise we'll need to darken it a little.
      if (Math.Max(WindowBackgroundColor.R, clockPanelColor.R) - Math.Min(WindowBackgroundColor.R, clockPanelColor.R) < 12 &&
        Math.Max(WindowBackgroundColor.G, clockPanelColor.G) - Math.Min(WindowBackgroundColor.G, clockPanelColor.G) < 12 &&
        Math.Max(WindowBackgroundColor.B, clockPanelColor.B) - Math.Min(WindowBackgroundColor.B, clockPanelColor.B) < 12)
        clockPanelColor = Color.FromArgb(
          (int)((WindowBackgroundColor.R * 7 + Theme.ColorScheme.SquareColors[1].R * 2) / 10),
          (int)((WindowBackgroundColor.G * 7 + Theme.ColorScheme.SquareColors[1].G * 2) / 10),
          (int)((WindowBackgroundColor.B * 7 + Theme.ColorScheme.SquareColors[1].B * 2) / 10));
      splitContainer3.Panel1.BackColor = clockPanelColor;
      //	Update the colors of the various controls
      mbControl.BackColor = WindowBackgroundColor;
      ehControl.BackColor = WindowBackgroundColor;
      listMoves.BackColor = WindowBackgroundColor;
      listThinking1.BackColor = WindowBackgroundColor;
      //splitContainer3.Panel1.BackColor = WindowBackgroundColor;
      splitContainer3.Panel2.BackColor = WindowBackgroundColor;
      evaluationTabControl.TabPages[0].BackColor = WindowBackgroundColor;
      evaluationTabControl.TabPages[1].BackColor = WindowBackgroundColor;
      lblReviewMode.BackColor = Theme.ColorScheme.BorderColor;
      lblReviewMode.ForeColor = Theme.ColorScheme.TextColor;
      //	Recalculate the size of the window, the size of the 
      //	controls, and the locations of the splitters
      Width = boardControl.Presentation.NativeSize().Width + 440;
      boardControl.Size = boardControl.Presentation.NativeSize();
      splitContainer1.SplitterDistance = boardControl.Presentation.NativeSize().Width + 4;
      splitContainer2.SplitterDistance = boardControl.Presentation.NativeSize().Height + 4;
      Height = boardControl.Presentation.NativeSize().Height + 300;
    }
    #endregion

    #region HumanEnabled
    public void HumanEnabled(bool humanEnabled)
    {
      if (!humanEnabled)
      {
        engineThinking = true;
        //	Disable the buttons for previous move and first move (review mode)
        pictFirst.Image = global::ChessV.GUI.Properties.Resources.icon_gray_first;
        pictPrevious.Image = global::ChessV.GUI.Properties.Resources.icon_gray_previous;
        //	Clear the contents of the thinkling list
        listThinking1.Items.Clear();
      }
      else
      {
        engineThinking = false;
        if (Game.GameMoveNumber > 0)
        {
          //	Enable the buttons for previous move and first move (review mode)
          pictFirst.Image = global::ChessV.GUI.Properties.Resources.icon_black_first;
          pictPrevious.Image = global::ChessV.GUI.Properties.Resources.icon_black_previous;
        }
      }
    }
    #endregion

    #region addMoveToMoveList
    private void addMoveToMoveList(int moveNumber, MoveInfo move, int turnNumber)
    {
      //	Find the previous list item and see if the move being added is a move by 
      //	the same player.  (This is important so that multi-move variants display 
      //	correctly.)
      ListViewItem previousItem = listMoves.Items.Count == 0 ? null : listMoves.Items[listMoves.Items.Count - 1];
      //	The prefix for the text being displayed depends on which player and whether 
      //	this move is from the same player as the previous one.
      string description = "\t";
      if (previousItem == null || ((MoveInfo)previousItem.Tag).Player != move.Player)
        if (move.Player == 0)
        {
          currentMoveNumber++;
          description = turnNumber.ToString() + "." + description;
        }
        else
          description = "       ..." + description;
      //	Now append to the string the actual discription of the move
      description += Game.DescribeMove(move, MoveNotation.Descriptive);
      moveDescriptions.Add(description);
      ListViewItem item = new ListViewItem(description.Split('\t'));
      item.Tag = move;
      listMoves.Items.Add(item);
      if (gameStarted)
      {
        //	Update the Board Control
        boardControl.Presentation.Update();
        boardControl.Invalidate();
        //	Do events (so the UI doesn't stall)
        Application.DoEvents();
      }
    }
    #endregion

    #region timeInMillisecondsToString
    private string timeInMillisecondsToString(long timeleft)
    {
      long minutesLeft = timeleft / 60000;
      long secondsLeft = (timeleft % 60000) / 1000;
      if (minutesLeft >= 60)
      {
        long hoursLeft = minutesLeft / 60;
        minutesLeft = minutesLeft % 60;
        return hoursLeft.ToString() + ":" + minutesLeft.ToString("d2") + ":" + secondsLeft.ToString("d2");
      }
      else
        return minutesLeft.ToString() + ":" + secondsLeft.ToString("d2");
    }
    #endregion

    #region updateThinking
    private void updateThinking(Dictionary<string, string> searchinfo)
    {
      if (searchinfo["Score"].IndexOf("M") >= 0)
        currentEval = searchinfo["Score"].IndexOf("-") >= 0 ? -20.0 : 20.0;
      else
      {
        Double d;
        if (Double.TryParse(searchinfo["Score"], out d))
          currentEval = d;
      }
      string[] items = new string[] { (searchinfo["Depth"].Length < 2 ? " " + searchinfo["Depth"] : searchinfo["Depth"]) + ": " + searchinfo["Score"], searchinfo["Time"], searchinfo["Nodes"], searchinfo["PV"] };
      ListViewItem newItem = new ListViewItem(items);
      listThinking1.Items.Insert(0, newItem);
      Application.DoEvents();
    }
    #endregion
  }
}
