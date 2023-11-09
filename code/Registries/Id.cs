using System.Text.RegularExpressions;

namespace Sandcube;

public record struct Id
{
    public static readonly Regex Regex = new("^[a-z_]+$");

    public string Name { get; init; }

    public Id(string name)
    {
        if(string.IsNullOrEmpty(name) || !Regex.IsMatch(name))
            throw new ArgumentException($"{nameof(name)} should match {nameof(Regex)}");
        Name = name;
    }

    public static implicit operator string(Id id) => id.ToString();
    public static explicit operator Id(string id) => new(id);

    public override string ToString()
    {
        return Name;
    }
}
