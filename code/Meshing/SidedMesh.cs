using Sandbox;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelWorld.Meshing;

public sealed class SidedMesh<V> : ISidedMeshPart<V> where V : unmanaged, IVertex
{
    public BBox Bounds { get; private set; }
    public bool IsEmpty { get; private set; }

    private readonly Dictionary<Direction, UnlimitedMesh<V>.Builder> _sidedElements = new();
    private readonly UnlimitedMesh<V>.Builder _notSidedElements = new();

    public SidedMesh()
    {
        IsEmpty = true;
    }

    public SidedMesh(Dictionary<Direction, UnlimitedMesh<V>.Builder> sidedElements, UnlimitedMesh<V>.Builder notSidedElements)
    {
        foreach(var (direction, sidedElement) in sidedElements)
        {
            var builder = new UnlimitedMesh<V>.Builder().Add(sidedElement);
            _sidedElements[direction] = builder;
        }
        _notSidedElements = new UnlimitedMesh<V>.Builder().Add(notSidedElements);

        RecalculateBounds();
        RecalculateEmptiness();
    }

    private SidedMesh(Dictionary<Direction, UnlimitedMesh<V>.Builder> sidedElements,
        UnlimitedMesh<V>.Builder notSidedElements, BBox bounds, bool isEmpty)
    {
        foreach(var sidedElementEntry in sidedElements)
        {
            var builder = new UnlimitedMesh<V>.Builder().Add(sidedElementEntry.Value);
            _sidedElements[sidedElementEntry.Key] = builder;
        }
        _notSidedElements = new UnlimitedMesh<V>.Builder().Add(notSidedElements);
        Bounds = bounds;
        IsEmpty = isEmpty;
    }

    public SidedMesh<T> Convert<T>(Func<V, T> vertexChanger) where T : unmanaged, IVertex
    {
        var sidedElements = _sidedElements.ToDictionary(e => e.Key, e => e.Value.Convert(vertexChanger));
        var notSidedElements = _notSidedElements.Convert(vertexChanger);

        return new SidedMesh<T>(sidedElements, notSidedElements);
    }

    public SidedMesh<V> Scale(Vector3 center, float scale) => Convert((v) =>
    {
        var position = v.GetPosition();
        var offset = position - center;
        v.SetPosition(position + offset * scale);
        return v;
    });

    public SidedMesh<V> RotateAround(RightAngle rightAngleRotation, Direction lookDirection, Vector3 center)
    {
        if(rightAngleRotation == RightAngle.Angle0)
            return this;

        var rotation = rightAngleRotation.ToRotation(lookDirection);
        var notSidedElements = new UnlimitedMesh<V>.Builder().Add(_notSidedElements).RotateAround(rotation, center);
        Dictionary<Direction, UnlimitedMesh<V>.Builder> sidedElements = new();
        foreach(var (oldDirection, oldBuilder) in _sidedElements)
        {
            var newDirection = Direction.ClosestTo(oldDirection.Normal * rotation);
            var newBuilder = new UnlimitedMesh<V>.Builder().Add(oldBuilder).RotateAround(rotation, center);
            sidedElements[newDirection] = newBuilder;
        }
        return new(sidedElements, notSidedElements);
    }

    public void AddToBuilder(UnlimitedMesh<V>.Builder builder, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default)
    {
        AddNotSidedPartToBuilder(builder, offset);
        foreach(var face in sidesToAdd)
            AddSideToBuilder(builder, face, offset);
    }

