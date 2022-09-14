using UnityEngine;

public class DroppedItem : Interactable
{
	public Item item;
	/// <summary>
	/// A randomized number used to determine which stack stays when combining items.
	/// </summary>
	public float preference = 0;

	private void Start()
	{
		preference = Random.Range(0, (float)1);
	}

	public override void Interact(PlayerInventory player)
	{
		int rem = player.GiveItems(item);

		if (rem <= 0)
		{
			Destroy(gameObject);
		}
		else
		{
			item.Count = rem;
			UIManager.Alert("Inventory full!");
		}

		player.UpdateObserving();
	}

	public override string GetInfoText()
	{
		return $"{item.Data.name} [{item.Count}]";
	}

	private void OnCollisionEnter(Collision collision)
	{
		GameObject g = collision.gameObject;

		if (g.CompareTag("DroppedItem"))
		{
			DroppedItem script = g.GetComponent<DroppedItem>();

			if (preference <= script.preference && script.item.Data == item.Data)
			{
				script.item.Count += item.Count;
				script.preference += preference;
				Destroy(gameObject);
			}
		}
	}
}
