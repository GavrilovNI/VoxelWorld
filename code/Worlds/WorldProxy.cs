using Sandbox;

namespace VoxelWorld.Worlds;

public sealed class WorldProxy : Component, IWorldProxy
{
    [Property] public bool DoNotEnableIfWorldIsNotValid { get; set; } = false;
    [Property] public World WorldComponent { get; set; } = null!;

    public IWorldAccessor World => WorldComponent;

    protected override void OnEnabled()
    {
        if(DoNotEnableIfWorldIsNotValid && WorldComponent.IsValid() == false)
            Enabled = false;
    }
}
