﻿using Sandbox;
using Sandcube.Mth;

namespace Sandcube.Worlds.Generation;

public class WorldAutoLoader : BaseComponent
{
    [Property] public World? World { get; set; } = null;
    [Property] public int Distance { get; set; } = 2;

    public override void Update()
    {
        if(!SandcubeGame.Started || World is null)
            return;

        var centralChunkPositrion = World.GetChunkPosition(Transform.Position);

        for(int x = centralChunkPositrion.x - Distance; x <= centralChunkPositrion.x + Distance; ++x)
        {
            for(int y = centralChunkPositrion.y - Distance; y <= centralChunkPositrion.y + Distance; ++y)
            {
                for(int z = centralChunkPositrion.z - Distance; z <= centralChunkPositrion.z + Distance; ++z)
                {
                    var chunkPosition = new Vector3Int(x, y, z);
                    World.GetChunk(chunkPosition, true);
                }
            }
        }
    }


}