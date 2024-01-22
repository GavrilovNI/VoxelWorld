using Sandcube.Blocks;
using Sandcube.Items;
using Sandcube.Registries;
using System.Threading.Tasks;

namespace Sandcube.Mods;

public interface ISandcubeMod
{
    public Id Id { get; }

    public Task RegisterBlocks(Registry<Block> registry);
    public Task RegisterItems(Registry<Item> registry);

    public void OnLoaded();
}
