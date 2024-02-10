using Sandcube.Blocks.States;
using Sandcube.Mth.Enums;
using Sandcube.Mth;
using Sandcube.Worlds.Generation.Meshes;
using System;
using Sandcube.Blocks.States.Properties;
using Sandcube.Interactions;
using System.Collections.Generic;
using Sandcube.Registries;
using System.Diagnostics.CodeAnalysis;
using Sandcube.Texturing;
using System.Linq;
using Sandbox;

namespace Sandcube.Blocks;

public class HorizontalDirectionalBlock : SimpleBlock
{
    public static readonly FilteredBlockProperty<Direction> DirectionProperty = new("direction", Direction.HorizontalSet.Contains);

    [SetsRequiredMembers]
    public HorizontalDirectionalBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id, uvProviders)
    {
    }

    public override IEnumerable<BlockProperty> CombineProperties() => new BlockProperty[] { DirectionProperty };

    public override BlockState CreateDefaultBlockState(BlockState blockState) => blockState.With(DirectionProperty, Direction.Forward);

    public override BlockState GetStateForPlacement(in BlockActionContext context)
    {
        var direction = Direction.ClosestTo(-context.TraceResult.Direction.WithAxis(Axis.Z, 0), Direction.Forward);
        return DefaultBlockState.With(DirectionProperty, direction);
    }

    public virtual BlockState Rotate(BlockState blockState, RightAngle rightAngle)
    {
        var direction = blockState.GetValue(DirectionProperty).Rotate(rightAngle, Direction.Down);
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
