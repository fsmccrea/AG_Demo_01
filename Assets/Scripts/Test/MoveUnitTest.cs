using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUnitTest : MonoBehaviour {

	public float moveDist = 4;
	CharacterController controller;

	Vector3 input;
	Vector3 velocity;

	Animator animator;

	// Use this for initialization
	void Start () {

		controller = GetComponent<CharacterController>();

		animator = GetComponentInChildren<Animator>();

	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetButton("Button2")) {
			input = Vector3.right * moveDist;
			transform.eulerAngles = new Vector3 (0, 90, 0);
		} if (Input.GetButton("Button3")) {
			input = Vector3.left * moveDist;
			transform.eulerAngles = new Vector3 (0, -90, 0);
		}
		if (Input.GetButton("Button2") || Input.GetButton("Button3")) {
		velocity = input; }
		else
		{
			velocity = Vector3.zero;
		}

		float speedPercent = velocity.magnitude / moveDist;

		animator.SetFloat("speedPercent", speedPercent, .1f, Time.deltaTime);

		controller.Move(velocity * Time.deltaTime);

	}
}
