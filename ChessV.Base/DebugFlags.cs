using System;

namespace ChessV
{
  public static class DebugFlags
  {
    // Enable temporary make/undo validation in Match.OnMoveMade
    public static bool ValidateMovesBeforeCommit { get; set; } = true;

    // If true, throw on detected invariant violations; otherwise log and continue safely
    public static bool ThrowOnInvariantViolation { get; set; } = true;

    // If true, include verbose diagnostic string building; otherwise keep it minimal
    public static bool VerboseDiagnostics { get; set; } = true;

    // If true, MoveList will assert that Board.HashCode is restored after every
    // LegalMovesOnly Make/Unmake round-trip (AddMove, AddCapture, EndMoveAddCore).
    // Default is ON, but a CI / release off-switch is provided via:
    //   * MSBuild constant DISABLE_MAKEUNMAKE_ASSERT (compile-time hard off)
    //   * Environment variable CHESSV_DISABLE_MAKEUNMAKE_ASSERT=1|true (runtime off)
    public static bool AssertMakeUnmakeRoundTrip { get; set; }

    static DebugFlags()
    {
#if DISABLE_MAKEUNMAKE_ASSERT
      AssertMakeUnmakeRoundTrip = false;
#else
      bool enabled = true;
      try
      {
        string raw = Environment.GetEnvironmentVariable("CHESSV_DISABLE_MAKEUNMAKE_ASSERT");
        if (!string.IsNullOrEmpty(raw))
        {
          string normalized = raw.Trim();
          if (string.Equals(normalized, "1", StringComparison.OrdinalIgnoreCase) ||
              string.Equals(normalized, "true", StringComparison.OrdinalIgnoreCase))
          {
            enabled = false;
          }
        }
      }
      catch
      {
        // Defensive: never let env-var lookup failure prevent class init.
      }
      AssertMakeUnmakeRoundTrip = enabled;
#endif
    }
  }
}
