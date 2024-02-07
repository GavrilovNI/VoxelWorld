using Sandbox;
using Sandcube.Inventories;
using Sandcube.Items;

namespace Sandcube.UI.Inventories;

public interface ISlotMouseEventListener
{
    void OnMouseClickedOnSlot(IReadOnlyIndexedCapability<Stack<Item>> capability, int slotIndex, MouseButtons mouseButton) { }
    void OnMouseDownOnSlot(IReadOnlyIndexedCapability<Stack<Item>> capability, int slotIndex, MouseButtons mouseButton) { }
    void OnMouseUpOnSlot(IReadOnlyIndexedCapability<Stack<Item>> capability, int slotIndex, MouseButtons mouseButton) { }
}
