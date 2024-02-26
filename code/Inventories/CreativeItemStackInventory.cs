using Sandcube.Items;
using Sandcube.Registries;
using System.Collections.Generic;
using System.Linq;

namespace Sandcube.Inventories;

public class CreativeItemStackInventory : IndexedCapability<Stack<Item>>
{
    public readonly static CreativeItemStackInventory Instance = new();

    protected static IReadOnlyDictionary<ModedId, Item> Items => SandcubeGame.Instance!.Registries.GetRegistry<Item>().All;

    public override int Size => Items.Count;

    public override Stack<Item> Get(int index)
    {
        var item = Items.Values.Skip(index).Take(1).First();
        return new Stack<Item>(item, item.StackLimit);
    }
    public override int SetMax(int index, Stack<Item> stack, bool simulate = false) => stack.Count;
    protected override Stack<Item> GetEmpty() => Stack<Item>.Empty;
}
