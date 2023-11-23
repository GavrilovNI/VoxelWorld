using Sandbox;
using Sandcube.Interactions;
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

    public virtual void OnRegistered() { }

    public virtual InteractionResult OnAttack(ItemActionContext context)
    {
        BlockActionContext blockActionContext = new(context);
        return blockActionContext.BlockState.Block.OnAttack(blockActionContext);
    }
    public virtual InteractionResult OnUse(ItemActionContext context) => InteractionResult.Pass;
}
