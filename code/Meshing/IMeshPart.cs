using Sandbox;

namespace Sandcube.Meshing;

public interface IMeshPart<V> where V : unmanaged, IVertex
{
    BBox Bounds { get; }

    void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 position);

    void AddAsCollisionMesh(ModelBuilder builder, Vector3 offset = default);
}
