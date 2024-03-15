using Sandbox;
using VoxelWorld.Data;
using VoxelWorld.Entities;
using VoxelWorld.Entities.Types;
using VoxelWorld.IO.Helpers;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.Mods.Base;
using VoxelWorld.Mth;
using VoxelWorld.Worlds;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VoxelWorld.Players;

public class PlayerSpawner : Component
{
    [Property] protected BBoxInt PreloadRange { get; set; } = BBoxInt.FromPositionAndRadius(0, new Vector3Int(2, 2, 3));
    [Property] protected BBoxInt SafeBounds { get; set; } = BBoxInt.FromMinsAndSize(0, new Vector3Int(1, 1, 2));
    protected static EntityType PlayerEntityType => BaseMod.Instance!.Entities.Player;


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

            await world.CreateChunk(world.GetChunkPosition(spawnBlockPosition + Vector3Int.Down));

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

        if(!Entity.TryReadWithWorld(tag, out var entity, enable))
        {
            player = null!;
            return false;
        }

        if(entity is not Player playerEntity)
        {
            player = null!;
            entity.Destroy();
            return false;
        }

        player = playerEntity;
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
        var blockMeshes = GameController.Instance!.BlockMeshes;
        foreach(var position in range.GetPositions())
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
        Vector3Int centerChunkPosition = world.GetChunkPosition(spawnPosition);
        var positionsToLoad = (range + centerChunkPosition).GetPositions().ToHashSet();

        return world.CreateChunksSimultaneously(positionsToLoad);
    }
}
