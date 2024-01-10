using Sandcube.Mth;
using Sandcube.Mth.Enums;

namespace Sandcube.Worlds.Generation.Meshes;

public class PhysicsMeshes
{
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
}
