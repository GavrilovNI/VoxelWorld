using System;
namespace Sandcube.Worlds;

[Flags]
public enum BlockSetFlags : byte
{
    None = 0,
    UpdateModel = 1 << 0,
    AwaitModelUpdate = 1 << 1,
    UpdateNeigbours = 1 << 2,


    Default = UpdateModel | UpdateNeigbours,
}
