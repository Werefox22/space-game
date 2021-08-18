using UnityEngine.EventSystems;

public class InvHotbarSlot : ItemSlot
{
	public override void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Right) return;

		UIManager.SetHotbarSlot(index, null);
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		// this stops the hotbar slots from acting as normal inventory slots
	}
}
