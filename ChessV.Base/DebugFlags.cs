namespace ChessV
{
  public static class DebugFlags
  {
    // Enable temporary make/undo validation in Match.OnMoveMade
    public static bool ValidateMovesBeforeCommit { get; set; } = false;

    // If true, throw on detected invariant violations; otherwise log and continue safely
    public static bool ThrowOnInvariantViolation { get; set; } = false;

    // If true, include verbose diagnostic string building; otherwise keep it minimal
    public static bool VerboseDiagnostics { get; set; } = true;
  }
}


