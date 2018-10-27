using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Temporary scene management
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

	//inspector variables
	public float maxWalkSpeed = 7;
	public float minWalkSpeed = 1;
	public float runSpeed = 10;
	public float jumpHeight = 2;

	public float turnSmoothTime = .3f;
	public float inputSmoothTime = .1f;

	public float gravity = -35;

	//temp
	public float sphereCastRadius = .75f;
	public float sphereCastDistance = .25f;

	//object reference
	public GameObject playerCam;
	CharacterController controller;
	BoxAnimator charAnimator;

	//temp scene reference
	Scene scene;

	//model
//	Vector3 inputMove;
	Vector3 smoothInput;
	Vector3 velocity;

	bool isGrounded;
//	Vector3[] groundCheckRays = new Vector3[8];

	//tags
	bool jumping;
	bool running;
	
	//ref
	Vector3 currentInputVelocity;
	Vector3 rotatedVector;
	float inputAngle;
	float currentGravAccel;
	float oldInputAngle;
	bool canJump;

	float facingAngle;

	void Start()
	{
		controller = GetComponent<CharacterController>();
		scene = SceneManager.GetActiveScene();
		charAnimator = GetComponent<BoxAnimator>();
	}

	void Update()
	{
		//temp scene management
		if (Input.GetButtonDown("Cancel"))
			SceneManager.LoadScene(scene.name, LoadSceneMode.Single);

		//raw input vector from input axes
		Vector3 inputMove = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

		running = Input.GetButton("Button5");

		//jumpin
		jumping = Input.GetButton("Button2");
		if (Input.GetButtonDown("Button2") && canJump)
			Jump();

		ApplyGravity();

		//call functions
		Move(inputMove);
	}

	void FixedUpdate()
	{
		isGrounded = GroundCheck();
	}

	void Move(Vector3 inputMove)
	{
		//camera angle relative to player
		float cameraAngle = Mathf.Atan2 (playerCam.transform.forward.x, playerCam.transform.forward.z) * Mathf.Rad2Deg;
		//input rotation so its relative to camera
		inputMove = (Quaternion.Euler(0, cameraAngle, 0) * inputMove);

		if (isGrounded) {

			//lerp walk speed based on input magnitude (clamped at 0.8) -- temporarily add runspeed in as maxwalkspeed if running is true
			float walkSpeed = Mathf.Lerp (minWalkSpeed, running ? runSpeed : maxWalkSpeed, Mathf.Clamp(inputMove.magnitude, 0f, 0.8f) / 0.8f);

			//facing -- add angle smoothing rather than doing it based on smoothInput
			//target facing angle = inputmove converted to angle
			//if input magnitude is zero, set facing angle to last recorded facing angle, else set it to smoothed between current and target inputangle
			float targetinputAngle = Mathf.Atan2 (inputMove.normalized.x, inputMove.normalized.z) * Mathf.Rad2Deg;
			inputAngle = inputMove.magnitude == 0 ? oldInputAngle : Mathf.LerpAngle(inputAngle, targetinputAngle, Time.deltaTime / turnSmoothTime);

			//if grounded, velocity is the inputmove*walkspeed aligned with the normals of the ground
			//if not grounded, velocity is the inputmove*walkspeed + grav accel on y axis
	//		if (isGrounded) {
			canJump = true;

//			Vector3 targetVel = PeepGround (inputMove.normalized) * walkSpeed;
			//target velocity determined by facing direction and ground angle now
			Vector3 targetVel = PeepGround (Quaternion.AngleAxis(inputAngle, Vector3.up) * Vector3.forward) * walkSpeed * inputMove.normalized.magnitude;
			//velocity is targetvel on x and z, on y if its grounded, else its handled by gravity calculation
			velocity = new Vector3 (targetVel.x, jumping ? velocity.y : targetVel.y, targetVel.z);

			facingAngle = inputAngle;

			charAnimator.UpdateSpeed(walkSpeed * inputMove.normalized.magnitude);

		} else {
			//if not grounded, do arial movement calculations
//			velocity.x = inputMove.x * walkSpeed;
//			velocity.z = inputMove.z * walkSpeed;
		}
		
		//old var
		oldInputAngle = inputAngle;

		//MOVEMENT + ROTATION output
		controller.Move(velocity * Time.deltaTime);
		transform.eulerAngles = Vector3.up * facingAngle;

		//animator output
		charAnimator.Lean(new Vector3(velocity.x, 0, velocity.z).magnitude, facingAngle);
	}

	void Jump() {

		canJump = controller.isGrounded;
		velocity.y = Mathf.Sqrt(-2 * jumpHeight * gravity);

	}

	Vector3 PeepGround(Vector3 dir)
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 1.1f, 1 << 9)) {
			//rotate input to normals below player
			rotatedVector = Quaternion.FromToRotation(Vector3.up, hit.normal) * dir;
			return rotatedVector;
		}
		return dir;
	}

	void ApplyGravity() {
		float downward = velocity.y > 0 ? 0 : -10;
		//if grounded, grav accel = 0, if not, grav accel = y velocity + gravity value+current downward accel (*deltatime)
		currentGravAccel = isGrounded ? 0 : velocity.y + (gravity + downward) * Time.deltaTime;

		if (!isGrounded)
			velocity.y = currentGravAccel;

		//for the head hit
		if ((controller.collisionFlags & CollisionFlags.Above) != 0) {
			if (velocity.y > 0)
			velocity.y = 0;
		}
	}

	bool GroundCheck() {
		//cast sphere down to just below feet, if true, grounded=true
		RaycastHit hit;
		if (Physics.SphereCast(transform.position + Vector3.up, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, 1 << 9, QueryTriggerInteraction.Ignore))
			return true;

		return false;
	} 

}
