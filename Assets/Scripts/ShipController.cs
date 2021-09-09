using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

public class ShipController : MonoBehaviour
{
	[Header("Settings")]
	public bool enginesOn = false;
	public bool stabilize = true;
	public bool hold = false;
	public Vector3 holdPos;
	public Vector3 targetAngles;
	public float stablizerStrength = 100;

	[Header("Info")]
	public float totalThrust;
	public float averageThrust;
	public float thrustRequiredToHover;
	public float currentSpeed;
	public Vector3 currentAcceleration;
	public float currentAccelMagnitude;

	[Header("References")]
	Rigidbody rb;

	List<ThrusterScript> thrusters = new List<ThrusterScript>();

	private void OnValidate()
	{
		rb = GetComponent<Rigidbody>();
	}

	void Start()
	{
		thrusters = GetComponentsInChildren<ThrusterScript>().ToList();

		rb.mass = transform.childCount;

		thrustRequiredToHover = rb.mass * Physics.gravity.magnitude;
	}

	bool oldEnginesOn;
	void Update()
	{
		if (oldEnginesOn != enginesOn)
		{
			SetThrustersActive(enginesOn);
		}

		Vector3 totalThrust = Vector3.zero;

		if (stabilize)
		{
			// position
			totalThrust += Vector3.up * thrustRequiredToHover;

			totalThrust -= rb.velocity * rb.mass * stablizerStrength;

			// angle

		}

		if (hold)
		{
			if (transform.position != holdPos)
			{
				float dist = Vector3.Distance(transform.position, holdPos);

				totalThrust += (holdPos - transform.position).normalized * dist;
			}
		}

		SetThrust(totalThrust);

		oldEnginesOn = enginesOn;

		Debug.DrawRay(transform.position, rb.velocity, Color.blue, Time.deltaTime, false);
		Debug.DrawRay(transform.position, currentAcceleration, Color.red, Time.deltaTime, false);
	}


	Vector3 oldVelocity;
	private void FixedUpdate()
	{
		currentSpeed = rb.velocity.magnitude;

		currentAcceleration = (rb.velocity - oldVelocity) / Time.fixedDeltaTime;
		currentAccelMagnitude = currentAcceleration.magnitude;

		oldVelocity = rb.velocity;
	}

	public void SetThrustersActive(bool active)
	{
		foreach (ThrusterScript ts in thrusters)
		{
			ts.thrusting = active;
		}
	}

	public void SetThrust(Vector3 velocity)
	{
		float thrust = velocity.magnitude;
		Vector3 dir = velocity.normalized;
		SetThrust(thrust, dir);
	}

	public void SetThrust(float thrust, Vector3 direction)
	{
		// get thruster counts
		float totalDot = 0;
		foreach (ThrusterScript ts in thrusters)
		{
			totalDot += Mathf.Clamp01(Vector3.Dot(direction, ts.transform.up));
		}

		thrust /= totalDot;

		float tt = 0;
		foreach (ThrusterScript ts in thrusters)
		{
			float dot = Mathf.Clamp01(Vector3.Dot(direction, ts.transform.up));

			ts.currentThrust = Mathf.Clamp(thrust * dot, ts.minThrust, ts.maxThrust);

			tt += ts.currentThrust;
		}

		totalThrust = tt;
		averageThrust = tt / thrusters.Count;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(holdPos, 0.5f);

		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position + rb.centerOfMass, 0.5f);
	}
}
