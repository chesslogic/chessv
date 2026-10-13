
/***************************************************************************

                                 ChessV

                  COPYRIGHT (C) 2012-2020 BY GREG STRONG

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
using System.Drawing.Imaging;
using ChessV.Games;

namespace ChessV.GUI
{
  public class BoardPresentation
  {
    // *** PROPERTIES *** //

    public Theme Theme { get; set; }
    public Board Board { get; private set; }
    public Game Game { get; private set; }
    public bool SmallPreview { get; private set; }
    public Piece PieceLifted { get; set; }
    public bool LiftedPieceIsDragged { get; set; }
    public Point MouseCoordinate { get; set; }
    private KingThreatSnapshot kingHints;
    private KingThreatRenderer kingThreatRenderer;
    public KingThreatSnapshot KingHints
    {
      get { return kingHints; }
      set
      {
        kingHints = value;
        kingThreatRenderer = value == null ? null : new KingThreatRenderer(value);
      }
    }

    public BitmapPieceSetPresentation PieceSetPresentation
    { get { return pieceSetPresentation; } }


    // *** HELPER STRUCT *** //

    protected struct PieceInfo
    {
      public PieceType PieceType { get; private set; }
      public int Player { get; private set; }

      public PieceInfo(PieceType pieceType, int player) : this()
      { PieceType = pieceType; Player = player; }
    }


    // *** DATA MEMBERS *** //

    protected LocationToPresentationMapping mapping;
    protected BitmapPieceSetPresentation pieceSetPresentation;
    protected Dictionary<Location, PieceInfo> pieces { get; private set; }


    // *** CONSTRUCTION *** //

    public BoardPresentation(Board board, Theme theme, bool smallPreview = false)
    {
      Board = board;
      Game = board.Game;
      SmallPreview = smallPreview;
      UpdateTheme(theme);
      Update();
    }


    protected virtual LocationToPresentationMapping CreateMapping(int borderSize, int squareSize)
    { return new LocationToPresentationMapping(Board, Theme, borderSize, squareSize); }

    public Size NativeSize()
    {
      return new Size(mapping.TotalWidth, mapping.TotalHeight);
    }

    public Location GetMouseLocation(bool rotateBoard)
    {
      return mapping.MapCoordinate(MouseCoordinate, rotateBoard);
    }

    public void UpdateTheme(Theme theme)
    {
      Theme = theme;
      int squareWidth = 50;
      int squareHeight = 50;
      if (Theme.PieceSet is PieceSet)
      {
        squareWidth = Theme.PieceSet.Width + 6;
        squareHeight = Theme.PieceSet.Height + 6;
        pieceSetPresentation = new BitmapPieceSetPresentation(Game, (PieceSet)Theme.PieceSet);
        pieceSetPresentation.Initialize(Theme);
      }
      else
        throw new Exception("Non-Bitmap piece sets not implemented yet");
      int squareSize = squareWidth > squareHeight ? squareWidth : squareHeight;
      if (Theme.CustomThemeName != null)
        Game.GetThemeSquareSize(theme.CustomThemeName, ref squareSize);
      mapping = CreateMapping(24, squareSize);
    }

    public void Update()
    {
      pieces = new Dictionary<Location, PieceInfo>();
      List<Piece> allPieces = Board.Game.GetPieceList();
      foreach (Piece piece in allPieces)
        if (piece.Square >= 0)
          pieces.Add(piece.Location, new PieceInfo(piece.PieceType, piece.Player));
    }

    public virtual Bitmap Render(bool highlightComputerMove = false, bool rotateBoard = false, Piece piece = null, bool endgame = false)
    {
      try
      {
        Size boardSize = NativeSize();
        Bitmap bmp = new Bitmap(boardSize.Width, boardSize.Height, PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(bmp);

        SolidBrush moveBrush = null;
        SolidBrush captureBrush = null;
        if (PieceLifted != null)
        {
          moveBrush = new SolidBrush(Color.FromArgb(0, 255, 0));
          captureBrush = new SolidBrush(Color.FromArgb(255, 0, 0));
        }

        //	Draw border - may be texture or color
        if (Theme.ColorScheme.BorderTexture != null)
        {
          Image borderImage =
              Theme.ColorScheme.BorderTexture.LargeImage != null
            ? Theme.ColorScheme.BorderTexture.LargeImage
            : Theme.ColorScheme.BorderTexture.Images[0];
          g.DrawImage(borderImage, new Rectangle(0, 0, mapping.TotalWidth + 1, mapping.TotalHeight + 1));
        }
        else
        {
          SolidBrush borderBrush = new SolidBrush(Theme.ColorScheme.BorderColor);
          g.FillRectangle(borderBrush, 0, 0, mapping.TotalWidth + 1, mapping.TotalHeight + 1);
        }

        //	Have Game render board if custom theme is active
        if (Theme.CustomThemeName != null)
          Game.RenderCustomThemeBoard(g, mapping.BorderSize, Theme.CustomThemeName);

        //	Create brushes for square colors
        SolidBrush[] squareBrushes = new SolidBrush[Theme.ColorScheme.NumberOfColors];
        for (int colorNumber = 0; colorNumber < Theme.ColorScheme.NumberOfColors; colorNumber++)
          squareBrushes[colorNumber] = new SolidBrush(Theme.ColorScheme.SquareColors[colorNumber]);

        //	Created required pens
        Pen blackOutlinePen = null;
        if (Theme.NSquareColors == 1)
          blackOutlinePen = new Pen(Color.Black, 2);
        Brush pstNumberDisplayBrush = null;
        if (piece != null)
          pstNumberDisplayBrush = new SolidBrush(Color.Black);


        // *** ITERATE THROUGH ALL SQUARES *** //

        for (int square = 0; square < Board.NumSquaresExtended; square++)
        {
          //	Draw the square itself
          Location location = Board.SquareToLocation(square);
          int squareColorNumber = Game.GetSquareColor(location, Theme.NSquareColors);
          Rectangle rect = mapping.MapLocation(location, rotateBoard);
          if (Theme.CustomThemeName == null /*&& piece == null*/ )
          {
            if (Theme.ColorScheme.SquareTextures != null && Theme.ColorScheme.SquareTextures.ContainsKey(squareColorNumber))
              g.DrawImage(Theme.ColorScheme.SquareTextures[squareColorNumber].Images[square % Theme.ColorScheme.SquareTextures[squareColorNumber].NumberOfImages], rect);
            else
              g.FillRectangle(squareBrushes[squareColorNumber], rect);
          }
          //	if we are showing the PST (piece-square-table) then draw the value for this square
          if (piece != null)
          {
            int relativeSquare = piece.Board.PlayerSquare(piece.Player, square);
            string pstValue = endgame ? piece.PieceType.GetEndgamePST(relativeSquare).ToString() : piece.PieceType.GetMidgamePST(relativeSquare).ToString();
            g.DrawString(pstValue, System.Drawing.SystemFonts.CaptionFont, pstNumberDisplayBrush, rect);
          }
          //	draw black outline if this is uncheckered board or if we are showing PST
          if (Theme.NSquareColors == 1 && Theme.CustomThemeName == null)
            g.DrawRectangle(blackOutlinePen, rect);

          //	is the current square one to which the lifted piece can move?
          if (PieceLifted != null)
          {
            MoveInfo[] moves;
            int nMoves = Game.GetRootMoves(out moves, PieceLifted);
            for (int x = 0; x < nMoves; x++)
              if (moves[x].ToSquare == square)
              {
                Brush br = (moves[x].MoveType & MoveType.StandardCapture) > 0 ? captureBrush : moveBrush;
                if (PieceSetPresentation.PieceSet.Name == "Eurasian")
                {
                  //	Cheezy hack so that the Eurasian peices don't cover the capture indicators
                  g.FillRectangle(br, new Rectangle(rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8));
                  g.DrawRectangle(Pens.Black, new Rectangle(rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8));
                }
                else
                {
                  g.FillEllipse(br, new Rectangle(rect.X + 6, rect.Y + 6, rect.Width - 12, rect.Height - 12));
                  g.DrawEllipse(Pens.Black, new Rectangle(rect.X + 6, rect.Y + 6, rect.Width - 12, rect.Height - 12));
                }

                if (square == Board.LocationToSquare(GetMouseLocation(rotateBoard)) ||
                  (!LiftedPieceIsDragged && square == PieceLifted.Square))
                {
                  Pen borderpen = new Pen(Color.FromArgb(0, 0, 0), 3);
                  g.DrawRectangle(borderpen, new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3));
                }
              }
          }

          //	Draw piece on square (if any)
          if (pieces.ContainsKey(location))
          {
            PieceInfo pieceOnSquare = pieces[location];
            if (PieceLifted == null || !LiftedPieceIsDragged || location != PieceLifted.Location)
              pieceSetPresentation.Render(g, rect, pieceOnSquare.Player, pieceOnSquare.PieceType.TypeNumber);
          }
        }

        //	draw thin line between squares and border and outside border
        Pen thinBlackPen = new Pen(Color.Black, 1);
        if (Theme.NSquareColors > 1 && GetType() == typeof(BoardPresentation))
        {
          g.DrawRectangle(thinBlackPen, mapping.BorderSize, mapping.BorderSize, mapping.SquareSize * Board.NumFiles,
            mapping.SquareSize * Board.NumRanks);
        }
        g.DrawRectangle(thinBlackPen, 0, 0, mapping.TotalWidth - 1, mapping.TotalHeight - 1);

        //  highlight border of squares of recent move if computer has just finished moving
        if (highlightComputerMove && Game.HighlightSquares != null)
        {
          Pen moveOutlinePen = new Pen(Theme.ColorScheme.HighlightColor, 3);
          foreach (int square in Game.HighlightSquares)
          {
            Location location = Board.SquareToLocation(square);
            Rectangle rect = mapping.MapLocation(location, rotateBoard);
            rect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height - 1);
            g.DrawRectangle(moveOutlinePen, rect);
          }
        }

        //	create "centered" text alignment for drawing rank & file labels
        StringFormat centered = new StringFormat();
        centered.Alignment = StringAlignment.Center;
        centered.LineAlignment = StringAlignment.Center;

        if (!SmallPreview)
        {
          //	draw file labels
          for (int file = 0; file < Board.NumFiles; file++)
          {
            Location squareAboveLocation = new Location(0, rotateBoard ? Board.NumFiles - file - 1 : file);
            Rectangle squareAboveRect = mapping.MapLocation(squareAboveLocation);
            Rectangle textrect = new Rectangle(squareAboveRect.Left, squareAboveRect.Bottom + 2, squareAboveRect.Width, mapping.BorderSize - 2);
            g.DrawString(Board.GetFileNotation(file), System.Drawing.SystemFonts.CaptionFont, new SolidBrush(Theme.ColorScheme.TextColor), textrect, centered);
          }

          //	draw rank labels
          for (int rank = 0; rank < Board.NumRanks; rank++)
          {
            Location squareToTheRightLocation = new Location(rotateBoard ? Board.NumRanks - rank - 1 : rank, 0);
            Rectangle squareToTheRightRect = mapping.MapLocation(squareToTheRightLocation);
            Rectangle textrect = new Rectangle(squareToTheRightRect.Left - mapping.BorderSize, squareToTheRightRect.Top, mapping.BorderSize, squareToTheRightRect.Height);
            g.DrawString(Board.GetRankNotation(rank), System.Drawing.SystemFonts.CaptionFont, new SolidBrush(Theme.ColorScheme.TextColor), textrect, centered);
          }
        }

        if (!SmallPreview && piece == null && kingThreatRenderer != null)
        {
          Location hoveredLocation = GetMouseLocation(rotateBoard);
          int? hoveredSquare = hoveredLocation.IsNull ? null : Board.LocationToSquare(hoveredLocation);
          kingThreatRenderer.Render(g, Board,
            square => mapping.MapLocation(Board.SquareToLocation(square), rotateBoard), hoveredSquare);
        }

        //	draw lifted piece (if any)
        if (PieceLifted != null && LiftedPieceIsDragged)
          pieceSetPresentation.RenderFloatingPiece(g, MouseCoordinate, PieceLifted);

        return bmp;
      }
      catch (Exception ex)
      {
        ExceptionForm form = new ExceptionForm(ex, Game);
        form.ShowDialog();
        return null;
      }
    }
  }
}
