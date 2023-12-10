using System;

namespace Sandcube.Registries;

public readonly record struct ModedId
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
            modId = SandcubeGame.Instance!.Id;
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

    public static implicit operator string(ModedId id) => id.ToString();

    public readonly override string ToString()
    {
        return $"{ModId}:{Name}";
    }
}
