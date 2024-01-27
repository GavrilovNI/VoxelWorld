using Sandcube.Blocks.Properties;
using Sandcube.Mods;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;

namespace Sandcube.Blocks;

public sealed class SandcubeBlocks : ModRegisterables<Block>
{
    private static readonly PathedTextureMap _textureMap = SandcubeGame.Instance!.BlocksTextureMap;

    private static string GetBlockPathPart(string blockId) => $"{SandcubeBaseMod.ModName}/blocks/{blockId}";

    private static ModedId MakeId(string blockId) => new(SandcubeBaseMod.ModName, blockId);

    public SimpleBlock Air { get; } = new(MakeId("air"), TextureMapPart.Transparent)
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public SimpleBlock Stone { get; } = new(MakeId("stone"), _textureMap.GetOrLoadTexture($"{GetBlockPathPart("stone")}.png"));
    public SlabBlock StoneSlab { get; } = new(MakeId("stone_slab"), _textureMap.GetOrLoadTexture($"{GetBlockPathPart("stone")}.png"));
    public SimpleBlock Dirt { get; } = new(MakeId("dirt"), _textureMap.GetOrLoadTexture($"{GetBlockPathPart("dirt")}.png"));
    public SimpleBlock Cobblestone { get; } = new(MakeId("cobblestone"), _textureMap.GetOrLoadTexture($"{GetBlockPathPart("cobblestone")}.png"));
    public SimpleBlock Glass { get; } = new(MakeId("glass"), _textureMap.GetOrLoadTexture($"{GetBlockPathPart("glass")}.png"))
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public PillarBlock WoodLog { get; } = new(MakeId("wood_log"),
        BlockTexturesLoader.SimplePillar.LoadTextureUvs(_textureMap, GetBlockPathPart("wood_log")));

    public HorizontalDirectionalBlock Furnace { get; } = new(MakeId("furnace"),
        BlockTexturesLoader.SimplePillar.With(Direction.Forward, BlockTexturesLoader.FrontSuffix)
        .LoadTextureUvs(_textureMap, GetBlockPathPart("furnace")));
}
