using UnityEngine;
using UnityEngine.UI;

public class CursorAnchorScript : MonoBehaviour
{
	[Header("Settings")]
	public float padding = 6.25f;

	[Header("References")]
	public RectTransform rectTransform;
	public Image hoverTextImage;
	public Text hoverTextText;

	private void Start()
	{
		ClearHoverText();
		UIManager.cursorAnchor = this;
	}

	public void SetHoverText(string text)
	{
		if (text.Trim() == "")
		{
			ClearHoverText();
			return;
		}

		// set text
		hoverTextText.text = text;

		// set size
		Vector2 extents = Vector2.zero; // changing this value has no apparent effect. no idea what it does or why it's required, couldn't find any kind of documentation

		float targetWidth = hoverTextText.cachedTextGeneratorForLayout.GetPreferredWidth(text, hoverTextText.GetGenerationSettings(extents));
		float targetHeight = hoverTextText.cachedTextGeneratorForLayout.GetPreferredHeight(text, hoverTextText.GetGenerationSettings(extents));

		hoverTextImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth + padding * 2);
		hoverTextImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight + padding * 2);

		// enable
		hoverTextImage.gameObject.SetActive(true);
	}

	public void ClearHoverText()
	{
		hoverTextImage.gameObject.SetActive(false);
	}
}
