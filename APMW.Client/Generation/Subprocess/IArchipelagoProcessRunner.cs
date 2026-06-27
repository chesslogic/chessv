using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public interface IArchipelagoProcessRunner
  {
    Task<ArchipelagoProcessResult> RunAsync(
      ArchipelagoGenerationCommand command,
      CancellationToken cancellationToken);
  }
}
