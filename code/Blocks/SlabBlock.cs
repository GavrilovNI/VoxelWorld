using Sandbox;
using VoxelWorld.Blocks.States;
using VoxelWorld.Blocks.States.Properties;
using VoxelWorld.Blocks.States.Properties.Enums;
using VoxelWorld.Interactions;
using VoxelWorld.Meshing;
using VoxelWorld.Meshing.Blocks;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Registries;
using VoxelWorld.Texturing;
using VoxelWorld.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoxelWorld.Blocks;

public class SlabBlock : SimpleBlock
{
    public static readonly BlockProperty<SlabType> SlabTypeProperty = new((Id)"type");

    public SlabBlock(in ModedId id, IUvProvider fullBlockUvProvider) :
        base(id, fullBlockUvProvider)
    {
    }

    public SlabBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> fullBlockUvProviders) :
        base(id, fullBlockUvProviders)
    {
    }

    public override IEnumerable<BlockStateProperty> CombineProperties() => new BlockStateProperty[] { SlabTypeProperty };

    public override bool HidesNeighbourFace(BlockState blockState, BlockMeshType meshType, Direction directionToFace)
    {
        if(meshType == BlockMeshType.Visual && Properties.IsTransparent)
            return false;

        if(blockState.GetValue(SlabTypeProperty) == SlabType.Double)
            return true;

        if(directionToFace == Direction.Down)
            return blockState.GetValue(SlabTypeProperty) == SlabType.Bottom;

        if(directionToFace == Direction.Up)
            return blockState.GetValue(SlabTypeProperty) == SlabType.Top;

        return false;
    }

    public override BlockState CreateDefaultBlockState(BlockState blockState) => blockState.With(SlabTypeProperty, SlabType.Bottom);

    public override bool CanBeReplaced(BlockState currentBlockState, BlockState placingBlockState)
    {
        if(placingBlockState.Block != this)
            return false;

        var currentSlabType = currentBlockState.GetValue(SlabTypeProperty);
        if(currentSlabType == SlabType.Double)
            return false;

        var newSlabType = placingBlockState.GetValue(SlabTypeProperty);
        return newSlabType == SlabType.Double;
    }

    protected virtual SlabType GetSlabPartByHeight(IWorldProvider world, Vector3IntB blockPosition, Vector3 hitPosition, SlabType slabTypeAtCenter)
    {
        var blockLocalZPosition = hitPosition.z - world.GetBlockGlobalPosition(blockPosition).z;

        const float halfBlockHeight = MathV.UnitsInMeter / 2f;

        if(blockLocalZPosition.AlmostEqual(halfBlockHeight, 0.1f))
            return slabTypeAtCenter;

        bool isBottom = blockLocalZPosition < halfBlockHeight;
        return isBottom ? SlabType.Bottom : SlabType.Top;
    }

    public override BlockState GetStateForPlacement(in BlockActionContext context)
    {
        var currentBlockState = context.BlockState;

        if(currentBlockState.Block == this)
        {
            var currentSlabType = currentBlockState.GetValue(SlabTypeProperty);

            if(currentSlabType != SlabType.Double)
            {
                var hitNormal = context.TraceResult.Normal;
                bool placingSecondPart = hitNormal.AlmostEqual(Vector3.Up) && currentSlabType == SlabType.Bottom ||
                        hitNormal.AlmostEqual(Vector3.Down) && currentSlabType == SlabType.Top;

                if(placingSecondPart)
                    return DefaultBlockState.With(SlabTypeProperty, SlabType.Double);
            }
        }

        var slabType = GetSlabPartByHeight(context.World, context.Position, context.TraceResult.HitPosition, SlabType.Bottom);

        if(currentBlockState.Block == this)
            slabType = slabType.Combine(currentBlockState.GetValue(SlabTypeProperty));

        return DefaultBlockState.With(SlabTypeProperty, slabType);
    }

    public override async Task Break(BlockActionContext context)
    {
        var currentSlabType = context.BlockState.GetValue(SlabTypeProperty);
        if(currentSlabType != SlabType.Double)
        {
            await base.Break(context);
            return;
        }

        var slabType = GetSlabPartByHeight(context.World, context.Position, context.TraceResult.HitPosition, SlabType.Bottom);
        await context.World.SetBlockState(context.Position, DefaultBlockState.With(SlabTypeProperty, slabType.GetOpposite()));
    }

    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState)
    {
        var slabType = blockState.GetValue(SlabTypeProperty);

        if(slabType == SlabType.Double)
            return base.CreateVisualMesh(blockState);

        Func<Rect, Rect> uvSideModifier;
        VisualMeshes.AllSidedMeshMaker meshMaker;
        if(slabType == SlabType.Bottom)
        {
            uvSideModifier = uv => new Rect(uv.Left, uv.Top + uv.Height / 2f, uv.Width, uv.Height / 2f);
            meshMaker = VisualMeshes.BottomSlab;
        }
        else
        {
            uvSideModifier = uv => new Rect(uv.Left, uv.Top, uv.Width, uv.Height / 2f);
            meshMaker = VisualMeshes.TopSlab;
        }

        Dictionary<Direction, Rect> uvs = UvProviders.ToDictionary(e => e.Key, e =>
        {
            var uv = e.Value.Uv;
            if(Direction.HorizontalSet.Contains(e.Key))
                uv = uvSideModifier(uv);
            return uv;
        });

        return meshMaker.Make(uvs);
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

    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState)
    {
        var slabType = blockState.GetValue(SlabTypeProperty);

        if(slabType == SlabType.Bottom)
            return PhysicsMeshes.BottomSlab;

        if(slabType == SlabType.Top)
            return PhysicsMeshes.TopSlab;

        return PhysicsMeshes.FullBlock;
    }
}
