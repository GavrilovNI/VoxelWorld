using Sandcube.Blocks.States;
using Sandcube.Mth.Enums;
using System.Collections.Generic;

namespace Sandcube.Worlds.Generation.Meshes;

public class BlockMeshMap
{
    private readonly Dictionary<BlockState, ISidedMeshPart<ComplexVertex>> _meshes = new();

    public void Clear() => _meshes.Clear();
    public void Add(BlockState blockState) => _meshes.Add(blockState, blockState.Block.CreateMesh(blockState));
    public void AddBlockStateMeshToBuilder(BlockState blockState, ComplexMeshBuilder builder, Vector3 position, HashSet<Direction> visibleFaces) => _meshes[blockState].AddToBuilder(builder, position, visibleFaces);
}
