using Sandbox;
using Sandcube.Items;
using Sandcube.Players;

namespace Sandcube.Interactions;

public record class ItemActionContext
{
    public required SandcubePlayer Player { get; init; }
    public required Item Item { get; init; }
    public required PhysicsTraceResult TraceResult { get; init; }

    public ItemActionContext()
    {

    }
}
