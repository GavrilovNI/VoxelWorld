using Sandbox;
using Sandcube.Blocks.Interfaces;
using Sandcube.Blocks.States;
using Sandcube.Blocks.States.Properties;
using Sandcube.Blocks.States.Properties.Enums;
using Sandcube.Interactions;
using Sandcube.Meshing;
using Sandcube.Meshing.Blocks;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sandcube.Blocks;

public class DoorBlock : TwoPartBlock, IOneAxisRotatableBlock, IMirrorableBlock
{
    public static readonly FilteredBlockProperty<Direction> DirectionProperty = new("direction", Direction.HorizontalSet.Contains);
    public static readonly BlockProperty<BoolEnum> OpenedProperty = new("opened");
    public static readonly BlockProperty<DoorHingeSide> HingeProperty = new("hinge_side");

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

    public override InteractionResult OnInteract(in BlockActionContext context)
    {
        bool isOpen = context.BlockState.GetValue(OpenedProperty);
        context.World.SetBlockState(context.Position, context.BlockState.With(OpenedProperty, !isOpen));
        return InteractionResult.Success;
    }

    public override Direction GetDirectionToAnotherPart(BlockState blockState) =>
        blockState.GetValue(PartTypeProperty) == TwoPartBlockPartType.First ? Direction.Up : Direction.Down;

    public override bool HidesNeighbourFace(BlockState blockState, BlockMeshType meshType, Direction directionToFace)
    {
        if(meshType == BlockMeshType.Visual && Properties.IsTransparent)
            return false;

        var direction = blockState.GetValue(DirectionProperty);
        return direction == directionToFace;
    }
    public override ISidedMeshPart<ComplexVertex> CreateVisualMesh(BlockState blockState)
    {
        bool isBottom = blockState.GetValue(PartTypeProperty) == TwoPartBlockPartType.First;
        var meshMaker = isBottom ? VisualMeshes.BottomDoorBlock : VisualMeshes.TopDoorBlock;

        var uvProviders = isBottom ? FirstUvProviders : SecondUvProviders;
        var uvs = uvProviders.ToDictionary(e => e.Key, e => e.Value.Uv);

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

            var hingeRotationAngle = isLeftHinge ? RightAngle.Angle270 : RightAngle.Angle90;

            var doorHalfWidthPercent = DefaultValues.DoorWidth / MathV.UnitsInMeter / 2f;
            Vector3 hingePosition;
            if(isLeftHinge)
                hingePosition = new Vector3(MathV.UnitsInMeter * doorHalfWidthPercent, MathV.UnitsInMeter * (1 - doorHalfWidthPercent), MathV.UnitsInMeter / 2f);
            else
                hingePosition = new Vector3(MathV.UnitsInMeter * doorHalfWidthPercent, MathV.UnitsInMeter * doorHalfWidthPercent, MathV.UnitsInMeter / 2f);

            result = result.RotateAround(hingeRotationAngle, lookDirection, hingePosition);
        }

        var direction = blockState.GetValue(DirectionProperty);
        var angle = RightAngle.FromTo(Direction.Backward, direction, lookDirection);
        result = result.RotateAround(angle, lookDirection, MathV.UnitsInMeter / 2f);

        return result;
    }
}
