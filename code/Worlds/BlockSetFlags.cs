using System;
namespace Sandcube.Worlds;

[Flags]
public enum BlockSetFlags : byte
{
    None = 0,
    UpdateModel = 1 << 0,
    AwaitModelUpdate = 1 << 1,
    UpdateNeigbours = 1 << 2,
    MarkDirty = 1 << 3,

    Default = UpdateModel | UpdateNeigbours | MarkDirty,
}
