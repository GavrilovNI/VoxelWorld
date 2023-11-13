using Sandcube.Registries;
using Sandcube.Worlds.Generation;

namespace Sandcube.Worlds.Blocks;

public abstract class Block
{
    public readonly ModedId Id;
    public BlockState DefaultBlockState { get; init; }

    public Block(in ModedId id)
    {
        Id = id;
        DefaultBlockState = new BlockState(this);
    }

    public virtual void OnRegistered() { }

    public abstract VoxelMesh CreateMesh();

    public override string ToString() => $"{nameof(Block)}({Id})";

    public override int GetHashCode() => Id.GetHashCode();
}
