using System.Linq;
using Godot;
using NullGarel.Sandboxnator.Placeables;
using NullGarel.Util.ComponentSystem;

namespace NullGarel.Sandboxnator.Entity;

public partial class PlayerInteract : AbstractComponent<Player>
{
    [Export] private RayCast3D _interactionRaycast;
    private bool _isFacingInteractable;
    //used on UI to give visual feedback.
    public bool IsFacingInteractable => _isFacingInteractable;

    private PlayerInput _playerInput;

    public override void _Ready()
    {
        // this component should be authority of the server.
        SetMultiplayerAuthority(1);
        _playerInput = GetComponent<PlayerInput>();
        SetupInput();
    }

    private void SetupInput()
    {
        _playerInput.Interact += ClientInteract;
    }

    private void ClientInteract()
    {
        if (!_interactionRaycast.IsColliding()) return;
        RpcId(1, nameof(ServerBoundInteract));
    }

    public override void _Process(double delta)
    {
        _isFacingInteractable = false;
        if (!_interactionRaycast.IsColliding()) return;

        var hitObject = _interactionRaycast.GetCollider();
        if (hitObject is not Placeable hitPlaceable)
            return;

        _isFacingInteractable = hitPlaceable.HasInteractable;

    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerBoundInteract()
    {
        if (!Multiplayer.IsServer())
            return;

        var hitObject = _interactionRaycast.GetCollider();
        if (hitObject is not Placeable hitPlaceable)
            return;

        var interactable = hitPlaceable.componentHolder
            .GetChildren()
            .OfType<IInteractable>()
            .FirstOrDefault();

        if (interactable == null)
        {
            return;
        }

        interactable.Interact();
    }
}