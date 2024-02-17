using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mods;
using Sandcube.Mth;

namespace Sandcube.Worlds.Generation;

public class WorldGenerator : Component
{
    [Property] public PerlinNoise.Config2D HeightNoiseSettings { get; set; }
    [Property] public PerlinNoise.Config3D DensityNoiseSettings { get; set; }
    [Property] public int MinHeight { get; set; }
    [Property] public int MaxHeight { get; set; }
    [Property] public Curve HeightSurfaceCurve { get; set; }
    [Property] public Curve AdditiveDensityFromHeightCurve { get; set; }

    protected bool ShouldPlaceBlock(Vector3Int position, float surfaceHeight)
    {
        const int surfaceModifactionsHeight = 4;
        if(position.z < MinHeight - surfaceModifactionsHeight)
            return true;
        if(position.z > MaxHeight)
            return false;

        var density = PerlinNoise.Get(position, DensityNoiseSettings);
        var heightPercent = 1f * (position.z - MinHeight) / (MaxHeight - MinHeight);
        var additiveDensity = AdditiveDensityFromHeightCurve.Evaluate(heightPercent);
        density += additiveDensity;

        bool shouldPalceBlock = position.z < surfaceHeight && density > 0;
        return shouldPalceBlock;
    }

    public BlockState[,,] Generate(Vector3Int position, Vector3Int size)
    {
        BlockState[,,] result = new BlockState[size.x, size.y, size.z];

        var blocks = SandcubeBaseMod.Instance!.Blocks;

        for(int x = 0; x < size.x; ++x)
        {
            for(int y = 0; y < size.y; ++y)
            {
                Vector2Int globalBlockPositionXY = new(position.x + x, position.y + y);
                var heightNoise = (PerlinNoise.Get(new Vector2(globalBlockPositionXY), HeightNoiseSettings) + 1f) / 2f;
                heightNoise = HeightSurfaceCurve.Evaluate(heightNoise);
                var surfaceHeight = heightNoise * (MaxHeight - MinHeight) + MinHeight;

                for(int z = size.z - 1; z >= 0; --z)
                {
                    Vector3Int globalBlockPosition = new(globalBlockPositionXY, position.z + z);
                    var shouldPlaceBlock = ShouldPlaceBlock(globalBlockPosition, surfaceHeight);

                    BlockState blockState;
                    if(shouldPlaceBlock)
                    {
                        if(!ShouldPlaceBlockHigher(1))
                            blockState = blocks.Grass.DefaultBlockState;
                        else if(!ShouldPlaceBlockHigher(2) ||
                            !ShouldPlaceBlockHigher(3) ||
                            !ShouldPlaceBlockHigher(4))
                            blockState = blocks.Dirt.DefaultBlockState;
                        else
                            blockState = blocks.Stone.DefaultBlockState;
                    }
                    else
                    {
                        blockState = blocks.Air.DefaultBlockState;
                    }

                    result[x, y, z] = blockState;
                
                    bool ShouldPlaceBlockHigher(int heightOffset)
                    {
                        if(z + heightOffset >= size.z)
                            return ShouldPlaceBlock(globalBlockPosition + Vector3Int.Up * heightOffset, surfaceHeight);
                        else
                            return result[x, y, z + heightOffset] != blocks.Air.DefaultBlockState;
                    }
                }
            }
        }  

        return result;
    }
}
