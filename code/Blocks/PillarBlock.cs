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
    public static readonly BlockProperty<Axis> AxisProperty = new("axis", Axis.Z);

    public required TextureMapPart SideTexturePart { get; init; }
    public required TextureMapPart TopTexturePart { get; init; }
    public required TextureMapPart BottomTexturePart { get; init; }

    [SetsRequiredMembers]
    public PillarBlock(in ModedId id, Texture sideTexture, Texture topTexture, Texture bottomTexture) :
        this(id, SandcubeGame.Instance!.TextureMap.AddTexture(sideTexture),
            SandcubeGame.Instance!.TextureMap.AddTexture(topTexture),
            SandcubeGame.Instance!.TextureMap.AddTexture(bottomTexture))
    {
    }

    [SetsRequiredMembers]
    public PillarBlock(in ModedId id, Texture sideTexture, Texture topBottomTexture) :
        this(id, SandcubeGame.Instance!.TextureMap.AddTexture(sideTexture),
            SandcubeGame.Instance!.TextureMap.AddTexture(topBottomTexture))
    {
    }

    [SetsRequiredMembers]
    public PillarBlock(in ModedId id, TextureMapPart sideTexturePart, TextureMapPart topTexturePart, TextureMapPart bottomTexturePart) : base(id)
    {
        SideTexturePart = sideTexturePart;
        TopTexturePart = topTexturePart;
        BottomTexturePart = bottomTexturePart;
    }

    [SetsRequiredMembers]
    public PillarBlock(in ModedId id, TextureMapPart sideTexturePart, TextureMapPart topBottomTexturePart) :
        this(id, sideTexturePart, topBottomTexturePart, topBottomTexturePart)
    {
    }

    public override IEnumerable<BlockProperty> CombineProperties() => new BlockProperty[]{ AxisProperty };

    public override BlockState CreateDefaultBlockState(BlockState blockState) => blockState.With(AxisProperty, Axis.Z);

    public override BlockState GetStateForPlacement(BlockActionContext context) =>
        DefaultBlockState.With(AxisProperty, Direction.ClosestTo(context.TraceResult.Normal).Axis);

    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState)
    {
        var axis = blockState.GetValue(AxisProperty);
        var result = VisualMeshes.FullBlock.Make(SideTexturePart.Uv, TopTexturePart.Uv, BottomTexturePart.Uv);
        if(axis == Axis.X)
            result = result.Rotate90ClockwiseAround(MathV.InchesInMeter / 2f, Direction.Left);
        if(axis == Axis.Y)
            result = result.Rotate90ClockwiseAround(MathV.InchesInMeter / 2f, Direction.Forward);
        return result;
    }

    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState) => PhysicsMeshes.FullBlock;
}
