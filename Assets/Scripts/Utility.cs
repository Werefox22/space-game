using System;
using System.Collections.Generic;
using UnityEngine;

namespace FoxThorne
{
	public static class Utility
	{
		#region positional
		/// <summary>
		/// Takes a position and an array and returns the Transform closest to the given position.
		/// </summary>
		/// <param name="position">The position to reference.</param>
		/// <param name="gameObjects">An array of Transforms to check.</param>
		/// <returns>The closest Transform to the position.</returns>
		public static Transform FindNearest(Vector3 position, Transform[] transforms)
		{
			Transform retVal = null;
			foreach (Transform t in transforms)
			{
				if (t == null)
					continue;

				// if the return value hasn't been set yet
				if (retVal == null)
				{
					// set it
					retVal = t;
				}
				// otherwise, if the distance to the current transform is less than the distance to the current return value
				else if (Vector3.Distance(position, t.position) < Vector3.Distance(position, retVal.position))
				{
					// overwrite it
					retVal = t;
				}
			}

			return retVal;
		}
		public static Transform FindNearest(Vector3 position, List<Transform> transforms)
		{
			return FindNearest(position, transforms.ToArray());
		}
		/// <summary>
		/// Takes a position and an array and returns the GameObject closest to the given position.
		/// </summary>
		/// <param name="position">The position to reference.</param>
		/// <param name="gameObjects">An array of GameObjects to check.</param>
		/// <returns>The closest GameObject to the position.</returns>
		public static GameObject FindNearest(Vector3 position, GameObject[] gameObjects)
		{
			GameObject retVal = null;
			foreach (GameObject go in gameObjects)
			{
				if (go == null)
					continue;

				// if the return value hasn't been set yet
				if (retVal == null)
				{
					// set it
					retVal = go;
				}
				// otherwise, if the distance to the current GameObject is less than the distance to the current return value
				else if (Vector3.Distance(position, go.transform.position) < Vector3.Distance(position, retVal.transform.position))
				{
					// overwrite it
					retVal = go;
				}
			}

			return retVal;
		}
		/// <summary>
		/// Takes a position and a list and returns the GameObject closest to the given position.
		/// </summary>
		/// <param name="position">The position to reference.</param>
		/// <param name="gameObjects">A list of GameObjects to check.</param>
		/// <returns>The closest GameObject to the position.</returns>
		public static GameObject FindNearest(Vector3 position, List<GameObject> gameObjects)
		{
			return FindNearest(position, gameObjects.ToArray());
		}
		/// <summary>
		/// Checks if a value is in range of a target.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <param name="target">The target value.</param>
		/// <param name="range">The range, or how far the value can be from the target and still return true.</param>
		/// <returns>True if the value was within the range around the target, false if it was not.</returns>
		#endregion

		#region number ranges
		public static bool IsApproximate(float value, float target, float range)
		{
			return value >= target - range && value <= target + range;
		}

		public static bool IsApproximate(Vector3 value, Vector3 target, float range)
		{
			return IsApproximate(value.x, target.x, range) && IsApproximate(value.y, target.y, range) && IsApproximate(value.z, target.z, range);
		}

		/// <summary>
		/// Checks if an angle is in range of a target angle. This means that -100 will be equal to 260.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <param name="target">The target value.</param>
		/// <param name="range">The range, or how far the value can be from the target and still return true.</param>
		/// <returns>True if the value was within the range around the target, false if it was not.</returns>
		public static bool IsApproximateAngle(float value, float target, float range)
		{
			value = To360(value);
			target = To360(target);

			return IsApproximate(value, target, range);
		}

		/// <summary>
		/// Checks if value is inbetween min and max.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <param name="min">The minimum number the value can be to return true.</param>
		/// <param name="max">The maximum number the value can be to return true.</param>
		/// <returns>True if value is inbetween min and max, false if it is not.</returns>
		public static bool IsInRange(float value, float min, float max)
		{
			if (!VerifyMinMax(min, max))
				return false;

			return value >= min && value <= max;
		}

		/// <summary>
		/// Checks if value is inbetween min and max.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <param name="min">The minimum number the value can be to return true.</param>
		/// <param name="max">The maximum number the value can be to return true.</param>
		/// <returns>True if value is inbetween min and max, false if it is not.</returns>
		public static bool IsInRange(int value, int min, int max)
		{
			if (!VerifyMinMax(min, max))
				return false;

			return value >= min && value <= max;
		}



		/// <summary>
		/// Takes a value and modifies it to be inbetween 0 and 360.
		/// </summary>
		/// <param name="value">The value to move to be inbetween 0 and 360.</param>
		/// <returns></returns>
		public static float To360(float value)
		{
			return MoveToRange(value, 0, 360);
		}

		public static int MoveToRange(int value, int min, int max)
		{
			if (!VerifyMinMax(min, max))
				return value;

			int difference = max - min;

			while (value < min)
			{
				value += difference;
			}

			while (value > max)
			{
				value -= difference;
			}

			return value;
		}

