using Sandcube.Mth;
using System.IO;

namespace Sandcube.IO;

public readonly record struct WorldSaveOptions : IBinaryWritable, IBinaryStaticReadable<WorldSaveOptions>
{
    public Vector3Int RegionSize { get; init; }
    public Vector3Int ChunkSize { get; init; }


    public readonly Vector3Int GetRegionPosition(Vector3Int chunkPosition) =>
        (1f * chunkPosition / RegionSize).Floor();

    public readonly void Write(BinaryWriter writer)
    {
        writer.Write(RegionSize);
        writer.Write(ChunkSize);
    }

    public static WorldSaveOptions Read(BinaryReader reader)
    {
        Vector3Int regionSize = Vector3Int.Read(reader);
        Vector3Int chunkSize = Vector3Int.Read(reader);
        return new WorldSaveOptions()
        {
            RegionSize = regionSize,
            ChunkSize = chunkSize,
        };
    }
}
