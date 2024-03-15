using System;
using VoxelWorld.Blocks.States;
using System.Collections.Generic;

namespace VoxelWorld.Meshing.Blocks;

// Thread safe
public class BlockMeshMap
{
    private readonly object _lock = new object();
    private readonly Dictionary<BlockState, ISidedMeshPart<ComplexVertex>> _visualMeshes = new();
    private readonly Dictionary<BlockState, ISidedMeshPart<Vector3Vertex>> _physicsMeshes = new();
    private readonly Dictionary<BlockState, ISidedMeshPart<Vector3Vertex>> _interactionMeshes = new();

    public void Clear()
    {
        lock(_lock)
        {
            _visualMeshes.Clear();
            _physicsMeshes.Clear();
            _interactionMeshes.Clear();
        }
    }

    public void Update(BlockState blockState)
    {
        var block = blockState.Block;

        var visual = block.CreateVisualMesh(blockState);
        var physics = block.CreatePhysicsMesh(blockState);
        var interaction = block.CreateInteractionMesh(blockState);

        lock(_lock)
        {
            _visualMeshes[blockState] = visual;
            _physicsMeshes[blockState] = physics;
            _interactionMeshes[blockState] = interaction;
        }
    }

    public ISidedMeshPart<ComplexVertex>? GetVisual(BlockState blockState)
    {
        lock(_lock)
        {
            return _visualMeshes[blockState];
        }
    }

    public ISidedMeshPart<Vector3Vertex>? GetPhysics(BlockState blockState)
    {
        lock(_lock)
        {
            return _physicsMeshes[blockState];
        }
    }

    public ISidedMeshPart<Vector3Vertex>? GetInteraction(BlockState blockState)
    {
        lock(_lock)
        {
            return _interactionMeshes[blockState];
        }
    }
}
