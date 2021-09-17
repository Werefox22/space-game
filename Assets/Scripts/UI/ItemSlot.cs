using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : Selectable
{
	[Header("Item")]
	public ItemSO item;
	public int count;

	[Header("Inventory")]
	public int index;

	[Header("References")]
	public Image itemImage;
	public Text countText;

	public void SetItem(Item newItem)
	{
		item = newItem.data;
		count = newItem.count;

		itemImage.sprite = item.itemIcon;
		countText.text = count.ToString();

		itemImage.enabled = true;
		countText.enabled = true;
	}

	public void ClearItem()
	{
		item = null;
		count = 0;

		itemImage.enabled = false;
		countText.enabled = false;
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left || UIManager.grabbedItemSlot != null) return;

		UIManager.hideHoverText = true;
		UIManager.grabbedItemSlot = this;
		itemImage.transform.SetParent(UIManager.cursorAnchor.transform, false);
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left) return;

		// if moving to a different slot
		if (UIManager.hoveringItemSlot != null)
		{
			// if to hotbar
			if (UIManager.hoveredSlotIsHotbar)
			{
				UIManager.SetHotbarSlot(UIManager.hoveringItemSlot.index, item);
			}
			else // if to another inventory slot
			{
				UIManager.MoveItem(index, UIManager.hoveringItemSlot.index);
			}
		}

		// unset this as grabbed
		UIManager.hideHoverText = false;
		UIManager.grabbedItemSlot = null;

		itemImage.transform.SetParent(transform, false);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);

		UIManager.hoveringItemSlot = this;
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);

		if (UIManager.hoveringItemSlot == this)
		{
			UIManager.hoveringItemSlot = null;
		}
	}
}
