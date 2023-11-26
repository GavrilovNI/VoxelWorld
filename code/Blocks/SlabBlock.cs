using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Blocks.States.Properties;
using Sandcube.Interactions;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Worlds;
using Sandcube.Worlds.Generation;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sandcube.Blocks;

public class SlabBlock : SimpleBlock
{
    protected readonly Vector3[] CenterPositions = new Vector3[4]
    {
        (FullCubeCorners[0] + FullCubeCorners[4]) / 2f,
        (FullCubeCorners[1] + FullCubeCorners[5]) / 2f,
        (FullCubeCorners[2] + FullCubeCorners[6]) / 2f,
        (FullCubeCorners[3] + FullCubeCorners[7]) / 2f,
    };

    public static BlockProperty<SlabType> SlabTypeProperty = new("type", SlabType.Bottom);


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

    public override VoxelMesh CreateMesh(BlockState blockState)
    {
        var slabType = blockState.GetValue(SlabTypeProperty);

        if(slabType == SlabType.Double)
            return base.CreateMesh(blockState);

        var uv = SandcubeGame.Instance!.TextureMap.GetUv(TextureRect);
        if(slabType == SlabType.Bottom)
        {
            var sideUv = new Rect(uv.Left, uv.Top + uv.Height / 2f, uv.Width, uv.Height / 2f);

            VoxelMesh voxelMesh = new();
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(CenterPositions[3], FullCubeCorners[3], FullCubeCorners[0], CenterPositions[0], Vector3.Backward, VoxelMeshBuilder.TangentUp, sideUv), Direction.Backward);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(CenterPositions[1], FullCubeCorners[1], FullCubeCorners[2], CenterPositions[2], Vector3.Forward, VoxelMeshBuilder.TangentUp, sideUv), Direction.Forward);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(CenterPositions[2], FullCubeCorners[2], FullCubeCorners[3], CenterPositions[3], Vector3.Left, VoxelMeshBuilder.TangentUp, sideUv), Direction.Left);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(CenterPositions[0], FullCubeCorners[0], FullCubeCorners[1], CenterPositions[1], Vector3.Right, VoxelMeshBuilder.TangentUp, sideUv), Direction.Right);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(CenterPositions[2], CenterPositions[3], CenterPositions[0], CenterPositions[1], Vector3.Up, VoxelMeshBuilder.TangentForward, uv));
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[3], FullCubeCorners[2], FullCubeCorners[1], FullCubeCorners[0], Vector3.Down, VoxelMeshBuilder.TangentForward, uv), Direction.Down);
            return voxelMesh;
        }
        else
        {
            var sideUv = new Rect(uv.Left, uv.Top, uv.Width, uv.Height / 2f);
            VoxelMesh voxelMesh = new();
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[7], CenterPositions[3], CenterPositions[0], FullCubeCorners[4], Vector3.Backward, VoxelMeshBuilder.TangentUp, sideUv), Direction.Backward);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[5], CenterPositions[1], CenterPositions[2], FullCubeCorners[6], Vector3.Forward, VoxelMeshBuilder.TangentUp, sideUv), Direction.Forward);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[6], CenterPositions[2], CenterPositions[3], FullCubeCorners[7], Vector3.Left, VoxelMeshBuilder.TangentUp, sideUv), Direction.Left);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[4], CenterPositions[0], CenterPositions[1], FullCubeCorners[5], Vector3.Right, VoxelMeshBuilder.TangentUp, sideUv), Direction.Right);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(FullCubeCorners[6], FullCubeCorners[7], FullCubeCorners[4], FullCubeCorners[5], Vector3.Up, VoxelMeshBuilder.TangentForward, uv), Direction.Up);
            voxelMesh.AddElement(new VoxelMeshBuilder().AddQuad(CenterPositions[3], CenterPositions[2], CenterPositions[1], CenterPositions[0], Vector3.Down, VoxelMeshBuilder.TangentForward, uv));
            return voxelMesh;
        }
    }
}
