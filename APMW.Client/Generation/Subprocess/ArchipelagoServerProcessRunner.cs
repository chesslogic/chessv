using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public sealed class ArchipelagoServerProcessRunner : IArchipelagoServerProcessRunner
  {
    public async Task<IArchipelagoServerProcess> StartAsync(
      ArchipelagoGenerationCommand command,
      CancellationToken cancellationToken)
    {
      if (command == null)
      {
        throw new ArgumentNullException(nameof(command));
      }

      cancellationToken.ThrowIfCancellationRequested();

      var standardOutputLines = new List<string>();
      var standardErrorLines = new List<string>();
      object outputLock = new object();

      var startInfo = new ProcessStartInfo
      {
        FileName = command.ExecutablePath,
        WorkingDirectory = command.WorkingDirectory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
      };

      foreach (string argument in command.Arguments)
      {
        startInfo.ArgumentList.Add(argument);
      }

      var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
      process.OutputDataReceived += (_, args) =>
        ArchipelagoServerProcess.AddLine(standardOutputLines, outputLock, args.Data);
      process.ErrorDataReceived += (_, args) =>
        ArchipelagoServerProcess.AddLine(standardErrorLines, outputLock, args.Data);

      try
      {
        if (!process.Start())
        {
          throw new InvalidOperationException("Unable to start Archipelago server process.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        if (cancellationToken.IsCancellationRequested)
        {
          ArchipelagoServerProcess.TryKill(process);
          await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
          cancellationToken.ThrowIfCancellationRequested();
        }

        return new ArchipelagoServerProcess(process, standardOutputLines, standardErrorLines, outputLock);
      }
      catch
      {
        process.Dispose();
        throw;
      }
    }
  }
}
