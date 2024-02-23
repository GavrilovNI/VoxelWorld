using Sandbox;

namespace Sandcube.IO;

public static class BaseFileSystemExtensions
{
    public static BaseFileSystem CreateDirectoryAndSubSystem(this BaseFileSystem fileSystem, string path)
    {
        fileSystem.CreateDirectory(path);
        return fileSystem.CreateSubSystem(path);
    }
}
