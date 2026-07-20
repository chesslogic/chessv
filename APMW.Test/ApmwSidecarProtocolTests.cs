using Archipelago.APChessV;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace APMW.Test
{
  [TestClass]
  public class ApmwSidecarProtocolTests
  {
    private const string ContractHash =
      "f1456e916285bf79dd4be6f4c8c6e5798ed7bb1eebd2f6e1f81075f39e8ffc15";

    [TestMethod]
    public void SerializeRequest_ProducesTheCanonicalProtocolShape()
    {
      ApmwSidecarRequest request = CreateRequest(new[] { "8x8", "10x8" });

      string json = ApmwSidecarProtocol.SerializeRequest(request);

      Assert.AreEqual(
        "{\"protocol_version\":1,\"request_id\":\"request-1\",\"contract_hash\":\"" +
        ContractHash +
        "\",\"input\":{\"itemization\":\"legacy\",\"ordering\":\"stable\",\"seeds\":{\"pocket_seed\":\"1\"," +
        "\"pawn_seed\":\"2\",\"minor_seed\":\"3\",\"major_seed\":\"4\",\"queen_seed\":\"5\"}," +
        "\"item_counts\":{\"Progressive Pawn\":8}},\"geometries\":[\"8x8\",\"10x8\"]}",
        json);
    }

    [TestMethod]
    public void Request_RejectsInvalidInputAndGeometries()
    {
      using (JsonDocument array = JsonDocument.Parse("[]"))
      using (JsonDocument objectInput = JsonDocument.Parse("{}"))
      {
        Assert.ThrowsException<ArgumentException>(() =>
          new ApmwSidecarRequest("id", ContractHash, array.RootElement, new[] { "8x8" }));
        Assert.ThrowsException<ArgumentException>(() =>
          new ApmwSidecarRequest("id", ContractHash, objectInput.RootElement, Array.Empty<string>()));
        Assert.ThrowsException<ArgumentException>(() =>
          new ApmwSidecarRequest("id", ContractHash, objectInput.RootElement, new[] { "8x8", "8x8" }));
        Assert.ThrowsException<ArgumentException>(() =>
          new ApmwSidecarRequest("id", "not-a-hash", objectInput.RootElement, new[] { "8x8" }));
      }
    }

    [TestMethod]
    public void ParseResponse_AcceptsValidResponseAndOwnsProjectionJson()
    {
      ApmwSidecarResponse response = ApmwSidecarProtocol.ParseResponse(
        SuccessJsonFor("8x8"),
        CreateRequest(new[] { "8x8" }));

      Assert.IsFalse(response.IsError);
      Assert.AreEqual(
        ApmwSidecarRequest.DefaultExpectedRuntimeSemanticVersion,
        response.Success.RuntimeSemanticVersion);
      Assert.AreEqual("8x8", response.Success.Results[0].GeometryStage);
      Assert.AreEqual(
        "8x8",
        response.Success.Results[0].Projection.GetProperty("geometry_stage").GetString());
      Assert.AreEqual(
        ContractHash,
        response.Success.Results[0].Projection.GetProperty("contract_hash").GetString());
    }

    [TestMethod]
    public void ParseResponse_AcceptsEveryCanonicalGeometryFixture()
    {
      string[] geometries = { "8x8", "10x8", "10x10", "12x10", "12x12" };

      ApmwSidecarResponse response = ApmwSidecarProtocol.ParseResponse(
        SuccessJsonFor(geometries),
        CreateRequest(geometries));

      CollectionAssert.AreEqual(
        geometries,
        response.Success.Results.Select(result => result.GeometryStage).ToArray());
    }

    [TestMethod]
    public void ParseResponse_RejectsMalformedJsonAndDuplicateProperties()
    {
      ApmwSidecarRequest request = CreateRequest(new[] { "8x8" });

      AssertProtocolFailure("{");
      AssertProtocolFailure(
        "{\"protocol_version\":1,\"protocol_version\":1,\"request_id\":\"request-1\",\"contract_hash\":\"" +
        ContractHash + "\",\"runtime_semantic_version\":\"0.1.0\",\"results\":[]}",
        request);
      AssertProtocolFailure(SuccessJson(
        "[{\"geometry_stage\":\"8x8\",\"projection\":{\"geometry_stage\":\"8x8\",\"geometry_stage\":\"8x8\"}}]"),
        request);
    }

    [TestMethod]
    public void ParseResponse_RejectsUnknownAndMissingEnvelopeFields()
    {
      ApmwSidecarRequest request = CreateRequest(new[] { "8x8" });

      AssertProtocolFailure(
        "{\"protocol_version\":1,\"request_id\":\"request-1\",\"contract_hash\":\"" + ContractHash +
        "\",\"runtime_semantic_version\":\"0.1.0\",\"results\":[],\"extra\":true}",
        request);
      AssertProtocolFailure(
        "{\"protocol_version\":1,\"request_id\":\"request-1\",\"contract_hash\":\"" + ContractHash +
        "\",\"results\":[]}",
        request);
      AssertProtocolFailure(SuccessJson("[{\"geometry_stage\":\"8x8\"}]"), request);
    }

    [TestMethod]
    public void ParseResponse_RejectsProtocolCorrelationAndVersionFailures()
    {
      ApmwSidecarRequest request = CreateRequest(new[] { "8x8" });

      AssertProtocolFailure(SuccessJson("[]").Replace("\"protocol_version\":1", "\"protocol_version\":2"), request);
      AssertProtocolFailure(SuccessJson("[]").Replace("\"request_id\":\"request-1\"", "\"request_id\":\"other\""), request);
      AssertProtocolFailure(SuccessJson("[]").Replace(ContractHash, new string('0', 64)), request);
      AssertProtocolFailure(SuccessJson("[]").Replace("\"0.1.0\"", "\"01.0.0\""), request);
      AssertProtocolFailure(SuccessJson("[]").Replace("\"0.1.0\"", "\"0.1.1\""), request);
    }

    [TestMethod]
    public void ParseResponse_RejectsMissingDuplicateAndOutOfOrderGeometries()
    {
      ApmwSidecarRequest request = CreateRequest(new[] { "8x8", "10x8" });

      AssertProtocolFailure(SuccessJson("[]"), request);
      AssertProtocolFailure(SuccessJson(
        "[{\"geometry_stage\":\"8x8\",\"projection\":{\"geometry_stage\":\"8x8\"}}," +
        "{\"geometry_stage\":\"8x8\",\"projection\":{\"geometry_stage\":\"8x8\"}}]"),
        request);
      AssertProtocolFailure(SuccessJson(
        "[{\"geometry_stage\":\"10x8\",\"projection\":{\"geometry_stage\":\"10x8\"}}," +
        "{\"geometry_stage\":\"8x8\",\"projection\":{\"geometry_stage\":\"8x8\"}}]"),
        request);
    }

    [TestMethod]
    public void ParseResponse_RejectsProjectionGeometryMismatch()
    {
      string projection = ProjectionJson("8x8");
      int propertyIndex = projection.IndexOf("\"geometry_stage\"", StringComparison.Ordinal);
      int valueIndex = projection.IndexOf("\"8x8\"", propertyIndex, StringComparison.Ordinal);
      projection = projection.Substring(0, valueIndex) + "\"10x8\"" +
        projection.Substring(valueIndex + "\"8x8\"".Length);
      AssertProtocolFailure(SuccessJson(
        "[{\"geometry_stage\":\"8x8\",\"projection\":" + projection + "}]"));
    }

    [TestMethod]
    public void ParseResponse_RejectsUnknownNestedProjectionFields()
    {
      string projection = ProjectionJson("8x8").TrimEnd();
      projection = projection.Substring(0, projection.Length - 1) + ",\"extra\":true}";
      AssertProtocolFailure(SuccessJson(
        "[{\"geometry_stage\":\"8x8\",\"projection\":" + projection + "}]"));
    }

    [TestMethod]
    public void ParseResponse_AcceptsOnlyTheExactErrorEnvelope()
    {
      ApmwSidecarResponse error = ApmwSidecarProtocol.ParseResponse(
        "{\"error\":{\"code\":\"bad_input\",\"message\":\"input was invalid\"}}",
        CreateRequest(new[] { "8x8" }));

      Assert.IsTrue(error.IsError);
      Assert.AreEqual("bad_input", error.Error.Code);
      Assert.AreEqual("input was invalid", error.Error.Message);
      AssertProtocolFailure("{\"error\":{\"code\":\"bad_input\"}}");
      AssertProtocolFailure("{\"error\":{\"code\":\"bad_input\",\"message\":\"x\"},\"results\":[]}");
      AssertProtocolFailure("{\"error\":{\"code\":1,\"message\":\"x\"}}");
      AssertProtocolFailure("{\"error\":{\"code\":\"\",\"message\":\"x\"}}");
      AssertProtocolFailure("{\"error\":{\"code\":\"bad_input\",\"message\":\"\"}}");
    }

    [TestMethod]
    public async Task RunnerBoundary_PropagatesStructuredSidecarErrors()
    {
      IApmwSidecarRunner runner = new FailingRunner();

      ApmwSidecarRemoteException exception = await Assert.ThrowsExceptionAsync<ApmwSidecarRemoteException>(
        () => runner.RunAsync(CreateRequest(new[] { "8x8" }), CancellationToken.None));

      Assert.AreEqual("bad_input", exception.Code);
      Assert.AreEqual("input was invalid", exception.RemoteMessage);
      Assert.AreEqual(19, exception.ExitCode);
      Assert.AreEqual("details", exception.StandardError);
    }

    [TestMethod]
    public void ProcessFailureTypes_ExposeTimeoutExitCodeAndStandardError()
    {
      var timeout = new ApmwSidecarTimeoutException(
        TimeSpan.FromSeconds(2),
        "timed out",
        "termination was not confirmed");
      var writeFailure = new IOException("broken pipe");
      var nonzero = new ApmwSidecarProcessException("failed", 7, "diagnostic", writeFailure);

      Assert.IsNull(timeout.ExitCode);
      Assert.AreEqual(TimeSpan.FromSeconds(2), timeout.Timeout);
      Assert.AreEqual("timed out", timeout.StandardError);
      Assert.AreEqual("termination was not confirmed", timeout.CleanupDiagnostic);
      StringAssert.Contains(timeout.Message, "cleanup: termination was not confirmed");
      Assert.AreEqual(7, nonzero.ExitCode);
      Assert.AreEqual("diagnostic", nonzero.StandardError);
      Assert.AreSame(writeFailure, nonzero.InnerException);
    }

    [TestMethod]
    public void ProcessRunner_RequiresAnExplicitPathAndBoundedTimeout()
    {
      Assert.ThrowsException<ArgumentException>(() =>
        new ApmwSidecarProcessRunner("sidecar.exe", TimeSpan.FromSeconds(1)));
      Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
        new ApmwSidecarProcessRunner(@"C:\sidecar.exe", TimeSpan.Zero));
    }

    [TestMethod]
    public async Task ProcessRunner_ValidatesExecutableAtRunTimeAndWrapsTheFailure()
    {
      string missingPath = Path.Combine(AppContext.BaseDirectory, Guid.NewGuid().ToString("N") + ".exe");
      var runner = new ApmwSidecarProcessRunner(missingPath, TimeSpan.FromSeconds(1));

      ApmwSidecarProcessException exception =
        await Assert.ThrowsExceptionAsync<ApmwSidecarProcessException>(
          () => runner.RunAsync(CreateRequest(), CancellationToken.None));

      StringAssert.Contains(exception.Message, "does not exist or is not a file");
      StringAssert.Contains(exception.Message, missingPath);
      Assert.IsNull(exception.ExitCode);
      Assert.AreEqual(string.Empty, exception.StandardError);
    }

    [TestMethod]
    public async Task ProcessRunner_WrapsExecutableStartFailures()
    {
      string libraryPath = typeof(ApmwSidecarProcessRunner).Assembly.Location;
      var runner = new ApmwSidecarProcessRunner(libraryPath, TimeSpan.FromSeconds(1));

      ApmwSidecarProcessException exception =
        await Assert.ThrowsExceptionAsync<ApmwSidecarProcessException>(
          () => runner.RunAsync(CreateRequest(), CancellationToken.None));

      StringAssert.Contains(exception.Message, "could not start sidecar executable");
      Assert.IsNotNull(exception.InnerException);
      Assert.IsNull(exception.ExitCode);
    }

    [TestMethod]
    public void Request_RequiresAValidExpectedRuntimeSemanticVersion()
    {
      using (JsonDocument input = JsonDocument.Parse("{}"))
      {
        Assert.ThrowsException<ArgumentException>(() =>
          new ApmwSidecarRequest(
            "id",
            ContractHash,
            input.RootElement,
            new[] { "8x8" },
            "invalid"));
      }
    }

    private static ApmwSidecarRequest CreateRequest(string[] geometries = null)
    {
      using (JsonDocument input = JsonDocument.Parse(
        "{\"itemization\":\"legacy\",\"ordering\":\"stable\",\"seeds\":{\"pocket_seed\":\"1\"," +
        "\"pawn_seed\":\"2\",\"minor_seed\":\"3\",\"major_seed\":\"4\",\"queen_seed\":\"5\"}," +
        "\"item_counts\":{\"Progressive Pawn\":8}}"))
      {
        return new ApmwSidecarRequest(
          "request-1",
          ContractHash,
          input.RootElement,
          geometries ?? new[] { "8x8" });
      }
    }

    private static string SuccessJson(string results)
    {
      return "{\"protocol_version\":1,\"request_id\":\"request-1\",\"contract_hash\":\"" + ContractHash +
        "\",\"runtime_semantic_version\":\"0.1.0\",\"results\":" + results + "}";
    }

    private static string SuccessJsonFor(params string[] geometries)
    {
      string[] results = Array.ConvertAll(
        geometries,
        geometry => "{\"geometry_stage\":\"" + geometry + "\",\"projection\":" +
          ProjectionJson(geometry) + "}");
      return SuccessJson("[" + string.Join(",", results) + "]");
    }

    private static string ProjectionJson(string geometry)
    {
      string path = Path.Combine(
        AppContext.BaseDirectory,
        "Fixtures",
        "ProjectionV2",
        "cases.json");
      using (JsonDocument document = JsonDocument.Parse(File.ReadAllText(path)))
      {
        foreach (JsonElement testCase in document.RootElement.GetProperty("cases").EnumerateArray())
        {
          if (testCase.GetProperty("id").GetString() == "geometry-" + geometry)
            return testCase.GetProperty("output").GetRawText();
        }
      }
      throw new InvalidOperationException("Missing projection fixture for " + geometry);
    }

    private static void AssertProtocolFailure(string json, ApmwSidecarRequest request = null)
    {
      Assert.ThrowsException<ApmwSidecarProtocolException>(() =>
        ApmwSidecarProtocol.ParseResponse(json, request ?? CreateRequest(new[] { "8x8" })));
    }

    private sealed class FailingRunner : IApmwSidecarRunner
    {
      public Task<ApmwSidecarSuccessResponse> RunAsync(
        ApmwSidecarRequest request,
        CancellationToken cancellationToken)
      {
        return Task.FromException<ApmwSidecarSuccessResponse>(
          new ApmwSidecarRemoteException(
            new ApmwSidecarErrorResponse("bad_input", "input was invalid"),
            19,
            "details"));
      }
    }
  }
}
