using Sandbox;
using Sandcube.Mth;
using Sandcube.Threading;
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

    protected Vector3Int ChunkSize => World.ChunkSize;


    public virtual async Task<Chunk> PreloadChunk(Vector3Int position, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var chunk = await Task.RunInMainThreadAsync(() => CreateChunkObject(position));

        bool loaded = Loader.IsValid() && await Loader.TryProcess(chunk, cancellationToken);

        if(!loaded && LandscapeGenerator.IsValid())
            await LandscapeGenerator.TryProcess(chunk, cancellationToken);

        return chunk;
    }

    public async Task<Chunk> FinishChunk(Task<Chunk> chunkTask, CancellationToken cancellationToken)
    {
        var chunk = await chunkTask;
        return await FinishChunk(chunk, cancellationToken);
    }
    public virtual async Task<Chunk> FinishChunk(Chunk chunk, CancellationToken cancellationToken, bool enableChunk = true)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if(ModelAwaiter.IsValid() && EntitiesLoader.IsValid())
        {
            await ModelAwaiter.TryProcess(chunk, cancellationToken);
            await EntitiesLoader.TryProcess(chunk, cancellationToken);
        }

        await Task.MainThread();
        chunk.GameObject.Enabled = enableChunk;
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
