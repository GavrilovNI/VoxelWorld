using Sandcube.Interactions;
using Sandcube.Inventories;
using Sandcube.Mth;
using Sandcube.Registries;

namespace Sandcube.Items;

public class Item : IRegisterable, IStackValue
{
    public ModedId ModedId { get; }
    public int StackLimit { get; }

    public Item(in ModedId id, int stackLimit)
    {
        ModedId = id;
        StackLimit = stackLimit;
    }

    public Item(in ModedId id) : this(id, DefaultValues.ItemStackLimit)
    {
    }

    public virtual void OnRegistered() { }

    public virtual int GetStackLimit() => StackLimit;

    public virtual InteractionResult OnAttack(ItemActionContext context) => InteractionResult.Pass;
    public virtual InteractionResult OnUse(ItemActionContext context) => InteractionResult.Pass;
}
