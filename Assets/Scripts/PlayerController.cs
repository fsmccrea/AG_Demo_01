using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Temporary scene management
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {

	//inspector variables
	public float maxWalkSpeed = 7;
	public float minWalkSpeed = 1;
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
	
	//ref
	Vector3 currentInputVelocity;
	Vector3 rotatedVector;
	float facingAngle;
	float currentGravAccel;
	float oldFacingAngle;
	bool canJump;

	void Start()
	{
		controller = GetComponent<CharacterController>();
		scene = SceneManager.GetActiveScene();
	}

	void Update()
	{
		//temp scene management
		if (Input.GetButtonDown("Cancel"))
			SceneManager.LoadScene(scene.name, LoadSceneMode.Single);

		//raw input vector from input axes
		Vector3 inputMove = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

		//GROUND CHECK HERE TEMPORARILY
//		isGrounded = GroundCheck();

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

		//smooth input (simple) -- r e p u r p o s e
//		smoothInput = Vector3.SmoothDamp(smoothInput, inputMove, ref currentInputVelocity, inputSmoothTime);

		//lerp walk speed based on input magnitude
		float walkSpeed = Mathf.Lerp (minWalkSpeed, maxWalkSpeed, Mathf.Clamp(inputMove.magnitude, 0f, 0.8f) / 0.8f);
		
		//facing -- add angle smoothing rather than doing it based on smoothInput
		//if input magnitude is zero, set facing angle to last recorded facing angle, else set it to the angle of SMOOTHED input (looksnicer)
//		float facingAngle = inputMove.magnitude == 0 ? oldFacingAngle : Mathf.Atan2 (smoothInput.normalized.x, smoothInput.normalized.z) * Mathf.Rad2Deg;
		float targetFacingAngle = Mathf.Atan2 (inputMove.normalized.x, inputMove.normalized.z) * Mathf.Rad2Deg;
		facingAngle = inputMove.magnitude == 0 ? oldFacingAngle : Mathf.LerpAngle(facingAngle, targetFacingAngle, Time.deltaTime / turnSmoothTime);

		//if grounded, velocity is the inputmove*walkspeed (with small force to keep grounded) aligned with the normals of the ground
		//if not grounded, velocity is the inputmove*walkspeed + grav accel on y axis
		if (isGrounded) {
			canJump = true;
//			currentGravAccel = 0;
//			canJump = true;
	//		velocity = PeepGround (inputMove.normalized) * walkSpeed;
	//		velocity.y = velocity.y > -.1f ? velocity.y = -.1f : velocity.y -= .1f;

			//if grounded, give velocity ground movement
			//y velocity handled by apply gravity --//NAH gotta get that smooth slope movement --////BUT WTF THOEEE IT HAS IT IN THERE
			Vector3 targetVel = PeepGround (inputMove.normalized) * walkSpeed;
			velocity = new Vector3 (targetVel.x, jumping ? velocity.y : targetVel.y, targetVel.z);

		} else {
//			velocity = ApplyGravity(inputMove * walkSpeed);
			//if not grounded, do arial movement calculations
			velocity.x = inputMove.x * walkSpeed;
			velocity.z = inputMove.z * walkSpeed;
			//y velocity handled by ApplyGravity -- but now its in here from there :O
//			velocity.y = currentGravAccel;
		//	velocity.y += gravity * Time.deltaTime;
		}
		print ("Grounded = " + isGrounded);
	//	velocity = PeepGround (inputMove.normalized) * walkSpeed;

		//old var
		oldFacingAngle = facingAngle;

		//MOVEMENT + ROTATION output
		controller.Move(velocity * Time.deltaTime);
		transform.eulerAngles = Vector3.up * facingAngle;
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

/*	Vector3 ApplyGravity (Vector3 dir) {

		currentGravAccel += gravity * Time.deltaTime;
//		return dir * maxWalkSpeed + Vector3.up * currentGravAccel;
//		dir.y = currentGravAccel;
		return dir + Vector3.up * currentGravAccel;

	}
	*/
	void ApplyGravity() {
		float downward = velocity.y > 0 ? 0 : -10;
	/*	if (isGrounded) {
//			velocity.y = jumping ? velocity.y : -.1f;
		//	if (jumping) {
		//		velocity.y = velocity.y;
		//	} else {
		//		velocity.y = rotatedVector.y - 0.1f;
		//	}
			currentGravAccel = 0;
		} else {
			currentGravAccel = velocity.y + (gravity + downward) * Time.deltaTime;
		} */
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
	/*	foreach (Vector3 rayStart in groundCheckRays) {
			Vector3 pos = new Vector3
		}

		//debugfornow
		return false;
		*/
		RaycastHit hit;
		if (Physics.SphereCast(transform.position + Vector3.up, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, 1 << 9, QueryTriggerInteraction.Ignore))
			return true;

		return false;
	} 

}
