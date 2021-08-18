using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{
	[Header("Camera")]
	public float cameraSensitivity = 0.3f;
	public float minCamAngle = -89;
	public float maxCamAngle = 89;

	public bool invertYAxis = false;
	public bool invertXAxis = false;

	Vector2 lookInput;

	[Header("Movement")]
	public float walkSpeed = 5;
	public float sprintSpeed = 7;
	public float crouchedSpeed = 3;
	public float gravity = 0.2f;

	float currentSpeed;

	/// <summary>
	/// Is the player moving or trying to move?
	/// </summary>
	public bool IsMoving { get; private set; }

	[Header("Sprinting")]
	public bool toggleSprint = false;
	public float maxStamina = 10;
	public float staminaRecoveryRate = 1;

	/// <summary>
	/// Is the player currently sprinting?
	/// </summary>
	public bool IsSprinting { get; private set; }

	bool canSprint = true;

	public float CurrentStamina { get; private set; }

	[Header("Crouching")]
	public bool toggleCrouch = false;
	public float standingHeight = 2;
	public float crouchingHeight = 1;
	public float crouchTime = 0.25f;

	float currentHeight;

	/// <summary>
	/// Is the player currently crouching?
	/// </summary>
	public bool IsCrouching { get; private set; }

	[Header("Jumping")]
	public float jumpSpeed = 0.05f;

	public float verticalVelocity;
	public Vector2 moveInput;

	[Header("Footsteps")]
	public AudioClip[] footstepSounds;
	[Tooltip("How much time inbetween footsteps in seconds at a speed of 5.")]
	public float footstepTime = 0.75f;
	public float minPitch = 0.5f;
	public float maxPitch = 1.5f;

	float footstepTimer;

	[Header("References")]
	public Camera playerCam;
	public AudioSource source;
	public CharacterController characterController;
	public PlayerUI playerUI;

	UIManager uiManager;

	void Start()
	{
		uiManager = UIManager.Get();
	}

	void Update()
	{
		#region camera
		if (lookInput != Vector2.zero && !UIManager.IsUIOpen)
		{
			float newX = playerCam.transform.localEulerAngles.x - (lookInput.y * cameraSensitivity);
			float newY = playerCam.transform.localEulerAngles.y + (lookInput.x * cameraSensitivity);
			if (newX > 180)
				newX -= 360;
			newX = Mathf.Clamp(newX, -maxCamAngle, maxCamAngle);

			playerCam.transform.localEulerAngles = new Vector3(newX, newY);
		}
		#endregion

		#region movement
		if (characterController.isGrounded && verticalVelocity < 0)
		{
			verticalVelocity = -gravity * Time.deltaTime;
		}
		else
		{
			verticalVelocity -= gravity * Time.deltaTime;
		}

		Vector3 camForward = playerCam.transform.forward;
		camForward.y = 0;
		camForward.Normalize();
		Vector3 camRight = playerCam.transform.right;
		camRight.y = 0;
		camRight.Normalize();

		if (!canSprint || IsCrouching)
			IsSprinting = false;

		if (IsCrouching)
			currentSpeed = crouchedSpeed;
		else if (IsSprinting)
			currentSpeed = sprintSpeed;
		else
			currentSpeed = walkSpeed;

		Vector3 move = currentSpeed * moveInput.y * Time.deltaTime * camForward + currentSpeed * moveInput.x * Time.deltaTime * camRight;

		move.y = verticalVelocity;

		characterController.Move(move);

		// sprinting/stamina
		if (moveInput != Vector2.zero && IsSprinting)
		{
			CurrentStamina -= Time.deltaTime;
		}
		else if (CurrentStamina < maxStamina)
		{
			CurrentStamina += staminaRecoveryRate * Time.deltaTime;
		}

		if (CurrentStamina <= 0)
			canSprint = false;

		if (!canSprint && CurrentStamina == maxStamina)
			canSprint = true;

		CurrentStamina = Mathf.Clamp(CurrentStamina, 0, maxStamina);

		// crouching

		float distance = standingHeight - crouchingHeight;
		if (IsCrouching && currentHeight > crouchingHeight)
		{
			currentHeight -= distance * Time.deltaTime / crouchTime;
		}
		else if (!IsCrouching && currentHeight < standingHeight)
		{
			currentHeight += distance * Time.deltaTime / crouchTime;
		}

		currentHeight = Mathf.Clamp(currentHeight, crouchingHeight, standingHeight);
		playerCam.transform.localPosition = new Vector3(0, currentHeight, 0);

		#endregion

		IsMoving = moveInput != Vector2.zero;

		footstepTimer += Time.deltaTime;

		if (IsMoving && characterController.isGrounded)
		{
			if (footstepTimer >= footstepTime * (5 / currentSpeed))
			{
				footstepTimer = 0;
				source.pitch = Random.Range(minPitch, maxPitch);
				source.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
			}
		}
	}

	public void OnMove(InputValue value)
	{
		if (UIManager.IsUIOpen)
		{
			moveInput = Vector2.zero;
		}
		else
		{
			moveInput = value.Get<Vector2>();
		}
	}
	public void OnLook(InputValue value)
	{
		Vector2 input = value.Get<Vector2>();

		if (invertXAxis)
		{
			input.x *= -1;
		}
		if (invertYAxis)
		{
			input.y *= -1;
		}

		lookInput = input;
	}

	public void OnSprint(InputValue value)
	{
		float state = value.Get<float>();

		if (toggleSprint == false)
		{
			if (state == 1)
			{
				IsSprinting = true;
			}
			else
			{
				IsSprinting = false;
			}
		}
		else if (state == 1)
		{
			IsSprinting = !IsSprinting;
		}
	}

	public void OnCrouch(InputValue value)
	{
		if (UIManager.IsUIOpen) return;

		float state = value.Get<float>();

		if (toggleCrouch == false)
		{
			if (state == 1)
			{
				IsCrouching = true;
			}
			else
			{
				IsCrouching = false;
			}
		}
		else if (state == 1)
		{
			IsCrouching = !IsCrouching;
		}
	}

	public void OnJump()
	{
		if (characterController.isGrounded && !UIManager.IsUIOpen)
		{
			verticalVelocity = jumpSpeed;
		}
	}
}
