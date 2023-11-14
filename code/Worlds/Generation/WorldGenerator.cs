﻿using Sandcube.Mth;
using Sandcube.Worlds.Blocks;

namespace Sandcube.Worlds.Generation;

public class WorldGenerator : BaseComponent
{
    public void GenerateChunk(Chunk chunk)
    {
        chunk.Clear();
        Vector3Int blockOffset = chunk.BlockOffset;

        for(int x = 0; x < chunk.Size.x; ++x)
        {
            for(int y = 0; y < chunk.Size.y; ++y)
            {
                for(int z = 0; z < chunk.Size.z; ++z)
                {
                    Vector3Int localBlockPosition = new(x, y, z);
                    Vector3Int globalBlockPosition = localBlockPosition + blockOffset;

                    if(globalBlockPosition.z > 10)
                        continue;

                    var block = SandcubeGame.Instance!.Blocks.Stone.DefaultBlockState;
                    chunk.SetBlockState(localBlockPosition, block);
                }
            }
        }
    }
}