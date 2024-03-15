using Sandbox;
using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Data;
using Sandcube.Entities;
using Sandcube.Exceptions;
using Sandcube.Interfaces;
using Sandcube.IO;
using Sandcube.IO.Helpers;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Threading;
using Sandcube.Worlds.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Worlds;

public class World : Component, IWorldAccessor, ITickable
{
    public event Action<Vector3Int>? ChunkLoaded;
    public event Action<Vector3Int>? ChunkUnloaded;


    [Property, HideIf(nameof(IsSceneRunning), true)]
    public WorldOptions WorldOptions { get; private set; } = new WorldOptions() { ChunkSize = 16, RegionSize = 4 };
    
    [Property] protected GameObject? ChunksParent { get; set; }
    [Property] protected ChunksCreator ChunksCreator { get; set; } = null!;
    [Property] public BBoxInt LimitsInChunks { get; private set; } = new BBoxInt(new Vector3Int(-10000, -10000, -16), new Vector3Int(10000, 10000, 16));
    [Property] public BBoxInt Limits => LimitsInChunks * WorldOptions.ChunkSize;
    [Property] protected bool TickByItself { get; set; } = true;
    [Property] protected bool IsService { get; set; } = false;

    public bool Initialized { get; private set; }
    public new ModedId Id { get; private set; }
    public BaseFileSystem? WorldFileSystem { get; private set; }
    public Vector3Int ChunkSize => WorldOptions.ChunkSize;


    private bool IsSceneRunning => !Scene.IsEditor;
    
    
    private readonly Dictionary<ulong, Player> _players = new();

    protected readonly object ChunksCreationLocker = new();
    protected CancellationTokenSource TaskCreationTokenSource = new();
    protected readonly ChunksCollection Chunks = new(Vector3Int.XYZIterationComparer);
    protected readonly Dictionary<Vector3Int, (Task<Chunk> ChunkTask, ChunkCreationStatus Status, Chunk? PartiallyLoadedChunk)> CreatingChunks = new();


    public void Initialize(in ModedId id, BaseFileSystem fileSystem, in WorldOptions defaultWorldOptions)
    {
        if(Initialized)
            throw new InvalidOperationException($"{nameof(World)} {this} was already initialized");
        if(IsService)
            throw new InvalidOperationException($"{nameof(World)} {this} is service, it can't be initialized");
        Initialized = true;

        Id = id;
        WorldFileSystem = fileSystem;

        WorldSaveHelper saveHelper = new(WorldFileSystem);
        if(saveHelper.TryReadWorldOptions(out var worldOptions))
        {
            WorldOptions = worldOptions;
        }
        else
        {
            WorldOptions = defaultWorldOptions;
            saveHelper.SaveWorldOptions(WorldOptions);
        }

        foreach(var listener in Components.GetAll<IWorldInitializationListener>(FindMode.EverythingInSelfAndDescendants))
            listener.OnWorldInitialized(this);
    }

    protected override void OnAwake()
    {
        Chunks.ChunkLoaded += OnChunkLoaded;
        Chunks.ChunkUnloaded += OnChunkUnloaded;
        ChunksParent ??= GameObject;
    }

    protected override void OnStart()
    {
        if(!Initialized && !IsService)
        {
            Log.Warning($"{nameof(World)} {this} was not initialized before OnStart, destroying...");
            GameObject.Destroy();
            return;
        }
    }

    protected override void OnDestroy()
    {
        Clear();
        lock(ChunksCreationLocker)
        {
            TaskCreationTokenSource.Cancel();
            TaskCreationTokenSource.Dispose();
        }

        Chunks.ChunkLoaded -= OnChunkLoaded;
        Chunks.ChunkUnloaded -= OnChunkUnloaded;
        Chunks.Dispose();
    }

    protected override void OnUpdate()
    {
        if(TickByItself)
            TickInternal();
    }

    // Thread safe
    public virtual void Tick()
    {
        if(!TickByItself)
            TickInternal();
    }

    protected virtual void TickInternal()
    {
        foreach(var chunk in Chunks)
            chunk.Tick();
    }

