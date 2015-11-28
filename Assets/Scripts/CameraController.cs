using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public Transform playerPosition;
	public Vector3 offset;

	// Use this for initialization
	void Start () {
		if (playerPosition == null) {
			offset = new Vector3 (0, 0, 0);
		} else {
			offset = transform.position - playerPosition.position;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (playerPosition != null) {
			transform.position = playerPosition.position + offset;
		}
	}
}
