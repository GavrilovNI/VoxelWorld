using Sandbox;
using Sandcube.Entities;
using Sandcube.Inventories;
using Sandcube.Items;
using Sandcube.Worlds;

namespace Sandcube.Players;

public class ItemDropper : Component, IWorldInitializable
{
    [Property] protected float Velocity { get; set; } = 100;

    protected IWorldAccessor World { get; set; } = null!;

    public ItemStackEntity Drop(Stack<Item> itemStack)
    {
        EntitySpawnConfig config = new(new(Transform.Position), World);
        return ItemStackEntity.Create(itemStack, config, Transform.Rotation.Forward * Velocity);
    }

    public void InitializeWorld(IWorldAccessor world) => World = world;
}
