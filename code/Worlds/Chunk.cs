using Sandbox;
using VoxelWorld.Blocks.Entities;
using VoxelWorld.Blocks.States;
using VoxelWorld.Data.Blocks;
using VoxelWorld.Entities;
using VoxelWorld.Interfaces;
using VoxelWorld.IO;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using VoxelWorld.SandboxExtensions;
using VoxelWorld.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoxelWorld.Worlds;

public class Chunk : Component, IBlockStateAccessor, IBlockEntityProvider, ITickable
{
    public event Action<Chunk>? Destroyed = null;
    public event Action<Chunk, Entity>? EntityAdded = null;
    public event Action<Chunk, Entity>? EntityRemoved = null;

    [Property] protected GameObject EntitiesParent { get; set; } = null!;
    [Property, HideIf(nameof(Initialized), true)] public Vector3Int Position { get; internal set; }
    [Property, HideIf(nameof(Initialized), true)] public Vector3Int Size { get; internal set; } = 16;
    [Property] public ChunkModelUpdater ModelUpdater { get; internal set; } = null!;

    public bool Initialized { get; private set; } = false;

    public IWorldAccessor World { get; internal set; } = null!;

    public BBox ModelBounds => ModelUpdater.ModelBounds;

    public Vector3Int BlockOffset => Position * Size;

    public bool IsSaved
    {
        get
        {
            lock(Blocks)
            {
                lock(_entities)
                {
                    return Blocks.IsSaved && _entitiesSaveMarker.IsSaved;
                }
            }
        }
    }

    private IReadOnlySaveMarker _entitiesSaveMarker = SaveMarker.Saved;

    protected SizedBlocksCollection Blocks = null!;
    private readonly Dictionary<Guid, Entity> _entities = new();

    public IEnumerable<Entity> Entities
    {
        get
        {
            lock(_entities)
            {
                foreach(var (_, entity) in _entities)
                    yield return entity;
            }
        }
    }

    public virtual void Initialize(Vector3Int position, Vector3Int size, IWorldAccessor world)
    {
        if(Initialized)
            throw new InvalidOperationException($"{nameof(Chunk)} {this} was already initialized or enabled");
        ArgumentNullException.ThrowIfNull(world);

        Initialized = true;

        Position = position;
        Size = size;
        World = world;
        Blocks = new(world, World.GetBlockWorldPosition(position, Vector3Int.Zero), size);
    }

    public bool AddEntity(Entity entity)
    {
        lock(_entities)
        {
            if(!entity.IsValid)
                return false;

            if(_entities.ContainsKey(entity.Id))
                return false;

            _entities[entity.Id] = entity;
            _entitiesSaveMarker = SaveMarker.NotSaved;
            entity.GameObject.SetParentCalmly(EntitiesParent);
            entity.Destroyed += OnEntityDestroyed;
            entity.MovedToAnotherChunk += OnEntityMovedToAnotherChunk;
            EntityAdded?.Invoke(this, entity);
            return true;
        }
    }

    public Entity? GetEntityOrDefault(Guid id, Entity? defaultValue)
    {
        lock(_entities)
        {
            if(_entities.TryGetValue(id, out var entity))
                return defaultValue;

            return defaultValue;
        }
    }

    public bool RemoveEntity(Guid id)
    {
        lock(_entities)
        {
            return _entities.TryGetValue(id, out var entity) &&
                RemoveEntityInternal(entity);
        }
    }

    public bool RemoveEntity(Entity entity)
    {
        lock(_entities)
        {
            return _entities.TryGetValue(entity.Id, out var realEntity) &&
                realEntity == entity &&
                RemoveEntityInternal(entity);
        }
    }

    private bool RemoveEntityInternal(Entity entity)
    {
        lock(_entities)
        {
            bool removed = _entities.Remove(entity.Id);
            if(removed)
            {
                entity.Destroyed -= OnEntityDestroyed;
                entity.MovedToAnotherChunk -= OnEntityMovedToAnotherChunk;
                _entitiesSaveMarker = SaveMarker.NotSaved;
                EntityRemoved?.Invoke(this, entity);
            }
            return removed;
        }
    }

