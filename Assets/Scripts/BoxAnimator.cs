using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxAnimator : MonoBehaviour {

	public float leanMaxAngle = 45;

	Transform _boxTransform;

	// Use this for initialization
	void Start () {

		_boxTransform = transform.GetChild(1);

	}
	
	public void Lean (float accel, float turnVel) {

		float accelPercent = Mathf.Clamp(accel/40, -.2f, 1);
		float turnVelPercent = turnVel;

		Vector2 _currentLeanAnglePercent = new Vector2 (accelPercent, turnVelPercent);

		_boxTransform.localEulerAngles = Vector3.right * accelPercent * leanMaxAngle;

		Debug.DrawRay (_boxTransform.position, transform.right * turnVelPercent, Color.blue);
		print (_currentLeanAnglePercent);
	}
}
