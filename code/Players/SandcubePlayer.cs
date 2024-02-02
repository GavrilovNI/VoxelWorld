using Sandbox;
using Sandcube.Interactions;
using Sandcube.Inventories.Players;
using Sandcube.Mods;
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
        Inventory.TrySetHotbarItem(0, new(items.Stone));
        Inventory.TrySetHotbarItem(1, new(items.Dirt, 1));
        Inventory.TrySetHotbarItem(2, new(items.Cobblestone, 2));
        Inventory.TrySetHotbarItem(3, new(items.StoneSlab, 9));
        Inventory.TrySetHotbarItem(4, new(items.Glass, 10));
        Inventory.TrySetHotbarItem(5, new(items.WoodLog, 32));
        Inventory.TrySetHotbarItem(6, new(items.Furnace, 63));
        Inventory.TrySetHotbarItem(7, new(items.TallGrass, 64));
    }

    protected override void OnUpdate()
    {
        UpdateHandSlot();
    }

    protected virtual void UpdateHandSlot()
    {
        if(Inventory is null)
            return;

        if(Inventory.HotbarSize != 0)
        {
            for(int i = 0; i < Inventory.HotbarSize; ++i)
            {
                if(Input.Pressed($"Slot{i + 1}"))
                    Inventory.MainHandIndex = i;
            }

            int slotMoveDelta = (Input.Pressed("SlotPrev") ? -1 : 0) + (Input.Pressed("SlotNext") ? 1 : 0);
            slotMoveDelta -= (int)Input.MouseWheel.y;
            var newIndex = Inventory.MainHandIndex + slotMoveDelta;
            Inventory.MainHandIndex = (newIndex % Inventory.HotbarSize + Inventory.HotbarSize) % Inventory.HotbarSize;
        }

        if(Input.Pressed("HandSwap"))
        {
            var mainHandStack = Inventory.GetHandItem(HandType.Main);
            var secondaryHandStack = Inventory.GetHandItem(HandType.Secondary);

            if(Inventory.CanSetHandItem(HandType.Main, secondaryHandStack) &&
                Inventory.CanSetHandItem(HandType.Secondary, mainHandStack))
            {
                Inventory.TrySetHandItem(HandType.Main, secondaryHandStack);
                Inventory.TrySetHandItem(HandType.Secondary, mainHandStack);
            }
        }
    }
}
