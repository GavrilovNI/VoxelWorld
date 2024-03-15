﻿using Sandbox;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using System;

namespace VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;

public static class Vector2Extensions
{
    public static ListTag Write(this Vector2 value) => new()
    {
        value.x,
        value.y,
    };


    public static void Set(this CompoundTag collection, string key, Vector2 value) =>
        collection.Set(key, value.Write());

    public static void Add(this ListTag collection, Vector2 value) =>
        collection.Add(value.Write());

    public static void Insert(this ListTag collection, int index, Vector2 value) =>
        collection.Insert(index, value.Write());


    public static Vector2 Get<T>(this CompoundTag collection, string key) where T : IEquatable<Vector2> =>
        collection.GetTag(key).To<ListTag>().To<T>();

    public static Vector2 Get<T>(this ListTag collection, int index) where T : IEquatable<Vector2> =>
        collection.GetTag(index).To<ListTag>().To<T>();

    public static Vector2 To<T>(this ListTag tag) where T : IEquatable<Vector2> =>
        new(tag.Get<float>(0), tag.Get<float>(1));
}
