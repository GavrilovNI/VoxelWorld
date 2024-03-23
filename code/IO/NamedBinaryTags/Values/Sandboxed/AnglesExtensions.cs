using VoxelWorld.IO.NamedBinaryTags.Collections;
using System;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;

public static class AnglesExtensions
{
    public static ListTag Write(this Angles value) => new()
    {
        value.pitch,
        value.yaw,
        value.roll
    };


    public static void Set(this CompoundTag collection, string key, Angles value) =>
        collection.Set(key, value.Write());

    public static void Set(this ListTag collection, int index, Angles value) =>
        collection.Set(index, value.Write());

    public static void Add(this ListTag collection, Angles value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Angles value) =>
        collection.Insert(index, value.Write());


    public static Angles Get<T>(this CompoundTag collection, string key, Angles defaultValue = default) where T : IEquatable<Angles>
    {
        var tag = collection.GetTagOrNull(key);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<Angles>();
    }

    public static Angles Get<T>(this ListTag collection, int index, Angles defaultValue = default) where T : IEquatable<Angles>
    {
        var tag = collection.GetTagOrNull(index);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<Angles>();
    }

    public static Angles To<T>(this ListTag tag) where T : IEquatable<Angles> =>
        new(tag.Get<float>(0), tag.Get<float>(1), tag.Get<float>(2));
}
