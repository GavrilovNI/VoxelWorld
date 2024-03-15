using System;
using System.Collections.Generic;

namespace VoxelWorld.IO;

public class SaveMarker : IReadOnlySaveMarker
{
    public static readonly SaveMarker Saved = new(true);
    public static readonly IReadOnlySaveMarker NotSaved = new SaveMarker(false);

    public static SaveMarker NewNotSaved => new(false);

    public bool IsSaved { get; private set; }
    private readonly List<Action> _saveListeners = new();

    private SaveMarker(bool isSaved)
    {
        IsSaved = isSaved;
    }

    public void AddSaveListener(Action action)
    {
        if(IsSaved)
        {
            action();
            return;
        }

        _saveListeners.Add(action);
    }

    public bool MarkSaved()
    {
        if(IsSaved)
            return false;

        IsSaved = true;
        foreach(var saveListener in _saveListeners)
            saveListener();
        _saveListeners.Clear();
        return true;
    }

    public static implicit operator SaveMarker(bool isSaved) => isSaved ? Saved : NewNotSaved;
    public static implicit operator bool(SaveMarker marker) => marker.IsSaved;
}
