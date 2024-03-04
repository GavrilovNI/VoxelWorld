using Sandbox;
using Sandcube.Mth;
using Sandcube.Threading;
using System.Threading.Tasks;
using System.Threading;
using Sandcube.Worlds.Generation;
using Sandcube.IO.Helpers;
using System.IO;
using System;
using Sandcube.Data;

namespace Sandcube.Worlds.Loading;

public class ChunksCreator : Component
{
    [Property] protected World World { get; set; } = null!;
    [Property] protected GameObject ChunksParent { get; set; } = null!;
    [Property] public GameObject ChunkPrefab { get; protected set; } = null!;
    [Property] public WorldGenerator? Generator { get; protected set; }

    protected BaseFileSystem? WorldFileSystem => World.WorldFileSystem;
    protected WorldOptions WorldOptions => World.WorldOptions;

    [Property, Category("Debug")] public bool BreakFromPrefab { get; set; } = true;

    protected override void OnAwake()
    {
        ChunksParent ??= GameObject;
    }

    protected override void OnStart()
    {
        Generator?.SetSeed(WorldOptions.Seed);
    }

    // Call only in game thread
    public virtual async Task<Chunk> LoadOrCreateChunk(ChunkCreationData creationData, CancellationToken cancellationToken)
    {
        ThreadSafe.AssertIsMainThread();

        cancellationToken.ThrowIfCancellationRequested();
        var chunk = CreateChunkObject(creationData with { EnableOnCreate = false });
        
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.RunInThreadAsync(async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if(!TryLoadChunk(chunk)) // TODO: load all region and cache it?
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if(Generator.IsValid())
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await GenerateChunk(chunk, cancellationToken);
                    }
                }
                cancellationToken.ThrowIfCancellationRequested();
            });

            if(creationData.EnableOnCreate)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.RunInMainThreadAsync(() => {
                    cancellationToken.ThrowIfCancellationRequested();
                    chunk.GameObject.Enabled = true;
                });
            }
        }
        catch (OperationCanceledException)
        {
            chunk.GameObject.Destroy();
            throw;
        }
        return chunk;
    }

    // Call only in game thread
    protected virtual Chunk CreateChunkObject(ChunkCreationData creationData)
    {
        ThreadSafe.AssertIsMainThread();

        Transform cloneTransform = new(Transform.Position + creationData.Position * creationData.Size * MathV.UnitsInMeter, Transform.Rotation);
        var chunkGameObject = ChunkPrefab.Clone(cloneTransform, ChunksParent, false, $"Chunk {creationData.Position}");
        if(BreakFromPrefab || !Game.IsEditor)
            chunkGameObject.BreakFromPrefab();

        var chunk = chunkGameObject.Components.Get<Chunk>(true);
        chunk.Initialize(creationData.Position, creationData.Size, creationData.World);

        if(creationData.World.IsValid())
        {
            var proxies = chunkGameObject.Components.GetAll<WorldProxy>(FindMode.DisabledInSelfAndDescendants);
            foreach(var proxy in proxies)
                proxy.WorldComponent = creationData.World;
        }

        chunkGameObject.Tags.Add("world");
        chunkGameObject.Enabled = creationData.EnableOnCreate;
        return chunk;
    }

    protected virtual Task GenerateChunk(Chunk chunk, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.RunInThreadAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var states = Generator!.Generate(chunk.BlockOffset, chunk.Size);
            cancellationToken.ThrowIfCancellationRequested();
            await chunk.SetBlockStates(states, BlockSetFlags.UpdateModel);
            cancellationToken.ThrowIfCancellationRequested();
        });
    }

    public virtual bool TryLoadChunk(Chunk chunk)
    {
        if(WorldFileSystem is null)
            return false;

        if(WorldOptions.ChunkSize != chunk.Size)
            throw new InvalidOperationException($"Can't load chunk, saved chunk size {WorldOptions.ChunkSize} is not equal to chunk size {chunk.Size}");

        var regionPosition = (1f * chunk.Position / WorldOptions.RegionSize).Floor();
        var localChunkPosition = chunk.Position - regionPosition * WorldOptions.RegionSize;

        var worldSaveHelper = new WorldSaveHelper(WorldFileSystem);
        var regionSaveHelper = new RegionSaveHelper(WorldOptions);
        var blocksRegions = worldSaveHelper.GetRegions(WorldSaveHelper.BlocksRegionName);

        if(blocksRegions.HasRegionFile(regionPosition))
        {
            using(var regionReadStream = blocksRegions.OpenRegionRead(regionPosition))
            {
                using var reader = new BinaryReader(regionReadStream);
                if(regionSaveHelper.ReadOnlyOneChunk(reader, localChunkPosition))
                {
                    var blocksData = regionSaveHelper.GetChunkData(localChunkPosition)!;
                    if(blocksData is not null)
                    {
                        chunk.Load(blocksData);
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
