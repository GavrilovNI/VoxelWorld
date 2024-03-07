using Sandcube.IO.NamedBinaryTags.Collections;
using System;

namespace Sandcube.IO.NamedBinaryTags.Values.Sandboxed;

public static class BBoxExtensions
{
    public static CompoundTag Write(this BBox value)
    {
        var result = new CompoundTag();
        result.Set("mins", value.Mins);
        result.Set("maxs", value.Maxs);
        return result;
    }


    public static void Set(this CompoundTag collection, string key, BBox value) =>
        collection.Set(key, value.Write());

    public static void Add(this ListTag collection, BBox value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, BBox value) =>
        collection.Insert(index, value.Write());


    public static BBox Get<T>(this CompoundTag collection, string key) where T : IEquatable<BBox> =>
        collection.GetTag(key).To<CompoundTag>().To<T>();

    public static BBox Get<T>(this ListTag collection, int index) where T : IEquatable<BBox> =>
        collection.GetTag(index).To<CompoundTag>().To<T>();

    public static BBox To<T>(this CompoundTag tag) where T : IEquatable<BBox> =>
        new(tag.Get<Vector3>("mins"), tag.Get<Vector3>("maxs"));
}
