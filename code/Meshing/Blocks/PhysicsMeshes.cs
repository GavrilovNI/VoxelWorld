using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;

namespace VoxelWorld.Meshing.Blocks;

public class PhysicsMeshes
{
    public static readonly Vector3[] DoorCorners = MathV.GetCubeCorners(new Vector3(DefaultValues.DoorWidth, MathV.UnitsInMeter, MathV.UnitsInMeter));

    public static readonly SidedMesh<Vector3Vertex> Empty = new();

    public static readonly SidedMesh<Vector3Vertex> FullBlock = new SidedMesh<Vector3Vertex>.Builder()
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeCorners[7], MathV.MeterCubeCorners[3], MathV.MeterCubeCorners[0], MathV.MeterCubeCorners[4]), Direction.Backward)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeCorners[5], MathV.MeterCubeCorners[1], MathV.MeterCubeCorners[2], MathV.MeterCubeCorners[6]), Direction.Forward)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeCorners[6], MathV.MeterCubeCorners[2], MathV.MeterCubeCorners[3], MathV.MeterCubeCorners[7]), Direction.Left)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeCorners[4], MathV.MeterCubeCorners[0], MathV.MeterCubeCorners[1], MathV.MeterCubeCorners[5]), Direction.Right)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeCorners[6], MathV.MeterCubeCorners[7], MathV.MeterCubeCorners[4], MathV.MeterCubeCorners[5]), Direction.Up)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeCorners[3], MathV.MeterCubeCorners[2], MathV.MeterCubeCorners[1], MathV.MeterCubeCorners[0]), Direction.Down)
        .Build();

    public static readonly SidedMesh<Vector3Vertex> BottomSlab = new SidedMesh<Vector3Vertex>.Builder()
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeTopDownCenters[3], MathV.MeterCubeCorners[3], MathV.MeterCubeCorners[0], MathV.MeterCubeTopDownCenters[0]), Direction.Backward)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeTopDownCenters[1], MathV.MeterCubeCorners[1], MathV.MeterCubeCorners[2], MathV.MeterCubeTopDownCenters[2]), Direction.Forward)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeTopDownCenters[2], MathV.MeterCubeCorners[2], MathV.MeterCubeCorners[3], MathV.MeterCubeTopDownCenters[3]), Direction.Left)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeTopDownCenters[0], MathV.MeterCubeCorners[0], MathV.MeterCubeCorners[1], MathV.MeterCubeTopDownCenters[1]), Direction.Right)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeTopDownCenters[2], MathV.MeterCubeTopDownCenters[3], MathV.MeterCubeTopDownCenters[0], MathV.MeterCubeTopDownCenters[1]))
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeCorners[3], MathV.MeterCubeCorners[2], MathV.MeterCubeCorners[1], MathV.MeterCubeCorners[0]), Direction.Down)
        .Build();


    public static readonly SidedMesh<Vector3Vertex> TopSlab = new SidedMesh<Vector3Vertex>.Builder()
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeCorners[7], MathV.MeterCubeTopDownCenters[3], MathV.MeterCubeTopDownCenters[0], MathV.MeterCubeCorners[4]), Direction.Backward)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeCorners[5], MathV.MeterCubeTopDownCenters[1], MathV.MeterCubeTopDownCenters[2], MathV.MeterCubeCorners[6]), Direction.Forward)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeCorners[6], MathV.MeterCubeTopDownCenters[2], MathV.MeterCubeTopDownCenters[3], MathV.MeterCubeCorners[7]), Direction.Left)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeCorners[4], MathV.MeterCubeTopDownCenters[0], MathV.MeterCubeTopDownCenters[1], MathV.MeterCubeCorners[5]), Direction.Right)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeCorners[6], MathV.MeterCubeCorners[7], MathV.MeterCubeCorners[4], MathV.MeterCubeCorners[5]), Direction.Up)
        .Add(new PositionOnlyMeshBuilder().AddQuad(MathV.MeterCubeTopDownCenters[3], MathV.MeterCubeTopDownCenters[2], MathV.MeterCubeTopDownCenters[1], MathV.MeterCubeTopDownCenters[0]))
        .Build();

    public static readonly SidedMesh<Vector3Vertex> BottomDoorBlock = new SidedMesh<Vector3Vertex>.Builder()
        .Add(new PositionOnlyMeshBuilder().AddQuad(DoorCorners[7], DoorCorners[3], DoorCorners[0], DoorCorners[4]), Direction.Backward)
        .Add(new PositionOnlyMeshBuilder().AddQuad(DoorCorners[5], DoorCorners[1], DoorCorners[2], DoorCorners[6]))
        .Add(new PositionOnlyMeshBuilder().AddQuad(DoorCorners[6], DoorCorners[2], DoorCorners[3], DoorCorners[7]), Direction.Left)
        .Add(new PositionOnlyMeshBuilder().AddQuad(DoorCorners[4], DoorCorners[0], DoorCorners[1], DoorCorners[5]), Direction.Right)
        .Add(new PositionOnlyMeshBuilder().AddQuad(DoorCorners[3], DoorCorners[2], DoorCorners[1], DoorCorners[0]), Direction.Down)
        .Build();

    public static readonly SidedMesh<Vector3Vertex> TopDoorBlock = new SidedMesh<Vector3Vertex>.Builder()
        .Add(new PositionOnlyMeshBuilder().AddQuad(DoorCorners[7], DoorCorners[3], DoorCorners[0], DoorCorners[4]), Direction.Backward)
        .Add(new PositionOnlyMeshBuilder().AddQuad(DoorCorners[5], DoorCorners[1], DoorCorners[2], DoorCorners[6]))
        .Add(new PositionOnlyMeshBuilder().AddQuad(DoorCorners[6], DoorCorners[2], DoorCorners[3], DoorCorners[7]), Direction.Left)
        .Add(new PositionOnlyMeshBuilder().AddQuad(DoorCorners[4], DoorCorners[0], DoorCorners[1], DoorCorners[5]), Direction.Right)
        .Add(new PositionOnlyMeshBuilder().AddQuad(DoorCorners[6], DoorCorners[7], DoorCorners[4], DoorCorners[5]), Direction.Up)
        .Build();
}
