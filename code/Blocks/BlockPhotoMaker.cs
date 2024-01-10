using Sandbox;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Worlds;
using System;

namespace Sandcube.Blocks;

public class BlockPhotoMaker : Component
{
    [Property] public CameraComponent Camera { get; set; } = null!;
    [Property] public World World { get; set; } = null!;
    [Property] public DirectionalLight Sun { get; set; } = null!;

    public bool TryRenderToTexture(Texture texture)
    {
        //return Graphics.RenderToTexture(/* get scene cam */, texture);
        return false; // TODO
    }

    public bool TryMakePhoto(BlockState blockState, Texture output)
    {
        World.Clear();
        World.SetBlockState(Vector3Int.Zero, blockState);
        bool made = TryRenderToTexture(output);
        World.Clear();
        return made;
    }
}
