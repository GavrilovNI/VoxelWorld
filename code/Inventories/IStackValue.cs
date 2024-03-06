using Sandcube.IO;
using Sandcube.IO.NamedBinaryTags;
using Sandcube.Registries;

namespace Sandcube.Inventories;

public interface IStackValue<T> : INbtWritable, INbtStaticReadable<T>, IBinaryWritable, IBinaryStaticReadable<T> where T : IStackValue<T>
{
    int StackLimit { get; }
    ModedId Id { get; }
}
