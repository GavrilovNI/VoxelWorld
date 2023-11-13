using Sandcube.Mth;
using Sandcube.Worlds.Blocks;
using System.Collections.Generic;

namespace Sandcube.Worlds.Generation;

public class BlockMeshMap
{
    private readonly Dictionary<Block, VoxelMesh> _meshes = new();

    public void Clear() => _meshes.Clear();
    public void Add(Block block) => _meshes.Add(block, block.CreateMesh());
    public void BuildAt(VoxelMeshBuilder builder, Block block, Vector3 position, HashSet<Direction> visibleFaces) => _meshes[block].BuildAt(builder, position, visibleFaces);
}
