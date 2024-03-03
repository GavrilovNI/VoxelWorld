using Sandbox;
using Sandcube.Data;
using Sandcube.Entities;
using Sandcube.Entities.Types;
using Sandcube.IO.Helpers;
using Sandcube.Mods.Base;
using Sandcube.Mth;
using Sandcube.Players;
using Sandcube.Worlds;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Base;

public class PlayerSpawner : Component
{
    [Property] protected BBoxInt PreloadRange { get; set; } = BBoxInt.FromPositionAndRadius(0, new Vector3Int(2, 2, 3));
    [Property] protected BBoxInt SafeBounds { get; set; } = BBoxInt.FromMinsAndSize(0, new Vector3Int(1, 1, 2));
    protected static EntityType PlayerEntityType => SandcubeBaseMod.Instance!.Entities.Player;


    public virtual async Task<Player?> SpawnPlayer(ulong steamId, EntitySpawnConfig defaultSpawnConfig, CancellationToken cancellationToken)
    {
        if(!TryLoadPlayer(steamId, out var player, false))
            player = (PlayerEntityType.CreateEntity(defaultSpawnConfig with { StartEnabled = false }) as Player)!;
        player.SetSteamId(steamId);

        var world = player.World;
        if(world is not null)
        {
            if(cancellationToken.IsCancellationRequested)
                return null;

            var spawnPosition = player.Transform.Position;
            var spawnBlockPosition = world.GetBlockPosition(spawnPosition);
            spawnBlockPosition = await FindSafePosition(world, spawnBlockPosition, SafeBounds, cancellationToken);

            spawnPosition = world.GetBlockGlobalPosition(spawnBlockPosition) +
                new Vector3(MathV.UnitsInMeter / 2f, MathV.UnitsInMeter / 2f, 0);

            if(cancellationToken.IsCancellationRequested)
                return null;

            await world.LoadChunk(world.GetChunkPosition(spawnBlockPosition + Vector3Int.Down), true);

            if(cancellationToken.IsCancellationRequested)
                return null;

            player.Transform.Position = spawnPosition;

            await PreloadChunks(world, PreloadRange, spawnPosition);

            if(cancellationToken.IsCancellationRequested)
                return null;
        }

        foreach(var localPlayerInitializable in Scene.Components.GetAll<ILocalPlayerInitializable>(FindMode.EverythingInSelfAndDescendants))
            localPlayerInitializable.InitializeLocalPlayer(player);

        if(world is not null)
        {
            foreach(var worldInitializable in player.GameObject.Components.GetAll<IWorldInitializable>(FindMode.EverythingInSelfAndDescendants))
                worldInitializable.InitializeWorld(world);
        }

        player.Enabled = defaultSpawnConfig.StartEnabled;
        return player;
    }

    protected bool TryLoadPlayer(ulong steamId, out Player player, bool enable = true)
    {
        PlayerSaveHelper saveHelper = new(SandcubeGame.Instance!.CurrentGameSaveHelper!.PlayersFileSystem);

        if(!saveHelper.HasPlayerFile(steamId))
        {
            player = null!;
            return false;
        }

        using var stream = saveHelper.OpenPlayerRead(steamId);
        using var reader = new BinaryReader(stream);

        var playerData = PlayerData.Read(reader);
        player = playerData.CreatePlayer(enable);
        return true;
    }

    protected virtual async Task<Vector3Int> FindSafePosition(IWorldAccessor world, Vector3Int startPosition, BBoxInt range, CancellationToken cancellationToken)
    {
        while(!await IsEmpty(world, range + startPosition, cancellationToken))
            startPosition += Vector3Int.Up;

        while(await IsEmpty(world, range + startPosition, cancellationToken))
            startPosition += Vector3Int.Down;

        return startPosition + Vector3Int.Up;
    }

    protected virtual async Task<bool> IsEmpty(IWorldAccessor world, BBoxInt range, CancellationToken cancellationToken)
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

    protected virtual Task PreloadChunks(IWorldAccessor world, BBoxInt range, Vector3 spawnPosition)
    {
        Vector3Int centerChunkPosition = world.GetChunkPosition(spawnPosition);
        var positionsToLoad = (range + centerChunkPosition).GetPositions().ToHashSet();
        return world.LoadChunksSimultaneously(positionsToLoad, true);
    }
}
