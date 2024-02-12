using Sandcube.Blocks.Properties;
using Sandcube.Mods;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;

namespace Sandcube.Blocks;

public sealed class SandcubeBlocks : ModRegisterables<Block>
{
    private static PathedTextureMap TextureMap => SandcubeGame.Instance!.BlocksTextureMap;

    private static string GetBlockPathPart(string blockId) => $"{SandcubeBaseMod.ModName}/blocks/{blockId}";

    private static ModedId MakeId(string blockId) => new(SandcubeBaseMod.ModName, blockId);


    public SimpleBlock Air { get; } = new(MakeId("air"), TextureMap.Transparent)
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public SimpleBlock Stone { get; } = new(MakeId("stone"), TextureMap.GetOrLoadTexture($"{GetBlockPathPart("stone")}.png"));
    public SlabBlock StoneSlab { get; } = new(MakeId("stone_slab"), TextureMap.GetOrLoadTexture($"{GetBlockPathPart("stone")}.png"));
    public SimpleBlock Dirt { get; } = new(MakeId("dirt"), TextureMap.GetOrLoadTexture($"{GetBlockPathPart("dirt")}.png"));
    public SimpleBlock Cobblestone { get; } = new(MakeId("cobblestone"), TextureMap.GetOrLoadTexture($"{GetBlockPathPart("cobblestone")}.png"));
    public SimpleBlock Glass { get; } = new(MakeId("glass"), TextureMap.GetOrLoadTexture($"{GetBlockPathPart("glass")}.png"))
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public PillarBlock WoodLog { get; } = new(MakeId("wood_log"),
        BlockTexturesLoader.SimplePillar.LoadTextureUvs(TextureMap, GetBlockPathPart("wood_log")));

    public HorizontalDirectionalBlock Furnace { get; } = new(MakeId("furnace"),
        BlockTexturesLoader.SimplePillar.With(Direction.Forward, BlockTexturesLoader.FrontSuffix)
        .LoadTextureUvs(TextureMap, GetBlockPathPart("furnace")));

    public XShapedBlock TallGrass { get; } = new(MakeId("tall_grass"),
        TextureMap.GetOrLoadAnimatedTexture($"{GetBlockPathPart("tall_grass")}.png", new Mth.Vector2Int(2, 1), new float[2] { 1f, 1f }))
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public ItemStorageBlock Barrel { get; } = new(MakeId("barrel"),
        BlockTexturesLoader.SimplePillar.LoadTextureUvs(TextureMap, GetBlockPathPart("barrel")),
        18);

    public DoorBlock Door { get; } = new(MakeId("door"),
        BlockTexturesLoader.BottomDoor.LoadTextureUvs(TextureMap, GetBlockPathPart("door")),
        BlockTexturesLoader.TopDoor.LoadTextureUvs(TextureMap, GetBlockPathPart("door")));
}
