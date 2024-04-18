using System.Threading.Tasks;

namespace VoxelWorld.Worlds.Data;

public interface IAdditionalDataAccessor : IAdditionalDataProvider
{
    public Task SetAdditionalData<T>(BlocksAdditionalDataType<T> dataType, Vector3Int position, T value) where T : notnull;
    public Task ResetAdditionalData<T>(BlocksAdditionalDataType<T> dataType, Vector3Int position) where T : notnull;
}
