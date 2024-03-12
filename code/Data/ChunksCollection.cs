using Sandcube.Mth;
using Sandcube.Worlds;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sandcube.Data;

public class ChunksCollection : IEnumerable<Chunk>, IDisposable
{
    public event Action<Chunk>? ChunkLoaded = null;
    public event Action<Chunk>? ChunkUnloaded = null;


    protected bool Disposed { get; private set; } = false;

    protected readonly object Locker = new();
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

    public object GetLocker() => Locker;

    public void Clear()
    {
        lock(Locker)
        {
            foreach(var (position, chunk) in Chunks)
            {
                chunk.Destroyed -= OnChunkDestroyed;
                ChunkUnloaded?.Invoke(chunk);
            }

            Chunks.Clear();
        }
    }

    public void Add(Chunk chunk)
    {
        lock(Locker)
        {
            Remove(chunk.Position);
            Chunks[chunk.Position] = chunk;
            chunk.Destroyed += OnChunkDestroyed;
        }
        ChunkLoaded?.Invoke(chunk);
    }

    private bool RemoveInternal(Chunk chunk)
    {
        lock(Locker)
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
        lock(Locker)
        {
            if(Chunks.TryGetValue(chunk.Position, out var realChunk) && realChunk == chunk)
                return RemoveInternal(chunk);
        }
        return false;
    }

    public bool Remove(Vector3Int position)
    {
        lock(Locker)
        {
            if(Chunks.TryGetValue(position, out var chunk))
                return RemoveInternal(chunk);
        }
        return false;
    }

    public bool Contains(Vector3Int position)
    {
        lock(Locker)
        {
            return Chunks.TryGetValue(position, out var chunk) && chunk.IsValid;
        }
    }

    public Chunk? GetOrDefault(Vector3Int position, Chunk? defaultValue = null)
    {
        lock(Locker)
        {
            if(Chunks.TryGetValue(position, out var chunk) && chunk.IsValid)
                return chunk;
        }
        return defaultValue;
    }

    public bool TryGet(Vector3Int position, out Chunk chunk)
    {
        lock(Locker)
        {
            if(Chunks.TryGetValue(position, out chunk!) && chunk.IsValid)
                return true;
        }
        return false;
    }

    public IEnumerator<Chunk> GetEnumerator()
    {
        lock(Locker)
        {
            foreach(var (_, chunk) in Chunks)
            {
                if(chunk.IsValid)
                    yield return chunk;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected virtual void OnChunkDestroyed(Chunk chunk) => Remove(chunk);
}
