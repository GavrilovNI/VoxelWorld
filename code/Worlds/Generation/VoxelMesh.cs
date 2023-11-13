using Sandcube.Mth;
using System.Collections.Generic;

namespace Sandcube.Worlds.Generation;

public class VoxelMesh
{
    private readonly Dictionary<Direction, VoxelMeshBuilder> _sidedElements = new();
    private readonly VoxelMeshBuilder _notSidedElements = new();

    public VoxelMesh()
    {

    }

    private VoxelMeshBuilder GetOrCreateSidedBuilder(Direction direction)
    {
        if(!_sidedElements.TryGetValue(direction, out var builder))
        {
            builder = new();
            _sidedElements[direction] = builder;
        }
        return builder!;
    }

    public void AddElement(VoxelMeshBuilder voxelMeshBuilder) => _notSidedElements.AddMeshBuilder(voxelMeshBuilder);
    public void AddElement(VoxelMeshBuilder voxelMeshBuilder, Direction cullFace) => GetOrCreateSidedBuilder(cullFace).AddMeshBuilder(voxelMeshBuilder);


    public void BuildAt(VoxelMeshBuilder builder, Vector3 position, HashSet<Direction> visibleFaces)
    {
        builder.AddMeshBuilder(_notSidedElements, position);

        foreach(var face in visibleFaces)
        {
            if(_sidedElements.TryGetValue(face, out var element))
                builder.AddMeshBuilder(element, position);
        }
    }
}
