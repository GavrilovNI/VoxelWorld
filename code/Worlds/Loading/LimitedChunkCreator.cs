using Sandbox;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Loading;

public class LimitedChunkCreator : ChunkCreator
{
    [Property] public int MaxChunksToCreateAtTime { get; set; } = 15;

    protected int ActiveCreatingChunkCount = 0;
    [Property] public int ActiveCreatingChunks => ActiveCreatingChunkCount;

    protected record class ChunkCreatingData(ChunkCreationData CreationData, TaskCompletionSource<Chunk> TaskCompletionSource,
        CancellationTokenSource CancellationTokenSource);
    protected readonly ConcurrentQueue<ChunkCreatingData> ChunksToCreate = new();
    protected readonly CancellationTokenSource CommonCancellationTokenSource = new();


    public override Task<Chunk> CreateChunk(ChunkCreationData creationData, CancellationToken cancellationToken)
    {
        TaskCompletionSource<Chunk> taskCompletionSource = new();
        CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CommonCancellationTokenSource.Token);

        ChunkCreatingData creatingData = new(creationData, taskCompletionSource, cancellationTokenSource);
        ChunksToCreate.Enqueue(creatingData);
        return taskCompletionSource.Task;
    }

    protected override void OnUpdateInner()
    {
        base.OnUpdateInner();
        CreateQueuedChunks();
    }

    protected override void OnDestroyInner()
    {
        base.OnDestroyInner();
        CommonCancellationTokenSource.Cancel();
        CommonCancellationTokenSource.Dispose();
    }

    // Call only in game thread
    protected virtual void CreateQueuedChunks()
    {
        ThreadSafe.AssertIsMainThread();

        int creatingChunksCount = ActiveCreatingChunkCount;
        while(creatingChunksCount < MaxChunksToCreateAtTime &&
            ChunksToCreate.TryDequeue(out var chunkPositionToCreate))
        {
            _ = CreateChunk(chunkPositionToCreate);
            creatingChunksCount++;
        }
    }

    // Call only in game thread
    protected virtual Task<Chunk> CreateChunk(ChunkCreatingData creatingData)
    {
        ThreadSafe.AssertIsMainThread();

        Interlocked.Increment(ref ActiveCreatingChunkCount);

        var position = creatingData.CreationData.Position;
        var cancellationToken = creatingData.CancellationTokenSource.Token;

        var task = base.CreateChunk(creatingData.CreationData, cancellationToken)
            .ContinueWith(t =>
            {
                Interlocked.Decrement(ref ActiveCreatingChunkCount);

                if(t.IsCompletedSuccessfully)
                    creatingData.TaskCompletionSource.TrySetResult(t.Result);
                else
                    creatingData.TaskCompletionSource.TrySetCanceled();
                creatingData.CancellationTokenSource.Dispose();

                cancellationToken.ThrowIfCancellationRequested();
                return t.Result;
            });
        cancellationToken.ThrowIfCancellationRequested();

        return task;
    }
}
