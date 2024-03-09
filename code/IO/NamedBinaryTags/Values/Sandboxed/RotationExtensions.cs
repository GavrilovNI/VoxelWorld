using Sandcube.IO.NamedBinaryTags.Collections;
using System;

namespace Sandcube.IO.NamedBinaryTags.Values.Sandboxed;

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

    public static void Add(this ListTag collection, Rotation value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Rotation value) =>
        collection.Insert(index, value.Write());


    public static Rotation Get<T>(this CompoundTag collection, string key) where T : IEquatable<Rotation> =>
        collection.GetTag(key).To<ListTag>().To<T>();

    public static Rotation Get<T>(this ListTag collection, int index) where T : IEquatable<Rotation> =>
        collection.GetTag(index).To<ListTag>().To<T>();

    public static Rotation To<T>(this ListTag tag) where T : IEquatable<Rotation> =>
        new(tag.Get<float>(0), tag.Get<float>(1), tag.Get<float>(2), tag.Get<float>(3));
}
