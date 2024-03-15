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

    public static void Add(this ListTag collection, Angles value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Angles value) =>
        collection.Insert(index, value.Write());


    public static Angles Get<T>(this CompoundTag collection, string key) where T : IEquatable<Angles> =>
        collection.GetTag(key).To<ListTag>().To<T>();

    public static Angles Get<T>(this ListTag collection, int index) where T : IEquatable<Angles> =>
        collection.GetTag(index).To<ListTag>().To<T>();

    public static Angles To<T>(this ListTag tag) where T : IEquatable<Angles> =>
        new(tag.Get<float>(0), tag.Get<float>(1), tag.Get<float>(2));
}
