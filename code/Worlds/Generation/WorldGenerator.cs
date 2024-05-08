using Sandbox;
using VoxelWorld.Blocks.States;
using VoxelWorld.Mods.Base;
using VoxelWorld.Mth;
using System.Collections.Generic;

namespace VoxelWorld.Worlds.Generation;

public class WorldGenerator : Component, IWorldInitializationListener
{
    [Property] public PerlinNoise.Config2D HeightNoiseSettings { get; set; }
    [Property] public PerlinNoise.Config3D DensityNoiseSettings { get; set; }
    [Property] public int MinHeight { get; set; }
    [Property] public int MaxHeight { get; set; }
    [Property] public Curve HeightSurfaceCurve { get; set; }
    [Property] public Curve AdditiveDensityFromHeightCurve { get; set; }

    public void SetSeed(int seed)
    {
        HeightNoiseSettings = HeightNoiseSettings with { Seed = seed };
        DensityNoiseSettings = DensityNoiseSettings with { Seed = seed };
    }

    public void OnWorldInitialized(World world) => SetSeed(world.Options.Seed);

    protected bool ShouldPlaceBlock(in Vector3IntB position, float surfaceHeight)
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

    public Dictionary<Vector3IntB, BlockState> Generate(Vector3IntB position, Vector3IntB size)
    {
        Dictionary<Vector3IntB, BlockState> result = new();

        var blocks = BaseMod.Instance!.Blocks;

        for(int x = 0; x < size.x; ++x)
        {
            for(int y = 0; y < size.y; ++y)
            {
                Vector2IntB globalBlockPositionXY = new(position.x + x, position.y + y);
                var heightNoise = (PerlinNoise.Get(new Vector2(globalBlockPositionXY), HeightNoiseSettings) + 1f) / 2f;
                heightNoise = HeightSurfaceCurve.Evaluate(heightNoise);
                var surfaceHeight = heightNoise * (MaxHeight - MinHeight) + MinHeight;

                for(int z = size.z - 1; z >= 0; --z)
                {
                    var shouldPlaceBlock = ShouldPlaceBlockWithZOffset(0);

                    BlockState blockState;
                    if(shouldPlaceBlock)
                    {
                        if(!ShouldPlaceBlockWithZOffset(1))
                            blockState = blocks.Grass.DefaultBlockState;
                        else if(!ShouldPlaceBlockWithZOffset(2) ||
                            !ShouldPlaceBlockWithZOffset(3) ||
                            !ShouldPlaceBlockWithZOffset(4))
                            blockState = blocks.Dirt.DefaultBlockState;
                        else
                            blockState = blocks.Stone.DefaultBlockState;
                    }
                    else
                    {
                        blockState = blocks.Air.DefaultBlockState;
                    }

                    result[new(x, y, z)] = blockState;
                
                    bool ShouldPlaceBlockWithZOffset(int heightOffset)
                    {
                        if(heightOffset <= 0 || z + heightOffset >= size.z)
                        {
                            Vector3IntB globalBlockPosition = new(globalBlockPositionXY, position.z + z + heightOffset);
                            return ShouldPlaceBlock(globalBlockPosition, surfaceHeight);
                        }
                        else
                        {
                            return result[new(x, y, z + heightOffset)] != blocks.Air.DefaultBlockState;
                        }
                    }
                }
            }
        }  

        return result;
    }
}
