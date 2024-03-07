using Sandbox;
using Sandcube.IO.NamedBinaryTags.Collections;
using System;

namespace Sandcube.IO.NamedBinaryTags.Values.Sandboxed;

public static class Vector2Extensions
{
    public static CompoundTag Write(this Vector2 value)
    {
        var result = new CompoundTag();
        result.Set("x", value.x);
        result.Set("y", value.y);
        return result;
    }


    public static void Set(this CompoundTag collection, string key, Vector2 value) =>
        collection.Set(key, value.Write());

    public static void Add(this ListTag collection, Vector2 value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Vector2 value) =>
        collection.Insert(index, value.Write());


    public static Vector2 Get<T>(this CompoundTag collection, string key) where T : IEquatable<Vector2> =>
        collection.GetTag(key).To<CompoundTag>().To<T>();

    public static Vector2 Get<T>(this ListTag collection, int index) where T : IEquatable<Vector2> =>
        collection.GetTag(index).To<CompoundTag>().To<T>();

    public static Vector2 To<T>(this CompoundTag tag) where T : IEquatable<Vector2> =>
        new(tag.Get<float>("x"), tag.Get<float>("y"));
}
