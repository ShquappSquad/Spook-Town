using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {
	
	public float speed = 0.4f;
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
		if (east_west < 0.3f && east_west > -0.3f) {
			east_west = 0.0f;
		}


		if (east_west != 0.0f || north_south != 0.0f) {
			UpdateDirection ();
			RotatePlayer ();
			RaycastHit left = new RaycastHit();
			RaycastHit middle = new RaycastHit();
			RaycastHit right = new RaycastHit();
			Vector3 origin = transform.position;
			origin.y = 2.0f;
			if (east_west < 0) {
				east_west = -east_west;
			}
			if (north_south < 0) {
				north_south = -north_south;
			}
			float moveMag = speed * (east_west + north_south) / 2.0f;
			Ray leftRay = new Ray(origin + transform.right * -1.0f, transform.forward);
			Ray middleRay = new Ray(origin + transform.forward * 1.0f, transform.forward);
			Ray rightRay = new Ray(origin + transform.right * 1.0f, transform.forward);

			if (Physics.Raycast (leftRay, out left, moveMag)) {
				if (left.collider.tag == "Obstacle") {
					moveMag = left.distance;
				}
			}
			if (Physics.Raycast (middleRay, out middle, moveMag)) {
				if (middle.collider.tag == "Obstacle") {
					moveMag = middle.distance;
				}
			}
			if (Physics.Raycast (rightRay, out right, moveMag)) {
				if (right.collider.tag == "Obstacle") {
					moveMag = right.distance;
				}
			}

			transform.position += transform.forward * moveMag;
		}
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
