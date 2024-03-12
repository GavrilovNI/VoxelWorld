using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Creation;

public class ChunkModelAwaiter : ChunkCreationStage
{
    public override async Task<bool> TryProcess(Chunk chunk, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.MainThread();
        cancellationToken.ThrowIfCancellationRequested();
        chunk.GameObject.Enabled = true;
        cancellationToken.ThrowIfCancellationRequested();
        await chunk.GetModelUpdateTask();
        return true;
    }
}
