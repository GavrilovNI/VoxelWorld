using Sandcube.Mth;

namespace Sandcube.Editor;

[CustomEditor(typeof(Vector3Int))]
public class Vector3IntControlWidget : VectorControlWidget
{
    public Vector3IntControlWidget(SerializedProperty property) : base(property)
    {

    }
}
