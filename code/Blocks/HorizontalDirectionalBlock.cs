using VoxelWorld.Blocks.States;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Mth;
using VoxelWorld.Meshing;
using VoxelWorld.Meshing.Blocks;
using VoxelWorld.Blocks.States.Properties;
using VoxelWorld.Interactions;
using System.Collections.Generic;
using VoxelWorld.Registries;
using VoxelWorld.Texturing;
using System.Linq;
using VoxelWorld.Blocks.Interfaces;

namespace VoxelWorld.Blocks;

public class HorizontalDirectionalBlock : SimpleBlock, IOneAxisRotatableBlock, IMirrorableBlock
{
    public static readonly FilteredBlockStateProperty<Direction> DirectionProperty = new((Id)"direction", Direction.HorizontalSet.Contains);

    public HorizontalDirectionalBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id, uvProviders)
    {
    }

    public override IEnumerable<BlockStateProperty> CombineProperties() => new BlockStateProperty[] { DirectionProperty };

    public override BlockState CreateDefaultBlockState(BlockState blockState) => blockState.With(DirectionProperty, Direction.Forward);

    public override BlockState GetStateForPlacement(in BlockActionContext context)
    {
        var direction = Direction.ClosestTo(-context.TraceResult.Direction.WithAxis(Axis.Z, 0), Direction.Forward);
        return DefaultBlockState.With(DirectionProperty, direction);
    }

    public virtual Direction GetRotationLookDirection() => Direction.Down;
    public virtual BlockState Rotate(BlockState blockState, RightAngle rightAngle)
    {
        var direction = blockState.GetValue(DirectionProperty).Rotate(rightAngle, GetRotationLookDirection());
        return blockState.With(DirectionProperty, direction);
    }
    public virtual BlockState Mirror(BlockState blockState)
    {
        var direction = blockState.GetValue(DirectionProperty);
        direction = direction.GetOpposite();
        return blockState.With(DirectionProperty, direction);
    }

    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState)
    {
        var direction = blockState.GetValue(DirectionProperty);
        var result = VisualMeshes.FullBlock.Make(UvProviders.ToDictionary(p => p.Key, p => p.Value.Uv));

        var lookDirection = Direction.Down;
        var angle = RightAngle.FromTo(Direction.Forward, direction, lookDirection);
        return result.RotateAround(angle, lookDirection, MathV.UnitsInMeter / 2f);
    }

    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
}
