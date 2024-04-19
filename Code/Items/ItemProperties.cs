

namespace VoxelWorld.Items;

public readonly record struct ItemProperties
{
    public static readonly ItemProperties Default = new();

    public float Damage { get; init; } = 1f;
    public float BlockDamage { get; init; } = 10f;
    public float AttackingTime { get; init; } = 0.3f;


    public ItemProperties()
    {

    }
}
