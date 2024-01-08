using Sandcube.Blocks.States;
using System.Collections.Generic;

namespace Sandcube.Worlds.Generation.Meshes;

public class BlockMeshMap
{
    private readonly Dictionary<BlockState, ISidedMeshPart<ComplexVertex>> _visualMeshes = new();
    private readonly Dictionary<BlockState, ISidedMeshPart<Vector3Vertex>> _physicsMeshes = new();
    private readonly Dictionary<BlockState, ISidedMeshPart<Vector3Vertex>> _interactionMeshes = new();

    public void Clear()
    {
        _visualMeshes.Clear();
        _physicsMeshes.Clear();
        _interactionMeshes.Clear();
    }

    public void Add(BlockState blockState)
    {
        var block = blockState.Block;

        _visualMeshes.Add(blockState, block.CreateVisualMesh(blockState));
        _physicsMeshes.Add(blockState, block.CreatePhysicsMesh(blockState));
        _interactionMeshes.Add(blockState, block.CreateInteractionMesh(blockState));
    }

    public ISidedMeshPart<ComplexVertex>? GetVisual(BlockState blockState) => _visualMeshes[blockState];
    public ISidedMeshPart<Vector3Vertex>? GetPhysics(BlockState blockState) => _physicsMeshes[blockState];
    public ISidedMeshPart<Vector3Vertex>? GetInteraction(BlockState blockState) => _interactionMeshes[blockState];
}
