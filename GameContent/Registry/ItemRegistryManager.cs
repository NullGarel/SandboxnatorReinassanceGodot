using NullGarel.Sandboxnator.Registry;
using NullGarel.Util;
using NullGarel.Util.Log;

namespace NullGarel.Sandboxnator.Item;

/// <summary>
/// Registers item data from the game's content database.
/// </summary>
public partial class ItemRegistryManager : IRegistryManager
{
	public void Register()
	{
		foreach (ItemData itemData in GameRegistries.Instance.ContentDatabase.Items)
		{
			NcLogger.Log(
				$"Valid item resource is {itemData.ItemId}, registering...",
				NcLogger.LogType.Register
			);

			if (itemData is PlaceableItemData placingItemData)
			{
				NcLogger.Log(
					$"({itemData.ItemId}) is a placeable building, adding to building registry as well.",
					NcLogger.LogType.Register
				);

				GameRegistries.Instance.PlaceableRegistry.Register(
					itemData.ItemId,
					placingItemData.BuildingScene
				);
			}

			GameRegistries.Instance.ItemRegistry.Register(
				itemData.ItemId,
				itemData
			);
		}
	}
}