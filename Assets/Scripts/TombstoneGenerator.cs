using UnityEngine;
using System.Collections;

public class TombstoneGenerator : MonoBehaviour {

	public int xNumGraves = 20;
	public int yNumGraves = 20;
	public float xSpacing = 20;
	public float ySpacing = 20;
	public float leeway = 8;

	public GameObject grave1;
	public GameObject grave2;
	public GameObject grave3;
	public GameObject tree;
	public GameObject lamp;

	// Use this for initialization
	void Start () {
		if (grave1 == null || grave2 == null || grave3 == null ||
		    tree == null || lamp == null) {
			return; // nothing to do here
		}

		GenerateTombstones ();
	}

	void GenerateTombstones() {
		for (int x = 0; x < xNumGraves; x++) {
			for (int y = 0; y < yNumGraves; y++) {
				Vector3 loc = new Vector3(xSpacing * x - xSpacing * xNumGraves/2 - leeway/2 + Random.value*leeway,
				                          0,
				                          ySpacing * y - ySpacing * yNumGraves/2 - leeway/2 + Random.value*leeway);
				GameObject grave = null;
				switch (Random.Range (0, 11)) {
				case 0:
				case 1:
				case 2: {
					grave = (GameObject)(Instantiate (grave1, transform.position + loc, Quaternion.identity));
					RotateTombstone (grave, Random.Range (0,12));
					break;
				}
				case 3:
				case 4:
				case 5: {
					grave = (GameObject)(Instantiate (grave2, transform.position + loc, Quaternion.identity));
					RotateTombstone (grave, Random.Range (0,12));
					break;
				}
				case 6:
				case 7:
				case 8: {
					grave = (GameObject)(Instantiate (grave3, transform.position + loc, Quaternion.identity));
					RotateTombstone (grave, Random.Range (0,12));
					break;
				}
				case 9: {
					grave = (GameObject)(Instantiate (tree, transform.position + loc + new Vector3(0.0f, 3.0f, 0.0f), Quaternion.identity));
					RotateTree (grave, Random.Range (0, 12));
					break;
				}
				case 10: {
					grave = (GameObject)(Instantiate (lamp, transform.position + loc + new Vector3(0.0f, 2.7f, 0.0f), Quaternion.identity));
					grave.transform.eulerAngles = new Vector3(270.0f, 0.0f, 0.0f);
					break;
				}
				}
			}
		}
	}

	void RotateTombstone(GameObject tombstone, int direction) {
		switch (direction) {
		case 0: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
			break;
		}
		case 1: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 30.0f, 0.0f);
			break;
		}
		case 2: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 60.0f, 0.0f);
			break;
		}
		case 3: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
			break;
		}
		case 4: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 120.0f, 0.0f);
			break;
		}
		case 5: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 150.0f, 0.0f);
			break;
		}
		case 6: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
			break;
		}
		case 7: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 210.0f, 0.0f);
			break;
		}
		case 8: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 240.0f, 0.0f);
			break;
		}
		case 9: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 270.0f, 0.0f);
			break;
		}
		case 10: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 300.0f, 0.0f);
			break;
		}
		case 11: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 330.0f, 0.0f);
			break;
		}
		}
	}

	void RotateTree(GameObject tombstone, int direction) {
		switch (direction) {
		case 0: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 0.0f, 0.0f);
			break;
		}
		case 1: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 30.0f, 0.0f);
			break;
		}
		case 2: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 60.0f, 0.0f);
			break;
		}
		case 3: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 90.0f, 0.0f);
			break;
		}
		case 4: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 120.0f, 0.0f);
			break;
		}
		case 5: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 150.0f, 0.0f);
			break;
		}
		case 6: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 180.0f, 0.0f);
			break;
		}
		case 7: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 210.0f, 0.0f);
			break;
		}
		case 8: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 240.0f, 0.0f);
			break;
		}
		case 9: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 270.0f, 0.0f);
			break;
		}
		case 10: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 300.0f, 0.0f);
			break;
		}
		case 11: {
			tombstone.transform.eulerAngles = new Vector3(270.0f, 330.0f, 0.0f);
			break;
		}
		}
	}
}
