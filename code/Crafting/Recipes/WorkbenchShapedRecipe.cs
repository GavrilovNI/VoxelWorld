using Sandbox;
using System;
using System.Collections.Immutable;
using VoxelWorld.Inventories;
using VoxelWorld.Items;
using VoxelWorld.Mth;
using VoxelWorld.Registries;

namespace VoxelWorld.Crafting.Recipes;

public class WorkbenchShapedRecipe : WorkbenchRecipe
{
    public Vector2IntB Size { get; }
    public int NotEmptyInputSize { get; }
    public ImmutableArray<ImmutableArray<Item?>> Input { get; }
    public bool CanMirrorX { get; }
    public bool CanMirrorY { get; }

    public WorkbenchShapedRecipe(in ModedId id, Item?[,] input, Inventories.Stack<Item> output, bool canMirrorX = true, bool canMirrorY = true) : base(id, input.Length, output)
    {
        AssertInput();

        ImmutableArray<Item?>[] rows = new ImmutableArray<Item?>[input.GetLength(0)];

        int notEmptyInputSize = 0;
        for(int y = 0; y < input.GetLength(0); ++y)
        {
            Item?[] row = new Item?[input.GetLength(1)];
            for(int x = 0; x < input.GetLength(1); ++x)
            {
                row[x] = input[y, x];
                if(input[y, x] is not null)
                    notEmptyInputSize++;
            }
            rows[y] = ImmutableArray.Create(row);
        }

        NotEmptyInputSize = notEmptyInputSize;
        Input = ImmutableArray.Create(rows);
        Size = new(input.GetLength(1), input.GetLength(0));
        CanMirrorX = canMirrorX;
        CanMirrorY = canMirrorY;

        void AssertInput()
        {
            bool sideIsEmpty = true;
            for(int i = 0; i < input.GetLength(0); ++i)
                sideIsEmpty &= input[i, 0] is null;
            if(sideIsEmpty)
                throw new ArgumentException("one or more sides are empty", nameof(input));

            sideIsEmpty = true;
            for(int i = 0; i < input.GetLength(0); ++i)
                sideIsEmpty &= input[i, input.GetLength(1) - 1] is null;
            if(sideIsEmpty)
                throw new ArgumentException("one or more sides are empty", nameof(input));

            sideIsEmpty = true;
            for(int i = 0; i < input.GetLength(1); ++i)
                sideIsEmpty &= input[0, i] is null;
            if(sideIsEmpty)
                throw new ArgumentException("one or more sides are empty", nameof(input));

            sideIsEmpty = true;
            for(int i = 0; i < input.GetLength(1); ++i)
                sideIsEmpty &= input[input.GetLength(0) - 1, i] is null;
            if(sideIsEmpty)
                throw new ArgumentException("one or more sides are empty", nameof(input));
        }
    }

    public virtual bool Matches(IReadOnlyIndexedCapability<Inventories.Stack<Item>> input, Vector2IntB gridSize, Vector2IntB offset, bool mirrorX, bool mirrorY)
    {
        if(input.Size < NotEmptyInputSize)
            return false;

        if(gridSize.IsAnyAxis((a, v) => v < Size.GetAxis(a)))
            return false;

        int index = offset.y * gridSize.x + offset.x;
        for(int i = 0; i < index; ++i)
        {
            if(!getGivenInputOrEmpty(i).IsEmpty)
                return false;
        }

        for(int y = mirrorY ? Size.y - 1 : 0; mirrorY ? y >= 0 : y < Size.y; y += mirrorY ? -1 : 1)
        {
            for(int x = mirrorX ? Size.x - 1 : 0; mirrorX ? x >= 0 : x < Size.x; x += mirrorX ? -1 : 1)
            {
                var givenItem = getGivenInputOrEmpty(index++).Value;
                var requiredItem = Input[y][x];

                if(requiredItem is null)
                {
                    if(givenItem is not null)
                        return false;
                }
                else
                {
                    if(givenItem is null || !requiredItem.Equals(givenItem))
                        return false;
                }
            }

            for(int i = 0; i < gridSize.x - Size.x; ++i)
            {
                var givenItem = getGivenInputOrEmpty(index++).Value;
                if(givenItem is not null)
                    return false;
            }
        }

        for(; index < input.Size; ++index)
        {
            if(!input.Get(index).IsEmpty)
                return false;
        }
        return true;

        Inventories.Stack<Item> getGivenInputOrEmpty(int index)
        {
            if(index < 0 || index >= input.Size)
                return Inventories.Stack<Item>.Empty;
            return input.Get(index);
        }
    }

    public virtual bool Matches(IReadOnlyIndexedCapability<Inventories.Stack<Item>> input, Vector2IntB gridSize, Vector2IntB offset)
    {
        if(input.Size < NotEmptyInputSize)
            return false;

        if(input.Size < InputSize)
            return false;

        if(Matches(input, gridSize, offset, false, false))
            return true;

        bool shouldMirrorX = CanMirrorX && gridSize.x > 1;
        bool shouldMirrorY = CanMirrorY && gridSize.y > 1;

        if(shouldMirrorX && Matches(input, gridSize, offset, true, false))
            return true;

        if(shouldMirrorY && Matches(input, gridSize, offset, false, true))
            return true;

        if(shouldMirrorX && shouldMirrorY && Matches(input, gridSize, offset, true, true))
            return true;

        return false;
    }


    protected override bool Matches(IReadOnlyIndexedCapability<Inventories.Stack<Item>> input)
    {
        if(input.Size < NotEmptyInputSize)
            return false;

        if(input.Size < InputSize)
            return false;

        int width = MathF.Sqrt(input.Size).CeilToInt();
        var gridSize = new Vector2IntB(width, width);

        if(gridSize.IsAnyAxis((a, v) => v < Size.GetAxis(a)))
            return false;

        for(int x = 0; x <= gridSize.x - Size.x; ++x)
        {
            for(int y = 0; y <= gridSize.y - Size.y; ++y)
            {
                var offset = new Vector2IntB(x, y);
                if(Matches(input, gridSize, offset))
                    return true;
            }
        }

        return false;
    }

    public override void TakeInput(IIndexedCapability<Inventories.Stack<Item>> input)
    {
        foreach(var inputItems in Input)
        {
            foreach(var inputItem in inputItems)
            {
                if(inputItem is not null)
                    input.TryExtract(new Inventories.Stack<Item>(inputItem));
            }
        }
    }
}
