using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Inventories;

public class CombinedIndexedCapability<T> : IIndexedCapability<T> where T : class, IStack<T>
{
    private readonly List<IIndexedCapability<T>> _capabilities = new();

    public CombinedIndexedCapability()
    {
    }

    public CombinedIndexedCapability(IEnumerable<IIndexedCapability<T>> capabilities)
    {
        _capabilities.AddRange(capabilities);
    }

    public CombinedIndexedCapability(params IIndexedCapability<T>[] capabilities) : this(capabilities.AsEnumerable())
    {

    }

    public int Size => _capabilities.Sum(c => c.Size);

    protected (IIndexedCapability<T> capability, int localIndex) CalculateCapabilityAndIndex(int index)
    {
        foreach(var capability in _capabilities)
        {
            if(index < capability.Size)
                return (capability, index);
            index -= capability.Size;
        }
        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public T Get(int index)
    {
        (var capability, var localIndex) = CalculateCapabilityAndIndex(index);
        return capability.Get(localIndex);
    }

    public int SetMax(int index, T stack, bool simulate = false)
    {
        (var capability, var localIndex) = CalculateCapabilityAndIndex(index);
        return capability.SetMax(localIndex, stack, simulate);
    }

    public System.Collections.Generic.IEnumerator<T> GetEnumerator() => new Enumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    private struct Enumerator : IEnumerator<T>, IEnumerator
    {
        private readonly CombinedIndexedCapability<T> _capability;
        private int _capabilityIndex;
        private int _localIndex;

        public readonly T Current => _capability._capabilities[_capabilityIndex].Get(_localIndex);
        readonly object IEnumerator.Current => this.Current;

        internal Enumerator(CombinedIndexedCapability<T> capability)
        {
            _capability = capability;
            _capabilityIndex = 0;
            _localIndex = -1;
        }

        public readonly void Dispose()
        {
        }

        public bool MoveNext()
        {
            IIndexedCapability<T>? currentCapability = null;
            while(_capabilityIndex < _capability._capabilities.Count)
            {
                currentCapability = _capability._capabilities[_capabilityIndex];
                if(++_localIndex < currentCapability.Size)
                    break;
                ++_capabilityIndex;
                _localIndex = -1;
            }

            return currentCapability != null && _localIndex >= 0;
        }

        void IEnumerator.Reset()
        {
            _capabilityIndex = 0;
            _localIndex = -1;
        }
    }
}
