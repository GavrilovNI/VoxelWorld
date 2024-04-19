using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VoxelWorld.IO.NamedBinaryTags;
using VoxelWorld.Mth;
using VoxelWorld.Registries;

namespace VoxelWorld.Worlds.Data;

public abstract class BlocksAdditionalDataType : IRegisterable
{
    public ModedId Id { get; }
    public Type Type { get; }
    public object DefaultValue { get; }


    protected internal BlocksAdditionalDataType(ModedId id, Type type, object defaultValue)
    {
        Id = id;
        Type = type;
        DefaultValue = defaultValue;
    }

    public virtual Task OnValueChanged(IWorldAccessor world, in Vector3IntB blockPosition, object newValue) => Task.CompletedTask;

    public abstract BinaryTag SaveAsObject(object value);
    public abstract object LoadAsObject(BinaryTag tag);

    public static BlocksAdditionalDataType Get(in ModedId dataId) =>
        GameController.Instance!.Registries.GetRegistry<BlocksAdditionalDataType>().Get(dataId);

    public static bool TryGet(in ModedId dataId, out BlocksAdditionalDataType dataType)
    {
        if(GameController.Instance!.Registries.TryGetRegistry<BlocksAdditionalDataType>(out var registry) && registry.TryGet(dataId, out dataType))
            return true;

        dataType = null!;
        return false;
    }

    public static void AssertRegestered(BlocksAdditionalDataType dataType)
    {
        if(!TryGet(dataType.Id, out var realDataType) || realDataType != dataType)
            throw new InvalidOperationException($"{dataType} is not registered");
    }

    public static void AssertValueType(BlocksAdditionalDataType dataType, Type type)
    {
        if(dataType.Type != type)
            throw new InvalidOperationException($"type {type} is not valid {dataType}'s value's type");
    }

    public static void AssertValueType(BlocksAdditionalDataType dataType, object value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        var type = value.GetType();
        if(dataType.Type != type)
            throw new ArgumentException($"{value}'s type {type} is not valid {dataType}'s value's type", paramName);
    }
}

public abstract class BlocksAdditionalDataType<T> : BlocksAdditionalDataType where T : notnull
{
    public new T DefaultValue => (T)base.DefaultValue;

    public BlocksAdditionalDataType(ModedId id, T defaultValue) : base(id, typeof(T), defaultValue)
    {
    }

    [Obsolete("Try using OnValueChanged<T>")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    public sealed override Task OnValueChanged(IWorldAccessor world, in Vector3IntB blockPosition, object newValue) =>
        OnValueChanged(world, blockPosition, (T)newValue);
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    public virtual Task OnValueChanged(IWorldAccessor world, in Vector3IntB blockPosition, in T newValue) => Task.CompletedTask;

    public sealed override BinaryTag SaveAsObject(object value) => Save((T)value);
    public override object LoadAsObject(BinaryTag tag) => Load(tag);

    public abstract BinaryTag Save(T value);
    public abstract T Load(BinaryTag tag);
}
