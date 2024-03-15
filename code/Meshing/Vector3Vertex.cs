using Sandbox;

namespace VoxelWorld.Meshing;

public struct Vector3Vertex : IVertex
{
    public Vector3 Position;

    public readonly VertexAttribute[] Layout => new VertexAttribute[1]
    {
        new(VertexAttributeType.Position, VertexAttributeFormat.Float32)
    };

    public Vector3Vertex()
    {
    }

    public Vector3Vertex(Vector3 position)
    {
        Position = position;
    }

    public Vector3 GetPosition() => Position;
    public void SetPosition(Vector3 position) => Position = position;

    public static implicit operator Vector3(Vector3Vertex vertex) => vertex.Position;
    public static implicit operator Vector3Vertex(Vector3 position) => new(position);
}
