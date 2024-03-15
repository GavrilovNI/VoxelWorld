using Sandbox;

namespace VoxelWorld.IO;

public static class BaseFileSystemExtensions
{
    public static BaseFileSystem CreateDirectoryAndSubSystem(this BaseFileSystem fileSystem, string path)
    {
        fileSystem.CreateDirectory(path);
        return fileSystem.CreateSubSystem(path);
    }

    public static string GetPathFromData(this BaseFileSystem fileSystem, string path) =>
        fileSystem.GetFullPath(path).Substring(FileSystem.Data.GetFullPath("/").Length);
}
