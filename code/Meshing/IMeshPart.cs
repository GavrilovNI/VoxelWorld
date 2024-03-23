using Sandbox;
using System.Collections.Generic;

namespace VoxelWorld.Meshing;

public interface IMeshPart<V> where V : unmanaged, IVertex
{
    BBox Bounds { get; }
    bool IsEmpty { get; }

    void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 offset = default);

    void AddAsCollisionMesh(ModelBuilder builder, Vector3 offset = default);
    void AddAsCollisionHull(ModelBuilder builder, Vector3 center, Rotation rotation, Vector3 offset = default);

    (List<int> indices, List<V> vertices) ToRaw();
    List<V> CombineVertices();
    List<int> CombineIndices();
}
