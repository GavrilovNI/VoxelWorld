using Sandbox;
using VoxelWorld.Mth;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelWorld.Meshing;

public sealed class UnlimitedMesh<V> : IMeshPart<V> where V : unmanaged, IVertex
{
    private readonly List<List<V>> _vertices = new();
    private readonly List<List<ushort>> _indices = new();

    public BBox Bounds { get; private set; }

    public bool IsEmpty { get; private set; }

    public int PartsCount => _vertices.Count;

    public IReadOnlyList<V> GetVertices(int partIndex) => _vertices[partIndex].AsReadOnly();
    public IReadOnlyList<ushort> GetIndices(int partIndex) => _indices[partIndex].AsReadOnly();

    private UnlimitedMesh(List<List<V>> vertices, List<List<ushort>> indices)
    {
        _vertices = vertices;
        _indices = indices;
        RecalculateBounds();
        RecalculateEmptiness();
    }

    public UnlimitedMesh()
    {
        Bounds = default;
        IsEmpty = true;
    }

    public UnlimitedMesh(List<V> vertices, List<ushort> indices)
    {
        if(vertices.Count > ushort.MaxValue)
            throw new ArgumentException($"max vertex count is {ushort.MaxValue}", nameof(vertices));

        _vertices = new List<List<V>>() { vertices.ToList() };
        _indices = new List<List<ushort>>() { indices.ToList() };
        RecalculateBounds();
        RecalculateEmptiness();
    }

    public UnlimitedMesh(UnlimitedMesh<V> mesh)
    {
        _vertices = mesh._vertices.Select(l => new List<V>(l)).ToList();
        _indices = mesh._indices.Select(l => new List<ushort>(l)).ToList();
        Bounds = mesh.Bounds;
        IsEmpty = mesh.IsEmpty;
    }

    public UnlimitedMesh<T> Convert<T>(Func<V, T> vertexChanger) where T : unmanaged, IVertex
    {
        var result = new UnlimitedMesh<T>();

        if(IsEmpty)
            return result;

        foreach(var vertices in _vertices)
            result._vertices.Add(vertices.Select(vertexChanger).ToList());

        foreach(var indices in _indices)
            result._indices.Add(new List<ushort>(indices));

        result.RecalculateBounds();
        return result;
    }

    public UnlimitedMesh<V> Scale(Vector3 center, float scale) => Convert((v) =>
    {
        var position = v.GetPosition();
        var offset = position - center;
        v.SetPosition(position + offset * scale);
        return v;
    });

    public UnlimitedMesh<V> Scale(Vector3 center, Vector3 scale) => Convert((v) =>
    {
        var position = v.GetPosition();
        var offset = position - center;
        v.SetPosition(position + offset * scale);
        return v;
    });

    public UnlimitedMesh<V> RotateAround(Rotation rotation, Vector3 center)
    {
        if(IsEmpty)
            return this;

        var indices = _indices.Select(l => new List<ushort>(l)).ToList();
        var vertices = _vertices.Select(l => new List<V>(l.Select(v =>
        {
            v.SetPosition((v.GetPosition() - center) * rotation + center);
            return v;
        }))).ToList();

        return new(vertices, indices);
    }

    public (List<int> indices, List<V> vertices) ToRaw()
    {
        (List<int> indices, List<V> vertices) result = (new(), new());

        for(int i = 0; i < PartsCount; ++i)
        {
            result.indices.AddRange(_indices[i].Select(i => i + result.vertices.Count));
            result.vertices.AddRange(_vertices[i]);
        }

        return result;
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

    public void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 offset = default) => builder.Add(this, offset);

    // call only in main thread
    public void CreateBuffersFor(Mesh mesh, int partIndex, bool calculateBounds = true)
    {
        ThreadSafe.AssertIsMainThread();
        var vertices = _vertices[partIndex];
        var indices = _indices[partIndex];

        var layout = vertices.Count == 0 ? Array.Empty<VertexAttribute>() : vertices[0].Layout;

        mesh.CreateVertexBuffer(vertices.Count, layout, new List<V>(vertices));
        mesh.CreateIndexBuffer(indices.Count, indices.Select(i => (int)i).ToList());

        if(!calculateBounds)
            return;

        BBox bounds = vertices.Count == 0 ? new() : BBox.FromPositionAndSize(vertices[0].GetPosition());
        foreach(var vertex in vertices)
            bounds = bounds.AddPoint(vertex.GetPosition());

        mesh.Bounds = bounds;
    }

