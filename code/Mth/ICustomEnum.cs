using System.Collections.Generic;

namespace Sandcube.Mth;

public interface ICustomEnum<T> where T : ICustomEnum<T>
{
    public abstract static IReadOnlyList<T> All { get; } // do not rename. was: "All"
    public abstract static bool TryParse(string name, out T value); // do not rename. was: "TryParse"
}
