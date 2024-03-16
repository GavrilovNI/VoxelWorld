

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VoxelWorld.Inventories;

public class IndexedCapabilityPart<T> : IIndexedCapability<T> where T : class, IStack<T>
{
    private IIndexedCapability<T> _capability;
    private int _startIndex;

    public int Size { get; }

    public IndexedCapabilityPart(IIndexedCapability<T> capability, int count) : this(capability, 0, count)
    {

    }

    public IndexedCapabilityPart(IIndexedCapability<T> capability, int startIndex, int count)
    {
        if(startIndex < 0 || startIndex >= capability.Size)
            throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, $"should be >= 0 & < capability.Size ({capability.Size})");
        if(count < 0 || startIndex + count >= capability.Size)
            throw new ArgumentOutOfRangeException(nameof(count), count, $"should be >= 0 & {nameof(startIndex)}+{nameof(count)} < capability.Size ({capability.Size})");

        _capability = capability;
        _startIndex = startIndex;
        Size = count;
    }

    public T Get(int index) => _capability.Get(_startIndex + index);
    public IEnumerator<T> GetEnumerator() => _capability.Skip(_startIndex).Take(Size).GetEnumerator();
    public int SetMax(int index, T stack, bool simulate = false) => _capability.SetMax(_startIndex + index, stack, simulate);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