    // thread safe
    public void AddAsCollisionMesh(ModelBuilder builder, Vector3 offset = default)
    {
        for(int i = 0; i < PartsCount; ++i)
            AddAsCollisionMesh(builder, i, offset);
    }

    // thread safe
    public void AddAsCollisionMesh(ModelBuilder builder, int partIndex, Vector3 offset = default)
    {
        var vertices = _vertices[partIndex].Select(v => v.GetPosition() + offset).ToArray();
        var indices = _indices[partIndex].Select(i => (int)i).ToArray();

        builder.AddCollisionMesh(vertices, indices);
    }

    public void AddAsCollisionHull(ModelBuilder builder, Vector3 center, Rotation rotation, Vector3 offset = default)
    {
        IEnumerable<Vector3> vertices = Enumerable.Empty<Vector3>();
        for(int i = 0; i < PartsCount; ++i)
            vertices = vertices.Concat(_vertices[i].Select(v => v.GetPosition() + offset));
        builder.AddCollisionHull(vertices.ToArray(), center, rotation);
    }

    public void AddAsCollisionHull(ModelBuilder builder, Vector3 center, Rotation rotation, int partIndex, Vector3 offset = default)
    {
        var vertices = _vertices[partIndex].Select(v => v.GetPosition() + offset).ToArray();
        builder.AddCollisionHull(vertices, center, rotation);
    }

    private void RecalculateBounds()
    {
        BBox? bounds = null;
        foreach(var vertices in _vertices)
        {
            foreach(var vertex in vertices)
            {
                bounds = bounds.AddOrCreate(vertex.GetPosition());
            }
        }
        Bounds = bounds ?? new();
    }

    private void RecalculateEmptiness()
    {
        IsEmpty = _vertices.All(v => v.Count == 0) && _indices.All(i => i.Count == 0);
    }

    public class Builder : IMeshPart<V>
    {
        protected UnlimitedMesh<V> Mesh { get; private set; }

        protected List<List<V>> Vertices => Mesh._vertices;
        protected List<List<ushort>> Indices => Mesh._indices;

        protected List<V>? CurrentVertices;
        protected List<ushort>? CurrentIndices;

        private BBox? _buildingBounds = null;
        protected BBox? BuildingBounds
        {
            get => _buildingBounds;
            set
            {
                _buildingBounds = value;
                Mesh.Bounds = value ?? default;
            }
        }
        public BBox Bounds => BuildingBounds ?? default;

        public V Default;

        public int PartsCount => Mesh.PartsCount;


        public bool IsEmpty
        {
            get => Mesh.IsEmpty;
            protected set => Mesh.IsEmpty = value;
        }

        public Builder() : this(new())
        {

        }

        protected Builder(UnlimitedMesh<V> mesh)
        {
            Mesh = mesh;
            BuildingBounds = mesh.IsEmpty ? null : mesh.Bounds;

            CurrentVertices = mesh._vertices.Count == 0 ? null : mesh._vertices[^1];
            CurrentIndices = mesh._indices.Count == 0 ? null : mesh._indices[^1];
        }

        public virtual Builder Clear()
        {
            Vertices.Clear();
            Indices.Clear();
            IsEmpty = true;
            CurrentVertices = null;
            CurrentIndices = null;
            BuildingBounds = null;
            return this;
        }

        public virtual UnlimitedMesh<T>.Builder Convert<T>(Func<V, T> vertexChanger) where T : unmanaged, IVertex =>
            new(Mesh.Convert(vertexChanger));

        public virtual Builder ChangeEveryVertex(Func<V, V> vertexChanger)
        {
            if(IsEmpty)
                return this;

            Mesh = Mesh.Convert(vertexChanger);
            CurrentVertices = Vertices.Count == 0 ? null : Vertices[^1];
            CurrentIndices = Indices.Count == 0 ? null : Indices[^1];
            BuildingBounds = Mesh.Bounds;
            return this;
        }

        public virtual Builder Scale(Vector3 center, float scale) => ChangeEveryVertex((v) =>
        {
            var position = v.GetPosition();
            var offset = position - center;
            v.SetPosition(center + offset * scale);
            return v;
        });

        public virtual Builder Scale(Vector3 center, Vector3 scale) => ChangeEveryVertex((v) =>
        {
            var position = v.GetPosition();
            var offset = position - center;
            v.SetPosition(center + offset * scale);
            return v;
        });

