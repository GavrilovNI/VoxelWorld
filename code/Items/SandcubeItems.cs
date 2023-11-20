using Sandcube.Registries;

namespace Sandcube.Items;

public sealed class SandcubeItems : ModItems
{
    private static ModedId MakeId(string itemId) => new(SandcubeGame.ModName, itemId);

    [AutoBlockItem(SandcubeGame.ModName)]
    public BlockItem Stone { get; private set; } = null!;

    [AutoBlockItem(SandcubeGame.ModName)]
    public BlockItem StoneSlab { get; private set; } = null!;

    [AutoBlockItem(SandcubeGame.ModName)]
    public BlockItem Dirt { get; private set; } = null!;

    [AutoBlockItem(SandcubeGame.ModName)]
    public BlockItem Cobblestone { get; private set; } = null!;

    [AutoBlockItem(SandcubeGame.ModName)]
    public BlockItem Glass { get; private set; } = null!;
}
