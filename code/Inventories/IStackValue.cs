using Sandcube.IO;
using Sandcube.Registries;
using System.IO;

namespace Sandcube.Inventories;

public interface IStackValue<T> : IBinaryWritable, IBinaryStaticReadable<T> where T : IStackValue<T>
{
    int StackLimit { get; }
    ModedId Id { get; }
}
