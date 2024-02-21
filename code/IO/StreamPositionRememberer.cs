using System;
using System.IO;

namespace Sandcube.IO;

public class StreamPositionRememberer : IDisposable
{
    public readonly Stream Stream;
    public readonly long StartPosition;

    public StreamPositionRememberer(Stream stream, long startPosition)
    {
        Stream = stream;
        StartPosition = startPosition;
    }

    public StreamPositionRememberer(Stream stream) : this(stream, stream.Position)
    {
    }

    public static implicit operator StreamPositionRememberer(Stream stream) => new(stream);
    public static implicit operator StreamPositionRememberer(BinaryWriter writer) => new(writer.BaseStream);
    public static implicit operator StreamPositionRememberer(BinaryReader reader) => new(reader.BaseStream);

    public void Dispose() => Stream.Position = StartPosition;
}
