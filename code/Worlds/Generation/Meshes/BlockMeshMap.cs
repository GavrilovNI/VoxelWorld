using System;
using Sandcube.Blocks.States;
using System.Collections.Concurrent;

namespace Sandcube.Worlds.Generation.Meshes;

// Thread safe
public class BlockMeshMap
{
    private readonly object _lock = new object();
    private readonly ConcurrentDictionary<BlockState, ISidedMeshPart<ComplexVertex>> _visualMeshes = new();
    private readonly ConcurrentDictionary<BlockState, ISidedMeshPart<Vector3Vertex>> _physicsMeshes = new();
    private readonly ConcurrentDictionary<BlockState, ISidedMeshPart<Vector3Vertex>> _interactionMeshes = new();

    public void Clear()
    {
        lock(_lock)
        {
            _visualMeshes.Clear();
            _physicsMeshes.Clear();
            _interactionMeshes.Clear();
        }
    }

    public void Add(BlockState blockState)
    {
        var block = blockState.Block;

        lock(_lock)
        {
            if(!_visualMeshes.TryAdd(blockState, block.CreateVisualMesh(blockState)) ||
                !_physicsMeshes.TryAdd(blockState, block.CreatePhysicsMesh(blockState)) ||
                !_interactionMeshes.TryAdd(blockState, block.CreateInteractionMesh(blockState)))
            {
                throw new InvalidOperationException($"{nameof(BlockState)} {blockState} was already added");
            }
        }
    }

    public ISidedMeshPart<ComplexVertex>? GetVisual(BlockState blockState) => _visualMeshes[blockState];
    public ISidedMeshPart<Vector3Vertex>? GetPhysics(BlockState blockState) => _physicsMeshes[blockState];
    public ISidedMeshPart<Vector3Vertex>? GetInteraction(BlockState blockState) => _interactionMeshes[blockState];
}
