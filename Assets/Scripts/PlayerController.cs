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
	
//	Vector2 inputMove;
//	Vector2 _inputDir;
	Vector3 inputMove;
	Vector3 _inputDir;
	bool _running;
	bool _jumping;
	bool _canJump;
	bool _isGrounded;

	float _gravity = -20f;

	Vector3 _moveDir;
	Vector3 _currentMoveDirVel;

	float _moveAngle;
	float _lookAngle;
	float _currentAngle;
//	float _currentMoveTurnAccel;
	float _currentTurnAccel;
	float _currentSpeed;
	float _currentAccel;

	Vector3 _velocity;

	float oldVel;

	// Use this for initialization
	void Start () {

		_controller = GetComponent<CharacterController>();
		
	}
	
	// Update is called once per frame
	void Update () {
		
//		inputMove = new Vector2 (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		inputMove = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		_inputDir = inputMove.normalized;
		_running = Input.GetButton("Button5");

		_jumping = Input.GetButton("Button2");

		_velocity.y += ((_jumping) ? _gravity : _gravity - 35) * Time.deltaTime;
		if (_controller.isGrounded) {
			_velocity.y = -.01f;
			_canJump = true;
		}

		_jumping = Input.GetButton("Button2");
		if (Input.GetButtonDown("Button2") && _canJump)
			Jump();

		Move();
	}

	void Move() {

		float cameraAngle = Mathf.Atan2 (playerCam.transform.forward.x, playerCam.transform.forward.z) * Mathf.Rad2Deg;
		_inputDir = (Quaternion.Euler(0, cameraAngle, 0) * _inputDir);

		//facing calc

		/*
		float targetAngle = Mathf.Atan2 (_inputDir.x, _inputDir.z) * Mathf.Rad2Deg;// + cameraAngle;
		if (_inputDir.magnitude > 0)
			_moveAngle = Mathf.SmoothDampAngle(_moveAngle, targetAngle, ref _currentMoveTurnAccel, ((_controller.isGrounded) ? 0 : GetModifiedAccelTime(.1f)));
		_currentAngle = Mathf.SmoothDampAngle (_currentAngle, _moveAngle, ref _currentTurnAccel, turnTime);
		*/
		_moveDir = Vector3.SmoothDamp(_moveDir, _inputDir, ref _currentMoveDirVel, ((_controller.isGrounded) ? 0 : GetModifiedAccelTime(.1f)));
		float targetAngle = Mathf.Atan2 (_moveDir.x, _moveDir.z) * Mathf.Rad2Deg;
//		_lookAngle = Mathf.Lerp (_lookAngle, targetAngle, _inputDir.magnitude);
		if (_inputDir.magnitude > 0)
		_currentAngle = Mathf.SmoothDampAngle (_currentAngle, targetAngle, ref _currentTurnAccel, turnTime);

		//thumbstick magnitude calc
		float walkPercent = Mathf.Clamp(inputMove.magnitude, 0f, .8f) / 0.8f;
		float currentWalkSpeed = Mathf.Lerp(minWalkSpeed, maxWalkSpeed, walkPercent);

		//move calc
		float targetSpeed = ((_running) ? runSpeed : currentWalkSpeed) * _inputDir.magnitude;
		if (targetSpeed != 0) {
			_currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _currentAccel, GetModifiedAccelTime(/*false, */accelTime));
		} else {
			_currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _currentAccel, GetModifiedAccelTime(/*false, */stopTime));
		}
		if (_currentSpeed < .01f) _currentSpeed = 0;
//		_velocity = new Vector3(_inputDir.x, _velocity.y, _inputDir.y) * _currentSpeed * Time.deltaTime;
//		_velocity.x = _inputDir.x * _currentSpeed;
//		_velocity.z = _inputDir.z * _currentSpeed;

//		Vector3 moveDir = Quaternion.Euler(0, _moveAngle, 0) * Vector3.forward;
//		_velocity.x = (moveDir * _currentSpeed).x;
//		_velocity.z = (moveDir * _currentSpeed).z;

		_velocity.x = _moveDir.x * _currentSpeed;
		_velocity.z = _moveDir.z * _currentSpeed;

		//output
		transform.eulerAngles = Vector3.up * _currentAngle;
//		transform.Translate (Quaternion.Euler(0, cameraAngle, 0) * _velocity, Space.World);
//		_controller.Move(Quaternion.Euler(0, cameraAngle, 0) * _velocity);
		_controller.Move(_velocity * Time.deltaTime);
	}

	float GetModifiedAccelTime(/*bool rotation, */float theAccelTime) {
//		if (rotation) {
//			if (_controller.isGrounded) {
//			return theAccelTime;
//			}
//			if (airControl == 0) {
//				return float.MinValue;
//			}
//			return theAccelTime * airControl;
//		}
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
		print(_canJump);
	}
}
