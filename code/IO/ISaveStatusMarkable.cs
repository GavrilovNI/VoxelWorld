

namespace Sandcube.IO;

public interface ISaveStatusMarkable
{
    bool IsSaved { get; }
    void MarkSaved(IReadOnlySaveMarker saveMarker);
}
