

namespace Sandcube.Registries;

public record struct ModedId
{
    public string ModId { get; init; }
    public string Name { get; init; }

    public ModedId(string modId, string name)
    {
        if(string.IsNullOrEmpty(modId) || !Id.Regex.IsMatch(modId))
            throw new ArgumentException($"{nameof(modId)} {modId} should match {nameof(Id.Regex)}");
        if(string.IsNullOrEmpty(name) || !Id.Regex.IsMatch(name))
            throw new ArgumentException($"{nameof(name)} {name} should match {nameof(Id.Regex)}");
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

    public override string ToString()
    {
        return $"{ModId}:{Name}";
    }
}
