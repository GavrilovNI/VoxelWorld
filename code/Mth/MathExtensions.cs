using System;

namespace VoxelWorld.Mth;
public static class MathExtensions
{
    public static float Round(this float value) => MathF.Round(value);
    public static int RoundToInt(this float value) => (int)value.Round();
}
