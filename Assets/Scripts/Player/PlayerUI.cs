using FoxThorne;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUI : MonoBehaviour
{
	public bool IsShiftHeld { get; private set; }
	public bool IsControlHeld { get; private set; }
	public bool IsControlShiftHeld { get { return IsShiftHeld && IsControlHeld; } }

	[Header("References")]
	public PlayerInput playerInput;
	public PlayerInventory playerInventory;

	private void Start()
	{
		UIUpdate();
	}

	public void UIUpdate()
	{
		if (UIManager.IsUIOpen)
		{
			Cursor.lockState = CursorLockMode.None;
			playerInput.SwitchCurrentActionMap("UI");
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			playerInput.SwitchCurrentActionMap("Player");
		}
	}

	// Player action map buttons
	public void OnInventory()
	{
		UIManager.SetCurrentScreen("Inventory");

		UIUpdate();
	}

	// UI action map buttons
	public void OnCancel()
	{
		UIManager.SetCurrentScreen(-1);

		UIUpdate();
	}

	public void OnSubmit()
	{
		UIManager.SetCurrentScreen(-1);

		UIUpdate();
	}

	public void OnPoint(InputValue value)
	{
		UIManager.UpdateCursorPosition(value.Get<Vector2>());
	}

	public void OnUIDrop()
	{
		ItemSlot slot = UIManager.hoveringItemSlot;

		if (slot != null)
		{
			int amount = 1;

			if (IsShiftHeld)
				amount = slot.count;

			playerInventory.DropItem(slot.index, amount);
		}
	}

	public void OnShift(InputValue value)
	{
		float state = value.Get<float>();

		if (state == 0)
		{
			IsShiftHeld = false;
		}
		else
		{
			IsShiftHeld = true;
		}
	}

	public void OnControl(InputValue value)
	{
		float state = value.Get<float>();

		if (state == 0)
		{
			IsControlHeld = false;
		}
		else
		{
			IsControlHeld = true;
		}
	}
}
