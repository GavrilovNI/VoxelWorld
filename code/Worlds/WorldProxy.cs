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

    public Vector3 GetBlockGlobalPosition(Vector3Int position) => World.GetBlockGlobalPosition(position);
    public Vector3Int GetBlockPosition(Vector3 position) => World.GetBlockPosition(position);
    public Vector3Int GetBlockPosition(Vector3 position, Vector3 hitNormal) => World.GetBlockPosition(position, hitNormal);
    public Vector3Int GetBlockPositionInChunk(Vector3Int blockPosition) => World.GetBlockPositionInChunk(blockPosition);
    public BlockState GetBlockState(Vector3Int position) => World.GetBlockState(position);
    public BlockEntity? GetBlockEntity(Vector3Int position) => World.GetBlockEntity(position);
    public Vector3Int GetBlockWorldPosition(Vector3Int chunkPosition, Vector3Int blockLocalPosition) => GetBlockWorldPosition(chunkPosition, blockLocalPosition);
    public Chunk? GetChunk(Vector3Int position) => World.GetChunk(position);
    public Vector3Int GetChunkPosition(Vector3Int blockPosition) => World.GetChunkPosition(blockPosition);
    public Task SetBlockState(Vector3Int position, BlockState blockState) => World.SetBlockState(position, blockState);
}
