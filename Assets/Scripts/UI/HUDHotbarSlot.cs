using UnityEngine;
using UnityEngine.UI;

public class HUDHotbarSlot : MonoBehaviour
{
	[Header("Visual")]
	public Color selectedColor = Color.white;
	public Color deselectedColor = Color.clear;

	[Header("References")]
	public InvHotbarSlot linkedSlot;
	public Image background;
	public Image itemImage;
	public Text countText;

	private void Start()
	{
		Deselect();
	}

	public void UpdateItem()
	{
		if (linkedSlot.item != null)
		{
			itemImage.sprite = linkedSlot.item.itemIcon;
			countText.text = linkedSlot.count.ToString();

			itemImage.enabled = true;
			countText.enabled = true;

		}
		else // is empty slot
		{
			itemImage.enabled = false;
			countText.enabled = false;
		}
	}

	public void Select()
	{
		background.color = selectedColor;
	}

	public void Deselect()
	{
		background.color = deselectedColor;
	}
}
