using Sandcube.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.Data;

public class EntitiesData : IEnumerable<byte[]>
{
    public List<byte[]> Data = new();

    public int Count => Data.Count;

    public void Clear()
    {
        Data.Clear();
    }
    public bool IsEmpty() => Data.Count == 0;

    public void AddData(in Entity entity)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        entity.Write(writer);

        if(stream.Length == 0)
            return;

        var data = stream.ToArray();
        Data.Add(data);
    }

    public IEnumerator<byte[]> GetEnumerator() => Data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
