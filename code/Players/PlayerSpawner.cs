using Sandbox;
using VoxelWorld.Entities;
using VoxelWorld.Entities.Types;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.Mods.Base;
using VoxelWorld.Mth;
using VoxelWorld.Worlds;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VoxelWorld.Players;

public class PlayerSpawner : Component
{
    [Property] protected BBoxInt PreloadRange { get; set; } = BBoxInt.FromPositionAndRadius(0, new Vector3IntB(2, 2, 3));
    [Property] protected BBoxInt SafeBounds { get; set; } = BBoxInt.FromMinsAndSize(0, new Vector3IntB(1, 1, 2));
    protected static EntityType PlayerEntityType => BaseMod.Instance!.Entities.Player;


    public virtual async Task<Player?> SpawnPlayer(ulong steamId, EntitySpawnConfig defaultSpawnConfig, CancellationToken cancellationToken)
    {
        bool shouldTryLoadPLayer = !GameController.Instance!.WasPlayerSpawned(steamId);
        if(!shouldTryLoadPLayer || !TryLoadPlayer(steamId, out var player, false))
        {
            player = (PlayerEntityType.CreateEntity(defaultSpawnConfig with { StartEnabled = false }) as Player)!;
            player.SetSteamId(steamId);
        }

        var world = player.World;
        if(world is not null)
        {
            if(cancellationToken.IsCancellationRequested)
                return null;

            var spawnPosition = player.Transform.Position;
            var spawnBlockPosition = world.GetBlockPosition(spawnPosition);

            if(cancellationToken.IsCancellationRequested)
                return null;

            var currentBounds = SafeBounds + spawnBlockPosition;
            if(world.Limits.Overlaps(currentBounds))
            {
                if(!await IsEmpty(world, currentBounds, cancellationToken))
                {
                    if(cancellationToken.IsCancellationRequested)
                        return null;

                    spawnBlockPosition = await FindSafePosition(world, spawnBlockPosition, SafeBounds, cancellationToken);

                    spawnPosition = world.GetBlockGlobalPosition(spawnBlockPosition) +
                        new Vector3(MathV.UnitsInMeter / 2f, MathV.UnitsInMeter / 2f, 0);
                }
            }

            if(cancellationToken.IsCancellationRequested)
                return null;

            await world.CreateChunk(world.GetChunkPosition(spawnBlockPosition + Vector3IntB.Down));

            if(cancellationToken.IsCancellationRequested)
                return null;

            player.Transform.Position = spawnPosition;

            await PreloadChunks(world, PreloadRange, spawnPosition);

            if(cancellationToken.IsCancellationRequested)
                return null;
        }

        if(world is not null)
        {
            foreach(var worldInitializable in player.GameObject.Components.GetAll<IWorldInitializable>(FindMode.EverythingInSelfAndDescendants))
                worldInitializable.InitializeWorld(world);
        }

        player.Enabled = defaultSpawnConfig.StartEnabled;
        return player;
    }

    protected virtual bool TryLoadPlayer(ulong steamId, out Player player, bool enable = true)
    {
        var fileSystem = GameController.Instance!.CurrentGameSaveHelper!.PlayersFileSystem;

        if(!fileSystem.FileExists(steamId.ToString()))
        {
            player = null!;
            return false;
        }

        BinaryTag tag;
        using(var stream = fileSystem.OpenRead(steamId.ToString()))
        {
            using var reader = new BinaryReader(stream);
            tag = BinaryTag.Read(reader);
        }

        return Player.TryReadPlayer(tag, steamId, out player, enable);
    }

    protected virtual async Task<Vector3IntB> FindSafePosition(IWorldAccessor world, Vector3IntB startPosition, BBoxInt range, CancellationToken cancellationToken)
    {
        var currentRange = range + startPosition;
        while(world.Limits.Overlaps(currentRange) && !await IsEmpty(world, currentRange, cancellationToken))
        {
            startPosition += Vector3IntB.Up;
            currentRange = range + startPosition;
        }

        if(!world.Limits.Overlaps(currentRange))
        {
            startPosition += Vector3IntB.Down;
            currentRange = range + startPosition;

            if(!world.Limits.Overlaps(currentRange))
                return startPosition + Vector3IntB.Up;
        }

        while(world.Limits.Overlaps(currentRange) && await IsEmpty(world, currentRange, cancellationToken))
        {
            startPosition += Vector3IntB.Down;
            currentRange = range + startPosition;
        }

        return startPosition + Vector3IntB.Up;
    }

    protected virtual async Task<bool> IsEmpty(IWorldAccessor world, BBoxInt range, CancellationToken cancellationToken)
    {
        var limits = world.Limits;
        var blockMeshes = GameController.Instance!.BlockMeshes;
        foreach(var position in range.GetPositions(false))
        {
            if(cancellationToken.IsCancellationRequested)
                return false;

            if(!limits.Contains(position))
                continue;

            var chunkPosition = world.GetChunkPosition(position);
            await world.CreateChunk(chunkPosition);
            var state = world.GetBlockState(position);

            if(!state.IsAir() && !blockMeshes.GetPhysics(state)!.Bounds.Size.AlmostEqual(Vector3.Zero))
                return false;
        }
        return true;
    }

    protected virtual Task PreloadChunks(IWorldAccessor world, BBoxInt range, Vector3 spawnPosition)
    {
        var centerChunkPosition = world.GetChunkPosition(spawnPosition);
        var positionsToLoad = (range + centerChunkPosition).GetPositions().Where(p => world.IsChunkInLimits(p)).ToHashSet();

        return world.CreateChunksSimultaneously(positionsToLoad);
    }
}
