using Sandbox;
using Sandcube.Interactions;
using Sandcube.Menus;

namespace Sandcube.Inventories.Players;

public class HotbarController : Component
{
    [Property] public PlayerInventory Inventory { get; private set; } = null!; // TODO: change to IPlayerInventory

    protected override void OnAwake()
    {
        Inventory ??= Components.Get<PlayerInventory>();
    }

    protected override void OnUpdate()
    {
        bool canUse = Inventory is not null && !(MenuController.Instance?.IsAnyOpened ?? false);
        if(canUse)
        {
            ChangeMainHandIndex();

            if(Input.Pressed("HandSwap"))
                SwapHands();
        }
    }

    protected virtual int ChangeMainHandIndex()
    {
        var hotbar = Inventory.Hotbar;
        if(hotbar.Size == 0)
            return Inventory.MainHandIndex;

        for(int i = 0; i < hotbar.Size; ++i)
        {
            if(Input.Pressed($"Slot{i + 1}"))
                Inventory.MainHandIndex = i;
        }

        int slotMoveDelta = (Input.Pressed("SlotPrev") ? -1 : 0) + (Input.Pressed("SlotNext") ? 1 : 0);
        slotMoveDelta -= (int)Input.MouseWheel.y;
        var newIndex = Inventory.MainHandIndex + slotMoveDelta;
        newIndex = (newIndex % hotbar.Size + hotbar.Size) % hotbar.Size;
        Inventory.MainHandIndex = newIndex;
        return newIndex;
    }

    protected virtual bool SwapHands()
    {
        var mainHandStack = Inventory.GetHandItem(HandType.Main);
        var secondaryHandStack = Inventory.GetHandItem(HandType.Secondary);

        bool canSwap = Inventory.TrySetHandItem(HandType.Main, secondaryHandStack, true) &&
            Inventory.TrySetHandItem(HandType.Secondary, mainHandStack, true);

        if(canSwap)
        {
            Inventory.TrySetHandItem(HandType.Main, secondaryHandStack);
            Inventory.TrySetHandItem(HandType.Secondary, mainHandStack);
        }

        return canSwap;
    }
}
