using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.IO;
using Sandcube.Mth;
using Sandcube.Worlds;
using System.IO;

namespace Sandcube.Blocks.Entities;

public abstract class BlockEntity : IValid, ISaveStatusMarkable
{
    public Vector3Int Position { get; }
    public Vector3 GlobalPosition => World.GetBlockGlobalPosition(Position);
    public IWorldProvider World { get; }

    public BlockState BlockState => World.GetBlockState(Position);

    public bool IsValid { get; private set; }

    public virtual bool IsSaved => true;


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

    protected virtual void WriteAdditional(BinaryWriter writer) { }
    protected virtual void ReadAdditional(BinaryReader reader) { }

    public virtual void MarkSaved() { }

    public void Load(BinaryReader reader)
    {
        ReadAdditional(reader);
        MarkSaved();
    }

    public void Save(BinaryWriter writer, bool keepDirty = false)
    {
        WriteAdditional(writer);
        if(!keepDirty)
            MarkSaved();
    }
}
