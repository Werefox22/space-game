using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : Interactable
{
	public Item item;

	public override void Interact(PlayerInventory player)
	{
		int rem = player.GiveItems(item);

		if (rem <= 0)
		{
			Destroy(gameObject);
		}
		else
		{
			item.count = rem;
		}
	}

	public override string GetInfoText()
	{
		return $"{item.data.name} [{item.count}]";
	}
}
