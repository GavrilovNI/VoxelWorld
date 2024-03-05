using Sandcube.IO.NamedBinaryTags.Collections;
using System;

namespace Sandcube.IO.NamedBinaryTags.Values.Sandboxed;

public static class RotationExtensions
{
    public static CompoundTag Write(this Rotation value)
    {
        var result = new CompoundTag();
        result.Set("x", value.x);
        result.Set("y", value.y);
        result.Set("z", value.z);
        result.Set("w", value.w);
        return result;
    }


    public static void Set(this CompoundTag collection, string key, Rotation value) =>
        collection.Set(key, value.Write());

    public static void Add(this ListTag collection, Rotation value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Rotation value) =>
        collection.Insert(index, value.Write());


    public static Rotation Get<T>(this CompoundTag collection, string key) where T : IEquatable<Rotation> =>
        ((CompoundTag)collection.GetTag(key)).To<T>();

    public static Rotation Get<T>(this ListTag collection, int index) where T : IEquatable<Rotation> =>
        ((CompoundTag)collection.GetTag(index)).To<T>();

    public static Rotation To<T>(this CompoundTag tag) where T : IEquatable<Rotation> =>
        new(tag.Get<float>("x"), tag.Get<float>("y"), tag.Get<float>("z"), tag.Get<float>("w"));
}