    public async Task AddEntity(Entity entity)
    {
        ArgumentNotValidException.ThrowIfNotValid(entity);

        if(!object.ReferenceEquals(this, entity.World))
            throw new InvalidOperationException($"{nameof(Entity)}({entity})'s world was not set to {nameof(World)} {this}");

        bool wasEnabled = entity.Enabled;
        entity.Enabled = false;
        var chunkCreationStatus = entity is Player ? ChunkCreationStatus.Finishing : ChunkCreationStatus.Preloading;
        var chunk = await GetOrCreateChunk(entity.ChunkPosition, chunkCreationStatus);
        entity.Enabled = wasEnabled;
        chunk.AddEntity(entity);
    }

    public bool RemoveEntity(Entity entity) =>
        Chunks.TryGet(entity.ChunkPosition, out var chunk) && chunk.RemoveEntity(entity);

    protected virtual void OnEntityAddedToChunk(Chunk chunk, Entity entity)
    {
        if(entity is Player player)
        {
            lock(_players)
            {
                _players.Add(player.SteamId, player);
            }
        }
    }

    protected virtual void OnEntityRemovedFromChunk(Chunk chunk, Entity entity)
    {
        if(entity is Player player)
        {
            lock(_players)
            {
                _players.Remove(player.SteamId);
            }
        }
    }

    // Thread safe
    public virtual bool IsChunkInLimits(Vector3Int chunkPosition) => LimitsInChunks.Contains(chunkPosition);

    // Thread safe
    public virtual bool HasChunk(Vector3Int chunkPosition)
    {
        if(!IsChunkInLimits(chunkPosition))
            return false;

        return Chunks.Contains(chunkPosition);
    }

    // Thread safe
    protected virtual Task<Chunk> GetOrCreateChunk(Vector3Int chunkPosition, ChunkCreationStatus creationStatus = ChunkCreationStatus.Finishing)
    {
        if(!IsChunkInLimits(chunkPosition))
            throw new InvalidOperationException($"Chunk position {chunkPosition} is not in Limits {LimitsInChunks}");

        if(creationStatus != ChunkCreationStatus.Preloading && creationStatus != ChunkCreationStatus.Finishing)
            throw new NotSupportedException($"{creationStatus} is not supported");

        lock(ChunksCreationLocker)
        {
            var cancellationToken = TaskCreationTokenSource.Token;

            if(Chunks.TryGet(chunkPosition, out var chunk))
                return Task.FromResult(chunk);

            ChunkCreationStatus currentStatus = ChunkCreationStatus.None;
            Task<Chunk>? chunkTask = null;
            if(CreatingChunks.TryGetValue(chunkPosition, out var creatingChunkData))
            {
                if(creatingChunkData.Status >= creationStatus)
                    return creatingChunkData.ChunkTask;
                currentStatus = creatingChunkData.Status;
                chunkTask = creatingChunkData.ChunkTask;
            }

            TaskCompletionSource chunkTaskCreationTaskSource = new();

            if(currentStatus < ChunkCreationStatus.Preloading)
            {
                chunkTask = ChunksCreator.PreloadChunk(chunkPosition, cancellationToken);
                chunkTask = chunkTask!.ContinueWith(async t =>
                {
                    await chunkTaskCreationTaskSource.Task;
                    lock(ChunksCreationLocker)
                    {
                        if(!t.IsCompletedSuccessfully)
                        {
                            CreatingChunks.Remove(chunkPosition);
                            t.ThrowIfNotCompletedSuccessfully();
                        }

                        var partiallyLoadedChunk = t.Result;
                        var creatingData = CreatingChunks[partiallyLoadedChunk.Position];
                        CreatingChunks[partiallyLoadedChunk.Position] = creatingData with { PartiallyLoadedChunk = partiallyLoadedChunk };
                    }
                    return t.Result;
                }).Unwrap();
            }

            if(creationStatus == ChunkCreationStatus.Finishing)
            {
                chunkTask = ChunksCreator.FinishChunk(chunkTask!, cancellationToken);

                chunkTask = chunkTask!.ContinueWith(async t =>
                {
                    await chunkTaskCreationTaskSource.Task;
                    lock(ChunksCreationLocker)
                    {
                        CreatingChunks.Remove(chunkPosition);
                        t.ThrowIfNotCompletedSuccessfully();
                        Chunks.Add(t.Result);
                    }
                    return t.Result;
                }).Unwrap();
            }

            CreatingChunks[chunkPosition] = new(chunkTask!, creationStatus, null);
            chunkTaskCreationTaskSource.SetResult();

            return chunkTask!;
        }
    }

