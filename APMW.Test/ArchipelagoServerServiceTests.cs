using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archipelago.APChessV.Generation.Environment;
using Archipelago.APChessV.Generation.Settings;
using Archipelago.APChessV.Generation.Subprocess;

namespace Archipelago.APChessV
{
  [TestClass]
  public class ArchipelagoServerServiceTests
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
    public async Task LaunchAsync_ConstructsPythonMultiServerCommandAndReturnsConnectionDetails()
    {
      string rootPath = CreateSandbox("archipelago");
      ArchipelagoEnvironmentInfo environment = CreateEnvironment(rootPath, includeMultiServer: true);
      string artifactPath = CreateArtifact(rootPath);
      var process = new FakeServerProcess();
      var processRunner = new FakeServerProcessRunner { ProcessToReturn = process };
      var service = new ArchipelagoServerService(processRunner);

      ArchipelagoServerLaunchResult result = await service.LaunchAsync(
        new ArchipelagoServerLaunchRequest(environment, artifactPath));

      Assert.IsTrue(result.Success, FormatResult(result));
      Assert.AreEqual(1, processRunner.StartCount);
      Assert.AreEqual(environment.PythonExecutablePath, processRunner.LastCommand.ExecutablePath);
      Assert.AreEqual(environment.RootPath, processRunner.LastCommand.WorkingDirectory);
      CollectionAssert.AreEqual(
        new[]
        {
          environment.MultiServerScriptPath,
          artifactPath,
          "--host",
          ArchipelagoServerLaunchRequest.DefaultHost,
          "--port",
          ArchipelagoServerLaunchRequest.DefaultPort.ToString(),
        },
        processRunner.LastCommand.Arguments.ToArray());
      Assert.AreEqual(ArchipelagoServerLaunchRequest.DefaultHost, result.Host);
      Assert.AreEqual(ArchipelagoServerLaunchRequest.DefaultPort, result.Port);
      Assert.AreEqual("localhost:38281", result.ConnectionAddress);
      Assert.AreSame(process, result.ServerProcess);
      Assert.AreEqual(process.ProcessId, result.ProcessId);
      Assert.AreEqual(ArchipelagoServerProcessStatus.Running, result.Status);
    }

    [TestMethod]
    public async Task LaunchAsync_WithCustomHostAndPort_UsesRequestedConnectionDetails()
    {
      string rootPath = CreateSandbox("archipelago-custom");
      ArchipelagoEnvironmentInfo environment = CreateEnvironment(rootPath, includeMultiServer: true);
      string artifactPath = CreateArtifact(rootPath);
      var processRunner = new FakeServerProcessRunner();
      var service = new ArchipelagoServerService(processRunner);
      var request = new ArchipelagoServerLaunchRequest(environment, artifactPath)
      {
        Host = "127.0.0.1",
        Port = 45678,
      };

      ArchipelagoServerLaunchResult result = await service.LaunchAsync(request);

      Assert.IsTrue(result.Success, FormatResult(result));
      Assert.AreEqual("127.0.0.1", result.Host);
      Assert.AreEqual(45678, result.Port);
      Assert.AreEqual("127.0.0.1:45678", result.ConnectionAddress);
      CollectionAssert.AreEqual(
        new[]
        {
          environment.MultiServerScriptPath,
          artifactPath,
          "--host",
          "127.0.0.1",
          "--port",
          "45678",
        },
        processRunner.LastCommand.Arguments.ToArray());
    }

    [TestMethod]
    public async Task LaunchAsync_WithMissingArtifact_ReturnsValidationFailureWithoutStartingProcess()
    {
      string rootPath = CreateSandbox("archipelago-missing-artifact");
      ArchipelagoEnvironmentInfo environment = CreateEnvironment(rootPath, includeMultiServer: true);
      string missingArtifactPath = Path.Combine(rootPath, "missing.archipelago");
      var processRunner = new FakeServerProcessRunner();
      var service = new ArchipelagoServerService(processRunner);

      ArchipelagoServerLaunchResult result = await service.LaunchAsync(
        new ArchipelagoServerLaunchRequest(environment, missingArtifactPath));

      Assert.IsFalse(result.Success);
      Assert.AreEqual(0, processRunner.StartCount);
      Assert.IsTrue(
        result.ValidationErrors.Any(error => error.Contains("Generated .archipelago artifact was not found")),
        FormatResult(result));
      Assert.AreEqual(ArchipelagoServerProcessStatus.NotStarted, result.Status);
    }

