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
			item.count = rem;
			UIManager.Alert("Inventory full!");
		}

		player.UpdateObserving();
	}

	public override string GetInfoText()
	{
		return $"{item.data.name} [{item.count}]";
	}

	private void OnCollisionEnter(Collision collision)
	{
		GameObject g = collision.gameObject;

		if (g.CompareTag("DroppedItem"))
		{
			DroppedItem script = g.GetComponent<DroppedItem>();

			if (preference <= script.preference && script.item.data == item.data)
			{
				script.item.count += item.count;
				script.preference += preference;
				Destroy(gameObject);
			}
		}
	}
}
