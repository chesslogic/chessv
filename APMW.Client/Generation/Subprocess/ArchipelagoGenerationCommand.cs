using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public sealed class ArchipelagoGenerationCommand
  {
    public ArchipelagoGenerationCommand(
      string executablePath,
      IEnumerable<string> arguments,
      string workingDirectory)
    {
      if (string.IsNullOrWhiteSpace(executablePath))
      {
        throw new ArgumentException("Executable path is required.", nameof(executablePath));
      }

      if (arguments == null)
      {
        throw new ArgumentNullException(nameof(arguments));
      }

      ExecutablePath = executablePath;
      Arguments = new ReadOnlyCollection<string>(arguments.ToList());
      WorkingDirectory = workingDirectory;
    }

    public string ExecutablePath { get; }

    public IReadOnlyList<string> Arguments { get; }

    public string WorkingDirectory { get; }
  }
}
