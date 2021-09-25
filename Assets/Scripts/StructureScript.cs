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
		newBlock.tag = "Placeable";
		newBlock.rootStructure = this;
		blocks.Add(newBlock);
	}

	public Vector3 GetSnappedPosition(Vector3 position)
	{
		Vector3 localPos = transform.InverseTransformPoint(position);

		localPos = Utility.RoundVectorToInt(localPos);

		Vector3 worldPos = transform.TransformPoint(localPos);

		return worldPos;
	}
}
