using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	//inspector variables
	public float walkSpeed = 7;

	public float inputSmoothTime = .1f;

	//object reference
	public GameObject playerCam;
	CharacterController controller;
//	Transform slopePeeper;

	//model
	Vector3 inputMove;
	Vector3 smoothInput;
	
	//ref
	Vector3 currentInputVelocity;
	float oldFacingAngle;

	void Start()
	{
		controller = GetComponent<CharacterController>();
//		slopePeeper = transform.GetChild(2);
	}

	void Update()
	{
		//raw input vector from input axes
		inputMove = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

		//call functions
		Move();
		PeepNormals();
	}

	void Move()
	{
		//camera angle relative to player
		float cameraAngle = Mathf.Atan2 (playerCam.transform.forward.x, playerCam.transform.forward.z) * Mathf.Rad2Deg;
		//input rotation so its relative to camera
		inputMove = (Quaternion.Euler(0, cameraAngle, 0) * inputMove);

		//smooth input (simple)
		smoothInput = Vector3.SmoothDamp(smoothInput, inputMove, ref currentInputVelocity, inputSmoothTime);
		
		//facing
		//if input magnitude is zero, set facing angle to last recorded facing angle, else set it to the angle of input
		float facingAngle = ((inputMove.magnitude == 0) ? oldFacingAngle : Mathf.Atan2 (smoothInput.normalized.x, smoothInput.normalized.z) * Mathf.Rad2Deg);

		//old var
		oldFacingAngle = facingAngle;

		//MOVEMENT + ROTATION output
//		controller.Move(inputMove.normalized * walkSpeed * Time.deltaTime);
		controller.Move(PeepNormals() * walkSpeed * Time.deltaTime);
		transform.eulerAngles = Vector3.up * facingAngle;
	}

	Vector3 PeepNormals()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 10, 1 << 9)) {
			Debug.DrawLine (transform.position + Vector3.up, hit.point, Color.green);
			Debug.DrawRay (hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal) * inputMove, Color.blue);
//			slopePeeper.rotation = Quaternion.FromToRotation (Vector3.up, hit.normal);
			//temporary snap down to surface so it looks nice
//			slopePeeper.position = hit.point;
			return Quaternion.FromToRotation(Vector3.up, hit.normal) * inputMove;
		}
		return inputMove;
	}

}
