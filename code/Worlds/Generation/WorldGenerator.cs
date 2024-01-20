using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using System.Threading.Tasks;

namespace Sandcube.Worlds.Generation;

public class WorldGenerator : Component
{
    public BlockState Generate(Vector3Int position)
    {
        if(position.z > 10)
            return BlockState.Air;

        return SandcubeGame.Instance!.Blocks.Stone.DefaultBlockState;
    }

    public BlockState[,,] Generate(Vector3Int position, Vector3Int size)
    {
        BlockState[,,] result = new BlockState[size.x, size.y, size.z];

        for(int x = 0; x < size.x; ++x)
        {
            for(int y = 0; y < size.y; ++y)
            {
                for(int z = 0; z < size.z; ++z)
                {
                    Vector3Int globalBlockPosition = new Vector3Int(x, y, z) + position;
                    result[x, y, z] = Generate(globalBlockPosition);
                }
            }
        }

        return result;
    }
}
