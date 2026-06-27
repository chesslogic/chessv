using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.APChessV.Generation.Subprocess
{
  internal sealed class ArchipelagoServerProcess : IArchipelagoServerProcess
  {
    private readonly Process process;
    private readonly List<string> standardOutputLines;
    private readonly List<string> standardErrorLines;
    private readonly object outputLock;

    public ArchipelagoServerProcess(
      Process process,
      List<string> standardOutputLines,
      List<string> standardErrorLines,
      object outputLock)
    {
      this.process = process ?? throw new ArgumentNullException(nameof(process));
      this.standardOutputLines = standardOutputLines ?? throw new ArgumentNullException(nameof(standardOutputLines));
      this.standardErrorLines = standardErrorLines ?? throw new ArgumentNullException(nameof(standardErrorLines));
      this.outputLock = outputLock ?? throw new ArgumentNullException(nameof(outputLock));
    }

    public int? ProcessId
    {
      get
      {
        try
        {
          return process.Id;
        }
        catch (InvalidOperationException)
        {
          return null;
        }
      }
    }

    public bool HasExited
    {
      get
      {
        try
        {
          return process.HasExited;
        }
        catch (InvalidOperationException)
        {
          return true;
        }
      }
    }

    public int? ExitCode
    {
      get
      {
        if (!HasExited)
        {
          return null;
        }

        try
        {
          return process.ExitCode;
        }
        catch (InvalidOperationException)
        {
          return null;
        }
      }
    }

    public ArchipelagoServerProcessStatus Status =>
      HasExited ? ArchipelagoServerProcessStatus.Exited : ArchipelagoServerProcessStatus.Running;

    public IReadOnlyList<string> StandardOutputLines => Snapshot(standardOutputLines);

    public IReadOnlyList<string> StandardErrorLines => Snapshot(standardErrorLines);

    public Task WaitForExitAsync(CancellationToken cancellationToken = default)
    {
      return process.WaitForExitAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
      TryKill(process);
      await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
      process.WaitForExit();
    }

    public void Dispose()
    {
      process.Dispose();
    }

    private IReadOnlyList<string> Snapshot(IEnumerable<string> lines)
    {
      lock (outputLock)
      {
        return new ReadOnlyCollection<string>(lines.ToList());
      }
    }

    internal static void AddLine(ICollection<string> lines, object outputLock, string line)
    {
      if (line == null)
      {
        return;
      }

      lock (outputLock)
      {
        lines.Add(line);
      }
    }

    internal static void TryKill(Process process)
    {
      try
      {
        if (!process.HasExited)
        {
          process.Kill(entireProcessTree: true);
        }
      }
      catch (InvalidOperationException)
      {
      }
      catch (Win32Exception)
      {
      }
      catch (NotSupportedException)
      {
      }
    }
  }
}
