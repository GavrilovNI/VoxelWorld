using Sandbox;
using Sandcube.Mth;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Generation;

public class WorldAutoLoader : Component, IWorldInitializable
{
    [Property] public IWorldAccessor? World { get; set; } = null;
    [Property] public BBoxInt Bounds { get; set; } = BBoxInt.FromPositionAndRadius(Vector3Int.Zero, 2);
    [Property] public float TimeBetweenSuccessfulAttempts { get; set; } = 1f;

    protected HashSet<Vector3Int> LoadedChunks = new();
    protected Task? LoadingTask;

    protected bool IsStarted = false;

    public void InitializeWorld(IWorldAccessor world) => World = world;

    protected override void OnUpdate()
    {
        bool worldIsLoaded = SandcubeGame.LoadingStatus == LoadingStatus.Loaded && World is not null;
        if(worldIsLoaded && !IsStarted)
            StartLoading();
        else if(!worldIsLoaded && IsStarted)
            StopLoading();

        if(!IsStarted)
            return;

        bool loadedLastChunks = LoadingTask is null || LoadingTask.IsCompleted;
        if(!worldIsLoaded || !loadedLastChunks)
            return;

        var chunkPositionsToLoad = GetChunkPositionsToLoad();
        LoadingTask = LoadChunks(chunkPositionsToLoad);
    }

    protected override void OnDestroy()
    {
        StopLoading();
    }

    protected void StartLoading()
    {
        if(IsStarted)
            return;

        IsStarted = true;
        World!.ChunkLoaded += OnChunkLoaded;
        World.ChunkUnloaded += OnChunkUnloaded;
    }

    protected void StopLoading()
    {
        if(!IsStarted)
            return;

        IsStarted = false;
        World!.ChunkLoaded -= OnChunkLoaded;
        World.ChunkUnloaded -= OnChunkUnloaded;
    }

    protected virtual void OnChunkLoaded(Vector3Int position)
    {
        LoadedChunks.Add(position);
    }

    protected virtual void OnChunkUnloaded(Vector3Int position)
    {
        LoadedChunks.Remove(position);
    }

    protected async Task LoadChunks(IReadOnlySet<Vector3Int> positions)
    {
        if(positions.Count == 0)
            return;

        await World!.CreateChunksSimultaneously(positions);
    }

    protected virtual HashSet<Vector3Int> GetChunkPositionsToLoad()
    {
        var centralChunkPositrion = World!.GetChunkPosition(Transform.Position);
        HashSet<Vector3Int> chunksToLoad = (Bounds + centralChunkPositrion).GetPositions()
            .Where(p => !LoadedChunks.Contains(p) && World.IsChunkInLimits(p))
            .ToHashSet();

        HashSet<Vector3Int> except = new();
        foreach(var position in chunksToLoad.ToList())
        {
            if(World.HasChunk(position))
            {
                LoadedChunks.Add(position);
                except.Add(position);
            }
        }
        chunksToLoad.ExceptWith(except);

        return chunksToLoad;
    }
}
