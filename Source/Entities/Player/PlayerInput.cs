using Godot;
using System;
using NullGarel.Util.ComponentSystem;
using NullGarel.Sandboxnator.Registry;
namespace NullGarel.Sandboxnator.Entity;

public partial class PlayerInput : AbstractComponent<Player>
{
    //movement
    public Vector2 MovementVector { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsJumping { get; private set; }
    public event Action OnStopSprint;
    public event Action OnFly;
    //user interface

    public event Action OnShowChat;
    public event Action OnUiEscape;
    //Camerevent a
    public event Action OnToggleCursorCapture;
    public Vector2 LookVector { get; private set; }

    public event Action OnMouseMovement;
    //Building
    public event Action RotateCW;
    public event Action RotateCCW;
    public event Action<bool> OnChangeSnapMode;
    //has to be exported because synchronizerisms....
    [Export] public bool IsGridSnapMode { get; private set; } = true;
    //usage
    public event Action UsePrimary;
    public event Action UseSecondary;
    public event Action UseIncrement;
    public event Action UseDecrement;
    public event Action Interact;

    #region "magic strings" and constants
    private const float JoypadSensitivityDenominator = 100.0f;

    private const string ToggleCaptureAction = "toggle_capture";
    private const string UiEscapeAction = "sb_ui_escape";
    private const string ShowChatAction = "sb_ui_show_chat";

    private const string MvJumpAction = "mv_jump";
    private const string SprintAction = "mv_sprint";
    private const string MvLeftAction = "mv_left";
    private const string MvRightAction = "mv_right";
    private const string MvForwardAction = "mv_forward";
    private const string MvBackwardAction = "mv_backward";
    private const string MvSprintAction = "mv_sprint";
    private const string FlyAction = "fly";

    private const string BuildRotateClockwiseAction = "build_rotate_cw";
    private const string BuildRotateCounterClockwiseAction = "build_rotate_ccw";
    private const string BuildChangeSnapAction = "build_change_snap";

    private const string UsePrimaryAction = "use_primary";
    private const string UseSecondaryAction = "use_secondary";
    private const string UseIncrementAction = "use_increment";
    private const string UseDecrementAction = "use_decrement";
    private const string InteractAction = "interact";

    private const string LookLeftAction = "look_left";
    private const string LookRightAction = "look_right";
    private const string LookUpAction = "look_up";
    private const string LookDownAction = "look_down";
    #endregion

    private PlayerHUD _playerHud;

    public override void _Ready()
    {
        if (!IsMultiplayerAuthority()) return;
        _playerHud = GetComponent<PlayerHUD>();
    }

    #region Overrides
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!IsMultiplayerAuthority()) return;

        if (!_playerHud.IsHudBeingUsed)
        {
            HandleMovementInput();
            HandleBuildingInput();
            HandleUsageInput();
            HandleJoypadRstickInput();
            HandleTopLevelUiInput();
        }
        HandleGeneralUserInterfaceInput();
    }

    public override void _Input(InputEvent _event)
    {
        if (!IsMultiplayerAuthority())
            return;

        if (_event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            Vector2 mouseLookVector = new(mouseMotion.Relative.X, mouseMotion.Relative.Y);
            LookVector = mouseLookVector;
            OnMouseMovement?.Invoke();
        }
    }
    #endregion


    #region Handlers
    private void HandleGeneralUserInterfaceInput()
    {

        if (Input.IsActionJustPressed(UiEscapeAction))
        {
            OnUiEscape?.Invoke();
        }

    }

    /// <summary>
    /// Top-Level UI Input refers to UI related commands that are supposed to run
    /// when no UI menu is active.
    /// </summary>
    private void HandleTopLevelUiInput()
    {
        if (Input.IsActionJustPressed(ToggleCaptureAction))
        {
            OnToggleCursorCapture?.Invoke();
            GetViewport().SetInputAsHandled();
        }
        if (Input.IsActionPressed(ShowChatAction))
        {
            OnShowChat?.Invoke();
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GetViewport().SetInputAsHandled();
        }
    }


    private void HandleMovementInput()
    {

        if (Input.IsActionJustPressed(MvJumpAction))
        {
            IsJumping = true;
        }

        if (Input.IsActionJustPressed(FlyAction))
        {
            OnFly?.Invoke();
        }

        if (Input.IsActionJustReleased(MvJumpAction))
        {
            IsJumping = false;
        }

        if (Input.IsActionJustReleased(SprintAction))
        {
            OnStopSprint?.Invoke();
        }

        MovementVector = Input.GetVector(MvLeftAction, MvRightAction, MvForwardAction, MvBackwardAction);
        IsSprinting = Input.IsActionPressed(MvSprintAction);
    }

    private void HandleBuildingInput()
    {
        if (Input.IsActionJustPressed(BuildRotateClockwiseAction))
        {
            RotateCW?.Invoke();
        }

        if (Input.IsActionJustPressed(BuildRotateCounterClockwiseAction))
        {
            RotateCCW?.Invoke();
        }

        if (Input.IsActionJustPressed(BuildChangeSnapAction))
        {
            IsGridSnapMode = !IsGridSnapMode;
            OnChangeSnapMode?.Invoke(IsGridSnapMode);
        }
    }

    private void HandleUsageInput()
    {
        if (Input.IsActionJustPressed(UsePrimaryAction))
        {
            UsePrimary?.Invoke();
        }

        if (Input.IsActionJustPressed(UseSecondaryAction))
        {
            UseSecondary?.Invoke();
        }

        if (Input.IsActionJustPressed(UseIncrementAction))
        {
            UseIncrement?.Invoke();
        }

        if (Input.IsActionJustPressed(UseDecrementAction))
        {
            UseDecrement?.Invoke();
        }

        if (Input.IsActionJustPressed(InteractAction))
        {
            Interact?.Invoke();
        }
    }

    private void HandleJoypadRstickInput()
    {
        Vector2 joypadLookVector = new(Input.GetAxis(LookLeftAction, LookRightAction), Input.GetAxis(LookUpAction, LookDownAction));
        if (joypadLookVector.Length() > 0.1f)
        {
            // There is a proportion matter when it comes to this.
            // The default (raw) mouse sensitivity is 100, the default joypad sensitivity is 5.
            // in order to turn 100 into 5 in a proportional way, (5 * sens)/100
            // TOTEST: this needs further testing
            LookVector = joypadLookVector * (5 * (float)GameRegistries.Instance.SettingsData.LookSensitivity) / JoypadSensitivityDenominator;
            // This action name is slightly misleading but gets the job done.
            OnMouseMovement?.Invoke();
        }
    }

    #endregion

}
