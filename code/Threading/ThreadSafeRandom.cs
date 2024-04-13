using System;

namespace VoxelWorld.Threading;

public class ThreadSafeRandom : Random
{
    private readonly Random _random;

    public ThreadSafeRandom() => _random = new();
    public ThreadSafeRandom(int seed) => _random = new(seed);

    public override int Next()
    {
        lock(_random)
        {
            return _random.Next();
        }
    }

    public override int Next(int maxValue)
    {
        lock(_random)
        {
            return _random.Next(maxValue);
        }
    }

    public override int Next(int minValue, int maxValue)
    {
        lock(_random)
        {
            return _random.Next(minValue, maxValue);
        }
    }

    public override long NextInt64()
    {
        lock(_random)
        {
            return _random.NextInt64();
        }
    }

    public override long NextInt64(long maxValue)
    {
        lock(_random)
        {
            return _random.NextInt64(maxValue);
        }
    }

    public override long NextInt64(long minValue, long maxValue)
    {
        lock(_random)
        {
            return _random.NextInt64(minValue, maxValue);
        }
    }

    public override float NextSingle()
    {
        lock(_random)
        {
            return _random.NextSingle();
        }
    }

    public override double NextDouble()
    {
        lock(_random)
        {
            return _random.NextDouble();
        }
    }

    public override void NextBytes(byte[] buffer)
    {
        lock(_random)
        {
            _random.NextBytes(buffer);
        }
    }

    public override void NextBytes(Span<byte> buffer)
    {
        lock(_random)
        {
            _random.NextBytes(buffer);
        }
    }

    protected override double Sample() => throw new NotSupportedException();
}
