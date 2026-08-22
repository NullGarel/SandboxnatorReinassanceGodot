using Godot;
using Godot.Collections;
using NullGarel.Sandboxnator.Placeables;
using System;
using System.Collections.Generic;
using NullGarel.Sandboxnator.Entity;
using NullGarel.Sandboxnator.Registry;
using NullGarel.Util.Log;
using NullGarel.Sandboxnator.Item;
using NullGarel.Util.GodotHelpers;
namespace NullGarel.Sandboxnator.WorldAndScenes;

/// <summary>
/// Class that holds the world scene data
/// </summary>
public partial class World : Node3D
{
	public Action<long> OnPlayerJoin;
	public List<Snapper> snappers = [];

	[Export] private Node3D _networkedEntities;
	public Node3D NetworkedEntities => _networkedEntities;

	[Export] public MultiplayerSpawner PlaceableSpawner { get; private set; }
	[Export] public MultiplayerSpawner PlayerSpawner { get; private set; }

	private readonly HashSet<string> addedPlaceableScenes = [];

	public override void _EnterTree()
	{
		AddPlaceableScenesToSpawnList();
		PlaceableSpawner.SpawnFunction = new Callable(this, nameof(SpawnPlaceable));
	}

	private void AddPlaceableScenesToSpawnList()
	{
		//commit building items to the auto spawn list
		foreach (PackedScene placeableScene in GameRegistries.Instance.PlaceableRegistry.GetAllValues())
		{
			if (placeableScene == null)
			{
				NcLogger.Log("Found null buildingScene!");
				continue;
			}
			string resPath = placeableScene.ResourcePath;
			if (addedPlaceableScenes.Add(resPath))
			{
				PlaceableSpawner.AddSpawnableScene(resPath);
			}
		}
	}

	private Node SpawnPlaceable(Variant data)
	{
		var spawnData = DictPack.Deserialize<PlaceableSpawnData>((Dictionary)data);

		PackedScene scene = GameRegistries.Instance.PlaceableRegistry.Get(spawnData.ItemId);
		Placeable placeable = (Placeable)scene.Instantiate();

		placeable.ItemData = (PlaceableItemData)GameRegistries.Instance.ItemRegistry.Get(spawnData.ItemId);
		placeable.Position = spawnData.Position;
		placeable.Rotation = spawnData.Rotation;
		placeable.Name = Guid.NewGuid().GetHashCode().ToString();

		return placeable;
	}


	public Vector3 GetNearestSnapper(Vector3 referential, float maxRange)
	{
		foreach (Snapper snapper in snappers)
		{
			if ((referential.DistanceTo(snapper.GlobalPosition) <= maxRange) && !snapper.InsideBody)
			{
				return snapper.GlobalPosition;
			}
		}
		return referential;
	}

	/// <summary>
	/// Expected and designed to run on the server for now
	/// </summary>
	/// <returns>An array of the current players.</returns>
	public Array<Player> GetPlayers()
	{
		Array<Player> players = [];
		foreach (Node e in NetworkedEntities.GetChildren())
		{
			if (e is Player player)
			{
				players.Add(player);
			}
		}
		return players;
	}

	public Player GetPlayerById(long id)
	{
		foreach (Player player in GetPlayers())
		{
			if (player.componentHolder.entityId == id)
			{
				return player;
			}
		}
		return null;
	}

	public PlayerProfileData GetPlayerProfileDataByID(long id)
	{
		return GetPlayerById(id)?.ProfileData;
	}
}
