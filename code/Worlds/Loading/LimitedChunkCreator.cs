using Sandbox;
using System;
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


    public override Task<Chunk> LoadOrCreateChunk(ChunkCreationData creationData, CancellationToken cancellationToken)
    {
        TaskCompletionSource<Chunk> taskCompletionSource = new();
        CancellationTokenSource cancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CommonCancellationTokenSource.Token);

        ChunkCreatingData creatingData = new(creationData, taskCompletionSource, cancellationTokenSource);
        ChunksToCreate.Enqueue(creatingData);
        return taskCompletionSource.Task;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        CreateQueuedChunks();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
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
    protected virtual async Task<Chunk> CreateChunk(ChunkCreatingData creatingData)
    {
        ThreadSafe.AssertIsMainThread();

        Interlocked.Increment(ref ActiveCreatingChunkCount);
        var cancellationToken = creatingData.CancellationTokenSource.Token;

        Chunk chunk;
        try
        {
            chunk = await base.LoadOrCreateChunk(creatingData.CreationData, cancellationToken);
            Interlocked.Decrement(ref ActiveCreatingChunkCount);
            creatingData.CancellationTokenSource.Dispose();
            creatingData.TaskCompletionSource.TrySetResult(chunk);
        }
        catch (OperationCanceledException)
        {
            Interlocked.Decrement(ref ActiveCreatingChunkCount);
            creatingData.CancellationTokenSource.Dispose();
            creatingData.TaskCompletionSource.TrySetCanceled();
            throw;
        }
        return chunk;
    }
}
