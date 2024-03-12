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

    Task CreateChunk(Vector3Int chunkPosition);
    Task AddEntity(Entity entity);
    bool RemoveEntity(Entity entity);
    void Tick();
}
