

namespace VoxelWorld.Mth;

public static class DefaultValues
{
    public const int ItemStackLimit = 64;

    public const float DoorWidth = MathV.UnitsInMeter * 3 / 16;

    public static readonly Vector3Int ChunkSize = new(16);
    public static readonly Vector3Int RegionSize = new(4);
}
