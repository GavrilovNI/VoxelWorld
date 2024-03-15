using VoxelWorld.Blocks.States;
using VoxelWorld.Mth.Enums;

namespace VoxelWorld.Blocks.Interfaces;

public interface IRotatableBlock
{
    bool TryRotate(BlockState blockState, RightAngle rightAngle, Direction lookDirection, out BlockState result);
}
