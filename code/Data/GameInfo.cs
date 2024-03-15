using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.IO.NamedBinaryTags.Collections;

namespace VoxelWorld.Data;

public readonly record struct GameInfo : INbtWritable, INbtStaticReadable<GameInfo>
{
    public required string Name { get; init; }
    public required int Seed { get; init; }


    public BinaryTag Write()
    {
        CompoundTag tag = new();
        tag.Set("name", Name);
        tag.Set("seed", Seed);
        return tag;
    }

    public static GameInfo Read(BinaryTag tag)
    {
        CompoundTag compoundTag = tag.To<CompoundTag>();

        return new()
        {
            Name = compoundTag.Get<string>("name", "game name was lost"),
            Seed = compoundTag.Get<int>("seed"),
        };
    }
}
