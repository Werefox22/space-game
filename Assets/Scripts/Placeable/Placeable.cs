using FoxThorne;
using UnityEngine;

public class Placeable : MonoBehaviour
{
	[Header("Data")]
	public PlaceableItemSO item;
	public float health;

	[Header("References")]
	public StructureScript rootStructure;
	public GameObject collidersRoot;
	WaypointScript waypoint;

	private void Update()
	{
		if (waypoint != null)
		{
			waypoint.position = transform.position;
		}
	}

	public void ShowInHUD()
	{
		waypoint = UIManager.CreateWaypoint(transform.position, name);
	}

	public void HideInHUD()
	{
		UIManager.DestroyWaypoint(waypoint);
	}
}
