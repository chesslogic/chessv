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
  public class ArchipelagoGenerationServiceTests
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
    public async Task GenerateAsync_ConstructsPythonGenerateCommandAndSavesPlayerYaml()
    {
      string rootPath = CreateSandbox("archipelago");
      ArchipelagoEnvironmentInfo environment = CreateValidEnvironment(rootPath);
      string tempPath = CreateSandbox("temp");
      string outputPath = CreateSandbox("output");
      var processRunner = new FakeProcessRunner();
      var service = new ArchipelagoGenerationService(processRunner);

      ArchipelagoGenerationResult result = await service.GenerateAsync(new ArchipelagoGenerationRequest(
        new ChecksMateGenerationSettings(),
        environment,
        tempPath,
        outputPath));

      Assert.IsTrue(result.Success, FormatResult(result));
      Assert.AreEqual(1, processRunner.RunCount);
      Assert.AreEqual(environment.PythonExecutablePath, processRunner.LastCommand.ExecutablePath);
      Assert.AreEqual(environment.RootPath, processRunner.LastCommand.WorkingDirectory);
      CollectionAssert.AreEqual(
        new[]
        {
          environment.GenerateScriptPath,
          "--player_files_path",
          result.PlayerFilesDirectoryPath,
          "--outputpath",
          outputPath,
        },
        processRunner.LastCommand.Arguments.ToArray());
      Assert.IsTrue(File.Exists(result.PlayerFilePath), result.PlayerFilePath);
      StringAssert.Contains(File.ReadAllText(result.PlayerFilePath), "game: ChecksMate");
    }

    [TestMethod]
    public async Task GenerateAsync_WithInvalidEnvironment_ReturnsValidationFailureWithoutStartingProcess()
    {
      string outputPath = CreateSandbox("output");
      var environment = new ArchipelagoEnvironmentInfo(
        rootPath: null,
        pythonExecutablePath: null,
        generateScriptPath: null,
        multiServerScriptPath: null,
        checksMateWorldPath: null,
        validationMessages: new[]
        {
          new ArchipelagoEnvironmentValidationMessage(
            ArchipelagoEnvironmentValidationSeverity.Error,
            "No valid Archipelago environment was found."),
        });
      var processRunner = new FakeProcessRunner();
      var service = new ArchipelagoGenerationService(processRunner);

      ArchipelagoGenerationResult result = await service.GenerateAsync(new ArchipelagoGenerationRequest(
        new ChecksMateGenerationSettings(),
        environment,
        CreateSandbox("temp"),
        outputPath));

      Assert.IsFalse(result.Success);
      Assert.AreEqual(0, processRunner.RunCount);
      Assert.IsTrue(result.ValidationErrors.Any(error => error.Contains("Python executable")), FormatResult(result));
      Assert.IsTrue(result.ValidationErrors.Any(error => error.Contains("No valid Archipelago environment")), FormatResult(result));
    }

    [TestMethod]
    public async Task GenerateAsync_WithInvalidSettings_ReturnsValidationFailureWithoutStartingProcess()
    {
      string rootPath = CreateSandbox("archipelago");
      ArchipelagoEnvironmentInfo environment = CreateValidEnvironment(rootPath);
      string outputPath = Path.Combine(CreateSandbox("output-parent"), "output");
      var settings = new ChecksMateGenerationSettings
      {
        ProgressionBalancing = 101,
      };
      var processRunner = new FakeProcessRunner();
      var service = new ArchipelagoGenerationService(processRunner);

      ArchipelagoGenerationResult result = await service.GenerateAsync(new ArchipelagoGenerationRequest(
        settings,
        environment,
        CreateSandbox("temp"),
        outputPath));

      Assert.IsFalse(result.Success);
      Assert.AreEqual(0, processRunner.RunCount);
      Assert.IsNull(result.Command);
      Assert.IsFalse(Directory.Exists(outputPath), outputPath);
      Assert.IsTrue(
        result.ValidationErrors.Any(error => error.Contains("Settings.ProgressionBalancing")),
        FormatResult(result));
    }

    [TestMethod]
    public async Task GenerateAsync_WithSuccessfulProcess_CapturesOutputAndArtifacts()
    {
      string rootPath = CreateSandbox("archipelago");
      ArchipelagoEnvironmentInfo environment = CreateValidEnvironment(rootPath);
      string outputPath = CreateSandbox("output");
      var processRunner = new FakeProcessRunner
      {
        OnRunAsync = (command, _) =>
        {
          string commandOutputPath = GetArgumentValue(command, "--outputpath");
          File.WriteAllText(Path.Combine(commandOutputPath, "AP_ChecksMate.zip"), "generated");
          return Task.FromResult(new ArchipelagoProcessResult(
            0,
            new[] { "Generated ChecksMate seed." },
            new[] { "Non-fatal generator warning." }));
        },
      };
      var service = new ArchipelagoGenerationService(processRunner);

      ArchipelagoGenerationResult result = await service.GenerateAsync(new ArchipelagoGenerationRequest(
        new ChecksMateGenerationSettings(),
        environment,
        CreateSandbox("temp"),
        outputPath));

      Assert.IsTrue(result.Success, FormatResult(result));
      Assert.AreEqual(0, result.ExitCode);
      Assert.AreEqual(1, result.ArtifactPaths.Count);
      StringAssert.EndsWith(result.ArtifactPaths[0], "AP_ChecksMate.zip");
      CollectionAssert.Contains(result.StandardOutputLines.ToList(), "Generated ChecksMate seed.");
      CollectionAssert.Contains(result.StandardErrorLines.ToList(), "Non-fatal generator warning.");
      Assert.IsTrue(result.LogLines.Any(line => line.Contains("artifact:")), FormatResult(result));
    }

    [TestMethod]
    public async Task GenerateAsync_WithFailedProcess_ReturnsExitCodeAndDiagnostics()
    {
      string rootPath = CreateSandbox("archipelago");
      ArchipelagoEnvironmentInfo environment = CreateValidEnvironment(rootPath);
      var processRunner = new FakeProcessRunner
      {
        OnRunAsync = (_, __) => Task.FromResult(new ArchipelagoProcessResult(
          7,
          new[] { "Preparing player files." },
          new[] { "Generation failed: bad option." })),
      };
      var service = new ArchipelagoGenerationService(processRunner);

      ArchipelagoGenerationResult result = await service.GenerateAsync(new ArchipelagoGenerationRequest(
        new ChecksMateGenerationSettings(),
        environment,
        CreateSandbox("temp"),
        CreateSandbox("output")));

      Assert.IsFalse(result.Success);
      Assert.AreEqual(7, result.ExitCode);
      StringAssert.Contains(result.FailureMessage, "exit code 7");
      CollectionAssert.Contains(result.StandardOutputLines.ToList(), "Preparing player files.");
      CollectionAssert.Contains(result.StandardErrorLines.ToList(), "Generation failed: bad option.");
      Assert.IsTrue(result.LogLines.Any(line => line.Contains("stderr: Generation failed: bad option.")), FormatResult(result));
    }

    [TestMethod]
    public async Task GenerateAsync_WhenProcessRunnerObservesCancellation_ReturnsCancelledResult()
    {
      string rootPath = CreateSandbox("archipelago");
      ArchipelagoEnvironmentInfo environment = CreateValidEnvironment(rootPath);
      var runStarted = new TaskCompletionSource<bool>();
      var processRunner = new FakeProcessRunner
      {
        OnRunAsync = async (_, cancellationToken) =>
        {
          runStarted.TrySetResult(true);
          await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
          return new ArchipelagoProcessResult(0, Array.Empty<string>(), Array.Empty<string>());
        },
      };
      var service = new ArchipelagoGenerationService(processRunner);

      using (var cancellationTokenSource = new CancellationTokenSource())
      {
        Task<ArchipelagoGenerationResult> generateTask = service.GenerateAsync(
          new ArchipelagoGenerationRequest(
            new ChecksMateGenerationSettings(),
            environment,
            CreateSandbox("temp"),
            CreateSandbox("output")),
          cancellationTokenSource.Token);

        Task completedTask = await Task.WhenAny(runStarted.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        Assert.AreSame(runStarted.Task, completedTask, "The fake process runner was not started.");

        cancellationTokenSource.Cancel();
        ArchipelagoGenerationResult result = await generateTask;

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.WasCancelled);
        Assert.AreEqual(1, processRunner.RunCount);
        StringAssert.Contains(result.FailureMessage, "cancelled");
      }
    }

    [TestMethod]
    public async Task GenerateAsync_WhenCancelledBeforeStart_ReturnsCancelledResult()
    {
      string rootPath = CreateSandbox("archipelago");
      ArchipelagoEnvironmentInfo environment = CreateValidEnvironment(rootPath);
      var processRunner = new FakeProcessRunner();
      var service = new ArchipelagoGenerationService(processRunner);
      using (var cancellationTokenSource = new CancellationTokenSource())
      {
        cancellationTokenSource.Cancel();

        ArchipelagoGenerationResult result = await service.GenerateAsync(
          new ArchipelagoGenerationRequest(
            new ChecksMateGenerationSettings(),
            environment,
            CreateSandbox("temp"),
            CreateSandbox("output")),
          cancellationTokenSource.Token);

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.WasCancelled);
        Assert.AreEqual(0, processRunner.RunCount);
      }
    }

    private string CreateSandbox(string name)
    {
      string baseDirectory = Path.Combine(
        System.Environment.CurrentDirectory,
        "ArchipelagoGenerationServiceTestData");
      string path = Path.Combine(
        baseDirectory,
        $"{name}-{Guid.NewGuid():N}");

      Directory.CreateDirectory(path);
      createdDirectories.Add(path);
      return path;
    }

    private static ArchipelagoEnvironmentInfo CreateValidEnvironment(string rootPath)
    {
      string pythonPath = Path.Combine(rootPath, "python.exe");
      string generatePath = Path.Combine(rootPath, "Generate.py");
      string checksMateWorldPath = Path.Combine(rootPath, "worlds", "checksmate", "__init__.py");

      Directory.CreateDirectory(Path.GetDirectoryName(checksMateWorldPath));
      File.WriteAllText(pythonPath, string.Empty);
      File.WriteAllText(generatePath, string.Empty);
      File.WriteAllText(checksMateWorldPath, string.Empty);

      return new ArchipelagoEnvironmentInfo(
        rootPath,
        pythonPath,
        generatePath,
        multiServerScriptPath: null,
        checksMateWorldPath: checksMateWorldPath,
        validationMessages: Array.Empty<ArchipelagoEnvironmentValidationMessage>());
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

    private static string FormatResult(ArchipelagoGenerationResult result)
    {
      return string.Join(System.Environment.NewLine, result.LogLines.Concat(result.ValidationErrors));
    }

    private sealed class FakeProcessRunner : IArchipelagoProcessRunner
    {
      public int RunCount { get; private set; }

      public ArchipelagoGenerationCommand LastCommand { get; private set; }

      public Func<ArchipelagoGenerationCommand, CancellationToken, Task<ArchipelagoProcessResult>> OnRunAsync { get; set; }

      public Task<ArchipelagoProcessResult> RunAsync(
        ArchipelagoGenerationCommand command,
        CancellationToken cancellationToken)
      {
        RunCount++;
        LastCommand = command;

        return OnRunAsync == null
          ? Task.FromResult(new ArchipelagoProcessResult(0, Array.Empty<string>(), Array.Empty<string>()))
          : OnRunAsync(command, cancellationToken);
      }
    }
  }
}
