using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Archipelago.APChessV
{
  /// <summary>
  /// Strict, non-live parser for the shared APMW contract v2 manifest.
  ///
  /// Canonical hashing rejects duplicate keys, replaces the root manifest_sha256
  /// value with "", sorts every object by ordinal key, preserves array order,
  /// emits printable ASCII without insignificant whitespace, and hashes the
  /// resulting UTF-8 bytes with SHA-256.
  /// </summary>
  public static class ApmwContractV2Parser
  {
    public const int SupportedMajor = 2;
    public const int SupportedMinor = 0;

    private static readonly Regex Sha256Pattern = new Regex("^[0-9a-f]{64}$", RegexOptions.CultureInvariant);
    private static readonly Regex SemverPattern = new Regex(
      "^(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)\\.(0|[1-9][0-9]*)$",
      RegexOptions.CultureInvariant);
    private static readonly Regex StagePattern = new Regex(
      "^([1-9][0-9]*)x([1-9][0-9]*)$",
      RegexOptions.CultureInvariant);

    private static readonly string[] ItemizationModes = { "legacy", "fundamental" };
    private static readonly string[] OrderingModes = { "stable", "chaos" };
    private static readonly ModeCombination[] ModeCombinations =
    {
      new ModeCombination("legacy", "stable", "stable-legacy", true, true, false, true, true, true),
      new ModeCombination("legacy", "chaos", "chaos-legacy", true, true, false, false, true, true),
      new ModeCombination("fundamental", "stable", "stable-fundamental", true, true, false, false, true, true),
    };
    private static readonly int[] FileLadder = { 8, 10, 12 };
    private static readonly int[] RankLadder = { 8, 10, 12 };
    private static readonly string[] StageOrder = { "8x8", "10x8", "10x10", "12x10", "12x12" };
    private static readonly IReadOnlyDictionary<string, string> Algorithms =
      ReadOnly(new Dictionary<string, string>(StringComparer.Ordinal)
      {
        { "capacity", "expanded-formation-v2" },
        { "projection", "placement-role-material-v2" },
        { "overflow", "material-first-reserve-v2" },
        { "ordering", "independent-semantic-series-v1" },
        { "series_prf", "sha256-counter-v1" },
      });
    private static readonly IReadOnlyDictionary<string, int> ExpectedMaterial =
      ReadOnly(new Dictionary<string, int>(StringComparer.Ordinal)
      {
        { "weak", 75 },
        { "pawn", 100 },
        { "minor", 300 },
        { "major", 485 },
        { "castler", 500 },
        { "jack", 700 },
        { "queen", 900 },
        { "amazon", 1300 },
        { "material_item", 400 },
        { "play_as_white", 50 },
        { "pocket", 110 },
        { "consul", 325 },
        { "king_promotion", 425 },
      });
    private static readonly string[] SourceRoles =
    {
      "primary-royal", "additional-royal", "locked-castler",
      "jack-slot", "major-slot", "minor-slot", "pawn-slot",
    };
    private static readonly string[] FinalFamilies =
    {
      "pawn", "minor", "major", "jack", "queen", "amazon",
    };
    private static readonly string[] PawnCreationActions = { "new-pawn", "more-pawn" };
    private static readonly UpgradeTransition[] UpgradeTransitions =
    {
      new UpgradeTransition("better-pawn", "pawn", "pawn", new[] { "pawn-slot" }, "preserve"),
      new UpgradeTransition("pool-pawn-upgrade", "pawn", "pawn", new[] { "pawn-slot" }, "preserve"),
      new UpgradeTransition("pawn-to-minor", "pawn", "minor", new[] { "pawn-slot" }, "establish-minor-slot"),
      new UpgradeTransition("pawn-to-major", "pawn", "major", new[] { "pawn-slot" }, "establish-major-slot"),
      new UpgradeTransition("minor-to-major", "minor", "major", new[] { "minor-slot" }, "preserve"),
      new UpgradeTransition("minor-to-jack", "minor", "jack", new[] { "minor-slot" }, "preserve"),
      new UpgradeTransition("major-to-jack", "major", "jack", new[] { "major-slot", "minor-slot" }, "preserve"),
      new UpgradeTransition("major-to-queen", "major", "queen", new[] { "major-slot", "minor-slot" }, "preserve"),
      new UpgradeTransition(
        "jack-to-queen",
        "jack",
        "queen",
        new[] { "jack-slot", "major-slot", "minor-slot" },
        "preserve"),
      new UpgradeTransition(
        "queen-to-amazon",
        "queen",
        "amazon",
        new[] { "jack-slot", "major-slot", "minor-slot" },
        "preserve"),
    };
    private static readonly string[] SemanticSeriesIds =
    {
      "fundamental.wave.tie.{priority}",
      "fundamental.gateway.role",
      "fundamental.omission.{source-role}",
      "upgrade-source.{action}",
    };
    private static readonly string[] PresentationSeriesIds =
    {
      "placement.{source-role}.{formation-band}",
      "piece-type.{final-family}",
      "upgrade-target.{action}",
      "pocket.choice",
    };
    private static readonly IReadOnlyDictionary<string, int> CommonMaxima =
      ReadOnly(new Dictionary<string, int>(StringComparer.Ordinal)
      {
        { "Play as White", 1 },
        { "Progressive AI Intelligence Malus", 5 },
        { "Progressive Pocket", 12 },
        { "Progressive Pocket Range", 6 },
        { "Progressive King Promotion", 2 },
        { "Progressive Consul", 2 },
      });
    private static readonly IReadOnlyDictionary<string, int> LegacyMaxima =
      ReadOnly(new Dictionary<string, int>(StringComparer.Ordinal)
      {
        { "Progressive Pawn", 60 },
        { "Progressive Pawn Forwardness", 13 },
        { "Progressive Minor Piece", 15 },
        { "Progressive Major Piece", 11 },
        { "Progressive Major To Queen", 9 },
        { "Progressive Jack", 9 },
      });
    private static readonly IReadOnlyDictionary<string, int> FundamentalMaxima =
      ReadOnly(new Dictionary<string, int>(StringComparer.Ordinal)
      {
        { "Chessmen", 107 },
        { "Material", 321 },
        { "Castler", 2 },
      });

    public static ApmwContractV2 Parse(string json)
    {
      if (json == null)
        throw new ArgumentNullException(nameof(json));

      using JsonDocument document = ParseDocument(json);
      JsonElement root = document.RootElement;
      ExactFields(root, "$",
        "schema", "version", "manifest_sha256", "minimum_client_version", "minor_compatibility",
        "itemization_modes", "ordering_modes", "mode_combinations", "geometry", "algorithms",
        "expected_material", "source_roles", "upgrade_dag", "castler", "semantic_series_ids",
        "presentation_series_ids", "overflow_policy", "cpu_profiles", "effective_item_maxima");

      if (String(root, "schema", "$") != "apmw_contract")
        throw Error("$.schema must be apmw_contract");

      JsonElement versionElement = Property(root, "version", "$");
      ExactFields(versionElement, "$.version", "major", "minor");
      var version = new ContractVersion(
        Integer(versionElement, "major", "$.version"),
        Integer(versionElement, "minor", "$.version"));
      if (version.Major != SupportedMajor)
        throw Error("unsupported contract major version " + version.Major);
      if (version.Minor > SupportedMinor)
      {
        throw Error(
          "unsupported contract minor version " + version.Minor +
          "; parser supports through " + SupportedMinor);
      }

      string manifestHash = String(root, "manifest_sha256", "$");
      if (!Sha256Pattern.IsMatch(manifestHash))
        throw Error("$.manifest_sha256 must be 64 lowercase hexadecimal characters");
      string minimumClientVersion = String(root, "minimum_client_version", "$");
      if (!SemverPattern.IsMatch(minimumClientVersion))
        throw Error("$.minimum_client_version must be a three-part semantic version");
      string minorCompatibility = String(root, "minor_compatibility", "$");
      RequireEqual(minorCompatibility, "same-or-older", "$.minor_compatibility");

      IReadOnlyList<string> itemizationModes = StringArray(
        Property(root, "itemization_modes", "$"), "$.itemization_modes");
      IReadOnlyList<string> orderingModes = StringArray(
        Property(root, "ordering_modes", "$"), "$.ordering_modes");
      RequireSequence(itemizationModes, ItemizationModes, "$.itemization_modes");
      RequireSequence(orderingModes, OrderingModes, "$.ordering_modes");

      var combinations = new List<ModeCombination>();
      JsonElement combinationsElement = Array(Property(root, "mode_combinations", "$"), "$.mode_combinations");
      int combinationIndex = 0;
      foreach (JsonElement item in combinationsElement.EnumerateArray())
      {
        string path = "$.mode_combinations[" + combinationIndex + "]";
        ExactFields(
          item,
          path,
          "itemization",
          "ordering",
          "semantic_id",
          "permanent",
          "semantic_snapshot_deterministic",
          "experiential_prefix_stable",
          "planned_series_prefix_stable",
          "semantic_series_isolated",
          "aggregate_semantics_presentation_independent");
        combinations.Add(new ModeCombination(
          String(item, "itemization", path),
          String(item, "ordering", path),
          String(item, "semantic_id", path),
          Boolean(item, "permanent", path),
          Boolean(item, "semantic_snapshot_deterministic", path),
          Boolean(item, "experiential_prefix_stable", path),
          Boolean(item, "planned_series_prefix_stable", path),
          Boolean(item, "semantic_series_isolated", path),
          Boolean(item, "aggregate_semantics_presentation_independent", path)));
        combinationIndex++;
      }
      if (!combinations.SequenceEqual(ModeCombinations))
        throw Error("$.mode_combinations must equal the frozen v2.0 value");

      JsonElement geometry = Property(root, "geometry", "$");
      ExactFields(
        geometry,
        "$.geometry",
        "base",
        "file_ladder",
        "rank_ladder",
        "stage_order",
        "valid_pairs",
        "unlocks",
        "pawn_capacity_formula");
      JsonElement baseElement = Property(geometry, "base", "$.geometry");
      ExactFields(baseElement, "$.geometry.base", "files", "ranks");
      var baseGeometry = new Geometry(
        Integer(baseElement, "files", "$.geometry.base", 1),
        Integer(baseElement, "ranks", "$.geometry.base", 1));
      IReadOnlyList<int> fileLadder = IntegerArray(
        Property(geometry, "file_ladder", "$.geometry"), "$.geometry.file_ladder");
      IReadOnlyList<int> rankLadder = IntegerArray(
        Property(geometry, "rank_ladder", "$.geometry"), "$.geometry.rank_ladder");
      ValidateLadder(fileLadder, "$.geometry.file_ladder");
      ValidateLadder(rankLadder, "$.geometry.rank_ladder");
      if (baseGeometry.Files != fileLadder[0] || baseGeometry.Ranks != rankLadder[0])
        throw Error("$.geometry.base must use the first file and rank ladder values");
      RequireSequence(fileLadder, FileLadder, "$.geometry.file_ladder");
      RequireSequence(rankLadder, RankLadder, "$.geometry.rank_ladder");

      IReadOnlyList<string> stageOrder = StringArray(
        Property(geometry, "stage_order", "$.geometry"), "$.geometry.stage_order");
      var stages = new List<GeometryStage>();
      var seenPairs = new HashSet<string>(StringComparer.Ordinal);
      JsonElement stagesElement = Array(Property(geometry, "valid_pairs", "$.geometry"), "$.geometry.valid_pairs");
      int stageIndex = 0;
      foreach (JsonElement item in stagesElement.EnumerateArray())
      {
        string path = "$.geometry.valid_pairs[" + stageIndex + "]";
        ExactFields(item, path,
          "stage_id", "files", "ranks", "deployment_depth", "combined_non_primary_capacity",
          "gross_pawn_capacity", "forwardness_capacity", "non_pawn_capacity",
          "cpu_pawn_count", "cpu_non_king_count", "all_tactics_locations",
          "turns_locations", "no_tactics_locations");
        var stage = new GeometryStage(
          String(item, "stage_id", path),
          Integer(item, "files", path, 1),
          Integer(item, "ranks", path, 1),
          Integer(item, "deployment_depth", path, 1),
          Integer(item, "combined_non_primary_capacity", path, 1),
          Integer(item, "gross_pawn_capacity", path, 1),
          Integer(item, "forwardness_capacity", path, 1),
          Integer(item, "non_pawn_capacity", path, 1),
          Integer(item, "cpu_pawn_count", path, 1),
          Integer(item, "cpu_non_king_count", path, 1),
          Integer(item, "all_tactics_locations", path, 1),
          Integer(item, "turns_locations", path, 1),
          Integer(item, "no_tactics_locations", path, 1));

        Match match = StagePattern.Match(stage.StageId);
        if (!match.Success ||
            int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) != stage.Files ||
            int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) != stage.Ranks)
        {
          throw Error(path + ".stage_id must be canonical filesxranks");
        }
        if (!fileLadder.Contains(stage.Files) || !rankLadder.Contains(stage.Ranks))
          throw Error(path + " geometry is outside the declared ladders");
        string pairKey = stage.Files + "x" + stage.Ranks;
        if (!seenPairs.Add(pairKey))
          throw Error("$.geometry.valid_pairs contains duplicate geometries");

        int deploymentDepth = stage.Ranks - 3;
        int expectedLocations = 7 * stage.Files + 15 + stageIndex;
        int[] expected =
        {
          deploymentDepth,
          stage.Files * deploymentDepth - 1,
          stage.Files * (deploymentDepth - 1),
          stage.Files * (deploymentDepth - 2),
          stage.Files * (stage.Ranks - 6) - 1,
          stage.Files,
          stage.Files - 1,
          expectedLocations,
          expectedLocations - 6,
          expectedLocations - 10,
        };
        int[] actual =
        {
          stage.DeploymentDepth,
          stage.CombinedNonPrimaryCapacity,
          stage.GrossPawnCapacity,
          stage.ForwardnessCapacity,
          stage.NonPawnCapacity,
          stage.CpuPawnCount,
          stage.CpuNonKingCount,
          stage.AllTacticsLocations,
          stage.TurnsLocations,
          stage.NoTacticsLocations,
        };
        if (!actual.SequenceEqual(expected))
          throw Error(path + " does not match expanded-formation-v2");
        stages.Add(stage);
        stageIndex++;
      }
      RequireSequence(stages.Select(stage => stage.StageId).ToArray(), stageOrder, "$.geometry.stage_order");
      RequireSequence(stageOrder, StageOrder, "$.geometry.stage_order");
      if (stages.Count == 0 || stages[0].StageId != baseGeometry.StageId)
        throw Error("$.geometry.valid_pairs must begin with the base geometry");
      for (int index = 1; index < stages.Count; index++)
      {
        int fileDelta = stages[index].Files - stages[index - 1].Files;
        int rankDelta = stages[index].Ranks - stages[index - 1].Ranks;
        if (!((fileDelta == 2 && rankDelta == 0) || (fileDelta == 0 && rankDelta == 2)))
          throw Error("$.geometry.stage_order must advance one two-unit axis per stage");
      }

      JsonElement unlocksElement = Property(geometry, "unlocks", "$.geometry");
      ExactFields(unlocksElement, "$.geometry.unlocks", "roles", "selection_policy");
      var unlockRoles = new List<GeometryUnlockRole>();
      JsonElement unlockRolesElement = Array(
        Property(unlocksElement, "roles", "$.geometry.unlocks"),
        "$.geometry.unlocks.roles");
      int unlockIndex = 0;
      foreach (JsonElement item in unlockRolesElement.EnumerateArray())
      {
        string path = "$.geometry.unlocks.roles[" + unlockIndex + "]";
        ExactFields(item, path, "role_id", "base", "increment", "maximum");
        unlockRoles.Add(new GeometryUnlockRole(
          String(item, "role_id", path),
          Integer(item, "base", path, 1),
          Integer(item, "increment", path, 1),
          Integer(item, "maximum", path, 1)));
        unlockIndex++;
      }
      var geometryUnlocks = new GeometryUnlocks(
        new ReadOnlyCollection<GeometryUnlockRole>(unlockRoles),
        String(unlocksElement, "selection_policy", "$.geometry.unlocks"));
      if (geometryUnlocks.SelectionPolicy != "largest-componentwise-unlocked-valid-pair" ||
          geometryUnlocks.Roles.Count != 2 ||
          geometryUnlocks.Roles[0] != new GeometryUnlockRole("board-file-unlock", 8, 2, 12) ||
          geometryUnlocks.Roles[1] != new GeometryUnlockRole("board-rank-unlock", 8, 2, 12))
      {
        throw Error("$.geometry.unlocks must equal the frozen v2.0 value");
      }

      JsonElement formulaElement = Property(geometry, "pawn_capacity_formula", "$.geometry");
      ExactFields(
        formulaElement,
        "$.geometry.pawn_capacity_formula",
        "gross_pawn_capacity_algorithm",
        "non_pawns_beyond_back_algorithm",
        "active_pawn_capacity_algorithm",
        "back_rank_primary_royal_slots");
      var pawnCapacityFormula = new PawnCapacityFormula(
        String(
          formulaElement,
          "gross_pawn_capacity_algorithm",
          "$.geometry.pawn_capacity_formula"),
        String(
          formulaElement,
          "non_pawns_beyond_back_algorithm",
          "$.geometry.pawn_capacity_formula"),
        String(
          formulaElement,
          "active_pawn_capacity_algorithm",
          "$.geometry.pawn_capacity_formula"),
        Integer(
          formulaElement,
          "back_rank_primary_royal_slots",
          "$.geometry.pawn_capacity_formula",
          1));
      RequireEqual(
        pawnCapacityFormula,
        new PawnCapacityFormula(
          "width-times-ranks-minus-four-v1",
          "max-zero-n-minus-width-minus-one-v1",
          "gross-minus-non-pawns-beyond-back-v1",
          1),
        "$.geometry.pawn_capacity_formula");

      IReadOnlyDictionary<string, string> algorithms = StringMap(
        Property(root, "algorithms", "$"), "$.algorithms");
      RequireMap(algorithms, Algorithms, "$.algorithms");
      IReadOnlyDictionary<string, int> expectedMaterial = IntegerMap(
        Property(root, "expected_material", "$"), "$.expected_material");
      RequireMap(expectedMaterial, ExpectedMaterial, "$.expected_material");
      IReadOnlyList<string> sourceRoles = StringArray(
        Property(root, "source_roles", "$"), "$.source_roles");
      RequireSequence(sourceRoles, SourceRoles, "$.source_roles");

      JsonElement upgradeElement = Property(root, "upgrade_dag", "$");
      ExactFields(
        upgradeElement,
        "$.upgrade_dag",
        "final_families",
        "pawn_creation_actions",
        "pawn_creation_source_role",
        "transitions");
      IReadOnlyList<string> finalFamilies = StringArray(
        Property(upgradeElement, "final_families", "$.upgrade_dag"),
        "$.upgrade_dag.final_families");
      IReadOnlyList<string> pawnCreationActions = StringArray(
        Property(upgradeElement, "pawn_creation_actions", "$.upgrade_dag"),
        "$.upgrade_dag.pawn_creation_actions");
      string pawnCreationSourceRole = String(
        upgradeElement,
        "pawn_creation_source_role",
        "$.upgrade_dag");
      RequireSequence(finalFamilies, FinalFamilies, "$.upgrade_dag.final_families");
      RequireSequence(
        pawnCreationActions,
        PawnCreationActions,
        "$.upgrade_dag.pawn_creation_actions");
      RequireEqual(
        pawnCreationSourceRole,
        "pawn-slot",
        "$.upgrade_dag.pawn_creation_source_role");

      var transitions = new List<UpgradeTransition>();
      JsonElement transitionsElement = Array(
        Property(upgradeElement, "transitions", "$.upgrade_dag"),
        "$.upgrade_dag.transitions");
      int transitionIndex = 0;
      foreach (JsonElement item in transitionsElement.EnumerateArray())
      {
        string path = "$.upgrade_dag.transitions[" + transitionIndex + "]";
        ExactFields(
          item,
          path,
          "action",
          "from_family",
          "to_family",
          "allowed_source_roles",
          "source_role_rule");
        var transition = new UpgradeTransition(
          String(item, "action", path),
          String(item, "from_family", path),
          String(item, "to_family", path),
          StringArray(Property(item, "allowed_source_roles", path), path + ".allowed_source_roles"),
          String(item, "source_role_rule", path));
        if (transitionIndex >= UpgradeTransitions.Length ||
            !TransitionEquals(transition, UpgradeTransitions[transitionIndex]))
        {
          throw Error("$.upgrade_dag.transitions must equal the frozen v2.0 value");
        }
        transitions.Add(transition);
        transitionIndex++;
      }
      if (transitionIndex != UpgradeTransitions.Length)
        throw Error("$.upgrade_dag.transitions must equal the frozen v2.0 value");
      var upgradeDag = new UpgradeDag(
        finalFamilies,
        pawnCreationActions,
        pawnCreationSourceRole,
        new ReadOnlyCollection<UpgradeTransition>(transitions));

      JsonElement castlerElement = Property(root, "castler", "$");
      ExactFields(castlerElement, "$.castler",
        "source_role", "reclassifies_existing_chessman", "source_role_immutable",
        "normalized_material", "normalized_cost", "maximum",
        "adds_chessman", "requires_existing_chessman", "occupies_board_slot",
        "target_families", "upgrade_ceiling", "protected_from_higher_upgrades",
        "ordinary_omission_protection", "castling_eligibility");
      var castler = new CastlerSemantics(
        String(castlerElement, "source_role", "$.castler"),
        Boolean(castlerElement, "reclassifies_existing_chessman", "$.castler"),
        Boolean(castlerElement, "source_role_immutable", "$.castler"),
        Integer(castlerElement, "normalized_material", "$.castler"),
        Integer(castlerElement, "normalized_cost", "$.castler"),
        Integer(castlerElement, "maximum", "$.castler"),
        Boolean(castlerElement, "adds_chessman", "$.castler"),
        Boolean(castlerElement, "requires_existing_chessman", "$.castler"),
        Boolean(castlerElement, "occupies_board_slot", "$.castler"),
        StringArray(Property(castlerElement, "target_families", "$.castler"), "$.castler.target_families"),
        String(castlerElement, "upgrade_ceiling", "$.castler"),
        Boolean(castlerElement, "protected_from_higher_upgrades", "$.castler"),
        String(castlerElement, "ordinary_omission_protection", "$.castler"),
        String(castlerElement, "castling_eligibility", "$.castler"));
      if (castler.SourceRole != "locked-castler" ||
          !castler.ReclassifiesExistingChessman ||
          !castler.SourceRoleImmutable ||
          castler.NormalizedMaterial != 500 ||
          castler.NormalizedCost != 500 ||
          castler.Maximum != 2 ||
          castler.AddsChessman ||
          !castler.RequiresExistingChessman ||
          !castler.OccupiesBoardSlot ||
          !castler.TargetFamilies.SequenceEqual(new[] { "major", "jack" }) ||
          castler.UpgradeCeiling != "jack" ||
          !castler.ProtectedFromHigherUpgrades ||
          castler.OrdinaryOmissionProtection != "after-additional-royals-within-back-rank-capacity" ||
          castler.CastlingEligibility != "any-active-home-rank-major-or-jack")
      {
        throw Error("$.castler must equal the frozen v2.0 value");
      }

      IReadOnlyList<string> semanticSeriesIds = StringArray(
        Property(root, "semantic_series_ids", "$"), "$.semantic_series_ids");
      IReadOnlyList<string> presentationSeriesIds = StringArray(
        Property(root, "presentation_series_ids", "$"), "$.presentation_series_ids");
      RequireSequence(semanticSeriesIds, SemanticSeriesIds, "$.semantic_series_ids");
      RequireSequence(presentationSeriesIds, PresentationSeriesIds, "$.presentation_series_ids");

      JsonElement overflowElement = Property(root, "overflow_policy", "$");
      ExactFields(overflowElement, "$.overflow_policy",
        "id", "role_priority", "within_role_activation_order", "reserve_entry_order",
        "missing_material_accounting", "material_first_activation",
        "larger_geometry_reactivates_reserves", "ordinary_omission_protected_roles",
        "aggregate_semantics_ignore_presentation", "chaos_nonce_scope");
      var overflow = new OverflowPolicy(
        String(overflowElement, "id", "$.overflow_policy"),
        StringArray(Property(overflowElement, "role_priority", "$.overflow_policy"), "$.overflow_policy.role_priority"),
        StringArray(
          Property(overflowElement, "within_role_activation_order", "$.overflow_policy"),
          "$.overflow_policy.within_role_activation_order"),
        StringArray(
          Property(overflowElement, "reserve_entry_order", "$.overflow_policy"),
          "$.overflow_policy.reserve_entry_order"),
        String(overflowElement, "missing_material_accounting", "$.overflow_policy"),
        Boolean(overflowElement, "material_first_activation", "$.overflow_policy"),
        Boolean(overflowElement, "larger_geometry_reactivates_reserves", "$.overflow_policy"),
        StringArray(
          Property(overflowElement, "ordinary_omission_protected_roles", "$.overflow_policy"),
          "$.overflow_policy.ordinary_omission_protected_roles"),
        Boolean(overflowElement, "aggregate_semantics_ignore_presentation", "$.overflow_policy"),
        String(overflowElement, "chaos_nonce_scope", "$.overflow_policy"));
      if (overflow.Id != "material-first-reserve-v2" ||
          !overflow.RolePriority.SequenceEqual(SourceRoles) ||
          !overflow.WithinRoleActivationOrder.SequenceEqual(new[]
          {
            "final-expected-material-descending",
            "ap-granted-material-descending",
            "source-ordinal-ascending",
          }) ||
          !overflow.ReserveEntryOrder.SequenceEqual(new[]
          {
            "final-expected-material-ascending",
            "ap-granted-material-ascending",
            "source-ordinal-descending",
          }) ||
          overflow.MissingMaterialAccounting !=
            "normalized-ap-granted-material-per-reserve-slot-exactly-once" ||
          !overflow.MaterialFirstActivation ||
          !overflow.LargerGeometryReactivatesReserves ||
          !overflow.OrdinaryOmissionProtectedRoles.SequenceEqual(
            new[] { "primary-royal", "additional-royal", "locked-castler" }) ||
          !overflow.AggregateSemanticsIgnorePresentation ||
          overflow.ChaosNonceScope != "presentation-only")
      {
        throw Error("$.overflow_policy must equal the frozen v2.0 value");
      }

      JsonElement cpuElement = Property(root, "cpu_profiles", "$");
      ExactFields(cpuElement, "$.cpu_profiles", "layout_version", "location_profile_version", "army_ids");
      var cpuProfiles = new CpuProfiles(
        String(cpuElement, "layout_version", "$.cpu_profiles"),
        String(cpuElement, "location_profile_version", "$.cpu_profiles"),
        StringArray(Property(cpuElement, "army_ids", "$.cpu_profiles"), "$.cpu_profiles.army_ids"));
      RequireEqual(cpuProfiles.LayoutVersion, "apmw-cpu-layout-v1", "$.cpu_profiles.layout_version");
      RequireEqual(cpuProfiles.LocationProfileVersion, "apmw-location-profile-v1", "$.cpu_profiles.location_profile_version");
      RequireSequence(
        cpuProfiles.ArmyIds,
        new[] { "standard", "colourbound-clobberers", "remarkable-rookies", "nutty-knights" },
        "$.cpu_profiles.army_ids");

      JsonElement maximaElement = Property(root, "effective_item_maxima", "$");
      ExactFields(maximaElement, "$.effective_item_maxima", "common", "legacy", "fundamental");
      var maxima = new Dictionary<string, IReadOnlyDictionary<string, int>>(StringComparer.Ordinal)
      {
        { "common", IntegerMap(Property(maximaElement, "common", "$.effective_item_maxima"), "$.effective_item_maxima.common") },
        { "legacy", IntegerMap(Property(maximaElement, "legacy", "$.effective_item_maxima"), "$.effective_item_maxima.legacy") },
        { "fundamental", IntegerMap(Property(maximaElement, "fundamental", "$.effective_item_maxima"), "$.effective_item_maxima.fundamental") },
      };
      RequireMap(maxima["common"], CommonMaxima, "$.effective_item_maxima.common");
      RequireMap(maxima["legacy"], LegacyMaxima, "$.effective_item_maxima.legacy");
      RequireMap(maxima["fundamental"], FundamentalMaxima, "$.effective_item_maxima.fundamental");
      if (maxima["fundamental"]["Chessmen"] != stages.Max(stage => stage.CombinedNonPrimaryCapacity))
        throw Error("Chessmen maximum must equal the largest geometry capacity");
      if (maxima["fundamental"]["Material"] != 3 * maxima["fundamental"]["Chessmen"])
        throw Error("Material maximum must be three times the Chessmen maximum");

      string computedHash = ComputeManifestSha256(json);
      if (!string.Equals(computedHash, manifestHash, StringComparison.Ordinal))
      {
        throw Error(
          "manifest SHA-256 mismatch: embedded " + manifestHash + ", computed " + computedHash);
      }

      return new ApmwContractV2(
        version,
        manifestHash,
        minimumClientVersion,
        minorCompatibility,
        itemizationModes,
        orderingModes,
        new ReadOnlyCollection<ModeCombination>(combinations),
        baseGeometry,
        fileLadder,
        rankLadder,
        stageOrder,
        new ReadOnlyCollection<GeometryStage>(stages),
        geometryUnlocks,
        pawnCapacityFormula,
        algorithms,
        expectedMaterial,
        sourceRoles,
        upgradeDag,
        castler,
        semanticSeriesIds,
        presentationSeriesIds,
        overflow,
        cpuProfiles,
        new ReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>(maxima));
    }

    public static string ComputeManifestSha256(string json)
    {
      if (json == null)
        throw new ArgumentNullException(nameof(json));
      using JsonDocument document = ParseDocument(json);
      if (document.RootElement.ValueKind != JsonValueKind.Object)
        throw Error("manifest root must be an object");
      bool hasHash = document.RootElement.EnumerateObject().Any(
        property => property.NameEquals("manifest_sha256"));
      if (!hasHash)
        throw Error("manifest_sha256 is required for canonical hashing");
      var canonical = new StringBuilder();
      AppendCanonical(document.RootElement, canonical, "$", true);
      byte[] digest = SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString()));
      return Convert.ToHexString(digest).ToLowerInvariant();
    }

    private static JsonDocument ParseDocument(string json)
    {
      try
      {
        return JsonDocument.Parse(json, new JsonDocumentOptions
        {
          AllowTrailingCommas = false,
          CommentHandling = JsonCommentHandling.Disallow,
        });
      }
      catch (JsonException exception)
      {
        throw Error("invalid JSON: " + exception.Message, exception);
      }
    }

    private static void AppendCanonical(
      JsonElement value,
      StringBuilder output,
      string path,
      bool isRoot)
    {
      switch (value.ValueKind)
      {
        case JsonValueKind.Object:
          var names = new HashSet<string>(StringComparer.Ordinal);
          var properties = new List<JsonProperty>();
          foreach (JsonProperty property in value.EnumerateObject())
          {
            ValidateAscii(property.Name, path + ".<key>", true);
            if (!names.Add(property.Name))
              throw Error("duplicate JSON property: " + property.Name);
            properties.Add(property);
          }
          properties.Sort((left, right) => StringComparer.Ordinal.Compare(left.Name, right.Name));
          output.Append('{');
          for (int index = 0; index < properties.Count; index++)
          {
            if (index > 0)
              output.Append(',');
            AppendString(properties[index].Name, output);
            output.Append(':');
            if (isRoot && properties[index].NameEquals("manifest_sha256"))
              output.Append("\"\"");
            else
              AppendCanonical(properties[index].Value, output, path + "." + properties[index].Name, false);
          }
          output.Append('}');
          return;

        case JsonValueKind.Array:
          output.Append('[');
          int arrayIndex = 0;
          foreach (JsonElement item in value.EnumerateArray())
          {
            if (arrayIndex > 0)
              output.Append(',');
            AppendCanonical(item, output, path + "[" + arrayIndex + "]", false);
            arrayIndex++;
          }
          output.Append(']');
          return;

        case JsonValueKind.String:
          string text = value.GetString();
          ValidateAscii(text, path, false);
          AppendString(text, output);
          return;

        case JsonValueKind.Number:
          long integer;
          if (!value.TryGetInt64(out integer))
            throw Error(path + " must be a signed 64-bit integer");
          output.Append(integer.ToString(CultureInfo.InvariantCulture));
          return;

        case JsonValueKind.True:
          output.Append("true");
          return;

        case JsonValueKind.False:
          output.Append("false");
          return;

        default:
          throw Error(path + " contains unsupported JSON value " + value.ValueKind);
      }
    }

    private static void AppendString(string value, StringBuilder output)
    {
      output.Append('"');
      foreach (char character in value)
      {
        if (character == '"' || character == '\\')
          output.Append('\\');
        output.Append(character);
      }
      output.Append('"');
    }

    private static void ValidateAscii(string value, string path, bool allowEmpty)
    {
      if (value == null || (!allowEmpty && value.Length == 0))
        throw Error(path + " must not be empty");
      foreach (char character in value)
      {
        if (character < 0x20 || character > 0x7E)
          throw Error(path + " must be printable ASCII");
      }
    }

    private static JsonElement Property(JsonElement element, string name, string path)
    {
      JsonElement value;
      if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out value))
        throw Error(path + "." + name + " is required");
      return value;
    }

    private static JsonElement Array(JsonElement element, string path)
    {
      if (element.ValueKind != JsonValueKind.Array)
        throw Error(path + " must be an array");
      return element;
    }

    private static string String(JsonElement element, string name, string path)
    {
      JsonElement value = Property(element, name, path);
      if (value.ValueKind != JsonValueKind.String)
        throw Error(path + "." + name + " must be a string");
      string result = value.GetString();
      ValidateAscii(result, path + "." + name, false);
      return result;
    }

    private static int Integer(JsonElement element, string name, string path, int minimum = 0)
    {
      JsonElement value = Property(element, name, path);
      int result;
      if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out result))
        throw Error(path + "." + name + " must be a 32-bit integer");
      if (result < minimum)
        throw Error(path + "." + name + " must be at least " + minimum);
      return result;
    }

    private static bool Boolean(JsonElement element, string name, string path)
    {
      JsonElement value = Property(element, name, path);
      if (value.ValueKind == JsonValueKind.True)
        return true;
      if (value.ValueKind == JsonValueKind.False)
        return false;
      throw Error(path + "." + name + " must be a boolean");
    }

    private static IReadOnlyList<string> StringArray(JsonElement element, string path)
    {
      Array(element, path);
      var values = new List<string>();
      var seen = new HashSet<string>(StringComparer.Ordinal);
      int index = 0;
      foreach (JsonElement item in element.EnumerateArray())
      {
        if (item.ValueKind != JsonValueKind.String)
          throw Error(path + "[" + index + "] must be a string");
        string value = item.GetString();
        ValidateAscii(value, path + "[" + index + "]", false);
        if (!seen.Add(value))
          throw Error(path + " contains duplicates");
        values.Add(value);
        index++;
      }
      return new ReadOnlyCollection<string>(values);
    }

    private static IReadOnlyList<int> IntegerArray(JsonElement element, string path)
    {
      Array(element, path);
      var values = new List<int>();
      int index = 0;
      foreach (JsonElement item in element.EnumerateArray())
      {
        int value;
        if (item.ValueKind != JsonValueKind.Number || !item.TryGetInt32(out value) || value < 1)
          throw Error(path + "[" + index + "] must be a positive 32-bit integer");
        values.Add(value);
        index++;
      }
      return new ReadOnlyCollection<int>(values);
    }

    private static IReadOnlyDictionary<string, string> StringMap(JsonElement element, string path)
    {
      if (element.ValueKind != JsonValueKind.Object)
        throw Error(path + " must be an object");
      var values = new Dictionary<string, string>(StringComparer.Ordinal);
      foreach (JsonProperty property in element.EnumerateObject())
      {
        ValidateAscii(property.Name, path + ".<key>", false);
        if (property.Value.ValueKind != JsonValueKind.String)
          throw Error(path + "." + property.Name + " must be a string");
        string value = property.Value.GetString();
        ValidateAscii(value, path + "." + property.Name, false);
        if (!values.TryAdd(property.Name, value))
          throw Error("duplicate JSON property: " + property.Name);
      }
      return ReadOnly(values);
    }

    private static IReadOnlyDictionary<string, int> IntegerMap(JsonElement element, string path)
    {
      if (element.ValueKind != JsonValueKind.Object)
        throw Error(path + " must be an object");
      var values = new Dictionary<string, int>(StringComparer.Ordinal);
      foreach (JsonProperty property in element.EnumerateObject())
      {
        ValidateAscii(property.Name, path + ".<key>", false);
        int value;
        if (property.Value.ValueKind != JsonValueKind.Number ||
            !property.Value.TryGetInt32(out value) ||
            value < 0)
        {
          throw Error(path + "." + property.Name + " must be a non-negative 32-bit integer");
        }
        if (!values.TryAdd(property.Name, value))
          throw Error("duplicate JSON property: " + property.Name);
      }
      return ReadOnly(values);
    }

    private static void ExactFields(JsonElement element, string path, params string[] expected)
    {
      if (element.ValueKind != JsonValueKind.Object)
        throw Error(path + " must be an object");
      var actual = new HashSet<string>(StringComparer.Ordinal);
      foreach (JsonProperty property in element.EnumerateObject())
      {
        ValidateAscii(property.Name, path + ".<key>", false);
        if (!actual.Add(property.Name))
          throw Error("duplicate JSON property: " + property.Name);
      }
      var expectedSet = new HashSet<string>(expected, StringComparer.Ordinal);
      if (!actual.SetEquals(expectedSet))
      {
        string missing = string.Join(",", expectedSet.Except(actual).OrderBy(value => value, StringComparer.Ordinal));
        string unknown = string.Join(",", actual.Except(expectedSet).OrderBy(value => value, StringComparer.Ordinal));
        throw Error(path + " fields differ; missing=[" + missing + "], unknown=[" + unknown + "]");
      }
    }

    private static void ValidateLadder(IReadOnlyList<int> ladder, string path)
    {
      if (ladder.Count == 0)
        throw Error(path + " must not be empty");
      for (int index = 0; index < ladder.Count; index++)
      {
        if (ladder[index] % 2 != 0 || (index > 0 && ladder[index - 1] >= ladder[index]))
          throw Error(path + " must be a strictly increasing ladder of even values");
      }
    }

    private static void RequireSequence<T>(
      IEnumerable<T> actual,
      IEnumerable<T> expected,
      string path)
    {
      if (!actual.SequenceEqual(expected))
        throw Error(path + " must equal the frozen v2.0 value");
    }

    private static bool TransitionEquals(UpgradeTransition actual, UpgradeTransition expected)
    {
      return actual.Action == expected.Action &&
        actual.FromFamily == expected.FromFamily &&
        actual.ToFamily == expected.ToFamily &&
        actual.AllowedSourceRoles.SequenceEqual(expected.AllowedSourceRoles) &&
        actual.SourceRoleRule == expected.SourceRoleRule;
    }

    private static void RequireMap<T>(
      IReadOnlyDictionary<string, T> actual,
      IReadOnlyDictionary<string, T> expected,
      string path)
    {
      if (actual.Count != expected.Count ||
          expected.Any(pair =>
            !actual.TryGetValue(pair.Key, out T value) ||
            !EqualityComparer<T>.Default.Equals(value, pair.Value)))
      {
        throw Error(path + " must equal the frozen v2.0 value");
      }
    }

    private static void RequireEqual<T>(T actual, T expected, string path)
    {
      if (!EqualityComparer<T>.Default.Equals(actual, expected))
        throw Error(path + " must equal the frozen v2.0 value");
    }

    private static IReadOnlyDictionary<string, T> ReadOnly<T>(Dictionary<string, T> dictionary)
    {
      return new ReadOnlyDictionary<string, T>(dictionary);
    }

    private static ApmwContractException Error(string message, Exception innerException = null)
    {
      return new ApmwContractException(message, innerException);
    }
  }

  public sealed class ApmwContractException : FormatException
  {
    public ApmwContractException(string message, Exception innerException = null)
      : base(message, innerException)
    {
    }
  }

  public sealed record ContractVersion(int Major, int Minor);

  public sealed record Geometry(int Files, int Ranks)
  {
    public string StageId { get { return Files + "x" + Ranks; } }
  }

  public sealed record GeometryStage(
    string StageId,
    int Files,
    int Ranks,
    int DeploymentDepth,
    int CombinedNonPrimaryCapacity,
    int GrossPawnCapacity,
    int ForwardnessCapacity,
    int NonPawnCapacity,
    int CpuPawnCount,
    int CpuNonKingCount,
    int AllTacticsLocations,
    int TurnsLocations,
    int NoTacticsLocations);

  public sealed record ModeCombination(
    string Itemization,
    string Ordering,
    string SemanticId,
    bool Permanent,
    bool SemanticSnapshotDeterministic,
    bool ExperientialPrefixStable,
    bool PlannedSeriesPrefixStable,
    bool SemanticSeriesIsolated,
    bool AggregateSemanticsPresentationIndependent);

  public sealed record GeometryUnlockRole(
    string RoleId,
    int Base,
    int Increment,
    int Maximum);

  public sealed record GeometryUnlocks(
    IReadOnlyList<GeometryUnlockRole> Roles,
    string SelectionPolicy);

  public sealed record PawnCapacityFormula(
    string GrossPawnCapacityAlgorithm,
    string NonPawnsBeyondBackAlgorithm,
    string ActivePawnCapacityAlgorithm,
    int BackRankPrimaryRoyalSlots)
  {
    public int NonPawnsBeyondBack(int width, int activeNonPrimaryNonPawns)
    {
      return Math.Max(0, activeNonPrimaryNonPawns - (width - BackRankPrimaryRoyalSlots));
    }

    public int ActivePawnCapacity(
      int width,
      int grossPawnCapacity,
      int activeNonPrimaryNonPawns)
    {
      return grossPawnCapacity - NonPawnsBeyondBack(width, activeNonPrimaryNonPawns);
    }
  }

  public sealed record UpgradeTransition(
    string Action,
    string FromFamily,
    string ToFamily,
    IReadOnlyList<string> AllowedSourceRoles,
    string SourceRoleRule);

  public sealed record UpgradeDag(
    IReadOnlyList<string> FinalFamilies,
    IReadOnlyList<string> PawnCreationActions,
    string PawnCreationSourceRole,
    IReadOnlyList<UpgradeTransition> Transitions);

  public sealed record CastlerSemantics(
    string SourceRole,
    bool ReclassifiesExistingChessman,
    bool SourceRoleImmutable,
    int NormalizedMaterial,
    int NormalizedCost,
    int Maximum,
    bool AddsChessman,
    bool RequiresExistingChessman,
    bool OccupiesBoardSlot,
    IReadOnlyList<string> TargetFamilies,
    string UpgradeCeiling,
    bool ProtectedFromHigherUpgrades,
    string OrdinaryOmissionProtection,
    string CastlingEligibility);

  public sealed record OverflowPolicy(
    string Id,
    IReadOnlyList<string> RolePriority,
    IReadOnlyList<string> WithinRoleActivationOrder,
    IReadOnlyList<string> ReserveEntryOrder,
    string MissingMaterialAccounting,
    bool MaterialFirstActivation,
    bool LargerGeometryReactivatesReserves,
    IReadOnlyList<string> OrdinaryOmissionProtectedRoles,
    bool AggregateSemanticsIgnorePresentation,
    string ChaosNonceScope);

  public sealed record CpuProfiles(
    string LayoutVersion,
    string LocationProfileVersion,
    IReadOnlyList<string> ArmyIds);

  public sealed class ApmwContractV2
  {
    public ContractVersion Version { get; }
    public string ManifestSha256 { get; }
    public string MinimumClientVersion { get; }
    public string MinorCompatibility { get; }
    public IReadOnlyList<string> ItemizationModes { get; }
    public IReadOnlyList<string> OrderingModes { get; }
    public IReadOnlyList<ModeCombination> ModeCombinations { get; }
    public Geometry BaseGeometry { get; }
    public IReadOnlyList<int> FileLadder { get; }
    public IReadOnlyList<int> RankLadder { get; }
    public IReadOnlyList<string> StageOrder { get; }
    public IReadOnlyList<GeometryStage> Stages { get; }
    public GeometryUnlocks GeometryUnlocks { get; }
    public PawnCapacityFormula PawnCapacityFormula { get; }
    public IReadOnlyDictionary<string, string> Algorithms { get; }
    public IReadOnlyDictionary<string, int> ExpectedMaterial { get; }
    public IReadOnlyList<string> SourceRoles { get; }
    public UpgradeDag UpgradeDag { get; }
    public CastlerSemantics Castler { get; }
    public IReadOnlyList<string> SemanticSeriesIds { get; }
    public IReadOnlyList<string> PresentationSeriesIds { get; }
    public OverflowPolicy OverflowPolicy { get; }
    public CpuProfiles CpuProfiles { get; }
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> EffectiveItemMaxima { get; }

    internal ApmwContractV2(
      ContractVersion version,
      string manifestSha256,
      string minimumClientVersion,
      string minorCompatibility,
      IReadOnlyList<string> itemizationModes,
      IReadOnlyList<string> orderingModes,
      IReadOnlyList<ModeCombination> modeCombinations,
      Geometry baseGeometry,
      IReadOnlyList<int> fileLadder,
      IReadOnlyList<int> rankLadder,
      IReadOnlyList<string> stageOrder,
      IReadOnlyList<GeometryStage> stages,
      GeometryUnlocks geometryUnlocks,
      PawnCapacityFormula pawnCapacityFormula,
      IReadOnlyDictionary<string, string> algorithms,
      IReadOnlyDictionary<string, int> expectedMaterial,
      IReadOnlyList<string> sourceRoles,
      UpgradeDag upgradeDag,
      CastlerSemantics castler,
      IReadOnlyList<string> semanticSeriesIds,
      IReadOnlyList<string> presentationSeriesIds,
      OverflowPolicy overflowPolicy,
      CpuProfiles cpuProfiles,
      IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> effectiveItemMaxima)
    {
      Version = version;
      ManifestSha256 = manifestSha256;
      MinimumClientVersion = minimumClientVersion;
      MinorCompatibility = minorCompatibility;
      ItemizationModes = itemizationModes;
      OrderingModes = orderingModes;
      ModeCombinations = modeCombinations;
      BaseGeometry = baseGeometry;
      FileLadder = fileLadder;
      RankLadder = rankLadder;
      StageOrder = stageOrder;
      Stages = stages;
      GeometryUnlocks = geometryUnlocks;
      PawnCapacityFormula = pawnCapacityFormula;
      Algorithms = algorithms;
      ExpectedMaterial = expectedMaterial;
      SourceRoles = sourceRoles;
      UpgradeDag = upgradeDag;
      Castler = castler;
      SemanticSeriesIds = semanticSeriesIds;
      PresentationSeriesIds = presentationSeriesIds;
      OverflowPolicy = overflowPolicy;
      CpuProfiles = cpuProfiles;
      EffectiveItemMaxima = effectiveItemMaxima;
    }
  }
}
