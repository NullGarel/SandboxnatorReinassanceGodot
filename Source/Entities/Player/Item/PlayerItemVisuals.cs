using Godot;
using NullGarel.Sandboxnator.Item;
using NullGarel.Sandboxnator.Registry;
using NullGarel.Util.ComponentSystem;

namespace NullGarel.Sandboxnator.Entity;

/// <summary>
/// Responsible for handling the item visuals and instantiating the proper item scene.
/// </summary>
public partial class PlayerItemVisuals : AbstractComponent<Player>
{
	[Export] private Node3D _hand;
	[Export] private AnimationPlayer _handAnimator;

	private BaseItem _activeItemNode;
	private PlayerItemSync _playerItemSync;
	private PlayerItemUse _playerItemUse;

	public override void _Ready()
	{
		base._Ready();
		_playerItemSync = GetComponent<PlayerItemSync>();
		_playerItemUse = GetComponent<PlayerItemUse>();

		_playerItemSync.OnItemEquipped += UpdateItemModel;
		if (!string.IsNullOrEmpty(_playerItemSync.CurrentItemId))
		{
			UpdateItemModel(_playerItemSync.CurrentItemId);
		}
	}

	private void UpdateItemModel(string itemId)
	{
		foreach (var model in _hand.GetChildren())
		{
			_hand.RemoveChild(model);
			model.QueueFree();
		}

		if (string.IsNullOrEmpty(itemId)) return;

		ItemData itemResource = GameRegistries.Instance.ItemRegistry.Get(itemId);
		_activeItemNode = itemResource.ItemScene.Instantiate<BaseItem>();
		_activeItemNode.itemData = itemResource;
		_activeItemNode.Name = "EquippedItem";
		_activeItemNode.ItemUser = _playerItemUse;
		_playerItemUse.SetActiveItem(_activeItemNode);
		_activeItemNode.ReceiveItemState(_playerItemSync.ActiveItemState);
		_hand.AddChild(_activeItemNode);
	}

	public void PlayUseAnimation()
	{
		if (_activeItemNode != null && _activeItemNode.itemData.AnimateHand)
		{
			_handAnimator.Stop();
			_handAnimator.Play("HandUse");
		}
	}
}
