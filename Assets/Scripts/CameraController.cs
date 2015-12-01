using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public Transform playerPosition;
	private Vector3 offset;

	public float scrollSpeed;
	private float minimumOffset;
	private float maximumOffset;

	private float scroll;

	// Use this for initialization
	void Start () {
		if (playerPosition == null) {
			offset = new Vector3 (0.0f, 0.0f, 0.0f);
		} else {
			offset = transform.position - playerPosition.position;
			minimumOffset = offset.magnitude * 0.5f;
			maximumOffset = offset.magnitude * 1.2f;
		}
	}
	
	// Update is called once per frame
	void Update () {
		scroll = Input.GetAxis ("Mouse ScrollWheel");
		if (scroll > 0.0f && offset.magnitude > minimumOffset) {
			offset += transform.forward * scrollSpeed;
		} else if (scroll < 0.0f && offset.magnitude < maximumOffset) {
			offset -= transform.forward * scrollSpeed;
		}

		if (playerPosition != null) {
			transform.position = playerPosition.position + offset;
		}

	}
}
