using VoxelWorld.IO.NamedBinaryTags.Collections;
using System;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;

public static class Vector3IntExtensions
{
    public static ListTag Write(this Vector3Int value) => new()
    {
        value.x,
        value.y,
        value.z
    };


    public static void Set(this CompoundTag collection, string key, Vector3Int value) =>
        collection.Set(key, value.Write());

    public static void Set(this ListTag collection, int index, Vector3Int value) =>
        collection.Set(index, value.Write());

    public static void Add(this ListTag collection, Vector3Int value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Vector3Int value) =>
        collection.Insert(index, value.Write());


    public static Vector3Int Get<T>(this CompoundTag collection, string key, Vector3Int defaultValue = default) where T : IEquatable<Vector3Int>
    {
        var tag = collection.GetTagOrNull(key);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<Vector3Int>();
    }

    public static Vector3Int Get<T>(this ListTag collection, int index, Vector3Int defaultValue = default) where T : IEquatable<Vector3Int>
    {
        var tag = collection.GetTagOrNull(index);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<Vector3Int>();
    }

    public static Vector3Int To<T>(this ListTag tag) where T : IEquatable<Vector3Int> =>
        new(tag.Get<int>(0), tag.Get<int>(1), tag.Get<int>(2));
}
