using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementValidationScript : MonoBehaviour
{
	public bool IsValid = true;
	public bool IsShip = true;

	public string terrainLayerName = "Terrain";
	int terrainLayer;

	int offenders;

	private void Start()
	{
		terrainLayer = LayerMask.NameToLayer(terrainLayerName);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == terrainLayer)
		{
			IsShip = false;
		}
		else
		{
			offenders++;
			IsValid = false;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == terrainLayer)
		{
			IsShip = true;
		}
		else
		{
			offenders--;

			if (offenders <= 0)
				IsValid = true;
		}
	}
}
