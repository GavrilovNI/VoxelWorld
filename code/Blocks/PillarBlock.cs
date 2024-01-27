using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Blocks.States.Properties;
using Sandcube.Interactions;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;
using Sandcube.Worlds.Generation.Meshes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sandcube.Blocks;

public class PillarBlock : SimpleBlock
{
    public static readonly BlockProperty<Axis> AxisProperty = new("axis");

    [SetsRequiredMembers]
    public PillarBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id, uvProviders)
    {
    }

    public override IEnumerable<BlockProperty> CombineProperties() => new BlockProperty[]{ AxisProperty };

    public override BlockState CreateDefaultBlockState(BlockState blockState) => blockState.With(AxisProperty, Axis.Z);

    public override BlockState GetStateForPlacement(BlockActionContext context) =>
        DefaultBlockState.With(AxisProperty, Direction.ClosestTo(context.TraceResult.Normal).Axis);

    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState)
    {
        var axis = blockState.GetValue(AxisProperty);
        var result = VisualMeshes.FullBlock.Make(UvProviders.ToDictionary(p => p.Key, p => p.Value.Uv));
        if(axis == Axis.X)
            result = result.RotateAround(RightAngle.Angle90, Direction.Left, MathV.UnitsInMeter / 2f);
        if(axis == Axis.Y)
            result = result.RotateAround(RightAngle.Angle90, Direction.Forward, MathV.UnitsInMeter / 2f);
        return result;
    }

    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
}
