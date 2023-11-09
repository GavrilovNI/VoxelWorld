

namespace Sandcube.Worlds.Blocks;

public readonly struct BlockState
{
    public readonly Block Block;

    public BlockState(Block block)
    {
        Block = block;
    }

    public readonly bool IsAir() => Block == SandcubeGame.Instance!.Blocks.Air;

    public override string ToString() => $"{nameof(BlockState)}({Block})";
}
