using Sandcube.Blocks.Properties;
using Sandcube.Mods;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;

namespace Sandcube.Blocks;

public sealed class SandcubeBlocks : ModRegisterables<Block>
{
    private static string GetBlockPathPart(string blockId) => $"{SandcubeBaseMod.ModName}/blocks/{blockId}";

    private static ModedId MakeId(string blockId) => new(SandcubeBaseMod.ModName, blockId);

    public SimpleBlock Air { get; } = new(MakeId("air"), TextureMapPart.Transparent)
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public SimpleBlock Stone { get; } = new(MakeId("stone"), SandcubeGame.Instance!.BlocksTextureMap.GetOrLoadTexture($"{GetBlockPathPart("stone")}.png"));
    public SlabBlock StoneSlab { get; } = new(MakeId("stone_slab"), SandcubeGame.Instance!.BlocksTextureMap.GetOrLoadTexture($"{GetBlockPathPart("stone")}.png"));
    public SimpleBlock Dirt { get; } = new(MakeId("dirt"), SandcubeGame.Instance!.BlocksTextureMap.GetOrLoadTexture($"{GetBlockPathPart("dirt")}.png"));
    public SimpleBlock Cobblestone { get; } = new(MakeId("cobblestone"), SandcubeGame.Instance!.BlocksTextureMap.GetOrLoadTexture($"{GetBlockPathPart("cobblestone")}.png"));
    public SimpleBlock Glass { get; } = new(MakeId("glass"), SandcubeGame.Instance!.BlocksTextureMap.GetOrLoadTexture($"{GetBlockPathPart("glass")}.png"))
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public PillarBlock WoodLog { get; } = new(MakeId("wood_log"),
        BlockTexturesLoader.SimplePillar.LoadTextureUvs(SandcubeGame.Instance!.BlocksTextureMap, GetBlockPathPart("wood_log")));

    public HorizontalDirectionalBlock Furnace { get; } = new(MakeId("furnace"),
        BlockTexturesLoader.SimplePillar.With(Direction.Forward, BlockTexturesLoader.FrontSuffix)
        .LoadTextureUvs(SandcubeGame.Instance!.BlocksTextureMap, GetBlockPathPart("furnace")));
}
