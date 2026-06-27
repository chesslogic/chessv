using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public interface IArchipelagoServerProcess : IDisposable
  {
    int? ProcessId { get; }

    bool HasExited { get; }

    int? ExitCode { get; }

    ArchipelagoServerProcessStatus Status { get; }

    IReadOnlyList<string> StandardOutputLines { get; }

    IReadOnlyList<string> StandardErrorLines { get; }

    Task WaitForExitAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
  }
}
