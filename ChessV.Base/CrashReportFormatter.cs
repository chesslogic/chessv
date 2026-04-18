using System;
using System.Text;

namespace ChessV
{
  // Centralized formatter for crash reports / unhandled-exception text.
  //
  // The GUI's ExceptionForm presents this in two places:
  //   - label2.Text = Exception.Message               (visually clipped by Label
  //                                                   bounds; users have reported
  //                                                   only seeing partial info)
  //   - txtExceptionDetails.Text = FormatException(ex) (full multi-line, scrolled)
  //   - Save log              = FormatExceptionChain(ex) (entire inner chain)
  //
  // Living in ChessV.Base lets ChessV.Test verify the format end-to-end without
  // dragging in the WinForms project. ExceptionForm delegates to these helpers.
  public static class CrashReportFormatter
  {
    // Loose upper bound on what the GUI label can display before the user has
    // to dig into the Details pane. Tests assert the message-only output for a
    // realistic crash stays at or below this. Not a hard runtime limit.
    public const int LabelDisplayBudgetBytes = 4096;

    // Ceiling for the full per-exception detail text (one frame of the chain).
    // Larger than the label budget because the textbox scrolls; this is just a
    // sanity guard against runaway diagnostics.
    public const int DetailBudgetBytes = 16 * 1024;

    public static string FormatException(Exception exception)
    {
      if (exception == null)
        return string.Empty;

      var detail = new StringBuilder(1024);
      detail.Append("Exception type: ").Append(exception.GetType().FullName).Append(Environment.NewLine);
      detail.Append("Message: ").Append(exception.Message).Append(Environment.NewLine);
      detail.Append("Source: ").Append(exception.Source).Append(Environment.NewLine);
      detail.Append("Stack Trace: ").Append(Environment.NewLine);
      detail.Append(exception.StackTrace);
      return detail.ToString();
    }

    // Walks the full InnerException chain from outermost to innermost,
    // emitting FormatException for each. This is what gets persisted when
    // the user clicks "Save Log" in ExceptionForm.
    public static string FormatExceptionChain(Exception exception)
    {
      if (exception == null)
        return string.Empty;

      var sb = new StringBuilder(2048);
      Exception cursor = exception;
      while (cursor != null)
      {
        sb.Append(FormatException(cursor));
        sb.Append(Environment.NewLine);
        sb.Append(Environment.NewLine);
        cursor = cursor.InnerException;
      }
      return sb.ToString();
    }
  }
}
