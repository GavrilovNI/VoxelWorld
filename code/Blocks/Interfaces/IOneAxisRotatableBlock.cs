using Sandcube.Blocks.States;
using Sandcube.Mth.Enums;

namespace Sandcube.Blocks.Interfaces;

public interface IOneAxisRotatableBlock : IRotatableBlock
{
    Direction GetRotationLookDirection();

    BlockState Rotate(BlockState blockState, RightAngle rightAngle);

    bool IRotatableBlock.TryRotate(BlockState blockState, RightAngle rightAngle, Direction lookDirection, out BlockState result)
    {
        var defaultLookDirection = GetRotationLookDirection();
        if(lookDirection.Axis != defaultLookDirection.Axis)
        {
            result = blockState;
            return false;
        }
        if(lookDirection.AxisDirection != defaultLookDirection.AxisDirection)
            rightAngle = rightAngle.GetOpposite();

        result = Rotate(blockState, rightAngle);
        return true;
    }
}
