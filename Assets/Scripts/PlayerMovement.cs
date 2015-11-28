using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {
	
	public float speed;
	public float east_west;
	public float north_south;

	public int direction = 0;
	// 0: north	1: northeast
	// 2: east	3: southeast
	// 4: south	5: southwest
	// 6: west	7: northwest
	
	// Update is called once per frame
	void Update () {
		east_west = Input.GetAxis ("Horizontal");
		north_south = Input.GetAxis ("Vertical");

		UpdateDirection ();
		RotatePlayer ();
		transform.position += new Vector3 (east_west * speed, 0, north_south * speed);
	}

	void UpdateDirection() {
		if (north_south > 0) {
			if (east_west > 0) { // northeast
				direction = 1;
			} else if (east_west < 0) { // northwest
				direction = 7;
			} else { // north
				direction = 0;
			}
		} else if (north_south < 0) {
			if (east_west > 0) { // southeast
				direction = 3;
			} else if (east_west < 0) { // southwest
				direction = 5;
			} else { // south
				direction = 4;
			}
		} else {
			if (east_west > 0 ) { // east
				direction = 2;
			} else if (east_west < 0) { // west
				direction = 6;
			}
		}
	}

	// Sets the player's rotation based on direction
	void RotatePlayer() {
		switch (direction) {
		case 0: {
			transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
			break;
		}
		case 1: {
			transform.eulerAngles = new Vector3(0.0f, 45.0f, 0.0f);
			break;
		}
		case 2: {
			transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
			break;
		}
		case 3: {
			transform.eulerAngles = new Vector3(0.0f, 135.0f, 0.0f);
			break;
		}
		case 4: {
			transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
			break;
		}
		case 5: {
			transform.eulerAngles = new Vector3(0.0f, -135.0f, 0.0f);
			break;
		}
		case 6: {
			transform.eulerAngles = new Vector3(0.0f, -90.0f, 0.0f);
			break;
		}
		case 7: {
			transform.eulerAngles = new Vector3(0.0f, -45.0f, 0.0f);
			break;
		}
		}
	}
}
