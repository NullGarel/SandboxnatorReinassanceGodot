using Godot;
using NullGarel.Sandboxnator.Network;
using NullGarel.Sandboxnator.UI;
using NullGarel.Util.ComponentSystem;
namespace NullGarel.Sandboxnator.Entity;

/// <summary>
/// Centralized component of Graphical User Interface to a player
/// </summary>
[GodotClassName(nameof(PlayerHUD))]
public partial class PlayerHUD : AbstractComponent<Player>, IUiSignalLoader
{
    [ExportCategory("Main Controls")]
    [Export] public Control chatRoot;
    [Export] private Control _hotBar;
    [ExportCategory("CrossHair")]
    [Export] private TextureRect _crossHair;
    [Export] private Texture2D _defaultCrosshair;
    [Export] private Texture2D _interactionCrosshair;
    [ExportCategory("ESC Menu")]
    [Export] private Control _escMenu;
    [Export] private Button _leaveGameBtn;
    [Export] private Button _settingsBtn;

    [ExportCategory("Grid vs snapper information")]
    [Export] private TextureRect _alignmentInformationIcon;
    [Export] private Texture2D _gridIcon;
    [Export] private Texture2D _snapperIcon;

    public bool IsChatOpen { get; set; }
    public bool IsHudBeingUsed { get; private set; }

    private PlayerInput _playerInput;
    private PlayerInteract _playerInteract;

    public override void _Ready()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerInteract = GetComponent<PlayerInteract>();

        if (!IsMultiplayerAuthority())
            return;

        ConnectUISignals();

        _playerInput.OnUiEscape += () =>
        {
            if (IsChatOpen) return;
            //force mouse cursor to show up if it's not there
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
                Input.MouseMode = Input.MouseModeEnum.Visible;

            _escMenu.Visible = !_escMenu.Visible;
        };

        _playerInput.OnChangeSnapMode += isGrid =>
        {
            _alignmentInformationIcon.Texture = isGrid ? _gridIcon : _snapperIcon;
        };
    }

    public override void _Process(double delta)
    {
        if (!IsMultiplayerAuthority()) return;

        IsHudBeingUsed = IsChatOpen || _escMenu.Visible;
        _crossHair.Texture = _playerInteract.IsFacingInteractable ? _interactionCrosshair : _defaultCrosshair;
    }

    public override void _ExitTree()
    {
        if(!NetworkManager.Instance.HasMultiplayerPeer()) return;
        if (!IsInstanceValid(this)) return;
        if (!IsMultiplayerAuthority()) return;
        DisconnectUISignals();
    }

    public void ConnectUISignals()
    {
        _settingsBtn.Pressed += SandboxnatorMain.Instance.ToggleSettingsMenu;
        _leaveGameBtn.Pressed += SandboxnatorMain.Instance.LeaveWorld;
    }

    public void DisconnectUISignals()
    {
        _settingsBtn.Pressed -= SandboxnatorMain.Instance.ToggleSettingsMenu;
        _leaveGameBtn.Pressed -= SandboxnatorMain.Instance.LeaveWorld;
    }
}
