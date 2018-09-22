using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	//inspector variables
	public float maxWalkSpeed = 7;
	public float minWalkSpeed = 1;

	public float inputSmoothTime = .1f;

	public float gravity = -35;

	//object reference
	public GameObject playerCam;
	CharacterController controller;
//	Transform slopePeeper;

	//model
//	Vector3 inputMove;
	Vector3 smoothInput;
	Vector3 velocity;
	
	//ref
	Vector3 currentInputVelocity;
	float currentGravAccel;
	float oldFacingAngle;

	void Start()
	{
		controller = GetComponent<CharacterController>();
//		slopePeeper = transform.GetChild(2);
	}

	void Update()
	{
		//raw input vector from input axes
		Vector3 inputMove = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

		//call functions
		GroundMove(inputMove);
	}

	void GroundMove(Vector3 inputMove)
	{
		//camera angle relative to player
		float cameraAngle = Mathf.Atan2 (playerCam.transform.forward.x, playerCam.transform.forward.z) * Mathf.Rad2Deg;
		//input rotation so its relative to camera
		inputMove = (Quaternion.Euler(0, cameraAngle, 0) * inputMove);

		//smooth input (simple) -- r e p u r p o s e
		smoothInput = Vector3.SmoothDamp(smoothInput, inputMove, ref currentInputVelocity, inputSmoothTime);

		//lerp walk speed based on input magnitude
		float walkSpeed = Mathf.Lerp (minWalkSpeed, maxWalkSpeed, Mathf.Clamp(inputMove.magnitude, 0f, 0.8f) / 0.8f);
		
		//facing -- add angle smoothing rather than doing it based on smoothInput
		//if input magnitude is zero, set facing angle to last recorded facing angle, else set it to the angle of SMOOTHED input (looksnicer)
		float facingAngle = ((inputMove.magnitude == 0) ? oldFacingAngle : Mathf.Atan2 (smoothInput.normalized.x, smoothInput.normalized.z) * Mathf.Rad2Deg);

		//movement velocity (+gravity if not grounded)
//		velocity = inputMove * walkSpeed + Vector3.up * gravity * Time.deltaTime;
//		if (controller.isGrounded) {
//			velocity = PeepNormals (inputMove * walkSpeed - Vector3.up * .1f);
//		}
		//if grounded, velocity is the inputmove*walkspeed (with small force to keep grounded) aligned with the normals of the ground
		//if not grounded, velocity is the inputmove*walkspeed + grav accel on y axis
//		velocity = ApplyGravity(inputMove, walkSpeed);
//		velocity = PeepNormals (inputMove * walkSpeed);// - Vector3.up * 0.1f;
		velocity = PeepNormals (inputMove) * walkSpeed;
//		velocity.y = velocity.y - 1;
//		velocity = inputMove * walkSpeed;
//		velocity.y = velocity.y - .01f;
		print (controller.isGrounded);

		//old var
		oldFacingAngle = facingAngle;

		//MOVEMENT + ROTATION output
//		controller.Move(inputMove.normalized * walkSpeed * Time.deltaTime);
		controller.Move(velocity * Time.deltaTime);
		transform.eulerAngles = Vector3.up * facingAngle;
	}

	Vector3 PeepNormals(Vector3 dir)
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 10, 1 << 9)) {
	//		Debug.DrawLine (transform.position + Vector3.up, hit.point, Color.green);
	//		Debug.DrawRay (hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal) * inputMove, Color.blue);
//			slopePeeper.rotation = Quaternion.FromToRotation (Vector3.up, hit.normal);
//			//temporary snap down to surface so it looks nice
//			slopePeeper.position = hit.point;

			//rotate input to normals below player
			dir.y = dir.y - .1f;
			return Quaternion.FromToRotation(Vector3.up, hit.normal) * dir.normalized;
		}
		return dir.normalized;
	}

	Vector3 ApplyGravity (Vector3 dir, float speed) {
//		velocity.y += gravity * Time.deltaTime;
		if (controller.isGrounded) {
			currentGravAccel = 0;
			return PeepNormals (dir * speed + Vector3.up * .01f);
		}
		currentGravAccel += gravity + Time.deltaTime;
		return dir * speed + Vector3.up * currentGravAccel;
	}

}
