using Sandbox;
using Sandcube.Mth;
using System.Collections.Generic;

namespace Sandcube.Worlds.Generation;

public class VoxelMeshBuilder
{
    public readonly Vector4 TangentForward = new(Vector3.Forward, 1);
    public readonly Vector4 TangentUp = new(Vector3.Up, 1);

    protected readonly List<List<Vertex>> _vertices = new();
    protected readonly List<List<int>> _indices = new();

    protected List<Vertex>? _currentVertices = null;
    protected List<int>? _currentIndices = null;

    public Vertex Default;


    public VoxelMeshBuilder()
    {
        Clear();
    }

    protected void AddNewDataList()
    {
        _currentVertices = new List<Vertex>();
        _currentIndices = new List<int>();
        _vertices.Add(_currentVertices);
        _indices.Add(_currentIndices);
    }

    protected bool CanAddToCurrent(int vertexCount, int indexCount)
    {
        if(_currentIndices == null || _currentVertices == null)
            return false;
        return _currentVertices.Count + vertexCount <= ushort.MaxValue && _currentIndices.Count + indexCount <= ushort.MaxValue;
    }

    public virtual void Clear()
    {
        _vertices.Clear();
        _indices.Clear();
        _currentVertices = null;
        _currentIndices = null;
        Default = new() { Color = Color.White };
    }

    protected void AddRawIndex(int i) => _currentIndices!.Add(i);
    protected void AddIndex(int i) => AddRawIndex(_currentVertices!.Count - i);

    protected void AddTriangleIndex(int a, int b, int c)
    {
        AddIndex(a);
        AddIndex(b);
        AddIndex(c);
    }


    protected void AddVertex(Vertex vertex) => _currentVertices!.Add(vertex);

    protected void AddVertex(Vector3 position, Vector2 uv)
    {
        var vertex = Default;
        vertex.Position = position;
        vertex.TexCoord0.x = uv.x;
        vertex.TexCoord0.y = uv.y;
        AddVertex(vertex);
    }

    public void AddMeshBuilder(VoxelMeshBuilder builder)
    {
        for(int i = 0; i < builder._vertices.Count; ++i)
        {
            var vertices = builder._vertices[i];
            if(!object.ReferenceEquals(vertices, builder._currentVertices))
                _vertices.Add(new List<Vertex>(vertices));
        }
        for(int i = 0; i < builder._indices.Count; ++i)
        {
            var indices = builder._indices[i];
            if(!object.ReferenceEquals(indices, builder._currentIndices))
                _indices.Add(new List<int>(indices));
        }

        if(builder._currentVertices is null || builder._currentIndices is null)
            return;

        if(CanAddToCurrent(builder._currentVertices.Count, builder._currentIndices.Count))
        {
            int startIndex = _currentVertices!.Count;
            _currentVertices.AddRange(builder._currentVertices);
            foreach(var index in builder._currentIndices)
                AddRawIndex(index + startIndex);
        }
        else
        {
            AddNewDataList();
            _currentVertices!.AddRange(builder._currentVertices);
            _currentIndices!.AddRange(builder._currentIndices);
        }
    }

    public void AddTriangle(Vertex a, Vertex b, Vertex c)
    {
        if(!CanAddToCurrent(3, 3))
            AddNewDataList();

        AddVertex(a);
        AddVertex(b);
        AddVertex(c);

        AddTriangleIndex(3, 2, 1);
    }