    protected virtual Task<Chunk?> GetChunkOrAwaitFullyLoad(Vector3Int chunkPosition)
    {
        lock(ChunksCreationLocker)
        {
            if(Chunks.TryGet(chunkPosition, out var chunk))
                return Task.FromResult<Chunk?>(chunk);
            if(!CreatingChunks.ContainsKey(chunkPosition))
                return Task.FromResult<Chunk?>(null);
        }
        return GetOrCreateChunk(chunkPosition, ChunkCreationStatus.Finishing)!;
    }

    // Thread safe
    protected virtual void OnChunkLoaded(Chunk chunk)
    {
        chunk.GameObject.Parent = ChunksParent;
        foreach(var entity in chunk.Entities)
            OnEntityAddedToChunk(chunk, entity);
        chunk.EntityAdded += OnEntityAddedToChunk;
        chunk.EntityRemoved += OnEntityRemovedFromChunk;
        UpdateNeighboringChunks(chunk.Position);
        _ = Task.RunInMainThreadAsync(() => ChunkLoaded?.Invoke(chunk.Position));
    }

    protected virtual void OnChunkUnloaded(Chunk chunk)
    {
        chunk.EntityAdded -= OnEntityAddedToChunk;
        chunk.EntityRemoved -= OnEntityRemovedFromChunk;
        chunk.GameObject.Destroy();
        UpdateNeighboringChunks(chunk.Position);
        _ = Task.RunInMainThreadAsync(() => ChunkUnloaded?.Invoke(chunk.Position));
    }

    // Thread safe
    protected virtual Chunk? GetChunk(Vector3Int chunkPosition) => Chunks.GetOrDefault(chunkPosition);

    // Thread safe
    public virtual async Task CreateChunk(Vector3Int chunkPosition, ChunkCreationStatus creationStatus = ChunkCreationStatus.Finishing)
    {
        if(creationStatus == ChunkCreationStatus.None)
            return;

        await GetOrCreateChunk(chunkPosition, creationStatus);
    }

    // Thread safe
    public virtual async Task CreateChunksSimultaneously(IEnumerable<Vector3Int> chunkPositions, ChunkCreationStatus creationStatus = ChunkCreationStatus.Finishing)
    {
        if(creationStatus == ChunkCreationStatus.None)
            return;
        if(creationStatus != ChunkCreationStatus.Preloading && creationStatus != ChunkCreationStatus.Finishing)
            throw new NotSupportedException($"{creationStatus} is not supported");

        List<Task> tasks = new();
        foreach(var position in chunkPositions)
            tasks.Add(GetOrCreateChunk(position, ChunkCreationStatus.Preloading));

        await Task.WhenAll(tasks);

        if(creationStatus == ChunkCreationStatus.Preloading)
            return;

        tasks.Clear();
        foreach(var position in chunkPositions)
            tasks.Add(GetOrCreateChunk(position, ChunkCreationStatus.Finishing));

        await Task.WhenAll(tasks);
    }

    public virtual Vector3Int GetBlockPosition(Vector3 blockPosition, Vector3 hitNormal)
    {
        hitNormal = Transform.World.NormalToLocal(hitNormal.Normal);
        var result = Transform.World.PointToLocal(blockPosition).Divide(MathV.UnitsInMeter);

        foreach(var axis in Axis.All)
        {
            var mod = result.GetAxis(axis) % 1;
            var hitAxis = hitNormal.GetAxis(axis);

            if(hitAxis > 0)
            {
                if((mod.AlmostEqual(0) && mod >= 0) || mod.AlmostEqual(-1))
                    result = result.WithAxis(axis, result.GetAxis(axis) - 1);
            }
            else if(mod.AlmostEqual(1))
            {
                result = result.WithAxis(axis, result.GetAxis(axis) + 1);
            }
        }

        return result.Floor();
    }
    public virtual Vector3Int GetBlockPosition(Vector3 position) => Transform.World.PointToLocal(position).Divide(MathV.UnitsInMeter).Floor();

