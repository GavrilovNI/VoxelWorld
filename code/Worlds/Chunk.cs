using Sandbox;
using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Data;
using Sandcube.Interfaces;
using Sandcube.IO;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sandcube.Worlds;

public class Chunk : Component, IBlockStateAccessor, IBlockEntityProvider, ITickable
{
    public event Action<Chunk>? Destroyed = null;

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
                return Blocks.IsSaved;
            }
        }
    }

    protected BlocksContainer Blocks = null!;

    public virtual void Initialize(Vector3Int position, Vector3Int size, IWorldAccessor world)
    {
        if(Initialized)
            throw new InvalidOperationException($"{nameof(Chunk)} {this} was already initialized or enabled");
        ArgumentNullException.ThrowIfNull(world);

        Initialized = true;

        Position = position;
        Size = size;
        World = world;
        Blocks = new(world, position, size);
    }

    protected override void OnEnabled()
    {
        Initialized = true;
    }

    protected override void OnDestroy()
    {
        Destroyed?.Invoke(this);
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

    public virtual async Task<bool> Clear(BlockSetFlags flags = BlockSetFlags.Default)
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

    // thread safe
    public virtual Task Load(IReadOnlyBlocksData data, BlockSetFlags flags = BlockSetFlags.UpdateModel)
    {
        if(flags.HasFlag(BlockSetFlags.UpdateNeigbours))
            throw new NotSupportedException($"{BlockSetFlags.UpdateNeigbours} is not supported in {nameof(Chunk)}");

        lock(Blocks)
        {
            Blocks.Load(data, Size.GetPositionsFromZero(false), flags.HasFlag(BlockSetFlags.MarkDirty));
            return GetModelUpdateTask(flags);
        }
    }

    public virtual BlocksData Save(IReadOnlySaveMarker saveMarker)
    {
        lock(Blocks)
        {
            MarkSaved(saveMarker);
            return Blocks.ToBlocksData();
        }
    }

    protected void MarkSaved(IReadOnlySaveMarker saveMarker)
    {
        lock(Blocks)
        {
            Blocks.MarkSaved(saveMarker);
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
