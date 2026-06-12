using System;
using System.Collections.Generic;
using System.Text;

namespace ChessV
{
  public static class BoardDiagnosticFormatter
  {
    public const string EmptySquareText = "..";

    public static string Format(Game game)
    {
      if (game == null)
        return "Board Diagnostic" + Environment.NewLine + "Board diagnostic unavailable: game is null.";

      return Format(game.Board, game);
    }

    public static string Format(Board board)
    {
      return Format(board, board == null ? null : board.Game);
    }

    private static string Format(Board board, Game game)
    {
      if (board == null)
        return "Board Diagnostic" + Environment.NewLine + "Board diagnostic unavailable: board is null.";

      StringBuilder sb = new StringBuilder(2048);
      try
      {
        AppendBoard(sb, board, game);
      }
      catch (Exception ex)
      {
        AppendFailure(sb, ex);
      }
      return sb.ToString();
    }

    private static void AppendBoard(StringBuilder sb, Board board, Game game)
    {
      sb.AppendLine("Board Diagnostic");
      sb.AppendLine("Dimensions: " + board.NumFiles + " files x " + board.NumRanks + " ranks");
      sb.AppendLine("Squares: " + board.NumSquares + " board, " + board.NumSquaresExtended + " extended");
      sb.AppendLine("Current side: " + (game == null ? "<unavailable>" : game.CurrentSide.ToString()));
      sb.AppendLine("Game move number: " + (game == null ? "<unavailable>" : game.GameMoveNumber.ToString()));
      sb.AppendLine("Board hash: 0x" + board.HashCode.ToString("X16"));
      sb.AppendLine("Legend: " + EmptySquareText + " = empty, piece cells = compact piece notation + player marker");
      sb.AppendLine();

      if (board.NumFiles <= 0 || board.NumRanks <= 0)
      {
        sb.AppendLine("Board grid unavailable: NumFiles and NumRanks must both be positive.");
        return;
      }

      List<string> occupiedCells;
      int cellWidth;
      string[,] cells = BuildCells(board, out cellWidth, out occupiedCells);
      int rankLabelWidth = GetRankLabelWidth(board);

      AppendFileLabels(sb, board, rankLabelWidth, cellWidth);
      for (int rank = board.NumRanks - 1; rank >= 0; rank--)
      {
        string rankLabel = board.GetRankNotation(rank);
        sb.Append(rankLabel.PadLeft(rankLabelWidth));
        sb.Append(" | ");
        for (int file = 0; file < board.NumFiles; file++)
        {
          sb.Append(cells[rank, file].PadLeft(cellWidth));
          sb.Append(' ');
        }
        sb.Append("| ");
        sb.AppendLine(rankLabel);
      }
      AppendFileLabels(sb, board, rankLabelWidth, cellWidth);

      sb.AppendLine();
      AppendOccupiedCells(sb, occupiedCells);
    }

    private static string[,] BuildCells(Board board, out int cellWidth, out List<string> occupiedCells)
    {
      string[,] cells = new string[board.NumRanks, board.NumFiles];
      occupiedCells = new List<string>();
      cellWidth = EmptySquareText.Length;

      for (int file = 0; file < board.NumFiles; file++)
        cellWidth = Math.Max(cellWidth, board.GetFileNotation(file).Length);

      for (int rank = 0; rank < board.NumRanks; rank++)
      {
        for (int file = 0; file < board.NumFiles; file++)
        {
          int square = board.LocationToSquare(new Location(rank, file));
          Piece piece = board[square];
          string cellText = piece == null ? EmptySquareText : GetPieceText(piece);
          cells[rank, file] = cellText;
          cellWidth = Math.Max(cellWidth, cellText.Length);

          if (piece != null)
            occupiedCells.Add(board.GetDefaultSquareNotation(square) + "=" + cellText);
        }
      }

      return cells;
    }

    private static int GetRankLabelWidth(Board board)
    {
      int width = 1;
      for (int rank = 0; rank < board.NumRanks; rank++)
        width = Math.Max(width, board.GetRankNotation(rank).Length);
      return width;
    }

    private static string GetPieceText(Piece piece)
    {
      string notation = null;
      PieceType pieceType = piece.PieceType;
      if (pieceType != null && pieceType.NotationClean != null)
      {
        int player = piece.Player;
        if (player >= 0 && player < pieceType.NotationClean.Length)
          notation = pieceType.NotationClean[player];
      }
      if (string.IsNullOrEmpty(notation) &&
          pieceType != null &&
          pieceType.NotationClean != null &&
          pieceType.NotationClean.Length > 0)
        notation = pieceType.NotationClean[0];
      if (string.IsNullOrEmpty(notation) &&
          pieceType != null &&
          !string.IsNullOrEmpty(pieceType.Name))
        notation = pieceType.Name.Substring(0, 1);
      if (string.IsNullOrEmpty(notation))
        notation = "?";
      return notation + piece.Player;
    }

    private static void AppendFileLabels(StringBuilder sb, Board board, int rankLabelWidth, int cellWidth)
    {
      sb.Append(new string(' ', rankLabelWidth));
      sb.Append("   ");
      for (int file = 0; file < board.NumFiles; file++)
      {
        sb.Append(Center(board.GetFileNotation(file), cellWidth));
        sb.Append(' ');
      }
      sb.AppendLine();
    }

    private static string Center(string value, int width)
    {
      if (value == null)
        value = string.Empty;
      if (value.Length >= width)
        return value;

      int padding = width - value.Length;
      int left = padding / 2;
      int right = padding - left;
      return new string(' ', left) + value + new string(' ', right);
    }

    private static void AppendOccupiedCells(StringBuilder sb, List<string> occupiedCells)
    {
      if (occupiedCells.Count == 0)
      {
        sb.AppendLine("Occupied cells: none");
        return;
      }

      sb.AppendLine("Occupied cells:");
      for (int index = 0; index < occupiedCells.Count; index++)
      {
        if (index % 8 == 0)
          sb.Append("  ");
        else
          sb.Append(' ');

        sb.Append(occupiedCells[index]);

        if (index == occupiedCells.Count - 1 || index % 8 == 7)
          sb.AppendLine();
      }
    }

    private static void AppendFailure(StringBuilder sb, Exception ex)
    {
      if (sb.Length == 0)
        sb.AppendLine("Board Diagnostic");

      sb.AppendLine();
      sb.AppendLine("Board diagnostic formatting failed.");
      sb.AppendLine("Failure type: " + ex.GetType().FullName);
      sb.AppendLine("Failure message: " + ex.Message);
      if (ex.StackTrace != null)
      {
        sb.AppendLine("Failure stack trace:");
        sb.AppendLine(ex.StackTrace);
      }
    }
  }
}
