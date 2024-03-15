
using System;

namespace VoxelWorld.IO;

public interface IReadOnlySaveMarker
{
    bool IsSaved { get; }
    void AddSaveListener(Action action);
}
