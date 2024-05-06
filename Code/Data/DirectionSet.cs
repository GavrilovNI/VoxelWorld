using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using VoxelWorld.Mth.Enums;

namespace VoxelWorld.Data;

public struct DirectionSet : ISet<Direction>
{
    private static readonly DirectionSet _all = new(Direction.AllSet);
    public static DirectionSet All => new(_all);


    private byte _directions = 0;

    public DirectionSet()
    {

    }

    public DirectionSet(params Direction[] directions) : this((IEnumerable<Direction>)directions)
    {

    }

    public DirectionSet(DirectionSet other)
    {
        _directions = other._directions;
    }

    public DirectionSet(IEnumerable<Direction> directions)
    {
        ArgumentNullException.ThrowIfNull(directions);
        foreach(var direction in directions)
            _directions |= GetBit(direction);
    }

    public readonly int Count
    {
        get
        {
            int result = 0;
            int max = Direction.AllSet.Count;
            for(int i = 0; i < max; i++)
                result += (_directions >> i) & 1;
            return result;
        }
    }

    public readonly bool IsReadOnly => false;

    public bool Add(Direction direction)
    {
        var bit = GetBit(direction);
        if((_directions & bit) != 0)
            return false;
        _directions |= bit;
        return true;
    }
    void ICollection<Direction>.Add(Direction direction) => Add(direction);

    public bool Remove(Direction direction)
    {
        var bit = GetBit(direction);
        if((_directions & bit) == 0)
            return false;
        _directions &= (byte)~bit;
        return true;
    }

    public void Clear() => _directions = 0;
    public readonly bool Contains(Direction direction) => (_directions & GetBit(direction)) != 0;

    public readonly void CopyTo(Direction[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        var directions = this.ToArray();
        if(arrayIndex + directions.Length > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "Array is too small for inserting at given index.");

        Array.Copy(directions, 0, array, arrayIndex, directions.Length);
    }


    public readonly IEnumerator<Direction> GetEnumerator()
    {
        foreach(var direction in Direction.AllSet)
        {
            if((_directions & GetBit(direction)) != 0)
                yield return direction;
        }
    }
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void ExceptWith(IEnumerable<Direction> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if(other is not DirectionSet set)
            set = new(other);
        _directions &= (byte)~set._directions;
    }

    public void SymmetricExceptWith(IEnumerable<Direction> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if(other is not DirectionSet set)
            set = new(other);
        _directions ^= set._directions;
    }

    public void IntersectWith(IEnumerable<Direction> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if(other is not DirectionSet set)
            set = new(other);
        _directions &= set._directions;
    }

    public void UnionWith(IEnumerable<Direction> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if(other is not DirectionSet set)
            set = new(other);
        _directions |= set._directions;
    }

    public readonly bool SetEquals(IEnumerable<Direction> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if(other is not DirectionSet set)
            set = new(other);
        return _directions == set._directions;
    }

    public readonly bool Overlaps(IEnumerable<Direction> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if(other is not DirectionSet set)
            set = new(other);
        return (_directions & set._directions) != 0;
    }

    public readonly bool IsProperSubsetOf(IEnumerable<Direction> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if(other is not DirectionSet set)
            set = new(other);
        return _directions != set._directions && (_directions & set._directions) == _directions;
    }

    public readonly bool IsSubsetOf(IEnumerable<Direction> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if(other is not DirectionSet set)
            set = new(other);
        return (_directions & set._directions) == _directions;
    }

    public readonly bool IsProperSupersetOf(IEnumerable<Direction> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if(other is not DirectionSet set)
            set = new(other);
        return _directions != set._directions && (_directions & set._directions) == set._directions;
    }

    public readonly bool IsSupersetOf(IEnumerable<Direction> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if(other is not DirectionSet set)
            set = new(other);
        return (_directions & set._directions) == set._directions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte GetBit(Direction direction) => (byte)(1 << direction.Ordinal);
}
