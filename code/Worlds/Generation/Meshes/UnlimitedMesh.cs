using Sandbox;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Worlds.Generation.Meshes;

public sealed class UnlimitedMesh<V> : IMeshPart<V> where V : unmanaged, IVertex
{
    private readonly List<List<V>> _vertices = new();
    private readonly List<List<ushort>> _indices = new();

    public BBox Bounds { get; private set; }

    public int PartsCount => _vertices.Count;

    public IReadOnlyList<V> GetVertices(int partIndex) => _vertices[partIndex].AsReadOnly();
    public IReadOnlyList<ushort> GetIndices(int partIndex) => _indices[partIndex].AsReadOnly();

    private UnlimitedMesh(List<List<V>> vertices, List<List<ushort>> indices, BBox bounds)
    {
        _vertices = vertices;
        _indices = indices;
        Bounds = bounds;
    }

    public UnlimitedMesh()
    {

    }

    public UnlimitedMesh(UnlimitedMesh<V> mesh)
    {
        _vertices = mesh._vertices.Select(l => new List<V>(l)).ToList();
        _indices = mesh._indices.Select(l => new List<ushort>(l)).ToList();
        Bounds = mesh.Bounds;
    }

    public UnlimitedMesh<V> RotateAround(Rotation rotation, Vector3 center)
    {
        var indices = _indices.Select(l => new List<ushort>(l)).ToList();

        BBox bounds = IsEmpty() ? default : BBox.FromPositionAndSize(_vertices.First(v => v.Count > 0)[0].GetPosition(), 0f);
        var vertices = _vertices.Select(l => new List<V>(l.Select(v =>
        {
            var result = v;
            var newPosition = (result.GetPosition() - center) * rotation + center;
            result.SetPosition(newPosition);
            bounds = bounds.AddPoint(newPosition);
            return result;
        }))).ToList();

        return new(vertices, indices, bounds);
    }

    public bool IsEmpty()
    {
        foreach(var vertices in _vertices)
        {
            if(vertices.Count != 0)
                return false;
        }
        foreach(var indices in _indices)
        {
            if(indices.Count != 0)
                return false;
        }
        return true;
    }

    public List<V> CombineVertices()
    {
        List<V> result = new();

        foreach(var vertices in _vertices)
            result.AddRange(vertices);

        return result;
    }

    public List<int> CombineIndices()
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

