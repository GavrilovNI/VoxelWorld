using System;
using System.Collections.Generic;
using System.Linq;
using VoxelWorld.Crafting.Recipes;
using VoxelWorld.Items;
using VoxelWorld.Mods.Base;

namespace VoxelWorld.Inventories;

public class WorkbenchCapability : IndexedCapability<Stack<Item>>
{
    private readonly Dictionary<int, Stack<Item>> _stacks = new();
    public override int Size { get; }
    public Recipe<IIndexedCapability<Stack<Item>>, Stack<Item>>? CurrentRecipe { get; protected set; }

    public Stack<Item> Output
    {
        get => _stacks.GetValueOrDefault(Size - 1, Stack<Item>.Empty);
        set => Set(Size - 1, value);
    }

    public IIndexedCapability<Stack<Item>> Inputs { get; }

    private int? _stacksHashCode = null;
    protected int StacksHashCode
    {
        get
        {
            if(_stacksHashCode == null)
            {
                _stacksHashCode = 0;
                foreach(var (index, stack) in _stacks)
                    _stacksHashCode = HashCode.Combine(_stacksHashCode, index, stack);
            }
            return _stacksHashCode.Value;
        }
    }

    public WorkbenchCapability(int inputsCount = 9)
    {
        if(inputsCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(inputsCount), inputsCount, "should be positive");

        Size = inputsCount + 1;
        Inputs = new IndexedCapabilityPart<Stack<Item>>(this, inputsCount);
    }

    public override Stack<Item> Get(int index) => _stacks.GetValueOrDefault(index, Stack<Item>.Empty);

    protected virtual void Set(int index, Stack<Item> stack)
    {
        _stacks[index] = stack;
        _stacksHashCode = null;

        if(index != Size - 1)
            UpdateRecipe();
    }

    public override int SetMax(int index, Stack<Item> stack, bool simulate = false)
    {
        if(index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException(nameof(index));
        if(index == Size - 1)
            return 0;

        var limit = GetSlotLimit(index, stack);
        var maxCountToSet = Math.Min(limit, stack.Count);
        if(!simulate)
            Set(index, stack.WithCount(maxCountToSet));

        return maxCountToSet;
    }

    protected override Stack<Item> GetEmpty() => Stack<Item>.Empty;

    public override Stack<Item> ExtractMax(int index, int count, bool simulate = false)
    {
        if(index != Size - 1)
            return base.ExtractMax(index, count, simulate);

        if(count < Output.Count || CurrentRecipe is null)
            return Stack<Item>.Empty;

        var result = Output;
        if(!simulate)
            Process();

        return result;
    }

    protected virtual void UpdateRecipe()
    {
        var recipes = GameController.Instance!.Recipes
            .GetRecipes(BaseMod.Instance!.RecipeTypes.Workbench)
            .Cast<Recipe<IIndexedCapability<Stack<Item>>, Stack<Item>>>();

        foreach(var recipe in recipes)
        {
            if(recipe.Matches(Inputs))
            {
                CurrentRecipe = recipe;
                Output = recipe.GetResult();
                return;
            }
        }
        Output = Stack<Item>.Empty;
    }

    protected virtual void Process() => CurrentRecipe!.Process(Inputs);
}
