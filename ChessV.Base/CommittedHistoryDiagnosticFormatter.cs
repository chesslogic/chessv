using System;
using System.Collections.Generic;
using System.Text;

namespace ChessV
{
  public static class CommittedHistoryDiagnosticFormatter
  {
    public const int DefaultRecentMoveLimit = 8;

    public static string Format(Game game, int recentMoveLimit = DefaultRecentMoveLimit)
    {
      var output = new StringBuilder(1024);
      output.AppendLine("=== Committed History Metadata ===");

      if (game == null)
      {
        output.AppendLine("Game: <null>");
        output.AppendLine("Committed history metadata unavailable: game is null.");
        output.AppendLine("=== End Committed History Metadata ===");
        return output.ToString();
      }

      int gameMoveNumber;
      string failure;
      if (!TryReadGameMoveNumber(game, out gameMoveNumber, out failure))
      {
        output.Append("Game Move Number: <unavailable: ").Append(failure).AppendLine(">");
        output.AppendLine("Recent Committed Moves: <unavailable because GameMoveNumber could not be read>");
        output.AppendLine("Turn Boundary Context: <unavailable because GameMoveNumber could not be read>");
        output.AppendLine("=== End Committed History Metadata ===");
        return output.ToString();
      }

      output.Append("Game Move Number: ").Append(gameMoveNumber).AppendLine();

      int currentTurnNumber;
      bool hasCurrentTurnNumber = TryReadCurrentTurnNumber(game, out currentTurnNumber, out failure);
      if (hasCurrentTurnNumber)
        output.Append("Current Turn Number: ").Append(currentTurnNumber).AppendLine();
      else
        output.Append("Current Turn Number: <unavailable: ").Append(failure).AppendLine(">");

      if (gameMoveNumber < 0)
      {
        output.Append("Recent Committed Moves: <unavailable because GameMoveNumber is negative (")
          .Append(gameMoveNumber).AppendLine(")>");
        output.AppendLine("Turn Boundary Context: <unavailable because GameMoveNumber is invalid>");
        output.AppendLine("=== End Committed History Metadata ===");
        return output.ToString();
      }

      int limit = Math.Max(0, recentMoveLimit);
      int startMoveIndex = Math.Max(0, gameMoveNumber - limit);
      int inspectedCount = gameMoveNumber - startMoveIndex;
      var inspectedRecords = new List<HistoryRecord>(inspectedCount);

      output.Append("Recent Committed Moves (last ")
        .Append(inspectedCount)
        .Append(" of ")
        .Append(gameMoveNumber)
        .AppendLine("):");

      if (gameMoveNumber == 0)
        output.AppendLine("  (none)");
      else if (limit == 0)
        output.AppendLine("  (not shown; recent move limit is 0)");
      else
      {
        for (int moveIndex = startMoveIndex; moveIndex < gameMoveNumber; moveIndex++)
          AppendHistoricalMove(output, game, moveIndex, inspectedRecords);
      }

      AppendTurnBoundaryContext(
        output,
        inspectedRecords,
        startMoveIndex,
        gameMoveNumber,
        hasCurrentTurnNumber,
        currentTurnNumber);

      output.AppendLine("=== End Committed History Metadata ===");
      return output.ToString();
    }

    private static void AppendHistoricalMove(
      StringBuilder output,
      Game game,
      int moveIndex,
      List<HistoryRecord> inspectedRecords)
    {
      MoveInfo move;
      int turnNumber;
      string failure;
      int displayMoveNumber = moveIndex + 1;

      if (!TryGetHistoricalMove(game, moveIndex, out move, out turnNumber, out failure))
      {
        output.Append("  #").Append(displayMoveNumber)
          .Append(": <unavailable: ").Append(failure).AppendLine(">");
        return;
      }

      inspectedRecords.Add(new HistoryRecord(displayMoveNumber, turnNumber));
      output.Append("  #").Append(displayMoveNumber)
        .Append(" recorded-turn=").Append(turnNumber)
        .Append(": ")
        .AppendLine(DescribeMoveForDiagnostics(game, move));
    }

