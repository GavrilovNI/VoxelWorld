using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Worlds;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sandcube.Blocks;

public class BlockPhotoMaker : Component
{
    [Property] public CameraComponent Camera { get; set; } = null!;
    [Property] public World World { get; set; } = null!;
    [Property] public DirectionalLight Sun { get; set; } = null!;

    public bool TryRenderToTexture(Texture texture) => Camera.RenderToTexture(texture);

    public async Task<bool> TryMakePhoto(BlockState blockState, Texture texture)
    {
        World.Clear();
        await World.SetBlockState(Vector3Int.Zero, blockState);
        await Task.Frame();
        bool made = TryRenderToTexture(texture);
        World.Clear();
        return made;
    }
}
