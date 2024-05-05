using Sandbox;

namespace VoxelWorld.Meshing;

public struct ComplexVertex : IVertex
{
    public Vector3 Position;

    public Color32 Color;

    public Vector3 Normal;

    public Vector4 TexCoord0;

    public Vector4 TexCoord1;

    public Vector4 Tangent;

    public readonly VertexAttribute[] Layout => new VertexAttribute[6]
    {
        new(VertexAttributeType.Position, VertexAttributeFormat.Float32),
        new(VertexAttributeType.Color, VertexAttributeFormat.UInt8, 4),
        new(VertexAttributeType.Normal, VertexAttributeFormat.Float32),
        new(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 4),
        new(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 4, 1),
        new(VertexAttributeType.Tangent, VertexAttributeFormat.Float32, 4)
    };

    public ComplexVertex(Vector3 position)
        : this(position, Color32.White)
    {
    }

    public ComplexVertex(Vector3 position, Color32 color)
    {
        this = default;
        Position = position;
        Color = color;
    }

    public ComplexVertex(Vector3 position, Vector4 texCoord0, Color32 color)
    {
        this = default;
        Position = position;
        TexCoord0 = texCoord0;
        Color = color;
    }

    public ComplexVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector4 texCoord0)
    {
        this = default;
        Position = position;
        Normal = normal;
        Tangent = new Vector4(tangent.x, tangent.y, tangent.z, -1f);
        TexCoord0 = texCoord0;
        Color = Color32.White;
    }

    public readonly Vector3 GetPosition() => Position;
    public void SetPosition(Vector3 position) => Position = position;


    public static implicit operator Vertex(ComplexVertex vertex) => new(){
        Position = vertex.Position,
        Color = vertex.Color,
        Normal = vertex.Normal,
        TexCoord0 = vertex.TexCoord0,
        TexCoord1 = vertex.TexCoord1,
        Tangent = vertex.Tangent
    };

    public static implicit operator ComplexVertex(Vertex vertex) => new()
    {
        Position = vertex.Position,
        Color = vertex.Color,
        Normal = vertex.Normal,
        TexCoord0 = vertex.TexCoord0,
        TexCoord1 = vertex.TexCoord1,
        Tangent = vertex.Tangent
    };
}
