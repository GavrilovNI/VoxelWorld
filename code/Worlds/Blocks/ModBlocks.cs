

namespace Sandcube.Worlds.Blocks;

public class ModBlocks
{
    public virtual void Register(BlocksRegistry blocksRegistry)
    {
        var blockProperties = TypeLibrary.GetType(GetType()).Properties.Where(p => p.IsPublic && p.PropertyType.IsAssignableTo(typeof(Block)));

        foreach(var property in blockProperties)
        {
            if(property.IsStatic)
                Log.Warning($"{GetType().FullName}.{property.Name} shouldn't be static (causes error on hotload)");

            if(property.IsSetMethodPublic)
                Log.Warning($"set of {GetType().FullName}.{property.Name} should be private");

            var block = (property.GetValue(this) as Block)!;
            blocksRegistry.Add(block);
        }
    }
}