    public virtual Vector3Int GetChunkPosition(Vector3 position) => GetChunkPosition(GetBlockPosition(position));
    public virtual Vector3 GetBlockGlobalPosition(Vector3Int blockPosition) => blockPosition * MathV.UnitsInMeter;

    public virtual Vector3Int GetChunkPosition(Vector3Int blockPosition) => blockPosition.WithAxes((a, v) => (int)MathF.Floor(((float)v) / ChunkSize.GetAxis(a)));
    public virtual Vector3Int GetBlockPositionInChunk(Vector3Int blockPosition) => (blockPosition % ChunkSize + ChunkSize) % ChunkSize;
    public virtual Vector3Int GetBlockWorldPosition(Vector3Int chunkPosition, Vector3Int blockLocalPosition) => chunkPosition * ChunkSize + blockLocalPosition;


    // Thread safe
    public virtual async Task<BlockStateChangingResult> SetBlockState(Vector3Int blockPosition, BlockState blockState, BlockSetFlags flags = BlockSetFlags.Default)
    {
        if(!Limits.Contains(blockPosition))
            return BlockStateChangingResult.NotChanged;

        var chunkPosition = GetChunkPosition(blockPosition);
        bool preloadOnly = !flags.HasFlag(BlockSetFlags.UpdateModel) && !flags.HasFlag(BlockSetFlags.AwaitModelUpdate);
        var chunkCreationStatus = preloadOnly ? ChunkCreationStatus.Preloading : ChunkCreationStatus.Finishing;
        var chunk = await GetOrCreateChunk(chunkPosition, chunkCreationStatus);

        var localPosition = GetBlockPositionInChunk(blockPosition);

        var result = await chunk.SetBlockState(localPosition, blockState, flags & ~BlockSetFlags.AwaitModelUpdate & ~BlockSetFlags.UpdateNeigbours);
        if(result.Changed)
        {
            NotifyNeighboringChunksAboutEdgeUpdate(blockPosition, result.OldBlockState, blockState);
            if(flags.HasFlag(BlockSetFlags.UpdateNeigbours))
                NotifyNeighboursAboutBlockUpdate(blockPosition, result.OldBlockState, blockState);
        }

        if(flags.HasFlag(BlockSetFlags.AwaitModelUpdate))
            await chunk.GetModelUpdateTask();

        return result;
    }

    protected virtual void NotifyNeighboursAboutBlockUpdate(Vector3Int blockPosition, BlockState oldBlockState, BlockState newBlockState)
    {
        foreach(Direction direction in Direction.All)
        {
            var neighborBlockPosition = blockPosition + direction;
            NeighbourChangedContext context = new(this, neighborBlockPosition, direction.GetOpposite(), oldBlockState, newBlockState);
            context.ThisBlockState.Block.OnNeighbourChanged(context);
        }
    }

    protected virtual List<Direction> GetNeighboringChunkDirections(Vector3Int localBlockPosition)
    {
        List<Direction> result = new();

        if(localBlockPosition.x == 0)
            result.Add(Direction.Backward);
        if(localBlockPosition.x == ChunkSize.x - 1)
            result.Add(Direction.Forward);
        if(localBlockPosition.y == 0)
            result.Add(Direction.Right);
        if(localBlockPosition.y == ChunkSize.y - 1)
            result.Add(Direction.Left);
        if(localBlockPosition.z == 0)
            result.Add(Direction.Down);
        if(localBlockPosition.z == ChunkSize.z - 1)
            result.Add(Direction.Up);

        return result;
    }

    protected virtual bool NotifyNeighboringChunksAboutEdgeUpdate(Vector3Int updatedBlockPosition, BlockState oldBlockState, BlockState newBlockState)
    {
        var localBlockPosition = GetBlockPositionInChunk(updatedBlockPosition);
        List<Direction> neighboringChunkDirections = GetNeighboringChunkDirections(localBlockPosition);
        if(neighboringChunkDirections.Count == 0)
            return false;

        foreach(Direction direction in neighboringChunkDirections)
        {
            var neighborBlockPosition = updatedBlockPosition + direction;
            var chunkPosition = GetChunkPosition(neighborBlockPosition);
            var chunk = GetChunk(chunkPosition);
            chunk?.OnNeighbouringChunkEdgeUpdated(direction.GetOpposite(), updatedBlockPosition, oldBlockState, newBlockState);
        }
        return true;
    }

