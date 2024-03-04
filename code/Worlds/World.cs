using Sandbox;
using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Data;
using Sandcube.Entities;
using Sandcube.Exceptions;
using Sandcube.Interfaces;
using Sandcube.IO;
using Sandcube.IO.Helpers;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Players;
using Sandcube.Registries;
using Sandcube.Threading;
using Sandcube.Worlds.Loading;
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
    
    [Property] protected ChunksCreator ChunksCreator { get; set; } = null!;
    [Property] protected EntitiesCreator? EntitiesCreator { get; set; }
    [Property] public BBoxInt Limits { get; private set; } = new BBoxInt(new Vector3Int(-50000, -50000, -256), new Vector3Int(50000, 50000, 255));
    [Property] protected bool TickByItself { get; set; } = true;
    [Property] protected bool IsService { get; set; } = false;

    [Property] protected GameObject EntitiesParent { get; set; } = null!;

    public bool Initialized { get; private set; }
    public ModedId Id { get; private set; }
    public BaseFileSystem? WorldFileSystem { get; private set; }
    public Vector3Int ChunkSize => WorldOptions.ChunkSize;


    private bool IsSceneRunning => !Scene.IsEditor;


    protected ChunksCollection Chunks { get; private set; } = null!;

    protected EntitiesCollection Entities { get; private set; } = new();


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
    }

    protected override void OnAwake()
    {
        Chunks = new(DestroyChunk, Vector3Int.XYZIterationComparer);
        Chunks.ChunkLoaded += OnChunkLoaded;
        Chunks.ChunkUnloaded += OnChunkUnloaded;

        EntitiesParent ??= GameObject;
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
        Chunks.ChunkLoaded -= OnChunkLoaded;
        Chunks.ChunkUnloaded -= OnChunkUnloaded;
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
        foreach(var (_, chunk) in Chunks)
            chunk.Tick();
    }

    public void AddEntity(Entity entity)
    {
        ArgumentNotValidException.ThrowIfNotValid(entity);

        if(!object.ReferenceEquals(this, entity.World))
            throw new InvalidOperationException($"{nameof(Entity)}({entity})'s world was not set to {nameof(World)} {this}");

        Entities.Add(entity);
        entity.GameObject.Parent = EntitiesParent;
    }

    public bool RemoveEntity(Guid id) => Entities.Remove(id);

    public bool RemoveEntity(Entity entity) => Entities.Remove(entity);

    // Thread safe
    public virtual bool IsChunkInLimits(Vector3Int chunkPosition)
    {
        var chunkFirstBlockPosition = GetBlockWorldPosition(chunkPosition, 0);
        return Limits.Overlaps(BBoxInt.FromMinsAndSize(chunkFirstBlockPosition, ChunkSize));
    }

    // Thread safe
    public virtual bool HasChunk(Vector3Int chunkPosition)
    {
        if(!IsChunkInLimits(chunkPosition))
            return false;

        return Chunks.HasLoaded(chunkPosition);
    }

    // Thread safe
    protected virtual async Task<Chunk?> GetChunkOrLoad(Vector3Int chunkPosition, bool awaitModelUpdate = false)
    {
        if(!IsChunkInLimits(chunkPosition))
            throw new InvalidOperationException($"Chunk position {chunkPosition} is not in Limits {Limits}");

        var chunk = await Chunks.GetChunkOrLoad(chunkPosition, token => CreateChunk(chunkPosition, token));

        if(awaitModelUpdate && chunk.IsValid())
            await chunk.GetModelUpdateTask();

        return chunk;
    }

    // Thread safe
    protected virtual Chunk? GetChunk(Vector3Int chunkPosition) => Chunks.Get(chunkPosition);

    // Thread safe
    protected virtual async Task<Chunk> CreateChunk(Vector3Int chunkPosition, CancellationToken cancellationToken)
    {
        ChunkCreationData creationData = new()
        {
            Position = chunkPosition,
            Size = ChunkSize,
            EnableOnCreate = false,
            World = this,
        };

        var chunk = await Task.RunInMainThreadAsync(() => ChunksCreator.LoadOrCreateChunk(creationData, cancellationToken));

        var positionsToClear = chunk.Size.GetPositionsFromZero().Where(p => !Limits.Contains(p));
        await chunk.SetBlockStates(positionsToClear.ToDictionary(p => p, p => BlockState.Air));

        return chunk;
    }

    // Thread safe
    public virtual async Task LoadChunk(Vector3Int chunkPosition, bool awaitModelUpdate = false)
    {
        Chunk? chunk;
        do
        {
            if(!IsValid)
                throw new OperationCanceledException();
            chunk = await GetChunkOrLoad(chunkPosition, awaitModelUpdate);
        }
        while(!chunk.IsValid());
    }

    // Thread safe
    public virtual async Task LoadChunksSimultaneously(IReadOnlySet<Vector3Int> chunkPositions, bool awaitModelUpdate = false)
    {
        List<Task> tasks = new();

        foreach (var chunkPosition in chunkPositions)
            tasks.Add(LoadChunk(chunkPosition, awaitModelUpdate));

        await Task.WhenAll(tasks);
    }

    // Thread safe
    protected virtual void OnChunkLoaded(Chunk chunk)
    {
        _ = Task.RunInMainThreadAsync(() =>
        {
            if(!chunk.IsValid)
                return;

            chunk.GameObject.Enabled = true;
            chunk.Destroyed += OnChunkDestroyed;

            EntitiesCreator?.LoadOrCreateEntitiesForChunk(chunk.Position);

            ChunkLoaded?.Invoke(chunk.Position);
            UpdateNeighboringChunks(chunk.Position);
        });
    }

    // Thread safe
    protected virtual void OnChunkUnloaded(Chunk chunk)
    {
        _ = Task.RunInMainThreadAsync(() => ChunkUnloaded?.Invoke(chunk.Position));
    }

    protected virtual void OnChunkDestroyed(Chunk chunk)
    {
        chunk.Destroyed -= OnChunkDestroyed;
        Chunks.RemoveLoaded(chunk);
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
        var chunk = await GetChunkOrLoad(chunkPosition, false);

        if(!chunk.IsValid())
            throw new InvalidOperationException($"Couldn't load {nameof(Chunk)} at position {chunkPosition}");

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
    public virtual void Clear() => Chunks.Clear();

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

    public WorldData Save(IReadOnlySaveMarker saveMarker)
    {
        ThreadSafe.AssertIsMainThread();

        WorldData result = new();

        var chunksToSave = Chunks.Where(pair => !pair.Value.IsSaved)
                .Select(pair => pair.Value);

        result.Chunks = chunksToSave.ToDictionary(c => c.Position, c => c.Save(saveMarker));

        foreach(var entity in Entities.Where(e => e is not Player))
        {
            var entityPosition = entity.Transform.Position;
            var chunkPosition = GetChunkPosition(entityPosition);
            result.Entities.GetOrCreate(chunkPosition).AddData(entity);
        }

        foreach(var player in Entities.Where(e => e is Player).Cast<Player>())
            result.Players[player.SteamId] = new PlayerData(player);

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
