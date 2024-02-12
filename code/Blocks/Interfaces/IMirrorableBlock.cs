using Sandcube.Blocks.States;

namespace Sandcube.Blocks.Interfaces;

public interface IMirrorableBlock
{
    BlockState Mirror(BlockState blockState);
}
