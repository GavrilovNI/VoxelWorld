using VoxelWorld.IO.NamedBinaryTags.Collections;
using System;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;

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

    public static void Set(this ListTag collection, int index, Vector4 value) =>
        collection.Set(index, value.Write());

    public static void Add(this ListTag collection, Vector4 value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Vector4 value) =>
        collection.Insert(index, value.Write());


    public static Vector4 Get<T>(this CompoundTag collection, string key, Vector4 defaultValue = default) where T : IEquatable<Vector4>
    {
        var tag = collection.GetTagOrNull(key);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<Vector4>();
    }

    public static Vector4 Get<T>(this ListTag collection, int index, Vector4 defaultValue = default) where T : IEquatable<Vector4>
    {
        var tag = collection.GetTagOrNull(index);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<Vector4>();
    }

    public static Vector4 To<T>(this ListTag tag) where T : IEquatable<Vector4> =>
        new(tag.Get<float>(0), tag.Get<float>(1), tag.Get<float>(2), tag.Get<float>(3));
}