    public void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 position)
    {
        builder.Add(this, position);
    }

    // call only in main thread
    public void CreateBuffersFor(Mesh mesh, int partIndex)
    {
        ThreadSafe.AssertIsMainThread();
        var vertices = _vertices[partIndex];
        var indices = _indices[partIndex];

        mesh.CreateVertexBuffer(vertices.Count, Vertex.Layout, new List<V>(vertices));
        mesh.CreateIndexBuffer(indices.Count, indices.Select(i => (int)i).ToList());

        BBox bounds = vertices.Count == 0 ? new() : BBox.FromPositionAndSize(vertices[0].GetPosition());
        foreach(var vertex in vertices)
            bounds = bounds.AddPoint(vertex.GetPosition());

        mesh.Bounds = bounds;
    }

    // thread safe
    public void AddAsCollisionMesh(ModelBuilder modelBuilder)
    {
        for(int i = 0; i < PartsCount; ++i)
            AddAsCollisionMesh(modelBuilder, i);
    }

    // thread safe
    public void AddAsCollisionMesh(ModelBuilder modelBuilder, int partIndex)
    {
        var vertices = _vertices[partIndex].Select(v => v.GetPosition()).ToArray();
        var indices = _indices[partIndex].Select(i => (int)i).ToArray();

        modelBuilder.AddCollisionMesh(vertices, indices);
    }

    public UnlimitedMesh<T> Convert<T>(Func<V, T> vertexConvertor) where T : unmanaged, IVertex
    {
        UnlimitedMesh<T> result = new();

        foreach(var vertices in _vertices)
            result._vertices.Add(vertices.Select(vertexConvertor).ToList());

        foreach(var indices in _indices)
            result._indices.Add(new List<ushort>(indices));

        result.Bounds = Bounds;

        return result;
    }

    public class Builder : IMeshPart<V>, IBounded
    {
        protected UnlimitedMesh<V> Mesh { get; private set; } = new();

        protected List<List<V>> Vertices => Mesh._vertices;
        protected List<List<ushort>> Indices => Mesh._indices;

        protected List<V>? CurrentVertices;
        protected List<ushort>? CurrentIndices;

        public BBox Bounds { get; protected set; }

        public V Default;

        public int PartsCount => Mesh.PartsCount;


        public virtual bool IsEmpty() => Mesh.IsEmpty();

        public virtual Builder Clear()
        {
            Vertices.Clear();
            Indices.Clear();
            CurrentVertices = null;
            CurrentIndices = null;
            Bounds = default;
            return this;
        }

        public virtual Builder RotateAround(Rotation rotation, Vector3 center)
        {
            Mesh = Mesh.RotateAround(rotation, center);
            CurrentVertices = Vertices.Count == 0 ? null : Vertices[^1];
            CurrentIndices = Indices.Count == 0 ? null : Indices[^1];
            return this;
        }

        public virtual Builder Add(IMeshPart<V> meshPart, Vector3 offset = default)
        {
            meshPart.AddToBuilder(this, offset);
            return this;
        }

        public virtual Builder Add(UnlimitedMesh<V> mesh, Vector3 offset = default)
        {
            if(mesh.IsEmpty())
                return this;

            ExpandBounds(mesh.Bounds.Translate(offset));
            for(int i = 0; i < mesh._vertices.Count; ++i)
            {
                var vertices = mesh._vertices[i];
                var indices = mesh._indices[i];
                if(CanAddToCurrent(vertices.Count, indices.Count))
                {
                    int startIndex = CurrentVertices!.Count;
                    for(int j = 0; j < vertices.Count; ++j)
                    {
                        var newVertex = vertices[j];
                        newVertex.SetPosition(newVertex.GetPosition() + offset);
                        CurrentVertices.Add(newVertex);
                    }
                    foreach(var index in indices)
                        AddRawIndex((ushort)(index + startIndex));
                }
                else
                {
                    AddNewDataList();
                    for(int j = 0; j < vertices.Count; ++j)
                    {
                        var newVertex = vertices[j];
                        newVertex.SetPosition(newVertex.GetPosition() + offset);
                        CurrentVertices!.Add(newVertex);
                    }
                    CurrentIndices!.AddRange(indices);
                }
            }
            return this;
        }

        public virtual void AddToBuilder(Builder builder, Vector3 position) => builder.Add(Mesh, position);

        public virtual Builder AddTriangle(V a, V b, V c)
        {
            if(!CanAddToCurrent(3, 3))
                AddNewDataList();

            AddVertex(a);
            AddVertex(b);
            AddVertex(c);

            AddTriangleIndex(3, 2, 1);
            return this;
        }

        public virtual Builder AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            if(!CanAddToCurrent(3, 3))
                AddNewDataList();

            AddVertex(a);
            AddVertex(b);
            AddVertex(c);

            AddTriangleIndex(3, 2, 1);
            return this;
        }

        public virtual Builder AddQuad(V a, V b, V c, V d)
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

        public virtual Builder AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
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

        public virtual Builder AddCube(Vector3 position, Vector3 size)
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

            AddQuad(corners[3], corners[2], corners[1], corners[0]); // bottom
            AddQuad(corners[6], corners[7], corners[4], corners[5]); // top
            AddQuad(corners[4], corners[0], corners[1], corners[5]); // right
            AddQuad(corners[5], corners[1], corners[2], corners[6]); // back
            AddQuad(corners[6], corners[2], corners[3], corners[7]); // left
            AddQuad(corners[7], corners[3], corners[0], corners[4]); // front
            return this;
        }

        public virtual Builder AddCube(Vector3 position, Vector3 size, IReadOnlySet<Direction> sidesToAdd)
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
                AddQuad(corners[3], corners[2], corners[1], corners[0]); // bottom
            if(sidesToAdd.Contains(Direction.Up))
                AddQuad(corners[6], corners[7], corners[4], corners[5]); // top
            if(sidesToAdd.Contains(Direction.Right))
                AddQuad(corners[4], corners[0], corners[1], corners[5]); // right
            if(sidesToAdd.Contains(Direction.Forward))
                AddQuad(corners[5], corners[1], corners[2], corners[6]); // back
            if(sidesToAdd.Contains(Direction.Left))
                AddQuad(corners[6], corners[2], corners[3], corners[7]); // left
            if(sidesToAdd.Contains(Direction.Backward))
                AddQuad(corners[7], corners[3], corners[0], corners[4]); // front
            return this;
        }


        public List<V> CombineVertices() => Mesh.CombineVertices();

        public List<int> CombineIndices() => Mesh.CombineIndices();



        protected virtual Builder AddNewDataList()
        {
            CurrentVertices = new List<V>();
            CurrentIndices = new List<ushort>();
            Vertices.Add(CurrentVertices);
            Indices.Add(CurrentIndices);
            return this;
        }

        protected virtual bool CanAddToCurrent(int vertexCount, int indexCount)
        {
            if(CurrentIndices == null || CurrentVertices == null)
                return false;
            return CurrentVertices.Count + vertexCount <= ushort.MaxValue && CurrentIndices.Count + indexCount <= ushort.MaxValue;
        }

        protected virtual Builder AddRawIndex(ushort i)
        {
            CurrentIndices!.Add(i);
            return this;
        }
        protected virtual Builder AddIndex(ushort i) => AddRawIndex((ushort)(CurrentVertices!.Count - i));

        protected virtual Builder AddTriangleIndex(ushort a, ushort b, ushort c)
        {
            AddIndex(a);
            AddIndex(b);
            AddIndex(c);
            return this;
        }

        protected virtual Builder ExpandBounds(Vector3 toAddVertexPosition)
        {
            bool isFirstVertex = Vertices.Count == 0 || Vertices.Count == 1 && Vertices[0].Count == 0;
            if(isFirstVertex)
                Bounds = new(toAddVertexPosition, toAddVertexPosition);
            else
                Bounds = Bounds.AddPoint(toAddVertexPosition);
            return this;
        }

        protected virtual Builder ExpandBounds(BBox toAddVerticesBounds)
        {
            bool isFirstVertex = Vertices.Count == 0 || Vertices.Count == 1 && Vertices[0].Count == 0;
            if(isFirstVertex)
                Bounds = toAddVerticesBounds;
            else
                Bounds = Bounds.AddBBox(toAddVerticesBounds);
            return this;
        }

        protected virtual Builder AddVertex(V vertex)
        {
            ExpandBounds(vertex.GetPosition());
            CurrentVertices!.Add(vertex);
            return this;
        }

        protected virtual Builder AddVertex(Vector3 position)
        {
            var vertexToAdd = Default;
            vertexToAdd.SetPosition(position);
            ExpandBounds(position);
            CurrentVertices!.Add(vertexToAdd);
            return this;
        }

        // call only in main thread
        public virtual void CreateBuffersFor(Mesh mesh, int partIndex) => Mesh.CreateBuffersFor(mesh, partIndex);

        // thread safe if builder is not being changed during execution
        public virtual void AddAsCollisionMesh(ModelBuilder modelBuilder, int partIndex) => Mesh.AddAsCollisionMesh(modelBuilder, partIndex);
        // thread safe if builder is not being changed during execution
        public virtual void AddAsCollisionMesh(ModelBuilder modelBuilder) => Mesh.AddAsCollisionMesh(modelBuilder);

        public UnlimitedMesh<V> Build() => new(Mesh);
    }

    public class Builder<T> : Builder where T : Builder<T>
    {
        public override T Clear() => (T)base.Clear();

        public override T Add(IMeshPart<V> meshPart, Vector3 offset = default) => (T)base.Add(meshPart, offset);
        public override T Add(UnlimitedMesh<V> mesh, Vector3 offset = default) => (T)base.Add(mesh, offset);

        public override T AddTriangle(V a, V b, V c) => (T)base.AddTriangle(a, b, c);
        public override T AddTriangle(Vector3 a, Vector3 b, Vector3 c) => (T)base.AddTriangle(a, b, c);

        public override T AddQuad(V a, V b, V c, V d) => (T)base.AddQuad(a, b, c, d);
        public override T AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d) => (T)base.AddQuad(a, b, c, d);

        public override T AddCube(Vector3 position, Vector3 size) => (T)base.AddCube(position, size);
        public override T AddCube(Vector3 position, Vector3 size, IReadOnlySet<Direction> sidesToAdd) => (T)base.AddCube(position, size, sidesToAdd);

        protected override T AddNewDataList() => (T)base.AddNewDataList();

        protected override T AddRawIndex(ushort i) => (T)base.AddRawIndex(i);
        protected override T AddIndex(ushort i) => (T)base.AddIndex(i);
        protected override T AddTriangleIndex(ushort a, ushort b, ushort c) => (T)base.AddTriangleIndex(a, b, c);

        protected override T ExpandBounds(Vector3 toAddVertexPosition) => (T)base.ExpandBounds(toAddVertexPosition);
        protected override T ExpandBounds(BBox toAddVerticesBounds) => (T)base.ExpandBounds(toAddVerticesBounds);

        protected override T AddVertex(V vertex) => (T)base.AddVertex(vertex);
        protected override T AddVertex(Vector3 position) => (T)base.AddVertex(position);
    }
}