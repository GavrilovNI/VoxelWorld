using Sandcube.IO.NamedBinaryTags.Collections;
using System;

namespace Sandcube.IO.NamedBinaryTags.Values.Sandboxed;

public static class Vector4Extensions
{
    public static ListTag Write(this Vector4 value) => new()
    {
        value.x,
        value.y,
        value.z,
        value.w
    };


    public static void Set(this CompoundTag collection, string key, Vector4 value) =>
        collection.Set(key, value.Write());

    public static void Add(this ListTag collection, Vector4 value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Vector4 value) =>
        collection.Insert(index, value.Write());


    public static Vector4 Get<T>(this CompoundTag collection, string key) where T : IEquatable<Vector4> =>
        collection.GetTag(key).To<ListTag>().To<T>();

    public static Vector4 Get<T>(this ListTag collection, int index) where T : IEquatable<Vector4> =>
        collection.GetTag(index).To<ListTag>().To<T>();

    public static Vector4 To<T>(this ListTag tag) where T : IEquatable<Vector4> =>
        new(tag.Get<float>(0), tag.Get<float>(1), tag.Get<float>(2), tag.Get<float>(3));
}
