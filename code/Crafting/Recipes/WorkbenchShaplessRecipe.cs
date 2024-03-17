using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using VoxelWorld.Inventories;
using VoxelWorld.Items;
using VoxelWorld.Registries;

namespace VoxelWorld.Crafting.Recipes;

public class WorkbenchShaplessRecipe : WorkbenchRecipe
{
    public ImmutableArray<Item> Input { get; }

    public WorkbenchShaplessRecipe(in ModedId id, Item[] input, Inventories.Stack<Item> output) : base(id, input.Length, output)
    {
        Input = ImmutableArray.Create(input);
    }

    protected override bool Matches(IReadOnlyIndexedCapability<Inventories.Stack<Item>> input)
    {
        if(input.Size < InputSize)
            return false;

        var leftItems = Input.ToList();
        foreach(var itemStack in input)
        {
            if(itemStack.IsEmpty)
                continue;

            if(!leftItems.Remove(itemStack.Value!))
                return false;
        }

        return leftItems.Count == 0;
    }

    public override void TakeInput(IIndexedCapability<Inventories.Stack<Item>> input)
    {
        foreach(var inputItem in Input)
            input.TryExtract(new Inventories.Stack<Item>(inputItem));
    }
}
