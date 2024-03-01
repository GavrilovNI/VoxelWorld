using Sandbox;
using Sandcube.Blocks;
using Sandcube.Entities;
using Sandcube.Mods.Base.Blocks;
using Sandcube.Mods.Base.Entities;
using Sandcube.Mods.Base.Items;
using Sandcube.Registries;
using Sandcube.Worlds;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Mods.Base;

public sealed class SandcubeBaseMod : Component, ISandcubeMod
{
    public const string ModName = "sandcube";
    public static SandcubeBaseMod? Instance => SandcubeGame.Instance?.BaseMod;

    public Id Id { get; } = new(ModName);

    public SandcubeBlocks Blocks { get; private set; } = null!;
    public SandcubeItems Items { get; private set; } = null!;
    public SandcubeEntities Entities { get; private set; } = null!;

    private readonly ModedId _mainWorldId = new(ModName, "main");

    protected override void OnAwake()
    {
        Blocks = new();
        Items = new();
        Entities = Components.Get<SandcubeEntities>(true);
    }

    public void OnLoaded()
    {
        SandcubeGame.WorldAdded += OnWorldAdded;
        if(SandcubeGame.Instance!.Worlds.TryGetWorld(_mainWorldId, out var world))
            OnWorldAdded(world);
    }
    public void OnUnloaded()
    {
        SandcubeGame.WorldAdded -= OnWorldAdded;
    }

    public async Task RegisterValues(RegistriesContainer registries)
    {
        RegistriesContainer container = new();

        await Blocks.Register(registries);
        SandcubeGame.Instance!.RebuildBlockMeshes(registries.GetRegistry<Block>());
        await Items.Register(registries);
        await Entities.Register(registries);

        registries.Add(container);
    }

    public void OnGameLoaded()
    {
        SandcubeGame.Instance!.TryAddWorld(_mainWorldId, out _);
    }

    private void OnWorldAdded(World world)
    {
        if(world.Id != _mainWorldId)
            return;

        EntitySpawnConfig spawnConfig = new(world, true);
        _ = SandcubeGame.Instance!.PlayerSpawner.SpawnPlayer(spawnConfig, CancellationToken.None);
    }
}
