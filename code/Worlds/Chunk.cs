using Sandbox;
using Sandcube.Blocks.Entities;
using Sandcube.Blocks.Interfaces;
using Sandcube.Blocks.States;
using Sandcube.Data;
using Sandcube.Data.Enumarating;
using Sandcube.Interfaces;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Threading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sandcube.Worlds;

public class Chunk : ThreadHelpComponent, IBlockStateAccessor, IBlockEntityProvider, ITickable
{
    [Property, HideIf(nameof(Initialized), true)] public Vector3Int Position { get; internal set; }
    [Property, HideIf(nameof(Initialized), true)] public Vector3Int Size { get; internal set; } = 16;
    [Property] public ChunkModelUpdater ModelUpdater { get; internal set; } = null!;

    public bool Initialized { get; private set; } = false;

    public IWorldProvider WorldProvider { get; internal set; } = null!;

    public BBox ModelBounds => ModelUpdater.Bounds;

    public BBoxInt Bounds => BBoxInt.FromMinsAndSize(Position, Size);

    public Vector3Int BlockOffset => Position * Size;

    private bool _isDirty = false;
    public bool IsDirty
    {
        get
        {
            lock(Blocks)
            {
                // TODO: test blockEntities
                return _isDirty;
            }
        }
        protected set
        {
            lock(Blocks)
            {
                _isDirty = value;
                // TODO: reset blockEntities?
            }
        }
    }

    protected readonly BlocksContainer Blocks = new();

    public virtual void Initialize(Vector3Int position, Vector3Int size, IWorldProvider worldProvider)
    {
        if(Initialized)
            throw new InvalidOperationException($"{nameof(Chunk)} {this} was already initialized or enabled");
        ArgumentNullException.ThrowIfNull(worldProvider);

        Initialized = true;

        Position = position;
        Size = size;
        WorldProvider = worldProvider;
    }

    protected override void OnEnabled()
    {
        Initialized = true;
    }

    public virtual Task RequireModelUpdate() => ModelUpdater.RequireModelUpdate();
    public virtual Task GetModelUpdateTask() => ModelUpdater.GetModelUpdateTask();

