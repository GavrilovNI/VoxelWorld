using Sandcube.Mth;
using Sandcube.Registries;
using Sandcube.Worlds.Generation;
using System.Collections.Generic;

namespace Sandcube.Worlds.Blocks;

public abstract class Block
{
    public readonly ModedId Id;
    public BlockState DefaultBlockState { get; init; }

    public Block(ModedId id)
    {
        Id = id;
        DefaultBlockState = new BlockState(this);
    }

    public virtual void OnRegistered() { }

    public abstract VoxelMeshBuilder BuildMesh(Vector3Int voxelPosition, Vector3 voxelSize, HashSet<Direction> exceptSides);

    public override string ToString() => $"{nameof(Block)}({Id})";

    public override int GetHashCode() => Id.GetHashCode();
}
