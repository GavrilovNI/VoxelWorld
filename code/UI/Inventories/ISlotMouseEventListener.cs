using Sandbox;
using Sandcube.Inventories;
using Sandcube.Items;

namespace Sandcube.UI.Inventories;

public interface ISlotMouseEventListener
{
    void OnMouseClickedOnSlot(IIndexedCapability<Stack<Item>> capability, int slotIndex, MouseButtons mouseButton) { }
    void OnMouseDownOnSlot(IIndexedCapability<Stack<Item>> capability, int slotIndex, MouseButtons mouseButton) { }
    void OnMouseUpOnSlot(IIndexedCapability<Stack<Item>> capability, int slotIndex, MouseButtons mouseButton) { }
}
