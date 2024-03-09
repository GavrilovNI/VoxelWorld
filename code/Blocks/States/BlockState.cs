using Sandcube.Blocks.States.Properties;
using Sandcube.Mth.Enums;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Sandcube.Mods.Base;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;

namespace Sandcube.Blocks.States;

public sealed class BlockState : INbtWritable, INbtStaticReadable<BlockState>
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

    [Obsolete("Try using Change<T>")]
    public BlockState Change(BlockProperty property, Func<CustomEnum, CustomEnum> newValueAccessor)
    {
        var currentValue = GetValue(property);
        return With(property, newValueAccessor(currentValue));
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public BlockState With<T>(BlockProperty<T> property, T value) where T : CustomEnum<T>, ICustomEnum<T> =>
        With(property, (CustomEnum)value);

    public BlockState Change<T>(BlockProperty<T> property, Func<T, T> newValueAccessor) where T : CustomEnum<T>, ICustomEnum<T>
    {
        var currentValue = GetValue(property);
        return With<T>(property, newValueAccessor(currentValue));
    }
#pragma warning restore CS0618 // Type or member is obsolete

    public CustomEnum GetValue(BlockProperty blockProperty) => _properties[blockProperty];

    public T GetValue<T>(BlockProperty<T> blockProperty) where T : CustomEnum<T>, ICustomEnum<T> =>
        (GetValue((BlockProperty)blockProperty) as T)!;


    public BinaryTag Write()
    {
        CompoundTag result = new();
        result.Set("block", Block);

        ListTag propertiesTag = new(BinaryTagType.Compound);
        result.Set("properties", propertiesTag);

        foreach(var (property, propertyValue) in _properties)
        {
            CompoundTag propertyTag = new();
            propertiesTag.Add(propertyTag);

            propertyTag.Set("id", property.Id);
            propertyTag.Set("value", propertyValue);
        }

        return result;
    }

    public static BlockState Read(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();

        var block = Block.Read(compoundTag.GetTag("block"));
        var blockState = block.DefaultBlockState;

        ListTag propertiesTag = compoundTag.GetTag("properties").To<ListTag>();
        foreach(var propertyBinaryTag in propertiesTag)
        {
            var propertyTag = propertyBinaryTag.To<CompoundTag>();
            var propertyId = Id.Read(propertyTag.GetTag("id"));
            var property = blockState._properties.First(kv => kv.Key.Id == propertyId).Key;

            var propertyValue = CustomEnum.Read(propertyTag.GetTag("value"), property.PropertyType);
#pragma warning disable CS0618 // Type or member is obsolete
            blockState = blockState.With(property, propertyValue);
#pragma warning restore CS0618 // Type or member is obsolete
        }
        return blockState;
    }

    public override string ToString()
    {
        StringBuilder builder = new($"Block: \"{Block.Id}\"");

        if(_properties.Count > 0)
        {
            builder.Append(", Properties: {");
            foreach(var (property, propertyValue) in _properties)
                builder.Append($"\"{property.Id}\": \"{propertyValue.Name}\"");
            builder.Append("}");
        }

        return builder.ToString();
    }
}
