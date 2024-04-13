namespace VoxelWorld.Data;

public interface IOneOf
{
    object? Value { get; }
    int Index { get; }
}