    private static void AppendTurnBoundaryContext(
      StringBuilder output,
      List<HistoryRecord> inspectedRecords,
      int startMoveIndex,
      int gameMoveNumber,
      bool hasCurrentTurnNumber,
      int currentTurnNumber)
    {
      output.AppendLine("Turn Boundary Context:");

      if (gameMoveNumber == 0)
      {
        output.AppendLine("  No committed moves recorded.");
        if (hasCurrentTurnNumber)
          output.Append("  Current turn number: ").Append(currentTurnNumber).AppendLine(".");
        return;
      }

      if (inspectedRecords.Count == 0)
      {
        output.AppendLine("  No recent committed moves inspected.");
        if (hasCurrentTurnNumber)
          output.Append("  Current turn number: ").Append(currentTurnNumber).AppendLine(".");
        return;
      }

      if (startMoveIndex > 0)
        output.Append("  Earlier committed moves omitted before #").Append(startMoveIndex + 1).AppendLine(".");

      output.AppendLine("  Recorded turn ranges in inspected history:");
      int rangeTurnNumber = inspectedRecords[0].TurnNumber;
      int rangeStartMoveNumber = inspectedRecords[0].MoveNumber;

      for (int index = 1; index < inspectedRecords.Count; index++)
      {
        HistoryRecord record = inspectedRecords[index];
        if (record.TurnNumber != rangeTurnNumber)
        {
          AppendTurnRange(output, rangeTurnNumber, rangeStartMoveNumber, inspectedRecords[index - 1].MoveNumber);
          output.Append("  Turn number changed from ")
            .Append(rangeTurnNumber)
            .Append(" to ")
            .Append(record.TurnNumber)
            .Append(" at committed move #")
            .Append(record.MoveNumber)
            .AppendLine(".");
          rangeTurnNumber = record.TurnNumber;
          rangeStartMoveNumber = record.MoveNumber;
        }
      }

      HistoryRecord lastRecord = inspectedRecords[inspectedRecords.Count - 1];
      AppendTurnRange(output, rangeTurnNumber, rangeStartMoveNumber, lastRecord.MoveNumber);
      output.Append("  Last committed move #")
        .Append(lastRecord.MoveNumber)
        .Append(" recorded turn ")
        .Append(lastRecord.TurnNumber)
        .AppendLine(".");

      if (!hasCurrentTurnNumber)
      {
        output.AppendLine("  Current turn number unavailable; cannot compare with latest committed turn.");
        return;
      }

      if (currentTurnNumber == lastRecord.TurnNumber)
        output.AppendLine("  Current turn matches the latest recorded turn.");
      else if (currentTurnNumber > lastRecord.TurnNumber)
        output.Append("  Current turn ")
          .Append(currentTurnNumber)
          .Append(" is after latest recorded turn ")
          .Append(lastRecord.TurnNumber)
          .AppendLine(".");
      else
        output.Append("  Current turn ")
          .Append(currentTurnNumber)
          .Append(" is before latest recorded turn ")
          .Append(lastRecord.TurnNumber)
          .AppendLine("; history may reflect a partially unwound or loaded state.");
    }

    private static void AppendTurnRange(StringBuilder output, int turnNumber, int startMoveNumber, int endMoveNumber)
    {
      output.Append("    turn ").Append(turnNumber).Append(": ");
      if (startMoveNumber == endMoveNumber)
        output.Append("move #").Append(startMoveNumber);
      else
        output.Append("moves #").Append(startMoveNumber).Append("-#").Append(endMoveNumber);
      output.AppendLine();
    }

    private static bool TryReadGameMoveNumber(Game game, out int gameMoveNumber, out string failure)
    {
      try
      {
        gameMoveNumber = game.GameMoveNumber;
        failure = null;
        return true;
      }
      catch (Exception ex)
      {
        gameMoveNumber = 0;
        failure = FormatFailure(ex);
        return false;
      }
    }

    private static bool TryReadCurrentTurnNumber(Game game, out int currentTurnNumber, out string failure)
    {
      try
      {
        currentTurnNumber = game.GameTurnNumber;
        failure = null;
        return true;
      }
      catch (Exception ex)
      {
        currentTurnNumber = 0;
        failure = FormatFailure(ex);
        return false;
      }
    }

    private static bool TryGetHistoricalMove(
      Game game,
      int moveIndex,
      out MoveInfo move,
      out int turnNumber,
      out string failure)
    {
      try
      {
        move = game.GetHistoricalMove(moveIndex, out turnNumber);
        failure = null;
        return true;
      }
      catch (Exception ex)
      {
        move = default(MoveInfo);
        turnNumber = 0;
        failure = FormatFailure(ex);
        return false;
      }
    }

    private static string DescribeMoveForDiagnostics(Game game, MoveInfo move)
    {
      try
      {
        string description = game.DescribeMove(move, MoveNotation.StandardAlgebraic);
        if (!string.IsNullOrEmpty(description))
          return OneLine(description);
      }
      catch (Exception ex)
      {
        return FormatMoveInfo(move) + " (DescribeMove failed: " + FormatFailure(ex) + ")";
      }

      return FormatMoveInfo(move);
    }

    private static string FormatMoveInfo(MoveInfo move)
    {
      return string.Format(
        "type={0} player={1} from={2} to={3} tag={4} hash=0x{5:X16}",
        move.MoveType,
        move.Player,
        move.FromSquare,
        move.ToSquare,
        move.Tag,
        move.Hash);
    }

    private static string FormatFailure(Exception ex)
    {
      if (ex == null)
        return "unknown failure";
      string message = OneLine(ex.Message);
      if (string.IsNullOrEmpty(message))
        return ex.GetType().Name;
      return ex.GetType().Name + ": " + message;
    }

    private static string OneLine(string text)
    {
      if (string.IsNullOrEmpty(text))
        return text;
      return text.Replace('\r', ' ').Replace('\n', ' ');
    }

    private sealed class HistoryRecord
    {
      public HistoryRecord(int moveNumber, int turnNumber)
      {
        MoveNumber = moveNumber;
        TurnNumber = turnNumber;
      }

      public int MoveNumber { get; private set; }
      public int TurnNumber { get; private set; }
    }
  }
}
