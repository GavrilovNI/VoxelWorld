using VoxelWorld.Blocks.States;

namespace VoxelWorld.Blocks.Interfaces;

public interface IMirrorableBlock
{
    BlockState Mirror(BlockState blockState);
}
