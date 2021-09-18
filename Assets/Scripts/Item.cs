using System;

[Serializable]
public class Item
{
	public ItemSO data;
	public int count;

	public bool IsEmpty()
	{
		return data == null || count <= 0;
	}
}
