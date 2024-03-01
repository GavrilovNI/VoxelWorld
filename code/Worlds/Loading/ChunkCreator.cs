using Sandbox;
using Sandcube.Mth;
using Sandcube.Threading;
using System.Threading.Tasks;
using System.Threading;
using Sandcube.Worlds.Generation;
using Sandcube.IO;
using Sandcube.IO.Worlds;
using System.IO;
using System;

namespace Sandcube.Worlds.Loading;

public class ChunkCreator : Component, ISavePathInitializable
{
    [Property] protected GameObject ChunksParent { get; set; } = null!;
    [Property] public GameObject ChunkPrefab { get; set; } = null!;
    [Property] public WorldGenerator? Generator { get; set; }
    [Property] protected string SavePath { get; set; } = string.Empty;

    [Property, Category("Debug")] public bool BreakFromPrefab { get; set; } = true;

    public virtual void InitizlizeSavePath(string savePath) => SavePath = savePath;

    protected override void OnAwake()
    {
        ChunksParent ??= GameObject;
    }

    // Call only in game thread
    public virtual async Task<Chunk> CreateChunk(ChunkCreationData creationData, CancellationToken cancellationToken)
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
        if(string.IsNullOrWhiteSpace(SavePath))
            return false;

        return TryLoadChunk(FileSystem.Data.CreateDirectoryAndSubSystem(SavePath), chunk);
    }

    protected virtual bool TryLoadChunk(BaseFileSystem fileSystem, Chunk chunk)
    {
        var helper = new WorldSaveHelper(fileSystem);
        if(helper.TryReadWorldOptions(out var options))
        {
            if(options.ChunkSize != chunk.Size)
                throw new InvalidOperationException($"Can't load chunk, saved chunk size {options.ChunkSize} is not equal to chunk size {chunk.Size}");
        }
        else
        {
            return false;
        }

        var regionPosition = (1f * chunk.Position / options.RegionSize).Floor();
        var localChunkPosition = chunk.Position - regionPosition * options.RegionSize;

        var regionHelper = new RegionSaveHelper(options);
        if(helper.HasRegionFile(regionPosition))
        {
            using(var regionReadStream = helper.OpenRegionRead(regionPosition))
            {
                using var reader = new BinaryReader(regionReadStream);
                if(regionHelper.ReadOnlyOneChunk(reader, localChunkPosition))
                {
                    var blocksData = regionHelper.GetChunkData(localChunkPosition)!;
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
