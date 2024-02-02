using Sandbox;
using Sandcube.Interactions;
using Sandcube.Inventories;
using Sandcube.Inventories.Players;
using Sandcube.Mods;
using Sandcube.UI.Menus;
using Sandcube.Worlds;

namespace Sandcube.Players;

public class SandcubePlayer : Component
{
    [Property] public World World { get; private set; } = null!;
    public IPlayerInventory Inventory { get; private set; } = null!; // TODO: Make Property

    protected override void OnEnabled()
    {
        SandcubeGame.Started += OnGameStart;
    }

    protected override void OnDisabled()
    {
        SandcubeGame.Started -= OnGameStart;
    }

    protected override void OnAwake()
    {
        Inventory ??= Components.Get<PlayerInventory>();
    }

    protected virtual void OnGameStart()
    {
        var items = SandcubeBaseMod.Instance!.Items;
        var hotbar = Inventory.Hotbar;
        hotbar.TrySet(0, new(items.Stone));
        hotbar.TrySet(1, new(items.Dirt, 1));
        hotbar.TrySet(2, new(items.Cobblestone, 2));
        hotbar.TrySet(3, new(items.StoneSlab, 9));
        hotbar.TrySet(4, new(items.Glass, 10));
        hotbar.TrySet(5, new(items.WoodLog, 32));
        hotbar.TrySet(6, new(items.Furnace, 63));
        hotbar.TrySet(7, new(items.TallGrass, 64));
    }

    protected override void OnUpdate()
    {
        if(!(MenuController.Instance?.IsAnyOpened ?? false))
            UpdateHandSlot();
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