		public static float MoveToRange(float value, float min, float max)
		{
			if (!VerifyMinMax(min, max))
				return value;

			float difference = max - min;

			while (value < min)
			{
				value += difference;
			}

			while (value > max)
			{
				value -= difference;
			}

			return value;
		}
		#endregion

		#region gameobjects
		/// <summary>
		/// Takes a transform and returns the highest level transform in its hierarchy.
		/// </summary>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static Transform GetRootObject(Transform transform)
		{
			while (transform.transform.parent != null)
			{
				transform = transform.transform.parent;
			}
			return transform;
		}

		/// <summary>
		/// Takes a game object and returns the highest level game object in its hierarchy.
		/// </summary>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static GameObject GetRootObject(GameObject gameObject)
		{
			return GetRootObject(gameObject.transform).gameObject;
		}

		#endregion

		#region layers
		/// <summary>
		/// Sets the layer for a gameobject and all of its children.
		/// </summary>
		/// <param name="rootObject">The root gameobject.</param>
		/// <param name="layer">The layer to be set.</param>
		public static void SetLayerRecursively(GameObject rootObject, int layer)
		{
			if (rootObject == null)
				return;

			rootObject.layer = layer;

			foreach (Transform child in rootObject.transform)
			{
				if (child == null)
					return;

				SetLayerRecursively(child.gameObject, layer);
			}
		}

		/// <summary>
		/// Sets the tag for a gameobject and all of its children.
		/// </summary>
		/// <param name="rootObject">The root gameobject.</param>
		/// <param name="tag">The tag to be set.</param>
		public static void SetTagRecursively(GameObject rootObject, string tag)
		{
			if (rootObject == null)
				return;

			rootObject.tag = tag;

			foreach (Transform child in rootObject.transform)
			{
				if (child == null)
					return;

				SetTagRecursively(child.gameObject, tag);
			}
		}
		#endregion

		#region logging
		/// <summary>
		/// Verifies that min is less than max.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns>True if min is less than max, false otherwise.</returns>
		static bool VerifyMinMax(float min, float max)
		{
			if (min == max)
			{
				GameConsole.LogWarning("Min is equal to max. Was this intentional?");
			}
			else if (min > max)
			{
				GameConsole.LogError("Min was greater than max. This is not allowed.");
				return false;
			}
			return true;
		}
		#endregion

		#region rounding
		public static float RoundDecimal(float value, int decimalPlaces)
		{
			if (decimalPlaces == 0)
			{
				return Mathf.RoundToInt(value);
			}

			string valueString = value.ToString();

			if (!valueString.Contains(".")) // if there's no decimal, no rounding is required
			{
				return value;
			}

			int decimalIndex = valueString.IndexOf(".");

			value = float.Parse(valueString.Substring(0, decimalIndex + decimalPlaces + 1));
			float rValue = float.Parse(valueString.Substring(decimalIndex));
			rValue = Mathf.RoundToInt(rValue);

			float adjustmentAmount = rValue / Mathf.Pow(10, decimalPlaces);
			value += adjustmentAmount;

			return value;
		}

		public static Vector3Int RoundVectorToInt(Vector3 vector)
		{
			Vector3Int retval = Vector3Int.zero;

			retval.x = Mathf.RoundToInt(vector.x);
			retval.y = Mathf.RoundToInt(vector.y);
			retval.z = Mathf.RoundToInt(vector.z);

			return retval;
		}
		#endregion

		public static Vector3 GetVectorFromAngle(float angle)
		{
			float angleRad = angle * Mathf.Deg2Rad;
			return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
		}

		public static string Clockify(double time)
		{
			double minutes;
			double seconds;
			if (time >= 60) // at least a minute
			{
				minutes = time / 60;
				seconds = time % 60;
			}
			else
			{
				minutes = 0;
				seconds = time;
			}

			string minutesString = minutes.ToString();
			string secondsString = seconds.ToString();

			if (seconds < 10)
				secondsString = "0" + secondsString;

			if (minutesString.Contains("."))
				minutesString = minutesString.Substring(0, minutesString.IndexOf("."));
			if (secondsString.Contains("."))
				secondsString = secondsString.Substring(0, secondsString.IndexOf("."));

			return minutesString + ":" + secondsString;
		}

		/// <summary>
		/// Takes a list and checks for any duplicate values.
		/// </summary>
		/// <param name="list">The list to iterate through.</param>
		/// <returns>True if there was a duplicate value. False if there were no duplicates.</returns>
		public static bool CheckForDuplicateInts(List<int> list)
		{
			List<int> seen = new List<int>();

			for (int i = 0; i < list.Count; i++)
			{
				if (seen.Contains(list[i]))
				{
					return true;
				}
				else
				{
					seen.Add(list[i]);
				}
			}
			return false;
		}
	}
}
