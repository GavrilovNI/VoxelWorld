using Sandcube.Blocks.Entities;
using Sandcube.Blocks.Interfaces;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Worlds;
using System;
using System.Collections.Generic;

namespace Sandcube.Data;

public class BlocksContainer
{
    private readonly Dictionary<Vector3Int, BlockState> _blockStates = new();
    private readonly SortedDictionary<Vector3Int, BlockEntity> _blockEntities = new(Vector3Int.XYZIterationComparer);

    protected readonly IWorldProvider WorldProvider;
    protected readonly Vector3Int ChunkPosition;
    protected readonly Vector3Int ChunkSize;

    public IEnumerator<KeyValuePair<Vector3Int, BlockState>> BlockStates => _blockStates.GetEnumerator();
    public IEnumerator<KeyValuePair<Vector3Int, BlockEntity>> BlockEntities => _blockEntities.GetEnumerator();

    public BlocksContainer(IWorldProvider worldProvider, Vector3Int chunkPosition, Vector3Int chunkSize)
    {
        WorldProvider = worldProvider;
        ChunkPosition = chunkPosition;
        ChunkSize = chunkSize;
    }

    public BlocksData ToBlocksData(bool keepEntitiesDirty = false) => new(_blockStates, _blockEntities, keepEntitiesDirty);

    public bool IsEmpty() => _blockStates.Count == 0 && _blockEntities.Count == 0;

    public bool Clear()
    {
        if(IsEmpty())
            return false;

        foreach(var (_, blockEntity) in _blockEntities)
            blockEntity.OnDestroyed();

        _blockStates.Clear();
        _blockEntities.Clear();
        return true;
    }

    public BlockState GetBlockState(Vector3Int position) => _blockStates!.GetValueOrDefault(position, BlockState.Air);
    public BlockEntity? GetBlockEntity(Vector3Int position) => _blockEntities!.GetValueOrDefault(position, null);

    public void SetBlockState(Vector3Int position, BlockState blockState)
    {
        if(blockState.IsAir())
        {
            _blockStates.Remove(position);
            return;
        }

        _blockStates[position] = blockState;
    }

    public void SetBlockEntity(Vector3Int position, BlockEntity blockEntity)
    {
        RemoveBlockEntity(position);
        _blockEntities[position] = blockEntity;
        blockEntity.OnCreated();
    }

    public bool RemoveBlockEntity(Vector3Int position) => RemoveBlockEntity(position, out _);
    private bool RemoveBlockEntity(Vector3Int position, out BlockEntity oldBlockEntity) => RemoveBlockEntity(position, out oldBlockEntity, true);
    private bool RemoveBlockEntity(Vector3Int position, out BlockEntity oldBlockEntity, bool destroyBlockEntity = true)
    {
        bool removed = _blockEntities.Remove(position, out oldBlockEntity!);
        if(removed && destroyBlockEntity)
            oldBlockEntity.OnDestroyed();
        return removed;
    }

    public BlockStateChangingResult PlaceBlock(Vector3Int position, BlockState blockState)
    {
        var oldState = GetBlockState(position);
        if(oldState == blockState)
            return new(false, oldState);

        if(blockState.Block is IEntityBlock entityBlock)
        {
            var globalPosition = WorldProvider.GetBlockWorldPosition(ChunkPosition, position);

            if(entityBlock.HasEntity(WorldProvider, globalPosition, blockState))
            {
                var oldBlockEntity = GetBlockEntity(position);

                bool isValid = oldBlockEntity is not null &&
                    entityBlock.IsValidEntity(WorldProvider, globalPosition, blockState, oldBlockEntity);

                if(isValid)
                {
                    SetBlockState(position, blockState);
                }
                else
                {
                    RemoveBlockEntity(position);

                    var newBlockEntity = entityBlock.CreateEntity(WorldProvider, globalPosition, blockState);
                    if(newBlockEntity is null)
                        throw new InvalidOperationException($"Couldn't create {typeof(BlockEntity)} for {blockState}");

                    SetBlockState(position, blockState);
                    SetBlockEntity(position, newBlockEntity);
                }
            }
            else
            {
                RemoveBlockEntity(position);
                SetBlockState(position, blockState);
            }
        }
        else
        {
            RemoveBlockEntity(position);
            SetBlockState(position, blockState);
        }

        return new(true, oldState);
    }
}
