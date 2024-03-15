using VoxelWorld.Blocks.States;
using System.Text.Json.Serialization;

namespace VoxelWorld.Worlds;

public record struct BlockStateChangingResult
{
    public bool Changed { get; init; }
    private readonly BlockState? _oldBlockState;

    [JsonIgnore]
    public BlockState OldBlockState => _oldBlockState ?? BlockState.Air;

    private BlockStateChangingResult(bool changed, BlockState? oldBlockState)
    {
        Changed = changed;
        _oldBlockState = oldBlockState;
    }

    public static BlockStateChangingResult FromChanged(BlockState oldBlockState) => new(true, oldBlockState);
    public static readonly BlockStateChangingResult NotChanged = new(false, null);

    public static implicit operator bool(BlockStateChangingResult result) => result.Changed;
}
