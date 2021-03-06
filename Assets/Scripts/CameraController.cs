﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public GameObject player;
	public LayerMask obstacles;
	 GameObject cameraFocus;
	 GameObject camRayTarget;
	 Camera theCamera;

	public Vector2 sensitivity = new Vector2(3, 3);
	public float smoothing = 3;
	public float followDist = 3;
	public float leadDist = 2;
	public float camDistance = 7;
	public float maxCamDistance = 20;
	public float minCamDistance = 1.5f;
	[Range(0.0f, 1.0f)]
	public float obstacleDistBuffer = 1f;

	Vector3 _rayTargetPosition;
	bool _mouseLock = true;
	
	Vector2 _mouseDelta;
	Vector2 _smoothMouse;
	Vector2 _mouseAbsolute;
	Vector3 _currentVelocity;

	// Use this for initialization
	void Start () {

		cameraFocus = transform.GetChild(0).gameObject;
		camRayTarget = cameraFocus.transform.GetChild(0).gameObject;
		theCamera = cameraFocus.transform.GetChild(1).GetComponent<Camera>();

		_rayTargetPosition = new Vector3 (0, 0, -camDistance);
		camRayTarget.transform.localPosition = _rayTargetPosition;

		Cursor.lockState = CursorLockMode.Locked;

	}
	
	// Update is called once per frame
	void Update () {		
		//mouse delta per frame
		_mouseDelta = new Vector2 (Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

		Vector2 triggerZoom = new Vector2 (Input.GetAxisRaw("L2") / 2 - .5f, Input.GetAxisRaw("R2") / 2 + .5f);

		if (triggerZoom != new Vector2 (0, 0)) {
			camDistance -= (triggerZoom.x + triggerZoom.y) / 10;
			camDistance = Mathf.Clamp(camDistance, minCamDistance, maxCamDistance);
		}

		CheckCursorLock();
		MoveCamera();
		CameraAvoidObstacles();
	}
	
	void CheckCursorLock() {
		if (Input.GetButtonDown("Pause"))
			_mouseLock = !_mouseLock;

		if (_mouseLock) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		} else {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	void MoveCamera() {
		//camera follow player
//		Vector3 targetPos = player.transform.GetChild(0).position;
		Vector3 targetPos = Vector3.Lerp(transform.position, player.transform.GetChild(0).position, .8f);
//		transform.position = Vector3.SmoothDamp(transform.position, player.transform.position, ref _currentVelocity, followDist * Time.deltaTime);
		transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, 5 * Time.deltaTime);

		//camera angle
		if (_mouseLock) {
			_mouseDelta = Vector2.Scale (_mouseDelta, sensitivity);
			_smoothMouse = Vector3.Lerp(_smoothMouse, _mouseDelta, 1f / smoothing);

			_mouseAbsolute += _smoothMouse;
			_mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -60f, 90f);

			transform.localRotation = Quaternion.AngleAxis(_mouseAbsolute.x, Vector3.up);
			cameraFocus.transform.localRotation = Quaternion.AngleAxis(_mouseAbsolute.y, Vector3.right);
		}
	}

	void CameraAvoidObstacles() {
		_rayTargetPosition.z = -camDistance - obstacleDistBuffer;
		camRayTarget.transform.localPosition = _rayTargetPosition;

		Vector3 dirToPoint = (camRayTarget.transform.position - player.transform.position  + Vector3.up * 1.55f).normalized;

		RaycastHit hit;
		if (Physics.Linecast(player.transform.position + Vector3.up * 1.55f, camRayTarget.transform.position, out hit, obstacles)) {
//			Debug.DrawRay(hit.point, -dirToPoint, Color.blue);
			theCamera.transform.position = hit.point - dirToPoint * obstacleDistBuffer;
		} else {
//			Debug.DrawLine(player.transform.position  + Vector3.up * 1.55f, camRayTarget.transform.position, Color.green);
			theCamera.transform.localPosition = new Vector3 (0, 0, -camDistance);
		}
	}

	void OnApplicationFocus(bool hasFocus) {
		_mouseLock = hasFocus;
	}
}
