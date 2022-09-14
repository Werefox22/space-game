using FoxThorne;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : Inventory
{
	[Header("Lists")]
	public List<ItemSO> hotbar = new List<ItemSO>(10);

	[Header("Hotbar")]
	public SelectedItem selectedItem;
	public struct SelectedItem
	{
		public SelectedItem(int slot = -1, ItemSO data = null)
		{
			Slot = slot;
			Data = data;
		}

		public int Slot;
		public ItemSO Data;
	}

	[Header("Placement")]
	public GameObject previewObj;
	public PlacementValidationScript previewValidator;
	public Vector3 previewPos;
	public Vector3 previewRot;
	public float bigRotationCooldown = 0.5f;
	public Material previewMaterial;
	public Color validColor = Color.green;
	public Color invalidColor = Color.red;

	public StructureScript observingStructure;

	Vector3 previewRotFrom;
	Vector3 previewRotInput = Vector3.zero;
	float rotCooldownTimer;

	[Header("Interaction")]
	public Interactable observingInteractable;

	[Header("Raycast")]
	public LayerMask raycastLayerMask;
	public float raycastMaxDistance = 10;
	public bool raycastHit;
	public RaycastHit hitInfo;

	Collider lastHitCollider;


	[Header("References")]
	public Inventory inventory;
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

		SelectSlot(selectedItem.Slot);
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

		// count down the cooldown no matter what
		rotCooldownTimer -= Time.deltaTime;

		// preview
		if (previewObj != null) // if previewing
		{
			// validation
			if (previewValidator.IsValid)
			{
				previewMaterial.SetColor("_UnlitColor", validColor);
			}
			else
			{
				previewMaterial.SetColor("_UnlitColor", invalidColor);
			}

			// rotation
			if (previewRotInput.magnitude >= 90)
			{
				// apply cooldown if rotating by 90 degrees
				if (rotCooldownTimer <= 0)
				{
					rotCooldownTimer = bigRotationCooldown;
					previewRotFrom = previewRot;
					previewRot += previewRotInput;
				}
			}
			else
			{
				previewRot += previewRotInput;
			}

			// if the raycast hit something
			if (raycastHit)
			{
				// if we're looking at a structure
				if (observingStructure != null)
				{
					previewPos = observingStructure.GetSnappedPosition(hitInfo.collider.transform.position + hitInfo.normal);
					previewRot = observingStructure.GetSnappedAngle(previewRot);
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

			float percent = 1 - (rotCooldownTimer / bigRotationCooldown);
			Vector3 angles = Vector3.Lerp(previewRotFrom, previewRot, percent);
			previewObj.transform.SetPositionAndRotation(previewPos, Quaternion.Euler(angles));
		}
		else // not previewing
		{
			previewRot = Vector3.zero;
			previewRotFrom = Vector3.zero;
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

	public void SelectSlot(int index)
	{
		// tell the currently selected slot to deselect
		if (selectedItem.Slot >= 0 && hotbar[selectedItem.Slot] != null)
		{
			hotbar[selectedItem.Slot].OnDeselect(this);

		}

		// select the new slot
		if (index >= 0)
		{
			selectedItem.Data = hotbar[index];

			if (selectedItem.Data != null)
			{
				selectedItem.Data.OnSelect(this);
			}
		}
		else // deselecting
		{
			selectedItem.Data = null;
		}

		selectedItem.Slot = index;
		UIManager.SelectHotbarSlot(index);
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
			Data = data,
			Count = count
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
		rb.isKinematic = true;
		rb.useGravity = false;

		// modify colliders
		Placeable p = previewObj.GetComponent<Placeable>();
		p.collidersRoot.transform.localScale *= 0.95f;
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

		int num = selectedItem.Slot;

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
		if (selectedItem.Slot >= 0 && hotbar[selectedItem.Slot] != null && HasItem(hotbar[selectedItem.Slot], 1))
		{
			if (hotbar[selectedItem.Slot].PrimaryAction(this))
			{
				RemoveItems(hotbar[selectedItem.Slot], 1);
			}
		}
	}

	public void OnDrop()
	{
		if (selectedItem.Data != null)
		{
			int amount = 1;

			if (playerUI.IsShiftHeld)
				amount = selectedItem.Data.maxStackSize;

			DropItem(selectedItem.Data, amount);
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
		float input = value.Get<float>();

		if (playerUI.IsShiftHeld || observingStructure != null)
		{
			input *= 90;
		}

		previewRotInput.x = input;
	}

	public void OnRotateYaw(InputValue value)
	{
		float input = value.Get<float>();

		if (playerUI.IsShiftHeld || observingStructure != null)
		{
			input *= 90;
		}

		previewRotInput.y = input;
	}

	public void OnRotateRoll(InputValue value)
	{
		float input = value.Get<float>();

		if (playerUI.IsShiftHeld || observingStructure != null)
		{
			input *= 90;
		}

		previewRotInput.z = input;
	}
	#endregion
}
