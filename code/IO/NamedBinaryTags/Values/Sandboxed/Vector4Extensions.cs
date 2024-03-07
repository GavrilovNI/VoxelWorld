using Sandcube.IO.NamedBinaryTags.Collections;
using System;

namespace Sandcube.IO.NamedBinaryTags.Values.Sandboxed;

public static class Vector4Extensions
{
    public static CompoundTag Write(this Vector4 value)
    {
        var result = new CompoundTag();
        result.Set("x", value.x);
        result.Set("y", value.y);
        result.Set("z", value.z);
        result.Set("w", value.w);
        return result;
    }


    public static void Set(this CompoundTag collection, string key, Vector4 value) =>
        collection.Set(key, value.Write());

    public static void Add(this ListTag collection, Vector4 value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Vector4 value) =>
        collection.Insert(index, value.Write());


    public static Vector4 Get<T>(this CompoundTag collection, string key) where T : IEquatable<Vector4> =>
        collection.GetTag(key).To<CompoundTag>().To<T>();

    public static Vector4 Get<T>(this ListTag collection, int index) where T : IEquatable<Vector4> =>
        collection.GetTag(index).To<CompoundTag>().To<T>();

    public static Vector4 To<T>(this CompoundTag tag) where T : IEquatable<Vector4> =>
        new(tag.Get<float>("x"), tag.Get<float>("y"), tag.Get<float>("z"), tag.Get<float>("w"));
}
