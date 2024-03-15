using Sandcube.Blocks;
using Sandcube.Blocks.Properties;
using Sandcube.Mods.Base;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;
using System.Linq;

namespace Sandcube.Mods.Base.Blocks;

public sealed class BaseModBlocks : ModRegisterables<Block>
{
    private static PathedTextureMap TextureMap => SandcubeGame.Instance!.BlocksTextureMap;

    private static string GetBlockPathPart(string blockId) => $"{BaseMod.ModName}/blocks/{blockId}";

    private static ModedId MakeId(string blockId) => new(BaseMod.ModName, blockId);


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
    public SimpleBlock Grass { get; } = new(MakeId("grass"),
        BlockTexturesLoader.Pillar.WithTexture(Direction.Down, $"{GetBlockPathPart("dirt")}.png").LoadTextureUvs(TextureMap, GetBlockPathPart("grass")));
    public SimpleBlock Cobblestone { get; } = new(MakeId("cobblestone"), TextureMap.GetOrLoadTexture($"{GetBlockPathPart("cobblestone")}.png"));
    public GlassBlock Glass { get; } = new(MakeId("glass"), TextureMap.GetOrLoadTexture($"{GetBlockPathPart("glass")}.png"));

    public PillarBlock WoodLog { get; } = new(MakeId("wood_log"),
        BlockTexturesLoader.SimplePillar.LoadTextureUvs(TextureMap, GetBlockPathPart("wood_log")));
    public SimpleBlock TreeLeaves { get; } = new(MakeId("tree_leaves"), TextureMap.GetOrLoadTexture($"{GetBlockPathPart("tree_leaves")}.png"))
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public HorizontalDirectionalBlock Furnace { get; } = new(MakeId("furnace"),
        BlockTexturesLoader.SimplePillar.With(Direction.Forward, BlockTexturesLoader.FrontSuffix)
        .LoadTextureUvs(TextureMap, GetBlockPathPart("furnace")));

    public BushBlock TallGrass { get; } = new(MakeId("tall_grass"),
        TextureMap.GetOrLoadAnimatedTexture($"{GetBlockPathPart("tall_grass")}.png", new Mth.Vector2Int(2, 1), new float[2] { 1f, 1f }))
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public BarrelBlock Barrel { get; } = new(MakeId("barrel"), BlockTexturesLoader.SimplePillar.LoadTextureUvs(TextureMap, GetBlockPathPart("barrel")));

    public DoorBlock Door { get; } = new(MakeId("door"),
        BlockTexturesLoader.AllSides.LoadTextureUvs(TextureMap, GetBlockPathPart("door_bottom"), Direction.AllSet.Except(new[] { Direction.Up }).ToHashSet()),
        BlockTexturesLoader.AllSides.LoadTextureUvs(TextureMap, GetBlockPathPart("door_top"), Direction.AllSet.Except(new[] { Direction.Down }).ToHashSet()));

    public PhysicsBlock Sand { get; } = new(MakeId("sand"), TextureMap.GetOrLoadTexture($"{GetBlockPathPart("sand")}.png"));

}
