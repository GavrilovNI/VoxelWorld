using Sandbox;
using VoxelWorld.Entities;
using VoxelWorld.Mth;
using VoxelWorld.Worlds.Creation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace VoxelWorld.Worlds;

public interface IWorldAccessor : IWorldProvider, IBlockStateAccessor
{
    GameObject GameObject { get; }
    Random Random { get; }

    Task CreateChunk(Vector3IntB chunkPosition, ChunkCreationStatus creationStatus = ChunkCreationStatus.Finishing);
    Task CreateChunksSimultaneously(IEnumerable<Vector3IntB> chunkPositions, ChunkCreationStatus creationStatus = ChunkCreationStatus.Finishing);
    Task AddEntity(Entity entity);
    bool RemoveEntity(Entity entity);
    void Tick();
}