    public virtual void UpdateTexture(Texture texture) => ModelUpdater.UpdateTexture(texture);


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
            return Blocks.BlockStates.GetValueOrDefault(position, BlockState.Air);
        }
    }

    // Thread safe
    public BlockEntity? GetBlockEntity(Vector3Int position)
    {
        lock(Blocks)
        {
            return Blocks.BlockEntities!.GetValueOrDefault(position, null);
        }
    }


    protected BlockStateChangingResult SetBlockStateInternal(Vector3Int localPosition, BlockState blockState)
    {
        lock(Blocks)
        {
            var oldState = Blocks.BlockStates.GetValueOrDefault(localPosition, BlockState.Air);
            if(oldState == blockState)
                return new(false, oldState);

            BlockEntity? oldBlockEntity = null;
            if(Blocks.BlockEntities.Remove(localPosition, out var oldEntity))
            {
                if(oldState.Block == blockState.Block)
                    oldBlockEntity = oldEntity;
                else
                    oldEntity.OnDestroyed();
            }

            if(blockState.Block is IEntityBlock entityBlock)
            {
                var globalPosition = WorldProvider.GetBlockWorldPosition(Position, localPosition);
                if(!entityBlock.HasEntity(WorldProvider, globalPosition, blockState))
                {
                    oldBlockEntity?.OnDestroyed();
                    Blocks.BlockStates[localPosition] = blockState;
                }
                else if(oldBlockEntity == null)
                {
                    Blocks.BlockStates[localPosition] = blockState;

                    var newBlockEntity = entityBlock.CreateEntity(WorldProvider, globalPosition, blockState);
                    if(newBlockEntity is null)
                        throw new InvalidOperationException($"Couldn't create {typeof(BlockEntity)} for {blockState}");

                    Blocks.BlockEntities[localPosition] = newBlockEntity;
                    newBlockEntity.OnCreated();
                }
                else
                {
                    Blocks.BlockStates[localPosition] = blockState;
                    Blocks.BlockEntities[localPosition] = oldBlockEntity;
                }
            }
            else
            {
                oldBlockEntity?.OnDestroyed();
                Blocks.BlockStates[localPosition] = blockState;
            }

            return new(true, oldState);
        }
    }

    // Thread safe
    public Task<BlockStateChangingResult> SetBlockState(Vector3Int localPosition, BlockState blockState, BlockSetFlags flags = BlockSetFlags.Default)
    {
        if(flags.HasFlag(BlockSetFlags.UpdateNeigbours))
            throw new NotSupportedException($"{BlockSetFlags.UpdateNeigbours} is not supported in {nameof(Chunk)}");

        if(!IsInBounds(localPosition))
            throw new ArgumentOutOfRangeException(nameof(localPosition), localPosition, "block position is out of chunk bounds");

        BlockStateChangingResult result;

        lock(Blocks)
        {
            result = SetBlockStateInternal(localPosition, blockState);

            if(result.Changed && flags.HasFlag(BlockSetFlags.MarkDirty))
                IsDirty = true;
        }

        if(flags.HasFlag(BlockSetFlags.UpdateModel))
            _ = RequireModelUpdate();

        if(flags.HasFlag(BlockSetFlags.AwaitModelUpdate))
            return GetModelUpdateTask().ContinueWith(t => result);

        return Task.FromResult(result);
    }

    // Thread safe
    public Task<bool> SetBlockStates(Vector3Int localPosition, BlockState[,,] blockStates, BlockSetFlags flags = BlockSetFlags.Default)
    {
        var size = new Vector3Int(blockStates.GetLength(0), blockStates.GetLength(1), blockStates.GetLength(2));
        if(size.IsAnyAxis(v => v <= 0))
            return Task.FromResult(false);

        var lastPosition = localPosition + size - Vector3Int.One;
        if(!IsInBounds(localPosition) || !IsInBounds(lastPosition))
            throw new InvalidOperationException($"setting range ({localPosition} - {lastPosition}) is out of chunk bounds");

        bool modified = false;
        lock(Blocks)
        {
            foreach(var blockPosition in Bounds.GetPositions(false))
            {
                var blockState = blockStates[blockPosition.x, blockPosition.y, blockPosition.z];
                modified |= SetBlockStateInternal(localPosition + blockPosition, blockState);
            }

            if(modified && flags.HasFlag(BlockSetFlags.MarkDirty))
                IsDirty = true;
        }

        Task<bool> resultTask = ((flags.HasFlag(BlockSetFlags.UpdateModel) && modified) ? RequireModelUpdate() : GetModelUpdateTask())
            .ContinueWith(t => modified);

        if(flags.HasFlag(BlockSetFlags.AwaitModelUpdate))
            return resultTask;

        return Task.FromResult(modified);
    }

    public virtual Task<bool> Clear(BlockSetFlags flags = BlockSetFlags.Default)
    {
        if(flags.HasFlag(BlockSetFlags.UpdateNeigbours))
            throw new NotSupportedException($"{BlockSetFlags.UpdateNeigbours} is not supported in {nameof(Chunk)}");

        bool modified = false;

        lock(Blocks)
        {
            modified = !Blocks.IsEmpty();
            if(modified)
            {
                foreach(var (_, blockEntity) in Blocks.BlockEntities)
                    blockEntity.OnDestroyed();
                Blocks.Clear();
                if(flags.HasFlag(BlockSetFlags.MarkDirty))
                    IsDirty = true;
            }
        }

        Task<bool> resultTask = ((flags.HasFlag(BlockSetFlags.UpdateModel) && modified) ? RequireModelUpdate() : GetModelUpdateTask())
            .ContinueWith(t => modified);

        if(flags.HasFlag(BlockSetFlags.AwaitModelUpdate))
            return resultTask;

        return Task.FromResult(modified);
    }

    public virtual bool IsInBounds(Vector3Int localPosition) => !localPosition.IsAnyAxis((a, v) => v < 0 || v >= Size.GetAxis(a));

    public virtual BlockEntity? GetExternalBlockEntity(Vector3Int localPosition)
    {
        if(IsInBounds(localPosition))
            return GetBlockEntity(localPosition);

        Vector3Int globalPosition = WorldProvider.GetBlockWorldPosition(Position, localPosition);
        return WorldProvider.GetBlockEntity(globalPosition);
    }

    public virtual void OnNeighbouringChunkEdgeUpdated(Direction directionToNeighbouringChunk,
        Vector3Int updatedBlockPosition, BlockState oldBlockState, BlockState newBlockState)
    {
        var sidedBlockPosition = updatedBlockPosition - directionToNeighbouringChunk;
        var sidedLocalBlockPosition = WorldProvider.GetBlockPositionInChunk(sidedBlockPosition);
        var sidedBlockState = GetBlockState(sidedLocalBlockPosition);
        if(sidedBlockState.IsAir())
            return;

        RequireModelUpdate();
    }

    protected override void DrawGizmos()
    {
        var bounds = ModelBounds.Translate(-Transform.Position).Expanded(1);
        Gizmo.Hitbox.BBox(bounds);
    }

    public virtual Task Load(IReadOnlyBlocksContainer blocks, BlockSetFlags flags = BlockSetFlags.UpdateModel)
    {
        if(flags.HasFlag(BlockSetFlags.UpdateNeigbours))
            throw new NotSupportedException($"{BlockSetFlags.UpdateNeigbours} is not supported in {nameof(Chunk)}");

        lock(Blocks)
        {
            foreach(var blockPosition in Bounds.GetPositions(false))
            {
                if(Blocks.BlockEntities.Remove(blockPosition, out var oldBlockEntity))
                    oldBlockEntity.OnDestroyed();

                var blockState = blocks.GetBlockState(blockPosition) ?? BlockState.Air;
                Blocks.BlockStates[blockPosition] = blockState;

                var blockEntity = blocks.GetBlockEntity(blockPosition);
                if(blockEntity is not null)
                    Blocks.BlockEntities[blockPosition] = blockEntity;
            }

            if(flags.HasFlag(BlockSetFlags.MarkDirty))
                IsDirty = true;
            else
                IsDirty = false;
        }

        Task resultTask = (flags.HasFlag(BlockSetFlags.UpdateModel) ? RequireModelUpdate() : GetModelUpdateTask());

        if(flags.HasFlag(BlockSetFlags.AwaitModelUpdate))
            return resultTask;

        return Task.CompletedTask;
    }
}

public static class ComponentListChunkExtensions
{
    public static T Create<T>(this ComponentList components, Vector3Int position, Vector3Int size, IWorldProvider worldProvider, bool startEnabled = true) where T : Chunk, new()
    {
        var chunk = components.Create<T>(false);

        chunk.Initialize(position, size, worldProvider);
        chunk.Enabled = startEnabled;
        return chunk;
    }
}
