﻿using Sandcube.IO.NamedBinaryTags.Collections;
using System;

namespace Sandcube.IO.NamedBinaryTags.Values.Sandboxed;

public static class TransformExtensions
{
    public static ListTag Write(this Transform value) => new()
    {
        value.Position.x,
        value.Position.y,
        value.Position.z,
        value.Rotation.x,
        value.Rotation.y,
        value.Rotation.z,
        value.Rotation.w,
        value.Scale.x,
        value.Scale.y,
        value.Scale.z
    };


    public static void Set(this CompoundTag collection, string key, Transform value) =>
        collection.Set(key, value.Write());

    public static void Add(this ListTag collection, Transform value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Transform value) =>
        collection.Insert(index, value.Write());


    public static Transform Get<T>(this CompoundTag collection, string key) where T : IEquatable<Transform> =>
        collection.GetTag(key).To<ListTag>().To<T>();

    public static Transform Get<T>(this ListTag collection, int index) where T : IEquatable<Transform> =>
        collection.GetTag(index).To<ListTag>().To<T>();

    public static Transform To<T>(this ListTag tag) where T : IEquatable<Transform> =>
        new(new Vector3(tag.Get<float>(0), tag.Get<float>(1), tag.Get<float>(2)),
            new Rotation(tag.Get<float>(3), tag.Get<float>(4), tag.Get<float>(5), tag.Get<float>(6)),
            new Vector3(tag.Get<float>(7), tag.Get<float>(8), tag.Get<float>(9)));
}
