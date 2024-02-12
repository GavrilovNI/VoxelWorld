using Sandbox;
using Sandcube.Blocks;
using Sandcube.Blocks.States;
using Sandcube.Mth;
using Sandcube.Worlds;
using System.Threading.Tasks;

namespace Sandcube.Texturing.Items;

public class TwoPartBlockItemTextureMaker : BlockItemTextureMaker
{
    public TwoPartBlockItemTextureMaker(BlockPhotoMaker blockPhotoMaker) : base(blockPhotoMaker)
    {
    }

    public override async Task<bool> TryMakePhoto(BlockState blockState, Texture texture)
    {
        if(blockState.Block is not TwoPartBlock block)
            return false;

        SetupPhotoMaker(blockState);
        var world = BlockPhotoMaker.World;
        world.Clear();

        var flags = BlockSetFlags.Default | BlockSetFlags.AwaitModelUpdate & ~BlockSetFlags.UpdateNeigbours;
        _ = await world.SetBlockState(Vector3Int.Zero, blockState, flags);

        var secondPartPosition = Vector3Int.Zero + block.GetDirectionToAnotherPart(blockState);
        var secondPartBlockState = blockState.Change(TwoPartBlock.PartTypeProperty, p => p.GetOpposite());
        _ = await world.SetBlockState(secondPartPosition, secondPartBlockState, flags);

        return BlockPhotoMaker.TryRenderToTexture(texture);
    }

    protected override void SetupPhotoMaker(BlockState blockState)
    {
        if(blockState.Block is not TwoPartBlock block)
            return;

        var direction = block.GetDirectionToAnotherPart(blockState);
        var secondPartPosition = Vector3Int.Zero + direction;
        var firstPartGlobalPosition = BlockPhotoMaker.World.GetBlockGlobalPosition(Vector3Int.Zero);
        var secondPartGlobalPosition = BlockPhotoMaker.World.GetBlockGlobalPosition(secondPartPosition);

        var camOffset = (secondPartGlobalPosition - firstPartGlobalPosition) / 2;

        BlockPhotoMaker.Camera.Transform.Position += camOffset.ProjectOnPlane(BlockPhotoMaker.Camera.Transform.Rotation.Forward);

        var heightMultiplier = direction.Axis == Mth.Enums.Axis.Z ? 98f / 70f : 84f / 70f; // TODO: calculate?
        BlockPhotoMaker.Camera.OrthographicHeight *= heightMultiplier;
    }
}
