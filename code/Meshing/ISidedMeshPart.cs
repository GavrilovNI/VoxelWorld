using Sandbox;
using Sandcube.Mth.Enums;
using System.Collections.Generic;

namespace Sandcube.Meshing;

public interface ISidedMeshPart<V> : IMeshPart<V> where V : unmanaged, IVertex
{
    void AddToBuilder(UnlimitedMesh<V>.Builder builder, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default);
    void IMeshPart<V>.AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 offset) => AddToBuilder(builder, Direction.AllSet, offset);

    void AddAsCollisionMesh(ModelBuilder builder, IReadOnlySet<Direction> sidesToAdd, Vector3 offset = default);
    void IMeshPart<V>.AddAsCollisionMesh(ModelBuilder builder, Vector3 offset) => AddAsCollisionMesh(builder, Direction.AllSet, offset);
}
