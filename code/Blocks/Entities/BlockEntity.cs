using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Worlds;

namespace Sandcube.Blocks.Entities;

public abstract class BlockEntity
{
    public Vector3Int Position { get; set; }
    public IWorldProvider World { get; set; }

    public BlockState BlockState => World.GetBlockState(Position);


    public BlockEntity(Vector3Int position, IWorldProvider world)
    {
        Position = position;
        World = world;
    }

    public virtual void OnCreated()
    {

    }

    public virtual void OnDestroyed()
    {

    }
}
