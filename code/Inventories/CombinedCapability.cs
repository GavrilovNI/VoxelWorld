using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Inventories;

public class CombinedCapability<T> : ICapability<T> where T : class, IStack<T>
{
    private readonly List<ICapability<T>> _capabilities = new();

    public CombinedCapability()
    {
    }

    public CombinedCapability(IEnumerable<ICapability<T>> capabilities)
    {
        _capabilities.AddRange(capabilities);
    }

    public CombinedCapability(params ICapability<T>[] capabilities) : this(capabilities.AsEnumerable())
    {

    }

    public bool ContainsCapability(ICapability<T> capability) => _capabilities.Contains(capability);
    public void AddCapability(ICapability<T> capability) => _capabilities.Add(capability);
    public bool RemoveCapability(ICapability<T> capability) => _capabilities.Remove(capability);

    public virtual int InsertMax(T stack, bool simulate = false)
    {
        int insertedCount = 0;
        foreach(var capability in _capabilities)
        {
            var insertedCurrent = capability.InsertMax(stack, simulate);
            insertedCount += insertedCurrent;
            stack = stack.Sub(insertedCurrent);
            if(stack.Count <= 0)
                return insertedCount;
        }
        return insertedCount;
    }

    public virtual int ExtractMax(T stack, bool simulate = false)
    {
        int extractedCount = 0;
        foreach(var capability in _capabilities)
        {
            var extractedCurrent = capability.ExtractMax(stack, simulate);
            extractedCount += extractedCurrent;
            stack = stack.Sub(extractedCount);
            if(stack.Count <= 0)
                return extractedCount;
        }
        return extractedCount;
    }

    public IEnumerator<T> GetEnumerator() => new Enumerator<T>(_capabilities);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    internal struct Enumerator<E> : IEnumerator<E>, IEnumerator where E : class, IStack<E>
    {
        private readonly List<ICapability<E>> _list;
        private int _index;
        private IEnumerator<E>? _currentEnumerator;

        internal Enumerator(List<ICapability<E>> list)
        {
            _list = list;
            _index = 0;
        }

        public readonly void Dispose()
        {
            _currentEnumerator?.Dispose();
        }

        public bool MoveNext()
        {
            if(_currentEnumerator == null || !_currentEnumerator.MoveNext())
                return MoveNextRare();
            return true;
        }

        private bool MoveNextRare()
        {
            bool moved = false;
            while(!moved)
            {
                var capability = _list[_index++];
                _currentEnumerator = capability.GetEnumerator();
                moved = _currentEnumerator.MoveNext();
                if(!moved && _index >= _list.Count)
                    return false;
            }
            return true;
        }

        public readonly E Current => _currentEnumerator!.Current;

        readonly object? IEnumerator.Current => Current;

        void IEnumerator.Reset()
        {
            _index = 0;
            _currentEnumerator = null;
        }
    }
}
