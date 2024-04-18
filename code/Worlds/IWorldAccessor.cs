using Sandbox;
using VoxelWorld.Entities;
using VoxelWorld.Mth;
using VoxelWorld.Worlds.Creation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using VoxelWorld.Worlds.Data;

namespace VoxelWorld.Worlds;

public interface IWorldAccessor : IWorldProvider, IBlockStateAccessor, IAdditionalDataAccessor
{
    GameObject GameObject { get; }
    Random Random { get; }

    bool IsLoadedAt(Vector3IntB blockPosition);
    Task CreateChunk(Vector3IntB chunkPosition, ChunkCreationStatus creationStatus = ChunkCreationStatus.Finishing);
    Task CreateChunksSimultaneously(IEnumerable<Vector3IntB> chunkPositions, ChunkCreationStatus creationStatus = ChunkCreationStatus.Finishing);
    Task AddEntity(Entity entity);
    bool RemoveEntity(Entity entity);
    void Tick();
}
