using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public sealed class ArchipelagoServerLaunchResult
  {
    public ArchipelagoServerLaunchResult(
      bool success,
      bool wasCancelled,
      string host,
      int port,
      string artifactPath,
      ArchipelagoGenerationCommand command,
      IArchipelagoServerProcess serverProcess,
      IEnumerable<string> validationErrors,
      IEnumerable<string> logLines,
      string failureMessage)
    {
      Success = success;
      WasCancelled = wasCancelled;
      Host = string.IsNullOrWhiteSpace(host) ? ArchipelagoServerLaunchRequest.DefaultHost : host;
      Port = port;
      ArtifactPath = artifactPath;
      Command = command;
      ServerProcess = serverProcess;
      ValidationErrors = ReadOnlyStrings(validationErrors);
      LogLines = ReadOnlyStrings(logLines);
      FailureMessage = failureMessage;
    }

    public bool Success { get; }

    public bool WasCancelled { get; }

    public string Host { get; }

    public int Port { get; }

    public string ArtifactPath { get; }

    public string ConnectionAddress => Host + ":" + Port.ToString(CultureInfo.InvariantCulture);

    public ArchipelagoGenerationCommand Command { get; }

    public IArchipelagoServerProcess ServerProcess { get; }

    public int? ProcessId => ServerProcess?.ProcessId;

    public ArchipelagoServerProcessStatus Status =>
      ServerProcess == null ? ArchipelagoServerProcessStatus.NotStarted : ServerProcess.Status;

    public IReadOnlyList<string> ValidationErrors { get; }

    public IReadOnlyList<string> LogLines { get; }

    public string FailureMessage { get; }

    private static IReadOnlyList<string> ReadOnlyStrings(IEnumerable<string> values)
    {
      return new ReadOnlyCollection<string>(
        (values ?? Array.Empty<string>())
          .Where(value => value != null)
          .ToList());
    }
  }
}
