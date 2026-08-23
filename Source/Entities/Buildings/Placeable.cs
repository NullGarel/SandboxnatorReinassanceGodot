using Godot.Collections;
using Godot;
using NullGarel.Sandboxnator.Item;
using NullGarel.Util.ComponentSystem;
using System.Linq;
using NullGarel.Util.GodotHelpers;
using NullGarel.Sandboxnator.Data;
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
        GD.Print(Json.Stringify(Serialized()));
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

    public Dictionary Serialized()
    {
        var spawnData = new PlaceableSpawnData
        {
            ItemId = ItemData.ItemId,
            Position = Position,
            Rotation = Rotation
        };

        var dict = DictPack.Pack(spawnData);
        dict["ComponentHolder"] = new Dictionary
        {
            ["Components"] = DictPackExtensions.SerializeComponents(componentHolder.GetAllComponents())
        };
        return dict;
    }
}
