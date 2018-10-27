using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxAnimator : MonoBehaviour {

	public float forwardLeanMaxAngle = 45;
	public float forwardLeanMinAngle = 10;
	public float maxTurnLeanAngle = 20;

	Transform _boxTransform;
	Animator animator;

	float maxWalkSpeed;

	float _oldSpeed;
	float _oldTurnAngle;

	float accelPercent;
	float turnVelPercent;

	float turnLeanAngle;

//	float _currentLeanAngle;
	
//	float _currentAngleAccel;

	//ref
	float _currentAccelVelocity;
	float _currentTurnVelVel;

	// Use this for initialization
	void Start () {

		_boxTransform = transform.GetChild(1);
		animator = GetComponentInChildren<Animator>();

	}
	
	public void Lean (float speed, float turnAngle) {

		//acceleration and turn velocity calculated with new - old
		float accel = (speed - _oldSpeed);
		float turnVel = (turnAngle - _oldTurnAngle);
		
		//target acceleration percent is a lerp between 0 and 1 based on the acceleration value * max angle of forward lean
		float targetAccelPercent = Mathf.Lerp (0, 1, accel) * forwardLeanMaxAngle;
		//accelpercent smoothdamps between current and target at a speed of 0.2
		accelPercent = Mathf.SmoothDamp(accelPercent, targetAccelPercent, ref _currentAccelVelocity, 0.2f);
		//if accelpercent is below 0.1, it becomes zero, else it is itselff
		accelPercent = accelPercent < 0.1f ? 0 : accelPercent;

//		//targetTurnVelPercent is a lerp between zero and the turn velocity based on the current movement speed percent
//		float targetTurnVelPercent = Mathf.Lerp (0, turnVel, speed/7.1f) * 2f;
//		turnVelPercent = Mathf.SmoothDamp(turnVelPercent, targetTurnVelPercent, ref _currentTurnVelVel, .01f);

		float targetTurnVelPercent = Mathf.Lerp (0, 1, Mathf.Abs(turnVel) / 5) * (speed/7.1f) * Mathf.Sign(turnVel);
		turnVelPercent = Mathf.SmoothDamp(turnVelPercent, targetTurnVelPercent, ref _currentTurnVelVel, 0.05f);

		turnLeanAngle = turnVelPercent * maxTurnLeanAngle;

		//speedLeanPercent lerps between zero and the minimum angle based on the current speed
		float speedLeanPercent = Mathf.Lerp (0, forwardLeanMinAngle, speed/7.1f);

		_oldSpeed = speed;
		_oldTurnAngle = turnAngle;

		_boxTransform.localEulerAngles = Vector3.right * (accelPercent + speedLeanPercent) + Vector3.back * turnLeanAngle;
	}

	public void UpdateSpeed (float speedPercent) {

		animator.SetFloat("speedPercent", speedPercent);

	}
}
