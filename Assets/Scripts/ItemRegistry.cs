using FoxThorne;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemRegistry : MonoBehaviour
{
	public List<ItemSO> items = new List<ItemSO>();

	public static Dictionary<string, ItemSO> allItemsByName = new Dictionary<string, ItemSO>();
	public static Dictionary<string, PlaceableItemSO> blocksByName = new Dictionary<string, PlaceableItemSO>();
	public static Dictionary<string, MaterialItemSO> materialsByName = new Dictionary<string, MaterialItemSO>();

	private void Start()
	{
		// set up dictionaries and check for duplicate values
		GameConsole.Log("Setting up items");
		for (int i = 0; i < items.Count; i++)
		{
			// check against previous items in list
			for (int j = 0; j < i; j++)
			{
				if (items[j].itemName == items[i].itemName)
				{
					GameConsole.LogError($"Items number {i} and {j} have the same name!");
				}
			}

			// add to dictionaries
			allItemsByName.Add(items[i].name, items[i]);

			Type type = items[i].GetType();

			// would prefer to use a switch/case here, but it won't let me because "a constant value is expected"
			// I don't know enough about types and how they work to fix it but a else if chain does work so that's what I'm doing
			if (type == typeof(PlaceableItemSO))
			{
				blocksByName.Add(items[i].itemName, (PlaceableItemSO)items[i]);
			}
			else if (type == typeof(MaterialItemSO))
			{
				materialsByName.Add(items[i].itemName, (MaterialItemSO)items[i]);
			}
		}
		GameConsole.Log("Finished setting up items");
		GameConsole.Log($"Item counts:\nAll items: {allItemsByName.Count}\nBlocks: {blocksByName.Count}\nMaterials: {materialsByName.Count}\n");
	}
}