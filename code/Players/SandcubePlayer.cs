using Sandbox;
using Sandcube.Events;
using Sandcube.Inventories.Players;
using Sandcube.Worlds;

namespace Sandcube.Players;

public class SandcubePlayer : BaseComponent
{
    [Property] public World World { get; private set; } = null!;
    [Property] public IPlayerInventory Inventory { get; private set; } = null!;

    public override void OnEnabled()
    {
        Event.Register(this);
    }

    public override void OnDisabled()
    {
        Event.Unregister(this);
    }

    [SandcubeEvent.Game.Start]
    protected virtual void OnGameStart()
    {
        var items = SandcubeGame.Instance!.Items;
        Inventory = new PlayerInventory();
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

        for(int i = 1; i < Inventory.HotbarSize; ++i)
        {
            if(Input.Pressed($"Slot{i}"))
                Inventory.MainHandIndex = i - 1;
        }
    }
}
