using VoxelWorld.Entities;
using VoxelWorld.Inventories;
using VoxelWorld.Items;
using System.Collections.Generic;

namespace VoxelWorld.Menus;

public class CreativeInventoryMenu : ItemCapabilitiesMenu
{
    public CreativeInventoryMenu(IEnumerable<IIndexedCapability<Inventories.Stack<Item>>> playerCapabilities,
        Player player) : base(playerCapabilities, player)
    {
        Capabilities.Insert(0, new CreativeItemStackInventory());
    }

    public override bool IsStillValid() => Player.IsCreative;

    public override bool PlaceStack(IReadOnlyIndexedCapability<Inventories.Stack<Item>> capability, int slotIndex, int maxCount)
    {
        if(capability is CreativeItemStackInventory)
        {
            TakenStack = TakenStack.Subtract(maxCount);
            return true;
        }
        return base.PlaceStack(capability, slotIndex, maxCount);
    }
}
