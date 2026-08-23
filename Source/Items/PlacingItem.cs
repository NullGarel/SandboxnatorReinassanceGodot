using Godot;
using Godot.Collections;
using System;
using NullGarel.Sandboxnator.Audio;
using NullGarel.Sandboxnator.Entity;
using NullGarel.Sandboxnator.Placeables;
using NullGarel.Util.GodotHelpers;
namespace NullGarel.Sandboxnator.Item;

[GlobalClass]
[GodotClassName("PlacingItem")]
public partial class PlacingItem : BaseItem
{

	[ExportCategory("Overrides")]
	[Export] private Array<MeshInstance3D> _materialOverrideMeshes;
	[ExportCategory("Preview")]
	[Export] private MeshInstance3D _previewMesh;
	[Export] private PreviewCollider _previewCollider;
	[Export] private Vector3 _previewMeshOffset = Vector3.Zero;

	private PlaceableItemData _itemData;

	public override void _Ready()
	{
		_itemData = (PlaceableItemData)itemData;
		ComputeMaterialOverride();
	}

	private void ComputeMaterialOverride()
	{
		if (_itemData.MaterialOverride == null || _materialOverrideMeshes.Count == 0)
			return;

		foreach (var mesh in _materialOverrideMeshes)
		{
			mesh.ChangeMeshMaterial(_itemData.MaterialOverride);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!ItemUser.ComponentParent.IsMultiplayerAuthority())
		{
			_previewMesh.Visible = false;
			return;
		}
		GeneratePreviewMesh();
	}

	//Client Side
	private void GeneratePreviewMesh()
	{
		PlayerInput playerInput = ItemUser.GetComponent<PlayerInput>();

		ItemUser.isUseValid = !_previewCollider.IsColliding;

		_previewMesh.Visible = ItemUser.rayCast.IsColliding() && ItemUser.isUseValid;
		_previewMesh.GlobalPosition = GetSnappedPosition(
			ItemUser.rayCast.GetCollisionPoint(),
			ItemUser.rayCast.GetCollisionNormal(),
			playerInput.IsGridSnapMode
			) + _previewMeshOffset;
		_previewMesh.GlobalRotation = ItemUser.DesiredRotation;
		_previewCollider.GlobalPosition = _previewMesh.GlobalPosition;
		_previewCollider.GlobalRotation = _previewMesh.GlobalRotation;

	}

	//Server side
	public override void UseItem(ItemUsageArgs args)
	{
		PlayerInput playerInput = ItemUser.GetComponent<PlayerInput>();
		if (!ItemUser.isUseValid) return;

		var spawnData = new PlaceableSpawnData
		{
			ItemId = _itemData.ItemId,
			Position = GetSnappedPosition(args.Position, args.Normal, playerInput.IsGridSnapMode),
			Rotation = args.DesiredRotation
		};

		Node spawned = SandboxnatorMain.World.PlaceableSpawner.Spawn(DictPack.Pack(spawnData));
		PlayPlacingSound(spawnData.Position);
	}

	private void PlayPlacingSound(Vector3 placementPosition)
	{
		// When building logic places an object:
		WorldAudioManager.Instance.PlaySoundAt(((PlaceableItemData)itemData).PlacementSound, placementPosition);
	}

	private Vector3 GetSnappedPosition(Vector3 collisionPoint, Vector3 collisionNormal, bool hasGrid)
	{
		Vector3 offsetPos = collisionPoint + (collisionNormal * 0.5f);
		if (!hasGrid)
			return SandboxnatorMain.World.GetNearestSnapper(offsetPos, _itemData.SnapRange);

		//else if has a grid
		Vector3 snapped = new
		(
			Mathf.Floor(offsetPos.X + _itemData.GridSize.X),
			Mathf.Floor(offsetPos.Y + _itemData.GridSize.Y),
			Mathf.Floor(offsetPos.Z + _itemData.GridSize.Z)
		);
		return snapped;
	}

}
