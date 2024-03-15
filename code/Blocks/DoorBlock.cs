using Sandbox;
using VoxelWorld.Blocks.Interfaces;
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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace VoxelWorld.Blocks;

public class DoorBlock : TwoPartBlock, IOneAxisRotatableBlock, IMirrorableBlock
{
    public static readonly FilteredBlockProperty<Direction> DirectionProperty = new((Id)"direction", Direction.HorizontalSet.Contains);
    public static readonly BlockProperty<BoolEnum> OpenedProperty = new((Id)"opened");
    public static readonly BlockProperty<DoorHingeSide> HingeProperty = new((Id)"hinge_side");

    [SetsRequiredMembers]
    public DoorBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> bottomUvProviders,
        IReadOnlyDictionary<Direction, IUvProvider> topUvProviders) : base(id, bottomUvProviders, topUvProviders)
    {
    }

    [SetsRequiredMembers]
    public DoorBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id, uvProviders)
    {
    }

    public override IEnumerable<BlockProperty> CombineProperties() =>
        new BlockProperty[] { PartTypeProperty, DirectionProperty, OpenedProperty, HingeProperty };

    public override BlockState CreateDefaultBlockState(BlockState blockState) =>
        blockState.With(PartTypeProperty, TwoPartBlockPartType.First)
        .With(DirectionProperty, Direction.Backward)
        .With(OpenedProperty, false)
        .With(HingeProperty, DoorHingeSide.Left);

    public override BlockState GetStateForPlacement(in BlockActionContext context)
    {
        var direction = Direction.ClosestTo(-context.TraceResult.Direction.WithAxis(Axis.Z, 0), Direction.Backward);

        var hitNormal = Direction.ClosestTo(context.TraceResult.Normal);
        bool hitSide = hitNormal.Axis != Axis.Z && hitNormal.Axis != direction.Axis;

        bool isLeftHinge;
        if(hitSide)
        {
            isLeftHinge = RightAngle.FromTo(direction, hitNormal, Direction.Down) == RightAngle.Angle270;
        }
        else
        {
            var blockCenter = context.World.GetBlockGlobalPosition(context.Position) + new Vector3(MathV.UnitsInMeter / 2f);
            var deltaFromCenterToHit = context.TraceResult.HitPosition - blockCenter;

            var dot = direction.Rotate(RightAngle.Angle90, Direction.Down).Normal.Dot(deltaFromCenterToHit.WithZ(0));
            isLeftHinge = dot > 0;
        }

        return DefaultBlockState.With(DirectionProperty, direction).With(HingeProperty, isLeftHinge ? DoorHingeSide.Left : DoorHingeSide.Right);
    }

    public override async Task<InteractionResult> OnInteract(BlockActionContext context)
    {
        bool isOpen = context.BlockState.GetValue(OpenedProperty);
        await context.World.SetBlockState(context.Position, context.BlockState.With(OpenedProperty, !isOpen));
        return InteractionResult.Success;
    }

    public override Direction GetDirectionToAnotherPart(BlockState blockState) =>
        blockState.GetValue(PartTypeProperty) == TwoPartBlockPartType.First ? Direction.Up : Direction.Down;

    public override bool HidesNeighbourFace(BlockState blockState, BlockMeshType meshType, Direction directionToFace)
    {
        if(meshType == BlockMeshType.Visual && Properties.IsTransparent)
            return false;

        var direction = blockState.GetValue(DirectionProperty);
        var isOpen = blockState.GetValue(OpenedProperty);
        var hinge = blockState.GetValue(HingeProperty);
        
        if(isOpen)
        {
            var rotation = hinge == DoorHingeSide.Left ? RightAngle.Angle90 : RightAngle.Angle270;
            direction = direction.Rotate(rotation, Direction.Down);
        }

        return direction == directionToFace;
    }
    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState)
    {
        bool isBottom = blockState.GetValue(PartTypeProperty) == TwoPartBlockPartType.First;
        var meshMaker = isBottom ? VisualMeshes.BottomDoorBlock : VisualMeshes.TopDoorBlock;

        bool isLeftHinge = blockState.GetValue(HingeProperty) == DoorHingeSide.Left;
        bool isOpen = blockState.GetValue(OpenedProperty);

        var uvProviders = isBottom ? FirstUvProviders : SecondUvProviders;
        var uvs = uvProviders.ToDictionary(e =>
        {
            var axis = e.Key.Axis;
            if(axis == Axis.Z)
                return e.Key;

            bool shouldSwap = axis == Axis.X ? isOpen : isLeftHinge == isOpen;
            if(shouldSwap)
                return e.Key.GetOpposite();

            return e.Key;
        }, e =>
        {
            bool shouldSwap = e.Key.Axis == Axis.Z ? isLeftHinge == isOpen : !isLeftHinge;

            var result = e.Value.Uv;
            if(shouldSwap)
                (result.Left, result.Right) = (result.Right, result.Left);

            return result;
        });

        var mesh = meshMaker.Make(uvs);
        return RotateMesh(blockState, mesh);
    }

    public Direction GetRotationLookDirection() => Direction.Down;
    public virtual BlockState Rotate(BlockState blockState, RightAngle rightAngle)
    {
        var direction = blockState.GetValue(DirectionProperty).Rotate(rightAngle, GetRotationLookDirection());
        return blockState.With(DirectionProperty, direction);
    }
    public virtual BlockState Mirror(BlockState blockState)
    {
        var direction = blockState.GetValue(DirectionProperty);
        direction = direction.GetOpposite();
        return blockState.With(DirectionProperty, direction);
    }

    public override ISidedMeshPart<Vector3Vertex> CreatePhysicsMesh(BlockState blockState)
    {
        bool isBottom = blockState.GetValue(PartTypeProperty) == TwoPartBlockPartType.First;
        var mesh = isBottom ? PhysicsMeshes.BottomDoorBlock : PhysicsMeshes.TopDoorBlock;
        return RotateMesh(blockState, mesh);
    }

    public override ISidedMeshPart<Vector3Vertex> CreateInteractionMesh(BlockState blockState)
    {
        bool isBottom = blockState.GetValue(PartTypeProperty) == TwoPartBlockPartType.First;
        var mesh = isBottom ? PhysicsMeshes.BottomDoorBlock : PhysicsMeshes.TopDoorBlock;
        return RotateMesh(blockState, mesh);
    }

    protected virtual SidedMesh<V> RotateMesh<V>(BlockState blockState, SidedMesh<V> mesh) where V : unmanaged, IVertex
    {
        var lookDirection = Direction.Down;

        var result = mesh;
        bool isOpen = blockState.GetValue(OpenedProperty);
        if(isOpen)
        {
            bool isLeftHinge = blockState.GetValue(HingeProperty) == DoorHingeSide.Left;
            var hingeRotationAngle = isLeftHinge ? RightAngle.Angle90 : RightAngle.Angle270;
            result = result.RotateAround(hingeRotationAngle, lookDirection, MathV.UnitsInMeter / 2f);
        }

        var direction = blockState.GetValue(DirectionProperty);
        var angle = RightAngle.FromTo(Direction.Backward, direction, lookDirection);
        result = result.RotateAround(angle, lookDirection, MathV.UnitsInMeter / 2f);

        return result;
    }
}
