using Sandcube.Blocks.States;
using Sandcube.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.Data;

public class Palette<T> : IPalette<T>, IBinaryWritable/*, IBinaryStaticReadable<Palette<T>>*/ where T : notnull, IBinaryWritable
    /*, IBinaryStaticReadable<T>*/ // TODO: implement when T.Read will be whitelisted
{
    protected int _nextId = 0; // TODO: make private
    protected readonly Dictionary<int, T> _valueFromId = new(); // TODO: make private
    protected readonly Dictionary<T, int> _idFromValue = new(); // TODO: make private

    public int Count => _valueFromId.Count;

    public Palette()
    {

    }

    public void Clear()
    {
        _nextId = 0;
        _valueFromId.Clear();
        _idFromValue.Clear();
    }

    public int GetOrAdd(T value)
    {
        if(_idFromValue.TryGetValue(value, out var id))
            return id;

        id = _nextId++;
        _valueFromId[id] = value;
        _idFromValue[value] = id;
        return id;
    }

    public bool RemoveById(T value)
    {
        var removed = _idFromValue.Remove(value, out int id);
        if(removed && !_valueFromId.Remove(id))
            throw new InvalidOperationException($"Value {value} was not binded to id {id} correctly");
        return removed;
    }

    public bool RemoveByValue(int id)
    {
        var removed = _valueFromId.Remove(id, out T? value);
        if(removed && !_idFromValue.Remove(value!))
            throw new InvalidOperationException($"Id {id} was not binded to value {value} correctly");
        return removed;
    }

    public bool TryGetValue(int id, out T value) => _valueFromId.TryGetValue(id, out value!);
    public bool TryGetId(T value, out int id) => _idFromValue.TryGetValue(value, out id);

    public T GetValue(int id)
    {
        if(!_valueFromId.TryGetValue(id, out var value))
            throw new InvalidOperationException($"Id {id} wasn't present in palette");
        return value;
    }

    public int GetId(T value)
    {
        if(!_idFromValue.TryGetValue(value, out var id))
            throw new InvalidOperationException($"Value {value} wasn't present in palette");
        return id;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(_idFromValue.Count);
        foreach(var (value, id) in _idFromValue)
        {
            writer.Write(value);
            writer.Write(id);
        }
    }

    // TODO: implement when T.Read can be used
    /*public static Palette<T> Read(BinaryReader reader)
    {
    }*/
}

// TODO: remove when Palette.T.Read can be used
public class BlockStatePalette : Palette<BlockState>, IBinaryStaticReadable<BlockStatePalette>
{
    public static BlockStatePalette Read(BinaryReader reader)
    {
        BlockStatePalette result = new();
        int count = reader.ReadInt32();
        for(int i = 0; i < count; ++i)
        {
            var value = BlockState.Read(reader);
            int id = reader.ReadInt32();
            result._idFromValue[value] = id;
            result._valueFromId[id] = value;
            result._nextId = Math.Max(result._nextId, id);
        }
        return result;
    }
}
