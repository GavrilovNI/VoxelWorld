﻿using Sandbox;
using Sandcube.Entities;
using Sandcube.Entities.Types;
using Sandcube.Mods.Base;
using Sandcube.Mth;
using Sandcube.Players;
using Sandcube.Worlds;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Base;

public class PlayerSpawner : Component
{
    [Property] protected BBoxInt PreloadRange { get; set; } = BBoxInt.FromPositionAndRadius(0, new Vector3Int(2, 2, 3));
    [Property] protected BBoxInt SafeBounds { get; set; } = BBoxInt.FromMinsAndSize(0, new Vector3Int(1, 1, 2));
    protected static EntityType PlayerEntityType => SandcubeBaseMod.Instance!.Entities.Player;


    public virtual async Task<Entity?> SpawnPlayer(EntitySpawnConfig spawnConfig, CancellationToken cancellationToken)
    {
        var world = spawnConfig.World;
        if(world is not null)
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

            await PreloadChunks(world, PreloadRange, spawnPosition);
        }

        var player =  PlayerEntityType.CreateEntity(spawnConfig);

        foreach(var localPlayerInitializable in Scene.Components.GetAll<ILocalPlayerInitializable>(FindMode.EverythingInSelfAndDescendants))
            localPlayerInitializable.InitializeLocalPlayer(player);

        if(world is not null)
        {
            foreach(var worldInitializable in player.GameObject.Components.GetAll<IWorldInitializable>(FindMode.EverythingInSelfAndDescendants))
                worldInitializable.InitializeWorld(world);
        }
        return player;
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