    protected virtual void UpdateNeighboringChunks(Vector3Int chunkPosition)
    {
        foreach(Direction direction in Direction.All)
        {
            var neighboringChunkPosition = chunkPosition + direction;
            var chunk = GetChunk(neighboringChunkPosition);
            chunk?.RequireModelUpdate();
        }    
    }

    // Thread safe
    public virtual BlockState GetBlockState(Vector3Int blockPosition)
    {
        var chunkPosition = GetChunkPosition(blockPosition);
        var chunk = GetChunk(chunkPosition);
        if(chunk is null)
            return BlockState.Air;
        blockPosition = GetBlockPositionInChunk(blockPosition);
        return chunk.GetBlockState(blockPosition);
    }

    public BlockEntity? GetBlockEntity(Vector3Int blockPosition)
    {
        var chunkPosition = GetChunkPosition(blockPosition);
        var chunk = GetChunk(chunkPosition);
        if(chunk is null)
            return null;

        blockPosition = GetBlockPositionInChunk(blockPosition);
        return chunk.GetBlockEntity(blockPosition);
    }

    // Thread safe
    public virtual void Clear()
    {
        lock(ChunksCreationLocker)
        {
            Chunks.Clear();
            CreatingChunks.Clear();

            TaskCreationTokenSource.Cancel();
            TaskCreationTokenSource.Dispose();
            TaskCreationTokenSource = new();
        }
    }

    protected virtual void DestroyChunk(Chunk? chunk)
    {
        if(!chunk.IsValid())
            return;

        _ = Task.RunInMainThreadAsync(() =>
        {
            if(chunk.IsValid())
                chunk.GameObject.Destroy();
        });
    }

    public virtual async Task<(BinaryTag Blocks, ListTag Entities)> SaveChunk(Vector3Int chunkPosition, IReadOnlySaveMarker saveMarker)
    {
        var chunk = await GetChunkOrAwaitFullyLoad(chunkPosition);
        if(chunk is null)
            throw new ArgumentOutOfRangeException(nameof(chunkPosition), chunkPosition, "Chunk wasn't created/creating");

        return chunk.Save(saveMarker);
    }

    public virtual async Task<Dictionary<Vector3Int, (BinaryTag Blocks, ListTag Entities)>> SaveUnsavedChunks(IReadOnlySaveMarker saveMarker)
    {
        await FullyLoadUnsavedChunks();

        Dictionary<Vector3Int, (BinaryTag blocks, ListTag entities)> result = new();
        foreach(var chunk in Chunks.Where(c => !c.IsSaved))
            result[chunk.Position] = chunk.Save(saveMarker);

        return result;
    }

    protected virtual async Task FullyLoadUnsavedChunks()
    {
        List<Vector3Int> chunksToLoad = new();
        lock(ChunksCreationLocker)
        {
            foreach(var (position, creatingData) in CreatingChunks)
            {
                var chunk = creatingData.PartiallyLoadedChunk;
                if(chunk is not null && !chunk.IsSaved)
                    chunksToLoad.Add(position);
            }
        }

        Log.Info($"FullyLoadUnsavedChunks {chunksToLoad.Count}");
        var tasks = chunksToLoad.Select(GetChunkOrAwaitFullyLoad);
        await Task.WhenAll(tasks);
    }

    public virtual Dictionary<ulong, BinaryTag> SaveAllPlayers()
    {
        Dictionary<ulong, BinaryTag> result = new();

        lock(_players)
        {
            foreach(var (_, player) in _players)
                result.Add(player.SteamId, player.Write());
        }

        return result;
    }

    public static bool TryFind(GameObject? gameObject, out IWorldAccessor world)
    {
        if(!gameObject.IsValid())
        {
            world = default!;
            return false;
        }

        world = gameObject.Components.Get<IWorldAccessor>();
        world ??= gameObject.Components.Get<IWorldProxy>()?.World!;
        return world is not null;
    }
}
