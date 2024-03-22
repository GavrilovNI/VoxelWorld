using Sandbox;
using VoxelWorld.Blocks.States;
using VoxelWorld.Mth;
using VoxelWorld.Worlds;
using System.Threading.Tasks;

namespace VoxelWorld.Texturing;

public class BlockPhotoMaker : Component
{
    [Property] public CameraComponent Camera { get; set; } = null!;
    [Property] public World World { get; set; } = null!;
    [Property] public DirectionalLight Sun { get; set; } = null!;

    public bool TryRenderToTexture(Texture texture) => Camera.RenderToTexture(texture);

    public async Task<bool> TryMakePhoto(BlockState blockState, Texture texture)
    {
        World.Clear();
        await World.SetBlockState(Vector3IntB.Zero, blockState, BlockSetFlags.Default | BlockSetFlags.AwaitModelUpdate);
        bool made = TryRenderToTexture(texture);
        World.Clear();
        return made;
    }
}