    [TestMethod]
    public async Task LaunchAsync_WithoutMultiServer_ReturnsValidationFailureWithoutStartingProcess()
    {
      string rootPath = CreateSandbox("archipelago-no-server");
      ArchipelagoEnvironmentInfo environment = CreateEnvironment(rootPath, includeMultiServer: false);
      string artifactPath = CreateArtifact(rootPath);
      var processRunner = new FakeServerProcessRunner();
      var service = new ArchipelagoServerService(processRunner);

      ArchipelagoServerLaunchResult result = await service.LaunchAsync(
        new ArchipelagoServerLaunchRequest(environment, artifactPath));

      Assert.IsFalse(result.Success);
      Assert.AreEqual(0, processRunner.StartCount);
      Assert.IsTrue(
        result.ValidationErrors.Any(error => error.Contains("Archipelago MultiServer.py path is required")),
        FormatResult(result));
    }

    [TestMethod]
    public async Task LaunchAsync_WhenServerExitsImmediately_ReturnsFailure()
    {
      string rootPath = CreateSandbox("archipelago-exited");
      ArchipelagoEnvironmentInfo environment = CreateEnvironment(rootPath, includeMultiServer: true);
      string artifactPath = CreateArtifact(rootPath);
      var process = new FakeServerProcess
      {
        HasExited = true,
        ExitCodeOverride = 9,
        StandardError = new[] { "server failed" },
      };
      var processRunner = new FakeServerProcessRunner { ProcessToReturn = process };
      var service = new ArchipelagoServerService(processRunner);

      ArchipelagoServerLaunchResult result = await service.LaunchAsync(
        new ArchipelagoServerLaunchRequest(environment, artifactPath));

      Assert.IsFalse(result.Success);
      StringAssert.Contains(result.FailureMessage, "exited immediately");
      StringAssert.Contains(result.FailureMessage, "9");
      Assert.IsTrue(process.DisposeCalled);
      Assert.IsTrue(result.LogLines.Any(line => line.Contains("server failed")), FormatResult(result));
    }

    [TestMethod]
    public async Task StopAsync_StopsOnlyReturnedProcessHandle()
    {
      string rootPath = CreateSandbox("archipelago-stop");
      ArchipelagoEnvironmentInfo environment = CreateEnvironment(rootPath, includeMultiServer: true);
      string artifactPath = CreateArtifact(rootPath);
      var process = new FakeServerProcess();
      var processRunner = new FakeServerProcessRunner { ProcessToReturn = process };
      var service = new ArchipelagoServerService(processRunner);

      ArchipelagoServerLaunchResult result = await service.LaunchAsync(
        new ArchipelagoServerLaunchRequest(environment, artifactPath));
      await result.ServerProcess.StopAsync();

      Assert.IsTrue(process.StopCalled);
      Assert.IsTrue(result.ServerProcess.HasExited);
      Assert.AreEqual(ArchipelagoServerProcessStatus.Exited, result.Status);
    }

