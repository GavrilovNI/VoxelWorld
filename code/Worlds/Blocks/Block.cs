using Sandcube.Registries;
using Sandcube.Worlds.Blocks.States;
using Sandcube.Worlds.Blocks.States.Properties;
using Sandcube.Worlds.Generation;
using System.Collections.Generic;

namespace Sandcube.Worlds.Blocks;

public abstract class Block : IRegisterable
{
    public ModedId ModedId { get; }
    public readonly BlockState DefaultBlockState;
    public readonly BlockStateSet BlockStateSet;

    public Block(in ModedId id)
    {
        ModedId = id;
        BlockStateSet = new BlockStateSet(this, CombineProperties());
        DefaultBlockState = CreateDefaultBlockState(BlockStateSet.First());
    }

    public virtual void OnRegistered() { }

    public virtual bool IsFullBlock(BlockState blockState) => !blockState.IsAir();
    public virtual IEnumerable<BlockProperty> CombineProperties() => Enumerable.Empty<BlockProperty>();
    public virtual BlockState CreateDefaultBlockState(BlockState blockState) => blockState;

    public abstract VoxelMesh CreateMesh(BlockState blockState);

    public override string ToString() => $"{nameof(Block)}({ModedId})";

    public override int GetHashCode() => ModedId.GetHashCode();
}
