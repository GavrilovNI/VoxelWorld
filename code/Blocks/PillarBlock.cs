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

namespace Sandcube.Blocks;

public class PillarBlock : Block
{
    public static readonly BlockProperty<Axis> AxisProperty = new("axis");

    public required IUvProvider SideUvProvider { get; init; }
    public required IUvProvider TopUvProvider { get; init; }
    public required IUvProvider BottomUvProvider { get; init; }

    [SetsRequiredMembers]
    public PillarBlock(in ModedId id, IUvProvider sideUvProvider, IUvProvider topUvProvider, IUvProvider bottomUvProvider) : base(id)
    {
        SideUvProvider = sideUvProvider;
        TopUvProvider = topUvProvider;
        BottomUvProvider = bottomUvProvider;
    }

    [SetsRequiredMembers]
    public PillarBlock(in ModedId id, IUvProvider sideUvProvider, IUvProvider topBottomUvProvider) :
        this(id, sideUvProvider, sideUvProvider, topBottomUvProvider)
    {
    }

    [SetsRequiredMembers]
    public PillarBlock(in ModedId id, bool useTopTextureForBottom = false, string textureExtension = "png") : base(id)
    {
        var textureMap = SandcubeGame.Instance!.BlocksTextureMap;
        SideUvProvider = textureMap.GetOrLoadTexture($"{BlockPathPart}_side.{textureExtension}");
        TopUvProvider = textureMap.GetOrLoadTexture($"{BlockPathPart}_top.{textureExtension}");
        BottomUvProvider = useTopTextureForBottom ? TopUvProvider :
            textureMap.GetOrLoadTexture($"{BlockPathPart}_bottom.{textureExtension}");
    }

    public override IEnumerable<BlockProperty> CombineProperties() => new BlockProperty[]{ AxisProperty };

    public override BlockState CreateDefaultBlockState(BlockState blockState) => blockState.With(AxisProperty, Axis.Z);

    public override BlockState GetStateForPlacement(BlockActionContext context) =>
        DefaultBlockState.With(AxisProperty, Direction.ClosestTo(context.TraceResult.Normal).Axis);

    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState)
    {
        var axis = blockState.GetValue(AxisProperty);
        var result = VisualMeshes.FullBlock.Make(SideUvProvider.Uv, TopUvProvider.Uv, BottomUvProvider.Uv);
        if(axis == Axis.X)
            result = result.RotateAround(RightAngle.Angle90, Direction.Left, MathV.UnitsInMeter / 2f);
        if(axis == Axis.Y)
            result = result.RotateAround(RightAngle.Angle90, Direction.Forward, MathV.UnitsInMeter / 2f);
        return result;
    }

    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
}
