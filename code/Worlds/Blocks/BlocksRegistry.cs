using Sandcube.Registries;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sandcube.Worlds.Blocks;

public class BlocksRegistry
{
    private readonly Dictionary<ModedId, Block> _blocks = new();
    public ReadOnlyDictionary<ModedId, Block> All => _blocks.AsReadOnly();

    public void Clear() => _blocks.Clear();

    public void Add(Block block)
    {
        if(_blocks.ContainsKey(block.Id))
            throw new InvalidOperationException($"{nameof(Block)} with {nameof(ModedId)} '{block.Id
                }' already registered!");
        _blocks.Add(block.Id, block);
        block.OnRegistered();
    }

    public Block? Get(ModedId id) => _blocks!.GetValueOrDefault(id, null);
}
