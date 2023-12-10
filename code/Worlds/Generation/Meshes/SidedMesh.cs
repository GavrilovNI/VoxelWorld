using Sandcube.Mth.Enums;
using System.Collections.Generic;

namespace Sandcube.Worlds.Generation.Meshes;

public sealed class SidedMesh<V> : ISidedMeshPart<V> where V : unmanaged, IVertex
{
    private readonly Dictionary<Direction, UnlimitedMesh<V>.Builder> _sidedElements = new();
    private readonly UnlimitedMesh<V>.Builder _notSidedElements = new();

    public SidedMesh()
    {

    }

    public SidedMesh(Dictionary<Direction, UnlimitedMesh<V>.Builder> sidedElements, UnlimitedMesh<V>.Builder notSidedElements)
    {
        foreach(var sidedElementEntry in sidedElements)
            _sidedElements[sidedElementEntry.Key] = new UnlimitedMesh<V>.Builder().Add(sidedElementEntry.Value);
        _notSidedElements = new UnlimitedMesh<V>.Builder().Add(notSidedElements);
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


    public class Builder : ISidedMeshPart<V>
    {
        private SidedMesh<V> _sidedMesh = new();

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
            return this;
        }

        public Builder Add(UnlimitedMesh<V>.Builder voxelMeshBuilder, Direction cullFace)
        {
            GetOrCreateSidedBuilder(cullFace).Add(voxelMeshBuilder);
            return this;
        }

        public void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 position, IReadOnlySet<Direction> visibleFaces) =>
            _sidedMesh.AddToBuilder(builder, position, visibleFaces);
        public void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 position) =>
            _sidedMesh.AddToBuilder(builder, position);

        public SidedMesh<V> Build() => new(_sidedMesh._sidedElements, _sidedMesh._notSidedElements);
    }
}
