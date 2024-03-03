using Sandbox;
using Sandcube.Entities;
using Sandcube.Mth;
using Sandcube.Registries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sandcube.Worlds;

public interface IWorldAccessor : IWorldProvider, IBlockStateAccessor
{
    ModedId Id { get; }
    GameObject GameObject { get; }

    Task LoadChunk(Vector3Int chunkPosition, bool awaitModelUpdate = false);
    Task LoadChunksSimultaneously(IReadOnlySet<Vector3Int> chunkPositions, bool awaitModelUpdate = false);
    void AddEntity(Entity entity);
    bool RemoveEntity(Guid entityId);
    bool RemoveEntity(Entity entity);
    void Tick();
}
