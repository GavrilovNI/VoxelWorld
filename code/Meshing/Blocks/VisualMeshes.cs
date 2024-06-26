﻿using Sandbox;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using System.Collections.Generic;

namespace VoxelWorld.Meshing.Blocks;

public static class VisualMeshes
{
    public static readonly SidedMesh<ComplexVertex> Empty = new();

    public static readonly Vector3[] DoorCorners = PhysicsMeshes.DoorCorners;

    public static readonly AllSidedMeshMaker FullBlock = new(
        (Rect backUv, Rect frontUv, Rect leftUv, Rect rightUv, Rect topUv, Rect bottomUv) =>
        {
            return new SidedMesh<ComplexVertex>.Builder()
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[7], MathV.MeterCubeCorners[3], MathV.MeterCubeCorners[0], MathV.MeterCubeCorners[4], Vector3.Backward, ComplexMeshBuilder.TangentUp, backUv), Direction.Backward)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[5], MathV.MeterCubeCorners[1], MathV.MeterCubeCorners[2], MathV.MeterCubeCorners[6], Vector3.Forward, ComplexMeshBuilder.TangentUp, frontUv), Direction.Forward)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[6], MathV.MeterCubeCorners[2], MathV.MeterCubeCorners[3], MathV.MeterCubeCorners[7], Vector3.Left, ComplexMeshBuilder.TangentUp, leftUv), Direction.Left)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[4], MathV.MeterCubeCorners[0], MathV.MeterCubeCorners[1], MathV.MeterCubeCorners[5], Vector3.Right, ComplexMeshBuilder.TangentUp, rightUv), Direction.Right)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[6], MathV.MeterCubeCorners[7], MathV.MeterCubeCorners[4], MathV.MeterCubeCorners[5], Vector3.Up, ComplexMeshBuilder.TangentForward, topUv), Direction.Up)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[3], MathV.MeterCubeCorners[2], MathV.MeterCubeCorners[1], MathV.MeterCubeCorners[0], Vector3.Down, ComplexMeshBuilder.TangentForward, bottomUv), Direction.Down)
                .Build();
        }
    );

    public static readonly AllSidedMeshMaker BottomSlab = new(
        (Rect backUv, Rect frontUv, Rect leftUv, Rect rightUv, Rect topUv, Rect bottomUv) =>
        {
            return new SidedMesh<ComplexVertex>.Builder()
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeTopDownCenters[3], MathV.MeterCubeCorners[3], MathV.MeterCubeCorners[0], MathV.MeterCubeTopDownCenters[0], Vector3.Backward, ComplexMeshBuilder.TangentUp, backUv), Direction.Backward)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeTopDownCenters[1], MathV.MeterCubeCorners[1], MathV.MeterCubeCorners[2], MathV.MeterCubeTopDownCenters[2], Vector3.Forward, ComplexMeshBuilder.TangentUp, frontUv), Direction.Forward)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeTopDownCenters[2], MathV.MeterCubeCorners[2], MathV.MeterCubeCorners[3], MathV.MeterCubeTopDownCenters[3], Vector3.Left, ComplexMeshBuilder.TangentUp, leftUv), Direction.Left)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeTopDownCenters[0], MathV.MeterCubeCorners[0], MathV.MeterCubeCorners[1], MathV.MeterCubeTopDownCenters[1], Vector3.Right, ComplexMeshBuilder.TangentUp, rightUv), Direction.Right)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeTopDownCenters[2], MathV.MeterCubeTopDownCenters[3], MathV.MeterCubeTopDownCenters[0], MathV.MeterCubeTopDownCenters[1], Vector3.Up, ComplexMeshBuilder.TangentForward, topUv))
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[3], MathV.MeterCubeCorners[2], MathV.MeterCubeCorners[1], MathV.MeterCubeCorners[0], Vector3.Down, ComplexMeshBuilder.TangentForward, bottomUv), Direction.Down)
                .Build();
        }
    );

    public static readonly AllSidedMeshMaker TopSlab = new(
        (Rect backUv, Rect frontUv, Rect leftUv, Rect rightUv, Rect topUv, Rect bottomUv) =>
        {
            return new SidedMesh<ComplexVertex>.Builder()
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[7], MathV.MeterCubeTopDownCenters[3], MathV.MeterCubeTopDownCenters[0], MathV.MeterCubeCorners[4], Vector3.Backward, ComplexMeshBuilder.TangentUp, backUv), Direction.Backward)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[5], MathV.MeterCubeTopDownCenters[1], MathV.MeterCubeTopDownCenters[2], MathV.MeterCubeCorners[6], Vector3.Forward, ComplexMeshBuilder.TangentUp, frontUv), Direction.Forward)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[6], MathV.MeterCubeTopDownCenters[2], MathV.MeterCubeTopDownCenters[3], MathV.MeterCubeCorners[7], Vector3.Left, ComplexMeshBuilder.TangentUp, leftUv), Direction.Left)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[4], MathV.MeterCubeTopDownCenters[0], MathV.MeterCubeTopDownCenters[1], MathV.MeterCubeCorners[5], Vector3.Right, ComplexMeshBuilder.TangentUp, rightUv), Direction.Right)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[6], MathV.MeterCubeCorners[7], MathV.MeterCubeCorners[4], MathV.MeterCubeCorners[5], Vector3.Up, ComplexMeshBuilder.TangentForward, topUv), Direction.Up)
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeTopDownCenters[3], MathV.MeterCubeTopDownCenters[2], MathV.MeterCubeTopDownCenters[1], MathV.MeterCubeTopDownCenters[0], Vector3.Down, ComplexMeshBuilder.TangentForward, bottomUv))
                .Build();
        }
    );

    public static SidedMesh<ComplexVertex> XShaped(Rect uv) =>
        new SidedMesh<ComplexVertex>.Builder()
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[6], MathV.MeterCubeCorners[2], MathV.MeterCubeCorners[0], MathV.MeterCubeCorners[4], new Vector3(-1, 1).Normal, ComplexMeshBuilder.TangentUp, uv))
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[4], MathV.MeterCubeCorners[0], MathV.MeterCubeCorners[2], MathV.MeterCubeCorners[6], new Vector3(1, -1).Normal, ComplexMeshBuilder.TangentUp, uv))
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[7], MathV.MeterCubeCorners[3], MathV.MeterCubeCorners[1], MathV.MeterCubeCorners[5], new Vector3(-1, -1).Normal, ComplexMeshBuilder.TangentUp, uv))
                .Add(new ComplexMeshBuilder().AddQuad(MathV.MeterCubeCorners[5], MathV.MeterCubeCorners[1], MathV.MeterCubeCorners[3], MathV.MeterCubeCorners[7], new Vector3(1, 1).Normal, ComplexMeshBuilder.TangentUp, uv))
                .Build();

    public static readonly AllSidedMeshMaker BottomDoorBlock = new(
        (Rect backUv, Rect frontUv, Rect leftUv, Rect rightUv, Rect topUv, Rect bottomUv) =>
        {
            return new SidedMesh<ComplexVertex>.Builder()
                .Add(new ComplexMeshBuilder().AddQuad(DoorCorners[7], DoorCorners[3], DoorCorners[0], DoorCorners[4], Vector3.Backward, ComplexMeshBuilder.TangentUp, backUv), Direction.Backward)
                .Add(new ComplexMeshBuilder().AddQuad(DoorCorners[5], DoorCorners[1], DoorCorners[2], DoorCorners[6], Vector3.Forward, ComplexMeshBuilder.TangentUp, frontUv))
                .Add(new ComplexMeshBuilder().AddQuad(DoorCorners[6], DoorCorners[2], DoorCorners[3], DoorCorners[7], Vector3.Left, ComplexMeshBuilder.TangentUp, leftUv), Direction.Left)
                .Add(new ComplexMeshBuilder().AddQuad(DoorCorners[4], DoorCorners[0], DoorCorners[1], DoorCorners[5], Vector3.Right, ComplexMeshBuilder.TangentUp, rightUv), Direction.Right)
                .Add(new ComplexMeshBuilder().AddQuad(DoorCorners[3], DoorCorners[2], DoorCorners[1], DoorCorners[0], Vector3.Down, ComplexMeshBuilder.TangentForward, bottomUv), Direction.Down)
                .Build();
        }
    );

    public static readonly AllSidedMeshMaker TopDoorBlock = new(
        (Rect backUv, Rect frontUv, Rect leftUv, Rect rightUv, Rect topUv, Rect bottomUv) =>
        {
            return new SidedMesh<ComplexVertex>.Builder()
                .Add(new ComplexMeshBuilder().AddQuad(DoorCorners[7], DoorCorners[3], DoorCorners[0], DoorCorners[4], Vector3.Backward, ComplexMeshBuilder.TangentUp, backUv), Direction.Backward)
                .Add(new ComplexMeshBuilder().AddQuad(DoorCorners[5], DoorCorners[1], DoorCorners[2], DoorCorners[6], Vector3.Forward, ComplexMeshBuilder.TangentUp, frontUv))
                .Add(new ComplexMeshBuilder().AddQuad(DoorCorners[6], DoorCorners[2], DoorCorners[3], DoorCorners[7], Vector3.Left, ComplexMeshBuilder.TangentUp, leftUv), Direction.Left)
                .Add(new ComplexMeshBuilder().AddQuad(DoorCorners[4], DoorCorners[0], DoorCorners[1], DoorCorners[5], Vector3.Right, ComplexMeshBuilder.TangentUp, rightUv), Direction.Right)
                .Add(new ComplexMeshBuilder().AddQuad(DoorCorners[6], DoorCorners[7], DoorCorners[4], DoorCorners[5], Vector3.Up, ComplexMeshBuilder.TangentForward, topUv), Direction.Up)
                .Build();
        }
    );


    public class AllSidedMeshMaker
    {
        public delegate SidedMesh<ComplexVertex> MakeDelegate(Rect backUv, Rect frontUv, Rect leftUv, Rect rightUv, Rect topUv, Rect bottomUv);
        private readonly MakeDelegate _maker;

        public AllSidedMeshMaker(MakeDelegate maker)
        {
            _maker = maker;
        }

        public SidedMesh<ComplexVertex> Make(Rect backUv, Rect frontUv, Rect leftUv, Rect rightUv, Rect topUv, Rect bottomUv) =>
            _maker(backUv, frontUv, leftUv, rightUv, topUv, bottomUv);

        public SidedMesh<ComplexVertex> Make(IReadOnlyDictionary<Direction, Rect> uvs) =>
            _maker(uvs[Direction.Backward], uvs[Direction.Forward], uvs[Direction.Left],
                uvs[Direction.Right], uvs[Direction.Up], uvs[Direction.Down]);

        public SidedMesh<ComplexVertex> Make(Rect uv) => Make(uv, uv, uv, uv, uv, uv);

        public SidedMesh<ComplexVertex> Make(Rect sideUv, Rect topUv, Rect bottomUv) =>
            _maker(sideUv, sideUv, sideUv, sideUv, topUv, bottomUv);
        
        public SidedMesh<ComplexVertex> Make(Rect sideUv, Rect topBottomUv) =>
            _maker(sideUv, sideUv, sideUv, sideUv, topBottomUv, topBottomUv);

        public SidedMesh<ComplexVertex> Make(Dictionary<Direction, Rect> uvs) => Make(
            uvs.GetValueOrDefault(Direction.Backward, ComplexMeshBuilder.UvFull),
            uvs.GetValueOrDefault(Direction.Forward, ComplexMeshBuilder.UvFull),
            uvs.GetValueOrDefault(Direction.Left, ComplexMeshBuilder.UvFull),
            uvs.GetValueOrDefault(Direction.Right, ComplexMeshBuilder.UvFull),
            uvs.GetValueOrDefault(Direction.Up, ComplexMeshBuilder.UvFull),
            uvs.GetValueOrDefault(Direction.Down, ComplexMeshBuilder.UvFull));
    }

}
