using Sandcube.IO;
using System.IO;

namespace Sandcube.Data;

public readonly record struct GameInfo : IBinaryWritable, IBinaryStaticReadable<GameInfo>
{
    public required string Name { get; init; }



    public static GameInfo Read(BinaryReader reader)
    {
        var name = reader.ReadString();

        return new GameInfo()
        {
            Name = name,
        };
    }

    public readonly void Write(BinaryWriter writer)
    {
        writer.Write(Name);
    }
}
