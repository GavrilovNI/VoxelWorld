using Sandcube.Mth;
using System.Collections.Generic;

namespace Sandcube.Data;

public class WorldData
{
    public Dictionary<Vector3Int, BlocksData> Chunks = new();
    public Dictionary<Vector3Int, EntitiesData> Entities = new();
    public Dictionary<ulong, PlayerData> Players = new();
}
