using Sandbox;
using Sandcube.Worlds;

namespace Sandcube.Entities;

public record struct EntitySpawnConfig
{
    public Transform Transform;
    public IWorldAccessor? World;
    public bool StartEnabled;
    public string? Name;

    public EntitySpawnConfig(Transform transform, IWorldAccessor? world = null, bool startEnabled = true, string? name = null)
    {
        Transform = transform;
        World = world;
        StartEnabled = startEnabled;
        Name = name;
    }

    public EntitySpawnConfig(IWorldAccessor? world = null, bool startEnabled = true, string? name = null)
    {
        Transform = Transform.Zero;
        World = world;
        StartEnabled = startEnabled;
        Name = name;
    }

    public readonly CloneConfig ToCloneConfig() => new(Transform, World?.GameObject, StartEnabled, Name);
}
