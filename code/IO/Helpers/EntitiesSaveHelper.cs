using Sandcube.Data;
using Sandcube.Mth;
using System.IO;

namespace Sandcube.IO.Helpers;

public class EntitiesSaveHelper : RegionalChunkedSaveHelper<EntitiesData>
{
    public EntitiesSaveHelper(in Vector3Int regionSize) : base(regionSize)
    {
    }

    protected override EntitiesData ReadChunkData(BinaryReader reader)
    {
        var data = new EntitiesData();

        int entitiesCount = reader.ReadInt32();
        for(int i = 0; i < entitiesCount; ++i)
        {
            int blockEntityDataLength = reader.ReadInt32();
            if(blockEntityDataLength > 0)
            {
                var blockEntityData = reader.ReadBytes(blockEntityDataLength);
                data.Data.Add(blockEntityData);
            }
        }

        return data;
    }

    protected override void WriteChunkData(BinaryWriter writer, EntitiesData data)
    {
        writer.Write(data.Count);
        foreach(var entityData in data)
        {
            if(entityData.Length > 0)
            {
                writer.Write(entityData.Length);
                writer.Write(entityData);
            }
        }
    }
}
