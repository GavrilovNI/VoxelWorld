using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mods;
using Sandcube.Mth;

namespace Sandcube.Worlds.Generation;

public class WorldGenerator : Component
{
    [Property] public PerlinNoise.Config2D Config { get; set; }

    public BlockState Generate(Vector3Int position, float noiseValue)
    {
        if(position.z > noiseValue)
            return BlockState.Air;
        if(position.z > noiseValue - 1)
            return SandcubeBaseMod.Instance!.Blocks.Grass.DefaultBlockState;
        if(position.z > noiseValue - 4)
            return SandcubeBaseMod.Instance!.Blocks.Dirt.DefaultBlockState;

        return SandcubeBaseMod.Instance!.Blocks.Stone.DefaultBlockState;
    }

    public BlockState[,,] Generate(Vector3Int position, Vector3Int size)
    {
        BlockState[,,] result = new BlockState[size.x, size.y, size.z];

        for(int x = 0; x < size.x; ++x)
        {
            for(int y = 0; y < size.y; ++y)
            {
                Vector2Int globalBlockPositionXY = new Vector2Int(position.x + x, position.y + y);
                var noise = (PerlinNoise.Get(new Vector2(globalBlockPositionXY), Config) + 1) / 2 * 50;

                for(int z = 0; z < size.z; ++z)
                {
                    Vector3Int globalBlockPosition = new(globalBlockPositionXY, position.z + z);
                    result[x, y, z] = Generate(globalBlockPosition, noise);
                }
            }
        }  

        return result;
    }
}
