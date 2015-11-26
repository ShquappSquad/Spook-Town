using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {
	
	public float speed;
	public float east_west;
	public float north_south;
	
	// Update is called once per frame
	void Update () {
		east_west = Input.GetAxis("Horizontal");
		north_south = Input.GetAxis("Vertical");

		transform.position += new Vector3 (east_west * speed, 0, north_south * speed);
	}
}
