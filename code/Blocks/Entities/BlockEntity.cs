using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Worlds;

namespace Sandcube.Blocks.Entities;

public abstract class BlockEntity : IValid
{
    public Vector3Int Position { get; }
    public Vector3 GlobalPosition => World.GetBlockGlobalPosition(Position);
    public IWorldProvider World { get; }

    public BlockState BlockState => World.GetBlockState(Position);

    public bool IsValid { get; private set; }

    public BlockEntity(IWorldProvider world, Vector3Int position)
    {
        Position = position;
        World = world;
        IsValid = true;
    }

    public virtual void OnCreated()
    {
    }

    public void OnDestroyed()
    {
        IsValid = false;
        OnDestroyedInternal();
    }

    protected virtual void OnDestroyedInternal()
    {
    }
}
