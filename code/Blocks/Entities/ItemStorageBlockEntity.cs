using Sandcube.Entities;
using Sandcube.Inventories;
using Sandcube.IO;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.Menus;
using Sandcube.Mth;
using Sandcube.Worlds;
using System;
using System.IO;

namespace Sandcube.Blocks.Entities;

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

    protected override void WriteAdditional(BinaryWriter writer)
    {
        writer.Write(Capability);
    }

    protected override void ReadAdditional(BinaryReader reader)
    {
        Capability = ItemStackInventory.Read(reader);
    }

    protected override BinaryTag WriteAdditional()
    {
        CompoundTag tag = new();
        tag.Set("capability", Capability);
        return tag;
    }

    protected override void ReadAdditional(BinaryTag tag)
    {
        CompoundTag compoundTag = (CompoundTag)tag;
        Capability = ItemStackInventory.Read(compoundTag.GetTag("capability"));
    }

    protected override void MarkSavedInternal(IReadOnlySaveMarker saveMarker) => Capability.MarkSaved(saveMarker);
}
