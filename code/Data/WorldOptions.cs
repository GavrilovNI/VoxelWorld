using System;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;
using VoxelWorld.Mth;

namespace VoxelWorld.Data;

public readonly record struct WorldOptions : INbtWritable, INbtStaticReadable<WorldOptions>
{
    public Vector3Byte RegionSize { get; }
    public Vector3Byte ChunkSize { get; }
    public int Seed { get; init; } = 0;
    public float RandomTickSpeed { get; init; } = DefaultValues.RandomTickSpeed;

    public WorldOptions() : this(DefaultValues.RegionSize, DefaultValues.ChunkSize)
    {
    }

    public WorldOptions(Vector3Byte regionSize, Vector3Byte chunkSize)
    {
        if(regionSize == Vector3Byte.Zero)
            throw new ArgumentOutOfRangeException(nameof(regionSize), regionSize, $"{nameof(RegionSize)} can't be 0.");
        if(chunkSize == Vector3Byte.Zero)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), chunkSize, $"{nameof(ChunkSize)} can't be 0.");

        RegionSize = regionSize;
        ChunkSize = chunkSize;
    }

    public BinaryTag Write()
    {
        CompoundTag tag = new();
        tag.Set("region_size", RegionSize);
        tag.Set("chunk_size", ChunkSize);
        tag.Set("seed", Seed);
        tag.Set("random_tick_speed", RandomTickSpeed);
        return tag;
    }

    public static WorldOptions Read(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();

        var regionSize = Vector3Byte.Read(compoundTag.GetTag("region_size", DefaultValues.RegionSize.Write));
        if(regionSize == Vector3Byte.Zero)
        {
            regionSize = (Vector3Byte)compoundTag.Get<Vector3Int>("region_size"); // reading old regionSize type, TODO: remove
            if(regionSize == Vector3IntB.Zero)
            {
                Log.Warning("Read zero region size, setting to default.");
                regionSize = DefaultValues.RegionSize;
            }
        }

        var chunkSize = Vector3Byte.Read(compoundTag.GetTag("chunk_size", DefaultValues.ChunkSize.Write));
        if(chunkSize == Vector3IntB.Zero)
        {
            chunkSize = (Vector3Byte)compoundTag.Get<Vector3Int>("chunk_size"); // reading old chunkSize type, TODO: remove
            if(chunkSize == Vector3IntB.Zero)
            {
                Log.Warning("Read zero chunk size, setting to default.");
                chunkSize = DefaultValues.ChunkSize;
            }
        }

        return new(regionSize, chunkSize)
        {
            Seed = compoundTag.Get<int>("seed"),
            RandomTickSpeed = compoundTag.Get<float>("random_tick_speed", DefaultValues.RandomTickSpeed),
        };
    }
}
