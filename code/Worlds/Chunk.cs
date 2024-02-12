using Sandbox;
using Sandcube.Blocks.Entities;
using Sandcube.Blocks.Interfaces;
using Sandcube.Blocks.States;
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
    [Property] public Vector3Int Position { get; internal set; }
    [Property] public Vector3Int Size { get; internal set; } = 16;
    [Property] public ChunkModelUpdater ModelUpdater { get; internal set; } = null!;


    public bool Initialized { get; private set; } = false;

    public IWorldProvider WorldProvider { get; internal set; } = null!;

    public BBox Bounds => ModelUpdater.Bounds;

    public Vector3Int BlockOffset => Position * Size;


    protected readonly object BlocksLock = new();
    protected readonly Dictionary<Vector3Int, BlockState> BlockStates = new();
    protected readonly SortedDictionary<Vector3Int, BlockEntity> BlockEntities = new(Vector3Int.XYZIterationComparer);


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

        lock(BlocksLock)
        {
            foreach(var (_, blockEntity) in BlockEntities)
            {
                if(blockEntity is ITickable tickable)
                    tickable.Tick();
            }
        }
    }

    // Thread safe
    public BlockState GetBlockState(Vector3Int position)
    {
        lock(BlocksLock)
        {
            return BlockStates.GetValueOrDefault(position, BlockState.Air);
        }
    }

    // Thread safe
    public BlockEntity? GetBlockEntity(Vector3Int position)
    {
        lock(BlocksLock)
        {
            return BlockEntities!.GetValueOrDefault(position, null);
        }
    }


    protected BlockStateChangingResult SetBlockStateInternal(Vector3Int localPosition, BlockState blockState)
    {
        lock(BlocksLock)
        {
            var oldState = BlockStates.GetValueOrDefault(localPosition, BlockState.Air);
            if(oldState == blockState)
                return new(false, oldState);

            BlockEntity? oldBlockEntity = null;
            if(BlockEntities.Remove(localPosition, out var oldEntity))
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
                    BlockStates[localPosition] = blockState;
                }
                else if(oldBlockEntity == null)
                {
                    BlockStates[localPosition] = blockState;

                    var newBlockEntity = entityBlock.CreateEntity(WorldProvider, globalPosition, blockState);
                    if(newBlockEntity is null)
                        throw new InvalidOperationException($"Couldn't create {typeof(BlockEntity)} for {blockState}");

                    BlockEntities[localPosition] = newBlockEntity;
                    newBlockEntity.OnCreated();
                }
                else
                {
                    BlockStates[localPosition] = blockState;
                    BlockEntities[localPosition] = oldBlockEntity;
                }
            }
            else
            {
                oldBlockEntity?.OnDestroyed();
                BlockStates[localPosition] = blockState;
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

        var result = SetBlockStateInternal(localPosition, blockState);

        if(flags.HasFlag(BlockSetFlags.UpdateModel))
            _ = RequireModelUpdate();

        if(flags.HasFlag(BlockSetFlags.AwaitModelUpdate))
            return GetModelUpdateTask().ContinueWith(t => result);

        return Task.FromResult(result);
    }

    public Task<bool> SetBlockStates(Vector3Int localPosition, BlockState[,,] blockStates, BlockSetFlags flags = BlockSetFlags.Default)
    {
        var size = new Vector3Int(blockStates.GetLength(0), blockStates.GetLength(1), blockStates.GetLength(2));
        if(size.IsAnyAxis(v => v <= 0))
            return Task.FromResult(false);

        var lastPosition = localPosition + size - Vector3Int.One;
        if(!IsInBounds(localPosition) || !IsInBounds(lastPosition))
            throw new InvalidOperationException($"setting range ({localPosition} - {lastPosition}) is out of chunk bounds");

        bool modified = false;
        for(int x = 0; x < size.x; ++x)
        {
            for(int y = 0; y < size.y; ++y)
            {
                for(int z = 0; z < size.z; ++z)
                {
                    modified |= SetBlockStateInternal(localPosition + new Vector3Int(x, y, z), blockStates[x, y, z]);
                }
            }
        }

        Task<bool> resultTask = ((flags.HasFlag(BlockSetFlags.UpdateModel) && modified) ? RequireModelUpdate() : GetModelUpdateTask())
            .ContinueWith(t => modified);

        if(flags.HasFlag(BlockSetFlags.AwaitModelUpdate))
            return resultTask;

        return Task.FromResult(modified);
    }

    public virtual void Clear()
    {
        lock(BlocksLock)
        {
            foreach(var (_, blockEntity) in BlockEntities)
                blockEntity.OnDestroyed();
            BlockStates.Clear();
            BlockEntities.Clear();
        }
        RequireModelUpdate();
    }

    protected virtual bool IsInBounds(Vector3Int localPosition) => !localPosition.IsAnyAxis((a, v) => v < 0 || v >= Size.GetAxis(a));

    public virtual BlockState GetExternalBlockState(Vector3Int localPosition)
    {
        if(IsInBounds(localPosition))
            return GetBlockState(localPosition);

        Vector3Int globalPosition = WorldProvider.GetBlockWorldPosition(Position, localPosition);
        return WorldProvider.GetBlockState(globalPosition);
    }

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
        var bounds = Bounds.Translate(-Transform.Position).Expanded(1);
        Gizmo.Hitbox.BBox(bounds);
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
