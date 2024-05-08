using Sandbox;
using VoxelWorld.Data;
using VoxelWorld.IO.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace VoxelWorld.Worlds.Creation;

public class ChunkLoader : ChunkCreationStage, IWorldInitializationListener
{
    [Property] protected World World { get; set; } = null!;

    protected BaseFileSystem WorldFileSystem { get; private set; } = null!;
    protected WorldOptions WorldOptions { get; private set; }

    public void OnWorldInitialized(World world)
    {
        WorldFileSystem = world.WorldFileSystem!;
        WorldOptions = world.Options;
    }

    public override async Task<bool> TryProcess(Chunk chunk, CancellationToken cancellationToken)
    {
        if(WorldFileSystem is null)
            return false;

        if(WorldOptions.ChunkSize != chunk.Size)
            throw new InvalidOperationException($"Can't load chunk, saved chunk size {WorldOptions.ChunkSize} is not equal to chunk size {chunk.Size}");

        var worldSaveHelper = new WorldSaveHelper(WorldFileSystem);
        var regionalSaveHelper = worldSaveHelper.GetRegionalHelper(WorldSaveHelper.BlocksRegionName, WorldOptions.RegionSize);

        cancellationToken.ThrowIfCancellationRequested();

        var loaded = await Task.RunInThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool loaded = regionalSaveHelper.TryLoadOneChunkOnly(chunk.Position, out var chunkTag);// TODO: load all region and cache it?

            cancellationToken.ThrowIfCancellationRequested();
            if(loaded)
                await chunk.Load(chunkTag);

            return loaded;
        });

        return loaded;
    }
}
