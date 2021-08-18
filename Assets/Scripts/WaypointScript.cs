using UnityEngine;
using UnityEngine.UI;

public class WaypointScript : MonoBehaviour
{
	public Vector3 position;
	public string title;
	public Sprite icon;

	[Header("References")]
	public RectTransform rectTransform;
	public Text text;

	Camera mainCam;
	private void Start()
	{
		mainCam = Camera.main;

		text.text = title;
	}

	private void LateUpdate()
	{
		Vector2 screenPos = mainCam.WorldToScreenPoint(position);
		screenPos /= UIManager.canvas.scaleFactor;
		rectTransform.anchoredPosition = screenPos;
		

		Debug.DrawLine(position - Vector3.up, position + Vector3.up, Color.green);
		Debug.DrawLine(position - Vector3.right, position + Vector3.right, Color.red);
		Debug.DrawLine(position - Vector3.forward, position + Vector3.forward, Color.blue);
	}
}
