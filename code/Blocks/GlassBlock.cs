using VoxelWorld.Blocks.Properties;
using VoxelWorld.Blocks.States;
using VoxelWorld.Meshing.Blocks;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Registries;
using VoxelWorld.Texturing;
using System.Collections.Generic;

namespace VoxelWorld.Blocks;

public class GlassBlock : SimpleBlock
{
    public GlassBlock(in ModedId id, IUvProvider uvProvider) : base(id, uvProvider)
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        };
    }

    public GlassBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id, uvProviders)
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        };
    }

    public override bool ShouldAddFace(BlockState blockState, BlockMeshType meshType, Direction direction,
        BlockState neighborBlockState) => neighborBlockState.Block != this;
}
