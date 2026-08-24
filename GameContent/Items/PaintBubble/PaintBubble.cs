using Godot;
using NullGarel.Sandboxnator.Placeables;
using NullGarel.Sandboxnator.Registry;
using NullGarel.Util.GodotHelpers;
using NullGarel.Util.Log;
using Godot.Collections;
using System.Linq;
using NullGarel.Sandboxnator.Entity;

namespace NullGarel.Sandboxnator.Item;

//update as of 20260824 :: I think persisting item stacks work now.

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
        playerItemSync.PersistAndBroadcast(GetItemState());
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