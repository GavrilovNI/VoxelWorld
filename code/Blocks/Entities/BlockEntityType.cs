using VoxelWorld.Mth;
using VoxelWorld.Registries;
using VoxelWorld.Worlds;
using System;

namespace VoxelWorld.Blocks.Entities;

public class BlockEntityType : IRegisterable
{
    public ModedId Id { get; }
    private readonly Func<BlockEntity> _entitySupplier;

    public BlockEntityType(ModedId id, Func<BlockEntity> entitySupplier)
    {
        Id = id;
        _entitySupplier = entitySupplier;
    }

    public BlockEntity CreateBlockEntity() => _entitySupplier();
    public BlockEntity CreateBlockEntity(IWorldAccessor world, Vector3Int position)
    {
        var entity = CreateBlockEntity();
        entity.Initialize(world, position);
        return entity;
    }
}
