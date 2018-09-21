using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerOld : MonoBehaviour {

	//public
	public float minWalkSpeed = 1;
	public float maxWalkSpeed = 5;
	public float runSpeed = 10;
	public float turnTime = 0;
	public float jumpHeight = 1;
	public float accelTime = .1f;
	public float stopTime = 0;
	[Range (0,1)]
	public float airControl;
	//public reference
	public GameObject playerCam;
	//reference
	CharacterController _controller;
	Transform _camDir;
	//input
	Vector3 _inputMove;
	Vector3 _inputDir;
	bool _running;
	bool _jumping;
	bool _canJump;
	//model
	Vector3 _moveVel;
	Vector3 _velocity;
	float _currentSpeed;
	float _currentAngle;
	float _gravity = -35f;
	//ref
	Vector3 _currentMoveDirVel;
	float _currentAccel;
	float _currentTurnVel;


	// Use this for initialization
	void Start () {
		//initialize variables
		_controller = GetComponent<CharacterController>();
		_camDir = transform.GetChild(0);
	}
	
	// Update is called once per frame
	void Update () {
		//raw input vector from input axes
		_inputMove = new Vector3 (Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		//normalized input vector for direction
		_inputDir = _inputMove.normalized;
		//run bool
		_running = Input.GetButton("Button5");
		//jump bool
		_jumping = Input.GetButton("Button2");

		//call functions
		ApplyGravity();
		//jump input after gravity
		_jumping = Input.GetButton("Button2");
		if (Input.GetButtonDown("Button2") && _canJump)
			Jump();
		Move();
		GetComponent<BoxAnimator>().Lean(_currentSpeed, _currentAccel, _currentAngle);
	}

	void Move() {
		//find camera angle relative to player and then rotate input vector so that its relative to camera
		float cameraAngle = Mathf.Atan2 (playerCam.transform.forward.x, playerCam.transform.forward.z) * Mathf.Rad2Deg;
		_inputDir = (Quaternion.Euler(0, cameraAngle, 0) * _inputDir);

		//facing calc - find target angle based on movement velocity dir and smoothdamp from current angle to it
		float targetAngle = Mathf.Atan2 (_moveVel.x, _moveVel.z) * Mathf.Rad2Deg;
		if (_inputDir.magnitude > 0)
		_currentAngle = Mathf.SmoothDampAngle (_currentAngle, targetAngle, ref _currentTurnVel, turnTime);

		//get walk percent from input magnitude(thumb stick magnitude), lerp walk speed from min to max with that percent
		float walkPercent = Mathf.Clamp(_inputMove.magnitude, 0f, 0.8f) / 0.8f;
		float currentWalkSpeed = Mathf.Lerp(minWalkSpeed, maxWalkSpeed, walkPercent);

		//move calc - smoothdamped vector from current to input
		//move velocity vector - direction and magnitude (from 0 to 1)
		//time from current to target is a contidional, if on ground its 0, if in air its modifiable by GetModifiedAccelTime
		//SECTION NEEDS WORK
		_moveVel = Vector3.SmoothDamp(_moveVel, _inputDir, ref _currentMoveDirVel, ((_controller.isGrounded) ? 0 : GetModifiedAccelTime(.1f)));
		//if the player is running, set targetspeed to running, if not set it to walking (modified by thumb stick magnitude)
		float targetSpeed = ((_running) ? runSpeed : currentWalkSpeed) * _inputDir.magnitude;
		//different accel times for speeding up and slowing down
		if (targetSpeed != 0) {
			_currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _currentAccel, GetModifiedAccelTime(/*false, */accelTime));
		} else {
			_currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _currentAccel, GetModifiedAccelTime(/*false, */stopTime));
		}
		//to stop the weird tiny tiny increments of velocity from the smoothdamps
		if (_currentSpeed < .01f) _currentSpeed = 0;
		//apply horizontal movement
		_velocity.x = _moveVel.x * _currentSpeed;
		_velocity.z = _moveVel.z * _currentSpeed;

		//move cam target position
		_camDir.transform.position = transform.position + (_moveVel * _inputMove.magnitude);

		//output
		transform.eulerAngles = Vector3.up * _currentAngle;
		_controller.Move(_velocity * Time.deltaTime);
	}
	//air control modifications - if the controller is grounded, no change, not grounded, apply air control variable
	float GetModifiedAccelTime(float theAccelTime) {
		if (_controller.isGrounded) {
			return theAccelTime;
		}
		if (airControl == 0) {
			return float.MaxValue;
		}
		return theAccelTime / airControl;
	}
	//gravity
	void ApplyGravity() {
		//_downward makes falling down faster for more responsive jump
		float _downward = ((_velocity.y > 0) ? _downward = 0 : _downward = -10);
		//if jump is not being held, you fall faster
		//(change this? so jumping is also turned off when you are moving down?
		//I could just add downward to that part then and wouldnt need a seperate bit)
		_velocity.y += ((_jumping) ? _gravity + _downward : _gravity + _downward - 35) * Time.deltaTime;
		//small downward force when grounded to keep the controller grounded
		if (_controller.isGrounded) {
			_velocity.y = -.01f;
			_canJump = true;
		}
		//if you hit your head and velocity is still positive, make it zero so you can fall
		if ((_controller.collisionFlags & CollisionFlags.Above) != 0) {
			if (_velocity.y > 0)
			_velocity.y = 0;
		}
	}
	//if grounded, can jump (jumps based on a target height)
	void Jump() {
		_canJump = _controller.isGrounded;
		_velocity.y = Mathf.Sqrt(-2 * jumpHeight * _gravity);
	}
}
