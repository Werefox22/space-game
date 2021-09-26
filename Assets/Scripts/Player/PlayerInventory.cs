using FoxThorne;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
	[Header("Lists")]
	public List<Item> inventory = new List<Item>(60);
	public List<ItemSO> hotbar = new List<ItemSO>(10);

	[Header("Hotbar")]
	public int selectedItemSlot = -1;
	public ItemSO selectedItemData;

	[Header("Placement")]
	public GameObject previewObj;
	public PlacementValidationScript previewValidator;
	public Vector3 previewPos;
	public Vector3 previewRot;
	public Material previewMaterial;
	public Color validColor = Color.green;
	public Color invalidColor = Color.red;

	public StructureScript observingStructure;

	Vector3 previewRotInput = Vector3.zero;

	[Header("Interaction")]
	public Interactable observingInteractable;

	[Header("Raycast")]
	public LayerMask raycastLayerMask;
	public float raycastMaxDistance = 10;
	public bool raycastHit;
	public RaycastHit hitInfo;

	Collider lastHitCollider;


	[Header("References")]
	public Camera playerCam;
	public PlayerUI playerUI;
	public GameObject droppedItemPrefab;

	// bindings
	string interactBinding;

	void Start()
	{
		UIManager.player = this;

		UpdateBindingStrings();

		InventoryUpdate();

		SelectSlot(selectedItemSlot);
	}

	private void Update()
	{
		// raycast
		raycastHit = Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hitInfo, raycastMaxDistance, raycastLayerMask.value);

		if (raycastHit)
		{
			// if we're looking at a new object
			if (hitInfo.collider != lastHitCollider)
			{
				lastHitCollider = hitInfo.collider;

				StructureScript newStructure = null;
				Interactable newInteractable = null;
				string infoText = "";

				switch (lastHitCollider.tag)
				{
					case "Placeable": // A structure
						newStructure = GetObservingStructure();
						break;

					case "DroppedItem":
						newInteractable = lastHitCollider.GetComponentInParent<DroppedItem>();
						infoText = $"[{interactBinding}] Pick up {newInteractable.GetInfoText()}";
						break;
				}

				observingStructure = newStructure;
				observingInteractable = newInteractable;
				UIManager.SetInfoText(infoText);
			}
		}
		else // raycast did not hit
		{
			lastHitCollider = null;

			observingStructure = null;
			observingInteractable = null;
			UIManager.ClearInfoText();
		}
		
		// preview
		if (previewObj != null) // if previewing
		{
			if (previewValidator.IsValid)
			{
				previewMaterial.SetColor("_UnlitColor", validColor);
			}
			else
			{
				previewMaterial.SetColor("_UnlitColor", invalidColor);
			}

			previewRot += previewRotInput;

			// if the raycast hit something
			if (raycastHit)
			{
				// if we're looking at a structure
				if (observingStructure != null)
				{
					previewPos = observingStructure.GetSnappedPosition(hitInfo.collider.transform.position + hitInfo.normal);
					previewRot = observingStructure.transform.eulerAngles;
				}
				else // not looking at a structure
				{
					previewPos = hitInfo.point;
				}
			}
			else // raycast hit nothing
			{
				previewPos = playerCam.transform.position + playerCam.transform.forward * raycastMaxDistance;
			}

			previewObj.transform.SetPositionAndRotation(previewPos, Quaternion.Euler(previewRot));
		}
		else // not previewing
		{
			previewRot = Vector3.zero;
		}
	}

	/// <summary>
	/// Resets what the player is looking at so it will be updated on the next frame.
	/// </summary>
	public void UpdateObserving()
	{
		lastHitCollider = null;
	}

	public void UpdateBindingStrings()
	{
		interactBinding = PlayerSettings.GetBinding("Interact");
	}

	#region inventory
	public void InventoryUpdate()
	{
		for (int i = 0; i < inventory.Count; i++)
		{
			// if slot isn't empty but the item is missing or there are 0 item in that stack
			if (inventory[i] != null && (inventory[i].data == null || inventory[i].count <= 0))
			{

				inventory[i] = null;
			}
		}

		UIManager.InventoryUpdate();
	}

	public void SelectSlot(int index)
	{
		// if we're deselecting a slot (not selecting after having nothing selected)
		if (selectedItemSlot >= 0 && hotbar[selectedItemSlot] != null)
		{
			hotbar[selectedItemSlot].OnDeselect(this);

		}

		// if we're selecting a new slot (not pressing `, which would empty our hand)
		if (index >= 0)
		{
			selectedItemData = hotbar[index];

			if (selectedItemData != null)
			{
				selectedItemData.OnSelect(this);
			}
		}
		else // deselecting
		{
			selectedItemData = null;
		}

		selectedItemSlot = index;
		UIManager.SelectHotbarSlot(index);
	}

	public int GetTotalItemCount(ItemSO item)
	{
		int retval = 0;
		foreach (Item i in inventory)
		{
			if (i != null && i.data == item)
			{
				retval += i.count;
			}
		}

		return retval;
	}

	/// <summary>
	/// Gives the specified number of an item to the player's inventory.
	/// </summary>
	/// <param name="itemToGive">The item to give.</param>
	/// <param name="amount">How much of <paramref name="itemToGive"/> to give.</param>
	/// <returns>How many items did NOT fit in the player's inventory.</returns>
	public int GiveItems(ItemSO itemToGive, int amount)
	{
		// note: this is probably overcommented, but my brain isn't working and I need to walk myself through it so here we are

		// check for existing stacks of this item
		for (int i = 0; i < inventory.Count; i++)
		{
			if (amount <= 0)
			{
				GameConsole.LogWarning("Somehow, we reduced amount to 0 without properly ending the loop. You should probably check that out.");
				break;
			}

			// skip empty and other items
			if (inventory[i] == null || inventory[i].IsEmpty() || inventory[i].data != itemToGive)
			{
				continue;
			}

			// passing this check means we're looking at a slot that matches our item

			// check how much room is left in the stack
			int roomLeft = inventory[i].data.maxStackSize - inventory[i].count;
			// if the item has enough room left
			if (roomLeft >= amount)
			{
				// add the items, update the inventory, return 0 since all items fit
				inventory[i].count += amount;
				InventoryUpdate();
				return 0;
			}
			else // the item did not have enough room left
			{
				// fill the stack to capacity
				inventory[i].count += roomLeft;
				amount -= roomLeft;
			}
		}

		// if we've reached this point, we've checked the entire inventory and there are no more stacks of this item with room in them
		// so now we have to find the first empty slot and fill it
		for (int i = 0; i < inventory.Count; i++)
		{
			// if we have found an empty slot
			if (inventory[i] == null || inventory[i].IsEmpty())
			{
				// can fit all items in one stack
				if (itemToGive.maxStackSize >= amount)
				{
					// fill the slot
					inventory[i] = new Item
					{
						data = itemToGive,
						count = amount
					};

					// end method
					InventoryUpdate();
					return 0;
				}
				else // can't fit all items in one stack
				{
					// max out the slot
					inventory[i] = new Item
					{
						data = itemToGive,
						count = itemToGive.maxStackSize
					};

					// get the remaining items and continue
					amount -= itemToGive.maxStackSize;
				}
			}
		}

		// if we reach this point, there wasn't enough room in the inventory for all the items
		// so we tell whatever called this how many items didn't make it, so it knows to keep that amount of items
		InventoryUpdate();
		return amount;
	}

	/// <summary>
	/// Gives the specified item to the player's inventory.
	/// </summary>
	/// <param name="item">The item to give.</param>
	/// <returns>How many items did NOT fit in the player's inventory.</returns>
	public int GiveItems(Item item)
	{
		return GiveItems(item.data, item.count);
	}

	/// <summary>
	/// Checks that the player has enough room for an item in their inventory.
	/// </summary>
	/// <param name="itemToGive">The item to give.</param>
	/// <param name="amount">How much of <paramref name="itemToGive"/> to give.</param>
	/// <returns>True if the player had enough room in their inventory.</returns>
	public bool HasSpace(ItemSO itemToGive, int amount)
	{
		for (int i = 0; i < inventory.Count; i++)
		{
			// if the slot is empty
			if (inventory[i] == null || inventory[i].IsEmpty())
			{
				// if the amount we're giving fits in one slot
				if (amount <= itemToGive.maxStackSize)
				{
					return true;
				}
				else
				{
					amount -= itemToGive.maxStackSize;
				}
			}
			else if (inventory[i].data != itemToGive) // if the slot is filled by another item
			{
				continue;
			}
			else // the slot is filled by the same item
			{
				int roomLeft = itemToGive.maxStackSize - inventory[i].count;
				// if there's room left in the stack
				if (roomLeft <= amount)
				{
					return true;
				}
				else
				{
					amount -= roomLeft;
				}
			}
		}

		// if we're here, we've checked the entire inventory and still have some amount of items left
		return false;
	}

	/// <summary>
	/// Removes the specified number of an item from the player's inventory.
	/// </summary>
	/// <param name="itemToRemove">The item to remove.</param>
	/// <param name="amount">How much of <paramref name="itemToRemove"/> to remove.</param>
	/// <returns>How much of <paramref name="amount"/> is left over, or -1 if it fails.</returns>
	public int RemoveItems(ItemSO itemToRemove, int amount)
	{
		if (itemToRemove == null || amount <= 0) return -1;

		int remaining = amount;
		for (int i = inventory.Count - 1; i >= 0; i--)
		{
			// skip empty and irrelevant items
			if (inventory[i] == null || inventory[i].IsEmpty() || inventory[i].data != itemToRemove)
			{
				continue;
			}

			// if correct item, check its count
			if (inventory[i].count >= remaining) // if this stack had enough
			{
				// remove items
				inventory[i].count -= remaining;
				remaining = 0;
			}
			else // this stack did not have enough
			{
				remaining -= inventory[i].count;
				inventory[i] = null;
			}

			// if no more items need to be removed
			if (remaining <= 0)
			{
				InventoryUpdate();
				return 0;
			}
		}

		// if we've reached the end of the loop, the player didn't have enough items
		InventoryUpdate();
		return remaining;
	}

	/// <summary>
	/// Checks if player has at least the specified number of items in their inventory.
	/// </summary>
	/// <param name="item">The item to check for.</param>
	/// <param name="amount">The minimum number of items required.</param>
	/// <returns>True if player has at least <paramref name="amount"/> of <paramref name="item"/>.</returns>
	public bool HasItem(ItemSO item, int amount)
	{
		int total = 0;

		foreach (Item i in inventory)
		{
			// check item
			if (i == null || i.data != item)
			{
				continue;
			}
			else // if it matches
			{
				total += i.count;
			}

			if (total >= amount)
			{
				return true;
			}
		}

		return false;
	}

	public void DropItem(ItemSO item, int amount)
	{
		// get the number of items the player had in their inventory
		int r = amount - RemoveItems(item, amount);

		if (r > 0)
		{
			SpawnDroppedItem(item, r);
		}
	}

	public void DropItem(int index, int amount)
	{
		if (inventory[index] !=  null && !inventory[index].IsEmpty())
		{
			// if we're trying to drop more item than this slot has
			if (amount > inventory[index].count)
			{
				// only drop what's there
				amount = inventory[index].count;
			}

			SpawnDroppedItem(inventory[index].data, amount);
			inventory[index].count -= amount;
			InventoryUpdate();
		}
	}

	void SpawnDroppedItem(Item item)
	{
		GameObject go = Instantiate(droppedItemPrefab, playerCam.transform.position + playerCam.transform.forward, Quaternion.identity);
		DroppedItem script = go.GetComponent<DroppedItem>();

		script.item = item;
	}

	void SpawnDroppedItem(ItemSO data, int count)
	{
		Item i = new Item
		{
			data = data,
			count = count
		};

		SpawnDroppedItem(i);
	}
	#endregion

	#region placement
	public void CreatePreview(PlaceableItemSO item)
	{
		// if preview already exists, destroy it
		if (previewObj != null)
		{
			DestroyPreview();
		}

		previewObj = Instantiate(item.prefab, previewPos, Quaternion.Euler(previewRot));
		previewObj.name = "Preview of " + item.name;
		Utility.SetLayerRecursively(previewObj, LayerMask.NameToLayer("PlacementPreview"));

		// set up validation
		previewValidator = previewObj.AddComponent<PlacementValidationScript>();
		Rigidbody rb = previewObj.AddComponent<Rigidbody>();
		rb.isKinematic = false;

		// modify colliders
		Placeable p = previewObj.GetComponent<Placeable>();
		p.collidersRoot.transform.localScale *= 0.99f;
		foreach (Collider c in p.collidersRoot.GetComponentsInChildren<Collider>())
		{
			c.isTrigger = true;
		}
		

		// set preview mat
		foreach (Renderer r in previewObj.GetComponentsInChildren<Renderer>())
		{
			r.material = previewMaterial;
			r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		}
	}

	StructureScript GetObservingStructure()
	{
		if (raycastHit)
		{
			return hitInfo.collider.GetComponentInParent<StructureScript>();
		}
		else
		{
			return null;
		}
	}

	public StructureScript CreateStructureAtPreview()
	{
		GameObject go = new GameObject("Structure");
		go.transform.position = previewPos;
		go.transform.rotation = Quaternion.Euler(previewRot);

		StructureScript ss = go.AddComponent<StructureScript>();
		if (previewValidator.IsShip)
		{
			go.AddComponent<Rigidbody>();
			ss.isShip = true;
		}

		return ss;
	}

	public void DestroyPreview()
	{
		Destroy(previewObj);
	}
	#endregion

	#region input methods
	public void OnSelectNum(InputValue value)
	{
		int num = (int)value.Get<float>();
		SelectSlot(num - 1);
	}

	public void OnSelectScroll(InputValue value)
	{
		float input = value.Get<float>();

		if (input == 0) return;

		int num = selectedItemSlot;

		if (input > 0)
		{
			num--;
			if (num < 0)
			{
				num = hotbar.Count - 1;
			}
		}
		else
		{
			num++;
			if (num >= hotbar.Count)
			{
				num = 0;
			}
		}

		SelectSlot(num);
	}

	// left click
	public void OnPrimaryAction()
	{
		// must have a slot selected, an item in that slot, and at least 1 of those in your inventory
		if (selectedItemSlot >= 0 && hotbar[selectedItemSlot] != null && HasItem(hotbar[selectedItemSlot], 1))
		{
			if (hotbar[selectedItemSlot].PrimaryAction(this))
			{
				RemoveItems(hotbar[selectedItemSlot], 1);
			}
		}
	}

	public void OnDrop()
	{
		if (selectedItemData != null)
		{
			int amount = 1;

			if (playerUI.IsShiftHeld)
				amount = selectedItemData.maxStackSize;

			DropItem(selectedItemData, amount);
		}
	}

	public void OnInteract()
	{
		if (observingInteractable != null)
		{
			observingInteractable.Interact(this);
		}
	}

	public void OnRotatePitch(InputValue value)
	{
		previewRotInput.x = value.Get<float>();
	}

	public void OnRotateYaw(InputValue value)
	{
		previewRotInput.y = value.Get<float>();
	}

	public void OnRotateRoll(InputValue value)
	{
		previewRotInput.z = value.Get<float>();
	}
	#endregion
}
