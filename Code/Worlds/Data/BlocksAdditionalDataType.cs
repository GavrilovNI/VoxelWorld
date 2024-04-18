using System;
using System.Diagnostics.CodeAnalysis;
using VoxelWorld.Mth;
using VoxelWorld.Registries;

namespace VoxelWorld.Worlds.Data;

public class BlocksAdditionalDataType : IRegisterable
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

    public virtual void OnValueChanged(IWorldAccessor world, in Vector3IntB blockPosition, object newValue) { }


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
}

public class BlocksAdditionalDataType<T> : BlocksAdditionalDataType where T : notnull
{
    public new T DefaultValue => (T)base.DefaultValue;

    public BlocksAdditionalDataType(ModedId id, T defaultValue) : base(id, typeof(T), defaultValue)
    {
    }

    [Obsolete("Try using OnValueChanged<T>")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    public sealed override void OnValueChanged(IWorldAccessor world, in Vector3IntB blockPosition, object newValue) =>
        OnValueChanged(world, blockPosition, (T)newValue);
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    public virtual void OnValueChanged(IWorldAccessor world, in Vector3IntB blockPosition, in T newValue) { }
}
