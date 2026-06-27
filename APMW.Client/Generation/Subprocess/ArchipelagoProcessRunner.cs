using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public sealed class ArchipelagoProcessRunner : IArchipelagoProcessRunner
  {
    public async Task<ArchipelagoProcessResult> RunAsync(
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

      using (var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
      {
        process.OutputDataReceived += (_, args) => AddLine(standardOutputLines, outputLock, args.Data);
        process.ErrorDataReceived += (_, args) => AddLine(standardErrorLines, outputLock, args.Data);

        if (!process.Start())
        {
          throw new InvalidOperationException("Unable to start Archipelago generation process.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using (cancellationToken.Register(() => TryKill(process)))
        {
          try
          {
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
          }
          catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
          {
            TryKill(process);
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
          }
        }

        process.WaitForExit();

        lock (outputLock)
        {
          return new ArchipelagoProcessResult(
            process.ExitCode,
            standardOutputLines.ToArray(),
            standardErrorLines.ToArray());
        }
      }
    }

    private static void AddLine(ICollection<string> lines, object outputLock, string line)
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

    private static void TryKill(Process process)
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
