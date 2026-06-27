using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.APChessV.Generation.Subprocess
{
  public interface IArchipelagoServerProcessRunner
  {
    Task<IArchipelagoServerProcess> StartAsync(
      ArchipelagoGenerationCommand command,
      CancellationToken cancellationToken);
  }
}
