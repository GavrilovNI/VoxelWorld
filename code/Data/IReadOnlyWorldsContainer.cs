using VoxelWorld.Registries;
using VoxelWorld.Worlds;
using System.Collections.Generic;

namespace VoxelWorld.Data;

public interface IReadOnlyWorldsContainer : IEnumerable<World>
{
	int Count { get; }

	bool HasWorld(ModedId id);
    bool TryGetWorld(ModedId id, out World world);
    World GetWorld(ModedId id);
}
