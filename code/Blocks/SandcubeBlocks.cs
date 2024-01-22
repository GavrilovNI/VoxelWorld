using Sandbox;
using Sandcube.Blocks.Properties;
using Sandcube.Mth.Enums;
using Sandcube.Registries;
using System.Collections.Generic;

namespace Sandcube.Blocks;

public sealed class SandcubeBlocks : ModRegisterables<Block>
{
    private static ModedId MakeId(string blockId) => new(SandcubeGame.ModName, blockId);

    public SimpleBlock Air { get; } = new(MakeId("air"), new Rect(0, 0))
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public SimpleBlock Stone { get; } = new(MakeId("stone"));
    public SlabBlock StoneSlab { get; } = new(MakeId("stone_slab"), Texture.Load(FileSystem.Mounted, $"textures/{SandcubeGame.ModName}/blocks/stone.png", true) ?? Texture.Invalid);
    public SimpleBlock Dirt { get; } = new(MakeId("dirt"));
    public SimpleBlock Cobblestone { get; } = new(MakeId("cobblestone"));
    public SimpleBlock Glass { get; } = new(MakeId("glass"))
    {
        Properties = BlockProperties.Default with
        {
            IsTransparent = true,
        }
    };

    public PillarBlock WoodLog { get; } = new(MakeId("wood_log"),
        Texture.Load(FileSystem.Mounted, $"textures/{SandcubeGame.ModName}/blocks/wood_log_side.png", true),
        Texture.Load(FileSystem.Mounted, $"textures/{SandcubeGame.ModName}/blocks/wood_log_top.png", true));


    public HorizontalDirectionalBlock Furnace { get; } = new(MakeId("furnace"), new Dictionary<Direction, Texture>()
    {
        { Direction.Forward, Texture.Load(FileSystem.Mounted, $"textures/{SandcubeGame.ModName}/blocks/furnace_front.png", true) ?? Texture.Invalid },
        { Direction.Up, Texture.Load(FileSystem.Mounted, $"textures/{SandcubeGame.ModName}/blocks/furnace_top.png", true) ?? Texture.Invalid},
        { Direction.Down, Texture.Load(FileSystem.Mounted, $"textures/{SandcubeGame.ModName}/blocks/furnace_top.png", true) ?? Texture.Invalid},
        { Direction.Left, Texture.Load(FileSystem.Mounted, $"textures/{SandcubeGame.ModName}/blocks/furnace_side.png", true) ?? Texture.Invalid},
        { Direction.Right, Texture.Load(FileSystem.Mounted, $"textures/{SandcubeGame.ModName}/blocks/furnace_side.png", true) ?? Texture.Invalid},
        { Direction.Backward, Texture.Load(FileSystem.Mounted, $"textures/{SandcubeGame.ModName}/blocks/furnace_side.png", true) ?? Texture.Invalid},
    });
}
