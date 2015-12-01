using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public Transform playerPosition;
	public AudioSource music;

	private Vector3 offset;
	[Range(0.0f, 0.5f)]
	public float scrollSpeed = 0.08f;
	
	private float offsetPercent = 1.0f;
	public float minimumOffset = 0.4f;
	public float maximumOffset = 1.1f;
	private Vector3 originalOffset;
	private Vector3 offFromPlayer;

	private float scroll;

	// Use this for initialization
	void Start () {
		if (playerPosition == null) {
			offset = new Vector3 (0.0f, 0.0f, 0.0f);
		} else {
			offset = transform.position - playerPosition.position;
			offFromPlayer = new Vector3(0.0f, 0.0f, offset.z * 0.3f);
			originalOffset = offset - offFromPlayer;
			offset = originalOffset;
		}
	}
	
	// Update is called once per frame
	void Update () {
		scroll = Input.GetAxis ("Mouse ScrollWheel");
		if (scroll > 0.0f && offsetPercent > minimumOffset) {
			offset -= originalOffset * scrollSpeed;
			offsetPercent -= scrollSpeed;
		} else if (scroll < 0.0f && offsetPercent < maximumOffset) {
			offset += originalOffset * scrollSpeed;
			offsetPercent += scrollSpeed;
		}

		if (playerPosition != null) {
			transform.position = playerPosition.position + offset + offFromPlayer;
			transform.LookAt (playerPosition.position + new Vector3(0.0f, 3.0f, 0.0f));
		}

		if (Input.GetKeyDown (KeyCode.M) && music != null) {
			if (music.isPlaying) {
				music.Pause ();
			} else {
				music.UnPause ();
			}
		}
	}
}
