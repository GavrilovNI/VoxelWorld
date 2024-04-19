using System.Diagnostics.CodeAnalysis;
using VoxelWorld.Blocks.States;
using VoxelWorld.Mth;
using VoxelWorld.Mth.Enums;
using VoxelWorld.Worlds;

namespace VoxelWorld.Interactions;

public record class BlockAttackingContext : BlockActionContext
{
    public float _damage;
    public required float Damage
    {
        get => _damage;
        set
        {
            if(value < 0)
                throw new System.ArgumentOutOfRangeException(nameof(value), value, $"{nameof(Damage)} can't be negative");
            _damage = value;
        }
    }

    public BlockAttackingContext()
    {
    }

    public BlockAttackingContext(BlockAttackingContext context) : base(context)
    {
        Damage = context.Damage;
    }

    public BlockAttackingContext(BlockActionContext context, float damage) : base(context)
    {
        Damage = damage;
    }

    [SetsRequiredMembers]
    public BlockAttackingContext(ItemActionContext itemActionContext, IWorldAccessor world, Vector3IntB position, BlockState blockState, float damage) : base(itemActionContext, world, position, blockState)
    {
        Damage = damage;
    }

    public static BlockAttackingContext operator +(in BlockAttackingContext context, Direction direction)
    {
        var newPosition = context.Position + direction;
        return context with
        {
            Position = newPosition,
            BlockState = context.World.GetBlockState(newPosition)
        };
    }
}
