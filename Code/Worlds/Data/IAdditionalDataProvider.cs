using VoxelWorld.Mth;

namespace VoxelWorld.Worlds.Data;

public interface IAdditionalDataProvider
{
    public T GetAdditionalData<T>(BlocksAdditionalDataType<T> dataType, in Vector3IntB position) where T : notnull;
}
