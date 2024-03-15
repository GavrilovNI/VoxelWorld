using Sandbox;
using Sandcube.Entities;
using Sandcube.Mth;
using Sandcube.Worlds.Creation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sandcube.Worlds;

public interface IWorldAccessor : IWorldProvider, IBlockStateAccessor
{
    GameObject GameObject { get; }

    Task CreateChunk(Vector3Int chunkPosition, ChunkCreationStatus creationStatus = ChunkCreationStatus.Finishing);
    Task CreateChunksSimultaneously(IEnumerable<Vector3Int> chunkPositions, ChunkCreationStatus creationStatus = ChunkCreationStatus.Finishing);
    Task AddEntity(Entity entity);
    bool RemoveEntity(Entity entity);
    void Tick();
}
