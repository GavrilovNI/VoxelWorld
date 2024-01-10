﻿using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;

namespace Sandcube.Worlds;

public class WorldProxy : Component, IWorldAccessor
{
    [Property] public bool DoNotEnableIfWorldIsNotValid { get; set; } = false;
    [Property] public World World { get; set; } = null!;

    protected override void OnEnabled()
    {
        if(DoNotEnableIfWorldIsNotValid && World.IsValid() == false)
            Enabled = false;
    }

    public Vector3 GetBlockGlobalPosition(Vector3Int position) => World.GetBlockGlobalPosition(position);
    public Vector3Int GetBlockPosition(Vector3 position) => World.GetBlockPosition(position);
    public Vector3Int GetBlockPosition(Vector3 position, Vector3 hitNormal) => World.GetBlockPosition(position, hitNormal);
    public Vector3Int GetBlockPositionInChunk(Vector3Int blockPosition) => World.GetBlockPositionInChunk(blockPosition);
    public BlockState GetBlockState(Vector3Int position) => World.GetBlockState(position);
    public Vector3Int GetBlockWorldPosition(Vector3Int chunkPosition, Vector3Int blockLocalPosition) => GetBlockWorldPosition(chunkPosition, blockLocalPosition);
    public Chunk? GetChunk(Vector3Int position, bool forceLoad = false) => World.GetChunk(position, forceLoad);
    public Vector3Int GetChunkPosition(Vector3Int blockPosition) => World.GetChunkPosition(blockPosition);
    public void SetBlockState(Vector3Int position, BlockState blockState) => World.SetBlockState(position, blockState);
}