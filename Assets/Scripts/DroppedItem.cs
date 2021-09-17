using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItem : Interactable
{
	public Item item;

	public override string GetInfoText()
	{
		return $"{item.data.name} [{item.count}]";
	}
}
