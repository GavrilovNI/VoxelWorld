using Sandcube.Blocks.States;
using Sandcube.Mth.Enums;
using Sandcube.Mth;
using Sandcube.Worlds.Generation.Meshes;
using Sandcube.Blocks.States.Properties;
using Sandcube.Interactions;
using System.Collections.Generic;
using Sandcube.Registries;
using System.Diagnostics.CodeAnalysis;
using Sandcube.Texturing;
using System.Linq;

namespace Sandcube.Blocks;

public class DirectionalBlock : SimpleBlock
{
    public static readonly BlockProperty<Direction> DirectionProperty = new("direction");

    [SetsRequiredMembers]
    public DirectionalBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id, uvProviders)
    {
    }

    public override IEnumerable<BlockProperty> CombineProperties() => new BlockProperty[] { DirectionProperty };

    public override BlockState CreateDefaultBlockState(BlockState blockState) => blockState.With(DirectionProperty, Direction.Forward);

    public override BlockState GetStateForPlacement(BlockActionContext context)
    {
        var direction = Direction.ClosestTo(-context.TraceResult.Direction, Direction.Forward);
        return DefaultBlockState.With(DirectionProperty, direction);
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
