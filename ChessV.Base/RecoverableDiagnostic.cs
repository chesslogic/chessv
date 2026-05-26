using System;
using System.Text;

namespace ChessV
{
  public enum RecoverableDiagnosticResponse
  {
    Continue,
    ContinueAndSuppressDialogs
  }

  public sealed class RecoverableDiagnostic
  {
    public RecoverableDiagnostic(
      string source,
      string message,
      string details = null,
      Game game = null,
      IDebugMessageLog messageLog = null)
    {
      if (string.IsNullOrEmpty(source))
        throw new ArgumentException("source must be specified", nameof(source));
      if (string.IsNullOrEmpty(message))
        throw new ArgumentException("message must be specified", nameof(message));

      Source = source;
      Message = message;
      Details = details ?? string.Empty;
      Game = game;
      MessageLog = messageLog;
      CreatedAt = DateTime.Now;
    }

    public string Source { get; private set; }
    public string Message { get; private set; }
    public string Details { get; private set; }
    public Game Game { get; private set; }
    public IDebugMessageLog MessageLog { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public string FormatForLog()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("Recoverable diagnostic");
      sb.AppendLine("Source: " + Source);
      sb.AppendLine("Time: " + CreatedAt.ToString("u"));
      sb.AppendLine("Message: " + Message);
      if (!string.IsNullOrEmpty(Details))
      {
        sb.AppendLine();
        sb.AppendLine(Details);
      }
      return sb.ToString();
    }
  }

  public static class RecoverableDiagnostics
  {
    public static Func<RecoverableDiagnostic, RecoverableDiagnosticResponse> Handler { get; set; }
    public static bool SuppressDialogsForSession { get; set; }

    public static RecoverableDiagnosticResponse Report(RecoverableDiagnostic diagnostic)
    {
      if (diagnostic == null)
        throw new ArgumentNullException(nameof(diagnostic));

      Log(diagnostic);

      if (SuppressDialogsForSession || Handler == null)
        return RecoverableDiagnosticResponse.Continue;

      RecoverableDiagnosticResponse response = Handler(diagnostic);
      if (response == RecoverableDiagnosticResponse.ContinueAndSuppressDialogs)
        SuppressDialogsForSession = true;
      return response;
    }

    public static void ResetForTest()
    {
      Handler = null;
      SuppressDialogsForSession = false;
    }

    private static void Log(RecoverableDiagnostic diagnostic)
    {
      IDebugMessageLog log = diagnostic.MessageLog;
      if (log == null && diagnostic.Game != null)
        log = diagnostic.Game.MessageLog;
      if (log != null)
        log.DebugMessage(diagnostic.FormatForLog());
    }
  }
}
