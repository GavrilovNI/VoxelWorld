using System;

namespace VoxelWorld.Mods.Base.Blocks.Data;

public struct BlockBreakingProgress
{
    private float _value = 0f;
    public float Value
    {
        readonly get => _value;
        set => _value = Math.Clamp(value, 0f, 1f);
    }

    public BlockBreakingProgress(float progress = 0f)
    {
        Value = progress;
    }

    public static implicit operator float(BlockBreakingProgress progress) => progress.Value;
    public static implicit operator BlockBreakingProgress(float progress) => new(progress);
}
