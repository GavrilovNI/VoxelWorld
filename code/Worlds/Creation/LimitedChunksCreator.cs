using Sandbox;
using Sandcube.Mth;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Creation;

public class LimitedChunksCreator : ChunksCreator
{
    [Property] public int MaxCountToProccess { get; set; } = 15;
    [Property] public int ProcessingCount
    {
        get
        {
            lock(ProcessingChunks)
            {
                return ProcessingChunks.Count;
            }
        }
    }

    protected record class ChunkCreatingData(Vector3Int Position,
        TaskCompletionSource<Chunk> TaskCompletionSource,
        CancellationToken CancellationToken);

    protected record class ChunkFinishingData(Chunk Chunk,
        TaskCompletionSource<Chunk> TaskCompletionSource,
        CancellationToken CancellationToken);

    protected readonly ConcurrentQueue<ChunkFinishingData> ChunksToFinish = new();
    protected readonly ConcurrentQueue<ChunkCreatingData> ChunksToCreate = new();
    protected readonly HashSet<Vector3Int> ProcessingChunks = new();

    protected override Task<Chunk> CreateChunk(Vector3Int position, CancellationToken cancellationToken)
    {
        TaskCompletionSource<Chunk> taskCompletionSource = new();
        ChunkCreatingData creatingData = new(position, taskCompletionSource, cancellationToken);
        ChunksToCreate.Enqueue(creatingData);
        return taskCompletionSource.Task;
    }

    protected override Task<Chunk> FinishChunk(Chunk chunk, CancellationToken cancellationToken)
    {
        lock(ProcessingChunks)
        {
            if(ProcessingChunks.Contains(chunk.Position))
                return base.FinishChunk(chunk, cancellationToken);
        }

        TaskCompletionSource<Chunk> taskCompletionSource = new();
        ChunkFinishingData finishingData = new(chunk, taskCompletionSource, cancellationToken);
        ChunksToFinish.Enqueue(finishingData);
        return taskCompletionSource.Task;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        CreateQueuedChunks();
    }

    // Call only in game thread
    protected virtual void CreateQueuedChunks()
    {
        ThreadSafe.AssertIsMainThread();

        int count = ProcessingCount;
        while(count < MaxCountToProccess && ChunksToFinish.TryDequeue(out var finishingData))
        {
            _ = FinishChunk(finishingData);
            count++;
        }

        while(count < MaxCountToProccess && ChunksToCreate.TryDequeue(out var creationData))
        {
            _ = CreateChunk(creationData);
            count++;
        }
    }

    // Call only in game thread
    protected virtual async Task<Chunk> CreateChunk(ChunkCreatingData creatingData)
    {
        ThreadSafe.AssertIsMainThread();

        var position = creatingData.Position;
        lock(ProcessingChunks)
        {
            ProcessingChunks.Add(position);
        }
        var cancellationToken = creatingData.CancellationToken;

        try
        {
            var chunk = await base.CreateChunk(position, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            lock(ProcessingChunks)
            {
                ProcessingChunks.Remove(position);
            }
            creatingData.TaskCompletionSource.TrySetResult(chunk);
            return chunk;
        }
        catch(OperationCanceledException)
        {
            lock(ProcessingChunks)
            {
                ProcessingChunks.Remove(position);
            }
            creatingData.TaskCompletionSource.TrySetCanceled();
            throw;
        }
    }

    // Call only in game thread
    protected virtual async Task<Chunk> FinishChunk(ChunkFinishingData finishingData)
    {
        ThreadSafe.AssertIsMainThread();

        var position = finishingData.Chunk.Position;
        lock(ProcessingChunks)
        {
            ProcessingChunks.Add(position);
        }

        var cancellationToken = finishingData.CancellationToken;
        try
        {
            var chunk = await base.FinishChunk(finishingData.Chunk, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            lock(ProcessingChunks)
            {
                ProcessingChunks.Remove(position);
            }
            finishingData.TaskCompletionSource.TrySetResult(chunk);
            return chunk;
        }
        catch(OperationCanceledException)
        {
            lock(ProcessingChunks)
            {
                ProcessingChunks.Remove(position);
            }
            finishingData.TaskCompletionSource.TrySetCanceled();
            throw;
        }
    }
}
