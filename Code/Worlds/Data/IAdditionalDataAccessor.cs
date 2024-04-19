using System.Threading.Tasks;
using VoxelWorld.Mth;

namespace VoxelWorld.Worlds.Data;

public interface IAdditionalDataAccessor : IAdditionalDataProvider
{
    public Task SetAdditionalData<T>(BlocksAdditionalDataType<T> dataType, Vector3IntB position, T value) where T : notnull;
    public Task ResetAdditionalData(BlocksAdditionalDataType dataType, Vector3IntB position);
}
