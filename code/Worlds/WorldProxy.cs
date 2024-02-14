using Sandbox;
using Sandcube.Blocks.Entities;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using System.Threading.Tasks;

namespace Sandcube.Worlds;

public class WorldProxy : Component, IWorldAccessor, IBlockEntityProvider
{
    [Property] public bool DoNotEnableIfWorldIsNotValid { get; set; } = false;
    [Property] public World World { get; set; } = null!;

    protected override void OnEnabled()
    {
        if(DoNotEnableIfWorldIsNotValid && World.IsValid() == false)
            Enabled = false;
    }

    public Vector3 GetBlockGlobalPosition(Vector3Int blockPosition) => World.GetBlockGlobalPosition(blockPosition);
    public Vector3Int GetBlockPosition(Vector3 position) => World.GetBlockPosition(position);
    public Vector3Int GetBlockPosition(Vector3 position, Vector3 hitNormal) => World.GetBlockPosition(position, hitNormal);
    public Vector3Int GetBlockPositionInChunk(Vector3Int blockPosition) => World.GetBlockPositionInChunk(blockPosition);
    public BlockState GetBlockState(Vector3Int blockPosition) => World.GetBlockState(blockPosition);
    public BlockEntity? GetBlockEntity(Vector3Int blockPosition) => World.GetBlockEntity(blockPosition);
    public Vector3Int GetBlockWorldPosition(Vector3Int chunkPosition, Vector3Int blockLocalPosition) => GetBlockWorldPosition(chunkPosition, blockLocalPosition);
    public bool HasChunk(Vector3Int chunkPosition) => World.HasChunk(chunkPosition);
    public Chunk? GetChunk(Vector3Int chunkPosition) => World.GetChunk(chunkPosition);
    public Vector3Int GetChunkPosition(Vector3Int blockPosition) => World.GetChunkPosition(blockPosition);
    public Task<BlockStateChangingResult> SetBlockState(Vector3Int blockPosition, BlockState blockState, BlockSetFlags flags) =>
        World.SetBlockState(blockPosition, blockState, flags);
}