    private void OnEntityDestroyed(Entity entity)
    {
        RemoveEntityInternal(entity);
    }

    private void OnEntityMovedToAnotherChunk(Entity entity, Vector3Int oldChunk, Vector3Int newChunk)
    {
        if(newChunk != Position)
        {
            RemoveEntityInternal(entity);
            _ = World.AddEntity(entity);
        }
    }

    protected override void OnEnabled()
    {
        Initialized = true;
        _ = DisableEntitiesUntilModelUpdate();
    }

    protected virtual async Task DisableEntitiesUntilModelUpdate()
    {
        ThreadSafe.AssertIsMainThread();
        EntitiesParent.Enabled = false;
        await GetModelUpdateTask();
        await Task.MainThread();
        EntitiesParent.Enabled = true;
    }

    protected override void OnDestroy()
    {
        _ = Clear().ContinueWithOnMainThread(t => Destroyed?.Invoke(this));
    }

    public virtual Task RequireModelUpdate() => ModelUpdater.RequireModelUpdate();
    public virtual Task GetModelUpdateTask() => ModelUpdater.GetModelUpdateTask();

    protected virtual Task GetModelUpdateTask(BlockSetFlags flags, bool wasModified = true)
    {
        Task modelUpdateTask;

        if(wasModified && flags.HasFlag(BlockSetFlags.UpdateModel))
            modelUpdateTask = RequireModelUpdate();
        else
            modelUpdateTask = GetModelUpdateTask();

        if(flags.HasFlag(BlockSetFlags.AwaitModelUpdate))
            return modelUpdateTask;

        return Task.CompletedTask;
    }


    public virtual void Tick()
    {
        TickBlockEntities();
    }

    // Call only im main thread
    protected virtual void TickBlockEntities()
    {
        ThreadSafe.AssertIsMainThread();

        lock(Blocks)
        {
            foreach(var (_, blockEntity) in Blocks.BlockEntities)
            {
                if(blockEntity is ITickable tickable)
                    tickable.Tick();
            }
        }
    }

    // Thread safe
    public BlockState GetBlockState(Vector3Int position)
    {
        lock(Blocks)
        {
            return Blocks.GetBlockState(position);
        }
    }

    // Thread safe
    public BlockEntity? GetBlockEntity(Vector3Int position)
    {
        lock(Blocks)
        {
            return Blocks.GetBlockEntity(position);
        }
    }

    // Thread safe
    public async Task<BlockStateChangingResult> SetBlockState(Vector3Int localPosition, BlockState blockState, BlockSetFlags flags = BlockSetFlags.UpdateModel | BlockSetFlags.MarkDirty)
    {
        if(flags.HasFlag(BlockSetFlags.UpdateNeigbours))
            throw new NotSupportedException($"{BlockSetFlags.UpdateNeigbours} is not supported in {nameof(Chunk)}");

        if(!IsInBounds(localPosition))
            throw new ArgumentOutOfRangeException(nameof(localPosition), localPosition, "block position is out of chunk bounds");

        BlockStateChangingResult result;

        Task modelUpdateTask;
        lock(Blocks)
        {
            result = Blocks.PlaceBlock(localPosition, blockState, flags.HasFlag(BlockSetFlags.MarkDirty));
            modelUpdateTask = GetModelUpdateTask(flags, result.Changed);
        }
        await modelUpdateTask;
        return result;
    }

    // Thread safe
    public async Task<bool> SetBlockStates(IDictionary<Vector3Int, BlockState> blockStates, BlockSetFlags flags = BlockSetFlags.UpdateModel | BlockSetFlags.MarkDirty)
    {
        if(flags.HasFlag(BlockSetFlags.UpdateNeigbours))
            throw new NotSupportedException($"{BlockSetFlags.UpdateNeigbours} is not supported in {nameof(Chunk)}");

        bool modified = false;
        bool markDirty = flags.HasFlag(BlockSetFlags.MarkDirty);

        Task modelUpdateTask;
        lock(Blocks)
        {
            foreach(var (position, blockState) in blockStates)
            {
                if(!IsInBounds(position))
                    throw new InvalidOperationException($"block position {position} is out of chunk bounds");

                modified |= Blocks.PlaceBlock(position, blockState, markDirty);
            }

            modelUpdateTask = GetModelUpdateTask(flags, modified);
        }
        await modelUpdateTask;
        return modified;
    }

