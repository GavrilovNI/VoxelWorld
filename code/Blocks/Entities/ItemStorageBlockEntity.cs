using VoxelWorld.Entities;
using VoxelWorld.Inventories;
using VoxelWorld.IO;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.Menus;
using VoxelWorld.Mth;
using System;

namespace VoxelWorld.Blocks.Entities;

public class ItemStorageBlockEntity : BlockEntity
{
    public ItemStackInventory Capability { get; protected set; }

    protected override bool IsSavedInternal => Capability.IsSaved;

    public ItemStorageBlockEntity(BlockEntityType type, int storageSize,
        int slotLimit = DefaultValues.ItemStackLimit) : base(type)
    {
        if(storageSize < 0)
            throw new ArgumentOutOfRangeException(nameof(storageSize));
        if(slotLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(slotLimit));
        Capability = new ItemStackInventory(storageSize, slotLimit);
    }

    public IMenu CreateMenu(Player player)
    {
        return new ItemStorageBlockMenu(this, player);
    }

    protected override void OnDestroyedInternal()
    {
        BBox boxDropPosition = new BBox(GlobalPosition, GlobalPosition + MathV.UnitsInMeter).Grow(-MathV.UnitsInMeter / 4);
        foreach(var itemStack in Capability)
        {
            EntitySpawnConfig spawnConfig = new(new Transform(boxDropPosition.RandomPointInside), World);
            ItemStackEntity.Create(itemStack, spawnConfig);
        }
    }

    protected override BinaryTag WriteAdditional()
    {
        CompoundTag tag = new();
        tag.Set("capability", Capability);
        return tag;
    }

    protected override void ReadAdditional(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();
        Capability = ItemStackInventory.Read(compoundTag.GetTag("capability"));
    }

    protected override void MarkSavedInternal(IReadOnlySaveMarker saveMarker) => Capability.MarkSaved(saveMarker);
}
