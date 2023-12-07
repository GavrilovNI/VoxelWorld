﻿using Sandbox;
using Sandcube.Blocks;
using Sandcube.Events;
using Sandcube.Items;
using Sandcube.Registries;
using Sandcube.Worlds;
using Sandcube.Worlds.Generation;

namespace Sandcube;

public class SandcubeGame : Component, ISandcubeMod
{
    public const string ModName = "sandcube";

    public static SandcubeGame? Instance { get; private set; } = null;
    public static bool IsStarted => Instance?.Started ?? false;

    public Id Id { get; private set; } = new(ModName);

    public bool Started { get; private set; } = false;

    [Property] public World World { get; private set; } = null!;
    public Registry<Block> BlocksRegistry { get; private set; } = new ();
    public Registry<Item> ItemsRegistry { get; private set; } = new ();
    public TextureMap TextureMap { get; private set; } = new ();
    public SandcubeBlocks Blocks { get; private set; } = new();
    public SandcubeItems Items { get; private set; } = new();
    public BlockMeshMap BlockMeshes { get; private set; } = new();

    protected override void OnStart()
    {
        Event.Register(this);
        Instance = this;
        Prepare();

        Started = true;
        Event.Run(SandcubeEvent.Game.Start);
    }

    protected override void OnDestroy()
    {
        if(Instance == this)
        {
            Event.Run(SandcubeEvent.Game.Stop);
            Started = false;
            Instance = null;
        }
        Event.Unregister(this);

    }

    protected virtual void Prepare()
    {
        RegisterAllBlocks();
        RegisterAllItems();
        RebuildBlockMeshes();
    }

    [Event.Hotload]
    protected virtual void OnHotload()
    {
        Log.Info("Hotload");
        Instance = this;
        Prepare();
    }

    private void RebuildBlockMeshes()
    {
        BlockMeshes.Clear();
        foreach(var block in BlocksRegistry.All)
        {
            foreach(var blockState in block.Value.BlockStateSet)
                BlockMeshes.Add(blockState);
        }
    }

    private void RegisterAllBlocks()
    {
        BlocksRegistry.Clear();
        RegisterBlocks(BlocksRegistry);
    }

    private void RegisterAllItems()
    {
        ItemsRegistry.Clear();
        RegisterItems(ItemsRegistry);
    }

    public void RegisterBlocks(Registry<Block> registry)
    {
        Blocks.Register(registry);
    }

    public void RegisterItems(Registry<Item> registry)
    {
        Items.Register(registry);
    }
}