    public void AddNotSidedPartToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 offset = default)
    {
        builder.Add(_notSidedElements, offset);
    }

    public void AddSideToBuilder(UnlimitedMesh<V>.Builder builder, Direction sideToAdd, Vector3 offset = default)
    {
        if(_sidedElements.TryGetValue(sideToAdd, out var element))
            builder.Add(element, offset);
    }

    public void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 offset = default) => AddToBuilder(builder, Direction.AllSet, offset);
    // thread safe
    public void AddAsCollisionMesh(ModelBuilder builder, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default)
    {
        _notSidedElements.AddAsCollisionMesh(builder, offset);
        foreach(var direction in sidesToAdd)
        {
            if(_sidedElements.TryGetValue(direction, out var sidedElement))
                sidedElement.AddAsCollisionMesh(builder, offset);
        }
    }

    // thread safe
    public void AddAsCollisionMesh(ModelBuilder builder, Vector3 offset = default) => AddAsCollisionMesh(builder, Direction.AllSet, offset);

    // thread safe
    public void AddAsCollisionHull(ModelBuilder builder, Vector3 center, Rotation rotation, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default)
    {
        var vertices = _notSidedElements.CombineVertices();
        foreach(var direction in sidesToAdd)
        {
            if(_sidedElements.TryGetValue(direction, out var sidedElement))
                vertices.AddRange(sidedElement.CombineVertices());
        }

        var verticesResult = vertices.Select(v => v.GetPosition() + offset).ToArray();
        builder.AddCollisionHull(verticesResult, center, rotation);
    }
    // thread safe
    public void AddAsCollisionHull(ModelBuilder builder, Vector3 center, Rotation rotation, Vector3 offset = default) => AddAsCollisionHull(builder, center, rotation, Direction.AllSet, offset);

    private void RecalculateBounds()
    {
        BBox? bounds = null;
        foreach(var (direction, sidedElement) in _sidedElements)
        {
            if(!sidedElement.IsEmpty)
                bounds = bounds.AddOrCreate(sidedElement.Bounds);
        }
        if(!_notSidedElements.IsEmpty)
            bounds = bounds.AddOrCreate(_notSidedElements.Bounds);

        Bounds = bounds ?? new();
    }

    private void RecalculateEmptiness()
    {
        IsEmpty = _notSidedElements.IsEmpty && _sidedElements.All(e => e.Value.IsEmpty);
    }

    public (List<int> indices, List<V> vertices) ToRaw()
    {
        (List<int> indices, List<V> vertices) result = (_notSidedElements.CombineIndices(), _notSidedElements.CombineVertices());

        foreach(var (_, sidedElement) in _sidedElements)
        {
            result.indices.AddRange(sidedElement.CombineIndices().Select(i => i + result.vertices.Count));
            result.vertices.AddRange(sidedElement.CombineVertices());
        }

        return result;
    }

    public List<V> CombineVertices()
    {
        List<V> result = _notSidedElements.CombineVertices();

        foreach(var (_, sidedElement) in _sidedElements)
            result.AddRange(sidedElement.CombineVertices());

        return result;
    }

    public List<int> CombineIndices() => ToRaw().indices;


    public class Builder : ISidedMeshPart<V>
    {
        protected SidedMesh<V> Mesh { get; private set; } = new();

        protected BBox? BuildingBounds = null;
        public BBox Bounds => BuildingBounds ?? default;

        public virtual bool IsEmpty
        {
            get
            {
                Mesh.RecalculateEmptiness();
                return Mesh.IsEmpty;
            }
        }

        private UnlimitedMesh<V>.Builder GetOrCreateSidedBuilder(Direction direction)
        {
            if(!Mesh._sidedElements.TryGetValue(direction, out var builder))
            {
                builder = new();
                Mesh._sidedElements[direction] = builder;
            }
            return builder!;
        }

        public virtual Builder Clear()
        {
            Mesh = new();
            BuildingBounds = null;
            return this;
        }


        public virtual SidedMesh<T>.Builder Convert<T>(Func<V, T> vertexChanger) where T : unmanaged, IVertex
        {
            SidedMesh<T>.Builder result = new()
            {
                Mesh = Mesh.Convert(vertexChanger)
            };

            result.BuildingBounds = result.IsEmpty ? null : Mesh.Bounds;
            return result;
        }

        public virtual Builder ChangeEveryVertex(Func<V, V> vertexChanger)
        {
            Mesh = Mesh.Convert(vertexChanger);
            BuildingBounds = IsEmpty ? null : Mesh.Bounds;
            return this;
        }

        public virtual Builder Scale(Vector3 center, float scale) => ChangeEveryVertex((v) =>
        {
            var position = v.GetPosition();
            var offset = position - center;
            v.SetPosition(center + offset * scale);
            return v;
        });

        public virtual Builder RotateAround(RightAngle rightAngleRotation, Direction lookDirection, Vector3 center)
        {
            Mesh = Mesh.RotateAround(rightAngleRotation, lookDirection, center);
            BuildingBounds = IsEmpty ? null : Mesh.Bounds;
            return this;
        }

        public Builder Add(IMeshPart<V> part, Vector3 offset = default)
        {
            part.AddToBuilder(Mesh._notSidedElements, offset);

            if(!part.IsEmpty)
                BuildingBounds = BuildingBounds.AddOrCreate(part.Bounds + offset);
            return this;
        }

        public Builder Add(ISidedMeshPart<V> part, Vector3 offset = default)
        {
            part.AddNotSidedPartToBuilder(Mesh._notSidedElements, offset);
            foreach(var direction in Direction.All)
                part.AddSideToBuilder(GetOrCreateSidedBuilder(direction), direction, offset);

            if(!part.IsEmpty)
                BuildingBounds = BuildingBounds.AddOrCreate(part.Bounds + offset);
            return this;
        }

        public Builder Add(SidedMesh<V>.Builder builder)
        {
            Mesh._notSidedElements.Add(builder.Mesh._notSidedElements);

            foreach(var (direction, sidedElement) in builder.Mesh._sidedElements)
                Mesh._sidedElements[direction].Add(sidedElement);

            if(!builder.IsEmpty)
                BuildingBounds = BuildingBounds.AddOrCreate(builder.Bounds);
            return this;
        }

        public Builder Add(UnlimitedMesh<V>.Builder builder)
        {
            Mesh._notSidedElements.Add(builder);
            if(!builder.IsEmpty)
                BuildingBounds = BuildingBounds.AddOrCreate(builder.Bounds);
            return this;
        }

        public Builder Add(UnlimitedMesh<V>.Builder builder, Direction side)
        {
            GetOrCreateSidedBuilder(side).Add(builder);
            if(!builder.IsEmpty)
                BuildingBounds = BuildingBounds.AddOrCreate(builder.Bounds);
            return this;
        }

        public void AddToBuilder(UnlimitedMesh<V>.Builder builder, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default) =>
            Mesh.AddToBuilder(builder, sidesToAdd, offset);
        public void AddNotSidedPartToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 offset = default) =>
            Mesh.AddNotSidedPartToBuilder(builder, offset);
        public void AddSideToBuilder(UnlimitedMesh<V>.Builder builder, Direction sideToAdd, Vector3 offset = default) =>
            Mesh.AddSideToBuilder(builder, sideToAdd, offset);
        public void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 offset = default) =>
            Mesh.AddToBuilder(builder, offset);

        // thread safe if builder is not being changed during execution
        public void AddAsCollisionMesh(ModelBuilder builder, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default) =>
            Mesh.AddAsCollisionMesh(builder, sidesToAdd, offset);

        // thread safe if builder is not being changed during execution
        public void AddAsCollisionMesh(ModelBuilder builder, Vector3 offset = default) =>
            Mesh.AddAsCollisionMesh(builder, offset);

        // thread safe if builder is not being changed during execution
        public void AddAsCollisionHull(ModelBuilder builder, Vector3 center, Rotation rotation, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default) =>
            Mesh.AddAsCollisionHull(builder, center, rotation, sidesToAdd, offset);

        // thread safe if builder is not being changed during execution
        public void AddAsCollisionHull(ModelBuilder builder, Vector3 center, Rotation rotation, Vector3 offset = default) =>
            Mesh.AddAsCollisionHull(builder, center, rotation, offset);


        public (List<int> indices, List<V> vertices) ToRaw() => Mesh.ToRaw();
        public List<V> CombineVertices() => Mesh.CombineVertices();
        public List<int> CombineIndices() => Mesh.CombineIndices();

        public SidedMesh<V> Build() => new(Mesh._sidedElements, Mesh._notSidedElements, Bounds, IsEmpty);
    }
}
