using Sandcube.Blocks.States;
using Sandcube.Mth.Enums;

namespace Sandcube.Blocks.Interfaces;

public interface IRotatableBlock
{
    bool TryRotate(BlockState blockState, RightAngle rightAngle, Direction lookDirection, out BlockState result);
}
