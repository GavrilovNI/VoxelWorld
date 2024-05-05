using Editor;
using Sandbox;
using VoxelWorld.Mth;

namespace VoxelWorld.Editor;

[CustomEditor(typeof(Vector3IntB))]
[CustomEditor(typeof(Vector2IntB))]
[CustomEditor(typeof(Vector3Byte))]
public class VectorIntControlWidget : VectorControlWidget
{
    public VectorIntControlWidget(SerializedProperty property) : base(property)
    {

    }
}
