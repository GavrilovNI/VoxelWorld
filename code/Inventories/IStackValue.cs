using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.Registries;

namespace VoxelWorld.Inventories;

public interface IStackValue<T> : INbtWritable, INbtStaticReadable<T> where T : IStackValue<T>
{
    int StackLimit { get; }
    ModedId Id { get; }
}
