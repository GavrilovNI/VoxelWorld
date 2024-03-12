using Sandbox;
using Sandcube.Mth;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Creation;

public class ChunksCreator : Component
{
    [Property] protected GameObject ChunkPrefab { get; set; } = null!;
    [Property] protected GameObject ChunksParent { get; set; } = null!;
    [Property] protected bool BreakFromPrefab { get; set; } = true;
    [Property] protected World World { get; set; } = null!;

    [Property] protected ChunkLoader? Loader { get; set; } = null!;
    [Property] protected ChunkLandscapeGenerator? LandscapeGenerator { get; set; } = null!;
    [Property] protected ChunkModelAwaiter? ModelAwaiter { get; set; } = null!;
    [Property] protected ChunkEntitiesLoader? EntitiesLoader { get; set; } = null!;

    protected CancellationTokenSource CommonTokenSource { get; set; } = new();

    protected record class ChunkTaskData(Task<Chunk> Task, bool PreloadOnly);
    protected readonly Dictionary<Vector3Int, ChunkTaskData> CreatingChunks = new();

    protected Vector3Int ChunkSize => World.ChunkSize;


    public async Task<(Chunk chunk, bool fullyLoaded)> GetOrCreateChunk(Vector3Int position, bool preloadOnly, CancellationToken cancellationToken)
    {
        var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(CommonTokenSource.Token, cancellationToken);
        cancellationToken = cancellationTokenSource.Token;

        cancellationToken.ThrowIfCancellationRequested();

        Task<Chunk> resultTask;
        lock(CreatingChunks)
        {
            if(CreatingChunks.TryGetValue(position, out var chunkTaskData))
            {
                if(chunkTaskData.PreloadOnly && !preloadOnly)
                    resultTask = FinishChunk(chunkTaskData.Task, cancellationToken);
                else
                    resultTask = chunkTaskData.Task;
            }
            else
            {
                CreatingChunks[position] = new ChunkTaskData(null!, preloadOnly);
                var task = CreateChunk(position, cancellationToken);
                CreatingChunks[position] = CreatingChunks[position] with { Task = task };
                resultTask = task;
            }
        }

        var chunk = await resultTask;
        bool fullyLoaded = false;
        lock(CreatingChunks)
        {
            fullyLoaded = !CreatingChunks.TryGetValue(position, out var chunkTasData) || !chunkTasData.PreloadOnly;

            if(fullyLoaded)
                CreatingChunks.Remove(chunk.Position);
        }

        cancellationTokenSource.Dispose();
        return (chunk, fullyLoaded);
    }

    protected virtual async Task<Chunk> CreateChunk(Vector3Int position, CancellationToken cancellationToken)
    {
        var chunk = await PreloadChunk(position, cancellationToken);

        bool preloadOnly = false;
        lock(CreatingChunks)
        {
            preloadOnly = CreatingChunks[position].PreloadOnly;
        }

        if(!preloadOnly)
            chunk = await FinishChunk(chunk, cancellationToken);
        return chunk;
    }

    protected virtual async Task<Chunk> PreloadChunk(Vector3Int position, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var chunk = CreateChunkObject(position);

        bool loaded = Loader.IsValid() && await Loader.TryProcess(chunk, cancellationToken);

        if(!loaded && LandscapeGenerator.IsValid())
            await LandscapeGenerator.TryProcess(chunk, cancellationToken);

        return chunk;
    }

    protected async Task<Chunk> FinishChunk(Task<Chunk> chunkTask, CancellationToken cancellationToken)
    {
        var chunk = await chunkTask;
        return await FinishChunk(chunk, cancellationToken);
    }
    protected virtual async Task<Chunk> FinishChunk(Chunk chunk, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock(CreatingChunks)
        {
            CreatingChunks[chunk.Position] = CreatingChunks[chunk.Position] with { PreloadOnly = false };
        }

        if(ModelAwaiter.IsValid() && EntitiesLoader.IsValid())
        {
            await ModelAwaiter.TryProcess(chunk, cancellationToken);
            await EntitiesLoader.TryProcess(chunk, cancellationToken);
        }

        await Task.MainThread();
        chunk.GameObject.Enabled = true;
        return chunk;
    }


    // Call only in game thread
    protected virtual Chunk CreateChunkObject(Vector3Int position)
    {
        ThreadSafe.AssertIsMainThread();

        Transform cloneTransform = new(World.Transform.Position + position * ChunkSize * MathV.UnitsInMeter, Transform.Rotation);
        var chunkGameObject = ChunkPrefab.Clone(cloneTransform, ChunksParent, false, $"Chunk {position}");
        if(BreakFromPrefab || !Game.IsEditor)
            chunkGameObject.BreakFromPrefab();

        var chunk = chunkGameObject.Components.Get<Chunk>(true);
        chunk.Initialize(position, ChunkSize, World);

        var proxies = chunkGameObject.Components.GetAll<WorldProxy>(FindMode.DisabledInSelfAndDescendants);
        foreach(var proxy in proxies)
            proxy.WorldComponent = World;

        return chunk;
    }

    protected override void OnAwake()
    {
        ChunksParent ??= GameObject;
    }

    protected override void OnDestroy()
    {
        CommonTokenSource.Cancel();
        CommonTokenSource.Dispose();
    }
}
