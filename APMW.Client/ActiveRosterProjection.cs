using ChessV;
using ChessV.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.APChessV
{
  internal enum ProjectionRegion
  {
    BackRank,
    MixedBand,
    PawnOnlyBand,
  }

  internal readonly struct ProjectionCoordinate : IEquatable<ProjectionCoordinate>
  {
    public ProjectionCoordinate(int file, int relativeRank)
    {
      File = file;
      RelativeRank = relativeRank;
    }

    public int File { get; }
    public int RelativeRank { get; }

    public bool Equals(ProjectionCoordinate other)
    {
      return File == other.File && RelativeRank == other.RelativeRank;
    }

    public override bool Equals(object obj)
    {
      return obj is ProjectionCoordinate other && Equals(other);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(File, RelativeRank);
    }
  }

  internal sealed class ProjectionGeometry
  {
    private static readonly HashSet<string> ValidStageIds = new HashSet<string>(
      new[] { "8x8", "10x8", "10x10", "12x10", "12x12" },
      StringComparer.Ordinal);

    private ProjectionGeometry(int files, int ranks)
    {
      Files = files;
      Ranks = ranks;
      StageId = files + "x" + ranks;
      ProfileId = "expanded-formation-v2/" + StageId;
      if (!ValidStageIds.Contains(StageId))
        throw new ArgumentOutOfRangeException(nameof(files), "Unsupported APMW projection geometry " + StageId + ".");
    }

    public string StageId { get; }
    public string ProfileId { get; }
    public int Files { get; }
    public int Ranks { get; }
    public int HumanFormationRanks { get { return Ranks - 3; } }
    public int MixedBandCount { get { return Ranks - 7; } }
    public int BackOptionalCapacity { get { return Files - 1; } }
    public int MixedCapacity { get { return Files * MixedBandCount; } }
    public int NonPawnCapacity { get { return Files * (Ranks - 6) - 1; } }
    public int GrossPawnCapacity { get { return Files * (Ranks - 4); } }
    public int MaximumNonPrimary { get { return Files * HumanFormationRanks - 1; } }
    public int NominalForwardness { get { return Files * (Ranks - 5); } }
    public int PrimaryKingFile { get { return Files / 2; } }

    public int ActivePawnCapacity(int activeNonPrimaryNonPawns)
    {
      int beyondBack = Math.Max(0, activeNonPrimaryNonPawns - BackOptionalCapacity);
      return GrossPawnCapacity - beyondBack;
    }

    public static ProjectionGeometry For(int files, int ranks)
    {
      return new ProjectionGeometry(files, ranks);
    }

    public static ProjectionGeometry FromContractStage(GeometryStage stage)
    {
      if (stage == null)
        throw new ArgumentNullException(nameof(stage));
      ProjectionGeometry geometry = For(stage.Files, stage.Ranks);
      if (geometry.StageId != stage.StageId ||
          geometry.NonPawnCapacity != stage.NonPawnCapacity ||
          geometry.GrossPawnCapacity != stage.GrossPawnCapacity ||
          geometry.MaximumNonPrimary != stage.CombinedNonPrimaryCapacity ||
          geometry.NominalForwardness != stage.ForwardnessCapacity)
      {
        throw new InvalidOperationException("Contract geometry does not match expanded-formation-v2.");
      }
      return geometry;
    }

    public ProjectionRegion RegionForRank(int relativeRank)
    {
      if (relativeRank == 0)
        return ProjectionRegion.BackRank;
      return relativeRank <= MixedBandCount
        ? ProjectionRegion.MixedBand
        : ProjectionRegion.PawnOnlyBand;
    }

    public string FormationBandId(int relativeRank)
    {
      if (relativeRank == 0)
        return "back-rank";
      if (relativeRank <= MixedBandCount)
        return "mixed-" + relativeRank;
      return "pawn-only-" + (relativeRank - MixedBandCount);
    }
  }

  internal sealed class ProjectedPlacement
  {
    public ProjectedPlacement(
      ProjectionCoordinate coordinate,
      ProjectionRegion region,
      string formationBand)
    {
      Coordinate = coordinate;
      Region = region;
      FormationBand = formationBand;
    }

    public ProjectionCoordinate Coordinate { get; }
    public ProjectionRegion Region { get; }
    public string FormationBand { get; }
  }

  internal sealed class ProjectedRosterPiece
  {
    public ProjectedRosterPiece(RosterPiece ownedPiece, ProjectedPlacement placement)
    {
      if (ownedPiece == null)
        throw new ArgumentNullException(nameof(ownedPiece));
      StableId = ownedPiece.StableId;
      SourcePlacementRole = ownedPiece.SourcePlacementRole;
      SourceOrdinal = ownedPiece.SourceOrdinal;
      FinalFamily = ownedPiece.FinalFamily;
      ConcretePieceType = ownedPiece.ConcretePieceType;
      LockedCastler = ownedPiece.LockedCastler;
      GrantedMaterial = ownedPiece.GrantedMaterial;
      FinalExpectedMaterial = ownedPiece.FinalFamily.HasValue
        ? OwnedRosterGeneration.ExpectedMaterial(ownedPiece.FinalFamily.Value)
        : ownedPiece.FinalExpectedMaterial;
      UpgradePath = new ReadOnlyCollection<string>(ownedPiece.UpgradePath.ToList());
      PromotionEntitlements = new ReadOnlyCollection<string>(
        ownedPiece.PromotionEntitlements.OrderBy(value => value, StringComparer.Ordinal).ToList());
      PromotionEntitlementFamilies = new ReadOnlyCollection<string>(
        ownedPiece.PromotionEntitlementFamilies.ToList());
      Placement = placement;
    }

    public string StableId { get; }
    public SourcePlacementRole SourcePlacementRole { get; }
    public int SourceOrdinal { get; }
    public FinalPieceFamily? FinalFamily { get; }
    public PieceType ConcretePieceType { get; }
    public bool LockedCastler { get; }
    public int GrantedMaterial { get; }
    public int FinalExpectedMaterial { get; }
    public IReadOnlyList<string> UpgradePath { get; }
    public IReadOnlyList<string> PromotionEntitlements { get; }
    public IReadOnlyList<string> PromotionEntitlementFamilies { get; }
    public ProjectedPlacement Placement { get; }
  }

  internal sealed class ProjectionRegionUsage
  {
    public ProjectionRegionUsage(string bandId, ProjectionRegion region, int capacity, int used)
    {
      BandId = bandId;
      Region = region;
      Capacity = capacity;
      Used = used;
    }

    public string BandId { get; }
    public ProjectionRegion Region { get; }
    public int Capacity { get; }
    public int Used { get; }
    public int Available { get { return Capacity - Used; } }
  }

  internal sealed class CastlingRightMetadata
  {
    public CastlingRightMetadata(ProjectedRosterPiece piece, ProjectionCoordinate coordinate)
    {
      StableId = piece.StableId;
      SourcePlacementRole = piece.SourcePlacementRole;
      FinalFamily = piece.FinalFamily.Value;
      LockedCastler = piece.LockedCastler;
      Coordinate = coordinate;
      IsHomeRankEligible = coordinate.RelativeRank == 0 &&
        (piece.FinalFamily == FinalPieceFamily.Major || piece.FinalFamily == FinalPieceFamily.Jack);
      CoordinateRightsResolved = true;
    }

    public string StableId { get; }
    public SourcePlacementRole SourcePlacementRole { get; }
    public FinalPieceFamily FinalFamily { get; }
    public bool LockedCastler { get; }
    public ProjectionCoordinate Coordinate { get; }
    public bool IsHomeRankEligible { get; }
    public bool CoordinateRightsResolved { get; }
  }

  internal sealed class ActiveRosterProjection
  {
    public ActiveRosterProjection(
      ProjectionGeometry geometry,
      PieceType primaryKing,
      ProjectedPlacement primaryKingPlacement,
      IEnumerable<ProjectedRosterPiece> activePieces,
      IEnumerable<ProjectedRosterPiece> reservePieces,
      IEnumerable<ProjectionRegionUsage> regionUsage,
      IReadOnlyDictionary<SourcePlacementRole, int> activeCountsByRole,
      IReadOnlyDictionary<SourcePlacementRole, int> reserveCountsByRole,
      IReadOnlyDictionary<FinalPieceFamily, int> activeCountsByFamily,
      IReadOnlyDictionary<FinalPieceFamily, int> reserveCountsByFamily,
      IEnumerable<string> activePromotionCatalog,
      IEnumerable<string> reservePromotionCatalog,
      IEnumerable<string> reserveOnlyPromotionCatalog,
      IEnumerable<string> activeCastlers,
      IEnumerable<CastlingRightMetadata> castlingRights,
      int primaryKingGrantedMaterial,
      int primaryKingExpectedMaterial,
      int ownedExpectedMaterial,
      int exactActiveExpectedMaterial,
      int missingMaterial,
      IReadOnlyDictionary<SourcePlacementRole, int> missingMaterialByRole,
      IReadOnlyDictionary<FinalPieceFamily, int> missingMaterialByFamily,
      int dormantMaterial,
      int unallocatedMaterial,
      int unspentForwardness)
    {
      Geometry = geometry;
      PrimaryKing = primaryKing;
      PrimaryKingPlacement = primaryKingPlacement;
      ActivePieces = new ReadOnlyCollection<ProjectedRosterPiece>(activePieces.ToList());
      ReservePieces = new ReadOnlyCollection<ProjectedRosterPiece>(reservePieces.ToList());
      RegionUsage = new ReadOnlyCollection<ProjectionRegionUsage>(regionUsage.ToList());
      ActiveCountsByRole = activeCountsByRole;
      ReserveCountsByRole = reserveCountsByRole;
      ActiveCountsByFamily = activeCountsByFamily;
      ReserveCountsByFamily = reserveCountsByFamily;
      ActivePromotionCatalog = new ReadOnlyCollection<string>(activePromotionCatalog.ToList());
      ReservePromotionCatalog = new ReadOnlyCollection<string>(reservePromotionCatalog.ToList());
      ReserveOnlyPromotionCatalog = new ReadOnlyCollection<string>(reserveOnlyPromotionCatalog.ToList());
      ActivePromotionFamilyCatalog = new ReadOnlyCollection<string>(
        PromotionFamilies(ActivePieces, primaryKingExpectedMaterial > 0));
      ReservePromotionFamilyCatalog = new ReadOnlyCollection<string>(
        PromotionFamilies(ReservePieces));
      ActiveCastlers = new ReadOnlyCollection<string>(activeCastlers.ToList());
      CastlingRights = new ReadOnlyCollection<CastlingRightMetadata>(castlingRights.ToList());
      PrimaryKingGrantedMaterial = primaryKingGrantedMaterial;
      PrimaryKingExpectedMaterial = primaryKingExpectedMaterial;
      OwnedExpectedMaterial = ownedExpectedMaterial;
      ExactActiveExpectedMaterial = exactActiveExpectedMaterial;
      MissingMaterial = missingMaterial;
      MissingMaterialByRole = missingMaterialByRole;
      MissingMaterialByFamily = missingMaterialByFamily;
      DormantMaterial = dormantMaterial;
      UnallocatedMaterial = unallocatedMaterial;
      UnspentForwardness = unspentForwardness;
    }

    public ProjectionGeometry Geometry { get; }
    public PieceType PrimaryKing { get; }
    public ProjectedPlacement PrimaryKingPlacement { get; }
    public IReadOnlyList<ProjectedRosterPiece> ActivePieces { get; }
    public IReadOnlyList<ProjectedRosterPiece> ReservePieces { get; }
    public IReadOnlyList<ProjectionRegionUsage> RegionUsage { get; }
    public IReadOnlyDictionary<SourcePlacementRole, int> ActiveCountsByRole { get; }
    public IReadOnlyDictionary<SourcePlacementRole, int> ReserveCountsByRole { get; }
    public IReadOnlyDictionary<FinalPieceFamily, int> ActiveCountsByFamily { get; }
    public IReadOnlyDictionary<FinalPieceFamily, int> ReserveCountsByFamily { get; }
    public IReadOnlyList<string> ActivePromotionCatalog { get; }
    public IReadOnlyList<string> ReservePromotionCatalog { get; }
    public IReadOnlyList<string> ReserveOnlyPromotionCatalog { get; }
    public IReadOnlyList<string> ActivePromotionFamilyCatalog { get; }
    public IReadOnlyList<string> ReservePromotionFamilyCatalog { get; }
    public IReadOnlyList<string> ActiveCastlers { get; }
    public IReadOnlyList<CastlingRightMetadata> CastlingRights { get; }
    public int PrimaryKingGrantedMaterial { get; }
    public int PrimaryKingExpectedMaterial { get; }
    public int OwnedExpectedMaterial { get; }
    public int ExactActiveExpectedMaterial { get; }
    public int MissingMaterial { get; }
    public IReadOnlyDictionary<SourcePlacementRole, int> MissingMaterialByRole { get; }
    public IReadOnlyDictionary<FinalPieceFamily, int> MissingMaterialByFamily { get; }
    public int DormantMaterial { get; }
    public int UnallocatedMaterial { get; }
    public int DormantAndUnallocatedMaterial { get { return DormantMaterial + UnallocatedMaterial; } }
    public int UnspentForwardness { get; }

    private static List<string> PromotionFamilies(
      IEnumerable<ProjectedRosterPiece> pieces,
      bool includeRoyal = false)
    {
      string[] order = { "royal", "pawn", "minor", "major", "jack", "queen", "amazon" };
      HashSet<string> found = pieces.SelectMany(piece => piece.PromotionEntitlementFamilies)
        .ToHashSet(StringComparer.Ordinal);
      if (includeRoyal)
        found.Add("royal");
      return order.Where(found.Contains).ToList();
    }
  }

  internal static class GeneratedRosterProjector
  {
    private static readonly SourcePlacementRole[] OrdinaryNonPawnRolePriority =
    {
      SourcePlacementRole.JackSlot,
      SourcePlacementRole.MajorSlot,
      SourcePlacementRole.MinorSlot,
    };

    private static readonly SourcePlacementRole[] PlacementRolePriority =
    {
      SourcePlacementRole.AdditionalRoyal,
      SourcePlacementRole.LockedCastler,
      SourcePlacementRole.JackSlot,
      SourcePlacementRole.MajorSlot,
      SourcePlacementRole.MinorSlot,
    };

    public static ActiveRosterProjection Project(
      GeneratedRoster roster,
      ProjectionGeometry geometry,
      int forwardness,
      ApmwConfig config)
    {
      if (roster == null)
        throw new ArgumentNullException(nameof(roster));
      if (geometry == null)
        throw new ArgumentNullException(nameof(geometry));
      if (config == null)
        throw new ArgumentNullException(nameof(config));
      if (roster.PrimaryKing == null)
        throw new InvalidOperationException("Projection requires the mandatory primary King.");

      var active = new HashSet<string>(StringComparer.Ordinal);
      int remainingBackOptional = geometry.BackOptionalCapacity;
      ActivateRole(roster, SourcePlacementRole.AdditionalRoyal, remainingBackOptional, active);
      remainingBackOptional -= CountActiveRole(roster, active, SourcePlacementRole.AdditionalRoyal);
      ActivateRole(roster, SourcePlacementRole.LockedCastler, remainingBackOptional, active);

      int activeProtectedNonPawns =
        CountActiveRole(roster, active, SourcePlacementRole.AdditionalRoyal) +
        CountActiveRole(roster, active, SourcePlacementRole.LockedCastler);
      int remainingNonPawnCapacity = Math.Max(0, geometry.NonPawnCapacity - activeProtectedNonPawns);
      foreach (SourcePlacementRole role in OrdinaryNonPawnRolePriority)
      {
        int activated = ActivateRole(roster, role, remainingNonPawnCapacity, active);
        remainingNonPawnCapacity -= activated;
      }

      int activeNonPrimaryNonPawns = activeProtectedNonPawns +
        OrdinaryNonPawnRolePriority.Sum(role => CountActiveRole(roster, active, role));
      ActivateRole(
        roster,
        SourcePlacementRole.PawnSlot,
        geometry.ActivePawnCapacity(activeNonPrimaryNonPawns),
        active);

      List<RosterPiece> activePieces = roster.RosterPieces
        .Where(piece => active.Contains(piece.StableId))
        .OrderBy(piece => RolePriority(piece.SourcePlacementRole))
        .ThenBy(piece => piece.SourceOrdinal)
        .ThenBy(piece => piece.StableId, StringComparer.Ordinal)
        .ToList();
      List<RosterPiece> reservePieces = roster.RosterPieces
        .Where(piece => !active.Contains(piece.StableId))
        .OrderBy(piece => RolePriority(piece.SourcePlacementRole))
        .ThenBy(SemanticExpectedMaterial)
        .ThenBy(piece => piece.GrantedMaterial)
        .ThenByDescending(piece => piece.SourceOrdinal)
        .ThenBy(piece => piece.StableId, StringComparer.Ordinal)
        .ToList();

      Dictionary<string, ProjectionCoordinate> coordinates = Place(
        activePieces,
        geometry,
        config);
      int unspentForwardness = ApplyForwardness(
        activePieces,
        coordinates,
        geometry,
        Math.Max(0, forwardness),
        config);

      List<ProjectedRosterPiece> projectedActive = activePieces
        .Select(piece =>
        {
          ProjectionCoordinate coordinate = coordinates[piece.StableId];
          return new ProjectedRosterPiece(
            piece,
            new ProjectedPlacement(
              coordinate,
              geometry.RegionForRank(coordinate.RelativeRank),
              geometry.FormationBandId(coordinate.RelativeRank)));
        })
        .ToList();
      List<ProjectedRosterPiece> projectedReserve = reservePieces
        .Select(piece => new ProjectedRosterPiece(piece, null))
        .ToList();

      ProjectedPlacement primaryPlacement = new ProjectedPlacement(
        new ProjectionCoordinate(geometry.PrimaryKingFile, 0),
        ProjectionRegion.BackRank,
        geometry.FormationBandId(0));
      List<ProjectionRegionUsage> regionUsage = BuildRegionUsage(
        geometry,
        primaryPlacement,
        projectedActive);
      IReadOnlyDictionary<SourcePlacementRole, int> activeByRole = CountsByRole(activePieces, true);
      IReadOnlyDictionary<SourcePlacementRole, int> reserveByRole = CountsByRole(reservePieces, false);
      IReadOnlyDictionary<FinalPieceFamily, int> activeByFamily = CountsByFamily(activePieces);
      IReadOnlyDictionary<FinalPieceFamily, int> reserveByFamily = CountsByFamily(reservePieces);
      List<string> activePromotions = PromotionCatalog(activePieces);
      if (roster.PrimaryKingGrantedMaterial > 0 &&
          roster.PrimaryKing.Notation != null)
      {
        int player = ApmwCore.getInstance().GeriProvider();
        if (player >= 0 && player < roster.PrimaryKing.Notation.Length)
          activePromotions.Add(roster.PrimaryKing.Notation[player]);
        activePromotions = activePromotions.Distinct(StringComparer.Ordinal)
          .OrderBy(value => value, StringComparer.Ordinal).ToList();
      }
      List<string> reservePromotions = PromotionCatalog(reservePieces);
      List<string> reserveOnlyPromotions = reservePromotions.Except(activePromotions, StringComparer.Ordinal)
        .OrderBy(value => value, StringComparer.Ordinal)
        .ToList();
      List<CastlingRightMetadata> castlingRights = projectedActive
        .Where(piece => piece.Placement.Coordinate.RelativeRank == 0)
        .Where(piece => piece.FinalFamily == FinalPieceFamily.Major ||
          piece.FinalFamily == FinalPieceFamily.Jack)
        .Select(piece => new CastlingRightMetadata(piece, piece.Placement.Coordinate))
        .OrderBy(right => right.StableId, StringComparer.Ordinal)
        .ToList();
      int attachedAndUnallocatedMaterial =
        roster.PrimaryKingGrantedMaterial +
        roster.RosterPieces.Sum(piece => piece.GrantedMaterial) +
        roster.UnallocatedMaterial +
        roster.DormantMaterial;
      if (roster.OwnedMaterialLedger.GrantedTotal != attachedAndUnallocatedMaterial)
        throw new InvalidOperationException("Owned material ledger does not match attached, dormant, and unallocated material.");

      return new ActiveRosterProjection(
        geometry,
        roster.PrimaryKing,
        primaryPlacement,
        projectedActive,
        projectedReserve,
        regionUsage,
        activeByRole,
        reserveByRole,
        activeByFamily,
        reserveByFamily,
        activePromotions,
        reservePromotions,
        reserveOnlyPromotions,
        projectedActive
          .Where(piece => piece.LockedCastler)
          .Select(piece => piece.StableId)
          .OrderBy(value => value, StringComparer.Ordinal),
        castlingRights,
        roster.PrimaryKingGrantedMaterial,
        roster.PrimaryKingExpectedMaterial,
        roster.RosterPieces.Sum(SemanticExpectedMaterial),
        roster.PrimaryKingExpectedMaterial + activePieces.Sum(SemanticExpectedMaterial),
        reservePieces.Sum(piece => piece.GrantedMaterial),
        MissingByRole(reservePieces),
        MissingByFamily(reservePieces),
        roster.DormantMaterial,
        roster.UnallocatedMaterial,
        unspentForwardness);
    }

    private static int ActivateRole(
      GeneratedRoster roster,
      SourcePlacementRole role,
      int capacity,
      HashSet<string> active)
    {
      List<RosterPiece> candidates = roster.RosterPieces
        .Where(piece => piece.SourcePlacementRole == role)
        .OrderByDescending(SemanticExpectedMaterial)
        .ThenByDescending(piece => piece.GrantedMaterial)
        .ThenBy(piece => piece.SourceOrdinal)
        .ThenBy(piece => piece.StableId, StringComparer.Ordinal)
        .Take(Math.Max(0, capacity))
        .ToList();
      foreach (RosterPiece piece in candidates)
        active.Add(piece.StableId);
      return candidates.Count;
    }

    private static int CountActiveRole(
      GeneratedRoster roster,
      HashSet<string> active,
      SourcePlacementRole role)
    {
      return roster.RosterPieces.Count(piece =>
        piece.SourcePlacementRole == role && active.Contains(piece.StableId));
    }

    private static Dictionary<string, ProjectionCoordinate> Place(
      List<RosterPiece> activePieces,
      ProjectionGeometry geometry,
      ApmwConfig config)
    {
      var coordinates = new Dictionary<string, ProjectionCoordinate>(StringComparer.Ordinal);
      var availableByRank = Enumerable.Range(0, geometry.HumanFormationRanks)
        .ToDictionary(
          rank => rank,
          rank => Enumerable.Range(0, geometry.Files).ToList());
      availableByRank[0].Remove(geometry.PrimaryKingFile);

      foreach (SourcePlacementRole role in PlacementRolePriority)
      {
        List<RosterPiece> rolePieces = activePieces
          .Where(piece => piece.SourcePlacementRole == role)
          .OrderBy(piece => piece.SourceOrdinal)
          .ThenBy(piece => piece.StableId, StringComparer.Ordinal)
          .ToList();
        IEnumerable<int> ranks = role == SourcePlacementRole.AdditionalRoyal ||
          role == SourcePlacementRole.LockedCastler
            ? new[] { 0 }
            : Enumerable.Range(0, geometry.MixedBandCount + 1);
        PlaceAcrossRanks(rolePieces, ranks, availableByRank, geometry, config, coordinates);
      }

      List<RosterPiece> pawns = activePieces
        .Where(piece => piece.SourcePlacementRole == SourcePlacementRole.PawnSlot)
        .OrderBy(piece => piece.SourceOrdinal)
        .ThenBy(piece => piece.StableId, StringComparer.Ordinal)
        .ToList();
      PlaceAcrossRanks(
        pawns,
        Enumerable.Range(1, geometry.HumanFormationRanks - 1),
        availableByRank,
        geometry,
        config,
        coordinates);

      if (coordinates.Count != activePieces.Count)
        throw new InvalidOperationException("Projection capacity selected pieces that could not be placed.");
      return coordinates;
    }

    private static void PlaceAcrossRanks(
      List<RosterPiece> pieces,
      IEnumerable<int> ranks,
      Dictionary<int, List<int>> availableByRank,
      ProjectionGeometry geometry,
      ApmwConfig config,
      Dictionary<string, ProjectionCoordinate> coordinates)
    {
      List<RosterPiece> remaining = pieces.ToList();
      foreach (int rank in ranks)
      {
        List<int> files = availableByRank[rank];
        if (remaining.Count == 0)
          break;
        if (files.Count == 0)
          continue;

        string band = geometry.FormationBandId(rank);
        SourcePlacementRole role = remaining[0].SourcePlacementRole;
        // Semantic projection is presentation-mode independent. Live Chaos coordinate
        // randomization remains in the later board-placement adapter.
        CounterBasedSeedSeries series = ApmwSeedSeries.SemanticPresentation(
          config,
          PlacementSeriesId(role, band));
        int count = Math.Min(remaining.Count, files.Count);
        for (int index = 0; index < count; index++)
        {
          RosterPiece piece = remaining[0];
          remaining.RemoveAt(0);
          int fileIndex = series.Index(index, files.Count);
          int file = files[fileIndex];
          files.RemoveAt(fileIndex);
          coordinates.Add(piece.StableId, new ProjectionCoordinate(file, rank));
        }
      }
      if (remaining.Count > 0)
        throw new InvalidOperationException("Projection role exceeded its assigned region capacity.");
    }

    internal static string PlacementSeriesId(SourcePlacementRole role, string formationBand)
    {
      if (string.IsNullOrWhiteSpace(formationBand))
        throw new ArgumentException("Formation band is required.", nameof(formationBand));
      return "placement." + SourceRoleId(role) + "." + formationBand;
    }

    private static int ApplyForwardness(
      List<RosterPiece> activePieces,
      Dictionary<string, ProjectionCoordinate> coordinates,
      ProjectionGeometry geometry,
      int requested,
      ApmwConfig config)
    {
      int remaining = requested;
      int edgeCount = geometry.Ranks - 5;
      for (int waveLength = edgeCount; waveLength >= 1 && remaining > 0; waveLength--)
      {
        for (int edge = 0; edge < waveLength && remaining > 0; edge++)
        {
          int sourceRank = edge + 1;
          int targetRank = sourceRank + 1;
          HashSet<ProjectionCoordinate> occupied = coordinates.Values.ToHashSet();
          List<RosterPiece> movable = activePieces
            .Where(piece => piece.SourcePlacementRole == SourcePlacementRole.PawnSlot)
            .Where(piece => coordinates[piece.StableId].RelativeRank == sourceRank)
            .Where(piece =>
            {
              ProjectionCoordinate current = coordinates[piece.StableId];
              return !occupied.Contains(new ProjectionCoordinate(current.File, targetRank));
            })
            .OrderBy(piece => piece.StableId, StringComparer.Ordinal)
            .ToList();
          CounterBasedSeedSeries series = ApmwSeedSeries.SemanticPresentation(
            config,
            PlacementSeriesId(
              SourcePlacementRole.PawnSlot,
              "forwardness-" + sourceRank + "-" + targetRank));
          int counter = 0;
          while (remaining > 0 && movable.Count > 0)
          {
            int selectedIndex = series.Index(counter++, movable.Count);
            RosterPiece piece = movable[selectedIndex];
            movable.RemoveAt(selectedIndex);
            ProjectionCoordinate current = coordinates[piece.StableId];
            ProjectionCoordinate target = new ProjectionCoordinate(current.File, targetRank);
            if (coordinates.Values.Contains(target))
              continue;
            coordinates[piece.StableId] = target;
            remaining--;
          }
        }
      }
      return remaining;
    }

    private static List<ProjectionRegionUsage> BuildRegionUsage(
      ProjectionGeometry geometry,
      ProjectedPlacement primary,
      List<ProjectedRosterPiece> active)
    {
      var output = new List<ProjectionRegionUsage>();
      for (int rank = 0; rank < geometry.HumanFormationRanks; rank++)
      {
        int used = active.Count(piece => piece.Placement.Coordinate.RelativeRank == rank);
        if (rank == primary.Coordinate.RelativeRank)
          used++;
        output.Add(new ProjectionRegionUsage(
          geometry.FormationBandId(rank),
          geometry.RegionForRank(rank),
          geometry.Files,
          used));
      }
      return output;
    }

    private static IReadOnlyDictionary<SourcePlacementRole, int> CountsByRole(
      IEnumerable<RosterPiece> pieces,
      bool includePrimary)
    {
      Dictionary<SourcePlacementRole, int> counts = Enum.GetValues(typeof(SourcePlacementRole))
        .Cast<SourcePlacementRole>()
        .ToDictionary(role => role, role => 0);
      foreach (RosterPiece piece in pieces)
        counts[piece.SourcePlacementRole]++;
      if (includePrimary)
        counts[SourcePlacementRole.PrimaryRoyal] = 1;
      return new ReadOnlyDictionary<SourcePlacementRole, int>(counts);
    }

    private static IReadOnlyDictionary<FinalPieceFamily, int> CountsByFamily(
      IEnumerable<RosterPiece> pieces)
    {
      Dictionary<FinalPieceFamily, int> counts = Enum.GetValues(typeof(FinalPieceFamily))
        .Cast<FinalPieceFamily>()
        .ToDictionary(family => family, family => 0);
      foreach (RosterPiece piece in pieces.Where(piece => piece.FinalFamily.HasValue))
        counts[piece.FinalFamily.Value]++;
      return new ReadOnlyDictionary<FinalPieceFamily, int>(counts);
    }

    private static IReadOnlyDictionary<SourcePlacementRole, int> MissingByRole(
      IEnumerable<RosterPiece> reserve)
    {
      Dictionary<SourcePlacementRole, int> values = Enum.GetValues(typeof(SourcePlacementRole))
        .Cast<SourcePlacementRole>()
        .ToDictionary(role => role, role => 0);
      foreach (RosterPiece piece in reserve)
        values[piece.SourcePlacementRole] += piece.GrantedMaterial;
      return new ReadOnlyDictionary<SourcePlacementRole, int>(values);
    }

    private static IReadOnlyDictionary<FinalPieceFamily, int> MissingByFamily(
      IEnumerable<RosterPiece> reserve)
    {
      Dictionary<FinalPieceFamily, int> values = Enum.GetValues(typeof(FinalPieceFamily))
        .Cast<FinalPieceFamily>()
        .ToDictionary(family => family, family => 0);
      foreach (RosterPiece piece in reserve.Where(piece => piece.FinalFamily.HasValue))
        values[piece.FinalFamily.Value] += piece.GrantedMaterial;
      return new ReadOnlyDictionary<FinalPieceFamily, int>(values);
    }

    private static List<string> PromotionCatalog(IEnumerable<RosterPiece> pieces)
    {
      return pieces.SelectMany(piece => piece.PromotionEntitlements)
        .Distinct(StringComparer.Ordinal)
        .OrderBy(value => value, StringComparer.Ordinal)
        .ToList();
    }

    private static int RolePriority(SourcePlacementRole role)
    {
      switch (role)
      {
        case SourcePlacementRole.PrimaryRoyal: return 0;
        case SourcePlacementRole.AdditionalRoyal: return 1;
        case SourcePlacementRole.LockedCastler: return 2;
        case SourcePlacementRole.JackSlot: return 3;
        case SourcePlacementRole.MajorSlot: return 4;
        case SourcePlacementRole.MinorSlot: return 5;
        case SourcePlacementRole.PawnSlot: return 6;
        default: throw new ArgumentOutOfRangeException(nameof(role));
      }
    }

    private static int SemanticExpectedMaterial(RosterPiece piece)
    {
      return piece.FinalFamily.HasValue
        ? OwnedRosterGeneration.ExpectedMaterial(piece.FinalFamily.Value)
        : piece.FinalExpectedMaterial;
    }

    internal static string SourceRoleId(SourcePlacementRole role)
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
  }

}
