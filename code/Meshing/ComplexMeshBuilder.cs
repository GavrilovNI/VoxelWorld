using Sandbox;
using System.Collections.Generic;

namespace VoxelWorld.Meshing;

public class ComplexMeshBuilder : UnlimitedMesh<ComplexVertex>.Builder<ComplexMeshBuilder>
{
    public static readonly Rect UvFull = new(0, 1);
    public static readonly Vector4 TangentForward = new(Vector3.Forward, 1);
    public static readonly Vector4 TangentUp = new(Vector3.Up, 1);

    public virtual ComplexMeshBuilder AddTriangle(Vector3 a, Vector3 b, Vector3 c, Rect uv)
    {
        if(!CanAddToCurrent(4, 6))
            AddNewDataList();

        AddVertex(a, uv.TopLeft);
        AddVertex(b, uv.BottomLeft);
        AddVertex(c, uv.BottomRight);

        AddTriangleIndex(3, 2, 1);

        return this;
    }

    public virtual ComplexMeshBuilder AddTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal, Vector4 tangent, Rect uv)
    {
        if(!CanAddToCurrent(4, 6))
            AddNewDataList();

        var oldNormal = Default.Normal;
        var oldTangent = Default.Tangent;
        Default.Normal = normal;
        Default.Tangent = tangent;

        AddTriangle(a, b, c, uv);

        Default.Normal = oldNormal;
        Default.Tangent = oldTangent;
        return this;
    }

    public virtual ComplexMeshBuilder AddTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal, Vector4 tangent) =>
        AddTriangle(a, b, c, normal, tangent, UvFull);

    public virtual ComplexMeshBuilder AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Rect uv)
    {
        if(!CanAddToCurrent(4, 6))
            AddNewDataList();

        AddVertex(a, uv.TopLeft);
        AddVertex(b, uv.BottomLeft);
        AddVertex(c, uv.BottomRight);
        AddVertex(d, uv.TopRight);

        AddTriangleIndex(4, 3, 2);
        AddTriangleIndex(2, 1, 4);

        return this;
    }

    public virtual ComplexMeshBuilder AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector4 tangent, Rect uv)
    {
        if(!CanAddToCurrent(4, 6))
            AddNewDataList();

        var oldNormal = Default.Normal;
        var oldTangent = Default.Tangent;
        Default.Normal = normal;
        Default.Tangent = tangent;

        AddQuad(a, b, c, d, uv);

        Default.Normal = oldNormal;
        Default.Tangent = oldTangent;
        return this;
    }

    public virtual ComplexMeshBuilder AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector4 tangent) =>
        AddQuad(a, b, c, d, normal, tangent, UvFull);

    public virtual List<VertexBuffer> ToVertexBuffers() => Mesh.ToVertexBuffers();

    protected virtual ComplexMeshBuilder AddVertex(Vector3 position, Vector2 uv)
    {
        var vertex = Default;
        vertex.Position = position;
        vertex.TexCoord0.x = uv.x;
        vertex.TexCoord0.y = uv.y;
        AddVertex(vertex);
        return this;
    }
}

public class ComplexMeshBuilder<T> : ComplexMeshBuilder where T : ComplexMeshBuilder<T>
{
    public override T AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Rect uv) => (T)base.AddQuad(a, b, c, d, uv);
    public override T AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector4 tangent, Rect uv) => (T)base.AddQuad(a, b, c, d, normal, tangent, uv);
    public override T AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector4 tangent) => (T)base.AddQuad(a, b, c, d, normal, tangent);

    public override T AddCube(Vector3 position, Vector3 size) => (T)base.AddCube(position, size);
    
    protected override T AddVertex(Vector3 position, Vector2 uv) => (T)base.AddVertex(position, uv);
}

public static class ComplexMesh
{
    public static List<VertexBuffer> ToVertexBuffers(this UnlimitedMesh<ComplexVertex> complexMesh)
    {
        List<VertexBuffer> buffers = new();

        for(int i = 0; i < complexMesh.PartsCount; ++i)
        {
            var vertices = complexMesh.GetVertices(i);
            var indices = complexMesh.GetIndices(i);

            VertexBuffer buffer = new();
            buffer.Init(true);

            foreach(var vertex in vertices)
                buffer.Add(vertex);

            foreach(var index in indices)
                buffer.AddRawIndex(index);

            buffers.Add(buffer);
        }

        return buffers;
    }
}
