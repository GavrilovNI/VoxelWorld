using System.Linq;
using System.Threading.Tasks;

namespace Sandcube.Registries;

public class ModRegisterables<T> where T : IRegisterable
{
    public virtual Task Register(RegistriesContainer registries)
    {
        var properties = TypeLibrary.GetType(GetType()).Properties.Where(p => p.IsPublic && p.PropertyType.IsAssignableTo(typeof(T)));

        foreach(var property in properties)
        {
            if(property.IsStatic)
                Log.Warning($"{GetType().FullName}.{property.Name} shouldn't be static (causes error on hotload)");

            if(property.IsSetMethodPublic)
                Log.Warning($"set of {GetType().FullName}.{property.Name} should be private");

            registries.Register<T>((IRegisterable)property.GetValue(this));
        }

        return Task.CompletedTask;
    }
}
