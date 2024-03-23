using VoxelWorld.IO.NamedBinaryTags.Collections;
using System;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;

public static class Vector3Extensions
{
    public static ListTag Write(this Vector3 value) => new()
    {
        value.x,
        value.y,
        value.z
    };


    public static void Set(this CompoundTag collection, string key, Vector3 value) =>
        collection.Set(key, value.Write());

    public static void Set(this ListTag collection, int index, Vector3 value) =>
        collection.Set(index, value.Write());

    public static void Add(this ListTag collection, Vector3 value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Vector3 value) =>
        collection.Insert(index, value.Write());


    public static Vector3 Get<T>(this CompoundTag collection, string key, Vector3 defaultValue = default) where T : IEquatable<Vector3>
    {
        var tag = collection.GetTagOrNull(key);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<Vector3>();
    }

    public static Vector3 Get<T>(this ListTag collection, int index, Vector3 defaultValue = default) where T : IEquatable<Vector3>
    {
        var tag = collection.GetTagOrNull(index);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<Vector3>();
    }

    public static Vector3 To<T>(this ListTag tag) where T : IEquatable<Vector3> =>
        new(tag.Get<float>(0), tag.Get<float>(1), tag.Get<float>(2));
}
