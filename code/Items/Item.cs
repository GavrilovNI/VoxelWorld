using Sandbox;
using Sandcube.Interactions;
using Sandcube.Inventories;
using Sandcube.Mth;
using Sandcube.Registries;

namespace Sandcube.Items;

public class Item : IRegisterable, IStackValue
{
    public ModedId ModedId { get; }
    public Texture Texture { get; }
    public int StackLimit { get; }

    public Item(in ModedId id, Texture texture, int stackLimit)
    {
        ModedId = id;
        Texture = texture;
        StackLimit = stackLimit;
    }

    public Item(in ModedId id, Texture texture) : this(id, texture, DefaultValues.ItemStackLimit)
    {
    }

    public virtual void OnRegistered() { }

    public virtual int GetStackLimit() => StackLimit;

    public virtual InteractionResult OnAttack(ItemActionContext context) => InteractionResult.Pass;
    public virtual InteractionResult OnUse(ItemActionContext context) => InteractionResult.Pass;

    public override int GetHashCode() => HashCode.Combine(ModedId, StackLimit);
}
