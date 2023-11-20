using Sandcube.Blocks;
using Sandcube.Items;
using Sandcube.Registries;

namespace Sandcube;

public interface ISandcubeMod
{
    public Id Id { get; }

    public void RegisterBlocks(Registry<Block> registry);
    public void RegisterItems(Registry<Item> registry);
}
