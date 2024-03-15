

namespace VoxelWorld.Blocks.Properties;

public readonly record struct BlockProperties
{
    public static readonly BlockProperties Default = new();

    public bool IsTransparent { get; init; } = false;

    public BlockProperties()
    {

    }
}
