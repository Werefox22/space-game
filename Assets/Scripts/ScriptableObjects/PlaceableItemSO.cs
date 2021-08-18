using UnityEngine;

[CreateAssetMenu(fileName = "Placeable", menuName = "Scriptable Objects/Placeable", order = 1)]
public class PlaceableItemSO : ItemSO
{
	[Header("Placeable")]
	public GameObject prefab;
	public float maxHealth;
	
	public override bool PrimaryAction(PlayerInventory player)
	{
		StructureScript s = player.observingStructure;

		if (s == null)
		{
			s = player.CreateStructureAtPreview();
		}

		GameObject go = Instantiate(prefab, player.previewPos, player.previewRot, s.transform);
		s.AddBlock(go.GetComponent<Placeable>());

		return true;
	}

	public override void OnSelect(PlayerInventory player)
	{
		player.CreatePreview(this);
	}

	public override void OnDeselect(PlayerInventory player)
	{
		player.DestroyPreview();
	}
}
