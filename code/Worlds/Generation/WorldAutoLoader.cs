using Sandbox;
using Sandcube.Mth;
using Sandcube.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Generation;

public class WorldAutoLoader : ThreadHelpComponent
{
    [Property] public World? World { get; set; } = null;
    [Property] public Vector3Int Distance { get; set; } = 2;
    [Property] public float TimeBetweenSuccessfulAttempts { get; set; } = 1f;

    protected Task<int>? LoadingTask;
    protected TimeUntil TimeUntilNextAttempt;

    protected override void OnUpdateInner()
    {
        bool worldIsLoaded = SandcubeGame.IsStarted && World is not null;
        bool loadedLastChunks = LoadingTask is null || LoadingTask.IsCompleted;

        if(!worldIsLoaded || !loadedLastChunks)
            return;

        if(TimeUntilNextAttempt > 0)
            return;

        LoadingTask = LoadChunks();
    }

    protected Task<int> LoadChunks()
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

        var task = World.LoadChunksSimultaneously(chunksToLoad);
        _ = task.ContinueWith(t => RunInGameThread((ct) => OnChunksLoaded(t.Result, chunksToLoad.Count)));
        return task;
    }

    protected virtual void OnChunksLoaded(int loadedCount, int requestedCount)
    {
        ThreadSafe.AssertIsMainThread();
        if(requestedCount == loadedCount)
            TimeUntilNextAttempt = TimeBetweenSuccessfulAttempts;
    }

}
