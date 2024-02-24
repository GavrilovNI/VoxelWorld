using Sandcube.Blocks.Entities;
using Sandcube.Blocks.Interfaces;
using Sandcube.Blocks.States;
using Sandcube.Data.Enumarating;
using Sandcube.IO;
using Sandcube.Mth;
using Sandcube.Worlds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Sandcube.Data;

public class BlocksContainer : ISaveStatusMarkable
{
    private readonly Dictionary<Vector3Int, BlockState> _blockStates = new();
    private readonly SortedDictionary<Vector3Int, BlockEntity> _blockEntities = new(Vector3Int.XYZIterationComparer);

    protected readonly IWorldProvider WorldProvider;
    protected readonly Vector3Int ChunkPosition;
    protected readonly Vector3Int ChunkSize;

    public IEnumerable<KeyValuePair<Vector3Int, BlockState>> BlockStates => _blockStates.AsEnumerable();
    public IEnumerable<KeyValuePair<Vector3Int, BlockEntity>> BlockEntities => _blockEntities.AsEnumerable();

    private IReadOnlySaveMarker _saveMarker = SaveMarker.Saved;
    public bool IsSaved
    {
        get
        {
            if(!_saveMarker.IsSaved)
                return false;

            foreach(var (_, blockEntity) in BlockEntities)
            {
                if(!blockEntity.IsSaved)
                {
                    _saveMarker = SaveMarker.NotSaved;
                    break;
                }
            }

            return _saveMarker.IsSaved;
        }
    }

    public BlocksContainer(IWorldProvider worldProvider, Vector3Int chunkPosition, Vector3Int chunkSize)
    {
        WorldProvider = worldProvider;
        ChunkPosition = chunkPosition;
        ChunkSize = chunkSize;
    }

    public BlocksData ToBlocksData() => new(_blockStates, _blockEntities);

    public bool IsEmpty() => _blockStates.Count == 0 && _blockEntities.Count == 0;

    public bool Clear(bool markDirty = true)
    {
        if(IsEmpty())
            return false;

        foreach(var (_, blockEntity) in _blockEntities)
            blockEntity.OnDestroyed();

        _blockStates.Clear();
        _blockEntities.Clear();

        if(markDirty)
            MarkNotSaved();

        return true;
    }

    public BlockState GetBlockState(Vector3Int position) => _blockStates!.GetValueOrDefault(position, BlockState.Air);
    public BlockEntity? GetBlockEntity(Vector3Int position) => _blockEntities!.GetValueOrDefault(position, null);

    public void SetBlockState(Vector3Int position, BlockState blockState, bool markDirty = true)
    {
        if(blockState.IsAir())
        {
            _blockStates.Remove(position);
            return;
        }

        _blockStates[position] = blockState;

        if(markDirty)
            MarkNotSaved();
    }

    public void SetBlockEntity(Vector3Int position, BlockEntity blockEntity, bool markDirty = true)
    {
        RemoveBlockEntity(position, false);
        _blockEntities[position] = blockEntity;
        blockEntity.OnCreated();

        if(markDirty)
            MarkNotSaved();
    }

    public bool RemoveBlockEntity(Vector3Int position, bool markDirty = true) => RemoveBlockEntity(position, out _, true, markDirty);
    private bool RemoveBlockEntity(Vector3Int position, out BlockEntity oldBlockEntity, bool destroyBlockEntity = true, bool markDirty = true)
    {
        bool removed = _blockEntities.Remove(position, out oldBlockEntity!);
        if(removed && destroyBlockEntity)
            oldBlockEntity.OnDestroyed();

        if(markDirty)
            MarkNotSaved();

        return removed;
    }

    public BlockStateChangingResult PlaceBlock(Vector3Int position, BlockState blockState, bool markDirty = true)
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
                    SetBlockState(position, blockState, false);
                }
                else
                {
                    RemoveBlockEntity(position, false);

                    var newBlockEntity = entityBlock.CreateEntity(WorldProvider, globalPosition, blockState);
                    if(newBlockEntity is null)
                        throw new InvalidOperationException($"Couldn't create {typeof(BlockEntity)} for {blockState}");

                    SetBlockState(position, blockState, false);
                    SetBlockEntity(position, newBlockEntity, false);
                }
            }
            else
            {
                RemoveBlockEntity(position, false);
                SetBlockState(position, blockState, false);
            }
        }
        else
        {
            RemoveBlockEntity(position, false);
            SetBlockState(position, blockState, false);
        }

        if(markDirty)
            MarkNotSaved();

        return new(true, oldState);
    }

    public void Load(IReadOnlyBlocksData data, IEnumerator<Vector3Int> positions, bool setDirty = false)
    {
        foreach(var blockPosition in positions)
        {
            RemoveBlockEntity(blockPosition, false);

            var blockState = data.GetBlockState(blockPosition) ?? BlockState.Air;
            PlaceBlock(blockPosition, blockState, false);

            var blockEntity = GetBlockEntity(blockPosition);
            if(blockEntity is null)
                continue;

            data.UpdateEntity(blockPosition, blockEntity);
        }

        if(setDirty)
            MarkNotSaved();
        else
            MarkSaved(SaveMarker.Saved);
    }

    public void MarkSaved(IReadOnlySaveMarker saveMarker)
    {
        if(IsSaved)
            return;

        _saveMarker = saveMarker;
        foreach(var (_, blockEntity) in BlockEntities)
            blockEntity.MarkSaved(saveMarker);
    }

    protected void MarkNotSaved()
    {
        _saveMarker = SaveMarker.NotSaved;
    }
}
