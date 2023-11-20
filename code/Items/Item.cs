using Sandbox;
using Sandcube.Players;
using Sandcube.Registries;


namespace Sandcube.Items;

public class Item : IRegisterable
{
    public ModedId ModedId { get; }

    public Item(in ModedId id)
    {
        ModedId = id;
    }

    public void OnRegistered() {}

    public virtual InteractionResult OnAttack(SandcubePlayer player, PhysicsTraceResult traceResult)
    {
        var world = player.World;
        var blockPosition = world.GetBlockPosition(traceResult.EndPosition, traceResult.Normal);
        var blockState = world.GetBlockState(blockPosition);
        return blockState.Block.OnAttack(world, blockPosition, blockState, player);
    }
    public virtual InteractionResult OnUse(SandcubePlayer player, PhysicsTraceResult traceResult) => InteractionResult.Pass;
}
