

namespace Sandcube.IO;

public class SaveMarker : IReadOnlySaveMarker
{
    public static readonly SaveMarker Saved = new(true);
    public static readonly IReadOnlySaveMarker NotSaved = new SaveMarker(false);

    public static SaveMarker NewNotSaved => new(false);

    public bool IsSaved { get; private set; }

    private SaveMarker(bool isSaved)
    {
        IsSaved = isSaved;
    }

    public bool MarkSaved() => IsSaved = true;

    public static implicit operator SaveMarker(bool isSaved) => isSaved ? Saved : NewNotSaved;
    public static implicit operator bool(SaveMarker marker) => marker.IsSaved;
}