    public virtual async Task<bool> Clear(BlockSetFlags flags = BlockSetFlags.UpdateModel | BlockSetFlags.MarkDirty)
    {
        if(flags.HasFlag(BlockSetFlags.UpdateNeigbours))
            throw new NotSupportedException($"{BlockSetFlags.UpdateNeigbours} is not supported in {nameof(Chunk)}");

        bool modified = false;

        Task modelUpdateTask;
        lock(Blocks)
        {
            modified = Blocks.Clear(flags.HasFlag(BlockSetFlags.MarkDirty));
            modelUpdateTask = GetModelUpdateTask(flags, modified);
        }

        lock(_entities)
        {
            foreach(var entity in _entities.Values.ToList())
                modified |= RemoveEntityInternal(entity);
            _entities.Clear();
        }

        await modelUpdateTask;
        return modified;
    }

    public virtual bool IsInBounds(Vector3Int localPosition) => !localPosition.IsAnyAxis((a, v) => v < 0 || v >= Size.GetAxis(a));

    public virtual BlockEntity? GetExternalBlockEntity(Vector3Int localPosition)
    {
        if(IsInBounds(localPosition))
            return GetBlockEntity(localPosition);

        Vector3Int globalPosition = World.GetBlockWorldPosition(Position, localPosition);
        return World.GetBlockEntity(globalPosition);
    }

    public virtual void OnNeighbouringChunkEdgeUpdated(Direction directionToNeighbouringChunk,
        Vector3Int updatedBlockPosition, BlockState oldBlockState, BlockState newBlockState)
    {
        var sidedBlockPosition = updatedBlockPosition - directionToNeighbouringChunk;
        var sidedLocalBlockPosition = World.GetBlockPositionInChunk(sidedBlockPosition);
        var sidedBlockState = GetBlockState(sidedLocalBlockPosition);
        if(sidedBlockState.IsAir())
            return;

        RequireModelUpdate();
    }

    protected override void DrawGizmos()
    {
        Gizmo.Hitbox.BBox(ModelBounds);
    }


    public virtual (BinaryTag? Blocks, ListTag Entities) Save(IReadOnlySaveMarker saveMarker)
    {
        lock(Blocks)
        {
            lock(_entities)
            {
                MarkSaved(saveMarker);
                var blocks = Blocks.IsSaved ? null : SaveBlocks();
                var entities = SaveEntities();
                return (blocks, entities);
            }
        }
    }

    private BinaryTag SaveBlocks()
    {
        lock(Blocks)
        {
            return Blocks.Write(true);
        }
    }

    private ListTag SaveEntities()
    {
        lock(_entities)
        {
            ListTag result = new();

            foreach(var entity in Entities)
                result.Add(entity.Write());

            return result;
        }
    }

    public virtual Task Load(BinaryTag tag, BlockSetFlags flags = BlockSetFlags.UpdateModel)
    {
        if(flags.HasFlag(BlockSetFlags.UpdateNeigbours))
            throw new NotSupportedException($"{BlockSetFlags.UpdateNeigbours} is not supported in {nameof(Chunk)}");

        lock(Blocks)
        {
            Blocks.Read(tag);
            var result = GetModelUpdateTask(flags);
            _ = Task.RunInMainThreadAsync(() => DisableEntitiesUntilModelUpdate());
            return result;
        }
    }

    protected void MarkSaved(IReadOnlySaveMarker saveMarker)
    {
        lock(Blocks)
        {
            lock(_entities)
            {
                Blocks.MarkSaved(saveMarker);
                _entitiesSaveMarker = saveMarker;
            }
        }
    }
}

public static class ComponentListChunkExtensions
{
    public static T Create<T>(this ComponentList components, Vector3Int position, Vector3Int size, IWorldAccessor world, bool startEnabled = true) where T : Chunk, new()
    {
        var chunk = components.Create<T>(false);

        chunk.Initialize(position, size, world);
        chunk.Enabled = startEnabled;
        return chunk;
    }
}
