using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxAnimator : MonoBehaviour {

	public float forwardLeanMaxAngle = 45;
	public float forwardLeanMinAngle = 10;
	public float turnLeanTime = 0.05f;

	Transform _boxTransform;

	float _oldTurnAngle;
	float _currentLeanAngle;
	
	float _currentAngleAccel;

	// Use this for initialization
	void Start () {

		_boxTransform = transform.GetChild(1);

	}
	
	public void Lean (float speed, float accel, float turnAngle) {

		
		float turnVel = (turnAngle - _oldTurnAngle);

	//	_currentLeanAngle = Mathf.SmoothDampAngle(_currentLeanAngle, turnVel, ref _currentAngleAccel, turnLeanTime);
	//	_currentLeanAngle = Mathf.Lerp(_currentLeanAngle, turnVel, Time.deltaTime);
	//	print (_currentLeanAngle);

		float accelPercent = Mathf.Clamp(accel/40, 0, 1) * forwardLeanMaxAngle;
		float turnVelPercent = Mathf.Lerp (0, turnVel, speed/12);

		float speedLeanPercent = Mathf.Lerp (0, forwardLeanMinAngle, speed/12);

		_oldTurnAngle = turnAngle;

		_boxTransform.localEulerAngles = Vector3.right * (accelPercent + speedLeanPercent) + Vector3.back * turnVelPercent;

		Debug.DrawRay (_boxTransform.position, transform.right * turnVel, Color.blue);
	}
}
