using Archipelago.APChessV;
using ChessV.Base;
using ChessV.Games;
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
  [DoNotParallelize]
  public class ApmwSidecarPresentationAdapterTests
  {
    [TestInitialize]
    public void Setup()
    {
      Reset();
    }

    [TestCleanup]
    public void Cleanup()
    {
      Reset();
    }

    [TestMethod]
    public void Adapter_UsesCanonicalOwnedSlotsAndProjectionWithoutReplanning()
    {
      Fixture fixture = LoadFixture("fundamental-full");
      ApmwSidecarPresentationAdapter adapter = CreateAdapter(fixture.Input);

      GeneratedRoster roster = adapter.CreateOwnedRoster(fixture.Output);
      ActiveRosterProjection projection = adapter.CreateProjection(fixture.Output);

      Assert.AreEqual(
        fixture.Output.GetProperty("owned_slots").GetArrayLength() - 1,
        roster.RosterPieces.Count);
      Assert.AreEqual(
        fixture.Output.GetProperty("active_slots").GetArrayLength() - 1,
        projection.ActivePieces.Count);
      Assert.AreEqual(
        fixture.Output.GetProperty("reserve_slots").GetArrayLength(),
        projection.ReservePieces.Count);
      Assert.AreEqual(
        fixture.Output.GetProperty("owned_expected_material").GetInt32(),
        projection.OwnedExpectedMaterial);
      Assert.AreEqual(
        fixture.Output.GetProperty("exact_active_material").GetInt32(),
        projection.ExactActiveExpectedMaterial);
      Assert.AreEqual(
        fixture.Output.GetProperty("missing_material").GetInt32(),
        projection.MissingMaterial);
      Assert.AreEqual(
        fixture.Output.GetProperty("total_accounted_material").GetInt32(),
        roster.OwnedMaterialLedger.GrantedTotal);
      Assert.IsTrue(roster.RosterPieces.All(piece => piece.ConcretePieceType != null));
      CollectionAssert.AreEqual(
        fixture.Output.GetProperty("available_promotion_families")
          .EnumerateArray().Select(value => value.GetString()).ToList(),
        projection.ActivePromotionFamilyCatalog.ToList());
      CollectionAssert.AreEqual(
        fixture.Output.GetProperty("reserve_promotion_families")
          .EnumerateArray().Select(value => value.GetString()).ToList(),
        projection.ReservePromotionFamilyCatalog.ToList());
      CollectionAssert.AreEqual(
        fixture.Output.GetProperty("active_castlers")
          .EnumerateArray().Select(value => value.GetString()).ToList(),
        projection.ActiveCastlers.ToList());
      CollectionAssert.AreEqual(
        fixture.Output.GetProperty("castling_eligible_slots")
          .EnumerateArray().Select(value => value.GetString()).ToList(),
        projection.CastlingRights.Select(right => right.StableId).ToList());

      Dictionary<string, RosterPiece> byId = roster.RosterPieces.ToDictionary(
        piece => piece.StableId,
        StringComparer.Ordinal);
      foreach (JsonElement slot in fixture.Output.GetProperty("owned_slots").EnumerateArray())
      {
        if (slot.GetProperty("source_role").GetString() == "primary-royal")
          continue;
        RosterPiece piece = byId[slot.GetProperty("slot_id").GetString()];
        Assert.AreEqual(slot.GetProperty("source_ordinal").GetInt32(), piece.SourceOrdinal);
        Assert.AreEqual(
          slot.GetProperty("granted_material").GetInt32(),
          piece.GrantedMaterial);
        CollectionAssert.AreEqual(
          slot.GetProperty("upgrade_path").EnumerateArray().Select(value => value.GetString()).ToList(),
          piece.UpgradePath.ToList());
      }

      Dictionary<string, JsonElement> expectedPlacements = fixture.Output
        .GetProperty("active_placements")
        .EnumerateArray()
        .ToDictionary(value => value.GetProperty("slot_id").GetString(), value => value);
      foreach (ProjectedRosterPiece piece in projection.ActivePieces)
      {
        JsonElement expected = expectedPlacements[piece.StableId];
        Assert.AreEqual(expected.GetProperty("file").GetInt32(), piece.Placement.Coordinate.File);
        Assert.AreEqual(
          expected.GetProperty("relative_rank").GetInt32(),
          piece.Placement.Coordinate.RelativeRank);
        Assert.AreEqual(
          expected.GetProperty("formation_band").GetString(),
          piece.Placement.FormationBand);
      }
    }

    [TestMethod]
    public void Adapter_PresentationChoicesAreStableAcrossGeometrySnapshots()
    {
      Fixture small = LoadFixture("geometry-8x8");
      Fixture large = LoadFixture("geometry-12x12");
      ApmwSidecarPresentationAdapter adapter = CreateAdapter(small.Input);

      GeneratedRoster smallRoster = adapter.CreateOwnedRoster(small.Output);
      GeneratedRoster largeRoster = adapter.CreateOwnedRoster(large.Output);

      CollectionAssert.AreEqual(
        smallRoster.RosterPieces
          .OrderBy(piece => piece.StableId, StringComparer.Ordinal)
          .Select(piece => piece.StableId + "=" + piece.ConcretePieceType.Name)
          .ToList(),
        largeRoster.RosterPieces
          .OrderBy(piece => piece.StableId, StringComparer.Ordinal)
          .Select(piece => piece.StableId + "=" + piece.ConcretePieceType.Name)
          .ToList());
    }

    [TestMethod]
    public void Adapter_UsesSidecarPrimaryRoyalAndLedgerValues()
    {
      Fixture fixture = LoadFixture("legacy-normalization-all-ledgers");
      ApmwSidecarPresentationAdapter adapter = CreateAdapter(fixture.Input);

      GeneratedRoster roster = adapter.CreateOwnedRoster(fixture.Output);
      ActiveRosterProjection projection = adapter.CreateProjection(fixture.Output);
      JsonElement primary = fixture.Output.GetProperty("owned_slots")
        .EnumerateArray()
        .Single(slot => slot.GetProperty("source_role").GetString() == "primary-royal");

      Assert.AreEqual(primary.GetProperty("granted_material").GetInt32(), roster.PrimaryKingGrantedMaterial);
      Assert.AreEqual(primary.GetProperty("final_expected_material").GetInt32(), roster.PrimaryKingExpectedMaterial);
      Assert.AreEqual(
        fixture.Output.GetProperty("total_accounted_material").GetInt32(),
        roster.OwnedMaterialLedger.GrantedTotal);
      CollectionAssert.Contains(projection.ActivePromotionFamilyCatalog.ToList(), "royal");
    }

    [TestMethod]
    public void SidecarBackend_ReusesOneBatchUntilInvalidated()
    {
      Fixture fixture = LoadFixture("geometry-8x8");
      ApmwSidecarPresentationAdapter adapter = CreateAdapter(fixture.Input);
      ApmwContractV2 contract = Contract();
      ApmwSidecarInputSnapshot snapshot = Snapshot(fixture.Input, contract);
      var runner = new FixtureRunner();
      var cache = new ApmwSidecarBatchCache(runner, Identity(contract));
      var backend = new ApmwSidecarProjectionBackend(cache, () => snapshot, () => adapter);

      GeneratedRoster roster = backend.GenerateOwnedRoster();
      ActiveRosterProjection small = backend.Project(ProjectionGeometry.For(8, 8));
      ActiveRosterProjection large = backend.Project(ProjectionGeometry.For(12, 12));

      Assert.AreEqual(1, runner.Calls);
      Assert.AreEqual(8, roster.RosterPieces.Count);
      Assert.AreEqual("8x8", small.Geometry.StageId);
      Assert.AreEqual("12x12", large.Geometry.StageId);

      backend.Invalidate();
      backend.Project(ProjectionGeometry.For(8, 8));
      Assert.AreEqual(2, runner.Calls);
    }

    private static ApmwSidecarPresentationAdapter CreateAdapter(JsonElement input)
    {
      string ordering = input.GetProperty("ordering").GetString();
      JsonElement seeds = input.GetProperty("seeds");
      var slotData = new Dictionary<string, object>
      {
        { ApmwConstants.SlotKeyProgressionItemization, input.GetProperty("itemization").GetString() },
        { "piece_locations", ordering == "chaos" ? (int)PieceLocations.Chaos : (int)PieceLocations.Stable },
        { "piece_types", ordering == "chaos" ? (int)PieceTypes.Chaos : (int)PieceTypes.Stable },
        { "pocket_seed", seeds.GetProperty("pocket_seed").GetString() },
        { "pawn_seed", seeds.GetProperty("pawn_seed").GetString() },
        { "minor_seed", seeds.GetProperty("minor_seed").GetString() },
        { "major_seed", seeds.GetProperty("major_seed").GetString() },
        { "queen_seed", seeds.GetProperty("queen_seed").GetString() },
      };
      ApmwConfig config = ApmwConfig.getInstance();
      config.Instantiate(slotData);
      config.seed();
      new ApmwChessGame().earlyPopulatePieceTypes();
      return new ApmwSidecarPresentationAdapter(ApmwCore.getInstance(), config);
    }

    private static Fixture LoadFixture(string id)
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
          if (testCase.GetProperty("id").GetString() == id)
          {
            return new Fixture(
              testCase.GetProperty("input").Clone(),
              testCase.GetProperty("output").Clone());
          }
        }
      }
      throw new InvalidOperationException("Missing projection fixture " + id);
    }

    private static ApmwContractV2 Contract()
    {
      string path = Path.Combine(
        AppContext.BaseDirectory,
        "Fixtures",
        "ProjectionV2",
        "baseline.json");
      return ApmwContractV2Parser.Parse(File.ReadAllText(path));
    }

    private static ApmwSidecarIdentity Identity(ApmwContractV2 contract)
    {
      string path = Path.Combine(
        AppContext.BaseDirectory,
        "Fixtures",
        "ProjectorLock",
        "synthetic-lock.json");
      return ApmwSidecarIdentity.FromValidatedLock(
        ApmwProjectorLockParser.Parse(File.ReadAllText(path)),
        contract);
    }

    private static ApmwSidecarInputSnapshot Snapshot(JsonElement input, ApmwContractV2 contract)
    {
      return new ApmwSidecarInputSnapshot(
        contract,
        input.GetProperty("itemization").GetString(),
        input.GetProperty("ordering").GetString(),
        input.GetProperty("seeds").EnumerateObject()
          .Select(seed => new KeyValuePair<string, string>(seed.Name, seed.Value.GetString())),
        input.GetProperty("item_counts").EnumerateObject()
          .Select(item => new KeyValuePair<string, int>(item.Name, item.Value.GetInt32())));
    }

    private static void Reset()
    {
      ApmwCore._instance = null;
      ApmwConfig._instance = null;
    }

    private sealed class Fixture
    {
      public Fixture(JsonElement input, JsonElement output)
      {
        Input = input;
        Output = output;
      }

      public JsonElement Input { get; }
      public JsonElement Output { get; }
    }

    private sealed class FixtureRunner : IApmwSidecarRunner
    {
      private int calls;

      public int Calls { get { return Volatile.Read(ref calls); } }

      public Task<ApmwSidecarSuccessResponse> RunAsync(
        ApmwSidecarRequest request,
        CancellationToken cancellationToken)
      {
        Interlocked.Increment(ref calls);
        IReadOnlyList<ApmwSidecarProjectionResult> results = request.Geometries
          .Select(stage => new ApmwSidecarProjectionResult(
            stage,
            LoadFixture("geometry-" + stage).Output))
          .ToList()
          .AsReadOnly();
        return Task.FromResult(new ApmwSidecarSuccessResponse(
          request.RequestId,
          request.ContractHash,
          request.ExpectedRuntimeSemanticVersion,
          results));
      }
    }
  }
}
