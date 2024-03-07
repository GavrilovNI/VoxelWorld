using Sandcube.IO.NamedBinaryTags.Collections;
using System;

namespace Sandcube.IO.NamedBinaryTags.Values.Sandboxed;

public static class AnglesExtensions
{
    public static CompoundTag Write(this Angles value)
    {
        var result = new CompoundTag();
        result.Set("pitch", value.pitch);
        result.Set("yaw", value.yaw);
        result.Set("roll", value.roll);
        return result;
    }


    public static void Set(this CompoundTag collection, string key, Angles value) =>
        collection.Set(key, value.Write());

    public static void Add(this ListTag collection, Angles value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Angles value) =>
        collection.Insert(index, value.Write());


    public static Angles Get<T>(this CompoundTag collection, string key) where T : IEquatable<Angles> =>
        collection.GetTag(key).To<CompoundTag>().To<T>();

    public static Angles Get<T>(this ListTag collection, int index) where T : IEquatable<Angles> =>
        collection.GetTag(index).To<CompoundTag>().To<T>();

    public static Angles To<T>(this CompoundTag tag) where T : IEquatable<Angles> =>
        new(tag.Get<float>("pitch"), tag.Get<float>("yaw"), tag.Get<float>("roll"));
}
