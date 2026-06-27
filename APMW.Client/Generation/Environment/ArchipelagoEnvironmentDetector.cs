using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace Archipelago.APChessV.Generation.Environment
{
  public sealed class ArchipelagoEnvironmentDetector
  {
    private const string GenerateScriptFileName = "Generate.py";
    private const string MultiServerScriptFileName = "MultiServer.py";
    private const string ChecksMateApworldFileName = "checksmate.apworld";

    private static readonly string[] PythonExecutableRelativePaths =
    {
      "python.exe",
      @"Python\python.exe",
      @"python\python.exe",
      @"venv\Scripts\python.exe",
      @".venv\Scripts\python.exe",
      @"env\Scripts\python.exe",
    };

    private static readonly string[] ChecksMateWorldPackageRelativePaths =
    {
      @"worlds\checksmate\__init__.py",
      @"worlds\checks_mate\__init__.py",
    };

    private static readonly string[] ChecksMateApworldSearchRelativeDirectories =
    {
      "custom_worlds",
      "worlds",
      string.Empty,
    };

    private readonly string appBaseDirectory;
    private readonly Func<string, string> getEnvironmentVariable;

    public ArchipelagoEnvironmentDetector()
      : this(AppContext.BaseDirectory, System.Environment.GetEnvironmentVariable)
    {
    }

    internal ArchipelagoEnvironmentDetector(
      string appBaseDirectory,
      Func<string, string> getEnvironmentVariable)
    {
      this.appBaseDirectory = NormalizeInputPath(appBaseDirectory);
      this.getEnvironmentVariable = getEnvironmentVariable ?? System.Environment.GetEnvironmentVariable;
    }

    public ArchipelagoEnvironmentInfo Detect(string configuredRootPath = null)
    {
      var validationMessages = new List<ArchipelagoEnvironmentValidationMessage>();

      foreach (string candidateRootPath in GetCandidateRootPaths(configuredRootPath))
      {
        ArchipelagoEnvironmentInfo candidate = ValidateRoot(candidateRootPath);
        validationMessages.AddRange(candidate.ValidationMessages);

        if (candidate.IsValidForGeneration)
        {
          return new ArchipelagoEnvironmentInfo(
            candidate.RootPath,
            candidate.PythonExecutablePath,
            candidate.GenerateScriptPath,
            candidate.MultiServerScriptPath,
            candidate.ChecksMateWorldPath,
            candidate.ValidationMessages);
        }
      }

      validationMessages.Add(new ArchipelagoEnvironmentValidationMessage(
        ArchipelagoEnvironmentValidationSeverity.Error,
        "No valid Archipelago environment was found. Configure the Archipelago root path if it is installed somewhere else."));

      return ArchipelagoEnvironmentInfo.NotFound(validationMessages);
    }

    public ArchipelagoEnvironmentInfo ValidateRoot(string rootPath)
    {
      var validationMessages = new List<ArchipelagoEnvironmentValidationMessage>();
      string normalizedRootPath = NormalizeInputPath(rootPath);

      if (string.IsNullOrWhiteSpace(normalizedRootPath))
      {
        validationMessages.Add(new ArchipelagoEnvironmentValidationMessage(
          ArchipelagoEnvironmentValidationSeverity.Error,
          "Archipelago root path is empty."));
        return ArchipelagoEnvironmentInfo.NotFound(validationMessages);
      }

      if (!TryDirectoryExists(normalizedRootPath, validationMessages, ArchipelagoEnvironmentValidationSeverity.Error, out bool rootExists))
      {
        return new ArchipelagoEnvironmentInfo(normalizedRootPath, null, null, null, null, validationMessages);
      }

      if (!rootExists)
      {
        validationMessages.Add(new ArchipelagoEnvironmentValidationMessage(
          ArchipelagoEnvironmentValidationSeverity.Error,
          "Archipelago root directory was not found.",
          normalizedRootPath));
        return new ArchipelagoEnvironmentInfo(normalizedRootPath, null, null, null, null, validationMessages);
      }

      string generateScriptPath = FindRequiredFile(
        normalizedRootPath,
        GenerateScriptFileName,
        validationMessages);

      if (generateScriptPath == null)
      {
        validationMessages.Add(new ArchipelagoEnvironmentValidationMessage(
          ArchipelagoEnvironmentValidationSeverity.Error,
          "Required Archipelago Generate.py was not found.",
          Path.Combine(normalizedRootPath, GenerateScriptFileName)));
      }

      string pythonExecutablePath = FindPythonExecutable(normalizedRootPath, validationMessages);
      string multiServerScriptPath = FindOptionalMultiServer(normalizedRootPath, validationMessages);
      string checksMateWorldPath = FindChecksMateWorld(normalizedRootPath, validationMessages);

      return new ArchipelagoEnvironmentInfo(
        normalizedRootPath,
        pythonExecutablePath,
        generateScriptPath,
        multiServerScriptPath,
        checksMateWorldPath,
        validationMessages);
    }

    internal IReadOnlyList<string> GetCandidateRootPaths(string configuredRootPath = null)
    {
      var candidates = new List<string>();

      AddCandidateWithAncestors(candidates, configuredRootPath, 2);
      AddCandidateWithAncestors(candidates, appBaseDirectory, 6);

      string programDataPath = getEnvironmentVariable("ProgramData");
      AddCandidate(candidates, CombineIfNotEmpty(programDataPath, "Archipelago"));
      AddCandidate(candidates, @"C:\ProgramData\Archipelago");

      string localAppDataPath = getEnvironmentVariable("LOCALAPPDATA");
      AddCandidate(candidates, CombineIfNotEmpty(localAppDataPath, "Archipelago"));

      string userProfilePath = getEnvironmentVariable("USERPROFILE");
      AddCandidate(candidates, CombineIfNotEmpty(userProfilePath, @"Downloads\Archipelago"));

      return candidates
        .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList()
        .AsReadOnly();
    }

    private static string FindRequiredFile(
      string rootPath,
      string fileName,
      IList<ArchipelagoEnvironmentValidationMessage> validationMessages)
    {
      string path = Path.Combine(rootPath, fileName);
      return TryFileExists(
        path,
        validationMessages,
        ArchipelagoEnvironmentValidationSeverity.Error,
        out bool fileExists) && fileExists
          ? path
          : null;
    }

    private static string FindPythonExecutable(
      string rootPath,
      IList<ArchipelagoEnvironmentValidationMessage> validationMessages)
    {
      foreach (string relativePath in PythonExecutableRelativePaths)
      {
        string path = Path.Combine(rootPath, relativePath);
        if (TryFileExists(
          path,
          validationMessages,
          ArchipelagoEnvironmentValidationSeverity.Error,
          out bool fileExists) && fileExists)
        {
          return path;
        }
      }

      validationMessages.Add(new ArchipelagoEnvironmentValidationMessage(
        ArchipelagoEnvironmentValidationSeverity.Error,
        "Required Python executable was not found in the Archipelago environment.",
        rootPath));

      return null;
    }

    private static string FindOptionalMultiServer(
      string rootPath,
      IList<ArchipelagoEnvironmentValidationMessage> validationMessages)
    {
      string path = Path.Combine(rootPath, MultiServerScriptFileName);

      if (TryFileExists(
        path,
        validationMessages,
        ArchipelagoEnvironmentValidationSeverity.Warning,
        out bool fileExists) && fileExists)
      {
        return path;
      }

      validationMessages.Add(new ArchipelagoEnvironmentValidationMessage(
        ArchipelagoEnvironmentValidationSeverity.Warning,
        "Optional MultiServer.py was not found; local solo-server hosting was not validated.",
        path));

      return null;
    }

    private static string FindChecksMateWorld(
      string rootPath,
      IList<ArchipelagoEnvironmentValidationMessage> validationMessages)
    {
      foreach (string relativePath in ChecksMateWorldPackageRelativePaths)
      {
        string path = Path.Combine(rootPath, relativePath);
        if (TryFileExists(
          path,
          validationMessages,
          ArchipelagoEnvironmentValidationSeverity.Error,
          out bool fileExists) && fileExists)
        {
          return path;
        }
      }

      foreach (string relativeDirectory in ChecksMateApworldSearchRelativeDirectories)
      {
        string directoryPath = string.IsNullOrWhiteSpace(relativeDirectory)
          ? rootPath
          : Path.Combine(rootPath, relativeDirectory);

        string apworldPath = FindNamedChildFile(
          directoryPath,
          ChecksMateApworldFileName,
          validationMessages,
          ArchipelagoEnvironmentValidationSeverity.Error);

        if (apworldPath != null)
        {
          return apworldPath;
        }
      }

      validationMessages.Add(new ArchipelagoEnvironmentValidationMessage(
        ArchipelagoEnvironmentValidationSeverity.Error,
        "ChecksMate world was not found. Expected worlds\\checksmate or checksmate.apworld under custom_worlds/worlds.",
        rootPath));

      return null;
    }

    private static string FindNamedChildFile(
      string directoryPath,
      string fileName,
      IList<ArchipelagoEnvironmentValidationMessage> validationMessages,
      ArchipelagoEnvironmentValidationSeverity ioIssueSeverity)
    {
      if (!TryDirectoryExists(directoryPath, validationMessages, ioIssueSeverity, out bool directoryExists) ||
        !directoryExists)
      {
        return null;
      }

      try
      {
        foreach (string candidatePath in Directory.EnumerateFiles(directoryPath, "*.apworld", SearchOption.TopDirectoryOnly))
        {
          if (string.Equals(Path.GetFileName(candidatePath), fileName, StringComparison.OrdinalIgnoreCase))
          {
            return candidatePath;
          }
        }
      }
      catch (UnauthorizedAccessException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect Archipelago world directory.", directoryPath, ex);
      }
      catch (IOException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect Archipelago world directory.", directoryPath, ex);
      }
      catch (SecurityException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect Archipelago world directory.", directoryPath, ex);
      }
      catch (ArgumentException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect Archipelago world directory.", directoryPath, ex);
      }
      catch (NotSupportedException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect Archipelago world directory.", directoryPath, ex);
      }

      return null;
    }

    private static bool TryFileExists(
      string path,
      IList<ArchipelagoEnvironmentValidationMessage> validationMessages,
      ArchipelagoEnvironmentValidationSeverity ioIssueSeverity,
      out bool fileExists)
    {
      fileExists = false;

      try
      {
        FileAttributes attributes = File.GetAttributes(path);
        fileExists = (attributes & FileAttributes.Directory) == 0;
        return true;
      }
      catch (FileNotFoundException)
      {
        return true;
      }
      catch (DirectoryNotFoundException)
      {
        return true;
      }
      catch (UnauthorizedAccessException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect file.", path, ex);
      }
      catch (IOException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect file.", path, ex);
      }
      catch (SecurityException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect file.", path, ex);
      }
      catch (ArgumentException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect file.", path, ex);
      }
      catch (NotSupportedException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect file.", path, ex);
      }

      return false;
    }

    private static bool TryDirectoryExists(
      string path,
      IList<ArchipelagoEnvironmentValidationMessage> validationMessages,
      ArchipelagoEnvironmentValidationSeverity ioIssueSeverity,
      out bool directoryExists)
    {
      directoryExists = false;

      try
      {
        FileAttributes attributes = File.GetAttributes(path);
        directoryExists = (attributes & FileAttributes.Directory) == FileAttributes.Directory;
        return true;
      }
      catch (FileNotFoundException)
      {
        return true;
      }
      catch (DirectoryNotFoundException)
      {
        return true;
      }
      catch (UnauthorizedAccessException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect directory.", path, ex);
      }
      catch (IOException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect directory.", path, ex);
      }
      catch (SecurityException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect directory.", path, ex);
      }
      catch (ArgumentException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect directory.", path, ex);
      }
      catch (NotSupportedException ex)
      {
        AddIoDiagnostic(validationMessages, ioIssueSeverity, "Unable to inspect directory.", path, ex);
      }

      return false;
    }

    private static void AddIoDiagnostic(
      IList<ArchipelagoEnvironmentValidationMessage> validationMessages,
      ArchipelagoEnvironmentValidationSeverity severity,
      string message,
      string path,
      Exception exception)
    {
      validationMessages?.Add(new ArchipelagoEnvironmentValidationMessage(
        severity,
        $"{message} {exception.GetType().Name}: {exception.Message}",
        path));
    }

    private static void AddCandidateWithAncestors(
      ICollection<string> candidates,
      string path,
      int maxAncestors)
    {
      string currentPath = NormalizeInputPath(path);

      for (int i = 0; i <= maxAncestors && !string.IsNullOrWhiteSpace(currentPath); i++)
      {
        AddCandidate(candidates, currentPath);
        currentPath = GetParentPath(currentPath);
      }
    }

    private static void AddCandidate(ICollection<string> candidates, string path)
    {
      string normalizedPath = NormalizeInputPath(path);

      if (!string.IsNullOrWhiteSpace(normalizedPath))
      {
        candidates.Add(normalizedPath);
      }
    }

    private static string CombineIfNotEmpty(string rootPath, string relativePath)
    {
      return string.IsNullOrWhiteSpace(rootPath)
        ? null
        : Path.Combine(rootPath, relativePath);
    }

    private static string GetParentPath(string path)
    {
      try
      {
        return Directory.GetParent(path)?.FullName;
      }
      catch (ArgumentException)
      {
        return null;
      }
      catch (NotSupportedException)
      {
        return null;
      }
      catch (SecurityException)
      {
        return null;
      }
    }

    private static string NormalizeInputPath(string path)
    {
      if (string.IsNullOrWhiteSpace(path))
      {
        return null;
      }

      string normalizedPath = System.Environment.ExpandEnvironmentVariables(path.Trim().Trim('"'));

      if (string.Equals(Path.GetFileName(normalizedPath), GenerateScriptFileName, StringComparison.OrdinalIgnoreCase))
      {
        normalizedPath = Path.GetDirectoryName(normalizedPath);
      }

      if (string.IsNullOrWhiteSpace(normalizedPath))
      {
        return null;
      }

      try
      {
        return Path.GetFullPath(Path.TrimEndingDirectorySeparator(normalizedPath));
      }
      catch (ArgumentException)
      {
        return normalizedPath;
      }
      catch (NotSupportedException)
      {
        return normalizedPath;
      }
      catch (SecurityException)
      {
        return normalizedPath;
      }
    }
  }
}
