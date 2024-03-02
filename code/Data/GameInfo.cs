using Sandcube.IO;
using System.IO;

namespace Sandcube.Data;

public readonly record struct GameInfo : IBinaryWritable, IBinaryStaticReadable<GameInfo>
{
    public required string Name { get; init; }
    public required int Seed { get; init; }



    public static GameInfo Read(BinaryReader reader)
    {
        var name = reader.ReadString();
        var seed = reader.ReadInt32();

        return new GameInfo()
        {
            Name = name,
            Seed = seed,
        };
    }

    public readonly void Write(BinaryWriter writer)
    {
        writer.Write(Name);
        writer.Write(Seed);
    }
}
