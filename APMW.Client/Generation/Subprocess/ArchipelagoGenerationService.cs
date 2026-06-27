using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archipelago.APChessV.Generation.Environment;
using Archipelago.APChessV.Generation.Settings;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public sealed class ArchipelagoGenerationService
  {
    private const string PlayerFilesDirectoryName = "player-files";
    private const string DefaultPlayerFileName = "ChecksMate.yaml";

    private readonly IArchipelagoProcessRunner processRunner;

    public ArchipelagoGenerationService()
      : this(new ArchipelagoProcessRunner())
    {
    }

    public ArchipelagoGenerationService(IArchipelagoProcessRunner processRunner)
    {
      this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
    }

    public async Task<ArchipelagoGenerationResult> GenerateAsync(
      ArchipelagoGenerationRequest request,
      CancellationToken cancellationToken = default)
    {
      if (request == null)
      {
        throw new ArgumentNullException(nameof(request));
      }

      var validationErrors = GetValidationErrors(request);
      if (validationErrors.Count > 0)
      {
        return CreateValidationFailure(request.OutputDirectoryPath, validationErrors);
      }

      string playerFilesDirectoryPath = null;
      string playerFilePath = null;
      ArchipelagoGenerationCommand command = null;

      try
      {
        cancellationToken.ThrowIfCancellationRequested();

        string runDirectoryPath = Path.Combine(
          request.TempDirectoryPath,
          "archipelago-generation-" + Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture));
        playerFilesDirectoryPath = Path.Combine(runDirectoryPath, PlayerFilesDirectoryName);
        playerFilePath = Path.Combine(playerFilesDirectoryPath, GetPlayerFileName(request.PlayerFileName));

        Directory.CreateDirectory(playerFilesDirectoryPath);
        Directory.CreateDirectory(request.OutputDirectoryPath);
        ChecksMateGenerationSettingsYamlSerializer.Save(playerFilePath, request.Settings);

        command = BuildCommand(
          request.Environment,
          playerFilesDirectoryPath,
          request.OutputDirectoryPath);

        ArchipelagoProcessResult processResult = await processRunner.RunAsync(
          command,
          cancellationToken).ConfigureAwait(false);

        IReadOnlyList<string> artifactPaths = CollectArtifactPaths(request.OutputDirectoryPath);
        bool success = processResult.ExitCode == 0;
        string failureMessage = success
          ? null
          : "Archipelago generation failed with exit code " +
            processResult.ExitCode.ToString(CultureInfo.InvariantCulture) + ".";

        return new ArchipelagoGenerationResult(
          success,
          wasCancelled: false,
          exitCode: processResult.ExitCode,
          playerFilesDirectoryPath: playerFilesDirectoryPath,
          playerFilePath: playerFilePath,
          outputDirectoryPath: request.OutputDirectoryPath,
          command: command,
          artifactPaths: artifactPaths,
          standardOutputLines: processResult.StandardOutputLines,
          standardErrorLines: processResult.StandardErrorLines,
          logLines: BuildLogLines(command, processResult, artifactPaths, failureMessage),
          validationErrors: Array.Empty<string>(),
          failureMessage: failureMessage);
      }
      catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
      {
        string failureMessage = "Archipelago generation was cancelled.";
        return new ArchipelagoGenerationResult(
          success: false,
          wasCancelled: true,
          exitCode: null,
          playerFilesDirectoryPath: playerFilesDirectoryPath,
          playerFilePath: playerFilePath,
          outputDirectoryPath: request.OutputDirectoryPath,
          command: command,
          artifactPaths: CollectArtifactPaths(request.OutputDirectoryPath),
          standardOutputLines: Array.Empty<string>(),
          standardErrorLines: Array.Empty<string>(),
          logLines: new[] { failureMessage },
          validationErrors: Array.Empty<string>(),
          failureMessage: failureMessage);
      }
      catch (Exception ex) when (
        ex is IOException ||
        ex is UnauthorizedAccessException ||
        ex is System.Security.SecurityException ||
        ex is NotSupportedException ||
        ex is ArgumentException ||
        ex is InvalidOperationException)
      {
        string failureMessage = "Archipelago generation failed before completion: " +
          ex.GetType().Name + ": " + ex.Message;
        return new ArchipelagoGenerationResult(
          success: false,
          wasCancelled: false,
          exitCode: null,
          playerFilesDirectoryPath: playerFilesDirectoryPath,
          playerFilePath: playerFilePath,
          outputDirectoryPath: request.OutputDirectoryPath,
          command: command,
          artifactPaths: CollectArtifactPaths(request.OutputDirectoryPath),
          standardOutputLines: Array.Empty<string>(),
          standardErrorLines: new[] { failureMessage },
          logLines: new[] { failureMessage },
          validationErrors: Array.Empty<string>(),
          failureMessage: failureMessage);
      }
    }

    public static ArchipelagoGenerationCommand BuildCommand(
      ArchipelagoEnvironmentInfo environment,
      string playerFilesDirectoryPath,
      string outputDirectoryPath)
    {
      if (environment == null)
      {
        throw new ArgumentNullException(nameof(environment));
      }

      if (string.IsNullOrWhiteSpace(playerFilesDirectoryPath))
      {
        throw new ArgumentException("Player files directory path is required.", nameof(playerFilesDirectoryPath));
      }

      if (string.IsNullOrWhiteSpace(outputDirectoryPath))
      {
        throw new ArgumentException("Output directory path is required.", nameof(outputDirectoryPath));
      }

      return new ArchipelagoGenerationCommand(
        environment.PythonExecutablePath,
        new[]
        {
          environment.GenerateScriptPath,
          "--player_files_path",
          playerFilesDirectoryPath,
          "--outputpath",
          outputDirectoryPath,
        },
        environment.RootPath);
    }

    private static List<string> GetValidationErrors(ArchipelagoGenerationRequest request)
    {
      var errors = new List<string>();

      if (request.Settings == null)
      {
        errors.Add("ChecksMate generation settings are required.");
      }
      else
      {
        foreach (ChecksMateGenerationValidationError settingsError in request.Settings.Validate())
        {
          errors.Add("Settings." + settingsError.PropertyName + ": " + settingsError.Message);
        }
      }

      if (request.Environment == null)
      {
        errors.Add("Archipelago environment is required.");
      }
      else
      {
        if (!request.Environment.HasRootPath)
        {
          errors.Add("Archipelago environment root path is required.");
        }

        if (!request.Environment.HasPythonExecutable)
        {
          errors.Add("Archipelago Python executable path is required.");
        }

        if (!request.Environment.HasGenerateScript)
        {
          errors.Add("Archipelago Generate.py path is required.");
        }

        if (!request.Environment.HasChecksMateWorld)
        {
          errors.Add("ChecksMate world must be present in the Archipelago environment.");
        }

        foreach (ArchipelagoEnvironmentValidationMessage environmentError in
          request.Environment.ValidationMessages.Where(message => message.Severity == ArchipelagoEnvironmentValidationSeverity.Error))
        {
          errors.Add(environmentError.ToString());
        }
      }

      if (string.IsNullOrWhiteSpace(request.TempDirectoryPath))
      {
        errors.Add("Temporary directory path is required.");
      }

      if (string.IsNullOrWhiteSpace(request.OutputDirectoryPath))
      {
        errors.Add("Output directory path is required.");
      }

      return errors;
    }

    private static ArchipelagoGenerationResult CreateValidationFailure(
      string outputDirectoryPath,
      IReadOnlyList<string> validationErrors)
    {
      string failureMessage = "Archipelago generation request is invalid.";
      var logLines = new List<string> { failureMessage };
      logLines.AddRange(validationErrors.Select(error => "validation: " + error));

      return new ArchipelagoGenerationResult(
        success: false,
        wasCancelled: false,
        exitCode: null,
        playerFilesDirectoryPath: null,
        playerFilePath: null,
        outputDirectoryPath: outputDirectoryPath,
        command: null,
        artifactPaths: Array.Empty<string>(),
        standardOutputLines: Array.Empty<string>(),
        standardErrorLines: Array.Empty<string>(),
        logLines: logLines,
        validationErrors: validationErrors,
        failureMessage: failureMessage);
    }

    private static IReadOnlyList<string> CollectArtifactPaths(string outputDirectoryPath)
    {
      if (string.IsNullOrWhiteSpace(outputDirectoryPath) || !Directory.Exists(outputDirectoryPath))
      {
        return Array.Empty<string>();
      }

      return Directory
        .EnumerateFiles(outputDirectoryPath, "*", SearchOption.AllDirectories)
        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
        .ToList();
    }

    private static IReadOnlyList<string> BuildLogLines(
      ArchipelagoGenerationCommand command,
      ArchipelagoProcessResult processResult,
      IReadOnlyList<string> artifactPaths,
      string failureMessage)
    {
      var logLines = new List<string>
      {
        "Archipelago generation command: " + FormatCommand(command),
      };

      foreach (string line in processResult.StandardOutputLines)
      {
        logLines.Add("stdout: " + line);
      }

      foreach (string line in processResult.StandardErrorLines)
      {
        logLines.Add("stderr: " + line);
      }

      logLines.Add("Exit code: " + processResult.ExitCode.ToString(CultureInfo.InvariantCulture));

      foreach (string artifactPath in artifactPaths)
      {
        logLines.Add("artifact: " + artifactPath);
      }

      if (!string.IsNullOrWhiteSpace(failureMessage))
      {
        logLines.Add(failureMessage);
      }

      return logLines;
    }

    private static string FormatCommand(ArchipelagoGenerationCommand command)
    {
      if (command == null)
      {
        return string.Empty;
      }

      return QuoteArgument(command.ExecutablePath) + " " +
        string.Join(" ", command.Arguments.Select(QuoteArgument));
    }

    private static string QuoteArgument(string argument)
    {
      if (argument == null)
      {
        return "\"\"";
      }

      return "\"" + argument.Replace("\"", "\\\"") + "\"";
    }

    private static string GetPlayerFileName(string requestedPlayerFileName)
    {
      string fileName = string.IsNullOrWhiteSpace(requestedPlayerFileName)
        ? DefaultPlayerFileName
        : Path.GetFileName(requestedPlayerFileName.Trim());

      return string.IsNullOrWhiteSpace(fileName)
        ? DefaultPlayerFileName
        : fileName;
    }
  }
}
