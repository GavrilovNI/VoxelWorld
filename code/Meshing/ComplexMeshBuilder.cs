using Sandbox;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using System.Collections.Generic;

namespace Sandcube.Meshing;

public class ComplexMeshBuilder : UnlimitedMesh<ComplexVertex>.Builder<ComplexMeshBuilder>
{
    public static readonly Rect UvFull = new(0, 1);
    public static readonly Vector4 TangentForward = new(Vector3.Forward, 1);
    public static readonly Vector4 TangentUp = new(Vector3.Up, 1);

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

    public virtual ComplexMeshBuilder AddCube(Vector3 position, Vector3 size, Rect uv)
    {
        Vector3[] corners = MathV.GetCubeCorners(size);
        AddQuad(corners[3], corners[2], corners[1], corners[0], Vector3.Down, TangentForward, uv); // bottom
        AddQuad(corners[6], corners[7], corners[4], corners[5], Vector3.Up, TangentForward, uv); // top
        AddQuad(corners[4], corners[0], corners[1], corners[5], Vector3.Right, TangentUp, uv); // right
        AddQuad(corners[5], corners[1], corners[2], corners[6], Vector3.Forward, TangentUp, uv); // back
        AddQuad(corners[6], corners[2], corners[3], corners[7], Vector3.Left, TangentUp, uv); // left
        AddQuad(corners[7], corners[3], corners[0], corners[4], Vector3.Backward, TangentUp, uv); // front
        return this;
    }
    public override ComplexMeshBuilder AddCube(Vector3 position, Vector3 size) => AddCube(position, size, UvFull);

    public virtual ComplexMeshBuilder AddCube(Vector3 position, Vector3 size, Rect uv, IReadOnlySet<Direction> sidesToAdd)
    {
        Vector3[] corners = MathV.GetCubeCorners(size);

        if(sidesToAdd.Contains(Direction.Down))
            AddQuad(corners[3], corners[2], corners[1], corners[0], Vector3.Down, TangentForward, uv); // bottom
        if(sidesToAdd.Contains(Direction.Up))
            AddQuad(corners[6], corners[7], corners[4], corners[5], Vector3.Up, TangentForward, uv); // top
        if(sidesToAdd.Contains(Direction.Right))
            AddQuad(corners[4], corners[0], corners[1], corners[5], Vector3.Right, TangentUp, uv); // right
        if(sidesToAdd.Contains(Direction.Forward))
            AddQuad(corners[5], corners[1], corners[2], corners[6], Vector3.Forward, TangentUp, uv); // back
        if(sidesToAdd.Contains(Direction.Left))
            AddQuad(corners[6], corners[2], corners[3], corners[7], Vector3.Left, TangentUp, uv); // left
        if(sidesToAdd.Contains(Direction.Backward))
            AddQuad(corners[7], corners[3], corners[0], corners[4], Vector3.Backward, TangentUp, uv); // front
        return this;
    }
    public override ComplexMeshBuilder AddCube(Vector3 position, Vector3 size, IReadOnlySet<Direction> sidesToAdd) => AddCube(position, size, UvFull, sidesToAdd);

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
    public override T AddCube(Vector3 position, Vector3 size, Rect uv) => (T)base.AddCube(position, size, uv);
    public override T AddCube(Vector3 position, Vector3 size, Rect uv, IReadOnlySet<Direction> sidesToAdd) => (T)base.AddCube(position, size, uv, sidesToAdd);
    public override T AddCube(Vector3 position, Vector3 size, IReadOnlySet<Direction> sidesToAdd) => (T)base.AddCube(position, size, sidesToAdd);

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
