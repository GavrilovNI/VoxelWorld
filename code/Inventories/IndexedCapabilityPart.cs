

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VoxelWorld.Inventories;

public class IndexedCapabilityPart<T> : IIndexedCapability<T> where T : class, IStack<T>
{
    public IIndexedCapability<T> Capability { get; }
    private int _startIndex;

    public int Size { get; }

    public IndexedCapabilityPart(IIndexedCapability<T> capability, int count) : this(capability, 0, count)
    {

    }

    public IndexedCapabilityPart(IIndexedCapability<T> capability, int startIndex, int count)
    {
        if(startIndex < 0 || startIndex >= capability.Size)
            throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, $"should be >= 0 & < capability.Size ({capability.Size})");
        if(count < 0 || startIndex + count > capability.Size)
            throw new ArgumentOutOfRangeException(nameof(count), count, $"should be >= 0 & {nameof(startIndex)}+{nameof(count)} <= capability.Size ({capability.Size})");

        Capability = capability;
        _startIndex = startIndex;
        Size = count;
    }

    public int GetParentIndex(int index) => _startIndex + index;

    public T Get(int index) => Capability.Get(_startIndex + index);
    public IEnumerator<T> GetEnumerator() => Capability.Skip(_startIndex).Take(Size).GetEnumerator();
    public int SetMax(int index, T stack, bool simulate = false) => Capability.SetMax(_startIndex + index, stack, simulate);

    public int InsertMax(int index, T stack, bool simulate = false) => Capability.InsertMax(_startIndex + index, stack, simulate);
    public T ExtractMax(int index, int count, bool simulate = false) => Capability.ExtractMax(_startIndex + index, count, simulate);
    public int ExtractMax(int index, T stack, bool simulate) => Capability.ExtractMax(_startIndex + index, stack, simulate);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override int GetHashCode() => HashCode.Combine(Capability, _startIndex, Size);
}
