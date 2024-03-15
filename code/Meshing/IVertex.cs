using Sandbox;

namespace VoxelWorld.Meshing;

public interface IVertex
{
    VertexAttribute[] Layout { get; } // TODO: make static when be static methods get whitelisted

    Vector3 GetPosition();
    void SetPosition(Vector3 position);
}
