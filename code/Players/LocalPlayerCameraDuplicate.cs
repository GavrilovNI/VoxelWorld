using Sandbox;
using VoxelWorld.Entities;
using VoxelWorld.SandcubeExtensions;

namespace VoxelWorld.Players;

public class LocalPlayerCameraDuplicate : Component, ILocalPlayerInitializable
{
    [Property, RequireComponent] protected CameraComponent Camera { get; set; } = null!;
    [Property] protected CameraComponent? PlayerCamera { get; set; }
    [Property] protected bool DisableCameraObject { get; set; } = false;

    public void InitializeLocalPlayer(Player player)
    {
        PlayerCamera = player.Camera;
        PlayerCamera.CopyPropertiesTo(Camera);
    }

    protected override void OnAwake() => PlayerCamera?.CopyPropertiesTo(Camera);

    protected override void OnUpdate()
    {
        if(PlayerCamera.IsValid())
            Transform.World = PlayerCamera.Transform.World;

        var shouldBeEnabled = !PlayerCamera.IsValid() || !PlayerCamera.Active;
        Camera.Enabled = shouldBeEnabled;
        bool shouldObjectBeEnabled = shouldBeEnabled || !DisableCameraObject;
        Camera.GameObject.Enabled = shouldObjectBeEnabled;
    }
}
