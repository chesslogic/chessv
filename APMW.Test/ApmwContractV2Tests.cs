using Archipelago.APChessV;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace APMW.Test
{
  [TestClass]
  public class ApmwContractV2Tests
  {
    private const string ExpectedHash =
      "f1456e916285bf79dd4be6f4c8c6e5798ed7bb1eebd2f6e1f81075f39e8ffc15";

    private static string FixturePath
    {
      get
      {
        return Path.Combine(
          AppContext.BaseDirectory,
          "Fixtures",
          "ProjectionV2",
          "baseline.json");
      }
    }

    private string baseline;

    [TestInitialize]
    public void Initialize()
    {
      baseline = File.ReadAllText(FixturePath);
    }

    [TestMethod]
    public void Parse_AcceptsBaselineAndVerifiesFrozenHash()
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(baseline);

      Assert.AreEqual(2, contract.Version.Major);
      Assert.AreEqual(0, contract.Version.Minor);
      Assert.AreEqual(ExpectedHash, contract.ManifestSha256);
      Assert.AreEqual(ExpectedHash, ApmwContractV2Parser.ComputeManifestSha256(baseline));
      CollectionAssert.AreEqual(
        new[] { "8x8", "10x8", "10x10", "12x10", "12x12" },
        contract.StageOrder.ToArray());
      CollectionAssert.AreEqual(
        new[]
        {
          "8x8:15:32:39:24",
          "10x8:19:40:49:30",
          "10x10:39:60:69:50",
          "12x10:47:72:83:60",
          "12x12:71:96:107:84",
        },
        contract.Stages.Select(stage =>
          stage.StageId + ":" +
          stage.NonPawnCapacity + ":" +
          stage.GrossPawnCapacity + ":" +
          stage.CombinedNonPrimaryCapacity + ":" +
          stage.ForwardnessCapacity).ToArray());
      Assert.AreEqual(107, contract.EffectiveItemMaxima["fundamental"]["Chessmen"]);
    }

    [TestMethod]
    public void Parse_FreezesPlacementRolesUpgradePreservationAndModeSemantics()
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(baseline);
      CollectionAssert.AreEqual(
        new[]
        {
          "primary-royal",
          "additional-royal",
          "locked-castler",
          "jack-slot",
          "major-slot",
          "minor-slot",
          "pawn-slot",
        },
        contract.SourceRoles.ToArray());

      var transitions = contract.UpgradeDag.Transitions.ToDictionary(item => item.Action);
      Assert.AreEqual("establish-minor-slot", transitions["pawn-to-minor"].SourceRoleRule);
      Assert.AreEqual("establish-major-slot", transitions["pawn-to-major"].SourceRoleRule);
      CollectionAssert.AreEqual(
        new[] { "major-slot", "minor-slot" },
        transitions["major-to-queen"].AllowedSourceRoles.ToArray());
      CollectionAssert.AreEqual(
        new[] { "jack-slot", "major-slot", "minor-slot" },
        transitions["queen-to-amazon"].AllowedSourceRoles.ToArray());
      Assert.AreEqual("preserve", transitions["better-pawn"].SourceRoleRule);

      ModeCombination stableFundamental = contract.ModeCombinations.Single(
        mode => mode.SemanticId == "stable-fundamental");
      Assert.IsTrue(stableFundamental.SemanticSnapshotDeterministic);
      Assert.IsTrue(stableFundamental.SemanticSeriesIsolated);
      Assert.IsFalse(stableFundamental.ExperientialPrefixStable);
      Assert.IsFalse(stableFundamental.PlannedSeriesPrefixStable);
      ModeCombination stableLegacy = contract.ModeCombinations.Single(
        mode => mode.SemanticId == "stable-legacy");
      Assert.IsFalse(stableLegacy.ExperientialPrefixStable);
      Assert.IsTrue(stableLegacy.PlannedSeriesPrefixStable);
      CollectionAssert.Contains(
        contract.SemanticSeriesIds.ToArray(),
        "fundamental.omission.{source-role}");
    }

    [TestMethod]
    public void Parse_FreezesGeometryUnlocksAndDynamicPawnCapacityFormula()
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(baseline);
      CollectionAssert.AreEqual(
        new[]
        {
          "board-file-unlock:8:2:12",
          "board-rank-unlock:8:2:12",
        },
        contract.GeometryUnlocks.Roles.Select(role =>
          role.RoleId + ":" + role.Base + ":" + role.Increment + ":" + role.Maximum).ToArray());
      Assert.AreEqual(
        "largest-componentwise-unlocked-valid-pair",
        contract.GeometryUnlocks.SelectionPolicy);
      Assert.AreEqual(
        "width-times-ranks-minus-four-v1",
        contract.PawnCapacityFormula.GrossPawnCapacityAlgorithm);
      Assert.AreEqual(8, contract.PawnCapacityFormula.NonPawnsBeyondBack(8, 15));
      Assert.AreEqual(24, contract.PawnCapacityFormula.ActivePawnCapacity(8, 32, 15));
      Assert.AreEqual(60, contract.PawnCapacityFormula.NonPawnsBeyondBack(12, 71));
      Assert.AreEqual(36, contract.PawnCapacityFormula.ActivePawnCapacity(12, 96, 71));
    }

    [TestMethod]
    public void Parse_FreezesMaterialFirstActivationAndCastlerReclassification()
    {
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(baseline);
      CollectionAssert.AreEqual(
        contract.SourceRoles.ToArray(),
        contract.OverflowPolicy.RolePriority.ToArray());
      CollectionAssert.AreEqual(
        new[]
        {
          "final-expected-material-descending",
          "ap-granted-material-descending",
          "source-ordinal-ascending",
        },
        contract.OverflowPolicy.WithinRoleActivationOrder.ToArray());
      Assert.AreEqual(
        "normalized-ap-granted-material-per-reserve-slot-exactly-once",
        contract.OverflowPolicy.MissingMaterialAccounting);
      Assert.IsTrue(contract.OverflowPolicy.AggregateSemanticsIgnorePresentation);
      Assert.AreEqual("presentation-only", contract.OverflowPolicy.ChaosNonceScope);

      Assert.AreEqual("locked-castler", contract.Castler.SourceRole);
      Assert.IsTrue(contract.Castler.ReclassifiesExistingChessman);
      Assert.IsTrue(contract.Castler.SourceRoleImmutable);
      Assert.IsFalse(contract.Castler.AddsChessman);
      Assert.IsTrue(contract.Castler.RequiresExistingChessman);
      Assert.IsTrue(contract.Castler.OccupiesBoardSlot);
      CollectionAssert.AreEqual(
        new[] { "major", "jack" },
        contract.Castler.TargetFamilies.ToArray());
      Assert.AreEqual("jack", contract.Castler.UpgradeCeiling);
      Assert.AreEqual(500, contract.Castler.NormalizedMaterial);
      Assert.AreEqual(500, contract.Castler.NormalizedCost);
      Assert.AreEqual(2, contract.Castler.Maximum);
    }

    [TestMethod]
    public void Fixture_MatchesPythonMirrorWhenBothRepositoriesArePresent()
    {
      string pythonFixture = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "..",
        "rft50-checksmate",
        "worlds",
        "checksmate",
        "test",
        "fixtures",
        "projection-v2",
        "baseline.json"));
      if (File.Exists(pythonFixture))
      {
        // Git may materialize the tracked LF fixture as CRLF on Windows.
        Assert.AreEqual(
          File.ReadAllText(FixturePath).Replace("\r\n", "\n"),
          File.ReadAllText(pythonFixture).Replace("\r\n", "\n"));
      }
    }

    [TestMethod]
    public void Parse_RejectsUnknownMajorAndNewerMinorExplicitly()
    {
      JsonObject document = ParseObject();
      document["version"]["major"] = 3;
      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwContractV2Parser.Parse(document.ToJsonString())).Message,
        "unsupported contract major version 3");

      document["version"]["major"] = 2;
      document["version"]["minor"] = 1;
      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwContractV2Parser.Parse(document.ToJsonString())).Message,
        "unsupported contract minor version 1");
    }

    [TestMethod]
    public void Parse_RejectsHashMismatch()
    {
      JsonObject document = ParseObject();
      document["minimum_client_version"] = "0.4.1";

      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwContractV2Parser.Parse(document.ToJsonString())).Message,
        "manifest SHA-256 mismatch");
    }

    [TestMethod]
    public void Parse_RejectsMalformedGeometryDuplicateStageAndBadCapacity()
    {
      JsonObject malformedStage = ParseObject();
      malformedStage["geometry"]["valid_pairs"][1]["stage_id"] = "10-by-8";
      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwContractV2Parser.Parse(Rehash(malformedStage))).Message,
        "stage_id must be canonical");

      JsonObject duplicateStage = ParseObject();
      duplicateStage["geometry"]["valid_pairs"][1] =
        JsonNode.Parse(duplicateStage["geometry"]["valid_pairs"][0].ToJsonString());
      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwContractV2Parser.Parse(Rehash(duplicateStage))).Message,
        "duplicate geometries");

      JsonObject badCapacity = ParseObject();
      badCapacity["geometry"]["valid_pairs"][4]["non_pawn_capacity"] = 70;
      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwContractV2Parser.Parse(Rehash(badCapacity))).Message,
        "expanded-formation-v2");
    }

    [TestMethod]
    public void Parse_RejectsAlternativeInternallyConsistentGeometryPath()
    {
      JsonObject document = ParseObject();
      var alternative = new[]
      {
        (Files: 8, Ranks: 8),
        (Files: 8, Ranks: 10),
        (Files: 10, Ranks: 10),
        (Files: 10, Ranks: 12),
        (Files: 12, Ranks: 12),
      };
      document["geometry"]["stage_order"] = new JsonArray(
        alternative.Select(pair => JsonValue.Create(pair.Files + "x" + pair.Ranks)).ToArray());
      JsonArray stages = document["geometry"]["valid_pairs"].AsArray();
      for (int index = 0; index < alternative.Length; index++)
      {
        int width = alternative[index].Files;
        int height = alternative[index].Ranks;
        int locations = 7 * width + 15 + index;
        JsonObject stage = stages[index].AsObject();
        stage["stage_id"] = width + "x" + height;
        stage["files"] = width;
        stage["ranks"] = height;
        stage["deployment_depth"] = height - 3;
        stage["combined_non_primary_capacity"] = width * (height - 3) - 1;
        stage["gross_pawn_capacity"] = width * (height - 4);
        stage["forwardness_capacity"] = width * (height - 5);
        stage["non_pawn_capacity"] = width * (height - 6) - 1;
        stage["cpu_pawn_count"] = width;
        stage["cpu_non_king_count"] = width - 1;
        stage["all_tactics_locations"] = locations;
        stage["turns_locations"] = locations - 6;
        stage["no_tactics_locations"] = locations - 10;
      }

      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwContractV2Parser.Parse(Rehash(document))).Message,
        "frozen v2.0 value");
    }

    [TestMethod]
    public void Parse_RejectsMalformedUpgradeActivationAndCastlerSchema()
    {
      JsonObject badTransition = ParseObject();
      badTransition["upgrade_dag"]["transitions"][7]["source_role_rule"] = "become-queen-slot";
      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwContractV2Parser.Parse(Rehash(badTransition))).Message,
        "frozen v2.0 value");

      JsonObject badActivation = ParseObject();
      badActivation["overflow_policy"]["within_role_activation_order"][0] =
        "source-ordinal-descending";
      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwContractV2Parser.Parse(Rehash(badActivation))).Message,
        "frozen v2.0 value");

      JsonObject badCastler = ParseObject();
      badCastler["castler"]["target_families"] = new JsonArray("queen");
      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwContractV2Parser.Parse(Rehash(badCastler))).Message,
        "frozen v2.0 value");
    }

    [TestMethod]
    public void Parse_RejectsUnknownIdentifierAndDuplicateJsonProperty()
    {
      JsonObject unknownIdentifier = ParseObject();
      unknownIdentifier["algorithms"]["projection"] = "surprise-v9";
      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwContractV2Parser.Parse(Rehash(unknownIdentifier))).Message,
        "frozen v2.0 value");

      string duplicate = baseline.Replace(
        "\"schema\": \"apmw_contract\",",
        "\"schema\": \"apmw_contract\", \"schema\": \"apmw_contract\",");
      StringAssert.Contains(
        Assert.ThrowsException<ApmwContractException>(
          () => ApmwContractV2Parser.Parse(duplicate)).Message,
        "duplicate JSON property");
    }

    private JsonObject ParseObject()
    {
      return JsonNode.Parse(baseline).AsObject();
    }

    private static string Rehash(JsonObject document)
    {
      document["manifest_sha256"] = "";
      string blanked = document.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
      document["manifest_sha256"] = ApmwContractV2Parser.ComputeManifestSha256(blanked);
      return document.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }
  }
}
