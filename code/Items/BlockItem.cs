using Sandbox;
using VoxelWorld.Blocks;
using VoxelWorld.Interactions;
using VoxelWorld.Meshing;
using VoxelWorld.Mth.Enums;
using System.Threading.Tasks;
using VoxelWorld.Mth;
using VoxelWorld.Worlds;
using VoxelWorld.Blocks.States;
using System.Linq;

namespace VoxelWorld.Items;

public class BlockItem : Item
{
    public readonly string[] PlacingCollidingTags = new string[] { "player" };
    public readonly string[] PlacingIgnoringTags = new string[] { "world", "world_part" };

    public readonly Block Block;

    public BlockItem(Block block, Model[] models, Texture texture, int stackLimit, bool isFlatModel = false) : base(block.Id, models, texture, stackLimit, isFlatModel)
    {
        Block = block;
    }

    public BlockItem(Block block, Model[] models, Texture texture, bool isFlatModel = false) : base(block.Id, models, texture, isFlatModel)
    {
        Block = block;
    }


    protected virtual bool CanPlaceBlock(IWorldAccessor world, BlockState blockState, Vector3IntB blockPosition)
    {
        var position = world.GetBlockGlobalPosition(blockPosition);
        var physicsMesh = GameController.Instance!.BlockMeshes.GetPhysics(blockState)!;

        var worldGameObject = world.GameObject;
        var scene = worldGameObject.Scene;

        var bounds = physicsMesh.Bounds.Transform(worldGameObject.Transform.World.WithPosition(position));
        var collidesWithSomething = scene.FindInPhysics(bounds)
            .Where(o => (PlacingCollidingTags.Length == 0 || o.Tags.HasAny(PlacingCollidingTags)) && !o.Tags.HasAny(PlacingIgnoringTags)).Any();

        if(collidesWithSomething)
            return false;

        PhysicsBody body = new(scene.PhysicsWorld);
        body.AddHullShape(0, Rotation.Identity, physicsMesh.CombineVertices().Select(v => v.Position).ToList());
        var trace = scene.Trace.Body(body, worldGameObject.Transform.World.WithPosition(position), position)
            .UsePhysicsWorld();
        if(PlacingCollidingTags.Length != 0)
            trace = trace.WithAnyTags(PlacingCollidingTags);
        trace = trace.WithoutTags(PlacingIgnoringTags);

        var result = trace.Run();
        return !result.Hit;
    }

    public override async Task<InteractionResult> OnUse(ItemActionContext context)
    {
        var traceResult = context.TraceResult;

        if(!traceResult.Hit)
            return InteractionResult.Pass;

        if(!BlockActionContext.TryMakeFromItemActionContext(context, out var blockContext))
            return InteractionResult.Pass;

        if(!await TryPlace(blockContext))
        {
            blockContext += Direction.ClosestTo(blockContext.TraceResult.Normal);
            if(!await TryPlace(blockContext))
                return InteractionResult.Fail;
        }

        return InteractionResult.Success;
    }

    protected virtual async Task<bool> TryPlace(BlockActionContext context)
    {
        var currentBlockState = context.BlockState;

        var stateToPlace = Block.GetStateForPlacement(context);
        var block = stateToPlace.Block;

        bool canReplace = currentBlockState.Block.CanBeReplaced(context.BlockState, stateToPlace);
        if(!canReplace)
            return false;

        bool canStay = block.CanStay(context.World, context.Position, stateToPlace);
        if(!canStay)
            return false;

        if(!CanPlaceBlock(context.World, stateToPlace, context.Position))
            return false;

        var contextCopy = context;

        var changed = await context.World.SetBlockState(context.Position, stateToPlace);
        if(changed)
        {
            block.OnPlaced(contextCopy, stateToPlace);

            var blockCenterPosition = context.World.GetBlockGlobalPosition(context.Position) + MathV.UnitsInMeter / 2f;
            Sound.Play(block.Properties.PlaceSound, blockCenterPosition);
        }

        return changed;
    }

    public static bool TryFind(Block block, out BlockItem item)
    {
        var game = GameController.Instance!;
        if(!game.Registries.TryGetRegistry<Item>(out var registry))
        {
            item = null!;
            return false;
        }

        if(registry.TryGet(block.Id, out var foundItem) &&
            foundItem is BlockItem blockItem &&
            blockItem.Block == block)
        {
            item = blockItem;
            return true;
        }

        foreach(var currentItem in registry)
        {
            if(currentItem is BlockItem currentBlockItem && currentBlockItem.Block == block)
            {
                item = currentBlockItem;
                return true;
            }
        }

        item = null!;
        return false;
    }
}
