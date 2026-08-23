using Godot;
using Godot.Collections;
using NullGarel.Util.GodotHelpers;
using NullGarel.Sandboxnator.Chat;
using NullGarel.Sandboxnator.Entity;
using NullGarel.Util.Log;
using NullGarel.Sandboxnator.Network;
namespace NullGarel.Sandboxnator.WorldAndScenes;

public partial class PlayerManager : Singleton<PlayerManager>
{
	[Export] private PackedScene _playerScene;
	[Export] private Vector2 _rangeOfRandomPos;

	public override void _Ready()
	{
		// this component should be authority of the server.
		SetMultiplayerAuthority(1);
	}

	public void SendHandShake()
	{
		if (Multiplayer.IsServer())
			return;

		Dictionary profileDict = DictPack.Pack(PlayerProfileManager.Instance.CurrentProfile);
		RpcId(1, nameof(ServerBoundHandshake), profileDict);
	}

	public void AddPlayer(long id = 1)
	{

		Node3D player = (Node3D)_playerScene.Instantiate();
		if (player == null)
		{
			NcLogger.Log("Failed to instantiate player.", NcLogger.LogType.Error);
			return;
		}
		player.SetMultiplayerAuthority((int)id);
		player.Name = id.ToString();

		//handling those stupid ass exceptions
		if (SandboxnatorMain.World == null)
		{
			NcLogger.Log("SandboxnatorMain.World is null!", NcLogger.LogType.Error);
			return;
		}
		else if (SandboxnatorMain.World.NetworkedEntities == null)
		{
			NcLogger.Log("SandboxnatorMain.World.networkedEntities is null!", NcLogger.LogType.Error);
			return;
		}
		else
		{
			SandboxnatorMain.World.NetworkedEntities.AddChild(player);
		}

		SandboxnatorMain.World.OnPlayerJoin?.Invoke(id);

		if (Multiplayer.IsServer())
		{
			Vector2 randPos = new(GD.Randi() % _rangeOfRandomPos.X, GD.Randi() % _rangeOfRandomPos.Y);
			Vector3 desiredPosition = new(randPos.X, 20, randPos.Y);
			if (Multiplayer.GetUniqueId() == id)
			{
				player.Position = desiredPosition;
				NcLogger.Log($"Server owned Player:{id} placed on XYZ {player.Position}");
			}
			else
			{
				//send a RPC to the player who connected to set their position
				RpcId(id, nameof(ClientBoundSetInitialPosition), desiredPosition, player.Name);
			}
		}
	}

	public void RemovePlayer(long id)
	{
		if (!Multiplayer.IsServer())
			return;

		World world = SandboxnatorMain.World;

		if (world == null)
		{
			NcLogger.Log(
				$"RemovePlayer({id}): SandboxnatorMain.World is null. Ignoring disconnect cleanup.",
				NcLogger.LogType.Warn);

			return;
		}

		Node networkedEntities = world.NetworkedEntities;

		if (networkedEntities == null ||
			!IsInstanceValid(networkedEntities))
		{
			NcLogger.Log(
				$"RemovePlayer({id}): networkedEntities is unavailable. Ignoring disconnect cleanup.",
				NcLogger.LogType.Warn);

			return;
		}

		//announce the departure
		PlayerProfileData pData = world.GetPlayerProfileDataByID(id);

		if (pData != null)
		{
			ChatManager.Instance?.BroadcastPlayerlessMessage($"[color={pData.PlayerColor.ToHtml()}]{pData.PlayerName}[/color] left the game :C");
		}
		else
		{
			ChatManager.Instance?.BroadcastPlayerlessMessage($"[color=red]{id}[/color] left the game, he left so fast we couldn't even find his name :C");

			NcLogger.Log($"RemovePlayer({id}): Player profile not found.",
				NcLogger.LogType.Warn);
		}

		Node player = networkedEntities.GetNodeOrNull(id.ToString());

		if (player == null || !IsInstanceValid(player))
		{
			NcLogger.Log(
				$"RemovePlayer({id}): Player node does not exist or has already been freed.",
				NcLogger.LogType.Warn);

			return;
		}

		DisableProcessingRecursive(player);

		if (player.IsQueuedForDeletion())
		{
			NcLogger.Log(
				$"RemovePlayer({id}): Player is already queued for deletion.",
				NcLogger.LogType.Warn);

			return;
		}

		NcLogger.Log(
			$"RemovePlayer({id}): Removing player node.");

		player.QueueFree();
	}

	/// <summary>
	/// Cleans networked entities on disconnection...........
	/// </summary>
	public void PrepareForDisconnect()
	{
		Node networkedEntities = SandboxnatorMain.World?.NetworkedEntities;

		if (networkedEntities == null || !IsInstanceValid(networkedEntities))
		{
			return;
		}

		foreach (Node child in networkedEntities.GetChildren())
		{
			if (!IsInstanceValid(child))
				continue;

			DisableProcessingRecursive(child);

			if (child.IsQueuedForDeletion())
				continue;

			child.QueueFree();
		}
	}

	/// <summary>
	/// Disables a node to avoid those absolutely fucking disgusting bloody shit errors that are pissing the hell out of me at this point... >:(((
	/// </summary>
	/// <param name="node">do I need to explain?</param>
	private void DisableProcessingRecursive(Node node)
	{
		node.SetProcess(false);
		node.SetPhysicsProcess(false);
		node.SetProcessInput(false);
		node.SetProcessUnhandledInput(false);
		node.SetProcessUnhandledKeyInput(false);

		foreach (Node child in node.GetChildren())
			DisableProcessingRecursive(child);
	}

	#region Handshake RPCS
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void ServerBoundHandshake(Dictionary profileDict)
	{
		if (!Multiplayer.IsServer()) return;

		int remoteId = Multiplayer.GetRemoteSenderId();
		PlayerProfileData profileData = DictPack.Unpack<PlayerProfileData>(profileDict);

		AddPlayer(remoteId);

		NcLogger.Log($"SERVER::Handshake received from {remoteId} ; {profileData.PlayerName}");
		ChatManager.Instance.BroadcastPlayerlessMessage($"[color={profileData.PlayerColor.ToHtml()}]{profileData.PlayerName}[/color] joined the game :3");

		RpcId(remoteId, nameof(ClientBoundHandshake));
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void ClientBoundHandshake()
	{
		if (Multiplayer.IsServer()) return;

		NcLogger.Log("Handshake acknowledged on client");
		NetworkManager.Instance.NotifyConnectionEstablished();
		SandboxnatorMain.Instance.ActivateWorld();
	}
	#endregion


	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	private void ClientBoundSetInitialPosition(Vector3 position, string playerId)
	{
		if (!Multiplayer.IsServer())
		{
			Node3D playerInstance = SandboxnatorMain.World.NetworkedEntities.GetNodeOrNull<Node3D>(playerId);
			if (playerInstance == null)
			{
				NcLogger.Log("Player instance is lagging behind, delaying position change", NcLogger.LogType.Warn);
				CallDeferred(nameof(ClientBoundSetInitialPosition), position, playerId);
				return;
			}
			playerInstance.Position = position;
			RpcId(1, nameof(ServerBoundPositionCheck), position, playerId);
		}
	}

	// Call from a client to run on server to check if player position is synchronized
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void ServerBoundPositionCheck(Vector3 position, string playerId)
	{
		NcLogger.Log($"Server placed the remote player of ID:{playerId} placed on XYZ {position} via RPC");
	}

	internal void OnPeerConnected(long id)
	{
		NcLogger.Log($"Peer connected, waiting for handshake, connection id is {id}");
	}
}
