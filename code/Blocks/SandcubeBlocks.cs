using Sandbox;
using Sandcube.Blocks.Properties;
using Sandcube.Mods;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using Sandcube.Texturing;
using System.Collections.Generic;

namespace Sandcube.Blocks;

public sealed class SandcubeBlocks : ModRegisterables<Block>
{
    private static PathedTextureMap _textureMap = SandcubeGame.Instance!.BlocksTextureMap;

    private static ModedId MakeId(string blockId) => new(SandcubeBaseMod.ModName, blockId);

    public SimpleBlock Air { get; } = new(MakeId("air"), TextureMapPart.Transparent)
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public SimpleBlock Stone { get; } = new(MakeId("stone"));
    public SlabBlock StoneSlab { get; } = new(MakeId("stone_slab"), _textureMap.GetOrLoadTexture($"{Block.GetBlockPathPart(MakeId("stone"))}.png"));
    public SimpleBlock Dirt { get; } = new(MakeId("dirt"));
    public SimpleBlock Cobblestone { get; } = new(MakeId("cobblestone"));
    public SimpleBlock Glass { get; } = new(MakeId("glass"))
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public PillarBlock WoodLog { get; } = new(MakeId("wood_log"), true);

    public HorizontalDirectionalBlock Furnace { get; } = new(MakeId("furnace"), new Dictionary<Direction, TextureMapPart>()
    {
        { Direction.Forward, _textureMap.GetOrLoadTexture($"{Block.GetBlockPathPart(MakeId("furnace"))}_front.png") },
        { Direction.Backward, _textureMap.GetOrLoadTexture($"{Block.GetBlockPathPart(MakeId("furnace"))}_side.png") },
        { Direction.Left, _textureMap.GetOrLoadTexture($"{Block.GetBlockPathPart(MakeId("furnace"))}_side.png") },
        { Direction.Right, _textureMap.GetOrLoadTexture($"{Block.GetBlockPathPart(MakeId("furnace"))}_side.png") },
        { Direction.Up, _textureMap.GetOrLoadTexture($"{Block.GetBlockPathPart(MakeId("furnace"))}_top.png") },
        { Direction.Down, _textureMap.GetOrLoadTexture($"{Block.GetBlockPathPart(MakeId("furnace"))}_top.png") }
    });
}
