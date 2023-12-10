

namespace Sandcube.Worlds.Generation.Meshes;

public interface IMeshPart<V> where V : unmanaged, IVertex
{
    void AddToBuilder(UnlimitedMesh<V>.Builder builder, Vector3 position);
}
