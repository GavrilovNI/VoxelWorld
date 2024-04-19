using VoxelWorld.Blocks.States.Properties;
using VoxelWorld.Mth.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VoxelWorld.Blocks.States;

public sealed class BlockStateSet : IEnumerable<BlockState>
{
    public readonly Block Block;
    private readonly HashSet<BlockStateProperty> _properties = new();
    private readonly List<BlockState> _blockStates = new();

    public BlockStateSet(Block block, IEnumerable<BlockStateProperty> properties)
    {
        Block = block;
        foreach(var property in properties)
        {
            if(_properties.FirstOrDefault(p => p!.Id == property.Id, null) is not null)
                throw new ArgumentException("Can't create difinition with 2 or more properties with same name", nameof(properties));
            _properties.Add(property);
        }
        _blockStates = GenerateBlockStates();
    }

    public BlockState FindBlockState(IReadOnlyDictionary<BlockStateProperty, CustomEnum> stateProperties)
    {
        if(!_properties.SetEquals(stateProperties.Keys))
            throw new ArgumentException($"not all {nameof(BlockStateProperty)} are provided", nameof(stateProperties));

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
            states.Add(new BlockState(Block, new Dictionary<BlockStateProperty, CustomEnum>()));
            return states;
        }

        Dictionary<BlockStateProperty, CustomEnum> defaultStateProperties = new();
        foreach(var property in _properties)
        {
            var allValues = property.GetAllValues();
            if(allValues.Any())
                defaultStateProperties.Add(property, allValues.First());
        }

        List<Dictionary<BlockStateProperty, CustomEnum>> allStateProperties = new();
        allStateProperties.Add(defaultStateProperties);

        List<BlockStateProperty> leftProperties = new(_properties);
        for(int i = leftProperties.Count - 1; i >= 0; --i)
        {
            var property = leftProperties[i];
            int allStatePropertiesCount = allStateProperties.Count;
            for(int j = 0; j < allStatePropertiesCount; ++j)
            {
                foreach(var value in property.GetAllValues())
                {
                    Dictionary<BlockStateProperty, CustomEnum> newProperties = new(allStateProperties[j])
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
