using Godot;
using Godot.Collections;

namespace NullGarel.Sandboxnator.Item;

public partial class ItemStack(string itemId, int amount)
{
    [Export]
    public string ItemId { get; set; } = itemId;

    [Export]
    public int Amount { get; set; } = amount;

    [Export]
    public Dictionary StackData { get; set; } = [];

    public bool IsEmpty => string.IsNullOrEmpty(ItemId) || Amount <= 0;
}