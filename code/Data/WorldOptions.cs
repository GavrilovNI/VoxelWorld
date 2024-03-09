using Sandcube.IO.NamedBinaryTags;
using Sandcube.IO.NamedBinaryTags.Collections;
using Sandcube.Mth;

namespace Sandcube.Data;

public readonly record struct WorldOptions : INbtWritable, INbtStaticReadable<WorldOptions>
{
    public required Vector3Int ChunkSize { get; init; }
    public required Vector3Int RegionSize { get; init; }
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
            ChunkSize = Vector3Int.Read(compoundTag.GetTag("chunk_size", DefaultValues.ChunkSize.Write)),
            RegionSize = Vector3Int.Read(compoundTag.GetTag("region_size", DefaultValues.RegionSize.Write)),
            Seed = compoundTag.Get<int>("seed"),
        };
    }
}
