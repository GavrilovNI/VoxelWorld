using Sandbox;
using System.Threading;
using System.Threading.Tasks;

namespace VoxelWorld.Worlds.Creation;

public abstract class ChunkCreationStage : Component
{
    public abstract Task<bool> TryProcess(Chunk chunk, CancellationToken cancellationToken);
}
