using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public sealed class ArchipelagoProcessResult
  {
    public ArchipelagoProcessResult(
      int exitCode,
      IEnumerable<string> standardOutputLines,
      IEnumerable<string> standardErrorLines)
    {
      ExitCode = exitCode;
      StandardOutputLines = ReadOnlyStrings(standardOutputLines);
      StandardErrorLines = ReadOnlyStrings(standardErrorLines);
    }

    public int ExitCode { get; }

    public IReadOnlyList<string> StandardOutputLines { get; }

    public IReadOnlyList<string> StandardErrorLines { get; }

    private static IReadOnlyList<string> ReadOnlyStrings(IEnumerable<string> values)
    {
      return new ReadOnlyCollection<string>(
        (values ?? Array.Empty<string>())
          .Where(value => value != null)
          .ToList());
    }
  }
}
