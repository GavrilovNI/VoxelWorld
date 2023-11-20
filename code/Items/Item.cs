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
}
