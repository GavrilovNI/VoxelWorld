﻿using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Blocks.States.Properties;
using Sandcube.Interactions;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Worlds;
using Sandcube.Worlds.Generation.Meshes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sandcube.Blocks;

public class SlabBlock : SimpleBlock
{
    public static readonly BlockProperty<SlabType> SlabTypeProperty = new("type", SlabType.Bottom);


    [SetsRequiredMembers]
    public SlabBlock(in ModedId id, in Rect textureRect) : base(id, textureRect)
    {
    }

    [SetsRequiredMembers]
    public SlabBlock(in ModedId id, Texture texture) : base(id, texture)
    {
    }

    [SetsRequiredMembers]
    public SlabBlock(in ModedId id) : base(id)
    {
    }

    public override IEnumerable<BlockProperty> CombineProperties() => new BlockProperty[] { SlabTypeProperty };

    public override bool IsFullBlock(BlockState blockState) => blockState.GetValue(SlabTypeProperty) == SlabType.Double;

    public override BlockState CreateDefaultBlockState(BlockState blockState) => blockState.With(SlabTypeProperty, SlabType.Bottom);

    public override bool CanBeReplaced(BlockActionContext context, BlockState placingBlockState)
    {
        if(placingBlockState.Block != this)
            return false;

        var currentSlabType = context.BlockState.GetValue(SlabTypeProperty);
        if(currentSlabType == SlabType.Double)
            return false;

        var wantedType = GetSlabPart(context.World, context.Position, context.TraceResult, currentSlabType.GetOpposite());
        return wantedType != currentSlabType;
    }

    protected virtual SlabType GetSlabPart(IWorldProvider world, Vector3Int blockPosition, PhysicsTraceResult traceResult, SlabType slabTypeAtCenter)
    {
        var blockLocalYPosition = traceResult.HitPosition.z - world.GetBlockGlobalPosition(blockPosition).z;

        const float halfBlockHeight = MathV.InchesInMeter / 2f;

        if(blockLocalYPosition.AlmostEqual(halfBlockHeight, 0.1f))
            return slabTypeAtCenter;

        bool isBottom = blockLocalYPosition < halfBlockHeight;
        return isBottom ? SlabType.Bottom : SlabType.Top;
    }

    public override BlockState GetStateForPlacement(BlockActionContext context)
    {
        var currentBlockState = context.BlockState;
        if(currentBlockState.Block == this)
            return currentBlockState.With(SlabTypeProperty, SlabType.Double);

        var slabType = GetSlabPart(context.World, context.Position, context.TraceResult, SlabType.Bottom);
        return DefaultBlockState.With(SlabTypeProperty, slabType);
    }

    public override void Break(BlockActionContext context)
    {
        var currentSlabType = context.BlockState.GetValue(SlabTypeProperty);
        if(currentSlabType != SlabType.Double)
        {
            base.Break(context);
            return;
        }

        var slabType = GetSlabPart(context.World, context.Position, context.TraceResult, SlabType.Bottom);
        context.World.SetBlockState(context.Position, DefaultBlockState.With(SlabTypeProperty, slabType.GetOpposite()));
    }

    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState)
    {
        var slabType = blockState.GetValue(SlabTypeProperty);

        if(slabType == SlabType.Double)
            return base.CreateVisualMesh(blockState);

        var uv = SandcubeGame.Instance!.TextureMap.GetUv(TextureRect);
        if(slabType == SlabType.Bottom)
        {
            var sideUv = new Rect(uv.Left, uv.Top + uv.Height / 2f, uv.Width, uv.Height / 2f);
            return VisualMeshes.BottomSlab.Make(sideUv, uv);
        }
        else
        {
            var sideUv = new Rect(uv.Left, uv.Top, uv.Width, uv.Height / 2f);
            return VisualMeshes.TopSlab.Make(sideUv, uv);
        }
    }

    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState)
    {
        var slabType = blockState.GetValue(SlabTypeProperty);

        if(slabType == SlabType.Bottom)
            return PhysicsMeshes.BottomSlab;

        if(slabType == SlabType.Top)
            return PhysicsMeshes.TopSlab;

        return PhysicsMeshes.FullBlock;
    }
}
