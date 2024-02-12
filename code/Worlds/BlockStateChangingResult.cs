using Sandcube.Blocks.States;

namespace Sandcube.Worlds;

public record struct BlockStateChangingResult(bool Changed, BlockState OldBlockState)
{
    public static implicit operator bool(BlockStateChangingResult result) => result.Changed; 
}
