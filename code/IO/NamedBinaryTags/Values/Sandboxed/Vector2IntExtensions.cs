using VoxelWorld.IO.NamedBinaryTags.Collections;
using System;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;

public static class Vector2IntExtensions
{
    public static ListTag Write(this Vector2Int value) => new()
    {
        value.x,
        value.y,
    };


    public static void Set(this CompoundTag collection, string key, Vector2Int value) =>
        collection.Set(key, value.Write());

    public static void Set(this ListTag collection, int index, Vector2Int value) =>
        collection.Set(index, value.Write());

    public static void Add(this ListTag collection, Vector2Int value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Vector2Int value) =>
        collection.Insert(index, value.Write());


    public static Vector2Int Get<T>(this CompoundTag collection, string key, Vector2Int defaultValue = default) where T : IEquatable<Vector2Int>
    {
        var tag = collection.GetTagOrNull(key);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<Vector2Int>();
    }

    public static Vector2Int Get<T>(this ListTag collection, int index, Vector2Int defaultValue = default) where T : IEquatable<Vector2Int>
    {
        var tag = collection.GetTagOrNull(index);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<Vector2Int>();
    }

    public static Vector2Int To<T>(this ListTag tag) where T : IEquatable<Vector2Int> =>
        new(tag.Get<int>(0), tag.Get<int>(1));
}
