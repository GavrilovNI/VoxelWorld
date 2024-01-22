using Sandbox;
using Sandcube.Mth;
using Sandcube.Threading;
using System.Threading.Tasks;
using System.Threading;
using Sandcube.Worlds.Generation;

namespace Sandcube.Worlds.Loading;

public class ChunkLoader : ThreadHelpComponent
{
    [Property] public GameObject ChunkPrefab { get; set; } = null!;
    [Property] public WorldGenerator? Generator { get; set; }


    // Call only in game thread
    public virtual Task<Chunk> LoadChunk(ChunkCreationData creationData, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var chunk = CreateChunk(creationData with { EnableOnCreate = false });

        return Task.RunInThreadAsync(async () =>
        {
            if(Generator.IsValid())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await GenerateChunk(chunk, cancellationToken);
            }

            if(creationData.EnableOnCreate)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await RunInGameThread(ct => chunk.GameObject.Enabled = true, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return chunk;
        }).ContinueWith(t =>
        {
            if(!t.IsCompletedSuccessfully)
                chunk.Destroy();

            cancellationToken.ThrowIfCancellationRequested();
            return t.Result;
        });
    }

    // Call only in game thread
    protected virtual Chunk CreateChunk(ChunkCreationData creationData)
    {
        Transform cloneTransform = new(Transform.Position + creationData.Position * creationData.Size * MathV.InchesInMeter, Transform.Rotation);
        var chunkGameObject = ChunkPrefab.Clone(cloneTransform, GameObject, false, $"Chunk {creationData.Position}");
        chunkGameObject.BreakFromPrefab();

        var chunk = chunkGameObject.Components.Get<Chunk>(true);
        chunk.Initialize(creationData.Position, creationData.Size, creationData.World);

        if(creationData.World.IsValid())
        {
            var proxies = chunkGameObject.Components.GetAll<WorldProxy>(FindMode.DisabledInSelfAndDescendants);
            foreach(var proxy in proxies)
                proxy.World = creationData.World;
        }

        chunkGameObject.Enabled = creationData.EnableOnCreate;
        return chunk;
    }

    protected virtual Task GenerateChunk(Chunk chunk, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.RunInThreadAsync(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var states = Generator!.Generate(chunk.BlockOffset, chunk.Size);
            cancellationToken.ThrowIfCancellationRequested();
            chunk.SetBlockStates(Vector3Int.Zero, states);
            cancellationToken.ThrowIfCancellationRequested();
        });
    }
}
