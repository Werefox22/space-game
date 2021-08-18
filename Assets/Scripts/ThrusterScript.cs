using FoxThorne;
using System.Collections.Generic;
using UnityEngine;

public class ThrusterScript : MonoBehaviour
{
	[Header("Thruster Settings")]
	public bool thrusting = false;
	[Tooltip("Current thrust, in newtons.")]
	public float currentThrust = 1;
	public float minThrust = 0;
	public float maxThrust = 100;

	public const float newtonsToUnityMultiplier = 50;

	[Header("Flame Settings")]
	public float minFlameIntensity = 0;
	public float maxFlameIntensity = 100000;
	public float minParticleSpeed = 0;
	public float maxParticleSpeed = 30;

	[Header("References")]
	public GameObject flame;
	public Light flameLight;
	public ParticleSystem flameParticles;
	public Rigidbody rb;

	private void Start()
	{
		if (rb == null)
		{
			rb = GetComponentInParent<Rigidbody>();
		}
	}

	float oldSpeed;
	private void Update()
	{
		currentThrust = Mathf.Clamp(currentThrust, minThrust, maxThrust);

		if (flame.activeSelf != thrusting)
		{
			flame.SetActive(thrusting);
			flameLight.gameObject.SetActive(thrusting);
		}


	}

	private void FixedUpdate()
	{
		if (thrusting)
		{
			rb.AddForceAtPosition(transform.up * currentThrust * newtonsToUnityMultiplier * Time.fixedDeltaTime, transform.position);

			// adjust visuals
			float percent = currentThrust / maxThrust;
			flameLight.intensity = Mathf.Clamp(percent * maxFlameIntensity, minFlameIntensity, maxFlameIntensity);

			var main = flameParticles.main;
			main.startSpeed = Mathf.Clamp(percent * maxParticleSpeed, minParticleSpeed, maxParticleSpeed);

			flame.transform.localScale = new Vector3(0.9f, percent, 0.9f);
		}
	}
}
