using Sandcube.Entities;
using Sandcube.Inventories;
using Sandcube.IO;
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

    public ItemStorageBlockEntity(IWorldAccessor world, Vector3Int position, int storageSize, int slotLimit = DefaultValues.ItemStackLimit) : base(world, position)
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
        // TODO: drop items;
    }

    protected override void WriteAdditional(BinaryWriter writer)
    {
        writer.Write(Capability);
    }

    protected override void ReadAdditional(BinaryReader reader)
    {
        Capability = ItemStackInventory.Read(reader);
    }

    protected override void MarkSavedInternal(IReadOnlySaveMarker saveMarker) => Capability.MarkSaved(saveMarker);
}
