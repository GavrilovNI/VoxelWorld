﻿using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Worlds;

namespace Sandcube.Blocks.Interfaces;

public interface IEntityBlock
{
    public bool HasEntity(IWorldProvider world, Vector3Int position, BlockState blockState);
    public BlockEntity? CreateEntity(IWorldProvider world, Vector3Int position, BlockState blockState);
}