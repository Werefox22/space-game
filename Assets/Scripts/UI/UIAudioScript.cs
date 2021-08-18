using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAudioScript : MonoBehaviour
{
	[Header("Audio Clips")]
	public AudioClip click;

	[Header("References")]
	public AudioSource source;

	public void Click()
	{
		source.PlayOneShot(click);
	}
}
