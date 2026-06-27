using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.APChessV.Generation.Environment
{
  public sealed class ArchipelagoEnvironmentInfo
  {
    public ArchipelagoEnvironmentInfo(
      string rootPath,
      string pythonExecutablePath,
      string generateScriptPath,
      string multiServerScriptPath,
      string checksMateWorldPath,
      IEnumerable<ArchipelagoEnvironmentValidationMessage> validationMessages)
    {
      RootPath = rootPath;
      PythonExecutablePath = pythonExecutablePath;
      GenerateScriptPath = generateScriptPath;
      MultiServerScriptPath = multiServerScriptPath;
      ChecksMateWorldPath = checksMateWorldPath;
      ValidationMessages = new ReadOnlyCollection<ArchipelagoEnvironmentValidationMessage>(
        (validationMessages ?? Array.Empty<ArchipelagoEnvironmentValidationMessage>()).ToList());
    }

    public string RootPath { get; }

    public string PythonExecutablePath { get; }

    public string GenerateScriptPath { get; }

    public string MultiServerScriptPath { get; }

    public string ChecksMateWorldPath { get; }

    public IReadOnlyList<ArchipelagoEnvironmentValidationMessage> ValidationMessages { get; }

    public bool HasRootPath => !string.IsNullOrWhiteSpace(RootPath);

    public bool HasPythonExecutable => !string.IsNullOrWhiteSpace(PythonExecutablePath);

    public bool HasGenerateScript => !string.IsNullOrWhiteSpace(GenerateScriptPath);

    public bool HasOptionalSoloServer => !string.IsNullOrWhiteSpace(MultiServerScriptPath);

    public bool HasChecksMateWorld => !string.IsNullOrWhiteSpace(ChecksMateWorldPath);

    public bool IsValidForGeneration =>
      HasRootPath &&
      HasPythonExecutable &&
      HasGenerateScript &&
      HasChecksMateWorld;

    internal static ArchipelagoEnvironmentInfo NotFound(
      IEnumerable<ArchipelagoEnvironmentValidationMessage> validationMessages)
    {
      return new ArchipelagoEnvironmentInfo(null, null, null, null, null, validationMessages);
    }
  }
}
