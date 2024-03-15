using Sandbox;
using VoxelWorld.Entities;
using VoxelWorld.Inventories;
using VoxelWorld.Items;
using VoxelWorld.Worlds;

namespace VoxelWorld.Players;

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
