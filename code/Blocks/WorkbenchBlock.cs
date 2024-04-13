using System.Collections.Generic;
using System.Threading.Tasks;
using VoxelWorld.Interactions;
using VoxelWorld.Menus;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Registries;
using VoxelWorld.Texturing;

namespace VoxelWorld.Blocks;

public class WorkbenchBlock : SimpleBlock
{
    public WorkbenchBlock(in ModedId id, IUvProvider uvProvider) : base(id, uvProvider)
    {
    }

    public WorkbenchBlock(in ModedId id, IReadOnlyDictionary<Direction, IUvProvider> uvProviders) : base(id, uvProviders)
    {
    }

    public override Task<InteractionResult> OnInteract(BlockActionContext context)
    {
        var menu = new WorkbenchMenu(context.Player, context.World, context.Position);
        MenuController.Instance!.Open(menu);
        return Task.FromResult(InteractionResult.Success);
    }
}
