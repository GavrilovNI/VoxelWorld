using Sandbox;
using Sandcube.Events;
using Sandcube.Interactions;
using Sandcube.Inventories;
using Sandcube.Inventories.Players;
using Sandcube.Worlds;

namespace Sandcube.Players;

public class SandcubePlayer : BaseComponent
{
    [Property] public World World { get; private set; } = null!;
    public IPlayerInventory Inventory { get; private set; } = null!; // TODO: Make Property

    public override void OnEnabled()
    {
        Event.Register(this);
    }

    public override void OnDisabled()
    {
        Event.Unregister(this);
    }

    public override void OnAwake()
    {
        Inventory ??= GetComponent<PlayerInventory>();
    }

    [SandcubeEvent.Game.Start]
    protected virtual void OnGameStart()
    {
        var items = SandcubeGame.Instance!.Items;
        Inventory.TrySetHotbarItem(0, new(items.Stone, 1));
        Inventory.TrySetHotbarItem(1, new(items.Dirt, 1));
        Inventory.TrySetHotbarItem(2, new(items.Cobblestone, 1));
        Inventory.TrySetHotbarItem(3, new(items.StoneSlab, 1));
        Inventory.TrySetHotbarItem(4, new(items.Glass, 1));
    }

    public override void Update()
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
            slotMoveDelta -= Input.MouseWheel;
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
