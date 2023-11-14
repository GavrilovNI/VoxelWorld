using Sandcube.Mth;
using Sandcube.Worlds.Blocks.States.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Blocks.States;

public sealed class BlockStateSet : IEnumerable<BlockState>
{
    public readonly Block Block;
    private readonly HashSet<BlockProperty> _properties = new();
    private readonly List<BlockState> _blockStates = new();

    public BlockStateSet(Block block, IEnumerable<BlockProperty> properties)
    {
        Block = block;
        foreach(var property in properties)
        {
            if(_properties.FirstOrDefault(p => p.Name == property.Name, null) is not null)
                throw new ArgumentException("Can't create difinition with 2 or more properties with same name", nameof(properties));
            _properties.Add(property);
        }
        _blockStates = GenerateBlockStates();
    }

    public BlockState FindBlockState(IReadOnlyDictionary<BlockProperty, CustomEnum> stateProperties)
    {
        if(!_properties.SetEquals(stateProperties.Keys))
            throw new ArgumentException($"not all {nameof(BlockProperty)} are provided", nameof(stateProperties));

        foreach(var state in _blockStates)
        {
            bool allEqual = true;
            foreach(var propertyEntry in stateProperties)
            {
                if(state._properties[propertyEntry.Key] != propertyEntry.Value)
                {
                    allEqual = false;
                    break;
                }
            }
            if(allEqual)
                return state;
        }

        throw new InvalidOperationException("couldn't find blockstate with valid state proeprties");
    }

    private List<BlockState> GenerateBlockStates()
    {
        List<BlockState> states = new();

        if(_properties.Count == 0)
        {
            states.Add(new BlockState(Block, new Dictionary<BlockProperty, CustomEnum>()));
            return states;
        }

        Dictionary<BlockProperty, CustomEnum> defaultStateProperties = new();
        foreach(var property in _properties)
            defaultStateProperties.Add(property, property.DefaultValue);

        List<Dictionary<BlockProperty, CustomEnum>> allStateProperties = new();
        allStateProperties.Add(defaultStateProperties);

        List<BlockProperty> leftProperties = new(_properties);
        for(int i = leftProperties.Count - 1; i >= 0; --i)
        {
            var property = leftProperties[i];
            int allStatePropertiesCount = allStateProperties.Count;
            for(int j = 0; j < allStatePropertiesCount; ++j)
            {
                foreach(var value in property.GetAllValues())
                {
                    Dictionary<BlockProperty, CustomEnum> newProperties = new(allStateProperties[j])
                    {
                        [property] = value
                    };

                    allStateProperties.Add(newProperties);
                    states.Add(new BlockState(Block, newProperties));
                }
            }
            leftProperties.RemoveAt(i);
        }

        return states;
    }


    public IEnumerator<BlockState> GetEnumerator() => _blockStates.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _blockStates.GetEnumerator();
}
