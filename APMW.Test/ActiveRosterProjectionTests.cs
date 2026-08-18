using Archipelago.APChessV;
using ChessV;
using ChessV.Base;
using ChessV.Games;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ChessV.Test
{
  [TestClass]
  [DoNotParallelize]
  public class ActiveRosterProjectionTests
  {
    [TestInitialize]
    public void Setup()
    {
      Reset();
      ConfigureStable();
      new ApmwChessGame().earlyPopulatePieceTypes();
    }

    [TestCleanup]
    public void Cleanup()
    {
      Reset();
    }

    [TestMethod]
    public void GeometryCapacities_MatchAcceptedContractForEveryStage()
    {
      ApmwContractV2 contract = LoadContract();
      string[] expected =
      {
        "8x8:15:32:39:24",
        "10x8:19:40:49:30",
        "10x10:39:60:69:50",
        "12x10:47:72:83:60",
        "12x12:71:96:107:84",
      };

      CollectionAssert.AreEqual(
        expected,
        contract.Stages.Select(stage =>
        {
          ProjectionGeometry geometry = ProjectionGeometry.FromContractStage(stage);
          Assert.AreEqual(stage.DeploymentDepth, geometry.HumanFormationRanks);
          Assert.AreEqual(stage.NonPawnCapacity, geometry.NonPawnCapacity);
          Assert.AreEqual(stage.GrossPawnCapacity, geometry.GrossPawnCapacity);
          Assert.AreEqual(stage.CombinedNonPrimaryCapacity, geometry.MaximumNonPrimary);
          Assert.AreEqual(stage.ForwardnessCapacity, geometry.NominalForwardness);
          return geometry.StageId + ":" + geometry.NonPawnCapacity + ":" +
            geometry.GrossPawnCapacity + ":" + geometry.MaximumNonPrimary + ":" +
            geometry.NominalForwardness;
        }).ToArray());
    }

    [DataTestMethod]
    [DataRow(6, 8, 11, 18, 29)]
    [DataRow(8, 8, 15, 24, 39)]
    [DataRow(10, 8, 19, 30, 49)]
    [DataRow(10, 10, 39, 30, 69)]
    [DataRow(12, 10, 47, 36, 83)]
    [DataRow(12, 12, 71, 36, 107)]
    public void Projection_ActivatesExactStageCapacity(
      int files,
      int ranks,
      int nonPawns,
      int activePawns,
      int maximumNonPrimary)
    {
      var pieces = new List<RosterPiece>();
      for (int ordinal = 0; ordinal < nonPawns; ordinal++)
        pieces.Add(Piece(SourcePlacementRole.JackSlot, ordinal, FinalPieceFamily.Jack, 700, 700));
      ProjectionGeometry geometry = ProjectionGeometry.For(files, ranks);
      for (int ordinal = 0; ordinal < geometry.GrossPawnCapacity; ordinal++)
        pieces.Add(Piece(SourcePlacementRole.PawnSlot, ordinal, FinalPieceFamily.Pawn, 100, 100));

      ActiveRosterProjection projection = Project(Roster(pieces), files, ranks);

      Assert.AreEqual(nonPawns, projection.ActiveCountsByRole[SourcePlacementRole.JackSlot]);
      Assert.AreEqual(activePawns, projection.ActiveCountsByRole[SourcePlacementRole.PawnSlot]);
      Assert.AreEqual(maximumNonPrimary, projection.ActivePieces.Count);
    }

    [TestMethod]
    public void ProjectionGeometry_RecognizesSixByEightAndRejectsUnsupportedPairs()
    {
      ProjectionGeometry compact = ProjectionGeometry.For(6, 8);

      Assert.AreEqual("6x8", compact.StageId);
      Assert.AreEqual(11, compact.NonPawnCapacity);
      Assert.ThrowsException<ArgumentOutOfRangeException>(
        () => ProjectionGeometry.For(6, 10));
    }

    [TestMethod]
    public void Projection_UsesRolePriorityBeforeMaterialAndRoleLocalMaterialOrdering()
    {
      var pieces = new List<RosterPiece>();
      for (int ordinal = 0; ordinal < 15; ordinal++)
        pieces.Add(Piece(SourcePlacementRole.JackSlot, ordinal, FinalPieceFamily.Jack, 700, 700));
      pieces.Add(Piece(SourcePlacementRole.MajorSlot, 0, FinalPieceFamily.Amazon, 1300, 1300));
      pieces.Add(Piece(SourcePlacementRole.MajorSlot, 1, FinalPieceFamily.Major, 485, 485));
      GeneratedRoster roster = Roster(pieces);

      ActiveRosterProjection projection = Project(roster, 8, 8);

      Assert.AreEqual(15, projection.ActiveCountsByRole[SourcePlacementRole.JackSlot]);
      Assert.AreEqual(0, projection.ActiveCountsByRole[SourcePlacementRole.MajorSlot]);
      Assert.AreEqual(2, projection.ReserveCountsByRole[SourcePlacementRole.MajorSlot]);
      Assert.AreEqual(1785, projection.MissingMaterialByRole[SourcePlacementRole.MajorSlot]);

      var tied = Enumerable.Range(0, 16)
        .Select(ordinal => Piece(
          SourcePlacementRole.MajorSlot,
          ordinal,
          ordinal == 15 ? FinalPieceFamily.Amazon : FinalPieceFamily.Major,
          ordinal == 15 ? 1 : 9999,
          ordinal == 15 ? 1300 : 485))
        .ToList();
      ActiveRosterProjection withinRole = Project(Roster(tied), 8, 8);
      Assert.IsTrue(withinRole.ActivePieces.Any(piece => piece.SourceOrdinal == 15));
      Assert.AreEqual(
        14,
        withinRole.ReservePieces.Single().SourceOrdinal,
        "after the higher-family piece activates, the newest full-tie Major is reserved");

      var grantedTie = Enumerable.Range(0, 16)
        .Select(ordinal => Piece(
          SourcePlacementRole.MajorSlot,
          ordinal,
          FinalPieceFamily.Major,
          485,
          ordinal == 15 ? 600 : 485))
        .ToList();
      ActiveRosterProjection grantedOrdering = Project(Roster(grantedTie), 8, 8);
      Assert.IsTrue(grantedOrdering.ActivePieces.Any(piece => piece.SourceOrdinal == 15));
      Assert.AreEqual(14, grantedOrdering.ReservePieces.Single().SourceOrdinal);
    }

    [TestMethod]
    public void Projection_ProtectsAdditionalRoyalsThenLockedCastlersAndExposesOnlyActiveRights()
    {
      var pieces = new List<RosterPiece>();
      for (int ordinal = 0; ordinal < 6; ordinal++)
        pieces.Add(Piece(SourcePlacementRole.AdditionalRoyal, ordinal, null, 325, 325));
      pieces.Add(Piece(SourcePlacementRole.LockedCastler, 0, FinalPieceFamily.Jack, 700, 700, true));
      pieces.Add(Piece(SourcePlacementRole.LockedCastler, 1, FinalPieceFamily.Jack, 700, 700, true));
      for (int ordinal = 0; ordinal < 12; ordinal++)
        pieces.Add(Piece(SourcePlacementRole.JackSlot, ordinal, FinalPieceFamily.Jack, 700, 700));

      ActiveRosterProjection projection = Project(Roster(pieces), 8, 8);

      Assert.AreEqual(6, projection.ActiveCountsByRole[SourcePlacementRole.AdditionalRoyal]);
      Assert.AreEqual(1, projection.ActiveCountsByRole[SourcePlacementRole.LockedCastler]);
      Assert.AreEqual(1, projection.ReserveCountsByRole[SourcePlacementRole.LockedCastler]);
      Assert.AreEqual(1, projection.ActiveCastlers.Count);
      Assert.AreEqual(1, projection.CastlingRights.Count(right => right.LockedCastler));
      Assert.IsTrue(projection.CastlingRights.All(right =>
        right.IsHomeRankEligible && right.CoordinateRightsResolved));
      Assert.IsFalse(projection.CastlingRights.Any(right =>
        projection.ReservePieces.Any(piece => piece.StableId == right.StableId)));
    }

    [TestMethod]
    public void Projection_DynamicPawnCapacityAndPlacementUsageFollowExpandedFormation()
    {
      var pieces = new List<RosterPiece>();
      for (int ordinal = 0; ordinal < 15; ordinal++)
        pieces.Add(Piece(SourcePlacementRole.JackSlot, ordinal, FinalPieceFamily.Jack, 700, 700));
      for (int ordinal = 0; ordinal < 32; ordinal++)
        pieces.Add(Piece(SourcePlacementRole.PawnSlot, ordinal, FinalPieceFamily.Pawn, 100, 100));

      ActiveRosterProjection projection = Project(Roster(pieces), 8, 8);

      Assert.AreEqual(15, projection.ActiveCountsByRole[SourcePlacementRole.JackSlot]);
      Assert.AreEqual(24, projection.ActiveCountsByRole[SourcePlacementRole.PawnSlot]);
      Assert.AreEqual(8, projection.ReserveCountsByRole[SourcePlacementRole.PawnSlot]);
      Assert.AreEqual(39, projection.ActivePieces.Count);
      CollectionAssert.AreEqual(
        new[] { 8, 8, 8, 8, 8 },
        projection.RegionUsage.Select(usage => usage.Used).ToArray());
      Assert.IsTrue(projection.ActivePieces
        .Where(piece => piece.SourcePlacementRole != SourcePlacementRole.PawnSlot)
        .All(piece => piece.Placement.Region != ProjectionRegion.PawnOnlyBand));
    }

    [TestMethod]
    public void Projection_MaterialAndPromotionLedgersSplitActiveAndReserveExactlyOnce()
    {
      ApmwCore core = ApmwCore.getInstance();
      PieceType activePromotion = core.queens.First();
      PieceType reservePromotion = core.amazons.First();
      var pieces = new List<RosterPiece>();
      for (int ordinal = 0; ordinal < 14; ordinal++)
        pieces.Add(Piece(SourcePlacementRole.JackSlot, ordinal, FinalPieceFamily.Jack, 700, 700));
      pieces.Add(Piece(
        SourcePlacementRole.MajorSlot,
        0,
        FinalPieceFamily.Queen,
        900,
        1000,
        false,
        activePromotion));
      pieces.Add(Piece(
        SourcePlacementRole.MajorSlot,
        1,
        FinalPieceFamily.Major,
        485,
        600,
        false,
        reservePromotion));
      GeneratedRoster roster = Roster(pieces, unallocated: 123);

      ActiveRosterProjection projection = Project(roster, 8, 8);
      ProjectedRosterPiece reserve = projection.ReservePieces.Single();

      Assert.AreEqual(600, projection.MissingMaterial);
      Assert.AreEqual(600, projection.MissingMaterialByRole[SourcePlacementRole.MajorSlot]);
      Assert.AreEqual(600, projection.MissingMaterialByFamily[FinalPieceFamily.Major]);
      Assert.AreEqual(pieces.Sum(piece => piece.FinalExpectedMaterial), projection.OwnedExpectedMaterial);
      Assert.AreEqual(
        projection.ActivePieces.Sum(piece => piece.FinalExpectedMaterial),
        projection.ExactActiveExpectedMaterial);
      Assert.AreEqual(
        roster.OwnedMaterialLedger.GrantedTotal,
        projection.ActivePieces.Sum(piece => piece.GrantedMaterial) +
          projection.MissingMaterial +
          projection.DormantMaterial +
          projection.UnallocatedMaterial);
      CollectionAssert.Contains(
        projection.ActivePromotionCatalog.ToList(),
        activePromotion.Notation[core.GeriProvider()]);
      CollectionAssert.Contains(
        projection.ReservePromotionCatalog.ToList(),
        reservePromotion.Notation[core.GeriProvider()]);
      CollectionAssert.Contains(
        projection.ReserveOnlyPromotionCatalog.ToList(),
        reservePromotion.Notation[core.GeriProvider()]);
      Assert.AreEqual(reserve.StableId, projection.ReservePieces.Single().StableId);

      ApmwGeometryPreview preview = ApmwGeometryPreview.FromProjection(projection);
      Assert.AreEqual("8x8", preview.StageId);
      Assert.AreEqual(16, preview.ActiveCount, "the public count includes the primary King");
      Assert.AreEqual(1, preview.ReserveCount);
      Assert.AreEqual(600, preview.MissingMaterial);
      Assert.AreEqual(0, preview.DormantMaterial);
      Assert.AreEqual(123, preview.UnallocatedMaterial);
      Assert.AreEqual(0, preview.UnspentForwardness);
      Assert.AreEqual(1, preview.ReserveCountsByRole["major-slot"]);
      Assert.AreEqual(1, preview.ReserveCountsByFamily["major"]);
    }

    [TestMethod]
    public void Projection_PlacementIsStableAndUsesOnlyAcceptedPlacementSeries()
    {
      var pieces = new List<RosterPiece>();
      for (int ordinal = 0; ordinal < 12; ordinal++)
        pieces.Add(Piece(SourcePlacementRole.MajorSlot, ordinal, FinalPieceFamily.Major, 485, 485));
      for (int ordinal = 0; ordinal < 20; ordinal++)
        pieces.Add(Piece(SourcePlacementRole.PawnSlot, ordinal, FinalPieceFamily.Pawn, 100, 100));
      GeneratedRoster roster = Roster(pieces);

      ActiveRosterProjection first = Project(roster, 10, 10, forwardness: 7);
      ActiveRosterProjection second = Project(roster, 10, 10, forwardness: 7);

      Assert.AreEqual(ProjectionSignature(first), ProjectionSignature(second));
      Assert.AreEqual(9, first.CastlingRights.Count, "ordinary home-rank Major pieces expose rights");
      string template = LoadContract().PresentationSeriesIds.Single(
        id => id == "placement.{source-role}.{formation-band}");
      string id = GeneratedRosterProjector.PlacementSeriesId(SourcePlacementRole.MajorSlot, "mixed-1");
      Assert.AreEqual(
        template.Replace("{source-role}", "major-slot").Replace("{formation-band}", "mixed-1"),
        id);
    }

    [TestMethod]
    public void Projection_ChaosPresentationDoesNotChangeSemanticMembershipOrMaterial()
    {
      var pieces = new List<RosterPiece>();
      for (int ordinal = 0; ordinal < 20; ordinal++)
        pieces.Add(Piece(SourcePlacementRole.MajorSlot, ordinal, FinalPieceFamily.Major, 485, 485));
      for (int ordinal = 0; ordinal < 40; ordinal++)
        pieces.Add(Piece(SourcePlacementRole.PawnSlot, ordinal, FinalPieceFamily.Pawn, 100, 100));
      GeneratedRoster roster = Roster(pieces);
      ActiveRosterProjection stable = Project(roster, 8, 8, forwardness: 13);

      ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
      builder.PlayerPieceTypes = PieceTypes.Chaos;
      builder.PieceLocations = PieceLocations.Chaos;
      builder.DeterministicChaosSeed = 81237;
      ApmwConfig.getInstance().Instantiate(builder.Build().BuildSlotData());
      ApmwConfig.getInstance().seed();
      ActiveRosterProjection chaos = Project(roster, 8, 8, forwardness: 13);

      Assert.AreEqual(ProjectionSignature(stable), ProjectionSignature(chaos));
      CollectionAssert.AreEquivalent(
        stable.ActivePieces.Select(piece => piece.StableId).ToList(),
        chaos.ActivePieces.Select(piece => piece.StableId).ToList());
      CollectionAssert.AreEquivalent(
        stable.ReservePieces.Select(piece => piece.StableId).ToList(),
        chaos.ReservePieces.Select(piece => piece.StableId).ToList());
      Assert.AreEqual(stable.ExactActiveExpectedMaterial, chaos.ExactActiveExpectedMaterial);
      Assert.AreEqual(stable.MissingMaterial, chaos.MissingMaterial);
    }

    [TestMethod]
    public void Projection_ForwardnessUsesGeneralizedTriangularEdgesAndReportsBlockedAttempts()
    {
      var pieces = Enumerable.Range(0, 8)
        .Select(ordinal => Piece(
          SourcePlacementRole.PawnSlot,
          ordinal,
          FinalPieceFamily.Pawn,
          100,
          100))
        .ToList();

      ActiveRosterProjection projection = Project(Roster(pieces), 8, 8, forwardness: 30);

      Assert.AreEqual(6, projection.UnspentForwardness);
      Assert.IsTrue(projection.ActivePieces.All(piece =>
        piece.Placement.Coordinate.RelativeRank == 4));
      Assert.AreEqual(
        projection.ActivePieces.Count,
        projection.ActivePieces.Select(piece => piece.Placement.Coordinate).Distinct().Count());
    }

    [DataTestMethod]
    [DataRow(8)]
    [DataRow(10)]
    public void Projection_ForwardnessPreservesFullHomePawnRankTriangularOutcome(int files)
    {
      var pieces = Enumerable.Range(0, files)
        .Select(ordinal => Piece(
          SourcePlacementRole.PawnSlot,
          ordinal,
          FinalPieceFamily.Pawn,
          100,
          100))
        .ToList();

      ActiveRosterProjection projection = Project(
        Roster(pieces),
        files,
        8,
        forwardness: files * 3);

      Assert.AreEqual(0, projection.UnspentForwardness);
      Assert.IsTrue(projection.ActivePieces.All(piece =>
        piece.Placement.Coordinate.RelativeRank == 4));
    }

    [TestMethod]
    public void ProjectionFixtureSerialization_PreservesContractReserveOrder()
    {
      using JsonDocument cases = JsonDocument.Parse(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "ProjectionV2", "cases.json")));
      JsonElement fixtureCase = cases.RootElement.GetProperty("cases").EnumerateArray()
        .Single(item => item.GetProperty("id").GetString() == "omission-full-tie-source-ordinal");
      string json = ProjectionFixtureSerializer.Serialize(LoadContract(), fixtureCase.GetProperty("input"));
      using JsonDocument actual = JsonDocument.Parse(json);
      JsonElement reserve = actual.RootElement.GetProperty("reserve_slots");

      CollectionAssert.AreEqual(
        new[]
        {
          "major-slot:000010", "major-slot:000009", "major-slot:000008",
          "major-slot:000007", "major-slot:000006", "major-slot:000005",
          "major-slot:000004",
        },
        reserve.EnumerateArray().Select(item => item.GetProperty("slot_id").GetString()).ToArray());
    }

    private static string ProjectionSignature(ActiveRosterProjection projection)
    {
      return string.Join("|",
        projection.ActivePieces.Select(piece =>
          "A:" + PieceSignature(piece) + "@" + piece.Placement.Coordinate.File + "," +
          piece.Placement.Coordinate.RelativeRank)
          .Concat(projection.ReservePieces.Select(piece => "R:" + PieceSignature(piece)))
          .Concat(projection.RegionUsage.Select(usage =>
            "U:" + usage.BandId + ":" + usage.Capacity + ":" + usage.Used))
          .Concat(projection.ActiveCountsByRole.OrderBy(pair => pair.Key)
            .Select(pair => "AR:" + pair.Key + ":" + pair.Value))
          .Concat(projection.ReserveCountsByRole.OrderBy(pair => pair.Key)
            .Select(pair => "RR:" + pair.Key + ":" + pair.Value))
          .Concat(projection.ActiveCountsByFamily.OrderBy(pair => pair.Key)
            .Select(pair => "AF:" + pair.Key + ":" + pair.Value))
          .Concat(projection.ReserveCountsByFamily.OrderBy(pair => pair.Key)
            .Select(pair => "RF:" + pair.Key + ":" + pair.Value))
          .Concat(projection.CastlingRights.Select(right =>
            "C:" + right.StableId + ":" + right.Coordinate.File + ":" +
            right.Coordinate.RelativeRank))
          .Concat(new[]
          {
            string.Join(",", projection.ActivePromotionFamilyCatalog),
            string.Join(",", projection.ReservePromotionFamilyCatalog),
            string.Join(",", projection.ActiveCastlers),
            projection.PrimaryKingGrantedMaterial.ToString(),
            projection.PrimaryKingExpectedMaterial.ToString(),
            projection.OwnedExpectedMaterial.ToString(),
            projection.ExactActiveExpectedMaterial.ToString(),
            projection.MissingMaterial.ToString(),
            projection.DormantMaterial.ToString(),
            projection.UnallocatedMaterial.ToString(),
            projection.UnspentForwardness.ToString(),
          }));
    }

    private static string PieceSignature(ProjectedRosterPiece piece)
    {
      return string.Join(":",
        piece.StableId,
        piece.SourcePlacementRole,
        piece.SourceOrdinal,
        piece.FinalFamily,
        piece.LockedCastler,
        piece.GrantedMaterial,
        piece.FinalExpectedMaterial,
        string.Join(",", piece.UpgradePath),
        string.Join(",", piece.PromotionEntitlementFamilies));
    }

    private static ActiveRosterProjection Project(
      GeneratedRoster roster,
      int files,
      int ranks,
      int forwardness = 0)
    {
      return GeneratedRosterProjector.Project(
        roster,
        ProjectionGeometry.For(files, ranks),
        forwardness,
        ApmwConfig.getInstance());
    }

    private static GeneratedRoster Roster(IEnumerable<RosterPiece> pieces, int unallocated = 0)
    {
      List<RosterPiece> list = pieces.ToList();
      var ledger = new OwnedMaterialLedger();
      foreach (RosterPiece piece in list)
        ledger.Add(OwnedMaterialCategory.DirectPiece, piece.GrantedMaterial, piece.StableId);
      ledger.Add(OwnedMaterialCategory.Unallocated, unallocated);
      return new GeneratedRoster(
        ApmwCore.getInstance().kings[0],
        list,
        ledger,
        unallocated,
        "projection-test");
    }

    private static RosterPiece Piece(
      SourcePlacementRole role,
      int ordinal,
      FinalPieceFamily? family,
      int expected,
      int granted,
      bool locked = false,
      PieceType concrete = null)
    {
      return new RosterPiece(
        RoleId(role) + ":" + ordinal.ToString("D6"),
        role,
        ordinal,
        "test",
        family,
        concrete,
        locked,
        granted,
        expected);
    }

    private static void ConfigureStable()
    {
      ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
      builder.PlayerPieceTypes = PieceTypes.Stable;
      builder.PieceLocations = PieceLocations.Stable;
      ApmwConfig.getInstance().Instantiate(builder.Build().BuildSlotData());
      ApmwConfig.getInstance().seed();
      ApmwCore.getInstance().GeriProvider = () => 0;
    }

    private static ApmwContractV2 LoadContract()
    {
      string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "ProjectionV2", "baseline.json");
      return ApmwContractV2Parser.Parse(File.ReadAllText(path));
    }

    private static string RoleId(SourcePlacementRole role)
    {
      switch (role)
      {
        case SourcePlacementRole.AdditionalRoyal: return "additional-royal";
        case SourcePlacementRole.LockedCastler: return "locked-castler";
        case SourcePlacementRole.JackSlot: return "jack-slot";
        case SourcePlacementRole.MajorSlot: return "major-slot";
        case SourcePlacementRole.MinorSlot: return "minor-slot";
        case SourcePlacementRole.PawnSlot: return "pawn-slot";
        default: return "primary-royal";
      }
    }

    private static void Reset()
    {
      ApmwCore._instance = null;
      ApmwConfig._instance = null;
    }
  }
}
