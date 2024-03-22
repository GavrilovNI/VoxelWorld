using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;
using VoxelWorld.IO.NamedBinaryTags.Values.Sandboxed;
using VoxelWorld.Mth;

namespace VoxelWorld.Data;

public readonly record struct WorldOptions : INbtWritable, INbtStaticReadable<WorldOptions>
{
    public required Vector3IntB ChunkSize { get; init; }
    public required Vector3IntB RegionSize { get; init; }
    public int Seed { get; init; } = 0;

    public WorldOptions()
    {
    }

    public BinaryTag Write()
    {
        CompoundTag tag = new();
        tag.Set("chunk_size", ChunkSize);
        tag.Set("region_size", RegionSize);
        tag.Set("seed", Seed);
        return tag;
    }

    public static WorldOptions Read(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();

        return new()
        {
            ChunkSize = compoundTag.Get<Vector3Int>("chunk_size", DefaultValues.ChunkSize),
            RegionSize = compoundTag.Get<Vector3Int>("region_size", DefaultValues.RegionSize),
            Seed = compoundTag.Get<int>("seed"),
        };
    }
}
