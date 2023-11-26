using Sandbox;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using System.Collections.Generic;

namespace Sandcube.Worlds.Generation;

public class VoxelMeshBuilder
{
    public static readonly Vector4 TangentForward = new(Vector3.Forward, 1);
    public static readonly Vector4 TangentUp = new(Vector3.Up, 1);

    protected readonly List<List<Vertex>> _vertices = new();
    protected readonly List<List<ushort>> _indices = new();

    protected List<Vertex>? _currentVertices = null;
    protected List<ushort>? _currentIndices = null;

    public BBox Bounds { get; protected set; }

    public Vertex Default;

    public VoxelMeshBuilder()
    {
        Clear();
    }

    public bool IsEmpty()
    {
        foreach(var verteces in _vertices)
        {
            if(verteces.Count != 0)
                return false;
        }
        foreach(var indices in _indices)
        {
            if(indices.Count != 0)
                return false;
        }
        return true;
    }

    protected VoxelMeshBuilder AddNewDataList()
    {
        _currentVertices = new List<Vertex>();
        _currentIndices = new List<ushort>();
        _vertices.Add(_currentVertices);
        _indices.Add(_currentIndices);
        return this;
    }

    protected bool CanAddToCurrent(int vertexCount, int indexCount)
    {
        if(_currentIndices == null || _currentVertices == null)
            return false;
        return _currentVertices.Count + vertexCount <= ushort.MaxValue && _currentIndices.Count + indexCount <= ushort.MaxValue;
    }

    public virtual VoxelMeshBuilder Clear()
    {
        _vertices.Clear();
        _indices.Clear();
        _currentVertices = null;
        _currentIndices = null;
        Default = new() { Color = Color.White };
        Bounds = default;
        return this;
    }

    protected VoxelMeshBuilder AddRawIndex(ushort i)
    {
        _currentIndices!.Add(i);
        return this;
    }
    protected VoxelMeshBuilder AddIndex(ushort i) => AddRawIndex((ushort)(_currentVertices!.Count - i));

    protected VoxelMeshBuilder AddTriangleIndex(ushort a, ushort b, ushort c)
    {
        AddIndex(a);
        AddIndex(b);
        AddIndex(c);
        return this;
    }

    protected void ExpandBounds(Vector3 toAddVertexPosition)
    {
        bool isFirstVertex = _vertices.Count == 0 || (_vertices.Count == 1 && _vertices[0].Count == 0);
        if(isFirstVertex)
            Bounds = new(toAddVertexPosition, toAddVertexPosition);
        else
            Bounds = Bounds.AddPoint(toAddVertexPosition);
    }

    protected void ExpandBounds(BBox toAddVerticesBounds)
    {
        bool isFirstVertex = _vertices.Count == 0 || (_vertices.Count == 1 && _vertices[0].Count == 0);
        if(isFirstVertex)
            Bounds = toAddVerticesBounds;
        else
            Bounds = Bounds.AddBBox(toAddVerticesBounds);
    }

    protected VoxelMeshBuilder AddVertex(Vertex vertex)
    {
        ExpandBounds(vertex.Position);
        _currentVertices!.Add(vertex);
        return this;
    }

    protected VoxelMeshBuilder AddVertex(Vector3 position, Vector2 uv)
    {
        var vertex = Default;
        vertex.Position = position;
        vertex.TexCoord0.x = uv.x;
        vertex.TexCoord0.y = uv.y;
        AddVertex(vertex);
        return this;
    }

    public VoxelMeshBuilder AddMeshBuilder(VoxelMeshBuilder builder, Vector3 offset = default)
    {
        if(builder.IsEmpty())
            return this;

        ExpandBounds(builder.Bounds.Translate(offset));
        for(int i = 0; i < builder._vertices.Count; ++i)
        {
            var vertices = builder._vertices[i];
            if(!object.ReferenceEquals(vertices, builder._currentVertices))
            {
                List<Vertex> newVertices = new();
                for(int j = 0; j < vertices.Count; ++j)
                {
                    var newVertex = vertices[j];
                    newVertex.Position += offset;
                    newVertices.Add(newVertex);
                }
                _vertices.Add(newVertices);
            }
        }
        for(int i = 0; i < builder._indices.Count; ++i)
        {
            var indices = builder._indices[i];
            if(!object.ReferenceEquals(indices, builder._currentIndices))
                _indices.Add(new List<ushort>(indices));
        }

        if(builder._currentVertices is null || builder._currentIndices is null)
            return this;

        if(CanAddToCurrent(builder._currentVertices.Count, builder._currentIndices.Count))
        {
            int startIndex = _currentVertices!.Count;
            for(int j = 0; j < builder._currentVertices.Count; ++j)
            {
                var newVertex = builder._currentVertices[j];
                newVertex.Position += offset;
                _currentVertices.Add(newVertex);
            }
            foreach(var index in builder._currentIndices)
                AddRawIndex((ushort)(index + startIndex));
        }
        else
        {
            AddNewDataList();
            for(int j = 0; j < builder._currentVertices.Count; ++j)
            {
                var newVertex = builder._currentVertices[j];
                newVertex.Position += offset;
                _currentVertices!.Add(newVertex);
            }
            _currentIndices!.AddRange(builder._currentIndices);
        }
        return this;
    }

    public VoxelMeshBuilder AddTriangle(Vertex a, Vertex b, Vertex c)
    {
        if(!CanAddToCurrent(3, 3))
            AddNewDataList();

        AddVertex(a);
        AddVertex(b);
        AddVertex(c);

        AddTriangleIndex(3, 2, 1);
        return this;
    }

    public VoxelMeshBuilder AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector4 tangent, Rect uv)
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
        return this;
    }

    public VoxelMeshBuilder AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector4 tangent) =>
        AddQuad(a, b, c, d, normal, tangent, new Rect(0, 1));

    public VoxelMeshBuilder AddQuad(Vertex a, Vertex b, Vertex c, Vertex d)
    {
        if(!CanAddToCurrent(4, 6))
            AddNewDataList();

        AddVertex(a);
        AddVertex(b);
        AddVertex(c);
        AddVertex(d);

        AddTriangleIndex(4, 3, 2);
        AddTriangleIndex(2, 1, 4);
        return this;
    }

    public VoxelMeshBuilder AddVoxel(Vector3Int voxelPosition, Vector3 voxelSize, Rect uv) => AddCube(voxelPosition * voxelSize, voxelSize, uv);
    public VoxelMeshBuilder AddVoxel(Vector3Int voxelPosition, Vector3 voxelSize, Rect uv, IReadOnlySet<Direction> sidesToAdd) =>
        AddCube(voxelPosition * voxelSize, voxelSize, uv, sidesToAdd);

    public VoxelMeshBuilder AddCube(Vector3 position, Vector3 size) => AddCube(position, size, new Rect(0, 1));
    public VoxelMeshBuilder AddCube(Vector3 position, Vector3 size, Rect uv) => AddCube(position, size, uv, Direction.AllSet);

    public VoxelMeshBuilder AddCube(Vector3 position, Vector3 size, Rect uv, IReadOnlySet<Direction> sidesToAdd)
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
