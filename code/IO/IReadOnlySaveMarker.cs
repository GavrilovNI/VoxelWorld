
using System;

namespace Sandcube.IO;

public interface IReadOnlySaveMarker
{
    bool IsSaved { get; }
    void AddSaveListener(Action action);
}
