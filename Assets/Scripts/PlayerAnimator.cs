using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour {

	Animator animator;
	CharacterController controller;

	// Use this for initialization
	void Start () {

		controller = GetComponent<CharacterController>();
		animator = GetComponentInChildren<Animator>();
		
	}
	
	// Update is called once per frame
	void Update () {

		float speedPercent = controller.velocity.magnitude;
		animator.SetFloat("speedPercent", speedPercent);

	}
}
