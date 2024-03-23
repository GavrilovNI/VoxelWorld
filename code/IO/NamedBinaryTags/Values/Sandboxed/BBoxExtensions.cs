using VoxelWorld.IO.NamedBinaryTags.Collections;
using System;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;

public static class BBoxExtensions
{
    public static ListTag Write(this BBox value) => new()
    {
        value.Mins.x,
        value.Mins.y,
        value.Mins.z,
        value.Maxs.x,
        value.Maxs.y,
        value.Maxs.z
    };


    public static void Set(this CompoundTag collection, string key, BBox value) =>
        collection.Set(key, value.Write());

    public static void Set(this ListTag collection, int index, BBox value) =>
        collection.Set(index, value.Write());

    public static void Add(this ListTag collection, BBox value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, BBox value) =>
        collection.Insert(index, value.Write());


    public static BBox Get<T>(this CompoundTag collection, string key, BBox defaultValue = default) where T : IEquatable<BBox>
    {
        var tag = collection.GetTagOrNull(key);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<BBox>();
    }

    public static BBox Get<T>(this ListTag collection, int index, BBox defaultValue = default) where T : IEquatable<BBox>
    {
        var tag = collection.GetTagOrNull(index);
        if(tag is not ListTag listTag)
            return defaultValue;
        return listTag.To<BBox>();
    }

    public static BBox To<T>(this ListTag tag) where T : IEquatable<BBox> =>
        new(new Vector3(tag.Get<float>(0), tag.Get<float>(1), tag.Get<float>(2)),
            new Vector3(tag.Get<float>(3), tag.Get<float>(4), tag.Get<float>(5)));
}
