using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSettingsReferences : MonoBehaviour
{
	public PlayerInput playerInput;

	private void Start()
	{
		PlayerSettings.playerInput = playerInput;
	}

}
