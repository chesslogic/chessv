using Archipelago.APChessV.Generation.Environment;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public sealed class ArchipelagoServerLaunchRequest
  {
    public const string DefaultHost = "localhost";
    public const int DefaultPort = 38281;

    public ArchipelagoServerLaunchRequest(
      ArchipelagoEnvironmentInfo environment,
      string artifactPath)
    {
      Environment = environment;
      ArtifactPath = artifactPath;
    }

    public ArchipelagoEnvironmentInfo Environment { get; }

    public string ArtifactPath { get; }

    public string Host { get; set; } = DefaultHost;

    public int Port { get; set; } = DefaultPort;
  }
}
