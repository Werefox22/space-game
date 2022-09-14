using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	public List<Item> data = new List<Item>(60);
	public void InventoryUpdate()
	{
		for (int i = 0; i < data.Count; i++)
		{
			// if slot isn't empty but the item is missing or there are 0 item in that stack
			if (data[i] != null && data[i].IsEmpty())
			{
				data[i] = null;
			}
		}

		UIManager.InventoryUpdate();
	}


	/// <summary>
	/// Checks if the inventory has at least the specified number of items.
	/// </summary>
	/// <param name="item">The item to check for.</param>
	/// <param name="amount">The minimum number of items required.</param>
	/// <returns>True if inventory has at least <paramref name="amount"/> of <paramref name="item"/>.</returns>
	public bool HasItem(ItemSO item, int amount)
	{
		int total = 0;

		foreach (Item i in data)
		{
			// check item
			if (i == null || i.data != item)
			{
				continue;
			}
			else // if it matches
			{
				total += i.count;
			}

			if (total >= amount)
			{
				return true;
			}
		}

		return false;
	}

}
