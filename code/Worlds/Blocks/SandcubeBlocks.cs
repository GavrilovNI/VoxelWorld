using Sandcube.Registries;

namespace Sandcube.Worlds.Blocks;

public sealed class SandcubeBlocks : ModBlocks
{
    private static ModedId MakeId(string blockId) => new(SandcubeGame.ModName, blockId);

    public SimpleBlock Air { get; private set; } = new(MakeId("air"), new Rect(0, 0));
    public SimpleBlock Stone { get; private set; } = new(MakeId("stone"));
    public SimpleBlock Dirt { get; private set; } = new(MakeId("dirt"));
    public SimpleBlock Cobblestone { get; private set; } = new(MakeId("cobblestone"));
    public SimpleBlock Glass { get; private set; } = new(MakeId("glass"));
}
