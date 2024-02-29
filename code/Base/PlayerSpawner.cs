using Sandbox;
using Sandcube.Data.Enumarating;
using Sandcube.Entities;
using Sandcube.Entities.Types;
using Sandcube.Mods.Base;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Players;
using Sandcube.Worlds;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Base;

public class PlayerSpawner : Component
{
    [Property] protected BBoxInt SafeBounds { get; set; } = BBoxInt.FromMinsAndSize(0, new Vector3Int(1, 1, 2));
    protected static EntityType PlayerEntityType => SandcubeBaseMod.Instance!.Entities.Player;

    [Property] protected bool ClickToSpawnPlayer { get; set; } = false;

    public virtual async Task<Entity?> SpawnPlayer(EntitySpawnConfig spawnConfig, CancellationToken cancellationToken)
    {
        var world = spawnConfig.World;
        if(world.IsValid())
        {
            var spawnPosition = spawnConfig.Transform.Position;
            var spawnBlockPosition = world.GetBlockPosition(spawnPosition);
            spawnBlockPosition = await FindSafePosition(world, spawnBlockPosition, SafeBounds, cancellationToken);

            spawnPosition = world.GetBlockGlobalPosition(spawnBlockPosition) +
                new Vector3(MathV.UnitsInMeter / 2f, MathV.UnitsInMeter / 2f, 0);

            if(cancellationToken.IsCancellationRequested)
                return null;

            await world.LoadChunk(world.GetChunkPosition(spawnBlockPosition + Vector3Int.Down), true);

            if(cancellationToken.IsCancellationRequested)
                return null;

            spawnConfig.Transform.Position = spawnPosition;
        }
        return PlayerEntityType.CreateEntity(spawnConfig);
    }

    public virtual async Task<Vector3Int> FindSafePosition(World world, Vector3Int startPosition, BBoxInt range, CancellationToken cancellationToken)
    {
        while(!await IsEmpty(world, range + startPosition, cancellationToken))
            startPosition += Vector3Int.Up;

        while(await IsEmpty(world, range + startPosition, cancellationToken))
            startPosition += Vector3Int.Down;

        return startPosition + Vector3Int.Up;
    }

    public virtual async Task<bool> IsEmpty(World world, BBoxInt range, CancellationToken cancellationToken)
    {
        var limits = world.Limits;
        var blockMeshes = SandcubeGame.Instance!.BlockMeshes;
        foreach(var position in range.GetPositions())
        {
            if(cancellationToken.IsCancellationRequested)
                return false;

            if(!limits.Contains(position))
                continue;

            var chunkPosition = world.GetChunkPosition(position);
            await world.LoadChunk(chunkPosition);
            var state = world.GetBlockState(position);

            if(!state.IsAir() && !blockMeshes.GetPhysics(state)!.Bounds.Size.AlmostEqual(Vector3.Zero))
                return false;
        }
        return true;
    }

    protected override void OnUpdate()
    {
        if(ClickToSpawnPlayer)
        {
            ClickToSpawnPlayer = false;
            _ = SpawnTestPlayer();
        }
    }

    protected async Task SpawnTestPlayer()
    {
        var worlds = SandcubeGame.Instance!.Worlds;
        var world = worlds.Count == 0 ? null : worlds.First().Value;

        Vector3 position = world.IsValid() ? world.GetBlockGlobalPosition(new Vector3Int(0, 0, world.Limits.Maxs.z + 1)) : Vector3.Zero;
        EntitySpawnConfig spawnConfig = new(new Transform(position), world, false);

        var player = await SpawnPlayer(spawnConfig, CancellationToken.None);

        if(!player.IsValid())
            return;

        foreach(var localPlayerInitializable in Scene.Components.GetAll<ILocalPlayerInitializable>(FindMode.EverythingInSelfAndDescendants))
            localPlayerInitializable.InitializeLocalPlayer(player);

        if(world.IsValid())
        {
            foreach(var worldInitializable in player.GameObject.Components.GetAll<IWorldInitializable>(FindMode.EverythingInSelfAndDescendants))
                worldInitializable.InitializeWorld(world);
        }

        player.Enabled = true;
    }
}
