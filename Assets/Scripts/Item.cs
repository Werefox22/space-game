using System;

[Serializable]
public class Item
{
	public ItemSO Data { get; private set; }
	public int Count { get; private set; }

	public Item(ItemSO data, int count)
	{
		Data = data;
		Count = count;
	}

	/// <summary>
	/// Adds the specified amount to this item's count. If this would cause count to surpass the item's max stack size, it instead maxes out this item's stack and returns the difference.
	/// </summary>
	/// <param name="amount">The amount of items to add.</param>
	/// <returns>The amount of items which could not be added to this stack.</returns>
	public int Add(int amount)
	{
		if (Count + amount > Data.maxStackSize)
		{
			Count = Data.maxStackSize;
			return Count + amount - Data.maxStackSize;
		}
		else
		{
			Count += amount;
			return 0;
		}
	}

	/// <summary>
	/// Adds items to the target Item's count, taking from this Item's count as needed.
	/// </summary>
	/// <param name="item">The target Item, whose Count will increase.</param>
	/// <returns>This Item's Count after the operation.</returns>
	public int AddTo(Item item)
	{
		if (item.Data != Data)
		{
			throw new ArgumentException("Target item's Data does not match this Item's Data", "item");
		}

		Count = item.Add(Count);
		return Count;
	}

	/// <summary>
	/// Checks for missing data or less than 1 item.
	/// </summary>
	/// <returns>False if there is data and at least 1 item, true otherwise.</returns>
	public bool IsEmpty()
	{
		return Data == null || Count <= 0;
	}

	/// <summary>
	/// Clear's this Item's Data and Count.
	/// </summary>
	public void SetEmpty()
	{
		Data = null;
		Count = 0;
	}
}
