using ChessV;
using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;

namespace Archipelago.APChessV
{
  internal sealed class ApmwSidecarPresentationAdapter
  {
    private sealed class AdaptedSnapshot
    {
      public GeneratedRoster Roster;
      public ActiveRosterProjection Projection;
    }

    private readonly ApmwCore core;
    private readonly ApmwConfig config;

    public ApmwSidecarPresentationAdapter(ApmwCore core, ApmwConfig config)
    {
      this.core = core ?? throw new ArgumentNullException(nameof(core));
      this.config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public GeneratedRoster CreateOwnedRoster(JsonElement projection)
    {
      return Adapt(projection).Roster;
    }

    public ActiveRosterProjection CreateProjection(JsonElement projection)
    {
      return Adapt(projection).Projection;
    }

    private AdaptedSnapshot Adapt(JsonElement projection)
    {
      if (projection.ValueKind != JsonValueKind.Object)
        throw new ArgumentException("sidecar projection must be an object", nameof(projection));

      var chooser = new OwnedRosterGeneration.FamilyPieceChooser();
      List<JsonElement> ownedSlots = projection.GetProperty("owned_slots").EnumerateArray().ToList();
      JsonElement primarySlot = ownedSlots.Single(slot =>
        String(slot, "source_role") == "primary-royal");
      PieceType primaryKing = ChoosePrimaryKing(primarySlot);
      var pieces = new List<RosterPiece>();
      var pieceById = new Dictionary<string, RosterPiece>(StringComparer.Ordinal);
      foreach (JsonElement slot in ownedSlots.Where(slot =>
        String(slot, "source_role") != "primary-royal"))
      {
        RosterPiece piece = CreateRosterPiece(slot, chooser);
        if (!pieceById.TryAdd(piece.StableId, piece))
          throw new InvalidOperationException("sidecar returned a duplicate owned slot ID");
        pieces.Add(piece);
      }

      int dormantMaterial = Integer(projection, "dormant_material");
      int unallocatedMaterial = Integer(projection, "unallocated_material");
      OwnedMaterialLedger ledger = CreateLedger(projection, primarySlot, pieceById);
      var roster = new GeneratedRoster(
        primaryKing,
        pieces,
        ledger,
        unallocatedMaterial,
        String(projection, "itemization") + "-" + String(projection, "ordering"),
        Integer(primarySlot, "granted_material"),
        Integer(primarySlot, "final_expected_material"),
        dormantMaterial);
      ActiveRosterProjection activeProjection = CreateActiveProjection(
        projection,
        roster,
        primarySlot,
        pieceById);
      return new AdaptedSnapshot { Roster = roster, Projection = activeProjection };
    }

    private RosterPiece CreateRosterPiece(
      JsonElement slot,
      OwnedRosterGeneration.FamilyPieceChooser chooser)
    {
      string familyId = String(slot, "final_family");
      FinalPieceFamily? family = familyId == "royal" ? null : ParseFamily(familyId);
      SourcePlacementRole role = ParseRole(String(slot, "source_role"));
      List<string> upgradePath = Strings(slot, "upgrade_path");
      PieceType concrete;
      if (!family.HasValue)
      {
        concrete = core.kings == null ? null : core.kings.FirstOrDefault();
      }
      else if (family == FinalPieceFamily.Pawn)
      {
        concrete = OwnedRosterGeneration.ChoosePawn(config, Integer(slot, "source_ordinal"));
      }
      else
      {
        string finalAction = upgradePath.LastOrDefault();
        string seriesId = string.IsNullOrEmpty(finalAction) || finalAction == "castler"
          ? null
          : "upgrade-target." + finalAction;
        concrete = chooser.Choose(family.Value, config, seriesId);
      }

      return new RosterPiece(
        String(slot, "slot_id"),
        role,
        Integer(slot, "source_ordinal"),
        String(slot, "role_origin_action"),
        family,
        concrete,
        Boolean(slot, "locked_castler"),
        Integer(slot, "granted_material"),
        Integer(slot, "final_expected_material"),
        upgradePath,
        Strings(slot, "promotion_entitlement_families"));
    }

    private PieceType ChoosePrimaryKing(JsonElement primarySlot)
    {
      if (core.kings == null || core.kings.Count == 0)
        throw new InvalidOperationException("sidecar presentation requires an initialized King family");
      int promotionCount = Strings(primarySlot, "upgrade_path")
        .Count(action => action == "king-promotion");
      return core.kings[Math.Min(promotionCount, core.kings.Count - 1)];
    }

    private static OwnedMaterialLedger CreateLedger(
      JsonElement projection,
      JsonElement primarySlot,
      IReadOnlyDictionary<string, RosterPiece> pieceById)
    {
      var ledger = new OwnedMaterialLedger();
      foreach (string listName in new[] { "active_material_ledger", "reserve_material_ledger" })
      {
        foreach (JsonElement entry in projection.GetProperty(listName).EnumerateArray())
        {
          string slotId = NullableString(entry, "slot_id");
          OwnedMaterialCategory category = CategoryForSlot(slotId, primarySlot, pieceById);
          ledger.Add(category, Integer(entry, "amount"), slotId, String(entry, "source"));
        }
      }
      foreach (JsonElement entry in projection.GetProperty("dormant_material_ledger").EnumerateArray())
      {
        ledger.Add(
          OwnedMaterialCategory.Dormant,
          Integer(entry, "amount"),
          NullableString(entry, "slot_id"),
          String(entry, "source"));
      }
      foreach (JsonElement entry in projection.GetProperty("unallocated_material_ledger").EnumerateArray())
      {
        ledger.Add(
          OwnedMaterialCategory.Unallocated,
          Integer(entry, "amount"),
          NullableString(entry, "slot_id"),
          String(entry, "source"));
      }
      return ledger;
    }

    private static OwnedMaterialCategory CategoryForSlot(
      string slotId,
      JsonElement primarySlot,
      IReadOnlyDictionary<string, RosterPiece> pieceById)
    {
      if (slotId == String(primarySlot, "slot_id"))
        return OwnedMaterialCategory.PrimaryRoyal;
      if (slotId != null && pieceById.TryGetValue(slotId, out RosterPiece piece))
      {
        if (piece.SourcePlacementRole == SourcePlacementRole.AdditionalRoyal)
          return OwnedMaterialCategory.AdditionalRoyal;
        if (piece.SourcePlacementRole == SourcePlacementRole.LockedCastler)
          return OwnedMaterialCategory.LockedCastler;
      }
      return OwnedMaterialCategory.DirectPiece;
    }

    private static ActiveRosterProjection CreateActiveProjection(
      JsonElement projection,
      GeneratedRoster roster,
      JsonElement primarySlot,
      IReadOnlyDictionary<string, RosterPiece> pieceById)
    {
      ProjectionGeometry geometry = ProjectionGeometry.For(
        Integer(projection, "files"),
        Integer(projection, "ranks"));
      Dictionary<string, ProjectedPlacement> placements = projection
        .GetProperty("active_placements")
        .EnumerateArray()
        .ToDictionary(
          placement => String(placement, "slot_id"),
          placement => Placement(placement, geometry),
          StringComparer.Ordinal);
      string primaryId = String(primarySlot, "slot_id");
      if (!placements.TryGetValue(primaryId, out ProjectedPlacement primaryPlacement))
        throw new InvalidOperationException("sidecar projection omitted the primary royal placement");

      List<ProjectedRosterPiece> active = projection.GetProperty("active_slots")
        .EnumerateArray()
        .Where(slot => String(slot, "slot_id") != primaryId)
        .Select(slot =>
        {
          string slotId = String(slot, "slot_id");
          return new ProjectedRosterPiece(pieceById[slotId], placements[slotId]);
        })
        .ToList();
      List<ProjectedRosterPiece> reserve = projection.GetProperty("reserve_slots")
        .EnumerateArray()
        .Select(slot => new ProjectedRosterPiece(pieceById[String(slot, "slot_id")], null))
        .ToList();
      List<ProjectionRegionUsage> regionUsage = projection
        .GetProperty("region_usage")
        .GetProperty("ranks")
        .EnumerateArray()
        .Select(rank =>
        {
          int relativeRank = Integer(rank, "relative_rank");
          int used = Integer(rank, "non_pawns") + Integer(rank, "pawns");
          return new ProjectionRegionUsage(
            geometry.FormationBandId(relativeRank),
            ParseRegion(String(rank, "region")),
            Integer(rank, "capacity"),
            used);
        })
        .ToList();

      IReadOnlyDictionary<SourcePlacementRole, int> activeByRole =
        CountsByRole(active.Select(piece => piece.SourcePlacementRole), true);
      IReadOnlyDictionary<SourcePlacementRole, int> reserveByRole =
        CountsByRole(reserve.Select(piece => piece.SourcePlacementRole), false);
      IReadOnlyDictionary<FinalPieceFamily, int> activeByFamily =
        CountsByFamily(active.Where(piece => piece.FinalFamily.HasValue).Select(piece => piece.FinalFamily.Value));
      IReadOnlyDictionary<FinalPieceFamily, int> reserveByFamily =
        CountsByFamily(reserve.Where(piece => piece.FinalFamily.HasValue).Select(piece => piece.FinalFamily.Value));
      List<string> activePromotions = PromotionCatalog(active);
      if (Integer(primarySlot, "final_expected_material") > 0 &&
          roster.PrimaryKing.Notation != null)
      {
        int player = CorePlayer();
        if (player >= 0 && player < roster.PrimaryKing.Notation.Length)
          activePromotions.Add(roster.PrimaryKing.Notation[player]);
      }
      activePromotions = activePromotions.Distinct(StringComparer.Ordinal)
        .OrderBy(value => value, StringComparer.Ordinal).ToList();
      List<string> reservePromotions = PromotionCatalog(reserve);
      List<string> eligibleCastlers = Strings(projection, "castling_eligible_slots");
      List<CastlingRightMetadata> castlingRights = eligibleCastlers
        .Select(slotId =>
        {
          ProjectedRosterPiece piece = active.Single(item => item.StableId == slotId);
          return new CastlingRightMetadata(piece, piece.Placement.Coordinate);
        })
        .ToList();

      return new ActiveRosterProjection(
        geometry,
        roster.PrimaryKing,
        primaryPlacement,
        active,
        reserve,
        regionUsage,
        activeByRole,
        reserveByRole,
        activeByFamily,
        reserveByFamily,
        activePromotions,
        reservePromotions,
        reservePromotions.Except(activePromotions, StringComparer.Ordinal),
        Strings(projection, "active_castlers"),
        castlingRights,
        Integer(primarySlot, "granted_material"),
        Integer(primarySlot, "final_expected_material"),
        Integer(projection, "owned_expected_material"),
        Integer(projection, "exact_active_material"),
        Integer(projection, "missing_material"),
        MissingByRole(reserve),
        MissingByFamily(reserve),
        Integer(projection, "dormant_material"),
        Integer(projection, "unallocated_material"),
        Integer(projection, "unspent_forwardness"));
    }

    private static ProjectedPlacement Placement(JsonElement value, ProjectionGeometry geometry)
    {
      var coordinate = new ProjectionCoordinate(
        Integer(value, "file"),
        Integer(value, "relative_rank"));
      return new ProjectedPlacement(
        coordinate,
        geometry.RegionForRank(coordinate.RelativeRank),
        String(value, "formation_band"));
    }

    private static IReadOnlyDictionary<SourcePlacementRole, int> CountsByRole(
      IEnumerable<SourcePlacementRole> roles,
      bool includePrimary)
    {
      Dictionary<SourcePlacementRole, int> counts = Enum.GetValues(typeof(SourcePlacementRole))
        .Cast<SourcePlacementRole>()
        .ToDictionary(role => role, role => 0);
      foreach (SourcePlacementRole role in roles)
        counts[role]++;
      if (includePrimary)
        counts[SourcePlacementRole.PrimaryRoyal] = 1;
      return new ReadOnlyDictionary<SourcePlacementRole, int>(counts);
    }

    private static IReadOnlyDictionary<FinalPieceFamily, int> CountsByFamily(
      IEnumerable<FinalPieceFamily> families)
    {
      Dictionary<FinalPieceFamily, int> counts = Enum.GetValues(typeof(FinalPieceFamily))
        .Cast<FinalPieceFamily>()
        .ToDictionary(family => family, family => 0);
      foreach (FinalPieceFamily family in families)
        counts[family]++;
      return new ReadOnlyDictionary<FinalPieceFamily, int>(counts);
    }

    private static IReadOnlyDictionary<SourcePlacementRole, int> MissingByRole(
      IEnumerable<ProjectedRosterPiece> reserve)
    {
      Dictionary<SourcePlacementRole, int> result = Enum.GetValues(typeof(SourcePlacementRole))
        .Cast<SourcePlacementRole>()
        .ToDictionary(role => role, role => 0);
      foreach (ProjectedRosterPiece piece in reserve)
        result[piece.SourcePlacementRole] += piece.GrantedMaterial;
      return new ReadOnlyDictionary<SourcePlacementRole, int>(result);
    }

    private static IReadOnlyDictionary<FinalPieceFamily, int> MissingByFamily(
      IEnumerable<ProjectedRosterPiece> reserve)
    {
      Dictionary<FinalPieceFamily, int> result = Enum.GetValues(typeof(FinalPieceFamily))
        .Cast<FinalPieceFamily>()
        .ToDictionary(family => family, family => 0);
      foreach (ProjectedRosterPiece piece in reserve.Where(piece => piece.FinalFamily.HasValue))
        result[piece.FinalFamily.Value] += piece.GrantedMaterial;
      return new ReadOnlyDictionary<FinalPieceFamily, int>(result);
    }

    private static List<string> PromotionCatalog(IEnumerable<ProjectedRosterPiece> pieces)
    {
      return pieces.SelectMany(piece => piece.PromotionEntitlements)
        .Distinct(StringComparer.Ordinal)
        .OrderBy(value => value, StringComparer.Ordinal)
        .ToList();
    }

    private static SourcePlacementRole ParseRole(string value)
    {
      switch (value)
      {
        case "primary-royal": return SourcePlacementRole.PrimaryRoyal;
        case "additional-royal": return SourcePlacementRole.AdditionalRoyal;
        case "locked-castler": return SourcePlacementRole.LockedCastler;
        case "jack-slot": return SourcePlacementRole.JackSlot;
        case "major-slot": return SourcePlacementRole.MajorSlot;
        case "minor-slot": return SourcePlacementRole.MinorSlot;
        case "pawn-slot": return SourcePlacementRole.PawnSlot;
        default: throw new InvalidOperationException("sidecar returned an unknown source role");
      }
    }

    private static FinalPieceFamily ParseFamily(string value)
    {
      switch (value)
      {
        case "pawn": return FinalPieceFamily.Pawn;
        case "minor": return FinalPieceFamily.Minor;
        case "major": return FinalPieceFamily.Major;
        case "jack": return FinalPieceFamily.Jack;
        case "queen": return FinalPieceFamily.Queen;
        case "amazon": return FinalPieceFamily.Amazon;
        default: throw new InvalidOperationException("sidecar returned an unknown final family");
      }
    }

    private static ProjectionRegion ParseRegion(string value)
    {
      switch (value)
      {
        case "back": return ProjectionRegion.BackRank;
        case "mixed": return ProjectionRegion.MixedBand;
        case "pawn-only": return ProjectionRegion.PawnOnlyBand;
        default: throw new InvalidOperationException("sidecar returned an unknown projection region");
      }
    }

    private static int CorePlayer()
    {
      return ApmwCore.getInstance().GeriProvider();
    }

    private static string String(JsonElement value, string name)
    {
      return value.GetProperty(name).GetString();
    }

    private static string NullableString(JsonElement value, string name)
    {
      JsonElement property = value.GetProperty(name);
      return property.ValueKind == JsonValueKind.Null ? null : property.GetString();
    }

    private static int Integer(JsonElement value, string name)
    {
      return value.GetProperty(name).GetInt32();
    }

    private static bool Boolean(JsonElement value, string name)
    {
      return value.GetProperty(name).GetBoolean();
    }

    private static List<string> Strings(JsonElement value, string name)
    {
      return value.GetProperty(name).EnumerateArray()
        .Select(item => item.GetString())
        .ToList();
    }
  }
}
