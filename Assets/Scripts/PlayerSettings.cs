using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSettings : MonoBehaviour
{
	public static PlayerInput playerInput;

	public static InputActionMap map;

	static string seperator = "/";
	private void Start()
	{
		map = playerInput.actions.FindActionMap("Player");

		Debug.Log(GetBinding("Move"));
	}

	/// <summary>
	/// Returns a formatted string of all bindings for an action.
	/// </summary>
	/// <remarks> It's recommended to cache the string as this method has to iterate through multiple lists.</remarks>
	/// <param name="action">The name of the action to get the bindings from.</param>
	/// <returns>Returns a formatted string of all bindings for <paramref name="action"/>.</returns>
	public static string GetBinding(string action)
	{
		string retval = "";
		foreach (InputControl control in map.FindAction(action).controls)
		{
			// add a seperator if it's not the first control
			if (retval != "")
			{
				retval += seperator;
			}

			retval += control.displayName;
		}

		return retval;
	}
}