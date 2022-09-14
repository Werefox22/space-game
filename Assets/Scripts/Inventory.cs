using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	[Header("Data")]
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
	/// Gets the total amount of items in the inventory.
	/// </summary>
	/// <param name="item">Optional: A specific item to get the count of. If excluded, this function will return the count of all items instead.</param>
	/// <returns><c>int</c>: Amount of items in inventory.</returns>
	public int GetTotalItemCount(ItemSO item = null)
	{
		int total = 0;
		foreach (Item i in data)
		{
			if (i != null && (item == null || i.Data == item))
			{
				total += i.Count;
			}
		}

		return total;
	}

	/// <summary>
	/// Checks if the inventory has <paramref name="item"/> with a count within the specified range.
	/// </summary>
	/// <param name="item">The item to check for.</param>
	/// <param name="minimum">The minimum number of items required (inclusive).</param>
	/// <param name="maximum">The maximum number of items allowed (inclusive).</param>
	/// <returns>True if inventory has between <paramref name="minimum"/> (inclusive) and <paramref name="maximum"/> (inclusive) <paramref name="items"/>.</returns>
	public bool HasItem(ItemSO item, int minimum = 1, int maximum = int.MaxValue)
	{
		int total = GetTotalItemCount(item);

		return total >= minimum && total <= maximum;
	}

	/// <summary>
	/// Gives the specified number of an item to the player's inventory.
	/// </summary>
	/// <param name="itemToGive">The item to give.</param>
	/// <param name="amount">How much of <paramref name="itemToGive"/> to give.</param>
	/// <returns>How many items did NOT fit in the player's inventory.</returns>
	public int GiveItems(ItemSO itemToGive, int amount)
	{
		// check for existing stacks of this item
		for (int i = 0; i < inventory.Count; i++)
		{
			if (amount <= 0)
			{
				GameConsole.LogWarning("Somehow, we reduced amount to 0 without properly ending the loop. You should probably check that out.");
				break;
			}

			// skip empty and other items
			if (inventory[i] == null || inventory[i].IsEmpty() || inventory[i].data != itemToGive)
			{
				continue;
			}

			// passing this check means we're looking at a slot that matches our item

			// check how much room is left in the stack
			int roomLeft = inventory[i].data.maxStackSize - inventory[i].count;
			// if the item has enough room left
			if (roomLeft >= amount)
			{
				// add the items, update the inventory, return 0 since all items fit
				inventory[i].count += amount;
				InventoryUpdate();
				return 0;
			}
			else // the item did not have enough room left
			{
				// fill the stack to capacity
				inventory[i].count += roomLeft;
				amount -= roomLeft;
			}
		}

		// if we've reached this point, we've checked the entire inventory and there are no more stacks of this item with room in them
		// so now we have to find the first empty slot and fill it
		for (int i = 0; i < inventory.Count; i++)
		{
			// if we have found an empty slot
			if (inventory[i] == null || inventory[i].IsEmpty())
			{
				// can fit all items in one stack
				if (itemToGive.maxStackSize >= amount)
				{
					// fill the slot
					inventory[i] = new Item
					{
						Data = itemToGive,
						Count = amount
					};

					// end method
					InventoryUpdate();
					return 0;
				}
				else // can't fit all items in one stack
				{
					// max out the slot
					inventory[i] = new Item
					{
						Data = itemToGive,
						Count = itemToGive.maxStackSize
					};

					// get the remaining items and continue
					amount -= itemToGive.maxStackSize;
				}
			}
		}

		// if we reach this point, there wasn't enough room in the inventory for all the items
		// so we tell whatever called this how many items didn't make it, so it knows to keep that amount of items
		InventoryUpdate();
		return amount;
	}

	/// <summary>
	/// Gives the specified item to the player's inventory.
	/// </summary>
	/// <param name="item">The item to give.</param>
	/// <returns>How many items did NOT fit in the player's inventory.</returns>
	public int GiveItems(Item item)
	{
		return GiveItems(item.Data, item.Count);
	}

	/// <summary>
	/// Checks that the player has enough room for an item in their inventory.
	/// </summary>
	/// <param name="itemToGive">The item to give.</param>
	/// <param name="amount">How much of <paramref name="itemToGive"/> to give.</param>
	/// <returns>True if the player had enough room in their inventory.</returns>
	public bool HasSpace(ItemSO itemToGive, int amount)
	{
		for (int i = 0; i < inventory.Count; i++)
		{
			// if the slot is empty
			if (inventory[i] == null || inventory[i].IsEmpty())
			{
				// if the amount we're giving fits in one slot
				if (amount <= itemToGive.maxStackSize)
				{
					return true;
				}
				else
				{
					amount -= itemToGive.maxStackSize;
				}
			}
			else if (inventory[i].data != itemToGive) // if the slot is filled by another item
			{
				continue;
			}
			else // the slot is filled by the same item
			{
				int roomLeft = itemToGive.maxStackSize - inventory[i].count;
				// if there's room left in the stack
				if (roomLeft <= amount)
				{
					return true;
				}
				else
				{
					amount -= roomLeft;
				}
			}
		}

		// if we're here, we've checked the entire inventory and still have some amount of items left
		return false;
	}

	/// <summary>
	/// Removes the specified number of an item from the player's inventory.
	/// </summary>
	/// <param name="itemToRemove">The item to remove.</param>
	/// <param name="amount">How much of <paramref name="itemToRemove"/> to remove.</param>
	/// <returns>How much of <paramref name="amount"/> is left over, or -1 if it fails.</returns>
	public int RemoveItems(ItemSO itemToRemove, int amount)
	{
		if (itemToRemove == null || amount <= 0) return -1;

		int remaining = amount;
		for (int i = inventory.Count - 1; i >= 0; i--)
		{
			// skip empty and irrelevant items
			if (inventory[i] == null || inventory[i].IsEmpty() || inventory[i].data != itemToRemove)
			{
				continue;
			}

			// if correct item, check its count
			if (inventory[i].count >= remaining) // if this stack had enough
			{
				// remove items
				inventory[i].count -= remaining;
				remaining = 0;
			}
			else // this stack did not have enough
			{
				remaining -= inventory[i].count;
				inventory[i] = null;
			}

			// if no more items need to be removed
			if (remaining <= 0)
			{
				InventoryUpdate();
				return 0;
			}
		}

		// if we've reached the end of the loop, the player didn't have enough items
		InventoryUpdate();
		return remaining;
	}

}