        public virtual Builder RotateAround(Rotation rotation, Vector3 center)
        {
            if(IsEmpty)
                return this;

            Mesh = Mesh.RotateAround(rotation, center);
            CurrentVertices = Vertices.Count == 0 ? null : Vertices[^1];
            CurrentIndices = Indices.Count == 0 ? null : Indices[^1];
            BuildingBounds = Mesh.Bounds;
            return this;
        }

        public virtual Builder Add(IMeshPart<V> meshPart, Vector3 offset = default)
        {
            if(meshPart.IsEmpty)
                return this;

            meshPart.AddToBuilder(this, offset);
            BuildingBounds = BuildingBounds.AddOrCreate(meshPart.Bounds + offset);
            IsEmpty = false;
            return this;
        }

        public virtual Builder Add(UnlimitedMesh<V> mesh, Vector3 offset = default)
        {
            if(mesh.IsEmpty)
                return this;

            BuildingBounds = BuildingBounds.AddOrCreate(mesh.Bounds.Translate(offset));
            IsEmpty = false;
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

        public virtual void AddToBuilder(Builder builder, Vector3 offset = default) => builder.Add(Mesh, offset);

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
            Vector3[] corners = MathV.GetCubeCorners(size);

            foreach(var corner in corners)
                AddVertex(corner);

            AddTriangleIndex(5, 6, 7); // bottom
            AddTriangleIndex(7, 8, 5);

            AddTriangleIndex(2, 1, 4); // top
            AddTriangleIndex(4, 3, 2);

            AddTriangleIndex(4, 8, 7); // right
            AddTriangleIndex(7, 3, 4);

            AddTriangleIndex(3, 7, 6); // back
            AddTriangleIndex(6, 2, 3);

            AddTriangleIndex(2, 6, 5); // left
            AddTriangleIndex(5, 1, 2);

            AddTriangleIndex(1, 5, 8); // front
            AddTriangleIndex(8, 4, 1);

            return this;
        }

        public (List<int> indices, List<V> vertices) ToRaw() => Mesh.ToRaw();
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
            IsEmpty = false;
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

        protected virtual Builder AddVertex(V vertex)
        {
            CurrentVertices!.Add(vertex);
            BuildingBounds = BuildingBounds.AddOrCreate(vertex.GetPosition());
            IsEmpty = false;
            return this;
        }

        protected virtual Builder AddVertex(Vector3 position)
        {
            var vertexToAdd = Default;
            vertexToAdd.SetPosition(position);
            CurrentVertices!.Add(vertexToAdd);
            BuildingBounds = BuildingBounds.AddOrCreate(position);
            IsEmpty = false;
            return this;
        }

        // call only in main thread
        public virtual void CreateBuffersFor(Mesh mesh, int partIndex, bool calculateBounds = true) => Mesh.CreateBuffersFor(mesh, partIndex, calculateBounds);

        // thread safe if builder is not being changed during execution
        public virtual void AddAsCollisionMesh(ModelBuilder builder, Vector3 offset = default) => Mesh.AddAsCollisionMesh(builder, offset);
        // thread safe if builder is not being changed during execution
        public virtual void AddAsCollisionMesh(ModelBuilder builder, int partIndex, Vector3 offset = default) => Mesh.AddAsCollisionMesh(builder, partIndex, offset);
        
        // thread safe if builder is not being changed during execution
        public void AddAsCollisionHull(ModelBuilder builder, Vector3 center, Rotation rotation, Vector3 offset = default) => Mesh.AddAsCollisionHull(builder, center, rotation, offset);
        // thread safe if builder is not being changed during execution
        public void AddAsCollisionHull(ModelBuilder builder, int partIndex, Vector3 center, Rotation rotation, Vector3 offset = default) => Mesh.AddAsCollisionHull(builder, center, rotation, partIndex, offset);


        public UnlimitedMesh<V> Build() => new(Mesh._vertices, Mesh._indices);
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
        
        protected override T AddNewDataList() => (T)base.AddNewDataList();

        protected override T AddRawIndex(ushort i) => (T)base.AddRawIndex(i);
        protected override T AddIndex(ushort i) => (T)base.AddIndex(i);
        protected override T AddTriangleIndex(ushort a, ushort b, ushort c) => (T)base.AddTriangleIndex(a, b, c);

        protected override T AddVertex(V vertex) => (T)base.AddVertex(vertex);
        protected override T AddVertex(Vector3 position) => (T)base.AddVertex(position);
    }
}