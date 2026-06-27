using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archipelago.APChessV.Generation.Environment;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public sealed class ArchipelagoServerService
  {
    private readonly IArchipelagoServerProcessRunner processRunner;

    public ArchipelagoServerService()
      : this(new ArchipelagoServerProcessRunner())
    {
    }

    public ArchipelagoServerService(IArchipelagoServerProcessRunner processRunner)
    {
      this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
    }

    public async Task<ArchipelagoServerLaunchResult> LaunchAsync(
      ArchipelagoServerLaunchRequest request,
      CancellationToken cancellationToken = default)
    {
      if (request == null)
      {
        throw new ArgumentNullException(nameof(request));
      }

      string host = NormalizeHost(request.Host);
      int port = request.Port;
      var validationErrors = GetValidationErrors(request, host, port);
      if (validationErrors.Count > 0)
      {
        return CreateValidationFailure(request, host, port, validationErrors);
      }

      ArchipelagoGenerationCommand command = null;
      IArchipelagoServerProcess serverProcess = null;

      try
      {
        cancellationToken.ThrowIfCancellationRequested();
        command = BuildCommand(request.Environment, request.ArtifactPath, host, port);

        serverProcess = await processRunner.StartAsync(
          command,
          cancellationToken).ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);

        if (serverProcess.HasExited)
        {
          string failureMessage = "Archipelago server exited immediately" +
            (serverProcess.ExitCode.HasValue
              ? " with exit code " + serverProcess.ExitCode.Value.ToString(CultureInfo.InvariantCulture)
              : string.Empty) +
            ".";
          var logLines = new List<string>
          {
            "Archipelago server command: " + FormatCommand(command),
            failureMessage,
          };
          logLines.AddRange(serverProcess.StandardOutputLines.Select(line => "stdout: " + line));
          logLines.AddRange(serverProcess.StandardErrorLines.Select(line => "stderr: " + line));
          serverProcess.Dispose();
          serverProcess = null;

          return new ArchipelagoServerLaunchResult(
            success: false,
            wasCancelled: false,
            host: host,
            port: port,
            artifactPath: request.ArtifactPath,
            command: command,
            serverProcess: null,
            validationErrors: Array.Empty<string>(),
            logLines: logLines,
            failureMessage: failureMessage);
        }

        string logLine = "Archipelago server launched at " +
          host + ":" + port.ToString(CultureInfo.InvariantCulture) +
          " using artifact: " + request.ArtifactPath;

        return new ArchipelagoServerLaunchResult(
          success: true,
          wasCancelled: false,
          host: host,
          port: port,
          artifactPath: request.ArtifactPath,
          command: command,
          serverProcess: serverProcess,
          validationErrors: Array.Empty<string>(),
          logLines: new[]
          {
            "Archipelago server command: " + FormatCommand(command),
            logLine,
          },
          failureMessage: null);
      }
      catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
      {
        if (serverProcess != null)
        {
          await serverProcess.StopAsync(CancellationToken.None).ConfigureAwait(false);
          serverProcess.Dispose();
        }

        string failureMessage = "Archipelago server launch was cancelled.";
        return new ArchipelagoServerLaunchResult(
          success: false,
          wasCancelled: true,
          host: host,
          port: port,
          artifactPath: request.ArtifactPath,
          command: command,
          serverProcess: null,
          validationErrors: Array.Empty<string>(),
          logLines: new[] { failureMessage },
          failureMessage: failureMessage);
      }
      catch (Exception ex) when (
        ex is IOException ||
        ex is UnauthorizedAccessException ||
        ex is System.Security.SecurityException ||
        ex is NotSupportedException ||
        ex is ArgumentException ||
        ex is InvalidOperationException ||
        ex is Win32Exception)
      {
        string cleanupFailure = null;
        if (serverProcess != null)
        {
          try
          {
            await serverProcess.StopAsync(CancellationToken.None).ConfigureAwait(false);
            serverProcess.Dispose();
          }
          catch (Exception stopException) when (
            stopException is IOException ||
            stopException is UnauthorizedAccessException ||
            stopException is System.Security.SecurityException ||
            stopException is NotSupportedException ||
            stopException is InvalidOperationException ||
            stopException is Win32Exception)
          {
            cleanupFailure = "Additionally, the partially launched server process could not be stopped: " +
              stopException.GetType().Name + ": " + stopException.Message;
          }
        }

        string failureMessage = "Archipelago server launch failed: " +
          ex.GetType().Name + ": " + ex.Message;
        var logLines = new List<string> { failureMessage };
        if (cleanupFailure != null)
        {
          logLines.Add(cleanupFailure);
        }

        return new ArchipelagoServerLaunchResult(
          success: false,
          wasCancelled: false,
          host: host,
          port: port,
          artifactPath: request.ArtifactPath,
          command: command,
          serverProcess: null,
          validationErrors: Array.Empty<string>(),
          logLines: logLines,
          failureMessage: failureMessage);
      }
    }

    public static ArchipelagoGenerationCommand BuildCommand(
      ArchipelagoEnvironmentInfo environment,
      string artifactPath,
      string host = ArchipelagoServerLaunchRequest.DefaultHost,
      int port = ArchipelagoServerLaunchRequest.DefaultPort)
    {
      if (environment == null)
      {
        throw new ArgumentNullException(nameof(environment));
      }

      if (string.IsNullOrWhiteSpace(environment.RootPath))
      {
        throw new ArgumentException("Archipelago environment root directory is required.", nameof(environment));
      }

      if (string.IsNullOrWhiteSpace(environment.PythonExecutablePath))
      {
        throw new ArgumentException("Archipelago Python executable path is required.", nameof(environment));
      }

      if (string.IsNullOrWhiteSpace(environment.MultiServerScriptPath))
      {
        throw new ArgumentException("Archipelago MultiServer.py path is required.", nameof(environment));
      }

      if (string.IsNullOrWhiteSpace(artifactPath))
      {
        throw new ArgumentException("Archipelago artifact path is required.", nameof(artifactPath));
      }

      string normalizedHost = NormalizeHost(host);
      if (string.IsNullOrWhiteSpace(normalizedHost))
      {
        throw new ArgumentException("Archipelago server host is required.", nameof(host));
      }

      if (!IsValidPort(port))
      {
        throw new ArgumentOutOfRangeException(nameof(port), "Archipelago server port must be between 1 and 65535.");
      }

      return new ArchipelagoGenerationCommand(
        environment.PythonExecutablePath,
        new[]
        {
          environment.MultiServerScriptPath,
          artifactPath,
          "--host",
          normalizedHost,
          "--port",
          port.ToString(CultureInfo.InvariantCulture),
        },
        environment.RootPath);
    }

    private static List<string> GetValidationErrors(
      ArchipelagoServerLaunchRequest request,
      string host,
      int port)
    {
      var errors = new List<string>();

      if (request.Environment == null)
      {
        errors.Add("Archipelago environment is required.");
      }
      else
      {
        AddDirectoryValidation(errors, request.Environment.RootPath, "Archipelago environment root directory");
        AddFileValidation(errors, request.Environment.PythonExecutablePath, "Archipelago Python executable");
        AddFileValidation(errors, request.Environment.MultiServerScriptPath, "Archipelago MultiServer.py");
      }

      AddFileValidation(errors, request.ArtifactPath, "Generated .archipelago artifact");

      if (!string.IsNullOrWhiteSpace(request.ArtifactPath) &&
        !string.Equals(Path.GetExtension(request.ArtifactPath), ".archipelago", StringComparison.OrdinalIgnoreCase))
      {
        errors.Add("Generated artifact must use the .archipelago extension: " + request.ArtifactPath);
      }

      if (string.IsNullOrWhiteSpace(host))
      {
        errors.Add("Archipelago server host is required.");
      }

      if (!IsValidPort(port))
      {
        errors.Add("Archipelago server port must be between 1 and 65535.");
      }

      return errors;
    }

    private static void AddFileValidation(ICollection<string> errors, string path, string description)
    {
      if (string.IsNullOrWhiteSpace(path))
      {
        errors.Add(description + " path is required.");
        return;
      }

      try
      {
        FileAttributes attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
        {
          errors.Add(description + " path points to a directory: " + path);
        }
      }
      catch (FileNotFoundException)
      {
        errors.Add(description + " was not found: " + path);
      }
      catch (DirectoryNotFoundException)
      {
        errors.Add(description + " was not found: " + path);
      }
      catch (UnauthorizedAccessException ex)
      {
        errors.Add(description + " could not be inspected: " + ex.Message);
      }
      catch (IOException ex)
      {
        errors.Add(description + " could not be inspected: " + ex.Message);
      }
      catch (System.Security.SecurityException ex)
      {
        errors.Add(description + " could not be inspected: " + ex.Message);
      }
      catch (ArgumentException ex)
      {
        errors.Add(description + " path is invalid: " + ex.Message);
      }
      catch (NotSupportedException ex)
      {
        errors.Add(description + " path is invalid: " + ex.Message);
      }
    }

    private static void AddDirectoryValidation(ICollection<string> errors, string path, string description)
    {
      if (string.IsNullOrWhiteSpace(path))
      {
        errors.Add(description + " path is required.");
        return;
      }

      try
      {
        FileAttributes attributes = File.GetAttributes(path);
        if ((attributes & FileAttributes.Directory) != FileAttributes.Directory)
        {
          errors.Add(description + " path points to a file: " + path);
        }
      }
      catch (FileNotFoundException)
      {
        errors.Add(description + " was not found: " + path);
      }
      catch (DirectoryNotFoundException)
      {
        errors.Add(description + " was not found: " + path);
      }
      catch (UnauthorizedAccessException ex)
      {
        errors.Add(description + " could not be inspected: " + ex.Message);
      }
      catch (IOException ex)
      {
        errors.Add(description + " could not be inspected: " + ex.Message);
      }
      catch (System.Security.SecurityException ex)
      {
        errors.Add(description + " could not be inspected: " + ex.Message);
      }
      catch (ArgumentException ex)
      {
        errors.Add(description + " path is invalid: " + ex.Message);
      }
      catch (NotSupportedException ex)
      {
        errors.Add(description + " path is invalid: " + ex.Message);
      }
    }

    private static ArchipelagoServerLaunchResult CreateValidationFailure(
      ArchipelagoServerLaunchRequest request,
      string host,
      int port,
      IReadOnlyList<string> validationErrors)
    {
      string failureMessage = "Archipelago server launch request is invalid.";
      var logLines = new List<string> { failureMessage };
      logLines.AddRange(validationErrors.Select(error => "validation: " + error));

      return new ArchipelagoServerLaunchResult(
        success: false,
        wasCancelled: false,
        host: host,
        port: port,
        artifactPath: request.ArtifactPath,
        command: null,
        serverProcess: null,
        validationErrors: validationErrors,
        logLines: logLines,
        failureMessage: failureMessage);
    }

    private static bool IsValidPort(int port)
    {
      return port >= 1 && port <= 65535;
    }

    private static string NormalizeHost(string host)
    {
      return string.IsNullOrWhiteSpace(host)
        ? host
        : host.Trim();
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
  }
}
