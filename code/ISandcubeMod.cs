﻿using Sandcube.Worlds.Blocks;

namespace Sandcube;

public interface ISandcubeMod
{
    public Id Id { get; }

    public void RegsiterBlocks(BlocksRegistry registry);
}