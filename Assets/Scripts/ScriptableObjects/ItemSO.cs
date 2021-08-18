using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemSO : ScriptableObject
{
	public string itemName;
	public Sprite itemIcon;

	public enum HudHelper
	{
		None = -1,
		Placement = 0
	}

	/// <summary>
	/// Activates the item's primary action.
	/// </summary>
	/// <returns>True if an item should be consumed.</returns>
	public virtual bool PrimaryAction(PlayerInventory player)
	{
		return false;
	}

	/// <summary>
	/// Activates the item's secondary action.
	/// </summary>
	/// <returns>True if an item should be consumed.</returns>
	public virtual bool SecondaryAction(PlayerInventory player)
	{
		return false;
	}

	public virtual void OnSelect(PlayerInventory player)
	{

	}

	public virtual void OnDeselect(PlayerInventory player)
	{

	}

	/// <summary>
	/// Tells the UI which, if any, hud helper to show on the screen when an item is selected.
	/// </summary>
	/// <returns>The helper to display.</returns>
	public virtual HudHelper GetHudHelper()
	{
		return HudHelper.None;
	}
}
