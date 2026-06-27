using System;
using Archipelago.APChessV.Generation.Environment;
using Archipelago.APChessV.Generation.Settings;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public sealed class ArchipelagoGenerationRequest
  {
    public ArchipelagoGenerationRequest(
      ChecksMateGenerationSettings settings,
      ArchipelagoEnvironmentInfo environment,
      string tempDirectoryPath,
      string outputDirectoryPath)
    {
      Settings = settings ?? throw new ArgumentNullException(nameof(settings));
      Environment = environment ?? throw new ArgumentNullException(nameof(environment));
      TempDirectoryPath = tempDirectoryPath;
      OutputDirectoryPath = outputDirectoryPath;
    }

    public ChecksMateGenerationSettings Settings { get; }

    public ArchipelagoEnvironmentInfo Environment { get; }

    public string TempDirectoryPath { get; }

    public string OutputDirectoryPath { get; }

    public string PlayerFileName { get; set; } = "ChecksMate.yaml";
  }
}