    public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector4 tangent, Rect uv)
    {
        if(!CanAddToCurrent(4, 6))
            AddNewDataList();

        var oldNormal = Default.Normal;
        var oldTangent = Default.Tangent;
        Default.Normal = normal;
        Default.Tangent = tangent;

        AddVertex(a, uv.TopLeft);
        AddVertex(b, uv.BottomLeft);
        AddVertex(c, uv.BottomRight);
        AddVertex(d, uv.TopRight);

        AddTriangleIndex(4, 3, 2);
        AddTriangleIndex(2, 1, 4);

        Default.Normal = oldNormal;
        Default.Tangent = oldTangent;
    }

    public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector4 tangent) =>
        AddQuad(a, b, c, d, normal, tangent, new Rect(0, 1));

    public void AddQuad(Vertex a, Vertex b, Vertex c, Vertex d)
    {
        if(!CanAddToCurrent(4, 6))
            AddNewDataList();

        AddVertex(a);
        AddVertex(b);
        AddVertex(c);
        AddVertex(d);

        AddTriangleIndex(4, 3, 2);
        AddTriangleIndex(2, 1, 4);
    }

    public void AddVoxel(Vector3Int voxelPosition, Vector3 voxelSize, Rect uv) => AddCube(voxelPosition * voxelSize, voxelSize, uv);
    public void AddVoxel(Vector3Int voxelPosition, Vector3 voxelSize, Rect uv, HashSet<Direction> exceptSides) => AddCube(voxelPosition * voxelSize, voxelSize, uv, exceptSides);


    public void AddCube(Vector3 position, Vector3 size) => AddCube(position, size, new Rect(0, 1), new HashSet<Direction>());
    public void AddCube(Vector3 position, Vector3 size, Rect uv) => AddCube(position, size, uv, new HashSet<Direction>());

    public void AddCube(Vector3 position, Vector3 size, Rect uv, HashSet<Direction> exceptSides)
    {
        //        6_______5
        //       /|      /|
        //     7/_|____4/ |
        //      | |     | | 
        //      | |2____|_|1         z  x
        //      | /     | /          | /
        //      |/______|/        y__|/
        //      3 front 0 


        Vector3[] corners = new Vector3[8]
        {
            position,
            position + new Vector3(size.x, 0f, 0f),
            position + new Vector3(size.x, size.y, 0f),
            position + new Vector3(0f, size.y, 0f),
            position + new Vector3(0f, 0f, size.y),
            position + new Vector3(size.x, 0f, size.y),
            position + new Vector3(size.x, size.y, size.y),
            position + new Vector3(0f, size.y, size.y)
        };

        if(!exceptSides.Contains(Direction.Down))
            AddQuad(corners[3], corners[2], corners[1], corners[0], Vector3.Down, TangentForward, uv); // bottom
        if(!exceptSides.Contains(Direction.Up))
            AddQuad(corners[6], corners[7], corners[4], corners[5], Vector3.Up, TangentForward, uv); // top
        if(!exceptSides.Contains(Direction.Right))
            AddQuad(corners[4], corners[0], corners[1], corners[5], Vector3.Right, TangentUp, uv); // right
        if(!exceptSides.Contains(Direction.Forward))
            AddQuad(corners[5], corners[1], corners[2], corners[6], Vector3.Forward, TangentUp, uv); // back
        if(!exceptSides.Contains(Direction.Left))
            AddQuad(corners[6], corners[2], corners[3], corners[7], Vector3.Left, TangentUp, uv); // left
        if(!exceptSides.Contains(Direction.Backward))
            AddQuad(corners[7], corners[3], corners[0], corners[4], Vector3.Backward, TangentUp, uv); // front
    }


    public List<VertexBuffer> ToVertexBuffers()
    {
        List<VertexBuffer> buffers = new();

        for(int i = 0; i < _vertices.Count; ++i)
        {
            var vertices = _vertices[i];
            var indices = _indices[i];

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

    protected List<Vertex> CombineVertices()
    {
        List<Vertex> result = new();

        foreach(var vertices in _vertices)
            result.AddRange(vertices);

        return result;
    }

    protected List<int> CombineIndices()
    {
        List<int> result = new();

        int offset = 0;
        for(int i = 0; i < _indices.Count; ++i)
        {
            var indices = _indices[i];
            foreach(var index in indices)
                result.Add(index + offset);
            offset += _vertices[i].Count;
        }

        return result;
    }

    public void AddAsCollisionMesh(ModelBuilder modelBuilder)
    {
        modelBuilder.AddCollisionMesh(CombineVertices().Select(v => v.Position).ToArray(), CombineIndices().ToArray());
    }
}
