using Sandcube.IO;
using Sandcube.Mods.Base;
using System;
using System.IO;

namespace Sandcube.Registries;

public readonly record struct ModedId : IBinaryWritable, IBinaryStaticReadable<ModedId>
{
    public readonly Id ModId { get; init; }
    public readonly Id Name { get; init; }

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
            modId = SandcubeBaseMod.Instance!.Id;
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

    public void Write(BinaryWriter writer)
    {
        writer.Write<Id>(ModId);
        writer.Write<Id>(Name);
    }

    public static ModedId Read(BinaryReader reader)
    {
        var modId = Id.Read(reader);
        var name = Id.Read(reader);
        return new(modId, name);
    }
}
