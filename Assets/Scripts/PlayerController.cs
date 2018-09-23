using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	//inspector variables
	public float maxWalkSpeed = 7;
	public float minWalkSpeed = 1;
	public float jumpHeight = 2;

	public float turnSmoothTime = .3f;
	public float inputSmoothTime = .1f;

	public float gravity = -35;

	//object reference
	public GameObject playerCam;
	CharacterController controller;

	//model
//	Vector3 inputMove;
	Vector3 smoothInput;
	Vector3 velocity;

	//tags
	bool jumping;
	
	//ref
	Vector3 currentInputVelocity;
	float facingAngle;
//	float currentGravAccel;
	float oldFacingAngle;
//	bool canJump = true;

	void Start()
	{
		controller = GetComponent<CharacterController>();
	}

	void Update()
	{
		//raw input vector from input axes
		Vector3 inputMove = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

		//jumpin
		jumping = Input.GetButton("Button2");
		if (Input.GetButtonDown("Button2"))// && canJump)
			Jump();

		ApplyGravity();

		//call functions
		Move(inputMove);
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
		if (controller.isGrounded) {
//			currentGravAccel = 0;
//			canJump = true;
	//		velocity = PeepGround (inputMove.normalized) * walkSpeed;
	//		velocity.y = velocity.y > -.1f ? velocity.y = -.1f : velocity.y -= .1f;

			//if grounded, give velocity ground movement
			//y velocity handled by apply gravity
			Vector3 targetVel = PeepGround (inputMove.normalized) * walkSpeed;
			velocity = new Vector3 (targetVel.x, velocity.y, targetVel.z);

		} else {
//			velocity = ApplyGravity(inputMove * walkSpeed);
			//if not grounded, do arial movement calculations
			velocity.x = inputMove.x * walkSpeed;
			velocity.z = inputMove.z * walkSpeed;
			//y velocity handled by ApplyGravity
		//	velocity.y += gravity * Time.deltaTime;
		}
		print (transform.position.y);
	//	velocity = PeepGround (inputMove.normalized) * walkSpeed;

		//old var
		oldFacingAngle = facingAngle;

		//MOVEMENT + ROTATION output
		controller.Move(velocity * Time.deltaTime);
		transform.eulerAngles = Vector3.up * facingAngle;
	}

	void Jump() {

//		canJump = controller.isGrounded;
		velocity.y = Mathf.Sqrt(-2 * jumpHeight * gravity);

	}

	Vector3 PeepGround(Vector3 dir)
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 1.1f, 1 << 9)) {
			//rotate input to normals below player
			return Quaternion.FromToRotation(Vector3.up, hit.normal) * dir;
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
	void ApplyGravity () {
		float downward = velocity.y > 0 ? 0 : -10;
		if (controller.isGrounded) {
			velocity.y = jumping ? velocity.y : -.1f;
		} else {
			velocity.y += (gravity + downward) * Time.deltaTime;
		}
	}

}
