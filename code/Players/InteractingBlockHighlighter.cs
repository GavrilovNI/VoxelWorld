using Sandbox;
using System;
using VoxelWorld.Rendering;
using VoxelWorld.Worlds;

namespace VoxelWorld.Players;

public class InteractingBlockHighlighter : Component
{
    [Property, RequireComponent] protected WorldInteractor WorldInteractor { get; set; } = null!;
    [Property, RequireComponent] protected GameObject HighlightedBlockPrefab { get; set; } = null!;

    protected HighlightedBlock? HighlightedBlock;


    protected virtual void UpdateHighlightedBlock()
    {
        if(WorldInteractor.Active)
            UpdateHighlightedBlock(WorldInteractor.TraceResult);
        else
            HighlightedBlock!.GameObject.Enabled = false;
    }

    protected virtual void UpdateHighlightedBlock(PhysicsTraceResult traceResult)
    {
        if(!traceResult.Hit || !World.TryFindInObject(traceResult.Body?.GetGameObject(), out var world))
        {
            HighlightedBlock!.GameObject.Enabled = false;
            return;
        }

        var blockPosition = world.GetBlockPosition(traceResult.EndPosition, traceResult.Normal);
        HighlightedBlock!.Transform.World = world.GameObject.Transform.World.WithPosition(world.GetBlockGlobalPosition(blockPosition));
        HighlightedBlock.BlockState = world.GetBlockState(blockPosition);
        HighlightedBlock.GameObject.Enabled = true;
    }


    protected override void OnAwake()
    {
        var gameObject = HighlightedBlockPrefab.Clone();
        HighlightedBlock = gameObject.Components.Get<HighlightedBlock>();
        if(!HighlightedBlock.IsValid())
            throw new InvalidOperationException($"Couldn't create {nameof(Rendering.HighlightedBlock)}");

        UpdateHighlightedBlock();
    }

    protected override void OnUpdate() => UpdateHighlightedBlock();
    protected override void OnEnabled()
    {
        if(!HighlightedBlock.IsValid())
            return;

        HighlightedBlock.GameObject.Enabled = true;
        UpdateHighlightedBlock();
    }
    protected override void OnDisabled()
    {
        if(HighlightedBlock.IsValid())
            HighlightedBlock.GameObject.Enabled = false;
    }
    protected override void OnDestroy()
    {
        if(HighlightedBlock.IsValid())
            HighlightedBlock.GameObject.Destroy();
    }
}
