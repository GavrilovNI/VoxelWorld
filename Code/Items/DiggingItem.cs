using Sandbox;
using System.Threading.Tasks;
using VoxelWorld.Interactions;
using VoxelWorld.Mods.Base;
using VoxelWorld.Mods.Base.Blocks.Data;
using VoxelWorld.Registries;

namespace VoxelWorld.Items;

public class DiggingItem : Item
{
    public DiggingItem(in ModedId id, Model[] models, Texture texture, bool isFlatModel = false) : base(id, models, texture, isFlatModel)
    {

    }

    public DiggingItem(in ModedId id, Model[] models, Texture texture, int stackLimit, bool isFlatModel = false) : base(id, models, texture, stackLimit, isFlatModel)
    {

    }

    public override async Task<InteractionResult> OnAttack(ItemActionContext itemContext)
    {
        if(!BlockActionContext.TryMakeFromItemActionContext(itemContext, out var context))
            return await base.OnAttack(itemContext);

        var progressDataType = BaseMod.Instance!.BlockDataTypes.BreakingProgress;

        var progress = context.World.GetAdditionalData(progressDataType, context.Position);
        progress += 0.3f;
        await context.World.SetAdditionalData(progressDataType, context.Position, progress);

        return InteractionResult.Success;
    }
}
