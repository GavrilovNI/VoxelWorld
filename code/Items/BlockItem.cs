using Sandbox;
using Sandcube.Mth;
using Sandcube.Players;
using Sandcube.Worlds.Blocks;

namespace Sandcube.Items;

public class BlockItem : Item
{
    public readonly Block Block;

    public BlockItem(Block block) : base(block.ModedId)
    {
        Block = block;
    }
}
