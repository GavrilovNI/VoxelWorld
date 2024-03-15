using VoxelWorld.Blocks.Entities;
using VoxelWorld.Blocks.Interfaces;
using VoxelWorld.Blocks.States;
using VoxelWorld.IO;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.Mth;
using VoxelWorld.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace VoxelWorld.Data.Blocks;

public class SizedBlocksCollection : ISaveStatusMarkable
{
    public event Action<BlockEntity>? BlockEntityAdded;
    public event Action<BlockEntity>? BlockEntityRemoved;

    private readonly Dictionary<Vector3Int, BlockState> _blockStates = new();
    private readonly Dictionary<Vector3Int, BlockEntity> _blockEntities = new();

    protected IWorldAccessor World { get; }
    protected Vector3Int Offset { get; }
    public Vector3Int Size { get; }
    public bool AutoDestroyOldBlockEntities { get; set; } = true;

    public bool IsEmpty => _blockStates.Count == 0 && _blockEntities.Count == 0;

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


    public SizedBlocksCollection(IWorldAccessor world, Vector3Int offset, Vector3Int size)
    {
        if(size.IsAnyAxis(a => a <= 0))
            throw new ArgumentException("all axes of size must be positive");

        World = world;
        Offset = offset;
        Size = size;
    }

    public BlockState GetBlockState(Vector3Int position)
    {
        AssertPosition(position);
        return _blockStates!.GetValueOrDefault(position, BlockState.Air);
    }

    public BlockEntity? GetBlockEntity(Vector3Int position)
    {
        AssertPosition(position);
        return _blockEntities!.GetValueOrDefault(position, null);
    }

    public void SetBlockState(Vector3Int position, BlockState blockState, bool markDirty = true)
    {
        AssertPosition(position);

        if(GetBlockState(position) == blockState)
            return;

        if(blockState.IsAir())
        {
            _blockStates.Remove(position);
            return;
        }

        _blockStates[position] = blockState;

        if(markDirty)
            MarkNotSaved();
    }

    public BlockStateChangingResult PlaceBlock(Vector3Int position, BlockState blockState, bool markDirty = true)
    {
        var oldState = GetBlockState(position);
        if(oldState == blockState)
            return BlockStateChangingResult.NotChanged;

        if(blockState.Block is IEntityBlock entityBlock)
        {
            var globalPosition = Offset + position;

            if(entityBlock.HasEntity(World, globalPosition, blockState))
            {
                var oldBlockEntity = GetBlockEntity(position);

                bool isValid = oldBlockEntity is not null &&
                    entityBlock.IsValidEntity(World, globalPosition, blockState, oldBlockEntity);

                if(isValid)
                {
                    SetBlockState(position, blockState, false);
                }
                else
                {
                    RemoveBlockEntity(position, false);

                    var newBlockEntity = entityBlock.CreateEntity(World, globalPosition, blockState);
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

        return BlockStateChangingResult.FromChanged(oldState);
    }

    public void SetBlockEntity(Vector3Int position, BlockEntity blockEntity, bool markDirty = true)
    {
        ArgumentNullException.ThrowIfNull(blockEntity);
        AssertPosition(position);

        if(object.ReferenceEquals(GetBlockEntity(position), blockEntity))
            return;

        RemoveBlockEntity(position, false);
        _blockEntities[position] = blockEntity;
        BlockEntityAdded?.Invoke(blockEntity);

        if(markDirty)
            MarkNotSaved();
    }

    public bool RemoveBlockEntity(Vector3Int position, bool markDirty = true)
    {
        AssertPosition(position);

        bool hasBlockEntity = _blockEntities.TryGetValue(position, out var blockEntity);
        if(hasBlockEntity)
        {
            if(AutoDestroyOldBlockEntities)
                blockEntity!.OnDestroyed();
            _blockEntities.Remove(position);
            BlockEntityRemoved?.Invoke(blockEntity!);

            if(markDirty)
                MarkNotSaved();
        }

        return hasBlockEntity;
    }

    public bool Clear(bool markDirty = true)
    {
        if(IsEmpty)
            return false;

        if(AutoDestroyOldBlockEntities)
        {
            foreach(var (_, blockEntity) in _blockEntities)
                blockEntity.OnDestroyed();
        }

        _blockStates.Clear();
        foreach(var (_, blockEntity) in BlockEntities)
            BlockEntityRemoved?.Invoke(blockEntity);
        _blockEntities.Clear();

        if(markDirty)
            MarkNotSaved();

        return true;
    }

    public BinaryTag Write(bool keepDirty = false)
    {
        CompoundTag tag = new();
        tag.Set("size", Size);

        ListTag blocksTag = new(BinaryTagType.Compound);
        tag.Set("blocks", blocksTag);

        foreach(var position in Size.GetPositionsFromZero(false))
        {
            var blockState = GetBlockState(position);
            var blockEntity = GetBlockEntity(position);

            CompoundTag blockTag = new();
            blocksTag.Add(blockTag);

            blockTag.Set("state", blockState);
            if(blockEntity is not null)
                blockTag.Set("entity", blockEntity.Write());
        }

        if(!IsSaved && !keepDirty)
            MarkSaved(SaveMarker.Saved);

        return tag;
    }

    public void Read(BinaryTag tag, bool markSaved = true)
    {
        var compoundTag = tag.To<CompoundTag>();

        var size = Vector3Int.Read(compoundTag.GetTag("size"));
        if(size != Size)
            throw new InvalidOperationException($"read size {size} is not equal to actual size {Size}");

        ListTag blocksTag = compoundTag.GetTag("blocks").To<ListTag>();

        for(int i = 0; i < blocksTag.Count; ++i)
        {
            var blockTag = blocksTag[i].To<CompoundTag>();
            Vector3Int position = GetPositionFromIndex(i);

            BlockState blockState = BlockState.Read(blockTag.GetTag("state"));
            _blockStates[position] = blockState;

            if(blockTag.HasTag<BinaryTag>("entity"))
            {
                var entity = BlockEntity.Read(blockTag.GetTag("entity"));
                entity.Initialize(World, Offset + position);
                SetBlockEntity(position, entity);
            }
            else
            {
                RemoveBlockEntity(position);
            }
        }

        if(markSaved)
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

    protected Vector3Int GetPositionFromIndex(int index)
    {
        int z = index % Size.z;
        index /= Size.z;
        int y = index % Size.y;
        index /= Size.y;
        int x = index;
        return new Vector3Int(x, y, z);
    }

    protected void AssertPosition(Vector3Int position, [CallerArgumentExpression(nameof(position))] string? paramName = null)
    {
        if(position.IsAnyAxis((a, v) => v < 0 || v >= Size.GetAxis(a)))
            throw new ArgumentOutOfRangeException(paramName, position, "Posiiton is out of bounds");
    }
}
