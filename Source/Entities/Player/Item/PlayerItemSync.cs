using System;
using Godot;
using Godot.Collections;
using NullGarel.Sandboxnator.Item;
using NullGarel.Util.ComponentSystem;

namespace NullGarel.Sandboxnator.Entity;

/// <summary>
/// Component responsible for item synchronization and inventory logic
/// </summary>
public partial class PlayerItemSync : AbstractComponent<Player>
{
    [Export]
    private Inventory _inventory;
    private int _inventoryIndex;
    public event Action<string> OnItemEquipped;
    private string _currentItemId = string.Empty;
    public ItemStack CurrentItemStack => _inventory.GetSlot(_inventoryIndex);
    public int CurrentInventoryIndex => _inventoryIndex;

    [Export]
    public string CurrentItemId
    {
        get => _currentItemId;
        set
        {
            if (_currentItemId == value)
                return;

            _currentItemId = value;
            OnItemEquipped?.Invoke(_currentItemId);
        }
    }

    private Dictionary _activeItemState;

    public Dictionary ActiveItemState
    {
        get => _activeItemState;
        set
        {
            if (_activeItemState == value)
                return;

            _activeItemState = value;
        }
    }

    public override void _Ready()
    {
        SetMultiplayerAuthority(1);

        if (!string.IsNullOrEmpty(_currentItemId))
        {
            OnItemEquipped?.Invoke(_currentItemId);
        }
        else if (Multiplayer.IsServer())
        {
            SelectFirstAvailableItem();
        }

        if (Multiplayer.IsServer())
        {
            Multiplayer.PeerConnected += OnPeerConnected;
        }
    }

    private void SelectFirstAvailableItem()
    {
        for (int i = 0; i < _inventory.Count; i++)
        {
            ItemStack stack = _inventory.GetSlot(i);

            if (stack == null || stack.IsEmpty)
                continue;

            _inventoryIndex = i;
            CurrentItemId = stack.ItemId;
            return;
        }

        CurrentItemId = string.Empty;
    }

    private void OnPeerConnected(long id)
    {
        BroadcastItemState(ActiveItemState);
    }

    public void RequestCycleItem(int increment)
    {
        if (increment == 0)
            return;

        RpcId(1, nameof(ServerBoundRequestCycleItem), increment);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerBoundRequestCycleItem(int increment)
    {
        if (!Multiplayer.IsServer())
            return;

        if (increment == 0)
            return;

        int nextIndex = FindNextOccupiedSlot(_inventoryIndex, increment);

        if (nextIndex < 0)
            return;

        _inventoryIndex = nextIndex;

        ItemStack stack = _inventory.GetSlot(_inventoryIndex);

        if (stack == null || stack.IsEmpty)
            return;

        CurrentItemId = stack.ItemId;
        BroadcastItemState(stack.StackData);

        Rpc(nameof(ClientBoundConfirmItemChange), CurrentItemId);
    }

    private int FindNextOccupiedSlot(int currentIndex, int direction)
    {
        int step = Math.Sign(direction);

        if (step == 0)
            return -1;

        int index = currentIndex;

        for (int i = 0; i < _inventory.Count; i++)
        {
            index = WrapIndex(index + step, _inventory.Count);
            ItemStack stack = null;
            try
            {
                stack = _inventory.GetSlot(index);
            }
            catch
            {
                // do nowt xd
            }

            if (stack != null && !stack.IsEmpty)
                return index;
        }

        return -1;
    }

    private static int WrapIndex(int index, int count)
    {
        return ((index % count) + count) % count;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void ClientBoundConfirmItemChange(string itemId)
    {
        CurrentItemId = itemId;
    }

    /// <summary>
    /// broadcast state changes to clients.
    /// </summary>
    public void BroadcastItemState(Dictionary stateData)
    {
        if (!Multiplayer.IsServer()) return;

        ActiveItemState = stateData;
        Rpc(nameof(ClientBoundSyncItemState), stateData);
    }

    /// <summary>
    /// causes the data to be "commited" to the inventory stack.
    /// </summary>
    public void PersistItemState(Dictionary stateData)
    {
        if (!Multiplayer.IsServer()) return;

        var stack = CurrentItemStack;
        if (stack == null) return;

        foreach (var (key, value) in stateData)
            stack.StackData[key] = value;
    }

    /// <summary>
    /// sync the data to the inventory stack and then sync ephemerally to other players
    /// </summary>
    /// <param name="stateData"></param>
    public void PersistAndBroadcast(Dictionary stateData)
    {
        PersistItemState(stateData);
        BroadcastItemState(stateData);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void ClientBoundSyncItemState(Dictionary stateData)
    {
        ActiveItemState = stateData;
        GetComponent<PlayerItemUse>().Item?.ReceiveItemState(ActiveItemState);
    }
}