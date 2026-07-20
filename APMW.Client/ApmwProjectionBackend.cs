using ChessV.Base;
using System;
using System.Linq;
using System.Threading;

namespace Archipelago.APChessV
{
  internal interface IApmwProjectionBackend
  {
    GeneratedRoster GenerateOwnedRoster();
    ActiveRosterProjection Project(ProjectionGeometry geometry);
    void Invalidate();
  }

  internal sealed class CurrentCSharpProjectionBackend : IApmwProjectionBackend
  {
    public GeneratedRoster GenerateOwnedRoster()
    {
      return OwnedRosterGeneration.Generate();
    }

    public ActiveRosterProjection Project(ProjectionGeometry geometry)
    {
      return GeneratedRosterProjector.Project(
        GenerateOwnedRoster(),
        geometry,
        System.Math.Max(0, ChessV.Base.ApmwCore.getInstance().foundPawnForwardness),
        ApmwConfig.getInstance());
    }

    public void Invalidate()
    {
    }
  }

  internal sealed class ApmwSidecarProjectionBackend : IApmwProjectionBackend
  {
    private readonly ApmwSidecarBatchCache cache;
    private readonly Func<ApmwSidecarInputSnapshot> snapshotProvider;
    private readonly Func<ApmwSidecarPresentationAdapter> adapterProvider;

    public ApmwSidecarProjectionBackend(
      IApmwSidecarRunner runner,
      ApmwSidecarIdentity identity,
      ApmwCore core,
      ApmwConfig config,
      ApmwContractV2 contract)
      : this(
          new ApmwSidecarBatchCache(runner, identity),
          () => ApmwSidecarSnapshotCapture.Capture(core, config, contract),
          () => new ApmwSidecarPresentationAdapter(core, config))
    {
    }

    internal ApmwSidecarProjectionBackend(
      ApmwSidecarBatchCache cache,
      Func<ApmwSidecarInputSnapshot> snapshotProvider,
      Func<ApmwSidecarPresentationAdapter> adapterProvider)
    {
      this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
      this.snapshotProvider = snapshotProvider ?? throw new ArgumentNullException(nameof(snapshotProvider));
      this.adapterProvider = adapterProvider ?? throw new ArgumentNullException(nameof(adapterProvider));
    }

    public GeneratedRoster GenerateOwnedRoster()
    {
      ApmwSidecarSuccessResponse response = GetBatch();
      return adapterProvider().CreateOwnedRoster(response.Results[0].Projection);
    }

    public ActiveRosterProjection Project(ProjectionGeometry geometry)
    {
      if (geometry == null)
        throw new ArgumentNullException(nameof(geometry));
      ApmwSidecarProjectionResult result = GetBatch().Results.Single(
        item => item.GeometryStage == geometry.StageId);
      return adapterProvider().CreateProjection(result.Projection);
    }

    public void Invalidate()
    {
      cache.Invalidate();
    }

    private ApmwSidecarSuccessResponse GetBatch()
    {
      return cache.GetAsync(snapshotProvider(), CancellationToken.None)
        .GetAwaiter()
        .GetResult();
    }
  }
}
