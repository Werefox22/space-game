﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FoxThorne;

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
	public Vector3 previewPos;
	public Quaternion previewRot;
	public Material previewValidMaterial;
	public Material previewInvalidMaterial;

	public StructureScript observingStructure;

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

	void Start()
	{
		UIManager.player = this;

		InventoryUpdate();

		SelectSlot(selectedItemSlot);
	}

	private void Update()
	{
		// raycast
		raycastHit = Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hitInfo, raycastMaxDistance, raycastLayerMask.value);

		// if we're looking at a new object
		if (raycastHit && hitInfo.collider != lastHitCollider)
		{
			lastHitCollider = hitInfo.collider;

			bool clearStructure = true;

			switch (lastHitCollider.tag)
			{
				case "Placeable": // A structure
					observingStructure = GetObservingStructure();
					clearStructure = false;
					break;

				case "DroppedItem":
					observingInteractable = lastHitCollider.GetComponentInParent<DroppedItem>();
					UIManager.SetInfoText(observingInteractable.GetInfoText());
					break;

					
			}

			if (clearStructure)
			{
				observingStructure = null;
			}
			
		}
		
		// preview
		if (previewObj != null) // if previewing
		{
			// if the raycast hit something
			if (raycastHit)
			{
				// if we're looking at a structure
				if (observingStructure != null)
				{
					previewPos = observingStructure.GetSnappedPosition(hitInfo.point + hitInfo.normal / 2);
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

			previewObj.transform.position = previewPos;
		}
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
			if (inventory[i] == null || inventory[i].data != itemToGive)
			{
				continue;
			}

			// passing this check means we're looking at a slot that matches our item

			// check how much room is left in the stack
			int roomLeft = inventory[i].data.maxStackSize - inventory[i].count;
			// if the item has enough room left
			if (roomLeft <= amount)
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
			if (inventory[i] == null)
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
		}

		// if we reach this point, there wasn't enough room in the inventory for all the items
		// so we tell whatever called this how many items didn't make it, so it knows to keep that amount of items
		InventoryUpdate();
		return amount;
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
			if (inventory[i] == null)
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
	/// <returns>True if the player had at least <paramref name="amount"/> of <paramref name="itemToRemove"/> in their inventory.</returns>
	public bool RemoveItems(ItemSO itemToRemove, int amount)
	{
		int remaining = amount;
		for (int i = inventory.Count - 1; i >= 0; i--)
		{
			// skip empty and irrelevant items
			if (inventory[i] == null || inventory[i].data != itemToRemove)
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
				return true;
			}
		}

		// if we've reached the end of the loop, the player didn't have enough items
		InventoryUpdate();
		return false;
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
	#endregion

	#region placement
	public void CreatePreview(PlaceableItemSO item)
	{
		// if preview already exists, destroy it
		if (previewObj != null)
		{
			DestroyPreview();
		}

		previewObj = Instantiate(item.prefab, previewPos, previewRot);
		previewObj.name = "Preview of " + item.name;
		Utility.SetLayerRecursively(previewObj, LayerMask.NameToLayer("PlacementPreview"));

		// disable components
		foreach (Collider c in previewObj.GetComponentsInChildren<Collider>())
		{
			c.enabled = false;
		}

		// set preview mat
		foreach (Renderer r in previewObj.GetComponentsInChildren<Renderer>())
		{
			r.material = previewValidMaterial;
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
		go.transform.rotation = previewRot;

		StructureScript ss = go.AddComponent<StructureScript>();
		// tell structure whether it's a ship or not

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
		num--;
		SelectSlot(num);
	}

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

	public void OnRotatePitch(InputValue value)
	{
		previewRot.eulerAngles += new Vector3(value.Get<float>(), 0, 0);
	}

	public void OnRotateYaw(InputValue value)
	{

	}

	public void OnRotateRoll(InputValue value)
	{

	}
	#endregion
}
