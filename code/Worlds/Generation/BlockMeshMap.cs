using Sandcube.Blocks.States;
using Sandcube.Mth.Enums;
using System.Collections.Generic;

namespace Sandcube.Worlds.Generation;

public class BlockMeshMap
{
    private readonly Dictionary<BlockState, VoxelMesh> _meshes = new();

    public void Clear() => _meshes.Clear();
    public void Add(BlockState blockState) => _meshes.Add(blockState, blockState.Block.CreateMesh(blockState));
    public void BuildAt(VoxelMeshBuilder builder, BlockState blockState, Vector3 position, HashSet<Direction> visibleFaces) => _meshes[blockState].BuildAt(builder, position, visibleFaces);
}
