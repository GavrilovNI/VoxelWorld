﻿using Sandbox;
using Sandcube.Registries;

namespace Sandcube.Blocks;

public sealed class SandcubeBlocks : ModRegisterables<Block>
{
    private static ModedId MakeId(string blockId) => new(SandcubeGame.ModName, blockId);

    public SimpleBlock Air { get; } = new(MakeId("air"), new Rect(0, 0));
    public SimpleBlock Stone { get; } = new(MakeId("stone"));
    public SlabBlock StoneSlab { get; } = new(MakeId("stone_slab"), Texture.Load(FileSystem.Mounted, $"textures/{SandcubeGame.ModName}/blocks/stone.png", true));
    public SimpleBlock Dirt { get; } = new(MakeId("dirt"));
    public SimpleBlock Cobblestone { get; } = new(MakeId("cobblestone"));
    public SimpleBlock Glass { get; } = new(MakeId("glass"));
}