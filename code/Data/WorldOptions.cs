using Sandcube.IO;
using Sandcube.Mth;
using System.IO;

namespace Sandcube.Data;

public readonly record struct WorldOptions : IBinaryWritable, IBinaryStaticReadable<WorldOptions>
{
    public required Vector3Int ChunkSize { get; init; }
    public required Vector3Int RegionSize { get; init; }
    public int Seed { get; init; } = 0;

    public WorldOptions()
    {
    }

    public static WorldOptions Read(BinaryReader reader)
    {
        Vector3Int chunkSize = Vector3Int.Read(reader);
        Vector3Int regionSize = Vector3Int.Read(reader);
        var seed = reader.ReadInt32();

        return new WorldOptions()
        {
            ChunkSize = chunkSize,
            RegionSize = regionSize,
            Seed = seed,
        };
    }

    public readonly void Write(BinaryWriter writer)
    {
        writer.Write(ChunkSize);
        writer.Write(RegionSize);
        writer.Write(Seed);
    }
}
