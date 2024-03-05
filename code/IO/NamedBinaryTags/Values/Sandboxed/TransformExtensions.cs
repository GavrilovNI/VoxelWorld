using Sandcube.IO.NamedBinaryTags.Collections;
using System;

namespace Sandcube.IO.NamedBinaryTags.Values.Sandboxed;

public static class TransformExtensions
{
    public static CompoundTag Write(this Transform value)
    {
        var result = new CompoundTag();
        result.Set("position", value.Position);
        result.Set("rotation", value.Rotation);
        result.Set("scale", value.Scale);
        return result;
    }


    public static void Set(this CompoundTag collection, string key, Transform value) =>
        collection.Set(key, value.Write());

    public static void Add(this ListTag collection, Transform value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Transform value) =>
        collection.Insert(index, value.Write());


    public static Transform Get<T>(this CompoundTag collection, string key) where T : IEquatable<Transform> =>
        ((CompoundTag)collection.GetTag(key)).To<T>();

    public static Transform Get<T>(this ListTag collection, int index) where T : IEquatable<Transform> =>
        ((CompoundTag)collection.GetTag(index)).To<T>();

    public static Transform To<T>(this CompoundTag tag) where T : IEquatable<Transform> =>
        new(tag.Get<Vector3>("position"), tag.Get<Rotation>("rotation"), tag.Get<Vector3>("scale"));
}
