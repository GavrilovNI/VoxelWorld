using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VoxelWorld.Blocks.States;
using VoxelWorld.Mods.Base;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Registries;
using VoxelWorld.Texturing;
using VoxelWorld.Worlds;

namespace VoxelWorld.Blocks;

public class GrassBlock : SimpleBlock
{
    public BBoxInt SpreadingRange { get; init; } = new BBoxInt(-Vector3IntB.One, Vector3IntB.One);
    public int SpreadingSpeed { get; init; } = 3;

    public GrassBlock(in ModedId id, IUvProvider uvProvider) : base(id, uvProvider)
    {
    }

    public GrassBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id, uvProviders)
    {
    }

    protected virtual bool CanStayGrass(IWorldAccessor world, Vector3IntB position) =>
        world.GetBlockState(position + Vector3IntB.Up).IsAir();

    protected virtual bool TrySpread(IWorldAccessor world, Vector3IntB spreadPosition)
    {
        if(!world.IsLoadedAt(spreadPosition))
            return false;

        var spreadBlockState = world.GetBlockState(spreadPosition);
        bool canSpread = spreadBlockState.Block == BaseMod.Instance!.Blocks.Dirt && CanStayGrass(world, spreadPosition);

        if(canSpread)
            _ = world.SetBlockState(spreadPosition, DefaultBlockState);

        return canSpread;
    }

    public override void TickRandom(IWorldAccessor world, Vector3IntB position, BlockState blockState)
    {
        if(!CanStayGrass(world, position))
        {
            _ = world.SetBlockState(position, BaseMod.Instance!.Blocks.Dirt.DefaultBlockState);
            return;
        }

        for(int i = 0; i < SpreadingSpeed; ++i)
        {
            var spreadPosition = position + SpreadingRange.GetRandomIntPointInside(world.Random);
            TrySpread(world, spreadPosition);
        }
    }
}
