using System;

[Serializable]
public class Item
{
	public ItemSO data;
	public int count;

	/// <summary>
	/// Checks for missing data or less than 1 item.
	/// </summary>
	/// <returns>False if there is data and at least 1 item, true otherwise.</returns>
	public bool IsEmpty()
	{
		return data == null || count <= 0;
	}
}
