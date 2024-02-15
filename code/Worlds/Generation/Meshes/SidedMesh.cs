﻿using Sandbox;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using System.Collections.Generic;

namespace Sandcube.Worlds.Generation.Meshes;

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

    public void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 position, IReadOnlySet<Direction> visibleFaces)
    {
        builder.Add(_notSidedElements, position);

        foreach(var face in visibleFaces)
        {
            if(_sidedElements.TryGetValue(face, out var element))
                builder.Add(element, position);
        }
    }

    public void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 position) => AddToBuilder(builder, position, Direction.AllSet);
    // thread safe
    public void AddAsCollisionMesh(ModelBuilder modelBuilder, IReadOnlySet<Direction> facesToAdd, Vector3 offset = default)
    {
        _notSidedElements.AddAsCollisionMesh(modelBuilder);
        foreach(var direction in facesToAdd)
        {
            if(_sidedElements.TryGetValue(direction, out var sidedElement))
                sidedElement.AddAsCollisionMesh(modelBuilder, offset);
        }
    }
    // thread safe
    public void AddAsCollisionMesh(ModelBuilder modelBuilder, Vector3 offset = default) => AddAsCollisionMesh(modelBuilder, Direction.AllSet, offset);


    public class Builder : ISidedMeshPart<V>
    {
        private SidedMesh<V> _sidedMesh = new();

        private BBox? _bounds = null;
        public BBox Bounds => _bounds ?? default;

        private UnlimitedMesh<V>.Builder GetOrCreateSidedBuilder(Direction direction)
        {
            if(!_sidedMesh._sidedElements.TryGetValue(direction, out var builder))
            {
                builder = new();
                _sidedMesh._sidedElements[direction] = builder;
            }
            return builder!;
        }

        public Builder Add(UnlimitedMesh<V>.Builder voxelMeshBuilder)
        {
            _sidedMesh._notSidedElements.Add(voxelMeshBuilder);
            if(!voxelMeshBuilder.IsEmpty())
                _bounds = _bounds.AddOrCreate(voxelMeshBuilder.Bounds);
            return this;
        }

        public Builder Add(UnlimitedMesh<V>.Builder voxelMeshBuilder, Direction cullFace)
        {
            GetOrCreateSidedBuilder(cullFace).Add(voxelMeshBuilder);
            if(!voxelMeshBuilder.IsEmpty())
                _bounds = _bounds.AddOrCreate(voxelMeshBuilder.Bounds);
            return this;
        }

        public void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 position, IReadOnlySet<Direction> visibleFaces) =>
            _sidedMesh.AddToBuilder(builder, position, visibleFaces);
        public void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 position) =>
            _sidedMesh.AddToBuilder(builder, position);

        // thread safe if builder is not being changed during execution
        public void AddAsCollisionMesh(ModelBuilder modelBuilder, IReadOnlySet<Direction> facesToAdd, Vector3 offset = default) =>
            _sidedMesh.AddAsCollisionMesh(modelBuilder, facesToAdd, offset);

        // thread safe if builder is not being changed during execution
        public void AddAsCollisionMesh(ModelBuilder modelBuilder, Vector3 offset = default) =>
            _sidedMesh.AddAsCollisionMesh(modelBuilder, offset);


        public SidedMesh<V> Build() => new(_sidedMesh._sidedElements, _sidedMesh._notSidedElements, Bounds);
    }
}
