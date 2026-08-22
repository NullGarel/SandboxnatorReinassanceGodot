using Godot.Collections;
using Godot;
using NullGarel.Sandboxnator.Item;
using NullGarel.Util.ComponentSystem;
using System.Linq;
using NullGarel.Util.GodotHelpers;
namespace NullGarel.Sandboxnator.Placeables;

public partial class Placeable : RigidBody3D
{
    //TODO: destroy animation + health system
    [Export] public ComponentHolder componentHolder;
    public bool HasInteractable { get; private set; }
    public PlaceableItemData ItemData { get; set; }
    [Export] private Array<MeshInstance3D> _materialOverrideMeshes;

    public override void _Ready()
    {
        QueryForInteractables();
        ComputeMaterialOverride();
    }

    private void ComputeMaterialOverride()
    {
        if (ItemData.MaterialOverride == null || _materialOverrideMeshes.Count == 0)
            return;

        foreach (var mesh in _materialOverrideMeshes)
        {
            mesh.ChangeMeshMaterial(ItemData.MaterialOverride);
        }
    }

    private void QueryForInteractables()
    {
        var interactable = componentHolder
        .GetChildren()
        .OfType<IInteractable>()
        .FirstOrDefault();
        HasInteractable = interactable != null;
    }

    public void Destroy()
    {
        QueueFree();
    }

}
