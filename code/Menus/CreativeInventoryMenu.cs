using Sandcube.Entities;
using Sandcube.Inventories;
using Sandcube.Items;
using System.Collections.Generic;

namespace Sandcube.Menus;

public class CreativeInventoryMenu : ItemCapabilitiesMenu
{
    public CreativeInventoryMenu(IEnumerable<IIndexedCapability<Inventories.Stack<Item>>> playerCapabilities) : base(playerCapabilities)
    {
        Capabilities.Insert(0, new CreativeItemStackInventory());
    }

    public override bool IsStillValid(Player player) => player.IsCreative;

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
