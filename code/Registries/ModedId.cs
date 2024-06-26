﻿using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.Mods.Base;
using System;

namespace VoxelWorld.Registries;

public readonly record struct ModedId : INbtWritable, INbtStaticReadable<ModedId>
{
    public readonly Id ModId { get; }
    public readonly Id Name { get; }

    public ModedId(string modId, string name) : this((Id)modId, (Id)name)
    {
    }

    public ModedId(in Id modId, in Id name)
    {
        ModId = modId;
        Name = name;
    }

    public static bool TryParse(string id, out ModedId modedId)
    {
        var ids = id.Split(":");
        string modId;
        string name;
        if(ids.Length == 1)
        {
            modId = BaseMod.Instance!.Id;
            name = ids[0];
        }
        else if(ids.Length == 2)
        {
            modId = ids[0];
            name = ids[1];
        }
        else
        {
            modedId = default;
            return false;
        }

        try
        {
            modedId = new(modId, name);
        }
        catch(ArgumentException)
        {
            modedId = default;
            return false;
        }
        return true;
    }

    public static ModedId Parse(string id)
    {
        if(!TryParse(id, out var result))
            throw new FormatException($"Couldn't parse {nameof(ModedId)} from '{id}'");
        return result;
    }

    public static implicit operator string(ModedId id) => id.ToString();
    public static explicit operator ModedId(string id) => Parse(id);

    public readonly override string ToString()
    {
        return $"{ModId}:{Name}";
    }

    public BinaryTag Write()
    {
        CompoundTag result = new();
        result.Set("mod", ModId);
        result.Set("name", Name);
        return result;
    }

    public static ModedId Read(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();
        return new ModedId(Id.Read(compoundTag.GetTag("mod")), Id.Read(compoundTag.GetTag("name")));
    }
}
