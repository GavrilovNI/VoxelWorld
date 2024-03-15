using Sandbox;
using Sandcube.Entities.Types;
using Sandcube.Registries;

namespace Sandcube.Mods.Base.Entities;

public sealed class BaseModEntities : ModEntities
{
    private static ModedId MakeId(string entityId) => new(BaseMod.ModName, entityId);

    [Property] private PrefabScene PlayerPrefab { get; set; } = null!;
    [AutoPrefabEntityType(BaseMod.ModName)] public EntityType Player { get; private set; } = null!;

    [Property] private PrefabScene PhysicsBlockPrefab { get; set; } = null!;
    [AutoPrefabEntityType(BaseMod.ModName)] public EntityType PhysicsBlock { get; private set; } = null!;

    [Property] private PrefabScene ItemStackPrefab { get; set; } = null!;
    [AutoPrefabEntityType(BaseMod.ModName)] public EntityType ItemStack { get; private set; } = null!;
}
