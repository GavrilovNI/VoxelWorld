using Sandbox;
using Sandcube.Entities;
using Sandcube.Inventories;
using Sandcube.Inventories.Players;
using Sandcube.Items;
using Sandcube.Menus;
using System.Collections.Generic;

namespace Sandcube.Players;

public class Player : Entity
{
    [Property] public bool IsCreative { get; private set; } = false;
    [Property] public float ReachDistance { get; private set; } = 39.37f * 5;
    [Property] public PlayerInventory Inventory { get; private set; } = null!; // TODO: change to IPlayerInventory

    [Property] public ulong SteamId { get; private set; } = 0;

    public void SetSteamId(ulong steamId)
    {
        if(SteamId != 0)
            throw new InvalidOperationException($"steamId was already set");

        SteamId = steamId;
    }

    protected override void OnAwake()
    {
        Tags.Add("player");
    }

    public IMenu CreateInventoryMenu()
    {
        if(IsCreative)
        {
            return new CreativeInventoryMenu(new List<IIndexedCapability<Inventories.Stack<Item>>>()
            {
                Inventory.Hotbar
            });
        }
        else
        {
            return new ItemCapabilitiesMenu(new List<IIndexedCapability<Inventories.Stack<Item>>>()
            {
                Inventory.SecondaryHand,
                Inventory.Main,
                Inventory.Hotbar
            });
        }
    }
}
