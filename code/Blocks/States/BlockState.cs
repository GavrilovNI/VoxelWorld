using Sandcube.Blocks.States.Properties;
using Sandcube.Mods;
using Sandcube.Mth.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandcube.Blocks.States;

public sealed class BlockState
{
    public static BlockState Air => SandcubeBaseMod.Instance!.Blocks.Air.DefaultBlockState;

    internal readonly Dictionary<BlockProperty, CustomEnum> _properties;
    private readonly Dictionary<(BlockProperty, CustomEnum), BlockState> _neighbors;
    public readonly Block Block;

    internal BlockState(Block block, Dictionary<BlockProperty, CustomEnum> properties)
    {
        Block = block;
        _properties = properties;
        _neighbors = new();
    }

    public bool IsAir() => Block.IsAir();

    [Obsolete("Try using With<T>")]
    public BlockState With(BlockProperty property, CustomEnum value)
    {
        if(!_properties.ContainsKey(property))
            throw new ArgumentException($"{nameof(BlockProperty)} {property} is not registered in this {nameof(BlockState)}. this: {this}", nameof(property));
        if(!property.IsValidValue(value))
            throw new ArgumentException($"{value} is not valid type for {property}", nameof(value));

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

#pragma warning disable CS0618 // Type or member is obsolete
    public BlockState With<T>(BlockProperty<T> property, T value) where T : CustomEnum<T>, ICustomEnum<T> =>
        With(property, (CustomEnum)value);
#pragma warning restore CS0618 // Type or member is obsolete

    public CustomEnum GetValue(BlockProperty blockProperty) => _properties[blockProperty];

    public T GetValue<T>(BlockProperty<T> blockProperty) where T : CustomEnum<T>, ICustomEnum<T> =>
        (GetValue((BlockProperty)blockProperty) as T)!;

    public override string ToString()
    {
        StringBuilder builder = new($"Block: \"{Block.ModedId}\"");

        if(_properties.Count > 0)
        {
            builder.Append(", Properties: {");
            foreach(var property in _properties)
                builder.Append($"\"{property.Key.Name}\": \"{property.Value.Name}\"");
            builder.Append("}");
        }

        return builder.ToString();
    }
}
