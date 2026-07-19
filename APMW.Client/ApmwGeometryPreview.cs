using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Archipelago.APChessV
{
  public sealed class ApmwGeometryPreview
  {
    internal ApmwGeometryPreview(
      string stageId,
      int activeCount,
      int reserveCount,
      int missingMaterial,
      int dormantMaterial,
      int unallocatedMaterial,
      int unspentForwardness,
      IReadOnlyDictionary<string, int> reserveCountsByRole,
      IReadOnlyDictionary<string, int> reserveCountsByFamily)
    {
      StageId = stageId;
      ActiveCount = activeCount;
      ReserveCount = reserveCount;
      MissingMaterial = missingMaterial;
      DormantMaterial = dormantMaterial;
      UnallocatedMaterial = unallocatedMaterial;
      UnspentForwardness = unspentForwardness;
      ReserveCountsByRole = reserveCountsByRole;
      ReserveCountsByFamily = reserveCountsByFamily;
    }

    public string StageId { get; }
    public int ActiveCount { get; }
    public int ReserveCount { get; }
    public int MissingMaterial { get; }
    public int DormantMaterial { get; }
    public int UnallocatedMaterial { get; }
    public int UnspentForwardness { get; }
    public IReadOnlyDictionary<string, int> ReserveCountsByRole { get; }
    public IReadOnlyDictionary<string, int> ReserveCountsByFamily { get; }

    internal static ApmwGeometryPreview FromProjection(ActiveRosterProjection projection)
    {
      return new ApmwGeometryPreview(
        projection.Geometry.StageId,
        projection.ActivePieces.Count + 1,
        projection.ReservePieces.Count,
        projection.MissingMaterial,
        projection.DormantMaterial,
        projection.UnallocatedMaterial,
        projection.UnspentForwardness,
        ReadOnlyStringCounts(projection.ReserveCountsByRole),
        ReadOnlyStringCounts(projection.ReserveCountsByFamily));
    }

    private static IReadOnlyDictionary<string, int> ReadOnlyStringCounts<T>(
      IReadOnlyDictionary<T, int> counts)
    {
      return new ReadOnlyDictionary<string, int>(
        counts.ToDictionary(
          pair => Identifier(pair.Key),
          pair => pair.Value,
          System.StringComparer.Ordinal));
    }

    private static string Identifier<T>(T value)
    {
      if (value is SourcePlacementRole role)
      {
        return role switch
        {
          SourcePlacementRole.PrimaryRoyal => "primary-royal",
          SourcePlacementRole.AdditionalRoyal => "additional-royal",
          SourcePlacementRole.LockedCastler => "locked-castler",
          SourcePlacementRole.JackSlot => "jack-slot",
          SourcePlacementRole.MajorSlot => "major-slot",
          SourcePlacementRole.MinorSlot => "minor-slot",
          SourcePlacementRole.PawnSlot => "pawn-slot",
          _ => role.ToString(),
        };
      }

      if (value is FinalPieceFamily family)
        return family.ToString().ToLowerInvariant();
      return value.ToString();
    }
  }
}
