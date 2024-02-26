using Sandbox;
using Sandcube.Interactions;
using Sandcube.Inventories;
using Sandcube.Inventories.Players;
using Sandcube.Items;
using Sandcube.Menus;
using Sandcube.Mods;
using System.Collections.Generic;

namespace Sandcube.Players;

public class SandcubePlayer : Component
{
    [Property] public bool IsCreative { get; private set; } = false;
    [Property] public float ReachDistance { get; private set; } = 39.37f * 5;

    [Property] public PlayerInventory Inventory { get; private set; } = null!; // TODO: change to IPlayerInventory


    protected override void OnAwake()
    {
        Inventory ??= Components.Get<PlayerInventory>();
    }

    protected override void OnUpdate()
    {
        if(!(MenuController.Instance?.IsAnyOpened ?? false))
            UpdateHandSlot();
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

    protected virtual void UpdateHandSlot()
    {
        if(Inventory is null)
            return;

        var hotbar = Inventory.Hotbar;
        if(hotbar.Size != 0)
        {
            for(int i = 0; i < hotbar.Size; ++i)
            {
                if(Input.Pressed($"Slot{i + 1}"))
                    Inventory.MainHandIndex = i;
            }

            int slotMoveDelta = (Input.Pressed("SlotPrev") ? -1 : 0) + (Input.Pressed("SlotNext") ? 1 : 0);
            slotMoveDelta -= (int)Input.MouseWheel.y;
            var newIndex = Inventory.MainHandIndex + slotMoveDelta;
            Inventory.MainHandIndex = (newIndex % hotbar.Size + hotbar.Size) % hotbar.Size;
        }

        if(Input.Pressed("HandSwap"))
        {
            var mainHandStack = Inventory.GetHandItem(HandType.Main);
            var secondaryHandStack = Inventory.GetHandItem(HandType.Secondary);

            if(Inventory.TrySetHandItem(HandType.Main, secondaryHandStack, true) &&
                Inventory.TrySetHandItem(HandType.Secondary, mainHandStack, true))
            {
                Inventory.TrySetHandItem(HandType.Main, secondaryHandStack);
                Inventory.TrySetHandItem(HandType.Secondary, mainHandStack);
            }
        }
    }
}
