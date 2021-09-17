using FoxThorne;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using HudHelper = ItemSO.HudHelper;

public class UIManager : MonoBehaviour
{
	public static bool IsUIOpen
	{
		get
		{
			return CurrentScreen >= 0;
		}
	}

	[Header("Settings")]
	public int startingScreen = -1;

	[Header("Inventory")]
	public RectTransform inventoryRoot;
	public Vector2 slotStartPos;
	public Vector2 slotOffset;
	public Vector2Int inventorySize;
	public GameObject itemSlotPrefab;

	public static List<ItemSlot> itemSlots = new List<ItemSlot>();

	[Header("Hotbar")]
	public RectTransform invHotbarRoot;
	public RectTransform hudHotbarRoot;
	public Vector2 hotSlotStartPos;
	public float hotSlotOffset;
	public int hotbarSize;
	public GameObject invHotbarSlotPrefab;
	public GameObject hudHotbarSlotPrefab;
	public Text itemNameTextRef;

	public static Text itemNameText;

	public static List<InvHotbarSlot> invHotbarSlots = new List<InvHotbarSlot>();
	public static List<HUDHotbarSlot> hudHotbarSlots = new List<HUDHotbarSlot>();

	[Header("Waypoints")]
	public RectTransform waypointRootRef;
	public GameObject waypointPrefabRef;

	public static RectTransform waypointRoot;
	public static GameObject waypointPrefab;

	static List<WaypointScript> waypoints = new List<WaypointScript>();

	[Header("Audio")]
	public UIAudioScript uiAudioRef;

	public static UIAudioScript uiAudio;

	[Header("References")]
	public Canvas canvasRef;
	public RectTransform screensRoot;
	public RectTransform helpersRoot;
	public Text infoTextRef;

	public static PlayerInventory player;

	// static variables
	// references
	public static Canvas canvas;
	public static CanvasScaler canvasScaler;
	public static Text infoText;

	public static CursorAnchorScript cursorAnchor;
	public static Image hoverTextImage;
	public static Text hoverTextText;

	// inventory cursor
	public static bool hideHoverText = false;
	public static ItemSlot hoveringItemSlot;
	public static bool hoveredSlotIsHotbar = false;
	public static ItemSlot grabbedItemSlot;

	// screens
	public static int CurrentScreen { get; private set; } = -1;

	public static List<GameObject> screens = new List<GameObject>();

	// cursor

	static UIManager uim;

	private void Start()
	{
		// set static vars
		canvas = canvasRef;
		canvasScaler = canvas.GetComponent<CanvasScaler>();
		infoText = infoTextRef;
		waypointRoot = waypointRootRef;
		waypointPrefab = waypointPrefabRef;
		uiAudio = uiAudioRef;
		itemNameText = itemNameTextRef;

		// get screens
		foreach (Transform child in screensRoot)
		{
			screens.Add(child.gameObject);
		}

		SetCurrentScreen(startingScreen);

		// setup inventory
		for (int i = 0; i < inventorySize.x * inventorySize.y; i++)
		{
			int y = Mathf.FloorToInt((float)i / inventorySize.x);
			int x = i - (inventorySize.x * y);

			Vector2 pos = slotStartPos + new Vector2(slotOffset.x * x, slotOffset.y * y);
			GameObject go = Instantiate(itemSlotPrefab, inventoryRoot);
			go.name = $"Item Slot {i} ({x}, {y})";
			go.GetComponent<RectTransform>().anchoredPosition = pos;

			ItemSlot itsl = go.GetComponent<ItemSlot>();
			itsl.index = i;
			itemSlots.Add(itsl);
		}

		// setup hotbar
		for (int i = 0; i < hotbarSize; i++)
		{
			Vector2 pos = hotSlotStartPos + new Vector2(hotSlotOffset * i, 0);

			// inventory hotbar
			GameObject inv = Instantiate(invHotbarSlotPrefab, invHotbarRoot);
			inv.name = "Hotbar Slot " + i;
			inv.GetComponent<RectTransform>().anchoredPosition = pos;

			InvHotbarSlot invScript = inv.GetComponent<InvHotbarSlot>();
			invScript.index = i;
			invHotbarSlots.Add(invScript);

			// hud hotbar
			GameObject hud = Instantiate(hudHotbarSlotPrefab, hudHotbarRoot);
			hud.name = "Hotbar Slot " + i;
			hud.GetComponent<RectTransform>().anchoredPosition = pos;

			HUDHotbarSlot hudScript = hud.GetComponent<HUDHotbarSlot>();
			hudScript.linkedSlot = invScript;
			hudHotbarSlots.Add(hudScript);
		}

	}

	#region screens
	/// <summary>
	/// Change the current screen.
	/// </summary>
	/// <param name="screenIndex">The screen to enable. Set negative to disable all screens.</param>
	public static void SetCurrentScreen(int screenIndex)
	{
		for (int i = 0; i < screens.Count; i++)
		{
			// enable given index, disable all other screens
			screens[i].SetActive(i == screenIndex);
		}

		CurrentScreen = screenIndex;

		UpdateCursorPosition(new Vector2(0, 0));
	}

