using Sandbox;
using Sandcube.Mth;
using Sandcube.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Generation;

public class WorldAutoLoader : ThreadHelpComponent, IWorldInitializable
{
    [Property] public World? World { get; set; } = null;
    [Property] public Vector3Int Distance { get; set; } = 2;
    [Property] public float TimeBetweenSuccessfulAttempts { get; set; } = 1f;

    protected Dictionary<Vector3Int, Chunk> LoadedChunks = new();
    protected Task<int>? LoadingTask;

    public void InitializeWorld(World world) => World = world;

    protected override void OnUpdateInner()
    {
        bool worldIsLoaded = SandcubeGame.LoadingStatus == LoadingStatus.Loaded && World is not null;
        bool loadedLastChunks = LoadingTask is null || LoadingTask.IsCompleted;

        if(!worldIsLoaded || !loadedLastChunks)
            return;

        RemoveInvalidLoadedChunks();
        var chunkPositionsToLoad = GetChunkPositionsToLoad();
        LoadingTask = LoadChunks(chunkPositionsToLoad);
    }

    protected Task<int> LoadChunks(IReadOnlySet<Vector3Int> positions)
    {
        if(positions.Count == 0)
            return Task.FromResult(0);

        var task = World!.LoadChunksSimultaneously(positions);
        _ = task.ContinueWith(t => RunInGameThread((ct) =>
            OnChunksLoaded(t.Result, positions.Count, positions)));
        return task;
    }

    protected virtual HashSet<Vector3Int> GetChunkPositionsToLoad()
    {
        var centralChunkPositrion = World!.GetChunkPosition(Transform.Position);
        var start = centralChunkPositrion - Distance;
        var end = centralChunkPositrion + Distance;

        HashSet<Vector3Int> chunksToLoad = new();
        for(int x = start.x; x <= end.x; ++x)
        {
            for(int y = start.y; y <= end.y; ++y)
            {
                for(int z = start.z; z <= end.z; ++z)
                {
                    var chunkPosition = new Vector3Int(x, y, z);
                    chunksToLoad.Add(chunkPosition);
                }
            }
        }

        chunksToLoad.ExceptWith(LoadedChunks.Where(kv => kv.Value.IsValid).Select(kv => kv.Key));
        return chunksToLoad;
    }

    protected virtual void OnChunksLoaded(int loadedCount, int requestedCount, IReadOnlySet<Vector3Int> loadedChunkPositions)
    {
        ThreadSafe.AssertIsMainThread();
        AddLoadedChunks(loadedChunkPositions);
    }

    protected virtual void AddLoadedChunks(IReadOnlySet<Vector3Int> loadedChunkPositions)
    {
        foreach(var position in loadedChunkPositions)
        {
            var chunk = World!.GetChunk(position);
            if(chunk.IsValid())
                LoadedChunks[position] = chunk;
            else
                LoadedChunks.Remove(position);
        }
    }

    protected virtual void RemoveInvalidLoadedChunks()
    {
        foreach(var (position, _) in LoadedChunks.Where(kv => !kv.Value.IsValid))
            LoadedChunks.Remove(position);
    }
}
