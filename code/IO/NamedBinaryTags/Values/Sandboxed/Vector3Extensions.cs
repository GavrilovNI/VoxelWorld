using Sandcube.IO.NamedBinaryTags.Collections;
using System;

namespace Sandcube.IO.NamedBinaryTags.Values.Sandboxed;

public static class Vector3Extensions
{
    public static CompoundTag Write(this Vector3 value)
    {
        var result = new CompoundTag();
        result.Set("x", value.x);
        result.Set("y", value.y);
        result.Set("z", value.z);
        return result;
    }


    public static void Set(this CompoundTag collection, string key, Vector3 value) =>
        collection.Set(key, value.Write());

    public static void Add(this ListTag collection, Vector3 value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Vector3 value) =>
        collection.Insert(index, value.Write());


    public static Vector3 Get<T>(this CompoundTag collection, string key) where T : IEquatable<Vector3> =>
        ((CompoundTag)collection.GetTag(key)).To<T>();

    public static Vector3 Get<T>(this ListTag collection, int index) where T : IEquatable<Vector3> =>
        ((CompoundTag)collection.GetTag(index)).To<T>();

    public static Vector3 To<T>(this CompoundTag tag) where T : IEquatable<Vector3> =>
        new(tag.Get<float>("x"), tag.Get<float>("y"), tag.Get<float>("z"));
}
