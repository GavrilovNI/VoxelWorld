using Sandbox;
using System.Collections.Generic;
using VoxelWorld.Blocks.Entities;
using VoxelWorld.Entities;
using VoxelWorld.Inventories;
using VoxelWorld.Inventories.Players;
using VoxelWorld.Items;
using VoxelWorld.Mth;
using VoxelWorld.UI.Screens;
using VoxelWorld.Worlds;

namespace VoxelWorld.Menus;

public class WorkbenchMenu : ItemCapabilitiesMenu
{
    public IWorldAccessor? World { get; }
    public Vector3Int BlockPosition { get; }
    public IPlayerInventory PlayerInventory { get; }
    public WorkbenchCapability WorkbenchCapability { get; } = new();

    public WorkbenchMenu(Player player) : this(player, null!, Vector3Int.Zero)
    {
    }

    public WorkbenchMenu(Player player, IWorldAccessor world, Vector3Int blockPosition) : base(new List<IIndexedCapability<Inventories.Stack<Item>>>()
    {
        player.Inventory.Main,
        player.Inventory.Hotbar
    }, player)
    {
        Capabilities.Insert(0, WorkbenchCapability);
        PlayerInventory = player.Inventory;
        World = world;
        BlockPosition = blockPosition;
    }

    public override bool IsStillValid()
    {
        if(PlayerInventory != Player.Inventory)
            return false;

        if(World is null)
            return true;

        var globalPosition = World.GetBlockGlobalPosition(BlockPosition);
        var bbox = new BBox(globalPosition, globalPosition + new Vector3(MathV.UnitsInMeter));
        var closestPoint = bbox.ClosestPoint(Player.Transform.Position);
        var distanceSquared = closestPoint.DistanceSquared(Player.Transform.Position);
        return distanceSquared <= Player.ReachDistance * Player.ReachDistance;
    }

    public GameObject CreateScreen()
    {
        var gameObject = new GameObject();
        var screen = gameObject.Components.Create<WorkbenchScreen>();
        screen.Menu = this;
        return gameObject;
    }
}
