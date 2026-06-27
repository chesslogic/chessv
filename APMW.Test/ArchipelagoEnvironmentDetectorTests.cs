using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Archipelago.APChessV.Generation.Environment;

namespace Archipelago.APChessV
{
  [TestClass]
  public class ArchipelagoEnvironmentDetectorTests
  {
    private readonly List<string> createdDirectories = new List<string>();

    [TestCleanup]
    public void Cleanup()
    {
      foreach (string directory in createdDirectories.AsEnumerable().Reverse())
      {
        if (Directory.Exists(directory))
        {
          Directory.Delete(directory, true);
        }
      }
    }

    [TestMethod]
    public void ValidateRoot_WithCompleteInstall_IsValidAndFindsScripts()
    {
      string rootPath = CreateSandbox("complete");
      CreateArchipelagoInstall(rootPath, includeMultiServer: true);
      CreateChecksMateApworld(rootPath, "custom_worlds");

      ArchipelagoEnvironmentInfo result = CreateDetector().ValidateRoot(rootPath);

      Assert.IsTrue(result.IsValidForGeneration, FormatMessages(result));
      Assert.AreEqual(Path.Combine(rootPath, "python.exe"), result.PythonExecutablePath);
      Assert.AreEqual(Path.Combine(rootPath, "Generate.py"), result.GenerateScriptPath);
      Assert.AreEqual(Path.Combine(rootPath, "MultiServer.py"), result.MultiServerScriptPath);
      Assert.AreEqual(Path.Combine(rootPath, "custom_worlds", "checksmate.apworld"), result.ChecksMateWorldPath);
      Assert.IsFalse(result.ValidationMessages.Any(message => message.Severity == ArchipelagoEnvironmentValidationSeverity.Error), FormatMessages(result));
    }

    [TestMethod]
    public void ValidateRoot_WithoutMultiServer_RemainsGenerationValidAndWarns()
    {
      string rootPath = CreateSandbox("no-server");
      CreateArchipelagoInstall(rootPath, includeMultiServer: false);
      CreateChecksMateWorldPackage(rootPath);

      ArchipelagoEnvironmentInfo result = CreateDetector().ValidateRoot(rootPath);

      Assert.IsTrue(result.IsValidForGeneration, FormatMessages(result));
      Assert.IsFalse(result.HasOptionalSoloServer);
      Assert.IsTrue(
        result.ValidationMessages.Any(message =>
          message.Severity == ArchipelagoEnvironmentValidationSeverity.Warning &&
          message.Message.Contains("MultiServer.py")),
        FormatMessages(result));
    }

    [TestMethod]
    public void ValidateRoot_WithoutGenerateScript_IsInvalidAndReportsRequiredFile()
    {
      string rootPath = CreateSandbox("missing-generate");
      File.WriteAllText(Path.Combine(rootPath, "python.exe"), string.Empty);
      CreateChecksMateApworld(rootPath, "worlds");

      ArchipelagoEnvironmentInfo result = CreateDetector().ValidateRoot(rootPath);

      Assert.IsFalse(result.IsValidForGeneration);
      Assert.IsTrue(
        result.ValidationMessages.Any(message =>
          message.Severity == ArchipelagoEnvironmentValidationSeverity.Error &&
          message.Message.Contains("Generate.py")),
        FormatMessages(result));
    }

    [TestMethod]
    public void Detect_ConfiguredPathTakesPrecedenceOverAppBaseParents()
    {
      string configuredRootPath = CreateSandbox("configured");
      CreateArchipelagoInstall(configuredRootPath, includeMultiServer: true);
      CreateChecksMateApworld(configuredRootPath, "custom_worlds");

      string appBaseRootPath = CreateSandbox("app-base-root");
      CreateArchipelagoInstall(appBaseRootPath, includeMultiServer: true);
      CreateChecksMateApworld(appBaseRootPath, "custom_worlds");
      string appBaseDirectory = Path.Combine(appBaseRootPath, "ChecksMate", "net7.0-windows7.0");
      Directory.CreateDirectory(appBaseDirectory);

      ArchipelagoEnvironmentInfo result = CreateDetector(appBaseDirectory).Detect(configuredRootPath);

      Assert.IsTrue(result.IsValidForGeneration, FormatMessages(result));
      Assert.AreEqual(configuredRootPath, result.RootPath);
    }

