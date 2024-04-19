using Sandbox;
using VoxelWorld.Entities;
using VoxelWorld.Items;

namespace VoxelWorld.Interactions;

public record class ItemActionContext
{
    public required Player Player { get; init; }
    public required Item Item { get; init; }
    public required HandType HandType { get; init; }
    public required SceneTraceResult TraceResult { get; init; }

    public ItemActionContext()
    {

    }
}
