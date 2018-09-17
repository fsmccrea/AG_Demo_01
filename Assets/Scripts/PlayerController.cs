using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float minWalkSpeed = 1;
	public float maxWalkSpeed = 5;
	public float runSpeed = 10;
	public float turnTime = 0;
	public float jumpHeight = 1;
	public float accelTime = .1f;
	public float stopTime = 0;
	[Range (0,1)]
	public float airControl;

	public GameObject playerCam;

	CharacterController _controller;
	Transform _camPos;
	
	Vector3 _inputMove;
	Vector3 _inputDir;
	bool _running;
	bool _jumping;
	bool _canJump;
	bool _isGrounded;

	float _gravity = -35f;

	Vector3 _moveVel;
	Vector3 _currentMoveDirVel;

	float _moveAngle;
	float _lookAngle;
	float _currentAngle;
	float _currentTurnVel;
	float _currentSpeed;
	float _currentAccel;

	Vector3 _velocity;

	float oldVel;

	// Use this for initialization
	void Start () {

		_controller = GetComponent<CharacterController>();
		_camPos = transform.GetChild(0);
		
	}
	
	// Update is called once per frame
	void Update () {
		
		_inputMove = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		_inputDir = _inputMove.normalized;
		_running = Input.GetButton("Button5");

		_jumping = Input.GetButton("Button2");

		float _downward = ((_velocity.y > 0) ? _downward = 0 : _downward = -10);
		_velocity.y += ((_jumping) ? _gravity + _downward : _gravity + _downward - 35) * Time.deltaTime;
		if (_controller.isGrounded) {
			_velocity.y = -.01f;
			_canJump = true;
		}

		_jumping = Input.GetButton("Button2");
		if (Input.GetButtonDown("Button2") && _canJump)
			Jump();

		Move();
		GetComponent<BoxAnimator>().Lean(_currentSpeed, _currentAccel, _currentAngle);
	}

	void Move() {

		float cameraAngle = Mathf.Atan2 (playerCam.transform.forward.x, playerCam.transform.forward.z) * Mathf.Rad2Deg;
		_inputDir = (Quaternion.Euler(0, cameraAngle, 0) * _inputDir);

		//facing calc
		float targetAngle = Mathf.Atan2 (_moveVel.x, _moveVel.z) * Mathf.Rad2Deg;
		if (_inputDir.magnitude > 0)
		_currentAngle = Mathf.SmoothDampAngle (_currentAngle, targetAngle, ref _currentTurnVel, turnTime);

		//thumbstick magnitude calc
		float walkPercent = Mathf.Clamp(_inputMove.magnitude, 0f, 0.8f) / 0.8f;
		float currentWalkSpeed = Mathf.Lerp(minWalkSpeed, maxWalkSpeed, walkPercent);

		//move calc
		_moveVel = Vector3.SmoothDamp(_moveVel, _inputDir, ref _currentMoveDirVel, ((_controller.isGrounded) ? 0 : GetModifiedAccelTime(.1f)));

		float targetSpeed = ((_running) ? runSpeed : currentWalkSpeed) * _inputDir.magnitude;
		if (targetSpeed != 0) {
			_currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _currentAccel, GetModifiedAccelTime(/*false, */accelTime));
		} else {
			_currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _currentAccel, GetModifiedAccelTime(/*false, */stopTime));
		}
		if (_currentSpeed < .01f) _currentSpeed = 0;
		
		_velocity.x = _moveVel.x * _currentSpeed;
		_velocity.z = _moveVel.z * _currentSpeed;

		//move cam
		_camPos.transform.position = transform.position + (_moveVel * _currentSpeed) / 5;

		//output
		transform.eulerAngles = Vector3.up * _currentAngle;
		_controller.Move(_velocity * Time.deltaTime);
	}

	float GetModifiedAccelTime(float theAccelTime) {
		if (_controller.isGrounded) {
			return theAccelTime;
		}
		if (airControl == 0) {
			return float.MaxValue;
		}
		return theAccelTime / airControl;
	}

	void Jump() {
		_canJump = _controller.isGrounded;
		_velocity.y = Mathf.Sqrt(-2 * jumpHeight * _gravity);
	}
}
