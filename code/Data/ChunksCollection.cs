using Sandcube.Mth;
using Sandcube.Worlds;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Data;

public class ChunksCollection : IEnumerable<Chunk>, IDisposable
{
    public event Action<Chunk>? ChunkLoaded = null;
    public event Action<Chunk>? ChunkUnloaded = null;


    protected bool Disposed { get; private set; } = false;

    protected readonly SortedDictionary<Vector3Int, Chunk> Chunks;


    public ChunksCollection(IComparer<Vector3Int>? comparer = null)
    {
        Chunks = new(comparer);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool managed)
    {
        if(Disposed)
            return;
        Disposed = true;

        Clear();
    }

    public void Clear()
    {
        lock(Chunks)
        {
            var chunks = Chunks.Values.ToList(); // TODO: make it faster it smh?
            Chunks.Clear();

            foreach(var chunk in chunks)
            {
                chunk.Destroyed -= OnChunkDestroyed;
                ChunkUnloaded?.Invoke(chunk);
            }
        }
    }

    public Chunk GetOrAdd(Vector3Int position, Chunk chunk)
    {
        lock(Chunks)
        {
            if(TryGet(position, out var realChunk))
                return realChunk;
            Add(chunk);
            return chunk;
        }
    }

    public void Add(Chunk chunk)
    {
        lock(Chunks)
        {
            Remove(chunk.Position);
            Chunks[chunk.Position] = chunk;
            chunk.Destroyed += OnChunkDestroyed;
        }
        ChunkLoaded?.Invoke(chunk);
    }

    private bool RemoveInternal(Chunk chunk)
    {
        lock(Chunks)
        {
            if(!Chunks.Remove(chunk.Position))
                return false;
            chunk.Destroyed -= OnChunkDestroyed;
        }
        ChunkUnloaded?.Invoke(chunk);
        return true;
    }

    public bool Remove(Chunk chunk)
    {
        lock(Chunks)
        {
            if(Chunks.TryGetValue(chunk.Position, out var realChunk) && realChunk == chunk)
                return RemoveInternal(chunk);
        }
        return false;
    }

    public bool Remove(Vector3Int position)
    {
        lock(Chunks)
        {
            if(Chunks.TryGetValue(position, out var chunk))
                return RemoveInternal(chunk);
        }
        return false;
    }

    public bool Contains(Vector3Int position)
    {
        lock(Chunks)
        {
            return Chunks.TryGetValue(position, out var chunk) && chunk.IsValid;
        }
    }

    public Chunk? GetOrDefault(Vector3Int position, Chunk? defaultValue = null)
    {
        lock(Chunks)
        {
            if(Chunks.TryGetValue(position, out var chunk) && chunk.IsValid)
                return chunk;
        }
        return defaultValue;
    }

    public bool TryGet(Vector3Int position, out Chunk chunk)
    {
        lock(Chunks)
        {
            if(Chunks.TryGetValue(position, out chunk!) && chunk.IsValid)
                return true;
        }
        return false;
    }

    public IEnumerator<Chunk> GetEnumerator()
    {
        lock(Chunks)
        {
            var chunks = Chunks.Values.ToList(); // TODO: make it faster it smh?
            foreach(var chunk in chunks)
            {
                if(chunk.IsValid)
                    yield return chunk;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected virtual void OnChunkDestroyed(Chunk chunk) => Remove(chunk);
}
