using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
	public virtual void Interact(PlayerInventory player)
	{

	}

	public virtual string GetInfoText()
	{
		return "Default interactable text";
	}
}
