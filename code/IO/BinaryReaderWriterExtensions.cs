using Sandbox;
using System.Collections.Generic;
using System.IO;

namespace Sandcube.IO;
public static class BinaryReaderWriterExtensions
{
    public static void ReadTo<T>(this BinaryReader reader, ref T value) where T : IBinaryReadable
    {
        value.Read(reader);
    }

    // TODO: uncomment when T.Read(reader) get whitelisted
    /*public static T Read<T>(this BinaryReader reader) where T : IBinaryStaticReadable<T>
    {
        return T.Read(reader);
    }*/

    public static void Write<T>(this BinaryWriter writer, T value) where T : IBinaryWritable
    {
        value.Write(writer);
    }

    public static Transform ReadTransform(this BinaryReader reader)
    {
        Vector3 position = reader.ReadVector3();
        Rotation rotation = reader.ReadRotation();
        Vector3 scale = reader.ReadVector3();
        return new(position, rotation, scale);
    }

    public static void Write(this BinaryWriter writer, in Transform value)
    {
        writer.Write(value.Position);
        writer.Write(value.Rotation);
        writer.Write(value.Scale);
    }


    public static Rotation ReadRotation(this BinaryReader reader)
    {
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        float w = reader.ReadSingle();
        return new(x, y, z, w);
    }

    public static void Write(this BinaryWriter writer, in Rotation value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
        writer.Write(value.w);
    }


    public static Vector3 ReadVector3(this BinaryReader reader)
    {
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        return new(x, y, z);
    }

    public static void Write(this BinaryWriter writer, in Vector3 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }


    public static Vector2 ReadVector2(this BinaryReader reader)
    {
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        return new(x, y);
    }

    public static void Write(this BinaryWriter writer, in Vector2 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
    }

    public static Rect ReadRect(this BinaryReader reader)
    {
        float left = reader.ReadSingle();
        float top = reader.ReadSingle();
        float right = reader.ReadSingle();
        float bottom = reader.ReadSingle();
        return new Rect(left, top, right - left, bottom - top);
    }

    public static void Write(this BinaryWriter writer, in Rect value)
    {
        writer.Write(value.Left);
        writer.Write(value.Top);
        writer.Write(value.Right);
        writer.Write(value.Bottom);
    }
}
