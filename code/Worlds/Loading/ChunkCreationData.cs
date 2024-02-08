using Sandcube.Mth;

namespace Sandcube.Worlds.Loading;

public readonly record struct ChunkCreationData
{
    public required Vector3Int Position { get; init; }
    public required Vector3Int Size { get; init; }
    public required bool EnableOnCreate { get; init; }
    public required World World { get; init; }
}
