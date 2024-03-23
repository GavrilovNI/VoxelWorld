

namespace VoxelWorld.Mth;

public static class DefaultValues
{
    public const int ItemStackLimit = 64;

    public const float DoorWidth = MathV.UnitsInMeter * 3 / 16;

    public static readonly Vector3IntB ChunkSize = new(16);
    public static readonly Vector3IntB RegionSize = new(4);

    public const float FlatItemPixelSize = MathV.UnitsInMeter / 16f;
    public const float FlatItemThickness = MathV.UnitsInMeter / 16f;

    public const float ItemModelScale = 0.4f;
}
