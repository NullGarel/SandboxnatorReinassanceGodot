using Godot;
namespace NullGarel.Sandboxnator.Item;

/// <summary>
/// The data representation of an item in areas such as the inventory(as an ID) or the game registry in general.
/// </summary>
[GlobalClass]
public partial class ItemData : Resource
{
    [ExportCategory("Basic properties")]
    [Export] public PackedScene ItemScene { get; private set; }
    [Export] public string ItemId { get; private set; }

    [ExportCategory("Visual information")]
    [Export] public Texture2D ItemIcon { get; private set; }
    [Export] public bool AnimateHand { get; private set; } = true;
    [Export] public string ItemName { get; private set; }

    [ExportCategory("Inventory properties")]
    [Export] public int MaxStackSize { get; private set; } = 96;
    public bool IsStackable { get => MaxStackSize > 1; }

    [ExportCategory("Usage parameters")]
    [Export] public float RaycastReachMultiplier { get; private set; } = 1;
    [Export] public float UsageCooldown { get; private set; } = 0.125f;
}

