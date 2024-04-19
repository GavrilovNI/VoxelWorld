using Sandbox;
using VoxelWorld.Entities;
using VoxelWorld.Inventories;
using VoxelWorld.Items;

namespace VoxelWorld.Interactions;

public class EntityDamageInfo : DamageInfo
{
    public new Entity Attacker { get; set; }
    public new Stack<Item> Weapon { get; set; }

    public EntityDamageInfo(float damage, Entity attacker, Stack<Item> weapon) : base(damage, attacker.GameObject, null)
    {
        Attacker = attacker;
        Weapon = weapon;
    }
}
