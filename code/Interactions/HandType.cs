

namespace VoxelWorld.Interactions;

public enum HandType
{
    Main,
    Secondary
}

public static class HandTypeExtensions
{
    public static HandType GetOpposite(this HandType hand) =>
        hand == HandType.Main ? HandType.Secondary : HandType.Main;
}
