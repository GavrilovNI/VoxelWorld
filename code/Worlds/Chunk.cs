using Sandbox;
using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Data;
using Sandcube.Data.Enumarating;
using Sandcube.Interfaces;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Threading;
using System;
using System.Collections.Generic;
using System.Drawing;
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

    public Vector3Int BlockOffset => Position * Size;

    private bool _isDirty = false;
    public bool IsDirty
    {
        get
        {
            lock(Blocks)
            {
                foreach(var (_, blockEntity) in Blocks.BlockEntities)
                {
                    if(blockEntity.IsDirty)
                    {
                        _isDirty = true;
                        break;
                    }
                }

                return _isDirty;
            }
        }
        protected set
        {
            lock(Blocks)
            {
                _isDirty = value;
            }
        }
    }

    protected BlocksContainer Blocks = null!;

    public virtual void Initialize(Vector3Int position, Vector3Int size, IWorldProvider worldProvider)
    {
        if(Initialized)
            throw new InvalidOperationException($"{nameof(Chunk)} {this} was already initialized or enabled");
        ArgumentNullException.ThrowIfNull(worldProvider);

        Initialized = true;

        Position = position;
        Size = size;
        WorldProvider = worldProvider;
        Blocks = new(worldProvider, position, size);
    }

    protected override void OnEnabled()
    {
        Initialized = true;
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
    public Task<BlockStateChangingResult> SetBlockState(Vector3Int localPosition, BlockState blockState, BlockSetFlags flags = BlockSetFlags.Default)
    {
        if(flags.HasFlag(BlockSetFlags.UpdateNeigbours))
            throw new NotSupportedException($"{BlockSetFlags.UpdateNeigbours} is not supported in {nameof(Chunk)}");

        if(!IsInBounds(localPosition))
            throw new ArgumentOutOfRangeException(nameof(localPosition), localPosition, "block position is out of chunk bounds");

        BlockStateChangingResult result;

        lock(Blocks)
        {
            result = Blocks.PlaceBlock(localPosition, blockState);

            if(result.Changed && flags.HasFlag(BlockSetFlags.MarkDirty))
                IsDirty = true;

            return GetModelUpdateTask(flags, result.Changed).ContinueWith(t => result);
        }
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
            for(int x = 0; x < size.x; ++x)
            {
                for(int y = 0; y < size.y; ++y)
                {
                    for(int z = 0; z < size.z; ++z)
                    {
                        modified |= Blocks.PlaceBlock(localPosition + new Vector3Int(x, y, z), blockStates[x, y, z]);
                    }
                }
            }

            if(modified && flags.HasFlag(BlockSetFlags.MarkDirty))
                IsDirty = true;

            return GetModelUpdateTask(flags, modified).ContinueWith(t => modified);
        }
    }

    public virtual Task<bool> Clear(BlockSetFlags flags = BlockSetFlags.Default)
    {
        if(flags.HasFlag(BlockSetFlags.UpdateNeigbours))
            throw new NotSupportedException($"{BlockSetFlags.UpdateNeigbours} is not supported in {nameof(Chunk)}");

        bool modified = false;

        lock(Blocks)
        {
            modified = Blocks.Clear();
            if(modified)
            {
                if(flags.HasFlag(BlockSetFlags.MarkDirty))
                    IsDirty = true;
            }

            return GetModelUpdateTask(flags, modified).ContinueWith(t => modified);
        }
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

    // thread safe
    public virtual Task Load(BlocksData data, BlockSetFlags flags = BlockSetFlags.UpdateModel)
    {
        if(flags.HasFlag(BlockSetFlags.UpdateNeigbours))
            throw new NotSupportedException($"{BlockSetFlags.UpdateNeigbours} is not supported in {nameof(Chunk)}");

        lock(Blocks)
        {
            var setDirty = flags.HasFlag(BlockSetFlags.MarkDirty);

            foreach(var blockPosition in Size.GetPositionsFromZero(false))
            {
                Blocks.RemoveBlockEntity(blockPosition);

                var blockState = data.BlockStates.GetValueOrDefault(blockPosition, BlockState.Air);
                Blocks.PlaceBlock(blockPosition, blockState);

                var blockEntity = Blocks.GetBlockEntity(blockPosition);
                if(blockEntity is null)
                    continue;

                data.UpdateEntity(blockEntity, setDirty);
            }

            IsDirty = setDirty;

            return GetModelUpdateTask(flags);
        }
    }

    public virtual BlocksData Save(bool keepDirty = false)
    {
        lock(Blocks)
        {
            if(!keepDirty)
                IsDirty = false;
            return Blocks.ToBlocksData(keepDirty);
        }
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
