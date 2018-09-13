using System.Collections;
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
	public float camMaxDistance = 7;
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

		_rayTargetPosition = new Vector3 (0, 0, -camMaxDistance);
		camRayTarget.transform.localPosition = _rayTargetPosition;

		Cursor.lockState = CursorLockMode.Locked;

	}
	
	// Update is called once per frame
	void Update () {		
		//mouse delta per frame
		_mouseDelta = new Vector2 (Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

		CheckCursorLock();
		MoveCamera();
		CameraAvoidObstacles();
	}
	
	void CheckCursorLock() {
		if (Input.GetButtonDown("Pause"))
			_mouseLock = !_mouseLock;

		if (_mouseLock) {
			Cursor.lockState = CursorLockMode.Locked;
		} else {
			Cursor.lockState = CursorLockMode.None;
		}
	}

	void MoveCamera() {
		//camera follow player
		transform.position = Vector3.SmoothDamp(transform.position, player.transform.position, ref _currentVelocity, followDist * Time.deltaTime);

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
		_rayTargetPosition.z = -camMaxDistance - obstacleDistBuffer;
		camRayTarget.transform.localPosition = _rayTargetPosition;

		Vector3 dirToPoint = (camRayTarget.transform.position - cameraFocus.transform.position).normalized;

		RaycastHit hit;
		if (Physics.Linecast(cameraFocus.transform.position, camRayTarget.transform.position, out hit, obstacles)) {
			Debug.DrawRay(hit.point, -dirToPoint, Color.blue);
			theCamera.transform.position = hit.point - dirToPoint * obstacleDistBuffer;
		} else {
			Debug.DrawLine(cameraFocus.transform.position, camRayTarget.transform.position, Color.green);
			theCamera.transform.localPosition = new Vector3 (0, 0, -camMaxDistance);
		}
	}

	void OnApplicationFocus(bool hasFocus) {
		_mouseLock = hasFocus;
	}
}
