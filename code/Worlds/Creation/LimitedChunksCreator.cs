using Sandbox;
using Sandcube.Mth;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Creation;

public class LimitedChunksCreator : ChunksCreator
{
    [Property] public int MaxCountToProccess { get; set; } = 15;
    [Property] protected int ProcessingCountShower => ProcessingCount;

    private int _processingCount = 0;
    protected int ProcessingCount
    {
        get => Interlocked.CompareExchange(ref _processingCount, 0, 0);
        set => Interlocked.Exchange(ref _processingCount, value);
    }

    protected record struct ChunkPreloadingData(Vector3Int Position,
        TaskCompletionSource<ChunkCreationData> TaskCompletionSource, CancellationToken CancellationToken);

    protected record struct ChunkFinishingData(ChunkCreationData CreationData, bool EnableChunk,
        TaskCompletionSource<ChunkCreationData> TaskCompletionSource, CancellationToken CancellationToken);

    protected readonly ConcurrentQueue<ChunkPreloadingData> ChunksToPreload = new();
    protected readonly ConcurrentQueue<ChunkFinishingData> ChunksToFinish = new();

    public override Task<ChunkCreationData> PreloadChunk(Vector3Int position, CancellationToken cancellationToken)
    {
        TaskCompletionSource<ChunkCreationData> taskCompletionSource = new();
        ChunkPreloadingData creatingData = new(position, taskCompletionSource, cancellationToken);
        ChunksToPreload.Enqueue(creatingData);
        return taskCompletionSource.Task;
    }

    public override Task<ChunkCreationData> FinishChunk(ChunkCreationData creationData, CancellationToken cancellationToken, bool enableChunk = true)
    {
        TaskCompletionSource<ChunkCreationData> taskCompletionSource = new();
        ChunkFinishingData finishingData = new(creationData, enableChunk, taskCompletionSource, cancellationToken);
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
        while(count < MaxCountToProccess && ChunksToFinish.TryDequeue(out var data))
        {
            _ = FinishChunk(data);
            count++;
        }

        while(count < MaxCountToProccess && ChunksToPreload.TryDequeue(out var data))
        {
            _ = PreloadChunk(data);
            count++;
        }
    }

    protected virtual async Task<ChunkCreationData> PreloadChunk(ChunkPreloadingData creatingData)
    {
        var cancellationToken = creatingData.CancellationToken;

        ProcessingCount++;
        try
        {
            var chunk = await base.PreloadChunk(creatingData.Position, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            creatingData.TaskCompletionSource.TrySetResult(chunk);
            ProcessingCount--;
            return chunk;
        }
        catch(OperationCanceledException)
        {
            ProcessingCount--;
            creatingData.TaskCompletionSource.TrySetCanceled();
            throw;
        }
    }

    // Call only in game thread
    protected virtual async Task<ChunkCreationData> FinishChunk(ChunkFinishingData finishingData)
    {
        ThreadSafe.AssertIsMainThread();

        var cancellationToken = finishingData.CancellationToken;

        ProcessingCount++;
        try
        {
            var creationData = await base.FinishChunk(finishingData.CreationData, cancellationToken, finishingData.EnableChunk);
            cancellationToken.ThrowIfCancellationRequested();
            finishingData.TaskCompletionSource.TrySetResult(creationData);
            ProcessingCount--;
            return creationData;
        }
        catch(OperationCanceledException)
        {
            ProcessingCount--;
            finishingData.TaskCompletionSource.TrySetCanceled();
            throw;
        }
    }
}
