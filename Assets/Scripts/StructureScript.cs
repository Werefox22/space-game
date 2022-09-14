using System.Collections.Generic;
using UnityEngine;
using FoxThorne;

public class StructureScript : MonoBehaviour
{
	[Header("Structure")]
	public List<Placeable> blocks = new List<Placeable>();
	public bool isShip = false;
	public string structureName = "Structure";

	public static int structureCount;

	private void Start()
	{
		tag = "Structure";
		structureName = "Structure " + structureCount;
		name = structureName;
		structureCount++;
	}

	public void AddBlock(Placeable newBlock)
	{
		Utility.SetTagRecursively(newBlock.gameObject, "Placeable");
		newBlock.rootStructure = this;
		blocks.Add(newBlock);
	}

	public Vector3 GetSnappedPosition(Vector3 position)
	{
		// switch to local
		position = transform.InverseTransformPoint(position);
		// round it
		position = Utility.RoundVectorToInt(position);
		// switch back to global
		position = transform.TransformPoint(position);
		// return
		return position;
	}

	public Vector3 GetSnappedAngle(Vector3 angle)
	{
		// switch to local
		angle -= transform.eulerAngles;
		// round it
		angle = Utility.RoundToNearest(angle, 90);
		// switch back to global
		angle += transform.eulerAngles;
		// return
		return angle;
	}
}