    [TestMethod]
    public void Detect_AppBaseParentsFindArchipelagoRoot()
    {
      string rootPath = CreateSandbox("parent-root");
      CreateArchipelagoInstall(rootPath, includeMultiServer: true);
      CreateChecksMateApworld(rootPath, "custom_worlds");
      string appBaseDirectory = Path.Combine(rootPath, "ChecksMate", "net7.0-windows7.0");
      Directory.CreateDirectory(appBaseDirectory);

      ArchipelagoEnvironmentInfo result = CreateDetector(appBaseDirectory).Detect();

      Assert.IsTrue(result.IsValidForGeneration, FormatMessages(result));
      Assert.AreEqual(rootPath, result.RootPath);
    }

    [TestMethod]
    public void Detect_UsesProgramDataArchipelagoPath()
    {
      string programDataPath = CreateSandbox("program-data");
      string rootPath = Path.Combine(programDataPath, "Archipelago");
      Directory.CreateDirectory(rootPath);
      CreateArchipelagoInstall(rootPath, includeMultiServer: true);
      CreateChecksMateApworld(rootPath, "custom_worlds");

      string appBaseDirectory = Path.Combine(CreateSandbox("empty-app-base"), "ChecksMate");
      Directory.CreateDirectory(appBaseDirectory);
      ArchipelagoEnvironmentDetector detector = new ArchipelagoEnvironmentDetector(
        appBaseDirectory,
        name => string.Equals(name, "ProgramData", StringComparison.OrdinalIgnoreCase) ? programDataPath : null);

      ArchipelagoEnvironmentInfo result = detector.Detect();

      Assert.IsTrue(result.IsValidForGeneration, FormatMessages(result));
      Assert.AreEqual(rootPath, result.RootPath);
    }

    [TestMethod]
    public void Detect_ValidLaterCandidateDoesNotKeepErrorsFromEarlierCandidates()
    {
      string invalidConfiguredPath = CreateSandbox("invalid-configured");
      string programDataPath = CreateSandbox("program-data");
      string rootPath = Path.Combine(programDataPath, "Archipelago");
      Directory.CreateDirectory(rootPath);
      CreateArchipelagoInstall(rootPath, includeMultiServer: true);
      CreateChecksMateApworld(rootPath, "custom_worlds");

      ArchipelagoEnvironmentDetector detector = new ArchipelagoEnvironmentDetector(
        CreateSandbox("empty-app-base"),
        name => string.Equals(name, "ProgramData", StringComparison.OrdinalIgnoreCase) ? programDataPath : null);

      ArchipelagoEnvironmentInfo result = detector.Detect(invalidConfiguredPath);

      Assert.IsTrue(result.IsValidForGeneration, FormatMessages(result));
      Assert.AreEqual(rootPath, result.RootPath);
      Assert.IsFalse(
        result.ValidationMessages.Any(message => message.Severity == ArchipelagoEnvironmentValidationSeverity.Error),
        FormatMessages(result));
    }

    private ArchipelagoEnvironmentDetector CreateDetector(string appBaseDirectory = null)
    {
      return new ArchipelagoEnvironmentDetector(appBaseDirectory ?? CreateSandbox("app-base"), _ => null);
    }

    private string CreateSandbox(string name)
    {
      string baseDirectory = Path.Combine(
        System.Environment.CurrentDirectory,
        "ArchipelagoEnvironmentDetectorTestData");
      string path = Path.Combine(
        baseDirectory,
        $"{name}-{Guid.NewGuid():N}");

      Directory.CreateDirectory(path);
      createdDirectories.Add(path);
      return path;
    }

    private static void CreateArchipelagoInstall(string rootPath, bool includeMultiServer)
    {
      File.WriteAllText(Path.Combine(rootPath, "python.exe"), string.Empty);
      File.WriteAllText(Path.Combine(rootPath, "Generate.py"), string.Empty);

      if (includeMultiServer)
      {
        File.WriteAllText(Path.Combine(rootPath, "MultiServer.py"), string.Empty);
      }
    }

    private static void CreateChecksMateApworld(string rootPath, string relativeDirectory)
    {
      string directoryPath = Path.Combine(rootPath, relativeDirectory);
      Directory.CreateDirectory(directoryPath);
      File.WriteAllText(Path.Combine(directoryPath, "checksmate.apworld"), string.Empty);
    }

    private static void CreateChecksMateWorldPackage(string rootPath)
    {
      string directoryPath = Path.Combine(rootPath, "worlds", "checksmate");
      Directory.CreateDirectory(directoryPath);
      File.WriteAllText(Path.Combine(directoryPath, "__init__.py"), string.Empty);
    }

    private static string FormatMessages(ArchipelagoEnvironmentInfo result)
    {
      return string.Join(System.Environment.NewLine, result.ValidationMessages.Select(message => message.ToString()));
    }
  }
}
