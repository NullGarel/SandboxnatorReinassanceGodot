using Godot;
using NullGarel.Util.ComponentSystem;
using NullGarel.Util.StateMachine;
namespace NullGarel.Sandboxnator.Entity;

public enum MovementState
{
	Idle,
	Walk,
	Sprint,
	Jump,
	Fall,
	Fly
}

[GodotClassName("PlayerMovement")]
public partial class PlayerMovement : AbstractComponent<Player>
{
	[ExportCategory("Nodes")]
	[Export] private CharacterBody3D _characterBody;
	[Export] private Node3D _neck;
	[ExportCategory("Movement parameters")]
	[Export]
	public PlayerMovementStats Stats
	{
		get => _stats;
		set
		{
			_stats = value;
			SetupContextToStats();
		}
	}
	private PlayerMovementStats _stats;

	private PlayerMovementContext _context;
	private StateMachine<PlayerMovementContext> _stateMachine;
	private PlayerInput _playerInput;

	public float HorizontalSpeed
	{
		get
		{
			Vector3 v = _context?.Velocity ?? Vector3.Zero;
			return new Vector3(v.X, 0, v.Z).Length();
		}
	}

	[Export(PropertyHint.Enum, "Do not alter it in the editor. This is used for animations and for the internal FSM.")]
	public MovementState MovementType { get; private set; }

	public override void _Ready()
	{
		if (!ComponentParent.IsMultiplayerAuthority())
			return;

		_playerInput = GetComponent<PlayerInput>();

		SetupContextToStats();

		_stateMachine = new StateMachine<PlayerMovementContext>(_context, new StateIdle());
		MovementType = MovementState.Idle;

		_playerInput.OnStopSprint += StopSprint;
		_playerInput.OnFly += () => { _context.FlyMode = !_context.FlyMode; };
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!Multiplayer.HasMultiplayerPeer()) return;
		if (_characterBody == null) return;
		if (!ComponentParent.IsMultiplayerAuthority()) return;

		MovementProcess(delta);
	}

	private void SetupContextToStats()
	{
		_context = new PlayerMovementContext
		{
			CharacterBody = _characterBody,
			Input = _playerInput,
			WalkSpeed = _stats.WalkSpeed,
			SprintSpeed = _stats.SprintSpeed,
			JumpVelocity = _stats.JumpVelocity,
			CurrentSpeed = _stats.WalkSpeed
		};
	}

	private void MovementProcess(double delta)
	{
		_context.Facing = _neck.GlobalTransform.Basis.Z;
		_context.SetVelocity(_characterBody.Velocity);

		_stateMachine.PhysicsProcess(delta);
		MovementType = MapStateToEnum(_stateMachine.CurrentState);

		_characterBody.Velocity = _context.Velocity;
		_characterBody.MoveAndSlide();
	}

	private void StopSprint()
	{
		if (_stateMachine.CurrentState is StateSprint)
			_stateMachine.ChangeState(new StateWalk());
	}

	private static MovementState MapStateToEnum(IState<PlayerMovementContext> state) => state switch
	{
		StateIdle => MovementState.Idle,
		StateWalk => MovementState.Walk,
		StateSprint => MovementState.Sprint,
		StateJump => MovementState.Jump,
		StateFall => MovementState.Fall,
		StateFly => MovementState.Fly,
		_ => MovementState.Idle
	};
}