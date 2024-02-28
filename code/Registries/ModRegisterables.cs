using System.Linq;
using System.Threading.Tasks;

namespace Sandcube.Registries;

public class ModRegisterables<T> where T : IRegisterable
{
    public virtual Task Register(RegistriesContainer registries) => RegisterFrom(this, registries.GetOrAddRegistry<T>());

    public static Task RegisterFrom(object target, Registry<T> registry)
    {
        var properties = TypeLibrary.GetType(target.GetType()).Properties.Where(p => p.IsPublic && p.PropertyType.IsAssignableTo(typeof(T)));

        foreach(var property in properties)
        {
            if(property.IsStatic)
                Log.Warning($"{target.GetType().FullName}.{property.Name} shouldn't be static (causes error on hotload)");

            if(property.IsSetMethodPublic)
                Log.Warning($"set of {target.GetType().FullName}.{property.Name} should be private");

            var value = property.GetValue(target);
            if(value is null)
            {
                Log.Warning($"value {target.GetType().FullName}.{property.Name} was null");
                continue;
            }
            registry.Register((IRegisterable)value);
        }

        return Task.CompletedTask;
    }
}
