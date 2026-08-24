using Godot;
namespace NullGarel.Sandboxnator.Item;

[GlobalClass]
public partial class Inventory : Node
{
    public const int SlotCount = 32;

    private ItemStack[] _slots = [
        new("Hammer", 1),
        new("SmoothCube", 1),
        new("PaintBubble", 1),
        new("Door", 1),
        new("Compass", 1),
    ];

    public int Count => SlotCount;

    public ItemStack GetSlot(int index)
        => _slots[index];

    public void SetSlot(int index, ItemStack stack)
        => _slots[index] = stack;
}