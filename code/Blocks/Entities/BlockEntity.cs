using Sandbox;
using Sandbox.ModelEditor.Nodes;
using Sandcube.Blocks.States;
using Sandcube.IO;
using Sandcube.Mth;
using Sandcube.Worlds;
using System.IO;

namespace Sandcube.Blocks.Entities;

public abstract class BlockEntity : IValid, ISaveStatusMarkable, IBinaryWritable
{
    public Vector3Int Position { get; }
    public Vector3 GlobalPosition => World.GetBlockGlobalPosition(Position);
    public IWorldAccessor World { get; }

    public BlockState BlockState => World.GetBlockState(Position);

    public bool IsValid { get; private set; }

    private IReadOnlySaveMarker _saveMarker = SaveMarker.Saved;
    public bool IsSaved
    {
        get
        {
            if(!_saveMarker.IsSaved)
                return false;

            if(!IsSavedInternal)
                _saveMarker = SaveMarker.NotSaved;

            return _saveMarker.IsSaved;
        }
    }
    protected virtual bool IsSavedInternal => true;


    public BlockEntity(IWorldAccessor world, Vector3Int position)
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

    public void MarkSaved(IReadOnlySaveMarker saveMarker)
    {
        if(IsSaved)
            return;

        _saveMarker = saveMarker;
        MarkSavedInternal(saveMarker);
    }
    protected virtual void MarkSavedInternal(IReadOnlySaveMarker saveMarker) { }

    public void Load(BinaryReader reader)
    {
        Read(reader);
        MarkSaved(SaveMarker.Saved);
    }

    public void Save(BinaryWriter writer, IReadOnlySaveMarker saveMarker)
    {
        Write(writer);
        MarkSaved(saveMarker);
    }

    public void Write(BinaryWriter writer)
    {
        WriteAdditional(writer);
    }

    protected void Read(BinaryReader reader)
    {
        ReadAdditional(reader);
    }
}
