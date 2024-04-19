using System;
using System.Collections.Generic;
using System.Linq;

namespace VoxelWorld.Worlds.Data;

public class BlocksAdditionalDataCollection
{
    private readonly Dictionary<Vector3IntB, Dictionary<BlocksAdditionalDataType, object>> _data = new();


    public void Set<T>(BlocksAdditionalDataType<T> dataType, in Vector3IntB position, T value) where T : notnull
    {
        BlocksAdditionalDataType.AssertRegestered(dataType);

        if(dataType.DefaultValue.Equals(value))
        {
            Reset(dataType, position);
            return;
        }

        GetOrCreateBlockData(position)[dataType] = value;
    }

    public void Reset<T>(BlocksAdditionalDataType<T> dataType, in Vector3IntB position) where T : notnull
    {
        BlocksAdditionalDataType.AssertRegestered(dataType);

        if(!_data.TryGetValue(position, out var blockData))
            return;

        blockData.Remove(dataType);
        if(blockData.Count == 0)
            _data.Remove(position);
    }

    public T Get<T>(BlocksAdditionalDataType<T> dataType, in Vector3IntB position) where T : notnull
    {
        BlocksAdditionalDataType.AssertRegestered(dataType);

        if(_data.TryGetValue(position, out var blockData) && blockData.TryGetValue(dataType, out var value))
            return (T)value;

        return dataType.DefaultValue;
    }

    public IEnumerable<KeyValuePair<BlocksAdditionalDataType, object>> GetNotDefaultValuesAt(in Vector3Int position)
    {
        if(!_data.TryGetValue(position, out var blockData))
            return Enumerable.Empty<KeyValuePair<BlocksAdditionalDataType, object>>();

        return blockData;
    }

    public void ClearAt(in Vector3Int position) => _data.Remove(position);
    public void Clear() => _data.Clear();


    private Dictionary<BlocksAdditionalDataType, object> GetOrCreateBlockData(in Vector3Int position)
    {
        if(_data.TryGetValue(position, out var blockData))
            return blockData;

        blockData = new();
        _data[position] = blockData;
        return blockData;
    }
}
