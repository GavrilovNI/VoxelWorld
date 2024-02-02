using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sandcube.Worlds;

public class Chunk : ThreadHelpComponent, IBlockStateAccessor
{
    [Property] public Vector3Int Position { get; internal set; }
    [Property] public Vector3Int Size { get; internal set; } = 16;
    [Property] public ChunkModelUpdater ModelUpdater { get; internal set; } = null!;


    public bool Initialized { get; private set; } = false;

    public IWorldProvider? WorldProvider { get; internal set; }

    protected readonly ConcurrentDictionary<Vector3Int, BlockState> _blockStates = new();


    public BBox Bounds => ModelUpdater.Bounds;

    public Vector3Int BlockOffset => Position * Size;

    public virtual void Initialize(Vector3Int position, Vector3Int size, IWorldProvider? worldProvider)
    {
        if(Initialized)
            throw new InvalidOperationException($"{nameof(Chunk)} {this} was already initialized or enabled");
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

    public virtual void UpdateTexture(Texture texture) => ModelUpdater.UpdateTexture(texture);


    // Thread safe
    public BlockState GetBlockState(Vector3Int position) => _blockStates.GetValueOrDefault(position, BlockState.Air);

    // Thread safe
    public Task SetBlockState(Vector3Int localPosition, BlockState blockState)
    {
        if(!IsInBounds(localPosition))
            throw new ArgumentOutOfRangeException(nameof(localPosition), localPosition, "block position is out of chunk bounds");

        if(GetBlockState(localPosition) == blockState)
            return Task.CompletedTask;

        _blockStates[localPosition] = blockState;
        return RequireModelUpdate();
    }

    public Task SetBlockStates(Vector3Int localPosition, BlockState[,,] blockStates)
    {
        var size = new Vector3Int(blockStates.GetLength(0), blockStates.GetLength(1), blockStates.GetLength(2));
        if(size.IsAnyAxis(v => v <= 0))
            return Task.CompletedTask;

        var lastPosition = localPosition + size - Vector3Int.One;
        if(!IsInBounds(localPosition) || !IsInBounds(lastPosition))
            throw new InvalidOperationException($"setting range ({localPosition} - {lastPosition}) is out of chunk bounds");

        for(int x = 0; x < size.x; ++x)
        {
            for(int y = 0; y < size.y; ++y)
            {
                for(int z = 0; z < size.z; ++z)
                {
                    _blockStates[new(x, y, z)] = blockStates[x, y, z];
                }
            }
        }
        return RequireModelUpdate();
    }

    public virtual void Clear()
    {
        _blockStates.Clear();
        RequireModelUpdate();
    }

    protected virtual bool IsInBounds(Vector3Int localPosition) => !localPosition.IsAnyAxis((a, v) => v < 0 || v >= Size.GetAxis(a));

    public virtual BlockState GetExternalBlockState(Vector3Int localPosition)
    {
        if(IsInBounds(localPosition))
            return GetBlockState(localPosition);

        if(WorldProvider is null)
            return BlockState.Air;

        Vector3Int globalPosition = WorldProvider.GetBlockWorldPosition(Position, localPosition);
        return WorldProvider.GetBlockState(globalPosition);
    }

    public virtual void OnNeighbouringChunkEdgeUpdated(Direction directionToNeighbouringChunk,
        Vector3Int updatedBlockPosition, BlockState oldBlockState, BlockState newBlockState)
    {
        if(WorldProvider is not null)
        {
            var sidedBlockPosition = updatedBlockPosition - directionToNeighbouringChunk;
            var sidedLocalBlockPosition = WorldProvider.GetBlockPositionInChunk(sidedBlockPosition);
            var sidedBlockState = GetBlockState(sidedLocalBlockPosition);
            if(sidedBlockState.IsAir())
                return;
        }

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
    public static T Create<T>(this ComponentList components, Vector3Int position, Vector3Int size, IWorldProvider? worldProvider, bool startEnabled = true) where T : Chunk, new()
    {
        var chunk = components.Create<T>(false);

        chunk.Initialize(position, size, worldProvider);
        chunk.Enabled = startEnabled;
        return chunk;
    }
}
