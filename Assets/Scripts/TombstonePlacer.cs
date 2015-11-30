﻿using UnityEngine;
using System.Collections;

public class TombstonePlacer : MonoBehaviour {

	public int xNumGraves;
	public int yNumGraves;
	public float xSpacing;
	public float ySpacing;
	public float leeway;

	public GameObject grave1;
	public GameObject grave2;
	public GameObject grave3;

	// Use this for initialization
	void Start () {
		if (grave1 == null || grave2 == null || grave3 == null) {
			return; // nothing to do here
		}
		// validation
		if (xNumGraves == null) {
			xNumGraves = 1;
		}
		if (yNumGraves == null) {
			yNumGraves = 1;
		}
		if (xSpacing == null) {
			xSpacing = 0.0f;
		}
		if (ySpacing == null) {
			ySpacing = 0.0f;
		}
		if (leeway == null) {
			leeway = 0.0f;
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
				switch (Random.Range (0, 3)) {
				case 0: {
					grave = (GameObject)(Instantiate (grave1, transform.position + loc, Quaternion.identity));
					break;
				}
				case 1: {
					grave = (GameObject)(Instantiate (grave2, transform.position + loc, Quaternion.identity));
					break;
				}
				case 2: {
					grave = (GameObject)(Instantiate (grave3, transform.position + loc, Quaternion.identity));
					break;
				}
				}
				RotateTombstone (grave, Random.Range (0,9));
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
			tombstone.transform.eulerAngles = new Vector3(0.0f, 20.0f, 0.0f);
			break;
		}
		case 2: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 40.0f, 0.0f);
			break;
		}
		case 3: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 60.0f, 0.0f);
			break;
		}
		case 4: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 80.0f, 0.0f);
			break;
		}
		case 5: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 100.0f, 0.0f);
			break;
		}
		case 6: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 120.0f, 0.0f);
			break;
		}
		case 7: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 140.0f, 0.0f);
			break;
		}
		case 8: {
			tombstone.transform.eulerAngles = new Vector3(0.0f, 160.0f, 0.0f);
			break;
		}
		}
	}
}