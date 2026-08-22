using Godot;
using NullGarel.Util.ComponentSystem;

namespace NullGarel.Sandboxnator.Placeables;

public partial class Door : AbstractComponent<Node3D>, IInteractable
{

    //WARNING: door specific.
    private bool _isOpen = false;
    [Export]
    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            _isOpen = value;
            UpdateVisual();
        }
    }

    private const string DoorOpenState = "DoorOpen";
    private const string DoorClosedState = "DoorClosed";

    [Export] private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        SetMultiplayerAuthority(1);
        UpdateVisual();
    }

    public void Interact()
    {
        if (!Multiplayer.IsServer())
            return;

        _isOpen = !_isOpen;
        UpdateVisual();

    }

    private void UpdateVisual()
    {
        if (!IsInstanceValid(_animationPlayer))
            return;
        string animation = _isOpen ? DoorOpenState : DoorClosedState;
        _animationPlayer.Play(animation);
    }

}
