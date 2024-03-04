using Sandcube.Blocks.Entities;
using Sandcube.Entities;
using Sandcube.Inventories;
using Sandcube.Inventories.Players;
using Sandcube.Items;
using Sandcube.Mth;
using System.Collections.Generic;

namespace Sandcube.Menus;

public class ItemStorageBlockMenu : ItemCapabilitiesMenu
{
    public ItemStorageBlockEntity BlockEntity { get; }
    public IPlayerInventory PlayerInventory { get; }

    public ItemStorageBlockMenu(ItemStorageBlockEntity blockEntity, Player player) : base(new List<IIndexedCapability<Inventories.Stack<Item>>>()
    {
        blockEntity.Capability,
        player.Inventory.Main,
        player.Inventory.Hotbar
    }, player)
    {
        BlockEntity = blockEntity;
        PlayerInventory = player.Inventory;
    }

    public override bool IsStillValid()
    {
        if(PlayerInventory != Player.Inventory)
            return false;
        if(!BlockEntity.IsValid)
            return false;
        
        var globalPosition = BlockEntity.GlobalPosition;
        var bbox = new BBox(globalPosition, globalPosition + new Vector3(MathV.UnitsInMeter));
        var closestPoint = bbox.ClosestPoint(globalPosition);
        var distanceSquared = closestPoint.DistanceSquared(Player.Transform.Position);
        return distanceSquared <= Player.ReachDistance * Player.ReachDistance;
    }
}
