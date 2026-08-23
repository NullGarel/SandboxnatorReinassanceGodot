using Godot;
using NullGarel.Sandboxnator.Chat;
using NullGarel.Util.ComponentSystem;
namespace NullGarel.Sandboxnator.Entity;

public partial class PlayerChatHud : AbstractComponent<Player>
{

    [ExportCategory("Full chat menu")]
    [Export] private LineEdit _messageEdit;
    [Export] private RichTextLabel _messageBox;
    [ExportCategory("Notification")]
    [Export] private AudioStreamPlayer _notificationSound;
    [ExportCategory("Latest message")]
    [Export] private RichTextLabel _quickMessageBox;
    [Export] private Timer _messageFadeOutTimer;
    private string _latestMessage = "";
    //TODO: Tweakable in settings menu.
    private const double TimePerCharacterSeconds = 96d / 1000d;
    private const double MaxTimeCapSeconds = 120d;
    public string LatestMessage
    {
        get => _latestMessage;
        set
        {
            _latestMessage = value;
            _quickMessageBox.Text = _latestMessage;


            double messageDuration = _latestMessage.Length * TimePerCharacterSeconds;
            double clampedDuration = Mathf.Min(messageDuration, MaxTimeCapSeconds);
            _messageFadeOutTimer.WaitTime = clampedDuration;

            _messageFadeOutTimer.Start();
        }
    }

    private PlayerInput _playerInput;
    private PlayerHUD _playerHud;

    public override void _Ready()
    {
        _playerInput = GetComponent<PlayerInput>();
        _playerHud = GetComponent<PlayerHUD>();
        if (!IsMultiplayerAuthority()) return;

        _playerInput.OnShowChat += ShowChat;
        _playerInput.OnUiEscape += HideChat;
        ChatManager.Instance.OnMessageReceived += ReceiveMessage;
        _messageFadeOutTimer.Timeout += () =>
        {
            _quickMessageBox.Text = "";
        };
    }

    public override void _Process(double delta)
    {
        if (!IsMultiplayerAuthority()) return;
        _playerHud.IsChatOpen = _playerHud.chatRoot.Visible;
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (!IsMultiplayerAuthority()) return;
        if (inputEvent is InputEventKey eventKey)
        {
            if (eventKey.Pressed && eventKey.Keycode == Key.Enter)
            {
                SendMessage();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    private void ShowChat()
    {
        _playerHud.chatRoot.Visible = true;
        _messageEdit.FocusMode = Control.FocusModeEnum.All;
        _messageEdit.CallDeferred(Control.MethodName.GrabFocus);
    }

    private void HideChat()
    {
        _playerHud.chatRoot.Visible = false;
    }

    private void ReceiveMessage(ChatMessage message, PlayerProfileData senderData)
    {
        //-1: System notifications
        if (message.PlayerId != -1)
        {
            string computedText = $"[color={senderData.PlayerColor.ToHtml()}](@{senderData.PlayerName}) [/color] : {message.Content}\n";
            _messageBox.Text += computedText;

            //I personally thinks it's completely useless to make your own message you wrote yourself visible to yourself, idk, might be revisited.
            if (message.PlayerId != ComponentParent.componentHolder.EntityId)
                LatestMessage = computedText;
        }
        else
        {
            string computedText = $"[color=yellow][System]:[/color]{message.Content}\n";
            _messageBox.Text += computedText;
        }
        _notificationSound.Play();
    }

    private void SendMessage()
    {
        string msg = _messageEdit.Text;
        if (!string.IsNullOrEmpty(msg) && !string.IsNullOrWhiteSpace(_messageEdit.Text) && _playerHud.chatRoot.Visible)
        {
            ChatManager.Instance.RequestSendMessageToServer(msg);
            _messageEdit.Text = "";
        }
        _messageEdit.FocusMode = Control.FocusModeEnum.All;
        _messageEdit.CallDeferred(Control.MethodName.GrabFocus);
    }

    public void _on_send_btn_pressed()
    {
        SendMessage();
    }
}
