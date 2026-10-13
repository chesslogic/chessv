
/***************************************************************************

                                 ChessV

                  COPYRIGHT (C) 2012-2017 BY GREG STRONG

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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ChessV.Games;
using System.Linq;

namespace ChessV.GUI
{
  //**********************************************************************
  //
  //                             BoardControl
  //
  //
  //  This is a UserControl that handles the graphical representation of 
  //  the board(s) for a Game.  It receives user moves with mouse clicks 
  //  and reports them back to the Game.

  public partial class BoardControl : UserControl
  {
    // *** PROPERTIES *** //

    //	BoardPresentation encapsulating the logic for rendering the board
    public BoardPresentation Presentation { get; private set; }

    //	highlight the last move made unless we made it with this control
    public bool HighlightMove { get; set; }

    //	true if rendering from second player's perspective
    public bool RotateBoard { get; set; }

    //	Theme to use for rendering (color schemes, etc.)
    public Theme Theme { get; private set; }
    public bool IsTerminalInspection { get; private set; }
    private readonly Dictionary<Piece, KingThreatSnapshot> terminalHints = new Dictionary<Piece, KingThreatSnapshot>();
    private bool Reviewing => owningForm is GameForm form && form.ReviewMode;
    private bool SupportsKingHints => game.GetType() == typeof(Chess) || game is ApmwChessGame;


    //	protected members
    protected Game game;
    protected Board board;
    protected Form owningForm;
    protected MouseEventArgs lastMouseDown;
    protected DateTime lastMouseDownTime;
    protected Piece pieceBeingLifted;
    protected MouseEventArgs mouseDownEventArg;


    // *** CONSTRUCTION *** //

    public BoardControl()
    {
      InitializeComponent();

      this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
    }

    public void Initialize(Game game, Board board, Theme theme, Form owningForm = null)
    {
      if (Presentation != null)
        CancelSelection();
      terminalHints.Clear();
      if (this.game != game)
        IsTerminalInspection = false;
      this.game = game;
      this.board = board;
      Theme = theme;
      this.owningForm = owningForm;
      HighlightMove = true;
      RotateBoard = false;
      Presentation = PresentationFactory.CreatePresentation(game, theme);
    }


    // *** OPERATIONS *** //

    public void RefreshKingHints(bool terminal, int? player)
    {
      Piece focused = Presentation.KingHints?.King;
      CancelSelection();
      IsTerminalInspection = terminal;
      terminalHints.Clear();
      Presentation.KingHints = null;
      if (terminal && player.HasValue && SupportsKingHints)
      {
        foreach (Piece king in KingThreatAnalysis.GetKings(game, player.Value))
          terminalHints.Add(king, KingThreatAnalysis.Analyze(game, king, includeCastling: false));
        if (focused != null && terminalHints.TryGetValue(focused, out KingThreatSnapshot hints))
          Presentation.KingHints = hints;
        else if (terminalHints.Count > 0)
          Presentation.KingHints = terminalHints.Values.First();
      }
      Presentation.Update();
      Invalidate();
    }

    public void ClearKingHints()
    {
      CancelSelection();
      terminalHints.Clear();
      Presentation.KingHints = null;
    }

    private void CancelSelection()
    {
      timerPieceLiftTimer.Stop();
      pieceBeingLifted = null;
      mouseDownEventArg = null;
      lastMouseDown = null;
      if (Presentation != null)
      {
        Presentation.PieceLifted = null;
        if (!IsTerminalInspection)
          Presentation.KingHints = null;
      }
      Invalidate();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Escape)
      {
        CancelSelection();
        Presentation.MouseCoordinate = new Point(-1, -1);
        e.Handled = true;
      }
      base.OnKeyDown(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
      if (Presentation?.KingHints != null && Presentation.PieceLifted == null && pieceBeingLifted == null)
      {
        Presentation.MouseCoordinate = new Point(-1, -1);
        Invalidate();
      }
      base.OnMouseLeave(e);
    }

    public Location GetLocationAtCoordinate(Point pt)
    {
      Presentation.MouseCoordinate = pt;
      return Presentation.GetMouseLocation(RotateBoard);
    }


    // **************************************** //
    // ***                                  *** //
    // ***          Event Handlers          *** //
    // ***                                  *** //
    // **************************************** //

    //	Paint event handler
    private void BoardControl_Paint(object sender, PaintEventArgs e)
    {
      if (Presentation != null)
      {
        using Bitmap boardPresentation = Presentation.Render(HighlightMove, RotateBoard);
        if (boardPresentation != null)
          e.Graphics.DrawImageUnscaled(boardPresentation, new Point());
        return;
      }
    }

    //	Mouse Down event handler
    private void BoardControl_MouseDown(object sender, MouseEventArgs e)
    {
      if (Presentation == null)
        return;
      if (e.Button == MouseButtons.Right && Presentation.PieceLifted != null)
      {
        CancelSelection();
        return;
      }
      if (Presentation.PieceLifted == null && (!Reviewing || IsTerminalInspection))
      {
        Presentation.MouseCoordinate = e.Location;
        Location location = Presentation.GetMouseLocation(RotateBoard);
        if (!location.IsNull)
        {
          Piece piece = board[board.LocationToSquare(location)];
          if (piece != null)
          {
            //	check for right-button (which opens context menu)
            if (e.Button == MouseButtons.Right)
            {
              if (owningForm != null && owningForm is GameForm)
              {
                ((GameForm)owningForm).ShowContextMenu(PointToScreen(e.Location), piece);
                return;
              }
              return;
            }

            if (IsTerminalInspection)
            {
              if (terminalHints.TryGetValue(piece, out KingThreatSnapshot hints))
                Presentation.KingHints = hints;
              Invalidate();
              return;
            }

            //	if the game is over, just return
            if (!game.Result.IsNone)
              return;

            CancelSelection();
            Presentation.MouseCoordinate = e.Location;
            Focus();
            bool inspectingKing = SupportsKingHints && piece.Player == game.CurrentSide &&
              KingThreatAnalysis.GetKings(game, piece.Player).Contains(piece);
            if (inspectingKing)
              Presentation.KingHints = KingThreatAnalysis.Analyze(game, piece);

            //	see if this piece has any legal moves
            MoveInfo[] moves;
            int nMoves = game.GetRootMoves(out moves, piece);
            if (nMoves == 0)
            {
              if (inspectingKing)
              {
                Invalidate();
                return;
              }
              //	this piece can't move; determine appropriate explanation
              if (piece.Player != game.CurrentSide)
                MessageBox.Show("That is not your piece to move");
              else
                MessageBox.Show("That piece cannot move at this time");
              return;
            }

            //	record MouseDown event time - if we get a MouseClick quickly 
            //	enough, we'll consider this a click-and-click-again type movement 
            //	rather than a click-drag type movement.
            lastMouseDownTime = DateTime.Now;

            //	we'll also store some additional information and set the timer.
            //	if we don't get a MouseClick event that corresponds with this 
            //	quickly, we'll initiate the actual click-drag type movement.
            //	we don't want to do this prematurely because we don't want the 
            //	picture of the piece to "jump" on a simple click event.
            pieceBeingLifted = piece;
            mouseDownEventArg = e;
            timerPieceLiftTimer.Start();
          }
          else if (!IsTerminalInspection)
            CancelSelection();
        }
      }
    }

    //	Mouse Move event handler
    private void BoardControl_MouseMove(object sender, MouseEventArgs e)
    {
      if (Presentation != null && (Presentation.PieceLifted != null || Presentation.KingHints != null))
      {
        Presentation.MouseCoordinate = e.Location;
        if (IsTerminalInspection)
        {
          Location location = Presentation.GetMouseLocation(RotateBoard);
          Piece piece = location.IsNull ? null : board[board.LocationToSquare(location)];
          if (piece != null && terminalHints.TryGetValue(piece, out KingThreatSnapshot hints) &&
            Presentation.KingHints != hints)
            Presentation.KingHints = hints;
        }
        Invalidate();
      }
    }

    //	Mouse Up event handler
    private void BoardControl_MouseUp(object sender, MouseEventArgs e)
    {
      Presentation.MouseCoordinate = e.Location;
      //	MouseUp is only meaningful if a piece is lifted and being dragged
      if (Presentation.PieceLifted != null && Presentation.LiftedPieceIsDragged)
      {
        //	Find board location over which the mouse was released
        Location location = Presentation.GetMouseLocation(RotateBoard);
        if (!location.IsNull)
          TryMoveToLocation(location);
        else
          CancelSelection();
      }
    }

    private void BoardControl_MouseClick(object sender, MouseEventArgs e)
    {
      //	see if this click goes with a (just processed) mouse down event 
      //	which corresponded with lifting a piece for movement.  if so, 
      //	we convert this movement input from a hold-and-drag type to 
      //	a click and click again type.
      if (Presentation.PieceLifted != null && Presentation.LiftedPieceIsDragged &&
        (DateTime.Now - lastMouseDownTime).TotalMilliseconds < 500 &&
        Math.Abs(e.X - lastMouseDown.X) < 4 && Math.Abs(e.Y - lastMouseDown.Y) < 4)
      {
        //	stop the timer set during MouseDown since we now know 
        //	that this is not a click-drag type piece movement
        timerPieceLiftTimer.Stop();
        //	inform the Presentation that we have a piece lifted 
        //	and invalidate so it draws appropriately.
        Presentation.LiftedPieceIsDragged = false;
        //	redraw the board control
        Invalidate();
      }
      else
      {
        //	the other click of interest is if we are already performing 
        //	a click and click again type move and this is the second click.
        if (Presentation.PieceLifted != null && !Presentation.LiftedPieceIsDragged)
        {
          //	Find board location over which the mouse was released
          Location location = Presentation.GetMouseLocation(RotateBoard);
          if (!location.IsNull)
            TryMoveToLocation(location);
        }
      }
    }

    private void TryMoveToLocation(Location location)
    {
      if (IsTerminalInspection || Reviewing || !game.Result.IsNone)
      {
        CancelSelection();
        return;
      }
      //	is the current square one to which the lifted piece can move?
      int currentSquare = board.LocationToSquare(location);
      MoveInfo[] moves;
      int nMoves = game.GetRootMoves(out moves, Presentation.PieceLifted);
      List<MoveInfo> applicableMoves = new List<MoveInfo>();
      for (int x = 0; x < nMoves; x++)
        if (moves[x].PieceMoved == Presentation.PieceLifted && moves[x].ToSquare == currentSquare)
          applicableMoves.Add(moves[x]);

      Presentation.PieceLifted = null;
      Presentation.KingHints = null;
      pieceBeingLifted = null;
      timerPieceLiftTimer.Stop();
      if (applicableMoves.Count > 0)
      {
        MoveInfo selectedMove = applicableMoves[0];
        if (applicableMoves.Count > 1)
        {
          //	there are multiple possible moves between these coordinates 
          //	so ask the user with a MoveSelectForm
          MoveSelectForm frm = new MoveSelectForm(game, applicableMoves);
          frm.ShowDialog();
          selectedMove = frm.SelectedMove;
        }
        if (game.CurrentPlayer is HumanPlayer)
          ((HumanPlayer)game.CurrentPlayer).OnHumanMove(new List<Movement>() { selectedMove }, game.CurrentSide);
      }
      //	Presentation.Update();
      Invalidate();
    }

    private void timerPieceLiftTimer_Tick(object sender, EventArgs e)
    {
      //	if the timer is firing that means that we got a MouseDown event 
      //	without a quick MouseClick event, so we can now engage the actual 
      //	movement of a piece by the click-drag method.  before this, we 
      //	didn't know for sure if it was a click-drag movement or a 
      //	click-and-click again movement.
      timerPieceLiftTimer.Stop();
      if (pieceBeingLifted == null || pieceBeingLifted.Square < 0 ||
        IsTerminalInspection || Reviewing || !game.Result.IsNone)
        return;
      Presentation.PieceLifted = pieceBeingLifted;
      Presentation.LiftedPieceIsDragged = true;
      lastMouseDown = mouseDownEventArg;
      Invalidate();
    }
  }
}
