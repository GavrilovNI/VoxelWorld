using Sandbox;

namespace VoxelWorld.Meshing;

public interface IMeshPart<V> where V : unmanaged, IVertex
{
    BBox Bounds { get; }

    void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 offset = default);

    void AddAsCollisionMesh(ModelBuilder builder, Vector3 offset = default);
    void AddAsCollisionHull(ModelBuilder builder, Vector3 center, Rotation rotation, Vector3 offset = default);
}
