

namespace Sandcube.Worlds.Generation.Meshes;

public class PositionOnlyMeshBuilder : UnlimitedMesh<Vector3Vertex>.Builder<PositionOnlyMeshBuilder>
{
    protected override PositionOnlyMeshBuilder AddVertex(Vector3 position)
    {
        ExpandBounds(position);
        CurrentVertices!.Add(position);
        return this;
    }
}

public class PositionOnlyMeshBuilder<T> : PositionOnlyMeshBuilder where T : PositionOnlyMeshBuilder<T>
{
    protected override T AddVertex(Vector3 position) => (T)base.AddVertex(position);
}
