using System.Linq;

namespace Sandcube.Registries;

public class ModRegisterables<T> where T : class, IRegisterable
{
    public virtual void Register(Registry<T> registry)
    {
        var properties = TypeLibrary.GetType(GetType()).Properties.Where(p => p.IsPublic && p.PropertyType.IsAssignableTo(typeof(T)));

        foreach(var property in properties)
        {
            if(property.IsStatic)
                Log.Warning($"{GetType().FullName}.{property.Name} shouldn't be static (causes error on hotload)");

            if(property.IsSetMethodPublic)
                Log.Warning($"set of {GetType().FullName}.{property.Name} should be private");

            var value = (property.GetValue(this) as T)!;
            registry.Add(value);
        }
    }
}
