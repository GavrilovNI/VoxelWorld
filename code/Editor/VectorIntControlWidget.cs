using Editor;
using Sandbox;
using Sandcube.Mth;

namespace Sandcube.Editor;

[CustomEditor(typeof(Vector3Int))]
[CustomEditor(typeof(Vector2Int))]
public class VectorIntControlWidget : VectorControlWidget
{
    public VectorIntControlWidget(SerializedProperty property) : base(property)
    {

    }
}
