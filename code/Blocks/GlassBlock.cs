using Sandcube.Blocks.Properties;
using Sandcube.Blocks.States;
using Sandcube.Meshing.Blocks;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sandcube.Blocks;

public class GlassBlock : SimpleBlock
{
    [SetsRequiredMembers]
    public GlassBlock(in ModedId id, IUvProvider uvProvider) : base(id, uvProvider)
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        };
    }

    [SetsRequiredMembers]
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
