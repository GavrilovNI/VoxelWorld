using VoxelWorld.Inventories;
using VoxelWorld.Items;
using VoxelWorld.Registries;
using System;
using VoxelWorld.Crafting.Recipes.Exceptions;
using VoxelWorld.Mods.Base;

namespace VoxelWorld.Crafting.Recipes;

public abstract class WorkbenchRecipe : Recipe<IIndexedCapability<Stack<Item>>, Stack<Item>>
{
    public int InputSize { get; }
    public Stack<Item> Output { get; }

    protected WorkbenchRecipe(in ModedId id, int inputSize, Stack<Item> output) : base(BaseMod.Instance!.RecipeTypes.Workbench, id)
    {
        if(inputSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(inputSize), inputSize, "should be positive");
        if(output.IsEmpty)
            throw new ArgumentException("stack is empty", nameof(output));

        InputSize = inputSize;
        Output = output;
    }

    public sealed override bool Matches(IIndexedCapability<Stack<Item>> input) => Matches((IReadOnlyIndexedCapability<Stack<Item>>)input);
    protected abstract bool Matches(IReadOnlyIndexedCapability<Stack<Item>> input);

    public sealed override Stack<Item> GetResult() => Output;

    public abstract void TakeInput(IIndexedCapability<Stack<Item>> input);

    public sealed override Stack<Item> Process(in IIndexedCapability<Stack<Item>> input)
    {
        InputDoesNotMatchRecipeException<IIndexedCapability<Stack<Item>>>.ThrowIfDoesNotMatchRecipe(input, this);

        TakeInput(input);
        return GetResult();
    }
}
