using System.IO;

namespace VoxelWorld.IO;

public static class IOUtils
{
    public static string RemoveInvalidCharacters(string fileOrDirectoryName) =>
        string.Concat(fileOrDirectoryName.Split(Path.GetInvalidFileNameChars()));
}
