using System.Collections.Generic;

namespace VoxelWorld.Inventories;

public interface IReadOnlyIndexedCapability<T> : IEnumerable<T> where T : class, IStack<T>
{
    int Size { get; }
    T Get(int index);
}
