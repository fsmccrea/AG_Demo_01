using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxAnimator : MonoBehaviour {

	public float forwardLeanMaxAngle = 45;
	public float forwardLeanMinAngle = 10;
	public float turnLeanTime = 0.05f;

	Transform _boxTransform;

	float maxWalkSpeed;

	float _oldSpeed;
	float _oldTurnAngle;

	float accelPercent;
	float turnVelPercent;

//	float _currentLeanAngle;
	
//	float _currentAngleAccel;

	//ref
	float _currentAccelVelocity;
	float _currentTurnVelVel;

	// Use this for initialization
	void Start () {

		_boxTransform = transform.GetChild(1);

	}
	
	public void Lean (float speed, /*float accel,*/ float turnAngle) {

		float accel = (speed - _oldSpeed);
		float turnVel = (turnAngle - _oldTurnAngle);

	//	_currentLeanAngle = Mathf.SmoothDampAngle(_currentLeanAngle, turnVel, ref _currentAngleAccel, turnLeanTime);
	//	_currentLeanAngle = Mathf.Lerp(_currentLeanAngle, turnVel, Time.deltaTime);
	//	print (_currentLeanAngle);

	//	float accelPercent = Mathf.Clamp(accel/40, 0, 1) * forwardLeanMaxAngle;
		float targetAccelPercent = Mathf.Lerp (0, 1, accel) * forwardLeanMaxAngle;
		accelPercent = Mathf.SmoothDamp(accelPercent, targetAccelPercent, ref _currentAccelVelocity, 0.2f);
		accelPercent = accelPercent < 0.1 ? 0 : accelPercent;

		float targetTurnVelPercent = Mathf.Lerp (0, turnVel, speed/7) * 1.3f;
	/*	turnVelPercent = Mathf.SmoothDamp(turnVelPercent, targetTurnVelPercent, ref _currentTurnVelVel, 0.2f);
		print (targetTurnVelPercent); */

		float speedLeanPercent = Mathf.Lerp (0, forwardLeanMinAngle, speed/7);

		_oldSpeed = speed;
		_oldTurnAngle = turnAngle;

		_boxTransform.localEulerAngles = Vector3.right * (accelPercent + speedLeanPercent) + Vector3.back * targetTurnVelPercent;

//		Debug.DrawRay (_boxTransform.position, transform.right * turnVel, Color.blue);
	}
}
