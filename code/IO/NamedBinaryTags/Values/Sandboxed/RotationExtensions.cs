using VoxelWorld.IO.NamedBinaryTags.Collections;
using System;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;

public static class RotationExtensions
{
    public static ListTag Write(this Rotation value) => new()
    {
        value.x,
        value.y,
        value.z,
        value.w
    };


    public static void Set(this CompoundTag collection, string key, Rotation value) =>
        collection.Set(key, value.Write());

    public static void Set(this ListTag collection, int index, Rotation value) =>
        collection.Set(index, value.Write());

    public static void Add(this ListTag collection, Rotation value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Rotation value) =>
        collection.Insert(index, value.Write());


    public static Rotation Get<T>(this CompoundTag collection, string key, Rotation defaultValue = default) where T : IEquatable<Rotation>
    {
        var tag = collection.GetTagOrNull(key);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<Rotation>();
    }

    public static Rotation Get<T>(this ListTag collection, int index, Rotation defaultValue = default) where T : IEquatable<Rotation>
    {
        var tag = collection.GetTagOrNull(index);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<Rotation>();
    }

    public static Rotation To<T>(this ListTag tag) where T : IEquatable<Rotation> =>
        new(tag.Get<float>(0), tag.Get<float>(1), tag.Get<float>(2), tag.Get<float>(3));
}
