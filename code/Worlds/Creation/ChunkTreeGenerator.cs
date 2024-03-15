using Sandbox;
using Sandcube.Blocks;
using Sandcube.Blocks.States;
using Sandcube.Mods.Base;
using Sandcube.Mth;
using Sandcube.Mth.Enums;
using Sandcube.Worlds.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Creation;

public class ChunkTreeGenerator : ChunkCreationStage
{
    const int TrunkHeight = 4;

    [Property] protected World World { get; set; } = null!;
    [Property] protected WorldGenerator? Generator { get; set; }

    public override async Task<bool> TryProcess(Chunk chunk, CancellationToken cancellationToken)
    {
        int minHeight = Generator?.MinHeight ?? World.Limits.Mins.z;
        int maxHeight = Generator?.MaxHeight ?? World.Limits.Maxs.z;

        int chunkGlobalMinZ = World.GetBlockWorldPosition(chunk.Position, Vector3Int.Zero).z;
        int chunkGlobalMaxZ = World.GetBlockWorldPosition(chunk.Position, new Vector3Int(0, 0, chunk.Size.z - 1)).z;

        if(minHeight > chunkGlobalMaxZ || chunkGlobalMinZ > maxHeight)
            return true;

        int minZ = Math.Max(minHeight, chunkGlobalMinZ);
        int maxZ = Math.Min(maxHeight, chunkGlobalMaxZ);

        int localMinZ = World.GetBlockPositionInChunk(new Vector3Int(0, 0, minZ)).z;
        int localMaxZ = World.GetBlockPositionInChunk(new Vector3Int(0, 0, maxZ)).z;

        await TryPlaceTreeAtXY(chunk, 0, localMinZ, localMaxZ);

        return true;
    }

    protected virtual async Task<bool> TryPlaceTreeAtXY(Chunk chunk, Vector2Int localPositionXY, int zMin, int zMax)
    {
        for(int z = zMax; z >= zMin; --z)
        {
            var localPosition = new Vector3Int(localPositionXY, z);
            if(await TryPlaceTree(chunk, localPosition))
                return true;
        }
        return false;
    }

    protected virtual async Task<bool> TryPlaceTree(Chunk chunk, Vector3Int localPosition)
    {
        bool canPlace = await CanPlaceTreeAt(chunk, localPosition);
        if(canPlace)
            await PlaceTree(chunk, localPosition);
        return canPlace;
    }

    protected virtual async Task<bool> CanPlaceTreeAt(Chunk chunk, Vector3Int localPosition)
    {
        var blocks = SandcubeBaseMod.Instance!.Blocks;

        var groundBlockState = await GetBlockState(chunk, localPosition + Vector3Int.Down);
        bool isValidGround = groundBlockState.Block == blocks.Grass || groundBlockState.Block == blocks.Dirt;
        if(!isValidGround)
            return false;

        var log = blocks.WoodLog.DefaultBlockState.With(PillarBlock.AxisProperty, Axis.Z);
        List<Task<bool>> trunkTestTasks = new();
        for(int i = 0; i < TrunkHeight; ++i)
            trunkTestTasks.Add(CanReplaceBlock(chunk, localPosition + new Vector3Int(0, 0, i), log));

        var results = await Task.WhenAll(trunkTestTasks);
        return results.All(v => v);
    }

    protected virtual async Task PlaceTree(Chunk chunk, Vector3Int localPosition)
    {
        var blocks = SandcubeBaseMod.Instance!.Blocks;
        var log = blocks.WoodLog.DefaultBlockState.With(PillarBlock.AxisProperty, Axis.Z);
        var leaves = blocks.TreeLeaves.DefaultBlockState;

        const int leavesHalfWidth = 1;
        const int leavesMinHeigth = 2;

        List<Task> tasks = new();

        for(int i = 0; i < TrunkHeight; ++i)
            tasks.Add(SetBlockState(chunk, localPosition + new Vector3Int(0, 0, i), log));

        for(int x = -leavesHalfWidth; x <= 1; ++x)
        {
            for(int y = -leavesHalfWidth; y <= leavesHalfWidth; ++y)
            {
                for(int z = leavesMinHeigth; z < TrunkHeight + leavesHalfWidth; ++z)
                {
                    if(x == 0 && y == 0 && z < TrunkHeight)
                        continue;

                    tasks.Add(TryReplaceBlock(chunk, localPosition + new Vector3Int(x, y, z), leaves));
                }
            }
        }

        await Task.WhenAll(tasks);
    }



    protected virtual async Task<BlockStateChangingResult> SetBlockState(Chunk chunk, Vector3Int relativePosition, BlockState blockState, BlockSetFlags flags = BlockSetFlags.None)
    {
        if(relativePosition.IsAnyAxis((a, v) => v < 0 || v >= chunk.Size.GetAxis(a)))
        {
            var worldPosition = World.GetBlockWorldPosition(chunk.Position, relativePosition);
            return await World.SetBlockState(worldPosition, blockState, flags);
        }
        return await chunk.SetBlockState(relativePosition, blockState, flags);
    }

    protected virtual async Task<bool> CanReplaceBlock(Chunk chunk, Vector3Int relativePosition, BlockState blockState)
    {
        var currentBlockState = await GetBlockState(chunk, relativePosition);
        return currentBlockState.Block.CanBeReplaced(currentBlockState, blockState);
    }

    protected virtual async Task<BlockStateChangingResult> TryReplaceBlock(Chunk chunk, Vector3Int relativePosition, BlockState blockState, BlockSetFlags flags = BlockSetFlags.None)
    {
        if(await CanReplaceBlock(chunk, relativePosition, blockState))
            return await SetBlockState(chunk, relativePosition, blockState, flags);
        return BlockStateChangingResult.NotChanged;
    }

    protected virtual async Task<BlockState> GetBlockState(Chunk chunk, Vector3Int relativePosition)
    {
        if(relativePosition.IsAnyAxis((a, v) => v < 0 || v >= chunk.Size.GetAxis(a)))
        {
            var worldPosition = World.GetBlockWorldPosition(chunk.Position, relativePosition);
            var chunkPosition = World.GetChunkPosition(worldPosition);

            await World.CreateChunk(chunkPosition, ChunkCreationStatus.Preloading);
            return World.GetBlockState(worldPosition);
        }
        return chunk.GetBlockState(relativePosition);
    }
}
