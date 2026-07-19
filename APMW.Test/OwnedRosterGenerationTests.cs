using Archipelago.APChessV;
using ChessV.Base;
using ChessV.Games;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ChessV.Test
{
  [TestClass]
  [DoNotParallelize]
  public class OwnedRosterGenerationTests
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
    public void FundamentalOwnershipIsUncappedWhileCurrentBoardPlanningRetainsGeometryCaps()
    {
      ApmwConfig config = ConfigureFundamental(107, 321 * 400, PlannedChain());
      ApmwCore core = ApmwCore.getInstance();

      PieceGenerationAllocation eight = FundamentalSlotGraduationPlanner.Plan(core, config, 8);
      PieceGenerationAllocation ten = FundamentalSlotGraduationPlanner.Plan(core, config, 10);
      PieceGenerationAllocation owned = FundamentalSlotGraduationPlanner.PlanOwnedRoster(core, config);

      Assert.AreEqual(39, TotalSlots(eight), "8-file board path retains its characterized 5W-1 cap");
      Assert.AreEqual(49, TotalSlots(ten), "10-file board path retains its characterized 5W-1 cap");
      Assert.IsTrue(
        Enum.GetValues(typeof(NonPawnPieceFamily)).Cast<NonPawnPieceFamily>()
          .Sum(eight.NonPawnCount) <= 15,
        "8-file board path retains its characterized 2W-1 non-pawn cap");
      Assert.IsTrue(
        Enum.GetValues(typeof(NonPawnPieceFamily)).Cast<NonPawnPieceFamily>()
          .Sum(ten.NonPawnCount) <= 19,
        "10-file board path retains its characterized 2W-1 non-pawn cap");
      Assert.AreEqual(107, TotalSlots(owned));

      GeneratedRoster roster = OwnedRosterGeneration.Generate();
      Assert.AreEqual(107, roster.RosterPieces.Count(piece => piece.SourcePlacementRole != SourcePlacementRole.AdditionalRoyal));
      Assert.AreEqual(
        39,
        PlayerPieceSetGeneration.Generate(8).Item1.Values.Count(piece => !core.kings.Contains(piece)),
        "current 8-file board projection remains capped");
      Assert.AreEqual(
        49,
        PlayerPieceSetGeneration.Generate(10).Item1.Values.Count(piece => !core.kings.Contains(piece)),
        "current 10-file board projection remains capped");
    }

    [TestMethod]
    public void FundamentalRoster_PreservesImmutableRolesAcrossRepresentativeChains()
    {
      ConfigureFundamental(
        6,
        20000,
        JObject.FromObject(new Dictionary<string, int>
        {
          [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 10,
          [ApmwConstants.PieceUpgradeActions.MinorToMajor] = 9,
          [ApmwConstants.PieceUpgradeActions.MajorToJack] = 8,
          [ApmwConstants.PieceUpgradeActions.JackToQueen] = 7,
          [ApmwConstants.PieceUpgradeActions.QueenToAmazon] = 6,
        }));

      GeneratedRoster roster = OwnedRosterGeneration.Generate();
      List<RosterPiece> chessmen = roster.RosterPieces
        .Where(piece => piece.SourcePlacementRole != SourcePlacementRole.AdditionalRoyal)
        .ToList();

      Assert.AreEqual(6, chessmen.Count);
      Assert.IsTrue(chessmen.All(piece => piece.SourcePlacementRole == SourcePlacementRole.MinorSlot));
      Assert.IsTrue(chessmen.All(piece => piece.FinalFamily == FinalPieceFamily.Amazon));
      Assert.IsTrue(chessmen.All(piece =>
        piece.UpgradePath.SequenceEqual(new[]
        {
          ApmwConstants.PieceUpgradeActions.PawnToMinor,
          ApmwConstants.PieceUpgradeActions.MinorToMajor,
          ApmwConstants.PieceUpgradeActions.MajorToJack,
          ApmwConstants.PieceUpgradeActions.JackToQueen,
          ApmwConstants.PieceUpgradeActions.QueenToAmazon,
        })));
    }

    [TestMethod]
    public void FundamentalRoster_DirectPawnToMajorEstablishesMajorRoleAndCastlersStayLocked()
    {
      ConfigureFundamental(
        5,
        10000,
        JObject.FromObject(new Dictionary<string, int>
        {
          [ApmwConstants.PieceUpgradeActions.PawnToMajor] = 10,
          [ApmwConstants.PieceUpgradeActions.MajorToQueen] = 9,
          [ApmwConstants.PieceUpgradeActions.QueenToAmazon] = 8,
        }),
        castlers: 2);

      GeneratedRoster roster = OwnedRosterGeneration.Generate();
      List<RosterPiece> locked = roster.RosterPieces.Where(piece => piece.LockedCastler).ToList();
      List<RosterPiece> ordinary = roster.RosterPieces.Where(piece => !piece.LockedCastler).ToList();

      Assert.AreEqual(2, locked.Count);
      Assert.IsTrue(locked.All(piece =>
        piece.SourcePlacementRole == SourcePlacementRole.LockedCastler &&
        piece.FinalFamily == FinalPieceFamily.Major));
      Assert.IsTrue(ordinary.All(piece =>
        piece.SourcePlacementRole == SourcePlacementRole.MajorSlot &&
        piece.FinalFamily == FinalPieceFamily.Amazon));
    }

    [TestMethod]
    public void FundamentalRoster_LockedCastlerMayReachJackButNeverQueenOrAmazon()
    {
      ConfigureFundamental(
        2,
        1400,
        JObject.FromObject(new Dictionary<string, int>
        {
          [ApmwConstants.PieceUpgradeActions.MajorToJack] = 10,
          [ApmwConstants.PieceUpgradeActions.JackToQueen] = 9,
          [ApmwConstants.PieceUpgradeActions.QueenToAmazon] = 8,
        }),
        castlers: 2);

      GeneratedRoster roster = OwnedRosterGeneration.Generate();

      Assert.AreEqual(2, roster.RosterPieces.Count);
      Assert.IsTrue(roster.RosterPieces.All(piece =>
        piece.LockedCastler &&
        piece.SourcePlacementRole == SourcePlacementRole.LockedCastler &&
        piece.FinalFamily == FinalPieceFamily.Jack &&
        piece.UpgradePath.SequenceEqual(new[]
        {
          "castler",
          ApmwConstants.PieceUpgradeActions.MajorToJack,
        })));
      Assert.AreEqual(0, roster.UnallocatedMaterial);
    }

    [TestMethod]
    public void FundamentalRoster_ClampsDirectCastlerOvercountToAcceptedMaximum()
    {
      ConfigureFundamental(
        4,
        2000,
        JObject.FromObject(new Dictionary<string, int>
        {
          [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 0,
        }),
        castlers: 99);

      PieceGenerationAllocation allocation = FundamentalSlotGraduationPlanner.PlanOwnedRoster(
        ApmwCore.getInstance(),
        ApmwConfig.getInstance());
      GeneratedRoster roster = OwnedRosterGeneration.Generate();

      Assert.AreEqual(ItemGenerationValues.CastlerMaximum, allocation.LockedMajorCount);
      Assert.AreEqual(2, roster.RosterPieces.Count(piece => piece.LockedCastler));
      Assert.AreEqual(1000, roster.UnallocatedMaterial);
      Assert.AreEqual(
        4 * ItemGenerationValues.Pawn + 2000,
        roster.OwnedMaterialLedger.GrantedTotal,
        "excess Castler receipts are overcount/ungranted while all Material remains accounted");
    }

    [TestMethod]
    public void ItemSnapshot_ClampsCastlerOvercountBeforeOwnedGeneration()
    {
      ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
      builder.ProgressionItemization = ProgressionItemization.Fundamental;
      builder.PieceUpgradePreferenceProfile = ApmwPieceUpgradePreferenceProfile.FundamentalPlannedChain;
      builder.ChessmenCount = 4;
      builder.MaterialCount = 10;
      builder.CastlerCount = 99;

      using (ApmwFuzzScope scope = ApmwFuzzScope.Configure(builder.Build()))
      {
        new ApmwChessGame().earlyPopulatePieceTypes();
        Assert.AreEqual(ItemGenerationValues.CastlerMaximum, ApmwCore.getInstance().foundCastlers);
        GeneratedRoster roster = scope.Handler.GenerateOwnedRoster();
        Assert.AreEqual(2, roster.RosterPieces.Count(piece => piece.LockedCastler));
      }
    }

    [TestMethod]
    public void FundamentalSnapshot_RetainsCommonConsulsAndKingPromotionsWithLedgerAccounting()
    {
      ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
      builder.ProgressionItemization = ProgressionItemization.Fundamental;
      builder.PieceUpgradePreferenceProfile = ApmwPieceUpgradePreferenceProfile.FundamentalPlannedChain;
      builder.ChessmenCount = 4;
      builder.MaterialCount = 0;
      builder.ConsulCount = 9;
      builder.KingPromotionCount = 9;

      using (ApmwFuzzScope scope = ApmwFuzzScope.Configure(builder.Build()))
      {
        new ApmwChessGame().earlyPopulatePieceTypes();
        ApmwCore core = ApmwCore.getInstance();
        Assert.AreEqual(2, core.foundConsuls);
        Assert.AreEqual(2, core.foundKingPromotions);
        Assert.AreEqual(0, core.foundPawnForwardness);

        GeneratedRoster roster = scope.Handler.GenerateOwnedRoster();
        ActiveRosterProjection projection = GeneratedRosterProjector.Project(
          roster,
          ProjectionGeometry.For(8, 8),
          0,
          ApmwConfig.getInstance());

        Assert.AreSame(core.kings[2], roster.PrimaryKing);
        Assert.AreEqual(2, roster.RosterPieces.Count(piece =>
          piece.SourcePlacementRole == SourcePlacementRole.AdditionalRoyal));
        Assert.AreEqual(2 * ItemGenerationValues.KingPromotion, roster.PrimaryKingGrantedMaterial);
        Assert.AreEqual(
          4 * ItemGenerationValues.Pawn +
            2 * ItemGenerationValues.Consul +
            2 * ItemGenerationValues.KingPromotion +
            ItemGenerationValues.PlayAsWhite,
          roster.OwnedMaterialLedger.GrantedTotal);
        Assert.AreEqual(0, projection.DormantMaterial);
        Assert.AreEqual(2 * ItemGenerationValues.KingPromotion, projection.PrimaryKingGrantedMaterial);
        Assert.AreEqual(ItemGenerationValues.PlayAsWhite, projection.UnallocatedMaterial);
        Assert.AreEqual(
          4 * ItemGenerationValues.Pawn +
            2 * ItemGenerationValues.Consul +
            2 * ItemGenerationValues.KingPromotion,
          projection.ExactActiveExpectedMaterial);
        CollectionAssert.Contains(projection.ActivePromotionFamilyCatalog.ToList(), "royal");
        CollectionAssert.Contains(projection.ActivePromotionFamilyCatalog.ToList(), "pawn");
      }
    }

    [TestMethod]
    public void LegacyParentlessUpgrades_AreDormantRatherThanUnallocated()
    {
      ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
      Dictionary<string, object> slotData = builder.Build().BuildSlotData();
      slotData["apmw_contract"] = ApmwContractTestFixture.Document();
      ApmwConfig.getInstance().Instantiate(slotData);
      PreparePieceTypes();
      ApmwCore core = ApmwCore.getInstance();
      core.foundPawns = 0;
      core.foundMinors = 0;
      core.foundMajors = 0;
      core.foundJacks = 0;
      core.foundQueens = 3;
      core.foundAmazons = 0;
      core.foundConsuls = 0;
      core.foundKingPromotions = 0;
      core.foundPlayAsWhite = 0;
      core.foundPockets = 0;

      GeneratedRoster roster = OwnedRosterGeneration.Generate();
      ActiveRosterProjection projection = GeneratedRosterProjector.Project(
        roster,
        ProjectionGeometry.For(8, 8),
        0,
        ApmwConfig.getInstance());

      Assert.AreEqual(1245, roster.DormantMaterial);
      Assert.AreEqual(0, roster.UnallocatedMaterial);
      Assert.AreEqual(1245, projection.DormantMaterial);
      Assert.AreEqual(0, projection.UnallocatedMaterial);
      Assert.AreEqual(1245, roster.OwnedMaterialLedger.GrantedTotal);
    }

    [TestMethod]
    public void RuntimeSnapshots_ApplyAcceptedMaximaAndCommonGrantLedgers()
    {
      ApmwFuzzCase.Builder fundamental = ApmwFuzzCase.DefaultStandard().ToBuilder();
      fundamental.ProgressionItemization = ProgressionItemization.Fundamental;
      fundamental.PieceUpgradePreferenceProfile = ApmwPieceUpgradePreferenceProfile.FundamentalPlannedChain;
      fundamental.ChessmenCount = 999;
      fundamental.MaterialCount = 999;
      fundamental.CastlerCount = 999;
      fundamental.PocketCount = 999;
      fundamental.PlayAsWhiteCount = 999;
      fundamental.ConsulCount = 999;
      fundamental.KingPromotionCount = 999;

      using (ApmwFuzzScope scope = ApmwFuzzScope.Configure(fundamental.Build()))
      {
        new ApmwChessGame().earlyPopulatePieceTypes();
        ApmwCore core = ApmwCore.getInstance();
        Assert.AreEqual(107, core.foundChessmen);
        Assert.AreEqual(321 * ApmwConfig.DefaultMaterialItemValue, core.foundMaterialBudget);
        Assert.AreEqual(2, core.foundCastlers);
        Assert.AreEqual(12, core.foundPockets);
        Assert.AreEqual(1, core.foundPlayAsWhite);
        Assert.AreEqual(2, core.foundConsuls);
        Assert.AreEqual(2, core.foundKingPromotions);
        GeneratedRoster roster = scope.Handler.GenerateOwnedRoster();
        Assert.AreEqual(
          107 * ItemGenerationValues.Pawn +
            321 * ApmwConfig.DefaultMaterialItemValue +
            12 * ItemGenerationValues.Pocket +
            ItemGenerationValues.PlayAsWhite +
            2 * ItemGenerationValues.Consul +
            2 * ItemGenerationValues.KingPromotion,
          roster.OwnedMaterialLedger.GrantedTotal);
        ActiveRosterProjection projection = GeneratedRosterProjector.Project(
          roster,
          ProjectionGeometry.For(12, 12),
          0,
          ApmwConfig.getInstance());
        Assert.AreEqual(
          roster.OwnedMaterialLedger.GrantedTotal,
          projection.PrimaryKingGrantedMaterial +
            projection.ActivePieces.Sum(piece => piece.GrantedMaterial) +
            projection.MissingMaterial +
            projection.DormantMaterial +
            projection.UnallocatedMaterial);
      }
    }

    [TestMethod]
    public void LegacySnapshot_ClampsAcceptedMaximaAndCurrentContractIgnoresAmazon()
    {
      ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
      builder.PawnCount = 999;
      builder.PawnForwardnessCount = 999;
      builder.MinorPieceCount = 999;
      builder.MajorPieceCount = 999;
      builder.MajorToQueenCount = 999;
      builder.JackCount = 999;
      builder.AmazonCount = 999;
      builder.PocketCount = 999;
      builder.PlayAsWhiteCount = 999;
      builder.ConsulCount = 999;
      builder.KingPromotionCount = 999;
      ApmwFuzzCase fuzzCase = builder.Build();

      using (ApmwFuzzScope scope = ApmwFuzzScope.Configure(fuzzCase))
      {
        Dictionary<string, object> currentSlotData = fuzzCase.BuildSlotData();
        currentSlotData["apmw_contract"] = ApmwContractTestFixture.Document();
        ApmwConfig.getInstance().Instantiate(currentSlotData);
        scope.Handler.Hook();
        new ApmwChessGame().earlyPopulatePieceTypes();
        ApmwCore core = ApmwCore.getInstance();
        Assert.AreEqual(60, core.foundPawns);
        Assert.AreEqual(13, core.foundPawnForwardness);
        Assert.AreEqual(15, core.foundMinors);
        Assert.AreEqual(11, core.foundMajors);
        Assert.AreEqual(9, core.foundQueens);
        Assert.AreEqual(9, core.foundJacks);
        Assert.AreEqual(0, core.foundAmazons);
        Assert.AreEqual(12, core.foundPockets);
        Assert.AreEqual(1, core.foundPlayAsWhite);
        Assert.AreEqual(2, core.foundConsuls);
        Assert.AreEqual(2, core.foundKingPromotions);
        GeneratedRoster roster = scope.Handler.GenerateOwnedRoster();
        Assert.AreEqual(28740, roster.OwnedMaterialLedger.GrantedTotal);
        Assert.AreEqual(1370, roster.UnallocatedMaterial);
        Assert.AreEqual(0, roster.DormantMaterial);
      }
    }

    [TestMethod]
    public void LegacyAdapter_PreservesMarkerlessProgressiveAmazon()
    {
      ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
      builder.AmazonCount = 4;
      builder.FairyChessPawnUpgrades = FairyPawnUpgrades.Configure;
      builder.PieceUpgradePreferenceProfile = ApmwPieceUpgradePreferenceProfile.ListMinorToJackFirst;
      using (ApmwFuzzScope scope = ApmwFuzzScope.Configure(builder.Build()))
      {
        Assert.IsFalse(ApmwConfig.getInstance().UsesCurrentContract);
        Assert.AreEqual(4, ApmwCore.getInstance().foundAmazons);
      }
    }

    [TestMethod]
    public void StableSeries_AreCounterStableAndIsolatedBySeriesId()
    {
      ConfigureLegacy(PieceTypes.Stable, PieceLocations.Stable);
      ApmwConfig config = ApmwConfig.getInstance();
      CounterBasedSeedSeries family = ApmwSeedSeries.Semantic(config, "piece-type.major");
      ulong first = family.Value(0);
      ulong second = family.Value(1);

      _ = ApmwSeedSeries.Semantic(config, "upgrade-source.major-to-queen").Value(0);

      Assert.AreEqual(first, family.Value(0));
      Assert.AreEqual(second, family.Value(1));
      Assert.AreNotEqual(first, ApmwSeedSeries.Semantic(config, "piece-type.minor").Value(0));
    }

    [TestMethod]
    public void OwnedRosterModels_ConformToAcceptedV2ContractFixture()
    {
      string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "ProjectionV2", "baseline.json");
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(File.ReadAllText(path));

      CollectionAssert.AreEqual(
        contract.SourceRoles.ToArray(),
        Enum.GetValues(typeof(SourcePlacementRole))
          .Cast<SourcePlacementRole>()
          .Select(RoleId)
          .ToArray());
      CollectionAssert.AreEqual(
        contract.UpgradeDag.FinalFamilies.ToArray(),
        Enum.GetValues(typeof(FinalPieceFamily))
          .Cast<FinalPieceFamily>()
          .Select(family => family.ToString().ToLowerInvariant())
          .ToArray());
      Assert.AreEqual(contract.ExpectedMaterial["pawn"], OwnedRosterGeneration.ExpectedMaterial(FinalPieceFamily.Pawn));
      Assert.AreEqual(contract.ExpectedMaterial["amazon"], OwnedRosterGeneration.ExpectedMaterial(FinalPieceFamily.Amazon));
      Assert.AreEqual(contract.Castler.NormalizedCost, 500);
    }

    [TestMethod]
    public void ChaosLegacy_RerandomizesPresentationWithoutChangingSemanticOwnedRoster()
    {
      GeneratedRoster stable = ConfigureLegacy(PieceTypes.Stable, PieceLocations.Stable);
      string stableSemantic = SemanticRoster(stable);
      Assert.IsTrue(stable.RosterPieces.Count > 39, "high-count Legacy ownership must not be clipped to an 8-file setup");
      Assert.AreEqual(
        stable.RosterPieces.Sum(piece => piece.GrantedMaterial) + stable.UnallocatedMaterial,
        stable.OwnedMaterialLedger.GrantedTotal);
      Assert.AreEqual(
        stable.RosterPieces.Sum(piece => piece.EvaluatedMidgame - piece.FinalExpectedMaterial),
        stable.OwnedMaterialLedger.ConcreteEvaluationAdjustment,
        "Legacy concrete evaluation adjustment must reflect final upgraded pieces");

      Reset();
      GeneratedRoster chaos = ConfigureLegacy(PieceTypes.Chaos, PieceLocations.Chaos, deterministicChaosSeed: 78123);

      Assert.AreEqual(stableSemantic, SemanticRoster(chaos));
      Assert.AreEqual(stable.OwnedMaterialLedger.GrantedTotal, chaos.OwnedMaterialLedger.GrantedTotal);
    }

    [TestMethod]
    public void BetterPawnConfiguration_LeavesOwnedPiecesInPawnSlots()
    {
      ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
      builder.PawnCount = 12;
      builder.MinorPieceCount = 0;
      builder.MajorPieceCount = 0;
      builder.MajorToQueenCount = 0;
      builder.FairyChessPawnUpgrades = FairyPawnUpgrades.Max;

      ApmwConfig.getInstance().Instantiate(builder.Build().BuildSlotData());
      PreparePieceTypes();
      ApmwCore core = ApmwCore.getInstance();
      core.foundPawns = 12;
      core.foundMinors = 0;
      core.foundMajors = 0;
      core.foundJacks = 0;
      core.foundQueens = 0;
      core.foundAmazons = 0;
      core.foundConsuls = 0;
      core.foundKingPromotions = 0;

      GeneratedRoster roster = OwnedRosterGeneration.Generate();

      Assert.AreEqual(12, roster.RosterPieces.Count);
      Assert.IsTrue(roster.RosterPieces.All(piece =>
        piece.SourcePlacementRole == SourcePlacementRole.PawnSlot &&
        piece.FinalFamily == FinalPieceFamily.Pawn));
    }

    [TestMethod]
    public void FundamentalRoster_MatchesSharedWaveTierTotalsAndConservesLedger()
    {
      ApmwConfig config = ConfigureFundamental(19, 17 * 400, PlannedChain());
      ApmwCore core = ApmwCore.getInstance();
      PieceGenerationAllocation allocation = FundamentalSlotGraduationPlanner.PlanOwnedRoster(core, config);
      GeneratedRoster roster = OwnedRosterGeneration.Generate();

      foreach (FinalPieceFamily family in Enum.GetValues(typeof(FinalPieceFamily)))
      {
        int expected = family == FinalPieceFamily.Pawn
          ? allocation.PawnSlots
          : allocation.NonPawnCount((NonPawnPieceFamily)((int)family - 1));
        Assert.AreEqual(expected, roster.RosterPieces.Count(piece => piece.FinalFamily == family), family.ToString());
      }

      int pieceGranted = roster.RosterPieces.Sum(piece => piece.GrantedMaterial);
      Assert.AreEqual(pieceGranted + roster.UnallocatedMaterial, roster.OwnedMaterialLedger.GrantedTotal);
      Assert.AreEqual(19 * ItemGenerationValues.Pawn + 17 * 400, roster.OwnedMaterialLedger.GrantedTotal);
    }

    [TestMethod]
    public void FundamentalSurvivingPawns_RecordFamilyEntitlementsForActiveAndReserve()
    {
      ConfigureFundamental(
        40,
        0,
        JObject.FromObject(new Dictionary<string, int>
        {
          [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 0,
        }));

      GeneratedRoster roster = OwnedRosterGeneration.Generate();
      ActiveRosterProjection projection = GeneratedRosterProjector.Project(
        roster,
        ProjectionGeometry.For(8, 8),
        0,
        ApmwConfig.getInstance());

      Assert.IsTrue(roster.RosterPieces.All(piece =>
        piece.FinalFamily == FinalPieceFamily.Pawn &&
        piece.PromotionEntitlementFamilies.SequenceEqual(new[] { "pawn" })));
      Assert.AreEqual(32, projection.ActiveCountsByRole[SourcePlacementRole.PawnSlot]);
      Assert.AreEqual(8, projection.ReserveCountsByRole[SourcePlacementRole.PawnSlot]);
      CollectionAssert.Contains(projection.ActivePromotionFamilyCatalog.ToList(), "pawn");
      CollectionAssert.Contains(projection.ReservePromotionFamilyCatalog.ToList(), "pawn");
      Assert.IsTrue(projection.ActivePieces.All(piece =>
        piece.PromotionEntitlementFamilies.Contains("pawn")));
      Assert.IsTrue(projection.ReservePieces.All(piece =>
        piece.PromotionEntitlementFamilies.Contains("pawn")));
    }

    [TestMethod]
    public void SemanticUpgradeSourceSeries_UsesAcceptedV2IdentifierTemplate()
    {
      string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "ProjectionV2", "baseline.json");
      ApmwContractV2 contract = ApmwContractV2Parser.Parse(File.ReadAllText(path));
      string template = contract.SemanticSeriesIds.Single(id => id == "upgrade-source.{action}");

      foreach (string action in new[]
      {
        ApmwConstants.PieceUpgradeActions.PawnToMinor,
        ApmwConstants.PieceUpgradeActions.MinorToMajor,
        ApmwConstants.PieceUpgradeActions.MajorToJack,
        ApmwConstants.PieceUpgradeActions.MajorToQueen,
      })
      {
        string id = ApmwSeedSeries.UpgradeSourceSeriesId(action);
        Assert.AreEqual(template.Replace("{action}", action), id);
        Assert.IsFalse(id.Contains("legacy.upgrade.tie", StringComparison.Ordinal));
      }
    }

    [TestMethod]
    public void PromotionEntitlements_IncludeDirectAndIntermediateConcreteTypes()
    {
      PreparePieceTypes();
      ApmwCore core = ApmwCore.getInstance();
      PieceType direct = core.minors.First();
      PieceType upgraded = core.majors.First();
      var piece = new RosterPiece(
        "minor-slot:000000",
        SourcePlacementRole.MinorSlot,
        0,
        "direct-minor",
        FinalPieceFamily.Minor,
        direct,
        false,
        ItemGenerationValues.Minor,
        ItemGenerationValues.Minor);

      piece.Upgrade(
        ApmwConstants.PieceUpgradeActions.MinorToMajor,
        FinalPieceFamily.Major,
        upgraded,
        ItemGenerationValues.Major - ItemGenerationValues.Minor);

      CollectionAssert.Contains(piece.PromotionEntitlements.ToList(), direct.Notation[core.GeriProvider()]);
      CollectionAssert.Contains(piece.PromotionEntitlements.ToList(), upgraded.Notation[core.GeriProvider()]);
      CollectionAssert.AreEquivalent(
        new[] { "minor", "major" },
        piece.PromotionEntitlementFamilies.ToList());
    }

    private static ApmwConfig ConfigureFundamental(
      int chessmen,
      int material,
      JObject priorities,
      int castlers = 0)
    {
      ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
      builder.ProgressionItemization = ProgressionItemization.Fundamental;
      builder.PieceUpgradePreferenceProfile = ApmwPieceUpgradePreferenceProfile.FundamentalPlannedChain;
      Dictionary<string, object> slotData = builder.Build().BuildSlotData();
      slotData[ApmwConstants.SlotKeyFairyChessPawnUpgrades] = (int)FairyPawnUpgrades.Configure;
      slotData[ApmwConstants.SlotKeyPieceUpgradePreferences] = priorities;

      ApmwConfig config = ApmwConfig.getInstance();
      config.Instantiate(slotData);
      config.seed();
      PreparePieceTypes();
      ApmwCore core = ApmwCore.getInstance();
      core.foundChessmen = chessmen;
      core.foundMaterialBudget = material;
      core.foundCastlers = castlers;
      core.foundConsuls = 0;
      core.foundKingPromotions = 0;
      core.IgnoreCastlersReceived = false;
      return config;
    }

    private static GeneratedRoster ConfigureLegacy(
      PieceTypes types,
      PieceLocations locations,
      int? deterministicChaosSeed = null)
    {
      ApmwFuzzCase.Builder builder = ApmwFuzzCase.DefaultStandard().ToBuilder();
      builder.PlayerPieceTypes = types;
      builder.PieceLocations = locations;
      builder.DeterministicChaosSeed = deterministicChaosSeed;
      builder.PawnCount = 40;
      builder.MinorPieceCount = 18;
      builder.MajorPieceCount = 14;
      builder.JackCount = 7;
      builder.MajorToQueenCount = 6;
      builder.AmazonCount = 4;
      builder.FairyChessPawnUpgrades = FairyPawnUpgrades.Configure;
      builder.PieceUpgradePreferenceProfile = ApmwPieceUpgradePreferenceProfile.TiedGraduationWithProportions;

      ApmwConfig config = ApmwConfig.getInstance();
      config.Instantiate(builder.Build().BuildSlotData());
      PreparePieceTypes();
      ApmwCore core = ApmwCore.getInstance();
      core.foundPawns = builder.PawnCount;
      core.foundMinors = builder.MinorPieceCount;
      core.foundMajors = builder.MajorPieceCount;
      core.foundJacks = builder.JackCount;
      core.foundQueens = builder.MajorToQueenCount;
      core.foundAmazons = builder.AmazonCount;
      core.foundConsuls = 0;
      core.foundKingPromotions = 0;
      return OwnedRosterGeneration.Generate();
    }

    private static void PreparePieceTypes()
    {
      new ApmwChessGame().earlyPopulatePieceTypes();
    }

    private static int TotalSlots(PieceGenerationAllocation allocation)
    {
      return allocation.PawnSlots +
        Enum.GetValues(typeof(NonPawnPieceFamily))
          .Cast<NonPawnPieceFamily>()
          .Sum(allocation.NonPawnCount);
    }

    private static string SemanticAllocation(PieceGenerationAllocation allocation)
    {
      return allocation.PawnSlots + "|" + allocation.InitialSpareMaterial + "|" +
        allocation.LockedMajorCount + "|" +
        string.Join(",", Enum.GetValues(typeof(NonPawnPieceFamily))
          .Cast<NonPawnPieceFamily>()
          .Select(family => family + "=" + allocation.NonPawnCount(family))) + "|" +
        string.Join(",", allocation.AppliedGraduationCounts.OrderBy(pair => pair.Key)
          .Select(pair => pair.Key + "=" + pair.Value));
    }

    private static string SemanticRoster(GeneratedRoster roster)
    {
      return string.Join("|", roster.RosterPieces
        .OrderBy(piece => piece.StableId)
        .Select(piece => string.Join(",",
          piece.StableId,
          piece.SourcePlacementRole,
          piece.SourceOrdinal,
          piece.RoleOriginAction,
          piece.FinalFamily,
          piece.LockedCastler,
          piece.GrantedMaterial,
          piece.FinalExpectedMaterial,
          string.Join(">", piece.UpgradePath))));
    }

    private static JObject PlannedChain()
    {
      return JObject.FromObject(new Dictionary<string, int>
      {
        [ApmwConstants.PieceUpgradeActions.PawnToMinor] = 7,
        [ApmwConstants.PieceUpgradeActions.MinorToMajor] = 6,
        [ApmwConstants.PieceUpgradeActions.MajorToJack] = 5,
        [ApmwConstants.PieceUpgradeActions.MinorToJack] = 4,
        [ApmwConstants.PieceUpgradeActions.MajorToQueen] = 3,
        [ApmwConstants.PieceUpgradeActions.JackToQueen] = 2,
        [ApmwConstants.PieceUpgradeActions.QueenToAmazon] = 1,
      });
    }

    private static string RoleId(SourcePlacementRole role)
    {
      switch (role)
      {
        case SourcePlacementRole.PrimaryRoyal: return "primary-royal";
        case SourcePlacementRole.AdditionalRoyal: return "additional-royal";
        case SourcePlacementRole.LockedCastler: return "locked-castler";
        case SourcePlacementRole.JackSlot: return "jack-slot";
        case SourcePlacementRole.MajorSlot: return "major-slot";
        case SourcePlacementRole.MinorSlot: return "minor-slot";
        case SourcePlacementRole.PawnSlot: return "pawn-slot";
        default: throw new ArgumentOutOfRangeException(nameof(role));
      }
    }

    private static void Reset()
    {
      ApmwCore._instance = null;
      ApmwConfig._instance = null;
    }
  }
}
