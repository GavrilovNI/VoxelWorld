using Sandbox;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Loading;

public class LimitedChunkLoader : ChunkLoader
{
    [Property] public int MaxChunksToLoadAtTime { get; set; } = 15;

    protected int ActiveLoadingChunkCount = 0;
    [Property] public int ActiveLoadingChunks => ActiveLoadingChunkCount;

    protected record class ChunkLoadingData(ChunkCreationData CreationData, TaskCompletionSource<Chunk> TaskCompletionSource,
        CancellationTokenSource CancellationTokenSource);
    protected readonly ConcurrentQueue<ChunkLoadingData> ChunksToLoad = new();
    protected readonly CancellationTokenSource CommonCancellationTokenSource = new();


    public override Task<Chunk> LoadChunk(ChunkCreationData creationData, CancellationToken cancellationToken)
    {
        TaskCompletionSource<Chunk> taskCompletionSource = new();
        CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CommonCancellationTokenSource.Token);

        ChunkLoadingData loadingData = new(creationData, taskCompletionSource, cancellationTokenSource);
        ChunksToLoad.Enqueue(loadingData);
        return taskCompletionSource.Task;
    }

    protected override void OnUpdateInner()
    {
        base.OnUpdateInner();
        LoadQueuedChunks();
    }

    protected override void OnDestroyInner()
    {
        base.OnDestroyInner();
        CommonCancellationTokenSource.Cancel();
        CommonCancellationTokenSource.Dispose();
    }

    // Call only in game thread
    protected virtual void LoadQueuedChunks()
    {
        int loadingChunksCount = ActiveLoadingChunkCount;
        while(loadingChunksCount < MaxChunksToLoadAtTime &&
            ChunksToLoad.TryDequeue(out var chunkPositionToLoad))
        {
            _ = LoadChunk(chunkPositionToLoad);
            loadingChunksCount++;
        }
    }

    // Call only in game thread
    protected virtual Task<Chunk> LoadChunk(ChunkLoadingData loadingData)
    {
        Interlocked.Increment(ref ActiveLoadingChunkCount);

        var position = loadingData.CreationData.Position;
        var cancellationToken = loadingData.CancellationTokenSource.Token;

        var task = base.LoadChunk(loadingData.CreationData, cancellationToken)
            .ContinueWith(t =>
            {
                Interlocked.Decrement(ref ActiveLoadingChunkCount);

                if(t.IsCompletedSuccessfully)
                    loadingData.TaskCompletionSource.TrySetResult(t.Result);
                else
                    loadingData.TaskCompletionSource.TrySetCanceled();
                loadingData.CancellationTokenSource.Dispose();

                cancellationToken.ThrowIfCancellationRequested();
                return t.Result;
            });
        cancellationToken.ThrowIfCancellationRequested();

        return task;
    }
}
