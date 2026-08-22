using Godot;
using NullGarel.Sandboxnator.Placeables;
using NullGarel.Sandboxnator.Registry;
using NullGarel.Util.GodotHelpers;
using NullGarel.Util.Log;
using Godot.Collections;
using System.Linq;
using NullGarel.Sandboxnator.Entity;

namespace NullGarel.Sandboxnator.Item;

//TODO: There's something really important to be done: store data in this item persistently.
// For instance, if you change the color and scroll to another item and scroll back to this
// one, the color is reset and completely forgotten.
// I'm thinking of a dictionary for this purpose.
// Minecraft does this for things like durability, custom item names, custom properties, etc..
// And I think this is what NBT does on minecraft.
// I don't even know if this is inspiration or plagiarizing at this point xd

// Update as of 2026.8.8_saturday_(18:00 GMT-3)
// Now I am using dictionaries for the item state, and, for every item, there will be a comment specifying what the dictionary for an item state should look like in case an item requires an ItemState, so this is the current state of the paint bubble.

// There is no inventory or stack system yet, the dictionary implementation works flawlessly. There's just the persistent item data storage on the inventory left, since the inventory items are stored in the inventory just as string IDs I guess that's the main problem. There's another minor sync problem, when a guest player joins and the host player changed the item state before the joining of the guest, the guest doesn't know the actual colour the host player is holding until the host player updates it, whick is expected since there's no synchronizer, just a RPC for such event.

/// <summary>
/// An item that paints <see cref="Placeable"/> items with the <see cref="Paintable"/> component
/// 
/// Dictionary keys specification:
/// ColorIndex:int - Represents a colour from sandboxnator's registry colour pallete.
/// </summary>
public partial class PaintBubble : BaseItem
{
    [Export] private MeshInstance3D _bubble;
    [Export] private int _colorIndex = 0;

    private Color[] _colors = [.. GameRegistries.Instance.ContentDatabase.BuildingPallete.GetImage().PixelsOfImage()];

    public override void _EnterTree()
    {
        UpdateVisualLocal();
    }

    public override void UseItem(ItemUsageArgs args)
    {
        if (args.IsPrimaryUse)
        {
            var hitObject = ItemUser.rayCast.GetCollider();
            if (hitObject is not Placeable hitPlaceable)
                return;

            var paintable = hitPlaceable.componentHolder
                .GetChildren()
                .OfType<Paintable>()
                .FirstOrDefault();

            if (paintable == null)
            {
                NcLogger.Log($"Missing paintable component in {hitPlaceable.Name}");
                return;
            }

            paintable.TriggerPaint(_colors[_colorIndex]);
        }
        else
        {
            CycleColor();
        }
    }

    private void CycleColor()
    {
        _colorIndex = (_colorIndex + 1) % _colors.Length;
        PlayerItemSync playerItemSync = ItemUser.GetComponent<PlayerItemSync>();
        playerItemSync.BroadcastItemState(GetItemState());
    }

    // this used to be a whole RPC thing but now it's an overriden method from BaseItem
    public override void ReceiveItemState(Dictionary stateData)
    {
        if (stateData.TryGetValue("ColorIndex", out var variantColor))
        {
            _colorIndex = variantColor.AsInt32();
            UpdateVisualLocal();
        }
    }

    public override Dictionary GetItemState()
    {
        return new Dictionary { { "ColorIndex", _colorIndex } };
    }

    private void UpdateVisualLocal()
    {
        _bubble.ChangeMeshColor(_colors[_colorIndex]);
    }
}