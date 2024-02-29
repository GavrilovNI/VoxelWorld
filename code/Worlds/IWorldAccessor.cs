using Sandcube.Entities;
using Sandcube.Mth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sandcube.Worlds;

public interface IWorldAccessor : IWorldProvider, IBlockStateAccessor
{
    Task LoadChunk(Vector3Int chunkPosition, bool awaitModelUpdate = false);
    Task LoadChunksSimultaneously(IReadOnlySet<Vector3Int> chunkPositions, bool awaitModelUpdate = false);
    void AddEntity(Entity entity);
    void RemoveEntity(Entity entity);
    void Tick();
}
