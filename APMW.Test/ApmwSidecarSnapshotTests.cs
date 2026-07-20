using Archipelago.APChessV;
using ChessV.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace APMW.Test
{
  [TestClass]
  public class ApmwSidecarSnapshotTests
  {
    private const string ContractHash =
      "f1456e916285bf79dd4be6f4c8c6e5798ed7bb1eebd2f6e1f81075f39e8ffc15";

    [TestMethod]
    public void SnapshotAndRequest_UseTheExactCanonicalWireShape()
    {
      ApmwSidecarInputSnapshot snapshot = CreateSnapshot(new[]
      {
        new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.Pawn, 8),
      });

      Assert.AreEqual(
        "{\"itemization\":\"legacy\",\"ordering\":\"stable\",\"seeds\":{\"pocket_seed\":\"1\",\"pawn_seed\":\"2\",\"minor_seed\":\"3\",\"major_seed\":\"4\",\"queen_seed\":\"5\"},\"item_counts\":{\"Progressive Pawn\":8},\"upgrade_preferences\":[{\"action\":\"new-pawn\",\"priority\":2,\"proportion_numerator\":1,\"proportion_denominator\":2}]}",
        snapshot.CanonicalJson);

      ApmwSidecarRequest request = Identity().CreateRequest(snapshot);
      Assert.AreEqual("apmw-" + snapshot.InputSha256, request.RequestId);
      Assert.AreEqual(ContractHash, request.ContractHash);
      Assert.AreEqual("0.1.0", request.ExpectedRuntimeSemanticVersion);
      CollectionAssert.AreEqual(
        new[] { "8x8", "10x8", "10x10", "12x10", "12x12" },
        request.Geometries.ToArray());
      Assert.AreEqual(
        "{\"protocol_version\":1,\"request_id\":\"" + request.RequestId + "\",\"contract_hash\":\"" +
        ContractHash + "\",\"input\":" + snapshot.CanonicalJson +
        ",\"geometries\":[\"8x8\",\"10x8\",\"10x10\",\"12x10\",\"12x12\"]}",
        ApmwSidecarProtocol.SerializeRequest(request));
    }

    [TestMethod]
    public void SnapshotIdentity_IsIndependentOfInputDictionaryOrder()
    {
      ApmwContractV2 contract = Contract();
      var first = new ApmwSidecarInputSnapshot(
        contract, "legacy", "stable",
        Seeds().Reverse(),
        new[]
        {
          new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.Pawn, 8),
          new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.MinorPiece, 2),
        });
      var second = new ApmwSidecarInputSnapshot(
        contract, "legacy", "stable",
        Seeds(),
        new[]
        {
          new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.MinorPiece, 2),
          new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.Pawn, 8),
        });

      Assert.AreEqual(first.CanonicalJson, second.CanonicalJson);
      Assert.AreEqual(first.InputSha256, second.InputSha256);
      Assert.AreEqual(Identity().CreateRequest(first).RequestId, Identity().CreateRequest(second).RequestId);
    }

    [TestMethod]
    public void SnapshotGeometryStages_MatchTheValidatedContract()
    {
      CollectionAssert.AreEqual(
        Contract().Stages.Select(stage => stage.StageId).ToList(),
        ApmwSidecarInputSnapshot.AllGeometryStages.ToList());
    }

    [TestMethod]
    public void Snapshot_RejectsInvalidSemanticInputs()
    {
      ApmwContractV2 contract = Contract();
      Assert.ThrowsException<ArgumentException>(() => new ApmwSidecarInputSnapshot(
        contract, "fundamental", "chaos", Seeds(), Array.Empty<KeyValuePair<string, int>>()));
      Assert.ThrowsException<ArgumentException>(() => new ApmwSidecarInputSnapshot(
        contract, "legacy", "stable", Seeds().Take(4), Array.Empty<KeyValuePair<string, int>>()));
      Assert.ThrowsException<ArgumentException>(() => new ApmwSidecarInputSnapshot(
        contract, "legacy", "stable", Seeds().Concat(new[] { Seeds().First() }),
        Array.Empty<KeyValuePair<string, int>>()));
      Assert.ThrowsException<ArgumentException>(() => new ApmwSidecarInputSnapshot(
        contract, "legacy", "stable", Seeds(),
        new[] { new KeyValuePair<string, int>("not an item", 1) }));
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ApmwSidecarInputSnapshot(
        contract, "legacy", "stable", Seeds(),
        new[] { new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.Pawn, -1) }));
      Assert.ThrowsException<ArgumentException>(() => new ApmwSidecarInputSnapshot(
        contract, "legacy", "stable", Seeds(), Array.Empty<KeyValuePair<string, int>>(),
        new[] { new ApmwSidecarUpgradePreference("not-an-action", 0, 1, 1) }));
      Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ApmwSidecarInputSnapshot(
        contract, "legacy", "stable", Seeds(), Array.Empty<KeyValuePair<string, int>>(),
        new[] { new ApmwSidecarUpgradePreference(ApmwConstants.PieceUpgradeActions.NewPawn, 0, 1, 0) }));
      Assert.ThrowsException<ArgumentException>(() => new ApmwSidecarInputSnapshot(
        contract, "legacy", "stable", Seeds(), Array.Empty<KeyValuePair<string, int>>(),
        new[]
        {
          new ApmwSidecarUpgradePreference(ApmwConstants.PieceUpgradeActions.NewPawn, 0, 1, 1),
          new ApmwSidecarUpgradePreference(ApmwConstants.PieceUpgradeActions.NewPawn, 1, 1, 1),
        }));
    }

    [TestMethod]
    public void Capture_RequiresExactCurrentFundamentalMaterialConversion()
    {
      ApmwContractV2 contract = Contract();
      ApmwConfig config = CurrentConfig(contract, "fundamental");
      ApmwCore core = ReadyCore();
      core.foundMaterialBudget = 401;

      Assert.ThrowsException<InvalidOperationException>(() =>
        ApmwSidecarSnapshotCapture.Capture(core, config, contract));

      core.foundMaterialBudget = 800;
      ApmwSidecarInputSnapshot snapshot = ApmwSidecarSnapshotCapture.Capture(core, config, contract);
      Assert.AreEqual(2, snapshot.ItemCounts[ApmwConstants.ProgressiveItems.Material]);
      Assert.IsFalse(snapshot.ItemCounts.ContainsKey(ApmwConstants.ProgressiveItems.Amazon));
    }

    [TestMethod]
    public void Capture_KeepsFundamentalSemanticsStableWhenPresentationIsChaos()
    {
      ApmwContractV2 contract = Contract();
      ApmwConfig config = CurrentConfig(
        contract,
        "fundamental",
        PieceTypes.Chaos,
        PieceLocations.Chaos);
      ApmwCore core = ReadyCore();

      ApmwSidecarInputSnapshot snapshot = ApmwSidecarSnapshotCapture.Capture(core, config, contract);

      Assert.AreEqual("fundamental", snapshot.Itemization);
      Assert.AreEqual("stable", snapshot.Ordering);
    }

    [TestMethod]
    public async Task BatchCache_UsesOneRunnerCallForAllGeometryLookups()
    {
      var runner = new FakeRunner(request => Task.FromResult(ResponseFor(request)));
      var cache = new ApmwSidecarBatchCache(runner, Identity());
      ApmwSidecarInputSnapshot snapshot = CreateSnapshot();

      JsonElement first = await cache.GetProjectionAsync(snapshot, "8x8", CancellationToken.None);
      JsonElement second = await cache.GetProjectionAsync(snapshot, "12x12", CancellationToken.None);

      Assert.AreEqual(1, runner.Calls);
      Assert.AreEqual("8x8", first.GetProperty("geometry_stage").GetString());
      Assert.AreEqual("12x12", second.GetProperty("geometry_stage").GetString());
    }

    [TestMethod]
    public async Task BatchCache_SharesConcurrentCalls()
    {
      var completion = new TaskCompletionSource<ApmwSidecarSuccessResponse>(
        TaskCreationOptions.RunContinuationsAsynchronously);
      var runner = new FakeRunner(request => completion.Task);
      var cache = new ApmwSidecarBatchCache(runner, Identity());
      ApmwSidecarInputSnapshot snapshot = CreateSnapshot();

      Task<ApmwSidecarSuccessResponse> first = cache.GetAsync(snapshot, CancellationToken.None);
      Task<ApmwSidecarSuccessResponse> second = cache.GetAsync(snapshot, CancellationToken.None);
      Assert.IsTrue(SpinWait.SpinUntil(() => runner.Calls == 1, TimeSpan.FromSeconds(1)));
      completion.SetResult(ResponseFor(runner.Requests.Single()));

      await Task.WhenAll(first, second);
      Assert.AreEqual(1, runner.Calls);
    }

    [TestMethod]
    public async Task BatchCache_InvalidatePreventsAnInflightResultFromBeingCached()
    {
      var first = new TaskCompletionSource<ApmwSidecarSuccessResponse>(
        TaskCreationOptions.RunContinuationsAsynchronously);
      var second = new TaskCompletionSource<ApmwSidecarSuccessResponse>(
        TaskCreationOptions.RunContinuationsAsynchronously);
      FakeRunner runner = null;
      runner = new FakeRunner(request => runnerCall(runner, first, second));
      var cache = new ApmwSidecarBatchCache(runner, Identity());
      ApmwSidecarInputSnapshot snapshot = CreateSnapshot();

      Task<ApmwSidecarSuccessResponse> stale = cache.GetAsync(snapshot, CancellationToken.None);
      Assert.IsTrue(SpinWait.SpinUntil(() => runner.Calls == 1, TimeSpan.FromSeconds(1)));
      cache.Invalidate();
      first.SetResult(ResponseFor(runner.Requests[0]));
      await stale;

      Task<ApmwSidecarSuccessResponse> current = cache.GetAsync(snapshot, CancellationToken.None);
      Assert.IsTrue(SpinWait.SpinUntil(() => runner.Calls == 2, TimeSpan.FromSeconds(1)));
      second.SetResult(ResponseFor(runner.Requests[1]));
      await current;
      await cache.GetAsync(snapshot, CancellationToken.None);
      Assert.AreEqual(2, runner.Calls);
    }

    [TestMethod]
    public async Task BatchCache_RetriesAfterRunnerFailure()
    {
      FakeRunner runner = null;
      runner = new FakeRunner(request =>
        runner.Calls == 1
          ? Task.FromException<ApmwSidecarSuccessResponse>(new InvalidOperationException("failed"))
          : Task.FromResult(ResponseFor(request)));
      var cache = new ApmwSidecarBatchCache(runner, Identity());
      ApmwSidecarInputSnapshot snapshot = CreateSnapshot();

      await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => cache.GetAsync(snapshot, CancellationToken.None));
      await cache.GetAsync(snapshot, CancellationToken.None);
      Assert.AreEqual(2, runner.Calls);
    }

    [TestMethod]
    public void FractionConversion_IsExactWithinItsExplicitBound()
    {
      ApmwSidecarFraction fraction = ApmwSidecarFraction.FromDoubleExact(0.125);
      Assert.AreEqual(1, fraction.Numerator);
      Assert.AreEqual(8, fraction.Denominator);
      Assert.ThrowsException<ArgumentException>(() => ApmwSidecarFraction.FromDoubleExact(1.0 / 3.0));
    }

    [TestMethod]
    public void Identity_RejectsAnInvalidLockRuntimeOrContract()
    {
      ApmwContractV2 contract = Contract();
      var badRuntime = new ApmwProjectorLock(
        "0.1.1", 1, ContractHash, "owner/repository", new string('a', 40), "tag", null);
      var badContract = new ApmwProjectorLock(
        "0.1.0", 1, new string('0', 64), "owner/repository", new string('a', 40), "tag", null);

      Assert.ThrowsException<ArgumentException>(() =>
        ApmwSidecarIdentity.FromValidatedLock(badRuntime, contract));
      Assert.ThrowsException<ArgumentException>(() =>
        ApmwSidecarIdentity.FromValidatedLock(badContract, contract));
    }

    private static Task<ApmwSidecarSuccessResponse> runnerCall(
      FakeRunner runner,
      TaskCompletionSource<ApmwSidecarSuccessResponse> first,
      TaskCompletionSource<ApmwSidecarSuccessResponse> second)
    {
      return runner.Calls == 1 ? first.Task : second.Task;
    }

    private static ApmwSidecarInputSnapshot CreateSnapshot(
      IEnumerable<KeyValuePair<string, int>> itemCounts = null)
    {
      return new ApmwSidecarInputSnapshot(
        Contract(),
        "legacy",
        "stable",
        Seeds(),
        itemCounts ?? new[] { new KeyValuePair<string, int>(ApmwConstants.ProgressiveItems.Pawn, 8) },
        new[]
        {
          new ApmwSidecarUpgradePreference(ApmwConstants.PieceUpgradeActions.NewPawn, 2, 1, 2),
        });
    }

    private static IEnumerable<KeyValuePair<string, string>> Seeds()
    {
      return new[]
      {
        new KeyValuePair<string, string>("pocket_seed", "1"),
        new KeyValuePair<string, string>("pawn_seed", "2"),
        new KeyValuePair<string, string>("minor_seed", "3"),
        new KeyValuePair<string, string>("major_seed", "4"),
        new KeyValuePair<string, string>("queen_seed", "5"),
      };
    }

    private static ApmwSidecarIdentity Identity()
    {
      string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "ProjectorLock", "synthetic-lock.json");
      return ApmwSidecarIdentity.FromValidatedLock(ApmwProjectorLockParser.Parse(File.ReadAllText(path)), Contract());
    }

    private static ApmwContractV2 Contract()
    {
      string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "ProjectionV2", "baseline.json");
      return ApmwContractV2Parser.Parse(File.ReadAllText(path));
    }

    private static ApmwConfig CurrentConfig(
      ApmwContractV2 contract,
      string itemization,
      PieceTypes pieceTypes = PieceTypes.Stable,
      PieceLocations pieceLocations = PieceLocations.Stable)
    {
      var slotData = new Dictionary<string, object>
      {
        { "apmw_contract", "validated separately" },
        { ApmwConstants.SlotKeyProgressionItemization, itemization },
        { "piece_locations", (int)pieceLocations },
        { "piece_types", (int)pieceTypes },
        { "pocket_seed", "11" },
        { "pawn_seed", "12" },
        { "minor_seed", "13" },
        { "major_seed", "14" },
        { "queen_seed", "15" },
      };
      var config = new ApmwConfig();
      config.Instantiate(slotData, contract);
      return config;
    }

    private static ApmwCore ReadyCore()
    {
      return new ApmwCore
      {
        foundPockets = 0,
        foundPocketRange = 0,
        foundPawns = 0,
        foundMinors = 0,
        foundMajors = 0,
        foundJacks = 0,
        foundQueens = 0,
        foundAmazons = 0,
        foundConsuls = 0,
        foundKingPromotions = 0,
        foundPawnForwardness = 0,
        foundChessmen = 0,
        foundMaterialBudget = 0,
        foundCastlers = 0,
        foundPlayAsWhite = 0,
        EngineWeakeningProvider = () => 0,
      };
    }

    private static ApmwSidecarSuccessResponse ResponseFor(ApmwSidecarRequest request)
    {
      return new ApmwSidecarSuccessResponse(
        request.RequestId,
        request.ContractHash,
        request.ExpectedRuntimeSemanticVersion,
        request.Geometries.Select(geometry => new ApmwSidecarProjectionResult(
          geometry,
          Element("{\"geometry_stage\":\"" + geometry + "\"}"))).ToList().AsReadOnly());
    }

    private static JsonElement Element(string json)
    {
      using (JsonDocument document = JsonDocument.Parse(json))
        return document.RootElement.Clone();
    }

    private sealed class FakeRunner : IApmwSidecarRunner
    {
      private readonly Func<ApmwSidecarRequest, Task<ApmwSidecarSuccessResponse>> run;
      private int calls;

      public FakeRunner(Func<ApmwSidecarRequest, Task<ApmwSidecarSuccessResponse>> run)
      {
        this.run = run;
      }

      public int Calls { get { return Volatile.Read(ref calls); } }
      public List<ApmwSidecarRequest> Requests { get; } = new List<ApmwSidecarRequest>();

      public Task<ApmwSidecarSuccessResponse> RunAsync(
        ApmwSidecarRequest request,
        CancellationToken cancellationToken)
      {
        lock (Requests)
          Requests.Add(request);
        Interlocked.Increment(ref calls);
        return run(request);
      }
    }
  }
}
