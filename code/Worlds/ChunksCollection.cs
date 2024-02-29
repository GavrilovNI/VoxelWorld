using Sandbox;
using Sandcube.Mth;
using Sandcube.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Worlds;

public class ChunksCollection : IEnumerable<KeyValuePair<Vector3Int, Chunk>>, IDisposable
{
    public event Action<Chunk>? ChunkLoaded = null;
    public event Action<Chunk>? ChunkUnloaded = null;

    protected bool Disposed { get; private set; } = false;

    protected readonly object Locker = new();
    protected readonly SortedDictionary<Vector3Int, Chunk> Chunks;
    protected readonly Dictionary<Vector3Int, Task<Chunk?>> LoadingChunks = new();

    protected CancellationTokenSource LoadingTokenSource = new();

    protected readonly Action<Chunk?> DestroyChunk;

    public ChunksCollection(Action<Chunk?> chunkDestroyer, IComparer<Vector3Int>? comparer = null)
    {
        DestroyChunk = chunkDestroyer;
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

        if(managed)
            LoadingTokenSource.Dispose();

        Disposed = true;
    }

    public object GetLocker() => Locker;

    public void Clear()
    {
        lock(Locker)
        {
            LoadingTokenSource.Cancel();
            LoadingTokenSource.Dispose();
            LoadingTokenSource = new();

            foreach(var (position, chunk) in Chunks)
            {
                ChunkUnloaded?.Invoke(chunk);
                DestroyChunk(chunk);
            }

            Chunks.Clear();
            LoadingChunks.Clear();
        }
    }

    public bool RemoveLoaded(Chunk chunk)
    {
        lock(Locker)
        {
            if(TryGet(chunk.Position, out var realChunk) && realChunk == chunk)
                return Chunks.Remove(chunk.Position);
        }
        return false;
    }

    public bool RemoveLoaded(Vector3Int position)
    {
        lock(Locker)
        {
            return Chunks.Remove(position);
        }
    }

    public bool HasLoaded(Vector3Int position)
    {
        lock(Locker)
        {
            return Chunks.TryGetValue(position, out var chunk) && chunk.IsValid;
        }
    }

    public bool IsLoading(Vector3Int position)
    {
        lock(Locker)
        {
            return LoadingChunks.ContainsKey(position);
        }
    }

    public Chunk? Get(Vector3Int position)
    {
        lock(Locker)
        {
            if(Chunks.TryGetValue(position, out var chunk) && chunk.IsValid)
                return chunk;
        }
        return null;
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

    public Task<Chunk?> GetLoading(Vector3Int position)
    {
        lock(Locker)
        {
            if(LoadingChunks.TryGetValue(position, out var task))
                return task;
        }
        return Task.FromResult<Chunk?>(null);
    }

    public bool TryGetLoading(Vector3Int position, out Task<Chunk> task)
    {
        lock(Locker)
        {
            return LoadingChunks.TryGetValue(position, out task!);
        }
    }

    public virtual Task<Chunk?> GetChunkOrLoad(Vector3Int position, Func<CancellationToken, Task<Chunk>> loader)
    {
        lock(Locker)
        {
            if(TryGet(position, out var chunk))
                return Task.FromResult(chunk)!;

            if(TryGetLoading(position, out var task))
                return task!;

            Task<Chunk?> newTask = null!;
            newTask = loader(LoadingTokenSource.Token).ContinueWith(t =>
            {
                lock (Locker)
                {
                    var chunk = t.Result;
                    if(!LoadingChunks.TryGetValue(position, out var realTask) || realTask != newTask)
                    {
                        DestroyChunk(chunk);
                        return null;
                    }

                    LoadingChunks.Remove(position);

                    if(!t.IsCompletedSuccessfully)
                        DestroyChunk(chunk);

                    t.ThrowIfNotCompletedSuccessfully();

                    if(!chunk.IsValid())
                        return null;

                    Chunks[position] = chunk;
                    ChunkLoaded?.Invoke(chunk);
                    return chunk;
                }
            });
            LoadingChunks[position] = newTask;
            return newTask;
        }
    }

    public IEnumerator<KeyValuePair<Vector3Int, Chunk>> GetEnumerator()
    {
        lock(Locker)
        {
            foreach(var pair in Chunks)
            {
                if(pair.Value.IsValid)
                    yield return pair;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
