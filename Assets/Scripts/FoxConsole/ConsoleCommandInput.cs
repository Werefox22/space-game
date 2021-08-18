using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace FoxThorne
{
	[RequireComponent(typeof(InputField))]
	public class ConsoleCommandInput : MonoBehaviour
	{
		InputField inputField;

		private void Start()
		{
			inputField = GetComponent<InputField>();
		}

		public void SubmitCommand(string input)
		{
			if (input.Trim() == "") return;

			inputField.text = "";

			GameConsole.Log(input);
		}
	}
}