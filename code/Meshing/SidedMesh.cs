using Sandbox;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using System.Collections.Generic;

namespace Sandcube.Meshing;

public sealed class SidedMesh<V> : ISidedMeshPart<V> where V : unmanaged, IVertex
{
    public BBox Bounds { get; private set; }

    private readonly Dictionary<Direction, UnlimitedMesh<V>.Builder> _sidedElements = new();
    private readonly UnlimitedMesh<V>.Builder _notSidedElements = new();

    public SidedMesh()
    {

    }

    public SidedMesh(Dictionary<Direction, UnlimitedMesh<V>.Builder> sidedElements, UnlimitedMesh<V>.Builder notSidedElements)
    {
        BBox? bounds = null;
        foreach(var sidedElementEntry in sidedElements)
        {
            var builder = new UnlimitedMesh<V>.Builder().Add(sidedElementEntry.Value);
            _sidedElements[sidedElementEntry.Key] = builder;

            if(!builder.IsEmpty())
                bounds = bounds.AddOrCreate(builder.Bounds);
        }
        _notSidedElements = new UnlimitedMesh<V>.Builder().Add(notSidedElements);

        if(!_notSidedElements.IsEmpty())
            bounds = bounds.AddOrCreate(_notSidedElements.Bounds);

        Bounds = bounds ?? new();
    }

    private SidedMesh(Dictionary<Direction, UnlimitedMesh<V>.Builder> sidedElements,
        UnlimitedMesh<V>.Builder notSidedElements, BBox bounds)
    {
        foreach(var sidedElementEntry in sidedElements)
        {
            var builder = new UnlimitedMesh<V>.Builder().Add(sidedElementEntry.Value);
            _sidedElements[sidedElementEntry.Key] = builder;
        }
        _notSidedElements = new UnlimitedMesh<V>.Builder().Add(notSidedElements);
        Bounds = bounds;
    }

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
        builder.Add(_notSidedElements, offset);

        foreach(var face in sidesToAdd)
        {
            if(_sidedElements.TryGetValue(face, out var element))
                builder.Add(element, offset);
        }
    }

    public void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 offset = default) => AddToBuilder(builder, Direction.AllSet, offset);
    // thread safe
    public void AddAsCollisionMesh(ModelBuilder builder, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default)
    {
        _notSidedElements.AddAsCollisionMesh(builder);
        foreach(var direction in sidesToAdd)
        {
            if(_sidedElements.TryGetValue(direction, out var sidedElement))
                sidedElement.AddAsCollisionMesh(builder, offset);
        }
    }
    // thread safe
    public void AddAsCollisionMesh(ModelBuilder builder, Vector3 offset = default) => AddAsCollisionMesh(builder, Direction.AllSet, offset);


    public class Builder : ISidedMeshPart<V>
    {
        protected SidedMesh<V> Mesh { get; private set; } = new();

        protected BBox? BuildingBounds = null;
        public BBox Bounds => BuildingBounds ?? default;

        private UnlimitedMesh<V>.Builder GetOrCreateSidedBuilder(Direction direction)
        {
            if(!Mesh._sidedElements.TryGetValue(direction, out var builder))
            {
                builder = new();
                Mesh._sidedElements[direction] = builder;
            }
            return builder!;
        }

        public Builder Add(UnlimitedMesh<V>.Builder builder)
        {
            Mesh._notSidedElements.Add(builder);
            if(!builder.IsEmpty())
                BuildingBounds = BuildingBounds.AddOrCreate(builder.Bounds);
            return this;
        }

        public Builder Add(UnlimitedMesh<V>.Builder builder, Direction side)
        {
            GetOrCreateSidedBuilder(side).Add(builder);
            if(!builder.IsEmpty())
                BuildingBounds = BuildingBounds.AddOrCreate(builder.Bounds);
            return this;
        }

        public void AddToBuilder(UnlimitedMesh<V>.Builder builder, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default) =>
            Mesh.AddToBuilder(builder, sidesToAdd, offset);
        public void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 offset = default) =>
            Mesh.AddToBuilder(builder, offset);

        // thread safe if builder is not being changed during execution
        public void AddAsCollisionMesh(ModelBuilder builder, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default) =>
            Mesh.AddAsCollisionMesh(builder, sidesToAdd, offset);

        // thread safe if builder is not being changed during execution
        public void AddAsCollisionMesh(ModelBuilder builder, Vector3 offset = default) =>
            Mesh.AddAsCollisionMesh(builder, offset);


        public SidedMesh<V> Build() => new(Mesh._sidedElements, Mesh._notSidedElements, Bounds);
    }
}
