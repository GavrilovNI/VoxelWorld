using Sandbox;
using System.Linq;
using VoxelWorld.Entities;
using VoxelWorld.SandcubeExtensions;

namespace VoxelWorld.Players;

public class LocalPlayerCameraDuplicate : Component, ILocalPlayerInitializable
{
    [Property, RequireComponent] protected CameraComponent Camera { get; set; } = null!;
    [Property] protected CameraComponent? PlayerCamera { get; set; }

    public void InitializeLocalPlayer(Player player)
    {
        PlayerCamera = player.Camera;
        PlayerCamera.CopyPropertiesTo(Camera);
    }

    protected override void OnAwake() => PlayerCamera?.CopyPropertiesTo(Camera);

    protected override void OnUpdate()
    {
        if(PlayerCamera is not null)
            Transform.World = PlayerCamera.Transform.World;
        Camera.Enabled = PlayerCamera is not null && !PlayerCamera.Active;
    }
}
