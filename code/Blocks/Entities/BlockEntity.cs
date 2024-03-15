using Sandbox;
using VoxelWorld.Blocks.States;
using VoxelWorld.IO;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.Mth;
using VoxelWorld.Registries;
using VoxelWorld.Worlds;
using System;
using System.IO;

namespace VoxelWorld.Blocks.Entities;

public abstract class BlockEntity : IValid, ISaveStatusMarkable, INbtWritable, INbtStaticReadable<BlockEntity>
{
    public BlockEntityType Type { get; }
    public Vector3Int Position { get; private set; }
    public Vector3 GlobalPosition => World.GetBlockGlobalPosition(Position);
    public IWorldAccessor World { get; private set; } = null!;

    public BlockState BlockState => World.GetBlockState(Position);

    public bool Initialized { get; private set; } = false;
    public bool IsValid { get; private set; }

    private IReadOnlySaveMarker _saveMarker = SaveMarker.Saved;
    public bool IsSaved
    {
        get
        {
            if(!_saveMarker.IsSaved)
                return false;

            if(!IsSavedInternal)
                _saveMarker = SaveMarker.NotSaved;

            return _saveMarker.IsSaved;
        }
    }
    protected virtual bool IsSavedInternal => true;

    public BlockEntity(BlockEntityType type)
    {
        Type = type;
        IsValid = true;
        Initialized = false;
    }

    public void Initialize(IWorldAccessor world, Vector3Int position)
    {
        if(Initialized)
            throw new InvalidOperationException($"{GetType().Name} {this} was alread initialized");

        Initialized = true;
        World = world;
        Position = position;
    }

    public virtual void OnCreated()
    {
    }

    public void OnDestroyed()
    {
        IsValid = false;
        OnDestroyedInternal();
    }

    protected virtual void OnDestroyedInternal()
    {
    }

    protected virtual void WriteAdditional(BinaryWriter writer) { }
    protected virtual void ReadAdditional(BinaryReader reader) { }

    public void MarkSaved(IReadOnlySaveMarker saveMarker)
    {
        if(IsSaved)
            return;

        _saveMarker = saveMarker;
        MarkSavedInternal(saveMarker);
    }
    protected virtual void MarkSavedInternal(IReadOnlySaveMarker saveMarker) { }


    protected virtual BinaryTag WriteAdditional() => new CompoundTag();
    protected virtual void ReadAdditional(BinaryTag tag) { }


    public BinaryTag Write()
    {
        CompoundTag tag = new();
        tag.Set("type_id", Type.Id);

        var additionalData = WriteAdditional();
        if(!additionalData.IsDataEmpty)
            tag.Set("data", additionalData);

        return tag;
    }

    public static BlockEntity Read(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();
        var typeId = ModedId.Read(compoundTag.GetTag("type_id"));
        var registry = GameController.Instance!.Registries.GetRegistry<BlockEntityType>();
        var entityType = registry.Get(typeId)!;
        var entity = entityType.CreateBlockEntity();

        if(compoundTag.HasTag("data"))
            entity.ReadAdditional(compoundTag.GetTag("data"));

        return entity;
    }
}
