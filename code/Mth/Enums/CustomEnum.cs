using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Text.Json;
using Sandcube.IO;
using System.IO;

namespace Sandcube.Mth.Enums;


public abstract class CustomEnum : IBinaryWritable
{
    public int Ordinal { get; init; }
    public string Name { get; init; }

    [Obsolete("For serialization only", true)]
    public CustomEnum()
    {
        Ordinal = -1;
        Name = string.Empty;
    }

    protected CustomEnum(int ordinal, string name)
    {
        Ordinal = ordinal;
        Name = name;
    }

    // TODO: remove. its workaround of whitelist error when calling interface static member
    public static IEnumerable<CustomEnum> GetValues(Type t)
    {
        if(!t.IsAssignableTo(typeof(CustomEnum)))
            throw new ArgumentException($"{t} is not assignable to {nameof(CustomEnum)}");
        return TypeLibrary.GetType(t).Create<CustomEnum>().GetAll();
    }

    // TODO: remove. its workaround of whitelist error when calling interface static member
    public abstract IEnumerable<CustomEnum> GetAll();

    public void Write(BinaryWriter writer) => writer.Write(Ordinal);
    public static CustomEnum Read<T>(BinaryReader reader) => Read(reader, typeof(T));
    public static CustomEnum Read(BinaryReader reader, Type enumType)
    {
        var ordinal = reader.ReadInt32();
        return GetValues(enumType).ElementAt(ordinal);
    }
}

public abstract class CustomEnum<T> : CustomEnum, IBinaryWritable, IBinaryStaticReadable<T> where T : CustomEnum<T>, ICustomEnum<T>
{
    [Obsolete("For serialization only", true)]
    public CustomEnum()
    {

    }

    protected CustomEnum(int ordinal, string name) : base(ordinal, name)
    {
    }

    // TODO: remove. its workaround of whitelist error when calling interface static member
    public static IEnumerable<T> GetValues() => CustomEnum.GetValues(typeof(T)).Cast<T>();

    protected static bool TryParse(IReadOnlyList<T> all, string name, out T value)
    {
        foreach(var item in all)
        {
            if(item.Name.ToLower() == name.ToLower())
            {
                value = item;
                return true;
            }
        }

        value = all[0];
        return false;
    }

    public static explicit operator int(CustomEnum<T> enumValue) => enumValue.Ordinal;

    public bool Equals(T enumValue) => Ordinal == enumValue.Ordinal;
    public sealed override bool Equals(object? obj) => obj is T enumValue && Equals(enumValue);
    public sealed override int GetHashCode() => Ordinal;

    public override string ToString() => Name;

    public static T Read(BinaryReader reader) => (T)CustomEnum.Read<T>(reader);


    public class CustomEnumJsonConverter : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.String)
            {
                var name = reader.GetString();
                var result = GetValues().FirstOrDefault(v => v!.Name == name, null);
                if(result is not null)
                    return result;
            }

            Log.Warning($"Vector2IntFromJson - unable to read from {reader.TokenType}");
            return null!;
        }

        public override void Write(Utf8JsonWriter writer, T val, JsonSerializerOptions options)
        {
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new(2, 3);
            defaultInterpolatedStringHandler.AppendLiteral(val.Name);
            writer.WriteStringValue(defaultInterpolatedStringHandler.ToStringAndClear());
        }
    }
}
