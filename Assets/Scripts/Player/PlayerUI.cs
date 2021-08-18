using FoxThorne;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUI : MonoBehaviour
{
	[Header("References")]
	public PlayerInput playerInput;
	UIManager uiManager;

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
}
