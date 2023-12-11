using Sandcube.Blocks.States;
using Sandcube.Mth.Enums;
using System.Collections.Generic;

namespace Sandcube.Worlds.Generation.Meshes;

public class BlockMeshMap
{
    private readonly Dictionary<BlockState, ISidedMeshPart<ComplexVertex>> _visualMeshes = new();
    private readonly Dictionary<BlockState, ISidedMeshPart<Vector3Vertex>> _physicsMeshes = new();

    public void Clear()
    {
        _visualMeshes.Clear();
        _physicsMeshes.Clear();
    }

    public void Add(BlockState blockState)
    {
        var block = blockState.Block;

        _visualMeshes.Add(blockState, block.CreateVisualMesh(blockState));
        _physicsMeshes.Add(blockState, block.CreatePhysicsMesh(blockState));
    }

    public void AddVisualToMeshBuilder(BlockState blockState, ComplexMeshBuilder builder, Vector3 position, HashSet<Direction> visibleFaces) =>
        _visualMeshes[blockState].AddToBuilder(builder, position, visibleFaces);

    public void AddPhysicsToMeshBuilder(BlockState blockState, PositionOnlyMeshBuilder builder, Vector3 position, HashSet<Direction> visibleFaces) =>
        _physicsMeshes[blockState].AddToBuilder(builder, position, visibleFaces);
}
