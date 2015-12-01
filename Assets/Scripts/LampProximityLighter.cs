using UnityEngine;
using System.Collections;

public class LampProximityLighter : MonoBehaviour {

	public Transform player;
	public float activationProximity = 5.0f;
	public Light flame;
	public float maxIntensity = 4.0f;

	private ParticleSystem particle;
	private bool active = false;

	void Start () {
		if (flame != null) {
			flame.intensity = 0.0f;
			particle = flame.GetComponent<ParticleSystem> ();
			if (particle.isPlaying) {
				particle.Stop ();
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (player != null && flame != null && !active) {
			Vector3 offset = transform.position - player.position;
			if (offset.magnitude < activationProximity) {
				Activate ();
			}
		} else if (flame.intensity < maxIntensity) {
			flame.intensity += maxIntensity * 0.02f;
		}
	}

	void Activate () {
		active = true;
		particle.Play ();
	}
}