    [TestMethod]
    public async Task GenerateAsync_WithArchipelagoArtifact_ExposesPrimaryArchipelagoArtifact()
    {
      string rootPath = CreateSandbox("generation-archipelago-artifact");
      ArchipelagoEnvironmentInfo environment = CreateEnvironment(rootPath, includeMultiServer: true);
      string outputPath = CreateSandbox("generation-output");
      var processRunner = new FakeGenerationProcessRunner
      {
        OnRunAsync = (command, _) =>
        {
          string commandOutputPath = GetArgumentValue(command, "--outputpath");
          File.WriteAllText(Path.Combine(commandOutputPath, "AP_ChecksMate.archipelago"), "generated");
          File.WriteAllText(Path.Combine(commandOutputPath, "spoiler.txt"), "spoiler");
          return Task.FromResult(new ArchipelagoProcessResult(0, Array.Empty<string>(), Array.Empty<string>()));
        },
      };
      var service = new ArchipelagoGenerationService(processRunner);

      ArchipelagoGenerationResult result = await service.GenerateAsync(new ArchipelagoGenerationRequest(
        new ChecksMateGenerationSettings(),
        environment,
        CreateSandbox("generation-temp"),
        outputPath));

      Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.LogLines));
      Assert.AreEqual(1, result.ArchipelagoArtifactPaths.Count);
      StringAssert.EndsWith(result.PrimaryArchipelagoArtifactPath, "AP_ChecksMate.archipelago");
    }

    private string CreateSandbox(string name)
    {
      string baseDirectory = Path.Combine(
        Environment.CurrentDirectory,
        "ArchipelagoServerServiceTestData");
      string path = Path.Combine(
        baseDirectory,
        $"{name}-{Guid.NewGuid():N}");

      Directory.CreateDirectory(path);
      createdDirectories.Add(path);
      return path;
    }

    private static ArchipelagoEnvironmentInfo CreateEnvironment(string rootPath, bool includeMultiServer)
    {
      string pythonPath = Path.Combine(rootPath, "python.exe");
      string generatePath = Path.Combine(rootPath, "Generate.py");
      string multiServerPath = Path.Combine(rootPath, "MultiServer.py");
      string checksMateWorldPath = Path.Combine(rootPath, "worlds", "checksmate", "__init__.py");

      Directory.CreateDirectory(Path.GetDirectoryName(checksMateWorldPath));
      File.WriteAllText(pythonPath, string.Empty);
      File.WriteAllText(generatePath, string.Empty);
      File.WriteAllText(checksMateWorldPath, string.Empty);

      if (includeMultiServer)
      {
        File.WriteAllText(multiServerPath, string.Empty);
      }

      return new ArchipelagoEnvironmentInfo(
        rootPath,
        pythonPath,
        generatePath,
        includeMultiServer ? multiServerPath : null,
        checksMateWorldPath,
        Array.Empty<ArchipelagoEnvironmentValidationMessage>());
    }

    private static string CreateArtifact(string rootPath)
    {
      string artifactPath = Path.Combine(rootPath, "AP_ChecksMate.archipelago");
      File.WriteAllText(artifactPath, "generated");
      return artifactPath;
    }

    private static string GetArgumentValue(ArchipelagoGenerationCommand command, string argumentName)
    {
      for (int i = 0; i < command.Arguments.Count - 1; i++)
      {
        if (string.Equals(command.Arguments[i], argumentName, StringComparison.Ordinal))
        {
          return command.Arguments[i + 1];
        }
      }

      throw new InvalidOperationException("Argument was not found: " + argumentName);
    }

    private static string FormatResult(ArchipelagoServerLaunchResult result)
    {
      return string.Join(Environment.NewLine, result.LogLines.Concat(result.ValidationErrors));
    }

    private sealed class FakeServerProcessRunner : IArchipelagoServerProcessRunner
    {
      public int StartCount { get; private set; }

      public ArchipelagoGenerationCommand LastCommand { get; private set; }

      public IArchipelagoServerProcess ProcessToReturn { get; set; } = new FakeServerProcess();

      public Task<IArchipelagoServerProcess> StartAsync(
        ArchipelagoGenerationCommand command,
        CancellationToken cancellationToken)
      {
        StartCount++;
        LastCommand = command;
        return Task.FromResult(ProcessToReturn);
      }
    }

    private sealed class FakeServerProcess : IArchipelagoServerProcess
    {
      public int? ProcessId { get; } = 12345;

      public bool HasExited { get; set; }

      public int? ExitCodeOverride { get; set; }

      public int? ExitCode => HasExited ? ExitCodeOverride ?? 0 : null;

      public ArchipelagoServerProcessStatus Status =>
        HasExited ? ArchipelagoServerProcessStatus.Exited : ArchipelagoServerProcessStatus.Running;

      public IReadOnlyList<string> StandardOutput { get; set; } = Array.Empty<string>();

      public IReadOnlyList<string> StandardOutputLines => StandardOutput;

      public IReadOnlyList<string> StandardError { get; set; } = Array.Empty<string>();

      public IReadOnlyList<string> StandardErrorLines => StandardError;

      public bool StopCalled { get; private set; }

      public bool DisposeCalled { get; private set; }

      public Task WaitForExitAsync(CancellationToken cancellationToken = default)
      {
        HasExited = true;
        return Task.CompletedTask;
      }

      public Task StopAsync(CancellationToken cancellationToken = default)
      {
        StopCalled = true;
        HasExited = true;
        return Task.CompletedTask;
      }

      public void Dispose()
      {
        DisposeCalled = true;
      }
    }

    private sealed class FakeGenerationProcessRunner : IArchipelagoProcessRunner
    {
      public Func<ArchipelagoGenerationCommand, CancellationToken, Task<ArchipelagoProcessResult>> OnRunAsync { get; set; }

      public Task<ArchipelagoProcessResult> RunAsync(
        ArchipelagoGenerationCommand command,
        CancellationToken cancellationToken)
      {
        return OnRunAsync(command, cancellationToken);
      }
    }
  }
}
