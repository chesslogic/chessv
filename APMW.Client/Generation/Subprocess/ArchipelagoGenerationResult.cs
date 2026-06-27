using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public sealed class ArchipelagoGenerationResult
  {
    public ArchipelagoGenerationResult(
      bool success,
      bool wasCancelled,
      int? exitCode,
      string playerFilesDirectoryPath,
      string playerFilePath,
      string outputDirectoryPath,
      ArchipelagoGenerationCommand command,
      IEnumerable<string> artifactPaths,
      IEnumerable<string> standardOutputLines,
      IEnumerable<string> standardErrorLines,
      IEnumerable<string> logLines,
      IEnumerable<string> validationErrors,
      string failureMessage)
    {
      Success = success;
      WasCancelled = wasCancelled;
      ExitCode = exitCode;
      PlayerFilesDirectoryPath = playerFilesDirectoryPath;
      PlayerFilePath = playerFilePath;
      OutputDirectoryPath = outputDirectoryPath;
      Command = command;
      ArtifactPaths = ReadOnlyStrings(artifactPaths);
      ArchipelagoArtifactPaths = new ReadOnlyCollection<string>(
        ArtifactPaths
          .Where(IsArchipelagoArtifact)
          .ToList());
      StandardOutputLines = ReadOnlyStrings(standardOutputLines);
      StandardErrorLines = ReadOnlyStrings(standardErrorLines);
      LogLines = ReadOnlyStrings(logLines);
      ValidationErrors = ReadOnlyStrings(validationErrors);
      FailureMessage = failureMessage;
    }

    public bool Success { get; }

    public bool WasCancelled { get; }

    public int? ExitCode { get; }

    public string PlayerFilesDirectoryPath { get; }

    public string PlayerFilePath { get; }

    public string OutputDirectoryPath { get; }

    public ArchipelagoGenerationCommand Command { get; }

    public IReadOnlyList<string> ArtifactPaths { get; }

    public IReadOnlyList<string> ArchipelagoArtifactPaths { get; }

    public string PrimaryArchipelagoArtifactPath => ArchipelagoArtifactPaths.FirstOrDefault();

    public IReadOnlyList<string> StandardOutputLines { get; }

    public IReadOnlyList<string> StandardErrorLines { get; }

    public IReadOnlyList<string> LogLines { get; }

    public IReadOnlyList<string> ValidationErrors { get; }

    public string FailureMessage { get; }

    private static IReadOnlyList<string> ReadOnlyStrings(IEnumerable<string> values)
    {
      return new ReadOnlyCollection<string>(
        (values ?? Array.Empty<string>())
          .Where(value => value != null)
          .ToList());
    }

    private static bool IsArchipelagoArtifact(string path)
    {
      return string.Equals(
        System.IO.Path.GetExtension(path),
        ".archipelago",
        StringComparison.OrdinalIgnoreCase);
    }
  }
}
