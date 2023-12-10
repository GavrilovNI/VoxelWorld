using Sandcube.Mth.Enums;
using System.Collections.Generic;

namespace Sandcube.Worlds.Generation.Meshes;

public interface ISidedMeshPart<V> : IMeshPart<V> where V : unmanaged, IVertex
{
    void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 position, IReadOnlySet<Direction> visibleFaces);
    void IMeshPart<V>.AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 position) => AddToBuilder(builder, position, Direction.AllSet);
}
