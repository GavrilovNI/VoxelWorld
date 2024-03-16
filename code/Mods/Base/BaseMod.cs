﻿using Sandbox;
using VoxelWorld.Blocks;
using VoxelWorld.Mods.Base.Blocks;
using VoxelWorld.Mods.Base.Blocks.Entities;
using VoxelWorld.Mods.Base.Entities;
using VoxelWorld.Mods.Base.Items;
using VoxelWorld.Registries;
using System.Threading.Tasks;
using VoxelWorld.Mods.Base.Recipes;
using VoxelWorld.Crafting.Recipes;
using VoxelWorld.Items;
using VoxelWorld.Inventories;

namespace VoxelWorld.Mods.Base;

public sealed class BaseMod : Component, IMod
{
    public const string ModName = "voxelworld";
    public static BaseMod? Instance { get; private set; }

    public new Id Id { get; } = new(ModName);

    public BaseModBlocks Blocks { get; private set; } = null!;
    public BaseModBlockEntities BlockEntities { get; private set; } = null!;
    public BaseModItems Items { get; private set; } = null!;
    public BaseModEntities Entities { get; private set; } = null!;
    public BaseRecipeTypes RecipeTypes { get; private set; } = null!;

    private readonly ModedId _mainWorldId = new(ModName, "main");

    protected override void OnAwake()
    {
        if(Instance is not null)
        {
            Log.Warning($"{nameof(Scene)} {Scene} has to much instances of {nameof(GameController)}. Destroying {this}...");
            Destroy();
            return;
        }

        Instance = this;

        Blocks = new();
        BlockEntities = new();
        Items = new();
        Entities = Components.Get<BaseModEntities>(true);
        RecipeTypes = new();
    }

    protected override void OnDestroy()
    {
        if(Instance != this)
            return;

        Instance = null;
    }

    public async Task RegisterValues(RegistriesContainer registries)
    {
        RegistriesContainer container = new();

        await Blocks.Register(registries);
        await BlockEntities.Register(registries);
        GameController.Instance!.RebuildBlockMeshes(registries.GetRegistry<Block>());
        await Items.Register(registries);
        await Entities.Register(registries);
        await RecipeTypes.Register(registries);

        registries.Add(container);
    }

    private static ModedId MakeId(string id) => new(ModName, id);

    public void RegisterRecipes(RecipesContainer recipesContainer)
    {
        recipesContainer.AddRecipe(new WorkbenchShaplessRecipe(MakeId("wood_log_to_planks"),
            new Item[1] { Items.WoodLog }, new Stack<Item>(Items.WoodPlanks, 6)));
    }

    public void OnGameLoaded()
    {
        GameController.Instance!.TryAddWorld(_mainWorldId, out _);
    }
}
