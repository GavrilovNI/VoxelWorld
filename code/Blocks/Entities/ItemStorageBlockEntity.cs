using Sandcube.Inventories;
using Sandcube.Items;
using Sandcube.Menus;
using Sandcube.Mth;
using Sandcube.Players;
using Sandcube.Worlds;
using System;

namespace Sandcube.Blocks.Entities;

public class ItemStorageBlockEntity : BlockEntity
{
    public IIndexedCapability<Inventories.Stack<Item>> Capability { get; }

    public ItemStorageBlockEntity(IWorldProvider world, Vector3Int position, int storageSize, int slotLimit = DefaultValues.ItemStackLimit) : base(world, position)
    {
        if(storageSize < 0)
            throw new ArgumentOutOfRangeException(nameof(storageSize));
        if(slotLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(slotLimit));
        Capability = new ItemStackInventory(storageSize, slotLimit);
    }

    public IMenu CreateMenu(SandcubePlayer player)
    {
        return new ItemStorageBlockMenu(this, player.Inventory);
    }

    protected override void OnDestroyedInternal()
    {
        // TODO: drop items;
    }
}
