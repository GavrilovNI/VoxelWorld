using Sandbox;
using System.Linq;

namespace VoxelWorld.SandcubeExtensions;

public static class ComponentExtensions
{
    public static void CopyPropertiesTo(this Component source, Component destination)
    {
        var sourceCameraType = TypeLibrary.GetType(source.GetType());
        var destinationCameraType = TypeLibrary.GetType(destination.GetType());

        var sourceCameraProperties = sourceCameraType.Properties.Where(p => p.Attributes.Any(a => a is PropertyAttribute));
        var destinationCameraProperties = destinationCameraType.Properties.Where(p => p.Attributes.Any(a => a is PropertyAttribute));

        var propertiesToCopy = sourceCameraProperties.IntersectBy(destinationCameraProperties, p => p);

        foreach(var property in propertiesToCopy)
            property.SetValue(destination, property.GetValue(source));
    }
}
