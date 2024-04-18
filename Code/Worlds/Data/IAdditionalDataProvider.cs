

namespace VoxelWorld.Worlds.Data;

public interface IAdditionalDataProvider
{
    public T GetAdditionalData<T>(BlocksAdditionalDataType<T> dataType, in Vector3Int position) where T : notnull;
}
