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

public class DirectionalBlock : SimpleBlock, IRotatableBlock, IMirrorableBlock
{
    public static readonly BlockProperty<Direction> DirectionProperty = new((Id)"direction");

    public DirectionalBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id, uvProviders)
    {
    }

    public override IEnumerable<BlockProperty> CombineProperties() => new BlockProperty[] { DirectionProperty };

    public override BlockState CreateDefaultBlockState(BlockState blockState) => blockState.With(DirectionProperty, Direction.Forward);

    public override BlockState GetStateForPlacement(in BlockActionContext context)
    {
        var direction = Direction.ClosestTo(-context.TraceResult.Direction, Direction.Forward);
        return DefaultBlockState.With(DirectionProperty, direction);
    }

    public virtual bool TryRotate(BlockState blockState, RightAngle rightAngle, Direction lookDirection, out BlockState result)
    {
        var direction = blockState.GetValue(DirectionProperty).Rotate(rightAngle, lookDirection);
        result = blockState.With(DirectionProperty, direction);
        return true;
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

        var lookDirection = direction.Axis == Axis.Z ? Direction.Left : Direction.Down;
        var angle = RightAngle.FromTo(Direction.Forward, direction, lookDirection);

        return result.RotateAround(angle, lookDirection, MathV.UnitsInMeter / 2f);
    }

    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
}
