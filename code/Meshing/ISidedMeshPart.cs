using Sandbox;
using VoxelWorld.Mth.Enums;
using System.Collections.Generic;

namespace VoxelWorld.Meshing;

public interface ISidedMeshPart<V> : IMeshPart<V> where V : unmanaged, IVertex
{
    void AddSideToBuilder(UnlimitedMesh<V>.Builder builder, Direction sideToAdd, Vector3 offset = default);
    void AddNotSidedPartToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 offset = default);
    void AddToBuilder(UnlimitedMesh<V>.Builder builder, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default);
    void IMeshPart<V>.AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 offset) => AddToBuilder(builder, Direction.AllSet, offset);

    void AddAsCollisionMesh(ModelBuilder builder, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default);
    void IMeshPart<V>.AddAsCollisionMesh(ModelBuilder builder, Vector3 offset) => AddAsCollisionMesh(builder, Direction.AllSet, offset);

    void AddAsCollisionHull(ModelBuilder builder, Vector3 center, Rotation rotation, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default);
    void IMeshPart<V>.AddAsCollisionHull(ModelBuilder builder, Vector3 center, Rotation rotation, Vector3 offset) => AddAsCollisionHull(builder, center, rotation, Direction.AllSet, offset);
}
