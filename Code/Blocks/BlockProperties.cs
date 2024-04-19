

namespace VoxelWorld.Blocks;

public readonly record struct BlockProperties
{
    public static readonly BlockProperties Default = new();

    public bool IsTransparent { get; init; } = false;
    public string PlaceSound { get; init; } = "sounds/voxelworld/blocks/block_place.sound";
    public string DamageSound { get; init; } = "sounds/voxelworld/blocks/block_damage.sound";
    public string BreakSound { get; init; } = "sounds/voxelworld/blocks/block_break.sound";
    public string FootstepSound { get; init; } = "sounds/voxelworld/blocks/block_footstep.sound";

    public bool IsBreakable { get; init; } = true;
    public float Health { get; init; } = 100f;


    public BlockProperties()
    {

    }
}
