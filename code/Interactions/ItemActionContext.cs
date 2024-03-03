using Sandbox;
using Sandcube.Entities;
using Sandcube.Items;

namespace Sandcube.Interactions;

public record class ItemActionContext
{
    public required Player Player { get; init; }
    public required Item Item { get; init; }
    public required HandType HandType { get; init; }
    public required PhysicsTraceResult TraceResult { get; init; }

    public ItemActionContext()
    {

    }
}
