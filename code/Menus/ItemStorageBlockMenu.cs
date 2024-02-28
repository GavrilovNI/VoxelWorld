using Sandcube.Blocks.Entities;
using Sandcube.Inventories;
using Sandcube.Inventories.Players;
using Sandcube.Items;
using Sandcube.Mth;
using Sandcube.Players;
using System.Collections.Generic;

namespace Sandcube.Menus;

public class ItemStorageBlockMenu : ItemCapabilitiesMenu
{
    public ItemStorageBlockEntity BlockEntity { get; }
    public IPlayerInventory PlayerInventory { get; }

    public ItemStorageBlockMenu(ItemStorageBlockEntity blockEntity, IPlayerInventory playerInventory) : base(new List<IIndexedCapability<Inventories.Stack<Item>>>()
    {
        blockEntity.Capability,
        playerInventory.Main,
        playerInventory.Hotbar
    })
    {
        BlockEntity = blockEntity;
        PlayerInventory = playerInventory;
    }

    public override bool IsStillValid(Player player)
    {
        if(PlayerInventory != player.Inventory)
            return false;
        if(!BlockEntity.IsValid)
            return false;
        
        var globalPosition = BlockEntity.GlobalPosition;
        var bbox = new BBox(globalPosition, globalPosition + new Vector3(MathV.UnitsInMeter));
        var closestPoint = bbox.ClosestPoint(globalPosition);
        var distanceSquared = closestPoint.DistanceSquared(player.Transform.Position);
        return distanceSquared <= player.ReachDistance * player.ReachDistance;
    }
}
