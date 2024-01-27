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

namespace Sandcube.Blocks;

public class DirectionalBlock : Block
{
    public static readonly BlockProperty<Direction> DirectionProperty = new("direction");

    public IReadOnlyDictionary<Direction, IUvProvider> UvProviders { get; private set; }

    [SetsRequiredMembers]
    public DirectionalBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id)
    {
        UvProviders = new Dictionary<Direction, IUvProvider>(uvProviders);
    }

    [SetsRequiredMembers]
    public DirectionalBlock(in ModedId id, string textureExtension = "png") : base(id)
    {
        var textureMap = SandcubeGame.Instance!.BlocksTextureMap;
        UvProviders = new Dictionary<Direction, IUvProvider>()
        {
            { Direction.Forward, textureMap.GetOrLoadTexture($"{BlockPathPart}_front.{textureExtension}") },
            { Direction.Backward, textureMap.GetOrLoadTexture($"{BlockPathPart}_back.{textureExtension}") },
            { Direction.Left, textureMap.GetOrLoadTexture($"{BlockPathPart}_left.{textureExtension}") },
            { Direction.Right, textureMap.GetOrLoadTexture($"{BlockPathPart}_right.{textureExtension}") },
            { Direction.Up, textureMap.GetOrLoadTexture($"{BlockPathPart}_top.{textureExtension}") },
            { Direction.Down, textureMap.GetOrLoadTexture($"{BlockPathPart}_bottom.{textureExtension}") }
        };
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
