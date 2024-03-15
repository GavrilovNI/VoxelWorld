using Sandbox;
using VoxelWorld.Inventories;
using VoxelWorld.Items;

namespace VoxelWorld.UI.Inventories;

public interface ISlotMouseEventListener
{
    void OnMouseClickedOnSlot(IReadOnlyIndexedCapability<Stack<Item>> capability, int slotIndex, MouseButtons mouseButton) { }
    void OnMouseDownOnSlot(IReadOnlyIndexedCapability<Stack<Item>> capability, int slotIndex, MouseButtons mouseButton) { }
    void OnMouseUpOnSlot(IReadOnlyIndexedCapability<Stack<Item>> capability, int slotIndex, MouseButtons mouseButton) { }
}
