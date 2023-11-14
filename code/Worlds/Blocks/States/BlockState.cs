﻿

using Sandcube.Mth;
using Sandcube.Worlds.Blocks.States.Properties;
using System.Collections.Generic;

namespace Sandcube.Worlds.Blocks.States;

public sealed class BlockState
{
    internal readonly Dictionary<BlockProperty, CustomEnum> _properties;
    private readonly Dictionary<(BlockProperty, CustomEnum), BlockState> _neighbors;
    public readonly Block Block;

    internal BlockState(Block block, Dictionary<BlockProperty, CustomEnum> properties)
    {
        Block = block;
        _properties = properties;
        _neighbors = new();
    }

    public bool IsAir() => Block == SandcubeGame.Instance!.Blocks.Air;

    public override string ToString() => $"{nameof(BlockState)}({Block})";

    public BlockState With(BlockProperty property, CustomEnum value)
    {
        var key = (property, value);
        if(!_neighbors.TryGetValue(key, out var state))
        {
            Dictionary<BlockProperty, CustomEnum> properties = new(_properties)
            {
                [property] = value
            };
            state = Block.BlockStateSet.FindBlockState(properties);
            _neighbors.Add(key, state);
        }

        return state;
    }

    public CustomEnum GetValue(BlockProperty blockProperty) => _properties[blockProperty];

    public T GetValue<T>(BlockProperty<T> blockProperty) where T : CustomEnum<T>, ICustomEnum<T> =>
        (GetValue((BlockProperty)blockProperty) as T)!;
}