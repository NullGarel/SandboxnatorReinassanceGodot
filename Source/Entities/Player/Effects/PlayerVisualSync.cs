using Godot;
using Godot.Collections;
using NullGarel.Util.ComponentSystem;
using NullGarel.Util.GodotHelpers;

namespace NullGarel.Sandboxnator.Entity;

/// <summary>
/// A component responsible for the cosmetics, animations and profile synchronization.
/// </summary>
public partial class PlayerVisualSync : AbstractComponent<Player>
{
	[ExportCategory("In-Game")]
	[Export] private Array<Node3D> _elementsToHideAsFirstPerson;

	[ExportCategory("Player model")]
	[Export] private PlayerModel _playerModel;
	[Export] public Label3D _nameTag;
	[Export] private Array<Node3D> _rotateAlongNeck;
	[Export] private Node3D _neck;

	[ExportCategory("Animations")]
	[Export] private AnimationPlayer _movementStateAnimation;
	private const string IdleAnimation = "IdleAndHold";
	private const string WalkAnimation = "WalkAndHold";
	private const string FlightAnimation = "Flight";
	private MovementState? _lastMovementState = null;

	//Serialization
	private PlayerProfileData _profileData = new();
	private Dictionary _profileDataDict;

	//components
	private PlayerMovement _playerMovement;

	/// <summary>
	/// This dictionary is directly synced by <see cref="MultiplayerSynchronizer"/>
	/// </summary>
	[Export]
	public Dictionary ProfileDataDict
	{
		get => _profileDataDict;
		set
		{
			_profileDataDict = value;
			if (value != null && value.Count > 0)
			{
				_profileData = DictPack.Unpack<PlayerProfileData>(value);
				ApplyProfile(_profileData);
			}
		}
	}

	public override void _EnterTree()
	{
		if (IsMultiplayerAuthority())
		{
			foreach (Node3D element in _elementsToHideAsFirstPerson)
				element.Visible = false;
			ProfileDataDict = DictPack.Pack(PlayerProfileManager.Instance.CurrentProfile);
			_playerModel.handMesh.SetMeshClip(true);
		}
	}

	public override void _Ready()
	{
		_playerMovement = GetComponent<PlayerMovement>();
	}

	public override void _Process(double delta)
	{
		foreach (Node3D target in _rotateAlongNeck)
		{
			target.GlobalRotation = _neck.GlobalRotation;
		}

		MovementState currentState = _playerMovement.MovementType;
		if (_lastMovementState != currentState)
		{
			ChangeMovementAnimation(currentState);
			_lastMovementState = currentState;
		}
	}


	private void ChangeMovementAnimation(MovementState newState)
	{
		if (_movementStateAnimation.HasAnimation("RESET"))
		{
			_movementStateAnimation.Play("RESET");
			_movementStateAnimation.Advance(0);
		}

		string targetAnimation = newState switch
		{
			MovementState.Idle => IdleAnimation,
			MovementState.Walk => WalkAnimation,
			MovementState.Sprint => WalkAnimation,
			MovementState.Fly => FlightAnimation,
			_ => IdleAnimation
		};

		_movementStateAnimation.Play(targetAnimation);
	}

	/// <summary>
	/// This method is responsible for turning the profile data into visual information in-game.
	/// </summary>
	/// <param name="profile"></param>
	public void ApplyProfile(PlayerProfileData profile)
	{
		if (profile == null) return;

		if (ComponentParent != null)
		{
			ComponentParent.ProfileData = _profileData;
		}

		if (IsInstanceValid(_nameTag))
		{
			_nameTag.Text = profile.PlayerName;
			_nameTag.Modulate = profile.PlayerColor;
			_nameTag.OutlineModulate = ColorAndMeshUtils.InvertColor(profile.PlayerColor);
		}

		if (IsInstanceValid(_playerModel))
		{
			_playerModel.UpdateVisual(profile);
		}
	}
}