	public static void SetCurrentScreen(string screenName)
	{
		int index = -1;
		bool found = false;
		for (int i = 0; i < screens.Count; i++)
		{
			// enable given screen, disable all other screens
			if (found || screens[i].name.Contains(screenName))
			{
				screens[i].SetActive(true);
				index = i;

				// once we find a screen, we should stop checking for other screens. 
				// this prevents the possibility of having two screens active at once, 
				// which could happen if two screens have the same name.
				found = true;
			}
			else
			{
				screens[i].SetActive(false);
			}
		}

		CurrentScreen = index;
	}
	#endregion

	#region inventory
	public static void InventoryUpdate()
	{
		GameConsole.LogVerbose("Updating inventory");

		for (int i = 0; i < itemSlots.Count; i++)
		{
			if (i < player.inventory.Count && player.inventory[i] != null)
			{
				itemSlots[i].SetItem(player.inventory[i]);
			}
			else
			{
				itemSlots[i].ClearItem();
			}
		}

		HotbarUpdate();
	}

	public static void HotbarUpdate()
	{
		GameConsole.LogVerbose("Updating hotbar");

		for (int i = 0; i < invHotbarSlots.Count; i++)
		{
			if (i < player.hotbar.Count && player.hotbar[i] != null)
			{
				Item item = new Item
				{
					data = player.hotbar[i],
					count = player.GetTotalItemCount(player.hotbar[i])
				};

				invHotbarSlots[i].SetItem(item);
			}
			else
			{
				invHotbarSlots[i].ClearItem();
			}

			hudHotbarSlots[i].UpdateItem();
		}
	}

	public static void MoveItem(int fromIndex, int toIndex)
	{
		if (fromIndex == toIndex) return;

		Item from = player.inventory[fromIndex];
		Item to = player.inventory[toIndex];

		if (from == null) return;

		// if moving to an empty slot
		if (to == null)
		{
			to = from;
			from = null;
		}
		else if (from.data == to.data) // if combining stacks
		{
			to.count += from.count;
			from = null;
		}
		else // items don't match
		{
			return;
		}

		player.inventory[fromIndex] = from;
		player.inventory[toIndex] = to;

		uiAudio.Click();

		player.InventoryUpdate();
	}

	public static void SetHotbarSlot(int index, ItemSO item)
	{
		player.hotbar[index] = item;

		uiAudio.Click();

		HotbarUpdate();
	}

	public static void SelectHotbarSlot(int index)
	{
		for (int i = 0; i < hudHotbarSlots.Count; i++)
		{
			if (i == index) // is the slot
			{
				hudHotbarSlots[i].Select();
			}
			else // is not the slot
			{
				hudHotbarSlots[i].Deselect();
			}
		}

		HudHelper helper;

		// if we're selecting something
		if (index >= 0 && invHotbarSlots[index].item != null)
		{
			itemNameText.text = invHotbarSlots[index].item.name;
			helper = invHotbarSlots[index].item.GetHudHelper();
		}
		else // not selecting something
		{
			itemNameText.text = "";
			helper = HudHelper.None;
		}

		switch (helper)
		{
			case HudHelper.None:
			{
				// clear all helpers
				break;
			}
			case HudHelper.Placement:
			{

				break;
			}
		}

		uiAudio.Click();
	}
	#endregion

	#region cursor
	public static void UpdateCursorPosition(Vector2 pos)
	{
		Vector2 adjPos = pos;
		adjPos.x /= canvas.pixelRect.width;
		adjPos.y /= canvas.pixelRect.height;
		adjPos *= canvasScaler.referenceResolution;

		cursorAnchor.rectTransform.anchoredPosition = adjPos;

		// hover
		if (hoveringItemSlot != null)
		{
			// get the type
			if (hoveringItemSlot.GetType() == typeof(InvHotbarSlot))
			{
				hoveredSlotIsHotbar = true;
			}
			else // inventory slot
			{
				hoveredSlotIsHotbar = false;
			}

			// hover text
			if (!hideHoverText && hoveringItemSlot.item != null)
			{
				cursorAnchor.SetHoverText(hoveringItemSlot.item.name);
			}
			else
			{
				cursorAnchor.ClearHoverText();
			}
		}
		else // if not hovering over anything
		{
			cursorAnchor.ClearHoverText();
		}
	}
	#endregion

	#region hud
	public static WaypointScript CreateWaypoint(Vector3 position, string text)
	{
		GameObject go = Instantiate(waypointPrefab, waypointRoot);

		WaypointScript ws = go.GetComponent<WaypointScript>();
		ws.position = position;
		ws.title = text;

		waypoints.Add(ws);

		return ws;
	}

	public static void DestroyWaypoint(WaypointScript waypoint)
	{
		if (waypoints.Contains(waypoint))
		{
			int index = waypoints.IndexOf(waypoint);
			Destroy(waypoint.gameObject);
			waypoints.RemoveAt(index);
		}
		else
		{
			GameConsole.LogError("Tried to destroy waypoint, but it was not found in the list!");
		}
	}

	public static void SetInfoText(string text)
	{
		if (text.Trim() == "")
		{
			ClearInfoText();
			return;
		}

		infoText.text = text;
	}

	public static void ClearInfoText()
	{
		infoText.text = "";
	}
	#endregion

	public static UIManager Get()
	{
		if (uim == null)
			uim = FindObjectOfType<UIManager>(true);

		return uim;
	}
}
