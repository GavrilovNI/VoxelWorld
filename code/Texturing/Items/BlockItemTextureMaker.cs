using Sandbox;
using Sandcube.Blocks.States;
using System.Threading.Tasks;

namespace Sandcube.Texturing.Items;

public class BlockItemTextureMaker
{
    protected BlockPhotoMaker BlockPhotoMaker { get; }

    public BlockItemTextureMaker(BlockPhotoMaker blockPhotoMaker)
    {
        BlockPhotoMaker = blockPhotoMaker;
    }

    public virtual Task<bool> TryMakePhoto(BlockState blockState, Texture texture)
    {
        SetupPhotoMaker(blockState);
        return BlockPhotoMaker.TryMakePhoto(blockState, texture);
    }

    protected virtual void SetupPhotoMaker(BlockState blockState)
    {
        BlockPhotoMaker.ResetTransforms();
        BlockPhotoMaker.Camera.Orthographic = true;
        BlockPhotoMaker.Camera.OrthographicHeight = 70;
        BlockPhotoMaker.Camera.Transform.World = new(Vector3.One * 100, new Angles(35.4f, -135f, 0f).ToRotation());
        BlockPhotoMaker.Sun.Transform.World = new(Vector3.Zero, new Angles(75, -90f, 0f).ToRotation());
    }
}
